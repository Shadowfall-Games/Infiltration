using UnityEngine;
using Cinemachine;
using Zenject;

public class CinemachineExtensionPOV : CinemachineExtension
{
    [SerializeField] private float _clampAngle = 80f;
    [SerializeField] private float _horizontalSpeed = 10f;
    [SerializeField] private float _verticalSpeed = 10f;

    private InputSystem _inputSystem;

    private Vector3 _startingRot;

    [Inject]
    private void Construct(InputSystem inputSystem) => _inputSystem = inputSystem;

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (vcam.Follow)
            if (stage == CinemachineCore.Stage.Aim)
            {
                if (_startingRot == null) _startingRot = transform.localRotation.eulerAngles;
                Vector2 deltaInput = _inputSystem.Player.CameraLook.ReadValue<Vector2>();
                Vector2 startingRotation = new Vector2(_startingRot.x += deltaInput.x * _verticalSpeed * Time.deltaTime, startingRotation.y = Mathf.Clamp(_startingRot.y += deltaInput.y * _horizontalSpeed * Time.deltaTime, -_clampAngle, _clampAngle));
                state.RawOrientation = Quaternion.Euler(-startingRotation.y, startingRotation.x, 0f);
            }
    }
}