using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 2;

    private CharacterController _controller;
    private Vector3 _velocity;

    [Header("Jump")]
    [SerializeField] private float _jumpHeight = 1;

    [Header("Crouching")]
    [SerializeField] private float _crouchSpeed;
    [SerializeField] private float _crouchYScale;

    private bool _isCrouching;

    [Header("Sprinting")]
    [SerializeField] private float _sprintSpeed;
    [SerializeField] private float _sprintMaxDuration;
    [SerializeField] private float _takenAwayEnergyValue;
    [SerializeField] private Slider _sprintBar;

    private float _currentSprintDuration;

    private bool _isSprinting;

    [Header("Other")]
    [SerializeField] private GameObject _camera;

    private InputSystem _inputSystem;
    private float _speed;

    private const float GRAVITY = -3;

    [Inject]
    private void Construct(InputSystem inputSystem) => _inputSystem = inputSystem;

    private void Start()
    {
        _speed = _walkSpeed;
        _currentSprintDuration = _sprintMaxDuration;
        _sprintBar.value = _currentSprintDuration;
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (_controller.isGrounded) _velocity.y = 0;

        Vector2 direction = _inputSystem.Player.Movement.ReadValue<Vector2>();
        Vector3 move = _camera.transform.forward * direction.y + _camera.transform.right * direction.x;
        move.y = 0;
        _velocity.y += GRAVITY * Time.deltaTime;
        move += _velocity;
        _controller.Move(move * Time.deltaTime * _speed);

        if (_isSprinting == true) DecreaseSprintEnergy();
        else IncreaseSprintEnergy();
    }

    private void Jump(InputAction.CallbackContext callbackContext)
    {
        Stand();
        if (_controller.isGrounded) _velocity.y += Mathf.Sqrt(_jumpHeight * -2 * GRAVITY);
    }

    private void Crouch() => CrouchLogics(_crouchYScale, _crouchSpeed, true);

    private void Stand() => CrouchLogics(1, _walkSpeed, false);

    private void CrouchLogics(float crouchHeight, float speed, bool isCrouch)
    {
        if (!_controller.isGrounded) return;

        _controller.transform.localScale = new Vector3(transform.localScale.x, crouchHeight, _controller.transform.localScale.z);
        _speed = speed;

        if (isCrouch) { this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y - 0.12f, this.transform.position.z); _isCrouching = true; }
        else { this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 0.12f, this.transform.position.z); _isCrouching = false; }
    }

    private void StartSprint() => SprintLogics(_sprintSpeed, true);

    private void EndSprint() => SprintLogics(_walkSpeed, false);

    private void SprintLogics(float speed, bool isSprinting)
    {
        if (!_controller.isGrounded) return;

        _isSprinting = isSprinting;

        if (_isCrouching) Stand();

        if (_currentSprintDuration != 0) _speed = speed; 
    }

    private void DecreaseSprintEnergy()
    {
        if ((int)Math.Round(_currentSprintDuration) != 0 && !_isCrouching) _currentSprintDuration -= _takenAwayEnergyValue * Time.deltaTime;
        else EndSprint();
        _sprintBar.value = _currentSprintDuration/100;
    }

    private void IncreaseSprintEnergy()
    {
        if ((int)Math.Round(_currentSprintDuration) != _sprintMaxDuration) _currentSprintDuration += _takenAwayEnergyValue * Time.deltaTime;
        _sprintBar.value = _currentSprintDuration/100;
    }

    private void Crouch(InputAction.CallbackContext callbackContext) => Crouch();

    private void Stand(InputAction.CallbackContext callbackContext) => Stand();

    private void StartSprint(InputAction.CallbackContext callbackContext) => StartSprint();

    private void EndSprint(InputAction.CallbackContext callbackContext) => EndSprint();

    private void OnEnable()
    {
        _inputSystem.Player.Jump.performed += Jump;
        _inputSystem.Player.Crouch.performed += Crouch;
        _inputSystem.Player.Crouch.canceled += Stand;
        _inputSystem.Player.Sprint.performed += StartSprint;
        _inputSystem.Player.Sprint.canceled += EndSprint;
    }

    private void OnDisable()
    {
        _inputSystem.Player.Jump.performed -= Jump;
        _inputSystem.Player.Crouch.performed -= Crouch;
        _inputSystem.Player.Crouch.performed -= Stand;
        _inputSystem.Player.Sprint.performed -= StartSprint;
        _inputSystem.Player.Crouch.performed -= EndSprint;
    }
}