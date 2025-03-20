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
        _robberBehaviour.StartFleeState();
        print("########### GetSifled ###########");
        Debug.Log("Robber Siffled and Captured by " + playerID + " for: " + captureValue);
        GameManager.Instance.UIManager.UpdateCaptureThiefGauge((int)captureValue);
        print("You just got siffled for stepping into a trap: +" + captureValue + " - " + GameManager.Instance.UIManager.GetCurrentCaptureThiefAmount +
              "/" + GameManager.Instance.UIManager.GetmaxCaptureThiefAmount);
        print( "Round still going " + (GameManager.Instance.UIManager.GetCurrentCaptureThiefAmount < GameManager.Instance.UIManager.GetmaxCaptureThiefAmount).ToString() );
        print("########### GetSifled End ###########");

        // IF CURRENT CAPTURE BAR THIEF IS > MAX
        if (GameManager.Instance.UIManager.GetCurrentCaptureThiefAmount < GameManager.Instance.UIManager.GetmaxCaptureThiefAmount) return;
        //if (playerID == PlayerEnum.NONE) return;
        //_isCaptured = true;
        //GameManager.Instance.AllOtherPlayersLoseReputation(playerID, _capturedLoseReputationValue);
        //this.gameObject.SetActive(false);

    }

    public void StartVulnerability() => _robberBehaviour.StartVulnerableState();
    public void StopVulnerability() => _robberBehaviour.StopVunerableState();
}
