using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CaptureThiefManager : MonoBehaviour
{

    [Header("Managers")]
    [SerializeField]
    private UIManager _uiManager;
    public UIManager UIManager => _uiManager;

    [Header("Capture Thief")]
    //public struct CaptureThief
    //{

    //}
    [SerializeField]
    private int maxCaptureThiefAmount = 30;
    [SerializeField]
    int currentCaptureThiefAmount;
    public int CurrentCaptureThiefAmount => currentCaptureThiefAmount;
    public int MaxCaptureThiefAmount => maxCaptureThiefAmount;
    private Coroutine finishCaptureThiefRoutine;

    private void Start()
    {
        // todo: Find object ou drag and drop better ?
    }

    public void InitiateCaptureThiefText(TextMeshProUGUI captureThiefText)
    {
        captureThiefText.text = currentCaptureThiefAmount + "/" + maxCaptureThiefAmount;
    }

    public void UpdateCaptureThiefGauge(int amount, PlayerEnum playerID)
    {
        print("####### UpdateCaptureThiefGauge ####### ");
        float previousAmount = currentCaptureThiefAmount;
        currentCaptureThiefAmount = Mathf.Clamp(currentCaptureThiefAmount + amount, 0, maxCaptureThiefAmount);

        //todo: put a (coroutine) so ui capture fills up before show board 
        //if(finishUpdateCaptureThiefUIRoutine == null)
        //{
        finishCaptureThiefRoutine = StartCoroutine(UIManager.UpdateCaptureThiefUI(previousAmount, currentCaptureThiefAmount));
        //}

        if (currentCaptureThiefAmount >= maxCaptureThiefAmount)
        {
            print("Game Finish \n Show the Score Board P1 && P2");
            print("The player who has capture the thief at 100% is Player ID : " + playerID);
            GameManager.Instance.LosePlayerReputationByCapturingThief(playerID, 1);
        }
        print("####### UpdateCaptureThiefGauge End ####### ");
    }

}

//public class CaptureThiefManager : MonoBehaviour
//{
//    [Header("Managers")]
//    [SerializeField] private UIManager _uiManager;

//    [Header("Capture Thief")]
//    [SerializeField] private int maxCaptureThiefAmount = 30;
//    [SerializeField] private int currentCaptureThiefAmount;
//    private Coroutine finishCaptureThiefRoutine;

//    public int CurrentCaptureThiefAmount => currentCaptureThiefAmount;
//    public int MaxCaptureThiefAmount => maxCaptureThiefAmount;

//    public event Action<int, int> OnCaptureThiefUpdated;

//    private void Start()
//    {
//        if (_uiManager == null)
//        {
//            print("Drag and drop UIManager pour meilleur performance au lieu d'aller le chercher avec FindObjectOfType");
//            _uiManager = FindObjectOfType<UIManager>();
//        }
//    }

//    public void UpdateCaptureThiefGauge(int amount, PlayerEnum playerID)
//    {
//        print("####### UpdateCaptureThiefGauge ####### ");

//        float previousAmount = currentCaptureThiefAmount;
//        currentCaptureThiefAmount = Mathf.Clamp(currentCaptureThiefAmount + amount, 0, maxCaptureThiefAmount);

//        // Déclenchement de l'événement au lieu d'appeler directement l'UI
//        OnCaptureThiefUpdated?.Invoke(previousAmount, currentCaptureThiefAmount);

//        if (currentCaptureThiefAmount >= maxCaptureThiefAmount)
//        {
//            print($"Game Finish \n Show the Score Board. Winner: Player {playerID}");
//            GameManager.Instance.LosePlayerReputationByCapturingThief(playerID, 1);
//        }
//        print("####### UpdateCaptureThiefGauge End ####### ");
//    }
//}
