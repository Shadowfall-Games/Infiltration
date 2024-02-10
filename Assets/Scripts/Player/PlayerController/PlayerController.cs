using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _playerSpeed = 2.0f;
    [SerializeField] private float _jumpHeight = 1.0f;

    [SerializeField] private Transform _cameraTransform;

    private CharacterController _controller;

    private InputSystem _inputSystem;

    private Vector3 _playerVelocity;

    private float _gravityValue = -9.81f;

    [Inject]
    private void Construct(InputSystem inputSystem) => _inputSystem = inputSystem;

    private void Start()
    {
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
        _controller.Move(move * Time.deltaTime * _playerSpeed);

        if (move != Vector3.zero) gameObject.transform.forward = move;

        _playerVelocity.y += _gravityValue * Time.deltaTime;
        _controller.Move(_playerVelocity * Time.deltaTime);
    }

    private void Jump(InputAction.CallbackContext callbackContext)
    {
        if (_controller.isGrounded) _playerVelocity.y += Mathf.Sqrt(_jumpHeight * -3.0f * _gravityValue);
    }

    private void OnEnable() => _inputSystem.Player.Jump.performed += Jump;

    private void OnDisable() => _inputSystem.Player.Jump.performed -= Jump;
}