using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Range(0f, 20f)] public float speed;
    [Range(0f, 10f)] public float rotationSpeed;

    private Rigidbody rb;
    private float joystickPointDisplayDistance = 2;

    Vector3 joystickVirtualPoint;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector2 joystickInput = Gamepad.current.leftStick.ReadValue();
        float joystickInputMagnitude = Mathf.Sqrt(joystickInput.x * joystickInput.x + joystickInput.y * joystickInput.y);

        joystickVirtualPoint = new Vector3(transform.position.x + joystickInput.x * joystickPointDisplayDistance, transform.position.y, transform.position.z + joystickInput.y * joystickPointDisplayDistance);
        transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, joystickVirtualPoint - transform.position, rotationSpeed * Time.deltaTime, 0.0f));
        rb.velocity = transform.forward * joystickInputMagnitude * speed;        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(joystickVirtualPoint, 0.5f);
    }
}
