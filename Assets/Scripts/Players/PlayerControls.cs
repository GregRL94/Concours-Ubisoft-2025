using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class PlayerControls : MonoBehaviour
{
    #region Variables
    [Header("MOVEMENT PARAMETERS")]
    [SerializeField, Range(0f, 20f)] private float _speed;
    [SerializeField, Range(0f, 10f)] private float _rotationSpeed;
    [Space]
    [Header("INTERACTIONS PARAMETERS")]
    [SerializeField] private PlayerEnum _playerID;
    [SerializeField, Range(0f, 10f)] private float _raycastStartDistance;
    [SerializeField, Range(0f, 10f)] private float _interactionDistance;
    [SerializeField] private LayerMask _trapsMask;
    [SerializeField] private LayerMask _robberMask;
    [Space]
    [Header("ABILITIES BINDING")]
    [SerializeField] private AbilitiesEnum _rJoystickUPBind;
    [SerializeField] private AbilitiesEnum _rJoystickDOWNBind;
    [SerializeField] private AbilitiesEnum _rJoystickLEFTBind;
    [SerializeField] private AbilitiesEnum _rJoystickRIGHTBind;
    [Space]
    [Header("GIZMOS PARAMETERS")]
    [SerializeField] private bool _drawGizmos;
    [SerializeField, Range(0.1f, 0.5f)] private float _pointsRadii = 0.25f;
    
    private GameGrid _gameGrid;
    private Node _previousNode;
    private Node _currentNode;
    private Vector3 _snappedInteractionPoint;
    private GameObject _currentTrap;

    private InputActionAsset _inputAsset;
    private InputActionMap _playerControls;
    private PlayerActions _playerActions;
    private Dictionary<R_JoystickDirection, AbilitiesEnum> _bindingDict;

    private Rigidbody _rb;
    private Vector2 _leftJoystickInput;
    private Vector3 _leftjoystickVirtualPoint;
    private Animator _animator;

    private readonly float _joystickPointDisplayDistance = 2f;

    public PlayerEnum PlayerID => _playerID;
    public GameGrid GameGrid => _gameGrid;

    private Gamepad _gamepad; 
    #endregion

    #region MonoBehaviour Flow
    private void Awake()
    {
        _inputAsset = GetComponent<PlayerInput>().actions;
        _playerControls = _inputAsset.FindActionMap("PlayerControls");
        _playerActions = GetComponent<PlayerActions>();
    }

    void Start()
    {
        _gameGrid = GameGrid.Instance;
        _bindingDict = new Dictionary<R_JoystickDirection, AbilitiesEnum>
        {
            [R_JoystickDirection.UP] = _rJoystickUPBind,
            [R_JoystickDirection.DOWN] = _rJoystickDOWNBind,
            [R_JoystickDirection.LEFT] = _rJoystickLEFTBind,
            [R_JoystickDirection.RIGHT] = _rJoystickRIGHTBind,
            [R_JoystickDirection.NONE] = AbilitiesEnum.NONE,
        };
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        EnablePlayerInputs(true);
        _currentNode = _gameGrid.NodeFromWorldPos(transform.position);
        _previousNode = _currentNode;
        _gameGrid.UpdateNode(_currentNode);
    }

    public void Initialize(PlayerEnum id, Gamepad assignedGamepad)
    {
        _playerID = id;
        _gamepad = assignedGamepad;

        var playerInput = GetComponent<PlayerInput>();

        // Dissocier d'abord les anciens périphériques
        playerInput.user.UnpairDevices();

        // Associer manette spécifique
        InputUser.PerformPairingWithDevice(assignedGamepad, playerInput.user);

        // Activer le bon scheme (important pour qu'il prenne les bons bindings)
        playerInput.SwitchCurrentControlScheme("Gamepad", assignedGamepad);

        Debug.Log($"{_playerID} a été initialisé avec {_gamepad.displayName} sur {gameObject.name}");
    }

    private void FixedUpdate()
    {
        float joystickInputMagnitude = Mathf.Sqrt(_leftJoystickInput.x * _leftJoystickInput.x + _leftJoystickInput.y * _leftJoystickInput.y);

        _leftjoystickVirtualPoint = new Vector3(transform.position.x + _leftJoystickInput.x * _joystickPointDisplayDistance, transform.position.y, transform.position.z + _leftJoystickInput.y * _joystickPointDisplayDistance);
        transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, _leftjoystickVirtualPoint - transform.position, _rotationSpeed * Time.deltaTime, 0.0f));
        _rb.velocity = joystickInputMagnitude * _speed * transform.forward;

        //Gestion de l'animation de marche
        _animator.SetBool("EstEnMouvement", _rb.velocity.magnitude > 0f);
        _animator.SetFloat("Mouvement", joystickInputMagnitude);

        Node node = _gameGrid.NodeFromWorldPos(transform.position);

        if (node != _currentNode)
        {
            _previousNode = _currentNode;
            _currentNode = node;
        }

        _gameGrid.UpdateNode(_currentNode);
        _gameGrid.UpdateNode(_previousNode);
    }

    private void Update()
    {
        Vector3 raycastStartPoint = transform.position + transform.forward * _raycastStartDistance;
        Vector3 interactionPoint = raycastStartPoint + transform.forward * _interactionDistance;
        _snappedInteractionPoint = _gameGrid.SnapToGridPos(interactionPoint);

        Ray ray = new Ray(raycastStartPoint, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _trapsMask))
        {
            if (hit.collider.gameObject.CompareTag("TRAP"))
            {
                if (_currentTrap != hit.collider.gameObject)
                {
                    TypeOfTrap trap = hit.collider.gameObject.GetComponentInChildren<TypeOfTrap>();

                    try
                    {
                        if (trap.TrapOwner == _playerID)
                        {
                            _currentTrap = hit.collider.gameObject;
                            Debug.Log("GATHERED TRAP");
                        }
                    }
                    catch (NullReferenceException)
                    {
                        Debug.Log("Could not retrieve TypeOfTrap script");
                    }
                }                
            }
            else if (_currentTrap != null)
            {
                _currentTrap = null;
                Debug.Log("CLEARED TRAP");
            }
        }
        else if (_currentTrap != null)
        {
            _currentTrap = null;
            Debug.Log("CLEARED TRAP");
        }

        _playerActions.UpdateActionState(Time.deltaTime, _snappedInteractionPoint);
    }
    #endregion

    #region Movement
    public void Movement(InputAction.CallbackContext context)
    {
        if (!GameManager.Instance.TutorialManager.IsTutorialCompleted && GameManager.Instance.TutorialManager.CurrentTutorialType == UITutorialStep.MOVE_STEP) GameManager.Instance.UIManager.CurrentPlayerValidation.DynamicValidatePage(_playerID);
        _leftJoystickInput = context.ReadValue<Vector2>();        
    }

    public void Stop(InputAction.CallbackContext context)
    {
        _leftJoystickInput = Vector2.zero;
    }
    #endregion

    #region Actions
    private void ActionSelection(InputAction.CallbackContext context)
    {
        Vector2 rightJoystickInput = context.ReadValue<Vector2>();
        R_JoystickDirection r_joystick_dir = R_JoystickDirection.NONE;

        if (rightJoystickInput.x == 1f)
        {
            r_joystick_dir = R_JoystickDirection.RIGHT;
        }
        else if (rightJoystickInput.x == -1f)
        {
            r_joystick_dir = R_JoystickDirection.LEFT;
        }
        else if (rightJoystickInput.y == 1f)
        {
            r_joystick_dir = R_JoystickDirection.UP;
        }
        else if (rightJoystickInput.y == -1f)
        {
            r_joystick_dir = R_JoystickDirection.DOWN;
        }

        if (r_joystick_dir != R_JoystickDirection.NONE)
        {
            _playerActions.SelectAction(_bindingDict[r_joystick_dir]);
        }        
    }

    private void ActionDeselection(InputAction.CallbackContext context)
    {
        _playerActions.DeselectAction();
    }

    private void Whistle(InputAction.CallbackContext context)
    {
        _playerActions.PerformWhistle(_robberMask);
    }
    #endregion

    #region Traps
    private void OnStartTrapDeployment(InputAction.CallbackContext context)
    {
        _playerActions.StartTrapDeployment(_gameGrid.NodeFromWorldPos(_snappedInteractionPoint));
    }

    private void OnCancelTrapDeployment(InputAction.CallbackContext context)
    {
        _playerActions.CancelTrapDeployment();
    }

    private void OnRotateTrap(InputAction.CallbackContext context)
    {
        _playerActions.RotateTrap(context.ReadValue<float>(), _currentTrap);
    }

    private void OnUIValidatePage(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.TutorialManager.CurrentTutorialType != UITutorialStep.TALK_STEP && !GameManager.Instance.UIManager.CurrentPlayerValidation.SimpleValidate) return;
        if (GameManager.Instance.UIManager.CurrentPlayerValidation == null) return;
        GameManager.Instance.UIManager.CurrentPlayerValidation.DynamicValidatePage(_playerID);
    }
    #endregion

    #region Enable & Disable
    private void EnablePlayerInputs(bool enableState)
    {
        InputAction movementAction = _playerControls.FindAction("Movement");
        InputAction actionActivation = _playerControls.FindAction("ActionActivation");
        InputAction actionUIValidate = _playerControls.FindAction("UIValidate");

        if (enableState)
        {
            _playerControls.Enable();
            movementAction.performed += Movement;
            movementAction.canceled += Stop;
            _playerControls.FindAction("ActionSelection").performed += ActionSelection;
            _playerControls.FindAction("ActionDeselection").performed += ActionDeselection;
            actionActivation.performed += Whistle;
            actionActivation.started += OnStartTrapDeployment;
            actionActivation.canceled += OnCancelTrapDeployment;
            _playerControls.FindAction("TrapRotation").performed += OnRotateTrap;

            actionUIValidate.performed += OnUIValidatePage;
        }
        else
        {
            movementAction.performed -= Movement;
            movementAction.canceled -= Stop;
            _playerControls.FindAction("ActionSelection").performed -= ActionSelection;
            _playerControls.FindAction("ActionDeselection").performed -= ActionDeselection;
            actionActivation.performed -= Whistle;
            actionActivation.started -= OnStartTrapDeployment;
            actionActivation.canceled -= OnCancelTrapDeployment;
            _playerControls.FindAction("TrapRotation").performed -= OnRotateTrap;
            
            actionUIValidate.performed -= OnUIValidatePage;
            _playerControls.Disable();
        }

    }
    private void OnEnable()
    {
        EnablePlayerInputs(true);
    }

    private void OnDisable()
    {
        EnablePlayerInputs(false);
    }

    private void OnDestroy()
    {
        EnablePlayerInputs(false);
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (_drawGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_leftjoystickVirtualPoint, _pointsRadii);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + transform.forward * _raycastStartDistance, transform.position + (_interactionDistance + _raycastStartDistance) * transform.forward);
            Gizmos.DrawSphere(_snappedInteractionPoint, _pointsRadii);

            if (_playerActions != null)
            {
                _playerActions.OnDrawGizmos();
            }            
        }
    }
    #endregion
}
