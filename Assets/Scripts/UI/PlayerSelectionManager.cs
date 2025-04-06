using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using System.Linq;
using LevelManagement;

public class PlayerSelectionManager : MonoBehaviour
{
    public GameObject P1Join, P1PlayerCharacter, P1Ready;
    public GameObject P2Join, P2PlayerCharacter, P2Ready;

    public GameObject CountDownTimerScreen;
    public int beginCountDown = 3;
    public TextMeshProUGUI countDownText;
    private bool countDownState;
    private Coroutine CountDownTimerRoutine;

    private float backButtonPressTime;
    private float backButtonHoldDuration;
    private bool holdBackJoinToMenu;
    public Image fillBack;
    private Coroutine holdBackAnimationCoroutine;

    private Gamepad player1Controller;
    private Gamepad player2Controller;

    [SerializeField]
    private float _playDelay = 0.5f;

    [SerializeField]
    private TransitionFader startTransitionPrefab;
    
    void Start()
    {
        DetectControllers();


    }

    public void NextSceneTransition()
    {
        StartCoroutine(NextSceneTransitionRoutine());
    }

    private IEnumerator NextSceneTransitionRoutine()
    {
        if (startTransitionPrefab)
            TransitionFader.PlayTransition(startTransitionPrefab);
        
        yield return new WaitForSeconds(_playDelay);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    void Update()
    {

        if (player1Controller != null)
        {
            HandlePlayerInput(
                player1Controller.buttonSouth, player1Controller.buttonEast,
                ref P1Join, ref P1PlayerCharacter, ref P1Ready, "P1 (PS5)"
            );
            PlayerInfo.Instance.player1Gamepad = player1Controller;

        }

        if (player2Controller != null)
        {
            HandlePlayerInput(
                player2Controller.buttonSouth, player2Controller.buttonEast,
                ref P2Join, ref P2PlayerCharacter, ref P2Ready, "P2 (PS5)"
            );
            PlayerInfo.Instance.player2Gamepad = player2Controller;
        }

        if (P1Ready.activeInHierarchy && P2Ready.activeInHierarchy && !countDownState)
        {
            StartCountDown();
        }
    }
    private void DetectControllers()
    {
        var controllers = Gamepad.all
            .Where(gamepad => gamepad.enabled) // Only keep enables active controller  
            .ToList();

        Debug.Log($"Nb of controllers detected : {controllers.Count}");

        foreach (var gamepad in controllers)
        {
            Debug.Log($"Controller detected : {gamepad.displayName} (ID: {gamepad.deviceId})");
        }

        if (controllers.Count > 0)
        {
            player1Controller = controllers[0];
            Debug.Log($"P1 : {player1Controller.displayName} (ID: {player1Controller.deviceId})");
        }

        if (controllers.Count > 1)
        {
            player2Controller = controllers[1];
            Debug.Log($"P2 : {player2Controller.displayName} (ID: {player2Controller.deviceId})");
        }
    }


    private void HandlePlayerInput(ButtonControl confirm, ButtonControl back, ref GameObject join, ref GameObject character, ref GameObject ready, string playerName)
    {
        if (confirm.wasPressedThisFrame)
        {
            if (join.activeInHierarchy)
            {
                Debug.Log($"{playerName} pressed CONFIRM !");
                join.SetActive(false);
                character.SetActive(true);
            }
            else if (character.activeInHierarchy)
            {
                Debug.Log($"{playerName} pressed CONFIRM !");
                character.SetActive(false);
                ready.SetActive(true);
            }
        }

        if (back.wasPressedThisFrame)
        {
            backButtonPressTime = Time.time;
            backButtonHoldDuration = 0;
            Debug.Log($"Back {playerName}");

            if (CountDownTimerScreen.activeInHierarchy)
            {
                CancelCountDown();
                ResetSelection();
            }
            else if (ready.activeInHierarchy)
            {
                ready.SetActive(false);
                character.SetActive(true);
            }
            else if (character.activeInHierarchy)
            {
                character.SetActive(false);
                join.SetActive(true);
            }
            else if (join.activeInHierarchy)
            {
                holdBackJoinToMenu = true;
            }
        }

        if (holdBackJoinToMenu)
        {
            if (back.isPressed)
            {
                backButtonHoldDuration = Time.time - backButtonPressTime;
                Debug.Log($"Back Btn maintained for {backButtonHoldDuration} seconds");

                if (holdBackAnimationCoroutine == null)
                {
                    holdBackAnimationCoroutine = StartCoroutine(HoldBackAnimation());
                }
            }
            else if (back.wasReleasedThisFrame)
            {
                Debug.Log($"Back Btn released after {backButtonHoldDuration} seconds");
                holdBackJoinToMenu = false;

                if (holdBackAnimationCoroutine != null)
                {
                    StopCoroutine(holdBackAnimationCoroutine);
                    holdBackAnimationCoroutine = null;
                }
                fillBack.fillAmount = 0;
            }
        }
    }

    private void StartCountDown()
    {
        countDownState = true;
        CountDownTimerScreen.SetActive(true);
        if (holdBackAnimationCoroutine == null)
        {
            CountDownTimerRoutine = StartCoroutine(CountDownScreen());
        }
    }

    private void CancelCountDown()
    {
        countDownState = false;
        if (CountDownTimerRoutine != null)
        {
            StopCoroutine(CountDownTimerRoutine);
            CountDownTimerRoutine = null;
        }
        CountDownTimerScreen.SetActive(false);
    }

    private void ResetSelection()
    {
        P1Ready.SetActive(false);
        P2Ready.SetActive(false);
        P1PlayerCharacter.SetActive(true);
        P2PlayerCharacter.SetActive(true);
    }

    private IEnumerator CountDownScreen()
    {
        for (int i = beginCountDown; i >= 0; i--)
        {
            if (i == 0)
            {
                NextSceneTransition();
                yield break;
            }
            countDownText.text = i.ToString();
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator HoldBackAnimation()
    {
        while (true)
        {
            fillBack.fillAmount = Mathf.Clamp01(backButtonHoldDuration / 1f);

            if (fillBack.fillAmount >= 1f)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
                yield break;
            }

            yield return null;
        }
    }
}
