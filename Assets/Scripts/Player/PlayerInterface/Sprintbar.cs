using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Sprintbar : MonoBehaviour
{
    [SerializeField] private float _sprintEnergy = 100;
    [SerializeField] private float _lostEnergyAmount = 10;
    [SerializeField] private float _timeUntilReplenishment = 4;

    [SerializeField] private int _recoveredEneryAmount = 5;

    private float _currentSprintEnergy;

    private Slider _sprintBar;

    private CancellationTokenSource _cts = new CancellationTokenSource();

    private void Start()
    {
        _currentSprintEnergy = _sprintEnergy;
        _sprintBar = GetComponent<Slider>();
    }

    public void DecreaseEnergy()
    {
        if ((int)Math.Round(_currentSprintEnergy) != 0) _currentSprintEnergy -= _lostEnergyAmount * Time.deltaTime * 2;
        _sprintBar.value = _currentSprintEnergy/100;
    }

    public async void IncreaseEnergy()
    {
        await Task.Delay(_recoveredEneryAmount * 1000, _cts.Token);
        if ((int) Math.Round(_currentSprintEnergy) != _sprintEnergy) _currentSprintEnergy += _recoveredEneryAmount * Time.deltaTime * 2;
        _sprintBar.value = _currentSprintEnergy/100;
    }

    public void CancelTask(InputAction.CallbackContext _)
    {
        _cts.Cancel();
        _cts = new CancellationTokenSource();
    }
}