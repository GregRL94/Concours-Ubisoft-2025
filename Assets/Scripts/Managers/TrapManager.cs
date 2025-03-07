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

    [SerializeField]
    private NavMeshAgent _agent;
    public GameObject alarmTrap;
    public GameObject pushTrap;
    public GameObject captureTrap;
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

    [Header("Push Trap Effects")]
    [SerializeField] float pushDistance = 15f;
    float timeTillPushIndicatorAppear = 0.05f;
    [SerializeField] private float timeTillEnemyStop = 0.4f;
    [SerializeField] float stunPushDuration = 4f;

    [Header("Capture Trap Effects")]
    [SerializeField] float timeTillCaptureIndicatorAppear = 0f;
    [SerializeField] float captureDuration = 5f;

    [SerializeField]
    public RobberCapture robberCapture;

    private void Start()
    {

        // Actions for later
        //SubscribeToTrapEvents();
    }
    public void SetRobber(NavMeshAgent agent, Rigidbody rb, GameObject indicator, RobberCapture robber)
    {
        _agent = agent;
        _rb = rb;
        _indicator = indicator;
        robberCapture = robber;
    }

    #region Action Traps (Later)
    private void SubscribeToTrapEvents()
    {
        OnTrapTriggered += TriggerAlarmTrap;
        //OnTrapTriggered += ShowFleeIndicator;
        OnTrapTriggered += PlayEscapeSound;
        OnTrapTriggered += TriggerFleeAnimation;
    }

    public void UnsubscribeFromTrapEvents()
    {
        OnTrapTriggered -= TriggerAlarmTrap;
        //OnTrapTriggered -= ShowFleeIndicator;
        OnTrapTriggered -= PlayEscapeSound;
        OnTrapTriggered -= TriggerFleeAnimation;
    }



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

    private void Update()
    {
        //if (alarmTrap != null && alarmTrap.activeInHierarchy)
        //    agent.SetDestination(alarmTrap.transform.position);

        //if (pushTrap != null && pushTrap.activeInHierarchy && agent.enabled)
        //    agent.SetDestination(pushTrap.transform.position);

        //if (captureTrap != null && captureTrap.activeInHierarchy)
        //    agent.SetDestination(captureTrap.transform.position);
    }

    #region Alarm Trigger Behaviour
    public void TriggerAlarmTrap()
    {
        // Stops enemy agent
        if (_agent != null)
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
        }

        // Reaction time before flee
        StartCoroutine(RunAwayDelay());
    }

    private IEnumerator RunAwayDelay()
    {
        yield return new WaitForSeconds(timeTillAlarmIndicatorAppear);
        Destroy(alarmTrap);
        if (_indicator == null) yield break;
        _indicator?.SetActive(true);

        //Run Away for RobberBehaviour
        robberCapture?.GetSifled(10);

        // Optional - Reaction to alarm -> Turning right to left
        /*yield return new WaitForSeconds(0.5f);
        StartCoroutine(SmoothRotate(90f, 0.1f));
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(SmoothRotate(-180f, 0.1f));

        yield return new WaitForSeconds(timeTillFlee);

        // Regain his movement
        if (_agent != null)
        {
            _agent.isStopped = false;
        }

        // Determine a random direction to flee
        FleeFromAlarmTrap();

        // Calculate every frame till enemy has reached his flee position
        yield return StartCoroutine(FleeDestinationReached());*/
    }

    private void FleeFromAlarmTrap()
    {
        // Flee to random direction
        int randIndex = UnityEngine.Random.Range(0, fleeDirections.Length);
        Vector3 fleeDirection = fleeDirections[randIndex];

        // Flee position 
        fleePosition = transform.position + fleeDirection * fleeRange; 
        _agent.speed *= fleeSpeedMultiplier;
        hasFleeToDestination = true;
    }

    private IEnumerator FleeDestinationReached()
    {
        while (hasFleeToDestination)
        {
            _agent.SetDestination(fleePosition);
            
            // Check every frame if reached its flee destination
            if (Vector3.Distance(transform.position, fleePosition) <= 0.1f)
            {
                _agent.speed = agentInitialSpeed;
                hasFleeToDestination = false;
                _indicator.SetActive(false);
                Destroy(alarmTrap); // todo: when to destroy alarm gameobject ???
            }
            yield return null; // calculates it in every frame
        }
    }

    private IEnumerator SmoothRotate(float angle, float duration)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + angle, transform.eulerAngles.z);

        float elapsedTime = 0f;

        // Quaternion rotation based on the duration
        while (elapsedTime < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation; 
    }
    #endregion

    #region Push Trigger Behaviour
    public void TriggerPushTrap()
    {
        // Deactivate enemy agent
        if (_agent != null)
        {
            _agent.isStopped = true;
        }

        // Resets velocity to zero
        if (_rb != null)
        {
            //rb.isKinematic = false;
            _rb.velocity = Vector3.zero;
        }

        // Pushes enemy from a forward direction and apply a force instant force 
        Vector3 pushDirection = _agent.transform.forward * pushDistance; 
        _rb?.AddForce(pushDirection , ForceMode.Impulse); 

        StartCoroutine(StunAfterPush());
    }

    IEnumerator StunAfterPush()
    {
        yield return new WaitForSeconds(timeTillPushIndicatorAppear);
        if(_indicator == null)yield break;
        _indicator?.SetActive(true);

        // Push duration till Enemy freezes
        yield return new WaitForSeconds(timeTillEnemyStop);
        Destroy(pushTrap);


        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
            //rb.isKinematic = true;
        }
        else yield break;


        // Stun duration after being pushed 
        yield return new WaitForSeconds(stunPushDuration);

        if (_agent != null)
        {
            _agent.isStopped = false;
        }
        else yield break;

        _indicator?.SetActive(false);
        robberCapture?.GetSifled(20);
    }
    #endregion
  
    #region Capture Trigger Behaviour
    public void TriggerCaptureTrap()
    {
        // Disable collider, movement and reset velocity to zero
        captureTrap.GetComponent</*CapsuleCollider*/Collider>().enabled = false;

        if (_agent != null)
        {
            _agent.isStopped = true;
        }

        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
        }

        StartCoroutine(StunFromCapture());
    }

    IEnumerator StunFromCapture()
    {
        yield return new WaitForSeconds(timeTillCaptureIndicatorAppear);
        if(_indicator == null) yield break;
        _indicator?.SetActive(true);

        // Capture for certain amount of seconds
        robberCapture.StartVulnerability();
        yield return new WaitForSeconds(captureDuration);
        captureTrap.GetComponent<CapsuleCollider>().enabled = true;
        Destroy(captureTrap);

        if (_agent != null)
        {
            _agent.isStopped = false;
        }
        else yield break;

        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
        }
        
        _indicator?.SetActive(false);
        robberCapture?.StopVulnerability();
    }
    #endregion
}
// TODOS:
// Different indicator for each trap ??? 
