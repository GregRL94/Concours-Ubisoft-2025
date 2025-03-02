using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField, Range(0f, 20f)] private float speed;
    [SerializeField, Range(0f, 10f)] private float rotationSpeed;

    private Rigidbody rb;
    private float joystickPointDisplayDistance = 2;

    Vector3 joystickVirtualPoint;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (Gamepad.current != null)
        {
            Vector2 joystickInput = Gamepad.current.leftStick.ReadValue();
            float joystickInputMagnitude = Mathf.Sqrt(joystickInput.x * joystickInput.x + joystickInput.y * joystickInput.y);

            joystickVirtualPoint = new Vector3(transform.position.x + joystickInput.x * joystickPointDisplayDistance, transform.position.y, transform.position.z + joystickInput.y * joystickPointDisplayDistance);
            transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, joystickVirtualPoint - transform.position, rotationSpeed * Time.deltaTime, 0.0f));
            rb.velocity = transform.forward * joystickInputMagnitude * speed;
        }
        else
        {
            Debug.Log("Connect a Controller !!");
        }        
    }

    private void OnDrawGizmos()
    {
        if (Gamepad.current != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(joystickVirtualPoint, 0.5f);
        }        
    }
}
