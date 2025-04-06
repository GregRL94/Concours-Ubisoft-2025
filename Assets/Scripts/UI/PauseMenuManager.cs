using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenuPrefab;
    private GameObject pauseMenuInstance;
    private bool isPaused = false;
    private bool canToggle = true;

    void Start()
    {
        if (pauseMenuInstance == null)
        {
            pauseMenuInstance = Instantiate(pauseMenuPrefab);
            pauseMenuInstance.SetActive(false);
        }
    }

    void Update()
    {
        if(Gamepad.current != null)
        {
            if (Gamepad.current.startButton.wasPressedThisFrame && canToggle)
            {
                canToggle = false;
                TogglePause();
            }

            if (!Gamepad.current.startButton.isPressed) 
            {
                canToggle = true;
            }
        }
    }

    private void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    void PauseGame()
    {
        pauseMenuInstance.SetActive(true);
        isPaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        pauseMenuInstance.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f;
    }
}