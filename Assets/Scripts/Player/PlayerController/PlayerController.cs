using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 2.0f;

    private CharacterController _controller;
    private Vector3 _velocity;

    [Header("Jump")]
    [SerializeField] private float _jumpHeight = 1f;

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
    public float _speed;

    private const float GRAVITY = -18;

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
        if (_controller.isGrounded && _velocity.y < 0) _velocity.y = 0f;

        Vector2 direction = _inputSystem.Player.Movement.ReadValue<Vector2>();
        Vector3 move = new Vector3(direction.x, 0f, direction.y);
        move = _camera.transform.forward * move.z + _camera.transform.right * move.x;
        move.y = 0f;
        _velocity.y += GRAVITY * Time.deltaTime;
        move += _velocity * Time.deltaTime;
        _controller.Move(move * Time.deltaTime * _speed);

        if (_isSprinting == true && !_isCrouching) { DecreaseSprintEnergy(); }
        else { if (_currentSprintDuration != _sprintMaxDuration) { IncreaseSprintEnergy(); } }
    }

    private void Jump(InputAction.CallbackContext callbackContext)
    {
        if (_isCrouching) Crouch();
        if (_controller.isGrounded) _velocity.y += Mathf.Sqrt(_jumpHeight * -2f * GRAVITY);
    }

    private void Crouch()
    {
        if (!_controller.isGrounded) return;
        _isCrouching = !_isCrouching;

        if (_isCrouching == true) CrouchMechanic(_crouchYScale, _crouchSpeed, true);
        else CrouchMechanic(1f, _walkSpeed, false);
    }
    private void CrouchMechanic(float crouchHeight, float speed, bool isCrouch)
    {
        _controller.transform.localScale = new Vector3(transform.localScale.x, crouchHeight, _controller.transform.localScale.z);
        _speed = speed;

        if (isCrouch) this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y - 0.12f, this.transform.position.z);
        else this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 0.12f, this.transform.position.z);
    }

    private void Sprint()
    {
        if (!_controller.isGrounded) { return; }
        _isSprinting = !_isSprinting;

        if (_isCrouching) Crouch();

        if (_isSprinting == true) { SprintMechanic(_sprintSpeed); }
        else { SprintMechanic(_walkSpeed); }
    }

    private void SprintMechanic(float speed)
    {
        if (_currentSprintDuration != 0) _speed = speed; 
    }

    private void DecreaseSprintEnergy()
    {
        if ((int)Math.Round(_currentSprintDuration) != 0 && !_isCrouching) _currentSprintDuration -= _takenAwayEnergyValue * Time.deltaTime;
        else Sprint();
        _sprintBar.value = _currentSprintDuration/100;
    }

    private void IncreaseSprintEnergy()
    {
        if ((int)Math.Round(_currentSprintDuration) != _sprintMaxDuration) _currentSprintDuration += _takenAwayEnergyValue * Time.deltaTime;
        _sprintBar.value = _currentSprintDuration/100;
    }

    private void Crouch(InputAction.CallbackContext callbackContext) => Crouch();

    private void Sprint(InputAction.CallbackContext callbackContext) => Sprint();

    private void OnEnable()
    {
        _inputSystem.Player.Jump.performed += Jump;
        _inputSystem.Player.Crouch.performed += Crouch;
        _inputSystem.Player.Sprint.performed += Sprint;
        _inputSystem.Player.Sprint.canceled += Sprint;
    }

    private void OnDisable()
    {
        _inputSystem.Player.Jump.performed -= Jump;
        _inputSystem.Player.Crouch.performed -= Crouch;
        _inputSystem.Player.Sprint.performed -= Sprint;
    }
}