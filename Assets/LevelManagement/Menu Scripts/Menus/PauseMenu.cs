using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelManagement
{
    public class PauseMenu : Menu<PauseMenu>
    {
        ////////// DEBUG PAUSE MENU CONCOURS UBISOFT
        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            print("Pause");
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!gameObject.activeInHierarchy)
                {
                    gameObject.SetActive(true);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

        //////////


        public void OnResumePressed()
        {
            Time.timeScale = 1;
            base.OnBackPressed();
        }

        public void OnRestartPressed()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            base.OnBackPressed();
        }

        public void OnMainMenuPressed()
        {
            Time.timeScale = 1;
            LevelLoader.LoadMainMenuLevel();
            MainMenu.Open();
        }

        public void OnQuitPressed()
        {
            Application.Quit();
            #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false; // Exit option for editor 
            #endif
        }
    }
}
