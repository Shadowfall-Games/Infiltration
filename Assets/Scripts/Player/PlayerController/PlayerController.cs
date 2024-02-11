using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _walkSpeed = 2.0f;
    [SerializeField] private float _crouchSpeed;
    [SerializeField] private float _jumpHeight = 1.0f;
    [SerializeField] private float _crouchHeight = 0.75f;

    [SerializeField] private Transform _cameraTransform;

    private float _playerSpeed;

    private CharacterController _controller;

    private InputSystem _inputSystem;

    private Vector3 _playerVelocity;

    private bool isPlayerCrouching;

    private const float GRAVITY_VALUE = -9.81f;

    [Inject]
    private void Construct(InputSystem inputSystem) => _inputSystem = inputSystem;

    private void Start()
    {
        _playerSpeed = _walkSpeed;
        _controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (_controller.isGrounded && _playerVelocity.y < 0) _playerVelocity.y = 0f;

        Vector2 direction = _inputSystem.Player.Movement.ReadValue<Vector2>();
        Vector3 move = new Vector3(direction.x, 0f, direction.y);
        move = _cameraTransform.forward * move.z + _cameraTransform.right * move.x;
        move.y = 0f;
        _playerVelocity.y += GRAVITY_VALUE * Time.deltaTime;
        move += _playerVelocity * Time.deltaTime;
        _controller.Move(move * Time.deltaTime * _playerSpeed);
    }

    private void Jump(InputAction.CallbackContext callbackContext)
    {
        if (isPlayerCrouching) Crouch();
        if (_controller.isGrounded) _playerVelocity.y += Mathf.Sqrt(_jumpHeight * -3.0f * GRAVITY_VALUE);
    }

    private void Crouch()
    {
        isPlayerCrouching = !isPlayerCrouching;

        if (isPlayerCrouching)
        {
            _controller.transform.localScale = new Vector3(1, _crouchHeight, 1);
            _playerSpeed = _crouchSpeed;
        }
        else
        {
            _controller.transform.localScale = new Vector3(1, 1, 1);
            _playerSpeed = _walkSpeed;
        }
    }

    private void OnEnable()
    {
        _inputSystem.Player.Jump.performed += Jump;
        _inputSystem.Player.Crouch.performed += ctx => Crouch();
    }

    private void OnDisable()
    {
        _inputSystem.Player.Jump.performed -= Jump;
        _inputSystem.Player.Crouch.performed -= ctx => Crouch();
    }
}