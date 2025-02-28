using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeOfTrap : MonoBehaviour, ITrap
{
    private enum TrapType { ALARM_TRAP, PUSH_TRAP, CAPTURE_TRAP}
    [SerializeField]
    private TrapType selectedTrap;


    public void ActivateTrap(EnemyTrapBehaviour enemy)
    {
        if(selectedTrap == TrapType.ALARM_TRAP)
        {
            enemy.TriggerAlarmTrap();
        }
        else if (selectedTrap == TrapType.PUSH_TRAP)
        {
            enemy.TriggerPushTrap();
        }
        else if (selectedTrap == TrapType.CAPTURE_TRAP)
        {
            enemy.TriggerCaptureTrap();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            ITrap trap = GetComponent<ITrap>();
            trap.ActivateTrap(other.GetComponentInParent<EnemyTrapBehaviour>());
        }
    }
}
