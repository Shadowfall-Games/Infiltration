using UnityEngine;

public class CameraAudio : MonoBehaviour
{
    [SerializeField] private AudioSource _cameraAudioSource;
    [SerializeField] private AudioClip _cameraZoomAudioClip;
    [SerializeField] private CameraZoom _cameraZoom;

    private void OnEnable() => _cameraZoom.CameraZoomed += PlayAudio;

    private void PlayAudio()
    {
        if (_cameraZoom.CurrentFOV() != _cameraZoom.MaxFOV() && _cameraZoom.CurrentFOV() != _cameraZoom.MinFov()) _cameraAudioSource.PlayOneShot(_cameraZoomAudioClip);
    }

    private void OnDisable() => _cameraZoom.CameraZoomed -= PlayAudio;
}