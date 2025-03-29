using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class EnemyTrapBehaviour : MonoBehaviour
{
    public Action OnTrapTriggered;
    Rigidbody rb;

    public NavMeshAgent agent;
    public GameObject alarmTrap;
    public GameObject pushTrap;
    public GameObject captureTrap;
    public GameObject indicator;

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


    private void Start()
    {
        agentInitialSpeed = agent.speed;
        rb = GetComponent<Rigidbody>();

        // Actions for later
        //SubscribeToTrapEvents();
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
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // Reaction time before flee
        StartCoroutine(RunAwayDelay());
    }

    private IEnumerator RunAwayDelay()
    {
        yield return new WaitForSeconds(timeTillAlarmIndicatorAppear);
        indicator.SetActive(true);

        // Optional - Reaction to alarm -> Turning right to left
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(SmoothRotate(90f, 0.1f));
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(SmoothRotate(-180f, 0.1f));

        yield return new WaitForSeconds(timeTillFlee);

        // Regain his movement
        if (agent != null)
        {
            agent.isStopped = false;
        }

        // Determine a random direction to flee
        FleeFromAlarmTrap();

        // Calculate every frame till enemy has reached his flee position
        yield return StartCoroutine(FleeDestinationReached());
    }

    private void FleeFromAlarmTrap()
    {
        // Flee to random direction
        int randIndex = UnityEngine.Random.Range(0, fleeDirections.Length);
        Vector3 fleeDirection = fleeDirections[randIndex];

        // Flee position 
        fleePosition = transform.position + fleeDirection * fleeRange; 
        agent.speed *= fleeSpeedMultiplier;
        hasFleeToDestination = true;
    }

    private IEnumerator FleeDestinationReached()
    {
        while (hasFleeToDestination)
        {
            agent.SetDestination(fleePosition);
            
            // Check every frame if reached its flee destination
            if (Vector3.Distance(transform.position, fleePosition) <= 0.1f)
            {
                agent.speed = agentInitialSpeed;
                hasFleeToDestination = false;
                indicator.SetActive(false);
                Destroy(alarmTrap); 
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
        if (agent != null)
        {
            agent.isStopped = true;
        }

        // Resets velocity to zero
        if (rb != null)
        {
            //rb.isKinematic = false;
            rb.velocity = Vector3.zero;
        }

        // Pushes enemy from a forward direction and apply a force instant force 
        Vector3 pushDirection = transform.forward * pushDistance; 
        rb.AddForce(pushDirection , ForceMode.Impulse); 

        StartCoroutine(StunAfterPush());
    }

    IEnumerator StunAfterPush()
    {
        yield return new WaitForSeconds(timeTillPushIndicatorAppear);
        indicator.SetActive(true);

        // Push duration till Enemy freezes
        yield return new WaitForSeconds(timeTillEnemyStop);


        if (rb != null)
        {
            rb.velocity = Vector3.zero; 
            //rb.isKinematic = true;
        }

        Destroy(pushTrap);

        // Stun duration after being pushed 
        yield return new WaitForSeconds(stunPushDuration); 

        if (agent != null)
        {
            agent.isStopped = false; 
        }

        indicator.SetActive(false);
    }
    #endregion
  
    #region Capture Trigger Behaviour
    public void TriggerCaptureTrap()
    {
        // Disable collider, movement and reset velocity to zero
        captureTrap.GetComponent</*CapsuleCollider*/Collider>().enabled = false;

        if (agent != null)
        {
            agent.isStopped = true;
        }

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }

        StartCoroutine(StunFromCapture());
    }

    IEnumerator StunFromCapture()
    {
        yield return new WaitForSeconds(timeTillCaptureIndicatorAppear);
        indicator.SetActive(true);

        // Capture for certain amount of seconds
        yield return new WaitForSeconds(captureDuration);

        if (agent != null)
        {
            agent.isStopped = false;
        }

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }

        indicator.SetActive(false);
        captureTrap.GetComponent<CapsuleCollider>().enabled = true;
        Destroy(captureTrap);
    }
    #endregion
}
// TODOS:
// Different indicator for each trap ??? 
