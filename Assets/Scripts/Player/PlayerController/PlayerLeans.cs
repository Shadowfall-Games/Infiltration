using System.Threading;
using UnityEngine;
using Zenject;
using static UnityEngine.InputSystem.InputAction;

public class PlayerLeans : MonoBehaviour
{
    [SerializeField] private Transform _leanPivot;

    [SerializeField] private float _leanAngle;
    [SerializeField] private float _leanSmoothing;
    private float _currentLean;
    private float _targetLean;
    private float _leanVelocity;

    private InputSystem _inputSystem;
    private CancellationTokenSource _cts;

    [Inject]
    private void Construct(InputSystem inputSystem) => _inputSystem = inputSystem;

    private void OnEnable()
    {
        _inputSystem.Player.LeanRight.performed += LeanRight;
        _inputSystem.Player.LeanLeft.performed += LeanLeft;
        _inputSystem.Player.LeanRight.canceled += Reset;
        _inputSystem.Player.LeanLeft.canceled += Reset;
        _inputSystem.Player.Enable();
    }

    private void LeanLeft(CallbackContext _)
    {
        _targetLean = _leanAngle;
        Lean();
    }

    private void LeanRight(CallbackContext _)
    {
        _targetLean = -_leanAngle;
        Lean();
    }

    private void Reset(CallbackContext _)
    {
        _targetLean = 0;
        Lean();
    }

    private async void Lean()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        while (+(_leanPivot.localRotation.z - _targetLean) <= 0.2)
        {
            Debug.Log(+(_leanPivot.localRotation.z - _targetLean) <= 0.2);
            Debug.Log(+(_leanPivot.localRotation.z - _targetLean));
            await Awaitable.WaitForSecondsAsync(0.03f, _cts.Token);
            _currentLean = Mathf.SmoothDamp(_currentLean, _targetLean, ref _leanVelocity, _leanSmoothing);
            _leanPivot.localEulerAngles = new Vector3(_leanPivot.localRotation.x, _leanPivot.localRotation.y, _currentLean);
        }
    }

    private void OnDisable()
    {
        _inputSystem.Player.LeanRight.performed -= LeanRight;
        _inputSystem.Player.LeanLeft.performed -= LeanLeft;
        _inputSystem.Player.LeanRight.canceled -= Reset;
        _inputSystem.Player.LeanLeft.canceled -= Reset;
        _inputSystem.Player.Disable();
    }
}