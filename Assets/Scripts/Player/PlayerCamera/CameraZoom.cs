using System;
using UnityEngine;
using Zenject;
using static UnityEngine.InputSystem.InputAction;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private float _zoomSpeed = 10;
    [SerializeField] private float _minFOV = 20;
    [SerializeField] private float _maxFOV = 60;

    private InputSystem _inputSystem;

    private float _currentFOV;

    public event Action CameraZoomed;

    [Inject]
    private void Construct(InputSystem inputSystem) => _inputSystem = inputSystem;
    private void OnEnable()
    {
        _inputSystem.Player.CameraZoom.performed += ChangeFOV;
        _inputSystem.Enable();
    }

    private void Start() => _currentFOV = _mainCamera.fieldOfView;


    private void ChangeFOV(CallbackContext _)
    {
        _currentFOV -= _inputSystem.Player.CameraZoom.ReadValue<Vector2>().y * _zoomSpeed;
        _currentFOV = Mathf.Clamp(_currentFOV, _minFOV, _maxFOV);
        _mainCamera.fieldOfView = _currentFOV;
        CameraZoomed?.Invoke();
    }

    private void OnDisable()
    {
        _inputSystem.Player.CameraZoom.performed -= ChangeFOV;
        _inputSystem.Disable();
    }
}