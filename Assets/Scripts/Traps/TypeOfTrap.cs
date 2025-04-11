using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class TypeOfTrap : MonoBehaviour, ITrap
{
    [SerializeField] AbilitiesEnum selectedTrap;
    [SerializeField] private PlayerEnum trapOwner;
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private VisualEffect _optionalVisualEffect;

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

    public void ActivateTrap(Transform tr)
    {
        if(selectedTrap == AbilitiesEnum.ALARM_TRAP)
        {
            GameManager.Instance.TrapManager.TriggerAlarmTrap(tr);
        }
        else if (selectedTrap == AbilitiesEnum.PUSH_TRAP)
        {
            GameManager.Instance.TrapManager.TriggerPushTrap(tr);
        }
        else if (selectedTrap == AbilitiesEnum.CAPTURE_TRAP)
        {
            GameManager.Instance.TrapManager.TriggerCaptureTrap(trapOwner,tr);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ENEMY"))
        {
            trap.ActivateTrap(transform);
            if (_particleSystem != null)
            {
                _particleSystem.Play();
            }
            else if (_optionalVisualEffect != null)
            {
                _optionalVisualEffect.Play();
            }
        }
    }
}
