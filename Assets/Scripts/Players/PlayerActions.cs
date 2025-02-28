using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActions : MonoBehaviour
{
    [Header("INTERACTIONS PARAMETERS")]
    [SerializeField, Range(0f, 5f)] private float interactionDistance;
    [SerializeField] private LayerMask gameAgentsmask;
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

    private GameGrid gameGrid;
    private PlayerInputActions playerInputAction;
    private AbilitiesEnum selectedAbility;

    private Dictionary<R_JoystickDirection, AbilitiesEnum> bindingDict;
    private GameObject currentTrap;
    

    void Start()
    {
        gameGrid = GameGrid.Instance;
        playerInputAction = new PlayerInputActions();
        bindingDict = new Dictionary<R_JoystickDirection, AbilitiesEnum> {
            [R_JoystickDirection.UP] = rJoystickUPBind,
            [R_JoystickDirection.DOWN] = rJoystickDOWNBind,
            [R_JoystickDirection.LEFT] = rJoystickLEFTBind,
            [R_JoystickDirection.RIGHT] = rJoystickRIGHTBind,
            [R_JoystickDirection.NONE] = AbilitiesEnum.NONE,
        };
        EnablePlayerInputs();
    }

    void Update()
    {
        Vector3 interactionPoint = transform.position + interactionDistance * transform.forward;
        Vector3 snappedInteractionPoint = gameGrid.SnapToGridPos(interactionPoint);

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, gameAgentsmask))
        {
            if (hit.collider.gameObject.CompareTag("TRAP"))
            {
                currentTrap = gameObject;
            }
            else if (currentTrap != null)
            {
                currentTrap = null;
            }
        }
        else if (currentTrap != null)
        {
            currentTrap = null;
        }
    }

    private void ActionSelection(InputAction.CallbackContext callback)
    {
        Vector2 rightJoystickInput = callback.ReadValue<Vector2>();
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

        if (!(bindingDict[r_joystick_dir] == selectedAbility))
        {
            selectedAbility = bindingDict[r_joystick_dir];
        }

        Debug.Log(selectedAbility);
    }

    private void RotateTrap(InputAction.CallbackContext callback)
    {
        float shoulderPressed = callback.ReadValue<float>();
        int rotationDirection = 0;

        if (shoulderPressed < 0)
        {
            Debug.Log("LEFT SHOULDER PRESSED");
            rotationDirection = -1;
        }
        else if (shoulderPressed > 0)
        {
            Debug.Log("RIGHT SHOULDER PRESSED");
            rotationDirection = 1;
        }

        if (currentTrap != null && rotationDirection != 0)
        {
            currentTrap.transform.Rotate(new Vector3(0f, rotationDirection * 90f, 0f));
        }        
    }

    private void EnablePlayerInputs()
    {
        playerInputAction.PlayerActions.Enable();
        playerInputAction.PlayerActions.ActionSelection.performed += ActionSelection;
        playerInputAction.PlayerActions.TrapRotation.performed += RotateTrap;
    }

    private void DisablePlayerInputs()
    {
        playerInputAction.PlayerActions.ActionSelection.performed -= ActionSelection;
        playerInputAction.PlayerActions.TrapRotation.performed -= RotateTrap;
        playerInputAction.PlayerActions.Disable();
    }

    private void OnDestroy()
    {
        DisablePlayerInputs();
    }
}
