using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    #region Variables
    [Header("MOVEMENT PARAMETERS")]
    [SerializeField, Range(0f, 20f)] private float speed;
    [SerializeField, Range(0f, 10f)] private float rotationSpeed;
    [Space]
    [Header("INTERACTIONS PARAMETERS")]
    [SerializeField] private PlayerEnum playerID;
    [SerializeField, Range(0f, 10f)] private float raycastStartDistance;
    [SerializeField, Range(0f, 10f)] private float interactionDistance;
    [SerializeField, Range(0f, 10f)] private float whistleFleeDistance;
    [SerializeField, Range(0f, 10f)] private float whistleCaptureDistance;
    [SerializeField] private LayerMask gameAgentsMask;
    [Space]
    [Header("ABILITIES BINDING")]
    [SerializeField] private AbilitiesEnum rJoystickUPBind;
    [SerializeField] private AbilitiesEnum rJoystickDOWNBind;
    [SerializeField] private AbilitiesEnum rJoystickLEFTBind;
    [SerializeField] private AbilitiesEnum rJoystickRIGHTBind;
    [Space]
    [Header("ABILITIES COOLDOWN")]
    [SerializeField, Range(0f, 10f)] private float whistleCD;
    [SerializeField, Range(0f, 10f)] private float alarmTrapCD;
    [SerializeField, Range(0f, 10f)] private float pushTrapCD;
    [SerializeField, Range(0f, 10f)] private float captureTrapCD;
    [Space]
    [Header("TRAPS PREFABS")]
    [SerializeField] private GameObject alarmTrap;
    [SerializeField] private GameObject pushTrap;
    [SerializeField] private GameObject captureTrap;
    [Space]
    [Header("GIZMOS PARAMETERS")]
    [SerializeField] private bool drawGizmos;
    [SerializeField, Range(0.1f, 0.5f)] private float pointsRadii = 0.25f;
    
    private GameGrid gameGrid;
    private Node previousNode;
    private Node currentNode;
    private Vector3 snappedInteractionPoint;
    private GameObject currentTrap;

    private InputActionAsset inputAsset;
    private InputActionMap playerControls;
    private PlayerActions playerActions;
    private Dictionary<R_JoystickDirection, AbilitiesEnum> bindingDict;
    private AbilitiesEnum selectedAbility = AbilitiesEnum.NONE;
    private Coroutine currentCoroutine;

    private Rigidbody rb;
    private Vector2 leftJoystickInput;
    private Vector3 leftjoystickVirtualPoint;

    private float joystickPointDisplayDistance = 2f;

    public PlayerEnum PlayerID => playerID;
    #endregion

    #region MonoBehaviour Flow
    private void Awake()
    {
        inputAsset = GetComponent<PlayerInput>().actions;
        playerControls = inputAsset.FindActionMap("PlayerControls");
        playerActions = GetComponent<PlayerActions>();
    }

    void Start()
    {
        gameGrid = GameGrid.Instance;
        bindingDict = new Dictionary<R_JoystickDirection, AbilitiesEnum>
        {
            [R_JoystickDirection.UP] = rJoystickUPBind,
            [R_JoystickDirection.DOWN] = rJoystickDOWNBind,
            [R_JoystickDirection.LEFT] = rJoystickLEFTBind,
            [R_JoystickDirection.RIGHT] = rJoystickRIGHTBind,
            [R_JoystickDirection.NONE] = AbilitiesEnum.NONE,
        };
        rb = GetComponent<Rigidbody>();
        EnablePlayerInputs(true);
        currentNode = gameGrid.NodeFromWorldPos(transform.position);
        previousNode = currentNode;
        gameGrid.UpdateNode(currentNode);
    }

    private void FixedUpdate()
    {
        float joystickInputMagnitude = Mathf.Sqrt(leftJoystickInput.x * leftJoystickInput.x + leftJoystickInput.y * leftJoystickInput.y);

        leftjoystickVirtualPoint = new Vector3(transform.position.x + leftJoystickInput.x * joystickPointDisplayDistance, transform.position.y, transform.position.z + leftJoystickInput.y * joystickPointDisplayDistance);
        transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, leftjoystickVirtualPoint - transform.position, rotationSpeed * Time.deltaTime, 0.0f));
        rb.velocity = transform.forward * joystickInputMagnitude * speed;

        Node node = gameGrid.NodeFromWorldPos(transform.position);

        if (node != currentNode)
        {
            previousNode = currentNode;
            currentNode = node;
        }

        gameGrid.UpdateNode(currentNode);
        gameGrid.UpdateNode(previousNode);
    }

    private void Update()
    {
        Vector3 raycastStartPoint = transform.position + transform.forward * raycastStartDistance;
        Vector3 interactionPoint = raycastStartPoint + transform.forward * interactionDistance;
        snappedInteractionPoint = gameGrid.SnapToGridPos(interactionPoint);

        Ray ray = new Ray(raycastStartPoint, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, gameAgentsMask))
        {
            if (hit.collider.gameObject.CompareTag("TRAP"))
            {
                if (currentTrap != hit.collider.gameObject)
                {
                    TypeOfTrap trap = hit.collider.gameObject.GetComponentInChildren<TypeOfTrap>();

                    try
                    {
                        if (trap.TrapOwner == playerID)
                        {
                            currentTrap = hit.collider.gameObject;
                            Debug.Log("GATHERED TRAP");
                        }
                    }
                    catch (NullReferenceException)
                    {
                        Debug.Log("Could not retrieve TypeOfTrap script");
                    }
                }                
            }
            else if (currentTrap != null)
            {
                currentTrap = null;
                Debug.Log("CLEARED TRAP");
            }
        }
        else if (currentTrap != null)
        {
            currentTrap = null;
            Debug.Log("CLEARED TRAP");
        }
    }
    #endregion

    #region Movement
    public void Movement(InputAction.CallbackContext context)
    {
        leftJoystickInput = context.ReadValue<Vector2>();        
    }

    public void Stop(InputAction.CallbackContext context)
    {
        leftJoystickInput = Vector2.zero;
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

        if ((r_joystick_dir != R_JoystickDirection.NONE) && !(bindingDict[r_joystick_dir] == selectedAbility))
        {
            selectedAbility = bindingDict[r_joystick_dir];
            Debug.Log(selectedAbility);
        }        
    }

    private void ActionDeselection(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            selectedAbility = AbilitiesEnum.NONE;
            playerActions.OnAbilityDeselection();
        }        
    }

    private void Whistle(InputAction.CallbackContext context)
    {
        if (selectedAbility == AbilitiesEnum.WHISTLE)
        {
            // Play sound
            // Play Animation
            Collider[] agents = Physics.OverlapSphere(transform.position, whistleFleeDistance, gameAgentsMask);
            if (agents.Length > 0)
            {
                foreach (Collider collider in agents)
                {
                    if (collider.gameObject.CompareTag("ENEMY"))
                    {
                        RobberCapture robber = collider.gameObject.GetComponent<RobberCapture>();

                        if (Vector3.Distance(transform.position, collider.gameObject.transform.position) <= whistleCaptureDistance)
                        {
                            robber.GetSifled(playerID, 25f);
                        }
                        else
                        {
                            robber.GetSifled(playerID, 0f);
                        }
                    }
                }
            }
            Debug.Log("Whistled");
        }        
    }
    #endregion

    #region Traps
    private void StartTrapDeployment(InputAction.CallbackContext context)
    {
        Debug.Log("Started");
        currentCoroutine = StartCoroutine(TrapSetupTimer());
    }

    private void CancelTrapDeployment(InputAction.CallbackContext context)
    {
        Debug.Log("Canceled");
        StopCoroutine(currentCoroutine);
        currentCoroutine = null;
    }

    private void RotateTrap(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            float shoulderPressed = context.ReadValue<float>();
            int rotationDirection = 0;

            if (shoulderPressed < 0)
            {
                rotationDirection = -1;
            }
            else if (shoulderPressed > 0)
            {
                rotationDirection = 1;
            }

            if (currentTrap != null && rotationDirection != 0)
            {
                // Play sound
                currentTrap.transform.Rotate(new Vector3(0f, rotationDirection * 90f, 0f));
            }
        }
    }
    #endregion

    #region Enable & Disable
    private void EnablePlayerInputs(bool enableState)
    {
        InputAction movementAction = playerControls.FindAction("Movement");
        InputAction actionActivation = playerControls.FindAction("ActionActivation");

        if (enableState)
        {
            playerControls.Enable();
            movementAction.performed += Movement;
            movementAction.canceled += Stop;
            playerControls.FindAction("ActionSelection").performed += ActionSelection;
            playerControls.FindAction("ActionDeselection").performed += ActionDeselection;
            actionActivation.performed += Whistle;
            actionActivation.started += StartTrapDeployment;
            actionActivation.canceled += CancelTrapDeployment;
            playerControls.FindAction("TrapRotation").performed += RotateTrap;
        }
        else
        {
            movementAction.performed -= Movement;
            movementAction.canceled -= Stop;
            playerControls.FindAction("ActionSelection").performed -= ActionSelection;
            playerControls.FindAction("ActionDeselection").performed -= ActionDeselection;
            actionActivation.performed -= Whistle;
            actionActivation.started -= StartTrapDeployment;
            actionActivation.canceled -= CancelTrapDeployment;
            playerControls.FindAction("TrapRotation").performed -= RotateTrap;
            playerControls.Disable();
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

    IEnumerator TrapSetupTimer(float setupTime=5f)
    {
        yield return new WaitForSeconds(setupTime);
        Debug.Log("Done");
    }

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(leftjoystickVirtualPoint, pointsRadii);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + transform.forward * raycastStartDistance, transform.position + (interactionDistance + raycastStartDistance) * transform.forward);
            Gizmos.DrawSphere(snappedInteractionPoint, pointsRadii);

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, whistleFleeDistance);
            Gizmos.DrawWireSphere(transform.position, whistleCaptureDistance);
        }
    }
    #endregion
}
