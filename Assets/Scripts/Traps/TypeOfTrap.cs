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

    public void ActivateTrap(Vector3 pos)
    {
        if(selectedTrap == AbilitiesEnum.ALARM_TRAP)
        {
            GameManager.Instance.TrapManager.TriggerAlarmTrap();
        }
        else if (selectedTrap == AbilitiesEnum.PUSH_TRAP)
        {
            GameManager.Instance.TrapManager.TriggerPushTrap();
        }
        else if (selectedTrap == AbilitiesEnum.CAPTURE_TRAP)
        {
            GameManager.Instance.TrapManager.TriggerCaptureTrap(trapOwner,pos);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ENEMY"))
        {
            trap.ActivateTrap(transform.position);
        }
    }
}
