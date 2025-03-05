using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeOfTrap : MonoBehaviour, ITrap
{
    [SerializeField] AbilitiesEnum selectedTrap;
    [SerializeField] private PlayerEnum trapOwner;

    public PlayerEnum TrapOwner
    {
        get { return trapOwner; }
        set { trapOwner = value; }
    }

    private ITrap trap;
    private void Start()
    {
        trap = GetComponent<ITrap>();
    }

    public void ActivateTrap()
    {
        if(selectedTrap == AbilitiesEnum.ALARM_TRAP)
        {
            TrapManager.Instance.TriggerAlarmTrap();
        }
        else if (selectedTrap == AbilitiesEnum.PUSH_TRAP)
        {
            TrapManager.Instance.TriggerPushTrap();
        }
        else if (selectedTrap == AbilitiesEnum.CAPTURE_TRAP)
        {
            TrapManager.Instance.TriggerCaptureTrap();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ENEMY"))
        {
            trap.ActivateTrap();
        }
    }
}
