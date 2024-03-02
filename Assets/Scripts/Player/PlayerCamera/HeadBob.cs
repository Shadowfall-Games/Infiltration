using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [SerializeField] private bool _isEnable = true;

    [SerializeField] private float _amplitude = 0.015f;
    [SerializeField] private float _frequency = 10;

    [SerializeField] private Camera _camera;

    private float _toggleSpeed = 3;

    private Vector3 _startPosition;
    private CharacterController _characterController;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _startPosition = _camera.transform.localPosition;
    }

    private void Update()
    {
        CheckMotion();
        ResetPosition();
    }

    private void PlayMotion(Vector3 motion)
    {
        _camera.transform.localPosition += motion;
    }

    private void CheckMotion()
    {
        if (!_isEnable) return;

        float speed = new Vector3(_characterController.velocity.x, 0, _characterController.velocity.z).magnitude;

        if (speed < _toggleSpeed || !_characterController.isGrounded) return;

        PlayMotion(FootstepMotion());
    }

    private void ResetPosition()
    {
        if (_camera.transform.localPosition == _startPosition) return;

        _camera.transform.localPosition = Vector3.Lerp(_camera.transform.localPosition, _startPosition, Time.deltaTime * 1);
    }

    private Vector3 FootstepMotion()
    {
        Vector3 position = Vector3.zero;
        position.x += Mathf.Cos(Time.time * _frequency / 3) * _amplitude * 2;
        position.y += Mathf.Sin(Time.time * _frequency) * _amplitude;
        return position;
    }
}