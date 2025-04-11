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
    [SerializeField, Range(0, 3)] float timeTillAlarmIndicatorAppear = 0.5f;
    const float timeTillFlee = 1f;
    [SerializeField,Range(0,100)] int _alarmTrapValue = 0;

    [Header("Push Trap Effects")]
    [SerializeField, Range(0, 20)] float pushDistance = 15f;
    float timeTillPushIndicatorAppear = 0.05f;
    [SerializeField, Range(0,2)] private float timeTillEnemyStop = 0.4f;
    [SerializeField, Range(0, 10)] float stunPushDuration = 4;
    [SerializeField, Range(0, 100)] int _pushTrapValue = 0;

    [Header("Capture Trap Effects")]
    [SerializeField, Range(0, 3)] float timeTillCaptureIndicatorAppear = 0f;
    [SerializeField, Range(0, 10)] float captureDuration = 5f;
    [SerializeField, Range(0, 100)] int _captureTrapValue = 0;

    [SerializeField]
    public RobberCapture robberCapture;

    public void SetRobber(NavMeshAgent agent, Rigidbody rb, GameObject indicator, RobberCapture robber, Animator animator)
    {
        _agent = agent;
        _rb = rb;
        _indicator = indicator;
        robberCapture = robber;
        _animator = animator;
    }

    #region Alarm Trigger Behaviour
    public void TriggerAlarmTrap(Transform tr, PlayerEnum trapOwner = PlayerEnum.NONE)
    {
        Debug.Log($"[TriggerAlarmTrap] Alarm triggered by {trapOwner}");
        robberCapture?.GetSifled(trapOwner, _alarmTrapValue);

        if(tr.GetComponent<Collider>() != null)
        {
            tr.GetComponent<Collider>().enabled = false;
            tr.GetComponent<Collider>().providesContacts = true;
        }

        if (_agent != null)
        {
            Debug.Log("[TriggerAlarmTrap] Stopping agent and applying stun.");
            _agent.isStopped = true;
            _agent.updatePosition = false;
            _agent.updateRotation = false;
            _agent.velocity = Vector3.zero;
        }

        _animator.SetTrigger("Surpris");
        _animator.SetBool("Cours", false);

        StartCoroutine(RunAwayDelay(tr, trapOwner));
    }

    private IEnumerator RunAwayDelay(Transform tr, PlayerEnum trapOwner)
    {
        Debug.Log($"[RunAwayDelay] Stun duration: {timeTillFlee} seconds");
        float elapsedTime = 0f;

        while (elapsedTime < timeTillFlee)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log("[RunAwayDelay] Stun time elapsed, preparing to flee.");

        if (_agent != null)
        {
            _agent.Warp(_rb.position);
            _agent.updatePosition = true;
            _agent.updateRotation = true;
            _agent.isStopped = false;
            Debug.Log("[RunAwayDelay] Agent re-enabled and ready to flee.");
        }

        _animator.SetBool("Cours", true);
        Debug.Log("[RunAwayDelay] Animation updated: running.");

        
        _indicator.GetComponent<ParticleSystem>()?.Play();
        //if (_indicator != null)
        //{
        //    _indicator.SetActive(true);
        //    Debug.Log("[RunAwayDelay] Indicator activated.");
        //}
        //else
        //{
        //    Debug.LogWarning("[RunAwayDelay] Warning: Indicator is null!");
        //}
    }
    #endregion

    #region Push Trigger Behaviour
    private Coroutine activePushCoroutine = null; // Un seul ennemi, donc une seule coroutine active
    public void TriggerPushTrap(Transform tr, PlayerEnum trapOwner = PlayerEnum.NONE)
    {
        Debug.Log($"[PushTrap] L'ennemi a activé un piège !");

        robberCapture?.GetSifled(trapOwner, _pushTrapValue);
        _agent?.GetComponentInChildren<Animator>()?.SetBool("EstRepousser", true);

        if (tr.GetComponent<Collider>() != null)
        {
            tr.GetComponent<Collider>().enabled = false;
            tr.GetComponent<Collider>().providesContacts = true;
        }

        if (_agent != null)
        {
            _agent.isStopped = true;
            _agent.updatePosition = false;
        }

        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;

            // Direction normalized
            Vector3 pushDirection = tr.forward.normalized * (pushDistance / timeTillEnemyStop);
            _rb.velocity = pushDirection;

            _agent.transform.rotation = Quaternion.LookRotation(-tr.forward);
            _agent.updateRotation = false;
            Debug.Log($"[PushTrap] L'ennemi est repoussé avec une vitesse de {pushDirection}");
        }

        // if stuned, reset the timer for enemy to stop
        if (activePushCoroutine != null)
        {
            Debug.Log($"[PushTrap] Ancien stun annulé, un nouveau stun démarre !");
            StopCoroutine(activePushCoroutine);
        }

        // Démarrer une nouvelle coroutine et l'enregistrer
        activePushCoroutine = StartCoroutine(StunAfterPush(trapOwner, tr));
    }

    private IEnumerator StunAfterPush(PlayerEnum trapOwner, Transform tr)
    {
        Debug.Log($"[Stun] Début du stun, apparition de l'indicateur dans {timeTillPushIndicatorAppear} sec");

        yield return new WaitForSeconds(timeTillPushIndicatorAppear);
        _indicator.GetComponent<ParticleSystem>()?.Play();
        //_indicator?.SetActive(true);

        Debug.Log($"[Stun] Indicateur activé, l'ennemi sera bloqué progressivement sur {timeTillEnemyStop} sec");

        float elapsedTime = 0f;
        Vector3 initialVelocity = _rb.velocity;

        while (elapsedTime < timeTillEnemyStop)
        {
            elapsedTime += Time.deltaTime;
            _rb.velocity = Vector3.Lerp(initialVelocity, Vector3.zero, elapsedTime / timeTillEnemyStop);
            yield return null;
        }

        _rb.velocity = Vector3.zero;
        Debug.Log($"[Stun] L'ennemi est immobilisé");

        yield return new WaitForSeconds(stunPushDuration);

        if (_agent != null)
        {
            _agent.Warp(_rb.position);
            _agent.isStopped = false;
            _agent.updatePosition = true;
            _agent.updateRotation = true;
            _agent.GetComponentInChildren<Animator>()?.SetBool("EstRepousser", false);
        }

        _indicator.GetComponent<ParticleSystem>()?.Stop();
        //_indicator?.SetActive(false);
        Debug.Log($"[Stun] Fin du stun, l'ennemi peut à nouveau bouger.");

        // coroutine null if push trap behaviour done
        activePushCoroutine = null;
    }
    #endregion

    #region Capture Trigger Behaviour
    public void TriggerCaptureTrap(PlayerEnum trapOwner, Transform tr)
    {
        print("trapOwner " + trapOwner);
        robberCapture?.GetSifled(trapOwner, _captureTrapValue);

        if (tr.GetComponent<Collider>() != null)
        {
            tr.GetComponent<Collider>().enabled = false;
            tr.GetComponent<Collider>().providesContacts = true;
        }

        if (_rb != null)
        {
            _rb.angularVelocity = Vector3.zero;
            _rb.velocity = Vector3.zero;
            print("Angular ");
            print("_rb.velocity " + _rb.velocity);
        }
        if (_agent != null)
        {
            print("_agent.isStopped " + _agent.isStopped);
            print("capture trap pos: " + tr.position);
            _agent.SetDestination(tr.position);
            _agent.isStopped = true;
            _agent.GetComponentInChildren<ParticleSystem>()?.Play();
        }

        StartCoroutine(StunFromCapture(trapOwner, tr));
    }

    IEnumerator StunFromCapture(PlayerEnum trapOwner, Transform tr)
    {
        
        yield return new WaitForSeconds(timeTillCaptureIndicatorAppear);
        if(_indicator == null) yield break;
        _indicator.GetComponent<ParticleSystem>()?.Play();
        //_indicator?.SetActive(true);

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

        _indicator.GetComponent<ParticleSystem>()?.Stop();
        //_indicator?.SetActive(false);
        robberCapture?.StopVulnerability();
    }
    #endregion
}

