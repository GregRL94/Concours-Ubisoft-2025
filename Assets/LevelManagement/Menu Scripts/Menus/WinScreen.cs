using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace LevelManagement
{
    public class WinScreen : Menu<WinScreen>
    {

        [SerializeField]
        private float _playDelay = 0f;

        [SerializeField]
        private TransitionFader startTransitionPrefab;


        void OnEnable()
        {
            Time.timeScale = 0f; // Pause Game if Player Wins
        }

        void OnDisable()
        {
            Time.timeScale = 1f; // UnPause Game if Player Wins
        }


        private IEnumerator OnPlayPressedRoutine()
        {
            TransitionFader.PlayTransition(startTransitionPrefab);
            yield return new WaitForSecondsRealtime(_playDelay);
            LevelLoader.LoadNextLevel();
            GameMenu.Open();
        }

        public void OnNextLevelPressed()
        {
            StartCoroutine(OnPlayPressedRoutine());
        }


        public void OnRestartPressed()
        {
            base.OnBackPressed();
            LevelLoader.ReloadLevel();
        }

        public void OnMainMenuPressed()
        {
            LevelLoader.LoadMainMenuLevel();
            MainMenu.Open();
        }

    }
}
