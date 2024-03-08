using UnityEngine;

public class CameraAudio : MonoBehaviour
{
    [SerializeField] private AudioSource _cameraAudioSource;
    [SerializeField] private AudioClip _cameraZoomAudioClip;
    [SerializeField] private DynamicFov _dynamicFov;

    private void OnEnable() => _dynamicFov.CameraZoomed += PlayAudio;

    private void PlayAudio()
    {
        if (_dynamicFov.CurrentFOV() != _dynamicFov.MaxFOV() && _dynamicFov.CurrentFOV() != _dynamicFov.MinFov()) _cameraAudioSource.PlayOneShot(_cameraZoomAudioClip);
    }

    private void OnDisable() => _dynamicFov.CameraZoomed -= PlayAudio;
}