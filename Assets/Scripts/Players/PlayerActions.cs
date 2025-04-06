using AkuroTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    #region Variables
    private GameManager _gameManager;
    private PlayerControls _playerControls;
    private PlayerAbilitiesUI _pAbilitiesUI;
    private AbilitiesEnum _currentAbility;
    private GameManager.WhistleData _whistle;
    private Dictionary<AbilitiesEnum, GameManager.TrapData> _trapsDict;
    private GameObject _currentTrap;
    private Coroutine _trapSetupCoroutine;
    private Animator _animator;
    #endregion

    #region MonoBehaviour Flow
    private void Start()
    {
        _gameManager = GameManager.Instance;
        _playerControls = GetComponent<PlayerControls>();
        _pAbilitiesUI = GetComponent<PlayerAbilitiesUI>();
        _animator = GetComponentInChildren<Animator>();

        GameManager.WhistleData wBase = _gameManager.WhistleBase;
        GameManager.TrapData aTBase = _gameManager.AlarmTrapBase;
        GameManager.TrapData pTBase = _gameManager.PushTrapBase;
        GameManager.TrapData cTBase = _gameManager.CaptureTrapBase;
        
        _whistle = new GameManager.WhistleData(wBase.whistleFleeRange, wBase.whistleCaptureRange, wBase.whistleCapturePower, wBase.whistleCooldown, _pAbilitiesUI.whistleFillImage);
        _trapsDict = new Dictionary<AbilitiesEnum, GameManager.TrapData>
        {
            [AbilitiesEnum.ALARM_TRAP] = new GameManager.TrapData(aTBase.trapPrefab, aTBase.setupTime, aTBase.initialCount, aTBase.cooldown, _pAbilitiesUI.alarmTrapUI.countText, _pAbilitiesUI.alarmTrapUI.fillImage),
            [AbilitiesEnum.PUSH_TRAP] = new GameManager.TrapData(pTBase.trapPrefab, pTBase.setupTime, pTBase.initialCount, pTBase.cooldown, _pAbilitiesUI.pushTrapUI.countText, _pAbilitiesUI.pushTrapUI.fillImage),
            [AbilitiesEnum.CAPTURE_TRAP] = new GameManager.TrapData(cTBase.trapPrefab, cTBase.setupTime, cTBase.initialCount, cTBase.cooldown, _pAbilitiesUI.captureTrapUI.countText, _pAbilitiesUI.captureTrapUI.fillImage)
        };

        InitializeAbilities();
    }

    private void InitializeAbilities()
    {
        foreach (GameManager.TrapData _data in _trapsDict.Values)
        {
            _data.currentCount = _data.initialCount;
            _pAbilitiesUI.UpdateAbilityCountText(_data.countText, _data.currentCount);
        }
    }
    #endregion

    #region Action Logic
    public void UpdateActionState(float deltaTime, Vector3 interactionPoint)
    {
        // As we can not use a foreach - which is a numerator - to modify in place the content of a collection, we use a plain for loop
        List<KeyValuePair<AbilitiesEnum, GameManager.TrapData>> _trapsDictAsList = _trapsDict.ToList();

        for (int i = 0; i < _trapsDictAsList.Count; i++)
        {
            if (_trapsDict[_trapsDictAsList[i].Key].timer > 0) { _trapsDict[_trapsDictAsList[i].Key].timer -= deltaTime; }
        }
        //

        if (_whistle.timer > 0) { _whistle.timer -= deltaTime; }

        if (_currentAbility == AbilitiesEnum.NONE || _currentAbility == AbilitiesEnum.WHISTLE) { return; }
        if (_trapsDict[_currentAbility].currentCount <= 0 ) { return; }
        Node node = _playerControls.GameGrid.NodeFromWorldPos(interactionPoint);
        if (node != null) { PreviewTrap(_currentAbility, interactionPoint, node); }
    }

    public void SelectAction(AbilitiesEnum selectedAbility)
    {
        if(_currentAbility == selectedAbility) { return; }
        if (_currentTrap != null)
        {
            Destroy(_currentTrap);
            _currentTrap = null;
        }
        _currentAbility = selectedAbility;
        Debug.Log(_currentAbility);
        switch(selectedAbility)
        {
            case AbilitiesEnum.CAPTURE_TRAP:
                _pAbilitiesUI.setupTime = GameManager.Instance.CaptureTrapBase.setupTime;
                _pAbilitiesUI.Highlight(_playerControls.PlayerID, _pAbilitiesUI.captureTrapUI.fillImage);
            break;
            case AbilitiesEnum.PUSH_TRAP:
                _pAbilitiesUI.setupTime = GameManager.Instance.PushTrapBase.setupTime;
                _pAbilitiesUI.Highlight(_playerControls.PlayerID, _pAbilitiesUI.pushTrapUI.fillImage);
                break;
            case AbilitiesEnum.ALARM_TRAP:
                _pAbilitiesUI.setupTime = GameManager.Instance.AlarmTrapBase.setupTime;
                _pAbilitiesUI.Highlight(_playerControls.PlayerID, _pAbilitiesUI.alarmTrapUI.fillImage);
            break;
            case AbilitiesEnum.WHISTLE:
                _pAbilitiesUI.Highlight(_playerControls.PlayerID, _pAbilitiesUI.whistleFillImage);
            break;
        }
        
    }

    public void DeselectAction()
    {
        if (_trapSetupCoroutine != null)
        {
            StopCoroutine(_trapSetupCoroutine);
            _trapSetupCoroutine = null;
        }
        if (_currentTrap != null)
        {
            Destroy(_currentTrap);
            _currentTrap = null;
        }
        _currentAbility = AbilitiesEnum.NONE;
        Debug.Log(_currentAbility);
    }

    public void PerformWhistle(LayerMask gameAgentsMask)
    {
        if (_currentAbility != AbilitiesEnum.WHISTLE || _whistle.timer > 0f) { return; }
        // Play sound
        // Play Animation
        _animator.SetTrigger("Siffle");
        if (!GameManager.Instance.TutorialManager.IsTutorialCompleted && GameManager.Instance.TutorialManager.CurrentTutorialType == UITutorialStep.WHISTLE_STEP) GameManager.Instance.UIManager.CurrentPlayerValidation.DynamicValidatePage(_playerControls.PlayerID);
        AudioManager.instance.PlayClipAt(AudioManager.instance.allAudio["Player Whistle"], this.transform.position, AudioManager.instance.soundEffectMixer, true, false);

        Collider[] agents = Physics.OverlapSphere(transform.position, _whistle.whistleFleeRange, gameAgentsMask);

        if (agents.Length > 0)
        {
            foreach (Collider collider in agents)
            {
                if (collider.gameObject.CompareTag("ENEMY"))
                {
                    RobberCapture robber = collider.gameObject.GetComponent<RobberCapture>();

                    if (Vector3.Distance(transform.position, collider.gameObject.transform.position) <= _whistle.whistleCaptureRange)
                    {
                        robber.GetSifled(_playerControls.PlayerID, _whistle.whistleCapturePower);
                    }
                    else
                    {
                        robber.GetSifled(_playerControls.PlayerID, 0f);
                    }
                }
            }
        }
        _whistle.timer += _whistle.whistleCooldown;
        _pAbilitiesUI.UpdateCooldownFill(_whistle.fillImage, _whistle.whistleCooldown);
    }
    #endregion

    #region Trap Logic
    public void PreviewTrap(AbilitiesEnum trap, Vector3 previewPosition, Node node)
    {
        if (_currentTrap != null)
        {
            _currentTrap.transform.position = previewPosition;
        }
        else
        {
            _currentTrap = Instantiate(_trapsDict[trap].trapPrefab, previewPosition, Quaternion.identity, null);
            _currentTrap.GetComponent<Collider>().enabled = false;
            if(_currentTrap.GetComponent<SphereColliderWireframe>() != null) _currentTrap.GetComponent<SphereColliderWireframe>().enabled = false;
        }        

        try
        {
            DynamicOutline outline = _currentTrap.GetComponent<DynamicOutline>();
            if (node.isFree && node.playerZone == _playerControls.PlayerID)
            {
                outline.SetOutlineValid();
            }
            else
            {
                outline.SetOutlineInvalid();
            }
        }
        catch (NullReferenceException)
        {
            Debug.Log("Could not retrieve DynamicOutline script");
        }
    }

    public void StartTrapDeployment(Node deployTrapAtNode)
    {        
        if (_currentAbility == AbilitiesEnum.NONE || _currentAbility == AbilitiesEnum.WHISTLE) { return; }
        if (!GameManager.Instance.TutorialManager.IsTutorialCompleted && GameManager.Instance.TutorialManager.CurrentTutorialType == UITutorialStep.PLACE_TRAP_STEP) GameManager.Instance.UIManager.CurrentPlayerValidation.DynamicValidatePage(_playerControls.PlayerID);

        AudioManager.instance.PlayClipAt(AudioManager.instance.allAudio["Trap Placing"], this.transform.position, AudioManager.instance.soundEffectMixer, true, false);
        _pAbilitiesUI.DeployTrapUI();
        _animator.SetBool("installePiege", true);
        _animator.SetTrigger("installation");
        GameManager.TrapData currentTrapData = _trapsDict[_currentAbility];
        if (currentTrapData.currentCount <= 0 || currentTrapData.timer > 0) 
        {
            _pAbilitiesUI.ShowWarning(currentTrapData.fillImage, _pAbilitiesUI.DefaultWarningTime, _pAbilitiesUI.DefaultWarningColor);
            return;
        }
        if (!deployTrapAtNode.isFree || deployTrapAtNode.playerZone != _playerControls.PlayerID) { return; }

        _trapSetupCoroutine = StartCoroutine(TrapSetupTimer(currentTrapData.setupTime, deployTrapAtNode));
        Debug.Log("Trap deployment started");
        return;
    }

    public void CancelTrapDeployment()
    {
        if (_trapSetupCoroutine == null) { return; }
        StopCoroutine(_trapSetupCoroutine);
        _pAbilitiesUI.StopDeployTrapUI();
        _trapSetupCoroutine = null;
        _animator.SetBool("installePiege", false);
        Debug.Log("Trap deployment canceled");
    }

    public void RotateTrap(float inputValue, GameObject trap)
    {
        int rotationDirection = 0;

        if (inputValue < 0)
        {
            rotationDirection = -1;
        }
        else if (inputValue > 0)
        {
            rotationDirection = 1;
        }
        if (trap != null && rotationDirection != 0)
        {
            if (!GameManager.Instance.TutorialManager.IsTutorialCompleted && GameManager.Instance.TutorialManager.CurrentTutorialType == UITutorialStep.ROTATE_STRAP_STEP) GameManager.Instance.UIManager.CurrentPlayerValidation.DynamicValidatePage(_playerControls.PlayerID);
            AudioManager.instance.PlayClipAt(AudioManager.instance.allAudio["Trap Turning"], this.transform.position, AudioManager.instance.soundEffectMixer, true, false);

            trap.transform.Rotate(new Vector3(0f, rotationDirection * 90f, 0f));
        }
    }

    public bool DropTrap(Node dropAtNode)
    {
        if (_currentTrap)
        {
            try
            {
                DynamicOutline outline = _currentTrap.GetComponent<DynamicOutline>();

                outline.SetOutlineDefault();
            }
            catch (NullReferenceException)
            {
                Debug.Log("Could not retrieve DynamicOutline script");
            }

            _currentTrap.GetComponent<Collider>().enabled = true;
            if(_currentTrap.GetComponent<SphereColliderWireframe>() != null) _currentTrap.GetComponent<SphereColliderWireframe>().enabled = true;
            _currentTrap.GetComponentInChildren<TypeOfTrap>().TrapOwner = _playerControls.PlayerID;
            _currentTrap = null;
            _playerControls.GameGrid.UpdateNode(dropAtNode);

            GameManager.TrapData currentTrapData = _trapsDict[_currentAbility];
            currentTrapData.currentCount -= 1;
            _pAbilitiesUI.UpdateAbilityCountText(currentTrapData.countText, currentTrapData.currentCount);
            currentTrapData.timer += currentTrapData.cooldown;
            _pAbilitiesUI.UpdateCooldownFill(currentTrapData.fillImage, currentTrapData.cooldown);
            _currentAbility = AbilitiesEnum.NONE;
            return true;
        }
        Debug.Log("Failed to set " + _currentAbility);
        return false;
    }
    #endregion

    #region Coroutines
    IEnumerator TrapSetupTimer(float setupTime, Node trapAtNode)
    {
        yield return new WaitForSeconds(setupTime);
        _animator.SetBool("installePiege", false);
        DropTrap(trapAtNode);
    }
    #endregion

    #region Gizmos
    public void OnDrawGizmos()
    {
        if (_whistle != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, _whistle.whistleFleeRange);
            Gizmos.DrawWireSphere(transform.position, _whistle.whistleCaptureRange);
        }        
    }
    #endregion
}
