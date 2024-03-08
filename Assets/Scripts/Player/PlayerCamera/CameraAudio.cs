using UnityEngine;

public class CameraAudio : MonoBehaviour
{
    [SerializeField] private AudioSource _cameraAudioSource;
    [SerializeField] private AudioClip _cameraZoomAudioClip;
    [SerializeField] private DynamicFov _dynamicFov;

    private float _currentFov;

    private void OnEnable() => _dynamicFov.CameraZoomed += PlayAudio;

    private void PlayAudio()
    {
        _currentFov = _dynamicFov.CurrentFov();
        if (_currentFov != _dynamicFov.MaxFov() && _currentFov != _dynamicFov.MinFov()) _cameraAudioSource.PlayOneShot(_cameraZoomAudioClip);
    }

    private void OnDisable() => _dynamicFov.CameraZoomed -= PlayAudio;
}