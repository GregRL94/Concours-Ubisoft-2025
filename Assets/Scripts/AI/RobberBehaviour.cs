using AkuroTools;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class RobberBehaviour : BTAgent
{
    [Header("Metrics")]
    [SerializeField, Tooltip("Robber base speed")]
    private float _vBase = 10f;
    [SerializeField, Tooltip("Robber flee speed")]
    private float _vFlee = 10f;
    [SerializeField, Tooltip("Robber circular vision")]
    private float _radialVision = 5f;
    [SerializeField, Tooltip("Robber flee vision")]
    private float _fleeVision = 10f;
    private float _currentVision = 0f;
    [SerializeField, Tooltip("Robber stealing time")]
    private float _stealTime = 10f;
    [SerializeField, Tooltip("Robber stealing range")]
    private float _stealRange = 2f;
    
    [SerializeField]
    private GameObject _indicator;
    [SerializeField]
    private Rigidbody _rb;
    [SerializeField]
    private NavMeshAgent _robberAgent;
    [SerializeField]
    private RobberCapture _robberCapture;
    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private ParticleSystem _particleSystem;
    

    [Header("DEBUG READING")]
    [SerializeField, Tooltip("Robber stealing list")]
    private List<ObjectType> _stealingList;
    [SerializeField]
    private MuseumObjects _currentTargetObject;
    private BTNode.Status _hasStolen = BTNode.Status.RUNNING;
    [SerializeField]
    private bool _isVulnerable = false;
    [SerializeField]
    private bool _isFleeing = false;
    [SerializeField]
    private float _robberTimeFleeing = 5f;
    
    //Coroutines
    private Coroutine _stealingObjectTimer;
    private Coroutine _fleeingTimer;
    private Coroutine _detectPlayers;
    public bool IsVulnerable => _isVulnerable;
    public List<ObjectType> StealingList => _stealingList;

    // Start is called before the first frame update
    public override void Start()
    {
        StopAllCoroutines();
        base.Start();
        _currentVision = _radialVision;
        if(!_robberAgent)_robberAgent = GetComponent<NavMeshAgent>();
        if(!_rb)_rb = GetComponent<Rigidbody>();
        if(!_robberCapture) _robberCapture = GetComponent<RobberCapture>();
        if (!_animator) _animator = GetComponentInChildren<Animator>();

        _robberAgent.speed = _vBase;
        GameManager.Instance.TrapManager.SetRobber(_robberAgent, _rb, _indicator, _robberCapture, _animator);

        //Flee state
        BTLeaf isFleeing = new BTLeaf("Is fleeing", IsFleeing);
        BTInverter isntFleeing = new BTInverter("Isn't fleeing");
        BehaviourTree BTIsntFleeing = new BehaviourTree();
        BehaviourTree BTIsFleeing = new BehaviourTree();
        BTDependantSequence fleeState = new BTDependantSequence("Nigerundayo !", BTIsFleeing, agent);
        BTLeaf fleeFromPlayers = new BTLeaf("Flee from players", FleeFromPlayers);
        isntFleeing.AddChild(isFleeing);
        BTIsntFleeing.AddChild(isntFleeing);
        BTIsFleeing.AddChild(isFleeing);
        fleeState.AddChild(fleeFromPlayers);

        //Neutral state
        BTDependantSequence neutralState = new BTDependantSequence("Steal Objects", BTIsntFleeing, agent);
        BTLeaf goToObjectListed = new BTLeaf("Go To Object Listed", GoToObjectListed);
        BTLeaf stealObjectListed = new BTLeaf("StealObject", StealObject);
        BTLeaf hasStealedEverything = new BTLeaf("Has Stealed Everything", HasStealedEverything);
        
        neutralState.AddChild(hasStealedEverything);
        neutralState.AddChild(goToObjectListed);
        neutralState.AddChild(stealObjectListed);

        //Robber global behaviour
        BTSelector robberBehaviour = new BTSelector("Robber Behaviour");
        robberBehaviour.AddChild(neutralState);
        robberBehaviour.AddChild(fleeState);
        tree.AddChild(robberBehaviour);

        //Start detecting
        _detectPlayers = StartCoroutine(DetectPlayersLoop());
    }

    #region Steal
    public BTNode.Status StealObject()
    {
        if (_currentTargetObject == null) return BTNode.Status.SUCCESS;
        if (_stealingObjectTimer == null) _stealingObjectTimer = StartCoroutine(StealTimer(_stealTime));
        return _hasStolen;
    }
    public BTNode.Status HasStealedEverything()
    {
        if (_stealingList.Count <= 0)
        {
            Debug.Log("huh");
            return BTNode.Status.FAILURE;
        }
        else return BTNode.Status.SUCCESS;
    }
    public BTNode.Status GoToObjectListed()
    {
        if (_currentTargetObject == null) GetNearestObject(false);
        //stop the node if no object to steal
        if (_currentTargetObject == null)
        {
            Debug.LogWarning("NO CURRENT TARGET OBJECT");
            return BTNode.Status.FAILURE;
        }


        BTNode.Status s = GoToPosition(_currentTargetObject.transform.position);
        if (s == BTNode.Status.SUCCESS)
        {
            Debug.Log($"Start Stealing {_currentTargetObject.MuseumObjectType} !");
            _hasStolen = BTNode.Status.RUNNING;
        }
        return s;
    }

    //Get the nearest object possible
    //if all objects are in cd, bypass cd condition
    private void GetNearestObject(bool bypassObjectCDCondition)
    {
        MuseumObjects[] museumObjects = GameManager.Instance.MuseumObjectsManager.GetObjectList(_stealingList[0]);
        if (museumObjects == null) return;
        float minDistance = float.MaxValue;
        MuseumObjects nearestObject = null;
        int objectsInCd = 0;
        int objectsStolen = 0;
        for (int i = 0; i < museumObjects.Length; i++)
        {
            //skip already stealed objects
            MuseumObjects nearest = museumObjects[i];
            if (!nearest.gameObject.activeSelf)
                continue;
            
            //skip objects in cd and object already stolen
            if((!nearest.IsObjectStealable() && !bypassObjectCDCondition) || nearest.IsStolen)
            {
                objectsInCd++;
                if(nearest.IsStolen) objectsStolen++;
                continue;
            }


            float distance = Vector3.Distance(this.transform.position, museumObjects[i].transform.position);
            //skip not nearest objects
            if (distance >= minDistance)
                continue;

            minDistance = distance;
            nearestObject = nearest;
        }

        _currentTargetObject = nearestObject;
        if (objectsInCd >= museumObjects.Length)
        {
            _currentTargetObject = null;
            if(objectsStolen < museumObjects.Length)GetNearestObject(true);
        }

    }

    private IEnumerator StealTimer(float time)
    {
        state = ActionState.WORKING;
        StartVulnerableState();
        _currentTargetObject.SetObjectStealableCD();
        //Lance l'animation de vol
        _animator.SetBool("VolUnOeuvre", true);

        while (_hasStolen == BTNode.Status.RUNNING)
        {
            yield return new WaitForSeconds(time);
            //animation de vol fini
            _animator.SetBool("VolUnOeuvre", false);
            _hasStolen = BTNode.Status.SUCCESS;

            //steal object
            GameManager.Instance.MuseumObjectsManager.CheckArtefactStolen(_currentTargetObject);
            
            GameManager.Instance.LosePlayerReputation(_currentTargetObject.ObjectOwner, 1);
            _currentTargetObject.StealObject();
            AudioManager.instance.PlayClipAt(AudioManager.instance.allAudio["Object Stolen"], this.transform.position, AudioManager.instance.soundEffectMixer, true, false);
            print("End round for AI !");
            //GameManager.Instance.CheckEndRound();

            _currentTargetObject = null;
            _stealingList.RemoveAt(0);
            StopVunerableState();
            _stealingObjectTimer = null;
        }
    }
    #endregion

    #region Flee
    private IEnumerator DetectPlayersLoop()
    {
        while (true)
        {
            float playerDetected = DetectPlayers(transform.position, _currentVision);
            if (playerDetected > 0)
            {
                if (!_isFleeing) StartFleeState();
                else RestartFleeingTimer();
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private float DetectPlayers(Vector3 pos, float radius = 5f)
    {
        float radiusSqr = radius * radius;
        float playerDetected = 0;
        for (int i = 0; i < GameManager.Instance.Players.Length; i++)
        {
            if ((GameManager.Instance.Players[i].transform.position - pos).sqrMagnitude <= radiusSqr)
            if (Vector3.Distance(GameManager.Instance.Players[i].transform.position, this.transform.position) <= radius)
            playerDetected++;
        }
        return playerDetected;
    }

    public BTNode.Status FleeFromPlayers()
    {
        if (_fleeingTimer == null) _fleeingTimer = StartCoroutine(FleeTimer(_robberTimeFleeing));
        //relance le timer si il est en range de vision (nouvel leaf / function)
        if (Vector3.Distance(GetMostFarPosition(), this.transform.position) <= _stealRange)
        {
            _animator.SetBool("Cours", false);
            return BTNode.Status.SUCCESS;
        }
        BTNode.Status s = GoToPosition(GetMostFarPosition());
        return s;
    }
    private Vector3 GetMostFarPosition()
    {
        Vector3 mostFar = Vector3.zero;

        Node[,] gameGrid = GameGrid.Instance.Grid;

        float mostDistanceToPlayers = 0;

        for (int i = 0; i < gameGrid.GetLength(0); i++)
        {
            for (int j = 0; j < gameGrid.GetLength(1); j++)
            {
                float distanceToPlayer1 = Vector3.Distance(gameGrid[i, j].worldPos, GameManager.Instance.Players[0].transform.position);
                float distanceToPlayer2 = Vector3.Distance(gameGrid[i, j].worldPos, GameManager.Instance.Players[1].transform.position);
                float distanceToPlayers = distanceToPlayer1 + distanceToPlayer2;
                if (distanceToPlayers <= mostDistanceToPlayers) continue;
                mostDistanceToPlayers = distanceToPlayers;
                mostFar = gameGrid[i, j].worldPos;
            }
        }
        
        return mostFar;
    }

    private void RestartFleeingTimer()
    {
        if (_fleeingTimer != null) StopAndClearCoroutine(ref _fleeingTimer);
        _fleeingTimer = StartCoroutine(FleeTimer(_robberTimeFleeing));
    }

    private IEnumerator FleeTimer(float timer)
    {
        yield return new WaitForSeconds(timer);
        StopFleeingState();
    }

    public BTNode.Status IsFleeing()
    {
        if (_isFleeing) return BTNode.Status.SUCCESS;
        else return BTNode.Status.FAILURE;
    }
    #endregion

    #region States
    public void StartVulnerableState()
    {
        _isVulnerable = true;
        _robberAgent.speed = 0;
        _currentVision = 0;
        _robberAgent.isStopped = true;
        _rb.angularVelocity = Vector3.zero;
        _rb.velocity = Vector3.zero;
        //lance l'animation vulnerable
        _animator.SetBool("EstPieger", true);        
    }

    public void StopVunerableState()
    {
        if (!_isVulnerable) return;
        _isVulnerable = false;
        _robberAgent.speed = _vBase;
        _currentVision = _radialVision;
        _robberAgent.isStopped = false;
        //animation vulnerable fini
        _animator.SetBool("EstPieger", false);
    }

    public void StartFleeState()
    {
        StopVunerableState();
        AudioManager.instance.PlayClipAt(AudioManager.instance.allAudio["Enemy Detected"], this.transform.position, AudioManager.instance.soundEffectMixer, true, false);
        if(!GameManager.Instance.EndGame)AudioManager.instance.SpeedUpMusic();
        _isFleeing = true;
        if(_currentTargetObject != null)_currentTargetObject.SetObjectStealableCD();
        if (_stealingObjectTimer != null) StopAndClearCoroutine(ref _stealingObjectTimer);
        _currentTargetObject = null;
        _robberAgent.speed = _vFlee;
        _currentVision = _fleeVision;
        //lance l'animation de fuite

    }

    private void StopFleeingState()
    {
        AudioManager.instance.OriginalMusicSpeed();
        _isFleeing = false;
        if (_fleeingTimer != null) StopAndClearCoroutine(ref _fleeingTimer);
        _robberAgent.speed = _vBase;
        _currentVision = _radialVision;
        //animation de fuite fini

    }
    #endregion


    public BTNode.Status GoToPosition(Vector3 destination)
    {
        //lance l'animation de marche
        //si l'animation de fuite est differente
        //met une condition dans l'animator qui differencie la marche de la fuite
        //(la fuite utilise aussi cette fonction)
        
        float distanceToTarget = Vector3.Distance(destination, this.transform.position);
        if (state == ActionState.IDLE)
        {
            _animator.SetBool("Cours", true);
            agent.SetDestination(destination);
            state = ActionState.WORKING;
        }
        else if (Vector3.Distance(agent.pathEndPosition, destination) >= _stealRange)
        {
            _rb.velocity = Vector3.zero;
            _animator.SetBool("Cours", false);
            state = ActionState.IDLE;
            return BTNode.Status.FAILURE;
        }
        else if (distanceToTarget < _stealRange)
        {
            //animation de marche fini
            _animator.SetBool("Cours", false);
            _rb.velocity = Vector3.zero;
            state = ActionState.IDLE;
            return BTNode.Status.SUCCESS;
        }
        return BTNode.Status.RUNNING;
    }

  
    private void StopAndClearCoroutine(ref Coroutine coroutine)
    {
        StopCoroutine(coroutine);
        coroutine = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, _currentVision);
    }
}
