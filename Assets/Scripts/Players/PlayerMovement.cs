using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Range(0f, 10f)] public float speed;

    private Rigidbody rb;
    private float speedFactor = 500f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        var controllers = Input.GetJoystickNames();      
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 joystickInput = Gamepad.current.leftStick.ReadValue();
        float finalSpeed = speed * speedFactor * Time.deltaTime;

        rb.velocity = new Vector3(joystickInput.x * finalSpeed, rb.velocity.y, joystickInput.y * finalSpeed);
    }
}
