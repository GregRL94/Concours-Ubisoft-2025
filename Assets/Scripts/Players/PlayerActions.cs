using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    [Header("-- WHISTLE PARAMETERS --")]
    [SerializeField, Range(0f, 10f)] private float _whistleFleeDistance;
    [SerializeField, Range(0f, 10f)] private float _whistleCaptureDistance;
    [SerializeField, Range(0f, 10f)] private float _whistleCooldown;
    [Space]
    [Header("-- TRAPS PARAMETERS --")]
    [Header("Traps Prefabs")]
    [SerializeField] private GameObject _alarmTrap;
    [SerializeField] private GameObject _pushTrap;
    [SerializeField] private GameObject _captureTrap;
    [Space]
    [Header("Traps Setup Time")]
    [SerializeField, Range(0f, 10f)] private float _alarmTrapSetupTime;
    [SerializeField, Range(0f, 10f)] private float _pushTrapSetupTime;
    [SerializeField, Range(0f, 10f)] private float _captureTrapSetupTime;
    [Space]
    [Header("Traps Cooldown")]
    [SerializeField, Range(0f, 10f)] private float _alarmTrapCooldown;
    [SerializeField, Range(0f, 10f)] private float _pushTrapCooldown;
    [SerializeField, Range(0f, 10f)] private float _captureTrapCooldown;

    private GameObject _currentTrap;
    private Dictionary<AbilitiesEnum, GameObject> _trapsDictionary;
    private Dictionary<AbilitiesEnum, int> _trapsCountDictionary;
    private Dictionary<AbilitiesEnum, float> _trapsSetupTimeDictionary;
    private Dictionary<AbilitiesEnum, float> _cooldownsDictionary;
    private Dictionary<AbilitiesEnum, float> _timersDictionary;
    private GameManager _gameManager;
    private PlayerControls _playerControls;
    private AbilitiesEnum _currentAbility;
    private Coroutine _currentCoroutine;


    private void Start()
    {
        _gameManager = GameManager.Instance;
        _playerControls = GetComponent<PlayerControls>();
        _trapsDictionary = new Dictionary<AbilitiesEnum, GameObject>
        {
            [AbilitiesEnum.ALARM_TRAP] = _alarmTrap,
            [AbilitiesEnum.PUSH_TRAP] = _pushTrap,
            [AbilitiesEnum.CAPTURE_TRAP] = _captureTrap        
        };

        if (_gameManager != null)
        {
            _trapsCountDictionary = new Dictionary<AbilitiesEnum, int>
            {
                [AbilitiesEnum.ALARM_TRAP] = _gameManager.MaxTrapsCount.alarmTrapsCount,
                [AbilitiesEnum.PUSH_TRAP] = _gameManager.MaxTrapsCount.pushTrapCount,
                [AbilitiesEnum.CAPTURE_TRAP] = _gameManager.MaxTrapsCount.captureTrapCount
            };
        }
        else
        {
            _trapsCountDictionary = new Dictionary<AbilitiesEnum, int>
            {
                [AbilitiesEnum.ALARM_TRAP] = 2,
                [AbilitiesEnum.PUSH_TRAP] = 2,
                [AbilitiesEnum.CAPTURE_TRAP] = 2
            };
        }

        _trapsSetupTimeDictionary = new Dictionary<AbilitiesEnum, float>
        {
            [AbilitiesEnum.ALARM_TRAP] = _alarmTrapSetupTime,
            [AbilitiesEnum.PUSH_TRAP] = _pushTrapSetupTime,
            [AbilitiesEnum.CAPTURE_TRAP] = _captureTrapSetupTime,
        };

        _cooldownsDictionary = new Dictionary<AbilitiesEnum, float>
        {
            [AbilitiesEnum.WHISTLE] = _whistleCooldown,
            [AbilitiesEnum.ALARM_TRAP] = _alarmTrapCooldown,
            [AbilitiesEnum.PUSH_TRAP] = _pushTrapCooldown,
            [AbilitiesEnum.CAPTURE_TRAP] = _captureTrapCooldown
        };

        _timersDictionary = new Dictionary<AbilitiesEnum, float>
        {
            [AbilitiesEnum.WHISTLE] = 0f,
            [AbilitiesEnum.ALARM_TRAP] = 0f,
            [AbilitiesEnum.PUSH_TRAP] = 0f,
            [AbilitiesEnum.CAPTURE_TRAP] = 0f
        };
    }

    public void UpdateActionState(float deltaTime, Vector3 interactionPoint)
    {
        List<KeyValuePair<AbilitiesEnum, float>> dictAsList = _timersDictionary.ToList();

        for (int i = 0; i < _timersDictionary.Count; i++)
        {
            if (dictAsList[i].Value > 0)
            {
                _timersDictionary[dictAsList[i].Key] -= deltaTime;
            }
        }

        if (_currentAbility != AbilitiesEnum.NONE && _currentAbility != AbilitiesEnum.WHISTLE)
        {
            PreviewTrap(_currentAbility, interactionPoint, _playerControls.GameGrid.NodeFromWorldPos(interactionPoint));
        }
    }

    public void SelectAction(AbilitiesEnum selectedAbility)
    {
        if (!(_currentAbility == selectedAbility))
        {
            if (_currentTrap != null)
            {
                Destroy(_currentTrap);
                _currentTrap = null;
            }
            _currentAbility = selectedAbility;
            Debug.Log(_currentAbility);
        }       
    }

    public void DeselectAction()
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
            _currentCoroutine = null;
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
        if (_currentAbility == AbilitiesEnum.WHISTLE && _timersDictionary[AbilitiesEnum.WHISTLE] <= 0)
        {
            // Play sound
            // Play Animation
            Collider[] agents = Physics.OverlapSphere(transform.position, _whistleFleeDistance, gameAgentsMask);

            if (agents.Length > 0)
            {
                foreach (Collider collider in agents)
                {
                    if (collider.gameObject.CompareTag("ENEMY"))
                    {
                        RobberCapture robber = collider.gameObject.GetComponent<RobberCapture>();

                        if (Vector3.Distance(transform.position, collider.gameObject.transform.position) <= _whistleCaptureDistance)
                        {
                            robber.GetSifled(_playerControls.PlayerID, 25f);
                        }
                        else
                        {
                            robber.GetSifled(_playerControls.PlayerID, 0f);
                        }
                    }
                }
            }
            _timersDictionary[AbilitiesEnum.WHISTLE] += _cooldownsDictionary[AbilitiesEnum.WHISTLE];
            Debug.Log("Whistled");
        }
        else if (_timersDictionary[AbilitiesEnum.WHISTLE] > 0)
        {
            Debug.Log(AbilitiesEnum.WHISTLE + " in cooldown !");
        }
    }

    public void PreviewTrap(AbilitiesEnum trap, Vector3 previewPosition, Node node)
    {
        if (_currentTrap != null)
        {
            _currentTrap.transform.position = previewPosition;
        }
        else
        {
            _currentTrap = Instantiate(_trapsDictionary[trap], previewPosition, Quaternion.identity, null);
        }        

        try
        {
            DynamicOutline outline = _currentTrap.GetComponent<DynamicOutline>();
            if (node.isFree)
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
        if (_currentAbility != AbilitiesEnum.NONE && _currentAbility != AbilitiesEnum.WHISTLE)
        {
            if (deployTrapAtNode.isFree && _trapsCountDictionary[_currentAbility] > 0 && _timersDictionary[_currentAbility] <= 0f)
            {
                _currentCoroutine = StartCoroutine(TrapSetupTimer(_trapsSetupTimeDictionary[_currentAbility], deployTrapAtNode));
                Debug.Log("Trap deployment started");
                return;
            }
            if (!deployTrapAtNode.isFree)
            {
                Debug.Log("Node is already occupied");
            }
            if (_timersDictionary[_currentAbility] > 0f)
            {
                Debug.Log(_currentAbility + " in cooldown ! time remaining: " + _timersDictionary[_currentAbility]);
            }
        }        
    }

    public void CancelTrapDeployment()
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
            _currentCoroutine = null;
            Debug.Log("Trap deployment canceled");
        }
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

            _playerControls.GameGrid.UpdateNode(dropAtNode);
            _currentTrap = null;
            _trapsCountDictionary[_currentAbility] -= 1;
            _timersDictionary[_currentAbility] += _cooldownsDictionary[_currentAbility];
            Debug.Log(_currentAbility + " set. Traps of type " + _currentAbility + " remaining: " + _trapsCountDictionary[_currentAbility]);
            _currentAbility = AbilitiesEnum.NONE;
            return true;
        }
        Debug.Log("Failed to set " + _currentAbility);
        return false;
    }

    IEnumerator TrapSetupTimer(float setupTime, Node trapAtNode)
    {
        yield return new WaitForSeconds(setupTime);
        DropTrap(trapAtNode);
    }

    #region Gizmos
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, _whistleFleeDistance);
        Gizmos.DrawWireSphere(transform.position, _whistleCaptureDistance);
    }
    #endregion
}
