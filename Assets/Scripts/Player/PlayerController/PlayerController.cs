using PlasticPipe.PlasticProtocol.Messages;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _walkSpeed = 2.0f;
    [SerializeField] private float _crouchSpeed;
    [SerializeField] private float _jumpHeight = 1f;
    [SerializeField] private float _crouchHeight = 0.75f;

    [SerializeField] private GameObject _camera;

    private float _speed;

    private CharacterController _controller;

    private InputSystem _inputSystem;

    private Vector3 _velocity;

    private bool isCrouching;

    private const float GRAVITY = -9.81f;

    [Inject]
    private void Construct(InputSystem inputSystem) => _inputSystem = inputSystem;

    private void Start()
    {
        _speed = _walkSpeed;
        _controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (_controller.isGrounded && _velocity.y < 0) _velocity.y = 0f;

        Vector2 direction = _inputSystem.Player.Movement.ReadValue<Vector2>();
        Vector3 move = new Vector3(direction.x, 0f, direction.y);
        move = _camera.transform.forward * move.z + _camera.transform.right * move.x;
        move.y = 0f;

        _velocity.y += GRAVITY * Time.deltaTime;
        move += _velocity * Time.deltaTime;
        _controller.Move(move * Time.deltaTime * _speed);
    }

    private void Jump(InputAction.CallbackContext callbackContext)
    {
        if (isCrouching) Crouch();
        if (_controller.isGrounded) _velocity.y += Mathf.Sqrt(_jumpHeight * -3f * GRAVITY);
    }

    private void Crouch()
    {
        isCrouching = !isCrouching;

        if (isCrouching)
        {
            CrouchMechanic(_crouchHeight, _crouchSpeed);
        }
        else
        {
            CrouchMechanic(1f, _walkSpeed);
        }
    }

    private void Crouch(InputAction.CallbackContext callbackContext) => Crouch();

    private void CrouchMechanic(float crouchHeight, float speed)
    {
        _controller.transform.localScale = new Vector3(1, crouchHeight, 1);
        _speed = speed;
    }

    private void OnEnable()
    {
        _inputSystem.Player.Jump.performed += Jump;
        _inputSystem.Player.Crouch.performed += Crouch;
    }

    private void OnDisable()
    {
        _inputSystem.Player.Jump.performed -= Jump;
        _inputSystem.Player.Crouch.performed -= Crouch;
    }
}