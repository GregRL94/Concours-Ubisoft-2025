using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class BearTrapAnimation : MonoBehaviour
{
    [SerializeField]
    private PlayableDirector _director;
    [SerializeField]
    private TimelineAsset _asset;
    public void ActivateTrap()
    {
        _director.playableAsset = _asset;
        _director.Play();
    }
}
