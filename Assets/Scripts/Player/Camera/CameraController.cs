using UnityEngine;
using Zenject;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _rotateSpeed;
    [SerializeField] private float _clampAngle;

    [SerializeField] private GameObject _camera;

    private InputSystem _inputSystem;

    private Vector2 _rotation;

    [Inject]
    private void Construct(InputSystem inputSystem) => _inputSystem = inputSystem;

    private void Start() => Cursor.lockState = CursorLockMode.Locked;

    private void Update()
    {
        Vector2 _mouseDelta = _inputSystem.Player.CameraLook.ReadValue<Vector2>();

        if (_mouseDelta.sqrMagnitude < 0.1f) return;

        _mouseDelta *= _rotateSpeed * Time.deltaTime;
        _rotation.y += _mouseDelta.x;
        _rotation.x = Mathf.Clamp(_rotation.x - _mouseDelta.y, -_clampAngle, _clampAngle);

        _camera.transform.rotation = Quaternion.Euler(_rotation.x, _rotation.y, 0);
    }
}