using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeOfTrap : MonoBehaviour, ITrap
{
    [SerializeField] AbilitiesEnum selectedTrap;

    private PlayerEnum trapOwner;
    public PlayerEnum TrapOwner
    {
        get { return trapOwner; }
        set { trapOwner = value; }
    }

    public void ActivateTrap(EnemyTrapBehaviour enemy)
    {
        if(selectedTrap == AbilitiesEnum.ALARM_TRAP)
        {
            enemy.TriggerAlarmTrap();
        }
        else if (selectedTrap == AbilitiesEnum.PUSH_TRAP)
        {
            enemy.TriggerPushTrap();
        }
        else if (selectedTrap == AbilitiesEnum.CAPTURE_TRAP)
        {
            enemy.TriggerCaptureTrap();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ENEMY"))
        {
            ITrap trap = GetComponent<ITrap>();
            trap.ActivateTrap(other.GetComponentInParent<EnemyTrapBehaviour>());
        }
    }
}
