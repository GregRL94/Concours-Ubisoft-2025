using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Range(0f, 20f)] public float force;
    [Range(0f, 20)] public float maxSpeed;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        var controllers = Input.GetJoystickNames();
    }

    void FixedUpdate()
    {
        Vector2 joystickInput = Gamepad.current.leftStick.ReadValue();

        rb.AddForce(joystickInput.x * force, 0f, joystickInput.y * force, ForceMode.Force);
        rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed), rb.velocity.y, Mathf.Clamp(rb.velocity.z, -maxSpeed, maxSpeed));
    }
}
