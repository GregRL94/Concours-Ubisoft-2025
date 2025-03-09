using UnityEngine;
using UnityEngine.InputSystem;

public class TestController : MonoBehaviour
{
    void Update()
    {
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            Debug.Log("Bouton Sud appuyé !");
        }
    }
}
