using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuseumObjects : MonoBehaviour
{
    [Header("Metrics")]
    [SerializeField, Tooltip("Owner of object")]
    protected PlayerEnum _objectOwner;
    [SerializeField, Tooltip("Type of stealable object")]
    protected ObjectType _objectType;
    [SerializeField, Tooltip("cd before object can be stealed again")]
    protected float _objectStealableCD = 10f;
    [Header("DEBUG")]
    [SerializeField]
    protected float _timeStealableCD;
    [SerializeField]
    protected bool _isStolen;
    public ObjectType MuseumObjectType => _objectType;
    public PlayerEnum ObjectOwner => _objectOwner;
    public bool IsStolen => _isStolen;

    protected Coroutine _cdCoroutine;

    protected IEnumerator ReduceStealableCD()
    {
        yield return new WaitForSeconds(_timeStealableCD);
        _timeStealableCD = 0;
        StopCoroutine(_cdCoroutine);
    }
    public void SetObjectStealableCD()
    {
        if (_timeStealableCD > 0) return;
        _timeStealableCD = _objectStealableCD;
       _cdCoroutine = StartCoroutine(ReduceStealableCD());
    }

    public bool IsObjectStealable() => _timeStealableCD <= 0;
    public void StealObject() => _isStolen = true;

}
