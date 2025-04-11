using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelManagement
{
    public class PauseMenu : Menu<PauseMenu>
    {
        public void OnResumePressed()
        {
            Time.timeScale = 1;
            gameObject.SetActive(false);
            base.OnBackPressed();
        }

        public void OnRestartPressed()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            GameData.ResetPlayerPoints();
            base.OnBackPressed();
        }

        public void OnMainMenuPressed()
        {
            Time.timeScale = 1;
            LevelLoader.LoadMainMenuLevel();
            GameData.ResetPlayerPoints();
            MainMenu.Open();
        }

        public void OnQuitPressed()
        {
            GameData.ResetPlayerPoints();
            Application.Quit();
            #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false; // Exit option for editor 
            #endif
        }
    }
}
