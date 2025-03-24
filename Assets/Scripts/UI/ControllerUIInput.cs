using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ControllerUIInput : MonoBehaviour
{
    public GameObject FirstSelectedButton;

    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(FirstSelectedButton);
    }
}
