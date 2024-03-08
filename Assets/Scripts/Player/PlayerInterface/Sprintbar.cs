using Player;
using UnityEngine;
using UnityEngine.UI;

public class Sprintbar : MonoBehaviour
{
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private Image _sprintbarFilling;

    private void OnEnable() => _playerMovement.EnergyChanged += Display;

    private void Display(float energy) => _sprintbarFilling.fillAmount = energy / 100;

    private void OnDisable() => _playerMovement.EnergyChanged -= Display;
}