using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobberCapture : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)]
    private float _captureGauge = 0f;
    [SerializeField]
    private bool _isCaptured = false;
    private RobberBehaviour _robberBehaviour;

    private void Start()
    {
        _robberBehaviour = GetComponent<RobberBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            GetSifled(PlayerEnum.PLAYER1, 100f);
    }

    public void GetSifled(PlayerEnum playerID, float captureValue)
    {
        //if (!_robberBehaviour.IsVulnerable) return;
        _captureGauge += captureValue;
        _robberBehaviour.StartFleeState();
        if (_captureGauge < 100f) return;
        if (playerID == PlayerEnum.NONE) return;
        _isCaptured = true;
        //to do : make lose value modifiable
        GameManager.Instance.AllOtherPlayersLoseReputation(playerID, 100);
        this.gameObject.SetActive(false);
    }

    public void StartVulnerability() => _robberBehaviour.StartVulnerableState();
    public void StopVulnerability() => _robberBehaviour.StopVunerableState();
}
