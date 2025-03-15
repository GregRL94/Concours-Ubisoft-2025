using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Old_TypeOfTrap : MonoBehaviour, Old_ITrap
{
    [SerializeField] AbilitiesEnum selectedTrap;

    [SerializeField] private PlayerEnum trapOwner;
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
            Old_ITrap trap = GetComponent<Old_ITrap>();
            trap.ActivateTrap(other.GetComponentInParent<EnemyTrapBehaviour>());
        }
    }
}
