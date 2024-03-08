using System;
using System.Threading;
using UnityEngine;
using Zenject;
using static UnityEngine.InputSystem.InputAction;

public class DynamicFov : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private float _zoomSpeed = 10;
    [SerializeField] private float _minFov = 20;
    [SerializeField] private float _maxFov = 60;
    [SerializeField] private float _sprintFov = 120;

    private InputSystem _inputSystem;
    private CancellationTokenSource _cts;

    private float _currentFov;
    private bool _isSprinting;

    public event Action CameraZoomed;

    [Inject]
    private void Construct(InputSystem inputSystem) => _inputSystem = inputSystem;
    
    private void OnEnable()
    {
        _inputSystem.Player.CameraZoom.performed += ChangeFOV;
        _inputSystem.Enable();
    }

    private void Start() => _currentFov = _mainCamera.fieldOfView;


    private void ChangeFOV(CallbackContext _)
    {
        if (_isSprinting) return;

        _currentFov -= _inputSystem.Player.CameraZoom.ReadValue<Vector2>().y * _zoomSpeed;
        _currentFov = Mathf.Clamp(_currentFov, _minFov, _maxFov);
        _mainCamera.fieldOfView = _currentFov;
        CameraZoomed?.Invoke();
    }

    public async void IncreaseFOV(CallbackContext _)
    {
        if (_inputSystem.Player.Crouch.IsPressed()) return;

        _isSprinting = true;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        while (_currentFov < _sprintFov)
        {
            await Awaitable.WaitForSecondsAsync(0.04f, _cts.Token);
            _currentFov = Mathf.Lerp(_currentFov, _sprintFov, 0.18f);
            _mainCamera.fieldOfView = _currentFov;
        }
    }

    public async void DecreaseFOV(CallbackContext _)
    {
        _isSprinting = false;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        while (_currentFov > _maxFov)
        {
            await Awaitable.WaitForSecondsAsync(0.04f, _cts.Token);
            _currentFov = Mathf.Lerp(_currentFov, _maxFov, 0.18f);
            _mainCamera.fieldOfView = _currentFov;
        }
    }

    public float CurrentFov() => _currentFov;

    public float MinFov() => _minFov;

    public float MaxFov() => _maxFov;

    private void OnDisable()
    {
        _inputSystem.Player.CameraZoom.performed -= ChangeFOV;
        _inputSystem.Disable();
    }
}
