using System;
using System.Threading;
using UnityEngine;
using Zenject;
using static UnityEngine.InputSystem.InputAction;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private float _zoomSpeed = 10;
    [SerializeField] private float _minFOV = 20;
    [SerializeField] private float _maxFOV = 60;
    [SerializeField] private float _sprintFOV = 120;

    private InputSystem _inputSystem;
    private CancellationTokenSource _cts;

    private float _currentFOV;
    private bool _isSprinting;

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
        if (_isSprinting) return;

        _currentFOV -= _inputSystem.Player.CameraZoom.ReadValue<Vector2>().y * _zoomSpeed;
        _currentFOV = Mathf.Clamp(_currentFOV, _minFOV, _maxFOV);
        _mainCamera.fieldOfView = _currentFOV;
        CameraZoomed?.Invoke();
    }

    public async void IncreaseFOV(CallbackContext _)
    {
        _isSprinting = true;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        while (_currentFOV < _sprintFOV)
        {
            await Awaitable.WaitForSecondsAsync(0.04f, _cts.Token);
            _currentFOV = Mathf.Lerp(_currentFOV, _sprintFOV, 0.18f);
            _mainCamera.fieldOfView = _currentFOV;
        }
    }

    public async void DecreaseFOV(CallbackContext _)
    {
        _isSprinting = false;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        while (_currentFOV > _maxFOV)
        {
            await Awaitable.WaitForSecondsAsync(0.04f, _cts.Token);
            _currentFOV = Mathf.Lerp(_currentFOV, _maxFOV, 0.18f);
            _mainCamera.fieldOfView = _currentFOV;
        }
    }

    public float CurrentFOV() => _currentFOV;

    public float MinFov() => _minFOV;

    public float MaxFOV() => _maxFOV;

    private void OnDisable()
    {
        _inputSystem.Player.CameraZoom.performed -= ChangeFOV;
        _inputSystem.Disable();
    }
}