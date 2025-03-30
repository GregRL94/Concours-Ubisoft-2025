using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using UnityEngine.Events;


public class TrapManager : MonoBehaviour
{
    public Action OnTrapTriggered;
    Rigidbody _rb;
    Animator _animator;

    [SerializeField]
    private NavMeshAgent _agent;
    [SerializeField]
    private GameObject _indicator;

    [Header("Alarm Trap Effects")]
    [SerializeField] float fleeSpeedMultiplier = 2f;
    [SerializeField] float fleeRange = 8f;
    [SerializeField] float timeTillAlarmIndicatorAppear = 0.5f;
    [SerializeField] int timeTillFlee = 1;
    float agentInitialSpeed;
    private Vector3 fleePosition;
    private bool hasFleeToDestination;
    Vector3[] fleeDirections =
    {
        new Vector3(1, 0, 0),
        new Vector3(-1, 0, 0),
        new Vector3(0, 0, 1),
        new Vector3(0, 0, -1)
    };
    [SerializeField] int _alarmCaptureValue = 0;

    [Header("Push Trap Effects")]
    [SerializeField] float pushDistance = 15f;
    float timeTillPushIndicatorAppear = 0.05f;
    [SerializeField] private float timeTillEnemyStop = 0.4f;
    [SerializeField] float stunPushDuration = 4f;
    [SerializeField] int _pushCaptureValue = 0;

    [Header("Capture Trap Effects")]
    [SerializeField] float timeTillCaptureIndicatorAppear = 0f;
    [SerializeField] float captureDuration = 5f;
    [SerializeField] int _captureCaptureValue = 0;

    [SerializeField]
    public RobberCapture robberCapture;

    private void Start()
    {

        // Actions for later
        //SubscribeToTrapEvents();
    }
    public void SetRobber(NavMeshAgent agent, Rigidbody rb, GameObject indicator, RobberCapture robber, Animator animator)
    {
        _agent = agent;
        _rb = rb;
        _indicator = indicator;
        robberCapture = robber;
        _animator = animator;
    }

    #region Action Traps (Later)
    //private void SubscribeToTrapEvents()
    //{
    //    OnTrapTriggered += TriggerAlarmTrap;
    //    //OnTrapTriggered += ShowFleeIndicator;
    //    OnTrapTriggered += PlayEscapeSound;
    //    OnTrapTriggered += TriggerFleeAnimation;
    //}

    //public void UnsubscribeFromTrapEvents()
    //{
    //    OnTrapTriggered -= TriggerAlarmTrap;
    //    //OnTrapTriggered -= ShowFleeIndicator;
    //    OnTrapTriggered -= PlayEscapeSound;
    //    OnTrapTriggered -= TriggerFleeAnimation;
    //}



    // TRY ADD COROUTINE THERE 
    //public void ShowFleeIndicator()
    //{
    //    if (indicator != null)
    //    {
    //        indicator.SetActive(true);
    //    }
    //}

    public void PlayEscapeSound()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }


    public void TriggerFleeAnimation()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Flee"); 
        }
    }
    #endregion

    #region Alarm Trigger Behaviour
    public void TriggerAlarmTrap(Transform tr, PlayerEnum trapOwner = PlayerEnum.NONE)
    {
        // Stops enemy agent
        if (_agent != null)
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            _animator.SetTrigger("Surpris");
            _animator.SetBool("Cours", false);
        }

        // Reaction time before flee
        StartCoroutine(RunAwayDelay(tr,trapOwner));
    }

    private IEnumerator RunAwayDelay(Transform tr, PlayerEnum trapOwner)
    {
        //Run Away for RobberBehaviour
        robberCapture?.GetSifled(trapOwner, _alarmCaptureValue);

        _animator.SetBool("Cours", true);

        yield return new WaitForSeconds(timeTillAlarmIndicatorAppear);
        //Destroy(alarmTrap);
        if (_indicator == null) yield break;
        _indicator?.SetActive(true);
        tr.GetComponent<CapsuleCollider>().enabled = false;

    }
    #endregion

    #region Push Trigger Behaviour

    public void TriggerPushTrap(Transform tr, PlayerEnum trapOwner = PlayerEnum.NONE)
    {
        robberCapture?.GetSifled(trapOwner, _pushCaptureValue);
        _agent?.GetComponentInChildren<Animator>()?.SetBool("EstRepousser", true);

        if (_agent != null)
        {
            _agent.isStopped = true;  
            _agent.updatePosition = false; 
        }

        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
            Vector3 pushDirection = tr.forward * pushDistance;
            _rb.AddForce(pushDirection, ForceMode.Impulse);
        }

        StartCoroutine(StunAfterPush(trapOwner, tr));
    }

    private IEnumerator StunAfterPush(PlayerEnum trapOwner, Transform tr)
    {
        tr.GetComponent<CapsuleCollider>().enabled = false;

        yield return new WaitForSeconds(timeTillPushIndicatorAppear);
        _indicator?.SetActive(true);

        yield return new WaitForSeconds(timeTillEnemyStop);

        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
        }

        yield return new WaitForSeconds(stunPushDuration);

        if (_agent != null)
        {
            _agent.Warp(_rb.position);

            _agent.isStopped = false;
            _agent.updatePosition = true;
            _agent.GetComponentInChildren<Animator>()?.SetBool("EstRepousser", false);
        }

        _indicator?.SetActive(false);
    }

    #endregion



    #region Capture Trigger Behaviour
    public void TriggerCaptureTrap(PlayerEnum trapOwner, Transform tr)
    {
        // Disable collider, movement and reset velocity to zero
        if (_agent != null)
        {
            print("_agent.isStopped " + _agent.isStopped);
            print("capture trap pos: " + tr.position);
            //Vector3 test = new Vector3(34.5400009f, -0.141920924f, 9.07999992f);
            _agent.SetDestination(tr.position);
            _agent.isStopped = true;
        }
        if (_rb != null)
        {
            _rb.angularVelocity = Vector3.zero;
            _rb.velocity = Vector3.zero;
            print("Angular ");
            print("_rb.velocity " + _rb.velocity);
        }

        StartCoroutine(StunFromCapture(trapOwner, tr));
    }

    IEnumerator StunFromCapture(PlayerEnum trapOwner, Transform tr)
    {
        robberCapture?.GetSifled(trapOwner, _captureCaptureValue);
        
        yield return new WaitForSeconds(timeTillCaptureIndicatorAppear);
        if(_indicator == null) yield break;
        _indicator?.SetActive(true);

        // Capture for certain amount of seconds
        robberCapture.StartVulnerability();
        yield return new WaitForSeconds(captureDuration);

        //Destroy(captureTrap);
        if (_agent != null)
        {
            _agent.isStopped = false;
        }
        else yield break;

        if (_rb != null)
        {
            _rb.angularVelocity = Vector3.zero;
            _rb.velocity = Vector3.zero;
        }
        
        _indicator?.SetActive(false);
        tr.GetComponent<CapsuleCollider>().enabled = false;
        robberCapture?.StopVulnerability();
    }
    #endregion
}
// TODOS:
// Different indicator for each trap ??? 
