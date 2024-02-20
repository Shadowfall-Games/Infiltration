using UnityEngine;

public class CameraAudio : MonoBehaviour
{
    [SerializeField] private AudioSource _cameraAudioSource;
    [SerializeField] private AudioClip _cameraZoomAudioClip;
    [SerializeField] private CameraZoom _cameraZoom;

    private void OnEnable() => _cameraZoom.CameraZoomed += PlayAudio;

    private void PlayAudio() => _cameraAudioSource.PlayOneShot(_cameraZoomAudioClip);

    private void OnDisable() => _cameraZoom.CameraZoomed -= PlayAudio;
}