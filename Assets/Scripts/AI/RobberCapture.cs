using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobberCapture : MonoBehaviour
{
    [Header("Metrics")]
    [SerializeField]
    private int _capturedLoseReputationValue = 1;

    [Header("Debug")]
    [SerializeField]
    private bool _isCaptured = false;
    private RobberBehaviour _robberBehaviour;

    private void Start()
    {
        _robberBehaviour = GetComponent<RobberBehaviour>();
    }

    public void GetSifled(PlayerEnum playerID, float captureValue)
    {
        Debug.Log("Robber Siffled and Captured by " + playerID + " for: " + captureValue);
        
        Debug.LogWarning(playerID);
        GameManager.Instance.UIManager.UpdateCaptureThiefGauge((int)captureValue, playerID);

        _robberBehaviour.StartFleeState();
    }

    public void StartVulnerability() => _robberBehaviour.StartVulnerableState();
    public void StopVulnerability() => _robberBehaviour.StopVunerableState();
}
