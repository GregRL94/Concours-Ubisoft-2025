using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

// Automatically adds Player Abilities UI
[RequireComponent(typeof(PlayerAbilitiesUI))]
public class PlayerAbilities : MonoBehaviour
{
    [Serializable]
    public class Ability
    {
        public int initialCount;
        public TextMeshProUGUI countText;
        public float cooldownTime;
        public Image fillImage;
        [HideInInspector] public int count;
        [HideInInspector] public Coroutine cooldownRoutine;
    }

    // todo: Action Ability for player UI update 
    public event Action<Ability> OnAbilityUsed;
    public event Action<Ability> OnAbilityCooldown;
    public event Action<Ability> OnAbilityDepleted;

    [Header("Traps")]
    [SerializeField] private Ability alarmTrap = new Ability();
    [SerializeField] private Ability pushTrap = new Ability();
    [SerializeField] private Ability captureTrap = new Ability();

    [Header("Whistle")]
    [SerializeField] private float whistleCooldownTime;
    [SerializeField] private Image whistleFill;
    private Coroutine whistleCooldownRoutine;
    private bool canWhistle = true;

    [Header("No Ability Warning")]
    [SerializeField] private float warningTime;
    private Color initialColor = Color.white;

    // Référence à PlayerAbilitiesUI
    [SerializeField] private PlayerAbilitiesUI playerAbilitiesUI;

    void Start() { InitializeAbilities(); }

    private void InitializeAbilities()
    {
        InitializeAbility(alarmTrap);
        InitializeAbility(pushTrap);
        InitializeAbility(captureTrap);
    }

    private void InitializeAbility(Ability ability)
    {
        ability.count = ability.initialCount;
        playerAbilitiesUI.UpdateAbilityCountText(ability.countText, ability.count);
    }

    private void Update()
    {
        // Todo: Debug-> Get Player ID enum from Gregory
        if (gameObject.CompareTag("Player"))
        {
            CheckAbilityInput(KeyCode.Alpha1, alarmTrap);
            CheckAbilityInput(KeyCode.Alpha2, pushTrap);
            CheckAbilityInput(KeyCode.Alpha3, captureTrap);
            CheckWhistleInput(KeyCode.Alpha4);
        }
        if (gameObject.CompareTag("Player2"))
        {
            CheckAbilityInput(KeyCode.Q, alarmTrap);
            CheckAbilityInput(KeyCode.W, pushTrap);
            CheckAbilityInput(KeyCode.E, captureTrap);
            CheckWhistleInput(KeyCode.R);
        }


    }

    #region Player Abilities Action
    private void CheckAbilityInput(KeyCode key, Ability ability)
    {
        if (Input.GetKeyDown(key) && ability.cooldownRoutine == null)
        {
            if (ability.count > 0)
            {
                ability.count--;
                OnAbilityUsed?.Invoke(ability);
                playerAbilitiesUI.UpdateAbilityCountText(ability.countText, ability.count); // Mise à jour UI
                ability.cooldownRoutine = StartCoroutine(CooldownAbility(ability));
            }
            else
            {
                OnAbilityDepleted?.Invoke(ability);
                ShowWarning(ability.fillImage); // Affichage de l'avertissement via UIManager
            }
        }
    }

    private void CheckWhistleInput(KeyCode key)
    {
        if (Input.GetKeyDown(key) && whistleCooldownRoutine == null)
        {
            if (canWhistle)
            {
                canWhistle = false;
                whistleCooldownRoutine = StartCoroutine(CooldownWhistle());
            }
        }
    }
    #endregion

    #region Player Abilities Cooldown UI
    private IEnumerator CooldownAbility(Ability ability)
    {
        OnAbilityCooldown?.Invoke(ability);
        playerAbilitiesUI.UpdateCooldownFill(ability.fillImage, ability.cooldownTime); // Mise à jour du cooldown via UIManager
        yield return new WaitForSeconds(ability.cooldownTime);
        ability.cooldownRoutine = null;
    }

    private IEnumerator CooldownWhistle()
    {
        playerAbilitiesUI.UpdateCooldownFill(whistleFill, whistleCooldownTime); // Mise à jour du cooldown via UIManager
        yield return new WaitForSeconds(whistleCooldownTime);
        whistleCooldownRoutine = null;
        canWhistle = true;
    }

    private void ShowWarning(Image fillImage)
    {
        playerAbilitiesUI.ShowWarning(fillImage, warningTime, initialColor); // Avertissement via UIManager
    }
    #endregion




}
