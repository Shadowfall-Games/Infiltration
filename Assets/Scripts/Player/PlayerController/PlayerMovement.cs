using Codice.CM.Common;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

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

        [SerializeField] private Sprintbar _sprintbar;

        private InputSystem _inputSystem;
        private CharacterController _characterController;
        private Camera _playerCamera;

        private Vector3 _velocity;
        private Vector2 _rotation;
        private NativeArray<Vector2> _outputCamera;
        private NativeArray<Vector3> _outputVelocity;

        private bool _isCrouch;
        private bool _isSprint;

        [Inject]
        private void Construct(InputSystem inputSystem) => _inputSystem = inputSystem;

        private void Start()
        {
            _inputSystem.Player.Jump.performed += Jump;
            _inputSystem.Player.Crouch.performed += Crouch;
            _inputSystem.Player.Crouch.canceled += Stand;
            _inputSystem.Player.Sprint.canceled += _sprintbar.CancelTask;
            _inputSystem.Player.Enable();

            _characterController = GetComponent<CharacterController>();
            _playerCamera = GetComponentInChildren<Camera>();

            _outputCamera = new NativeArray<Vector2>(2, Allocator.Persistent);
            _outputVelocity = new NativeArray<Vector3>(2, Allocator.Persistent);

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
                IsCrouch = _inputSystem.Player.Crouch.IsPressed()
            };

            jobs[0] = cameraRotateCalculation.Schedule();
            jobs[1] = velocityCalculation.Schedule();
            JobHandle.CompleteAll(jobs);

            _rotation = _outputCamera[1];
            _velocity = _outputVelocity[1];

            _playerCamera.transform.localEulerAngles = _rotation;
            _characterController.Move(_velocity * Time.deltaTime);

            _isSprint = _inputSystem.Player.Sprint.IsPressed();
            if (_isSprint) _sprintbar.DecreaseEnergy();
            else _sprintbar.IncreaseEnergy();
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

            if (isCrouch) { this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y/2, this.transform.position.z); }
            else { this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y * 2, this.transform.position.z); }
        }

        private void Crouch() => CrouchInternal(true, _crouchHeight);

        private void Stand() => CrouchInternal(false, 2);

        private void Jump(InputAction.CallbackContext _) { if (_characterController.isGrounded && !_isCrouch) _velocity.y = _jumpForce; }

        private void Crouch(InputAction.CallbackContext _) { if (_characterController.isGrounded) Crouch(); }

        private void Stand(InputAction.CallbackContext _) { if (_characterController.isGrounded) Stand(); }

        private void OnDestroy()
        {
            _inputSystem.Player.Jump.performed -= Jump;
            _inputSystem.Player.Crouch.performed -= Crouch;
            _inputSystem.Player.Crouch.canceled -= Stand;
            _inputSystem.Player.Sprint.canceled -= _sprintbar.CancelTask;
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
            public NativeArray<Vector3> Velocity;

            public void Execute()
            {
                Vector3 velocity = Velocity[0];
                Direction *= IsCrouch ? CrouchSpeed : IsSprint ? RunSpeed : WalkSpeed;
                Vector3 move = Quaternion.Euler(0, CameraAnglesY, 0) * new Vector3(Direction.x, 0, Direction.y);
                velocity = new Vector3(move.x, velocity.y, move.z);
                Velocity[1] = velocity;
            }
        }
    }
}