using System;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using static UnityEngine.InputSystem.InputAction;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _walkSpeed = 5;
        [SerializeField] private float _runSpeed = 8;
        [SerializeField] private float _crouchSpeed = 2;
        [SerializeField] private float _crouchHeight = 0.5f;
        [SerializeField] private float _rotateSpeed = 10;
        [SerializeField] private float _jumpForce = 5;
        [SerializeField] private float _gravity = -9.81f;

        #region SprintbarFields
        [SerializeField] private float _sprintEnergy = 100;
        [SerializeField] private float _lostEnergyAmount = 10;
        [SerializeField] private float _recoveredEneryAmount = 5;

        [SerializeField] private int _timeUntilReplenishment = 4;

        private float _currentSprintEnergy;

        private bool _isSprint;

        private CancellationTokenSource _cts = new CancellationTokenSource();

        public event Action<float> CurrentEnergy;

        #endregion

        private InputSystem _inputSystem;
        private CharacterController _characterController;
        private Camera _playerCamera;

        private Vector3 _velocity;
        private Vector2 _rotation;
        private NativeArray<Vector2> _outputCamera;
        private NativeArray<Vector3> _outputVelocity;

        private bool _isCrouch;

        [Inject]
        private void Construct(InputSystem inputSystem) => _inputSystem = inputSystem;

        private void Start()
        {
            _inputSystem.Player.Jump.performed += Jump;
            _inputSystem.Player.Crouch.performed += Crouch;
            _inputSystem.Player.Crouch.canceled += Stand;
            _inputSystem.Player.Crouch.canceled += DecreaseEnergy;
            _inputSystem.Player.Sprint.performed += DecreaseEnergy;
            _inputSystem.Player.Sprint.performed += IncreaseEnergy;
            _inputSystem.Player.Enable();

            _characterController = GetComponent<CharacterController>();
            _playerCamera = GetComponentInChildren<Camera>();

            _outputCamera = new NativeArray<Vector2>(2, Allocator.Persistent);
            _outputVelocity = new NativeArray<Vector3>(2, Allocator.Persistent);

            _currentSprintEnergy = _sprintEnergy;

            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            NativeArray<JobHandle> jobs = new NativeArray<JobHandle>(2, Allocator.Temp);

            _outputCamera[0] = _rotation;
            _outputVelocity[0] = _velocity;

            CameraRotateCalculation cameraRotateCalculation = new CameraRotateCalculation
            {
                Rotation = _outputCamera,
                DeltaTime = Time.deltaTime,
                MouseDelta = _inputSystem.Player.CameraLook.ReadValue<Vector2>(),
                RotateSpeed = _rotateSpeed
            };
            VelocityCalculation velocityCalculation = new VelocityCalculation
            {
                Velocity = _outputVelocity,
                WalkSpeed = _walkSpeed,
                RunSpeed = _runSpeed,
                CrouchSpeed = _crouchSpeed,
                CameraAnglesY = _playerCamera.transform.localEulerAngles.y,
                Direction = _inputSystem.Player.Movement.ReadValue<Vector2>(),
                IsSprint = _inputSystem.Player.Sprint.IsPressed(),
                IsCrouch = _inputSystem.Player.Crouch.IsPressed(),
                CurrentSprintEnergy = _currentSprintEnergy
            };

            jobs[0] = cameraRotateCalculation.Schedule();
            jobs[1] = velocityCalculation.Schedule();
            JobHandle.CompleteAll(jobs);

            _rotation = _outputCamera[1];
            _velocity = _outputVelocity[1];

            _playerCamera.transform.localEulerAngles = _rotation;
            _characterController.Move(_velocity * Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (_characterController.isGrounded && _velocity.y <= 0f) _velocity.y = -0.1f;
            else _velocity.y += _gravity * Time.fixedDeltaTime;
        }

        private void CrouchInternal(bool isCrouch, float crouchHeight)
        {
            if (!_characterController.isGrounded) return;

            _isCrouch = isCrouch;

            _characterController.height = crouchHeight;

            if (isCrouch) { transform.position = new Vector3(transform.position.x, transform.position.y/2, transform.position.z); }
            else { transform.position = new Vector3(transform.position.x, transform.position.y * 2, transform.position.z); }
        }

        private void Crouch() => CrouchInternal(true, _crouchHeight);

        private void Stand() => CrouchInternal(false, 2);

        private void Jump(CallbackContext _) { if (_characterController.isGrounded && !_isCrouch) _velocity.y = _jumpForce; }

        private void Crouch(CallbackContext _) { Crouch(); }

        private void Stand(CallbackContext _) { Stand(); }

        public bool IsCrouch() => _isCrouch;

        #region Sprintbar
        private async void DecreaseEnergy(CallbackContext _)
        {
            if (_inputSystem.Player.Crouch.IsPressed()) return;

            if (_inputSystem.Player.Sprint.IsPressed())
            {
                _isSprint = true;

                _cts?.Cancel();
                _cts = new CancellationTokenSource();

                while (_isSprint == true && _currentSprintEnergy >= 0)
                {
                    await Awaitable.WaitForSecondsAsync(0.03f);
                    _currentSprintEnergy -= _lostEnergyAmount * 0.03f;
                    CurrentEnergy?.Invoke(_currentSprintEnergy);
                }
            }
        }

        private async void IncreaseEnergy(CallbackContext _)
        {
            _isSprint = false;
            await Awaitable.WaitForSecondsAsync(_timeUntilReplenishment, _cts.Token);

            while (_isSprint == false && _currentSprintEnergy <= _sprintEnergy)
            {
                await Awaitable.WaitForSecondsAsync(0.03f);
                _currentSprintEnergy += _recoveredEneryAmount * 0.03f;
                CurrentEnergy?.Invoke(_currentSprintEnergy);
            }
        }
        #endregion

        private void OnDestroy()
        {
            _inputSystem.Player.Jump.performed -= Jump;
            _inputSystem.Player.Crouch.performed -= Crouch;
            _inputSystem.Player.Crouch.canceled -= Stand;
            _inputSystem.Player.Crouch.canceled -= DecreaseEnergy;
            _inputSystem.Player.Sprint.performed -= DecreaseEnergy;
            _inputSystem.Player.Sprint.performed -= IncreaseEnergy;
            _inputSystem.Player.Disable();

            _outputCamera.Dispose();
            _outputVelocity.Dispose();
        }

        [BurstCompile]
        struct CameraRotateCalculation : IJob
        {
            [ReadOnly] public float DeltaTime;
            [ReadOnly] public Vector2 MouseDelta;
            [ReadOnly] public float RotateSpeed;
            public NativeArray<Vector2> Rotation;

            public void Execute()
            {
                Vector2 rotation = Rotation[0];
                if (MouseDelta.sqrMagnitude < 0.1f) return;

                MouseDelta *= RotateSpeed * DeltaTime;
                rotation.y += MouseDelta.x;
                rotation.x = Mathf.Clamp(rotation.x - MouseDelta.y, -90, 90);
                Rotation[1] = rotation;
            }
        }

        [BurstCompile]
        struct VelocityCalculation : IJob
        {
            [ReadOnly] public float WalkSpeed;
            [ReadOnly] public float RunSpeed;
            [ReadOnly] public float CrouchSpeed;
            [ReadOnly] public float CameraAnglesY;
            [ReadOnly] public bool IsSprint;
            [ReadOnly] public bool IsCrouch;
            [ReadOnly] public Vector2 Direction;
            [ReadOnly] public float CurrentSprintEnergy;
            public NativeArray<Vector3> Velocity;

            public void Execute()
            {
                Vector3 velocity = Velocity[0];
                Direction *= IsCrouch ? CrouchSpeed : IsSprint && CurrentSprintEnergy >= 0 ? RunSpeed : WalkSpeed;
                Vector3 move = Quaternion.Euler(0, CameraAnglesY, 0) * new Vector3(Direction.x, 0, Direction.y);
                velocity = new Vector3(move.x, velocity.y, move.z);
                Velocity[1] = velocity;
            }
        }
    }
}