using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelManagement
{
    [RequireComponent(typeof(ScreenFader))]
    public class SplashScreen : MonoBehaviour
    {
        [SerializeField]
        private ScreenFader _screenFader;

        [SerializeField]
        private float delay;

        private void Awake()
        {
            _screenFader = GetComponent<ScreenFader>();
        }

        private void Start()
        {
            _screenFader.FadeOn();
        }

        public void FadeAndLoad()
        {
            StartCoroutine(FadeAndLoadRoutine());
        }

        private IEnumerator FadeAndLoadRoutine()
        {
            print("delay "+ delay);
            yield return new WaitForSeconds(delay); // before when clicked splash screen
            _screenFader.FadeOff();
            LevelLoader.LoadMainMenuLevel();

            yield return new WaitForSeconds(_screenFader.FadeOffDuration); // wait x second till splash screen disappears

            Destroy(gameObject);

        }
    }
}
