using Player;
using UnityEngine;
using UnityEngine.UI;

public class Sprintbar : MonoBehaviour
{
    [SerializeField] private PlayerMovement _playerMovement;

    [SerializeField] private Image _sprintbarFilling;


    private void Start() => _playerMovement.CurrentEnergy += DisplayEnergy;

    private void DisplayEnergy(float energy)
    {
        _sprintbarFilling.fillAmount = energy / 100;
    }

    private void OnDestroy() => _playerMovement.CurrentEnergy -= DisplayEnergy;
}