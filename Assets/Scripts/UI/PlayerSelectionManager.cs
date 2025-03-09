//using System;
//using System.Collections;
//using UnityEngine;
//using UnityEngine.InputSystem;
//using TMPro;
//using UnityEngine.SceneManagement;
//using UnityEngine.InputSystem.Controls;
//using UnityEngine.UI;

//public class PlayerSelectionManager : MonoBehaviour
//{
//    public GameObject P1Join, P1PlayerCharacter, P1Ready;
//    public GameObject P2Join, P2PlayerCharacter, P2Ready;

//    public GameObject CountDownTimerScreen;
//    public int beginCountDown = 3;
//    public TextMeshProUGUI countDownText;
//    private bool countDownState;
//    private Coroutine CountDownTimerRoutine;

//    private float backButtonPressTime;
//    private float backButtonHoldDuration;
//    private bool holdBackJoinToMenu;
//    public Image fillBack;
//    private Coroutine holdBackAnimationCoroutine;

//    void Update()
//    {
//        // todo: Remplacer mes inputs par les inputs actions de Gregory 
//        //HandlePlayerInput(
//        //    Keyboard.current.spaceKey, Keyboard.current.backspaceKey,
//        //    ref P1Join, ref P1PlayerCharacter, ref P1Ready, "Joueur 1 (Clavier)"
//        //);

//        if (Gamepad.all.Count > 0)
//        {
//            Gamepad controller1 = Gamepad.all[0];
//            HandlePlayerInput(
//                controller1.buttonSouth, controller1.buttonEast,
//                ref P1Join, ref P1PlayerCharacter, ref P1Ready, "Joueur 1 (controller1 PS5)"
//            );

//            Gamepad controller2 = Gamepad.all[1];
//            HandlePlayerInput(
//                controller2.buttonSouth, controller2.buttonEast,
//                ref P2Join, ref P2PlayerCharacter, ref P2Ready, "Joueur 2 (controller2 PS5)"
//            );
//        }

//        if (P1Ready.activeInHierarchy && P2Ready.activeInHierarchy && !countDownState)
//        {
//            StartCountDown();
//        }
//    }

//    private void HandlePlayerInput(ButtonControl confirm, ButtonControl back, ref GameObject join, ref GameObject character, ref GameObject ready, string playerName)
//    {
//        // Forth
//        if (confirm.wasPressedThisFrame)
//        {
//            if (join.activeInHierarchy)
//            {
//                Debug.Log($"{playerName} a appuyé sur CONFIRM !");
//                join.SetActive(false);
//                character.SetActive(true);
//            }
//            else if (character.activeInHierarchy)
//            {
//                Debug.Log($"{playerName} a appuyé sur CONFIRM !");
//                character.SetActive(false);
//                ready.SetActive(true);
//            }
//        }

//        // Back
//        if (back.wasPressedThisFrame)
//        {
//            // Reset to go back to main menu
//            backButtonPressTime = Time.time;
//            backButtonHoldDuration = 0;
//            Debug.Log($"Back {playerName}");

//            if (CountDownTimerScreen.activeInHierarchy)
//            {
//                CancelCountDown();
//                ResetSelection();
//            }
//            else if (ready.activeInHierarchy)
//            {
//                ready.SetActive(false);
//                character.SetActive(true);
//            }
//            else if (character.activeInHierarchy)
//            {
//                character.SetActive(false);
//                join.SetActive(true);
//            }
//            else if (join.activeInHierarchy)
//            {
//                holdBackJoinToMenu = true;
//            }
//        }

//        // Détection du maintien du bouton Back (Join To Return to Menu)
//        if (holdBackJoinToMenu)
//        {
//            if (back.isPressed)
//            {
//                backButtonHoldDuration = Time.time - backButtonPressTime;
//                Debug.Log($"Bouton Back maintenu pendant {backButtonHoldDuration} secondes");

//                // Hold Duration Animation
//                if (holdBackAnimationCoroutine == null)
//                {
//                    holdBackAnimationCoroutine = StartCoroutine(HoldBackAnimation());
//                }
//            }
//            else if (back.wasReleasedThisFrame)
//            {
//                Debug.Log($"Bouton Back relâché après {backButtonHoldDuration} secondes");
//                holdBackJoinToMenu = false;

//                // Stop Coroutine when button released 
//                if (holdBackAnimationCoroutine != null)
//                {
//                    StopCoroutine(holdBackAnimationCoroutine);
//                    holdBackAnimationCoroutine = null;
//                }
//                fillBack.fillAmount = 0;
//            }
//        }

//    }

//    private void StartCountDown()
//    {
//        countDownState = true;
//        CountDownTimerScreen.SetActive(true);
//        if (holdBackAnimationCoroutine == null)
//        {
//            CountDownTimerRoutine = StartCoroutine(CountDownScreen());
//        }
//    }

//    private void CancelCountDown()
//    {
//        countDownState = false;
//        if (CountDownTimerRoutine != null)
//        {
//            StopCoroutine(CountDownTimerRoutine);
//            CountDownTimerRoutine = null;
//        }
//        CountDownTimerScreen.SetActive(false);
//    }

//    private void ResetSelection()
//    {
//        P1Ready.SetActive(false);
//        P2Ready.SetActive(false);
//        P1PlayerCharacter.SetActive(true);
//        P2PlayerCharacter.SetActive(true);
//    }

//    private IEnumerator CountDownScreen()
//    {
//        for (int i = beginCountDown; i >= 0; i--)
//        {
//            if (i == 0)
//            {
//                Debug.Log("Reload Scene !");
//                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
//                yield break;
//            }
//            countDownText.text = i.ToString();
//            yield return new WaitForSeconds(1);
//        }
//    }

//    private IEnumerator HoldBackAnimation()
//    {
//        while (true)
//        {
//            fillBack.fillAmount = Mathf.Clamp01(backButtonHoldDuration / 1f);

//            if (fillBack.fillAmount >= 1f)
//            {
//                // Back to main menu
//                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
//                yield break;
//            }

//            yield return null;
//        }
//    }
//}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using System.Linq;

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

    void Start()
    {
        DetectControllers();
    }

    void Update()
    {

        if (player1Controller != null)
        {
            HandlePlayerInput(
                player1Controller.buttonSouth, player1Controller.buttonEast,
                ref P1Join, ref P1PlayerCharacter, ref P1Ready, "Joueur 1 (PS5)"
            );
        }

        if (player2Controller != null)
        {
            HandlePlayerInput(
                player2Controller.buttonSouth, player2Controller.buttonEast,
                ref P2Join, ref P2PlayerCharacter, ref P2Ready, "Joueur 2 (PS5)"
            );
        }

        if (P1Ready.activeInHierarchy && P2Ready.activeInHierarchy && !countDownState)
        {
            StartCountDown();
        }
    }
    private void DetectControllers()
    {
        var controllers = Gamepad.all
            .Where(gamepad => gamepad.enabled) // Garde seulement les manettes actives
            .ToList();

        Debug.Log($"Nombre de manettes ACTIVES détectées : {controllers.Count}");

        foreach (var gamepad in controllers)
        {
            Debug.Log($"Manette détectée : {gamepad.displayName} (ID: {gamepad.deviceId})");
        }

        if (controllers.Count > 0)
        {
            player1Controller = controllers[0];
            Debug.Log($"Joueur 1 assigné à : {player1Controller.displayName} (ID: {player1Controller.deviceId})");
        }

        if (controllers.Count > 1)
        {
            player2Controller = controllers[1];
            Debug.Log($"Joueur 2 assigné à : {player2Controller.displayName} (ID: {player2Controller.deviceId})");
        }
    }


    private void HandlePlayerInput(ButtonControl confirm, ButtonControl back, ref GameObject join, ref GameObject character, ref GameObject ready, string playerName)
    {
        if (confirm.wasPressedThisFrame)
        {
            if (join.activeInHierarchy)
            {
                Debug.Log($"{playerName} a appuyé sur CONFIRM !");
                join.SetActive(false);
                character.SetActive(true);
            }
            else if (character.activeInHierarchy)
            {
                Debug.Log($"{playerName} a appuyé sur CONFIRM !");
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
                Debug.Log($"Bouton Back maintenu pendant {backButtonHoldDuration} secondes");

                if (holdBackAnimationCoroutine == null)
                {
                    holdBackAnimationCoroutine = StartCoroutine(HoldBackAnimation());
                }
            }
            else if (back.wasReleasedThisFrame)
            {
                Debug.Log($"Bouton Back relâché après {backButtonHoldDuration} secondes");
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
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
