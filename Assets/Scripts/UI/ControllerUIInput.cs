using UnityEngine;
using UnityEngine.EventSystems;

public class ControllerUIInput : MonoBehaviour
{
    public GameObject FirstSelectedButton;

    private void OnEnable()
    {
        SetSelectedButton();
    }

    private void Update()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
        {
            SetSelectedButton();
        }
    }

    private void SetSelectedButton()
    {
        if (FirstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(FirstSelectedButton);
        }
    }
}
