using Player;
using UnityEngine;
using UnityEngine.UI;

public class Sprintbar : MonoBehaviour
{
    [SerializeField] private PlayerMovement _playerMovement;

    private Slider _sprintBar;


    private void Start()
    {
        _sprintBar = GetComponent<Slider>();
        _playerMovement.CurrentEnergy += DisplayEnergy;
    }

    private void DisplayEnergy(float energy)
    {
        _sprintBar.value = energy / 100;
    }

    private void OnDestroy() => _playerMovement.CurrentEnergy -= DisplayEnergy;
}