using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

#region Action For Player Ability UI (Test)
//public class PlayerAbilitiesUI : MonoBehaviour
//{
//    [SerializeField] private PlayerAbilities playerAbilities;

//    void OnEnable()
//    {
//        playerAbilities.OnAbilityUsed += HandleAbilityUsed;    
//        playerAbilities.OnAbilityCooldown += HandleAbilityCooldown;    
//        playerAbilities.OnAbilityDepleted += HandleAbilityDepleted;    
//    }

//    void OnDisable()
//    {   
//        playerAbilities.OnAbilityUsed -= HandleAbilityUsed;
//        playerAbilities.OnAbilityCooldown -= HandleAbilityCooldown;    
//        playerAbilities.OnAbilityDepleted -= HandleAbilityDepleted;    

//    }
//    void HandleAbilityUsed(PlayerAbilities.Ability ability)
//    {
//        Debug.Log($"Ability {ability.countText.name} used!");
//    }
//    void HandleAbilityCooldown(PlayerAbilities.Ability ability)
//    {
//        Debug.Log($"Ability {ability.countText.name} is on cooldown!");
//    }
//    void HandleAbilityDepleted(PlayerAbilities.Ability ability)
//    {
//        Debug.Log($"Ability {ability.countText.name} depleted!");
//    }

//}
#endregion

public class PlayerAbilitiesUI : MonoBehaviour
{
    [Serializable]
    public class AlarmTrapUI
    {
        public TextMeshProUGUI countText;
        public Image fillImage;
    }

    [Serializable]
    public class PushTrapUI
    {
        public TextMeshProUGUI countText;
        public Image fillImage;
    }

    [Serializable]
    public class CaptureTrapUI
    {
        public TextMeshProUGUI countText;
        public Image fillImage;
    }

    [Header("Traps Displays")]
    public AlarmTrapUI alarmTrapUI;
    public PushTrapUI pushTrapUI;
    public CaptureTrapUI captureTrapUI;
    [Space]

    [Header("Whistle Displays")]
    public Image whistleFillImage;
    [Space]

    [Header("Warning parameters")]
    [SerializeField] private float defaultWarningTime = 0.3f;
    public float DefaultWarningTime => defaultWarningTime;

    [SerializeField] private Color defaultWarningColor = Color.white;
    public Color DefaultWarningColor => defaultWarningColor;

    [Header("Selection Displays")]
    public GameObject selection;

    public float speed = 0.15f;
    public float scaleAmount = 0.05f;
    private Vector3 initialScale;

    [Header("Trap Selections/SetupTime UI")]
    public GameObject trapUI;
    public Image fillImage;
    public float setupTime;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip chargingSound;
    public AudioClip completeSound;
    public ParticleSystem completeEffect;

    private Coroutine trapCoroutine;
    void Start()
    {
        if (selection != null)
        {
            initialScale = selection.transform.localScale;
        }
    }
    void Update()
    {
        HighlightAnimation();
    }

    #region Trap Deployment Feedback UI

    public void DeployTrapUI() => trapCoroutine = StartCoroutine(HandleTrapDeployment());
    public void StopDeployTrapUI()
    {
        if (trapCoroutine != null)
        {
            StopCoroutine(trapCoroutine);
            trapCoroutine = null;

            ResetUI();
        }
    }

    private IEnumerator HandleTrapDeployment()
    {
        float elapsedTime = 0f;
        fillImage.fillAmount = 0f;

        trapUI.SetActive(true);

        if (audioSource && chargingSound)
            audioSource.PlayOneShot(chargingSound);

        // progress trap deployment
        while (elapsedTime < setupTime)
        {
            elapsedTime += Time.deltaTime;
            fillImage.fillAmount = Mathf.Clamp01(elapsedTime / setupTime);
            yield return null;
        }

        if (elapsedTime >= setupTime)
        {
            fillImage.fillAmount = 1f;

            if (audioSource && completeSound)
                audioSource.PlayOneShot(completeSound);

            if (completeEffect)
                completeEffect.Play();

            Debug.Log("Piège prêt !");
        }

        trapUI.SetActive(false);
    }

    private void ResetUI()
    {
        trapUI.SetActive(false);
        fillImage.fillAmount = 0f;
    }

    #endregion

    #region Selection UI / Update Animation
    public void Highlight(PlayerEnum currentSelection, Image fillImage)
    {
        if (currentSelection == PlayerEnum.PLAYER1)
        {
            SelectionUI(fillImage);
        }
        else if (currentSelection == PlayerEnum.PLAYER2)
        {
            SelectionUI(fillImage);
        }
        else if (currentSelection == PlayerEnum.NONE)
        {
            SelectionUI(fillImage);
        }
    }
    private void SelectionUI(Image fillImage)
    {
        // Selection
        Transform abilityUIPos = fillImage.transform.parent;
        selection.transform.position = abilityUIPos.position;
    }
    public void HighlightAnimation()
    {
        if (selection != null)
        {
            float scaleFactor = 1 + Mathf.PingPong(Time.time * speed, scaleAmount);
            selection.transform.localScale = initialScale * scaleFactor;
        }
    }
    #endregion

    #region Update Text UI 
    public void UpdateAbilityCountText(TextMeshProUGUI countText, int count)
    {
        countText.text = count.ToString();
    }
    #endregion

    #region Update Cooldowns
    public void UpdateCooldownFill(Image fillImage, float cooldownTime)
    {
        StartCoroutine(StartCooldown(fillImage, cooldownTime));
    }
    public IEnumerator StartCooldown(Image fillImage, float cooldownTime)
    {
        float elapsedTime = 0f;
        while (elapsedTime < cooldownTime)
        {
            fillImage.fillAmount = Mathf.Lerp(0, 1, elapsedTime / cooldownTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        fillImage.fillAmount = 1;
    }
    #endregion

    #region Warn Player if Cooldowns Empty
    public void ShowWarning(Image fillImage, float warningTime, Color initialColor)
    {
        StartCoroutine(ShowWarningCoroutine(fillImage, warningTime, initialColor));
    }

    private IEnumerator ShowWarningCoroutine(Image fillImage, float warningTime, Color initialColor)
    {
        float elapsedTime = 0f;
        while (elapsedTime < warningTime)
        {
            fillImage.color = new Color(fillImage.color.r, Mathf.Lerp(0, 1, elapsedTime / warningTime), Mathf.Lerp(0, 1, elapsedTime / warningTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        fillImage.color = initialColor;
    }
    #endregion
}
