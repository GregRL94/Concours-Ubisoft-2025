using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace LevelManagement
{
    [RequireComponent(typeof(ScreenFader))]
    public class SplashScreen : MonoBehaviour
    {
        [SerializeField]
        private ScreenFader _screenFader;

        [SerializeField]
        private float delay;

        private bool _isFading = false;

        private void Awake()
        {
            _screenFader = GetComponent<ScreenFader>();
        }

        private void Start()
        {
            _screenFader.FadeOn();
        }

        private void Update()
        {
            if (!_isFading && AnyGamepadButtonPressed())
            {
                _isFading = true;
                FadeAndLoad();
            }
        }

        public void FadeAndLoad()
        {
            StartCoroutine(FadeAndLoadRoutine());
        }

        private IEnumerator FadeAndLoadRoutine()
        {
            yield return new WaitForSeconds(delay);
            _screenFader.FadeOff();
            LevelLoader.LoadMainMenuLevel();
            yield return new WaitForSeconds(_screenFader.FadeOffDuration);
            Destroy(gameObject);
        }

        private bool AnyGamepadButtonPressed()
        {
            if (Gamepad.current == null) return false;

            foreach (var control in Gamepad.current.allControls)
            {
                if (control is ButtonControl button && button.isPressed)
                    return true;
            }
            return false;
        }
    }
}
