using AkuroTools;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LevelManagement
{
    public class CreditsScreen : Menu<CreditsScreen>
    {
        private InputAction backAction;
        public GameObject mainMenu;


        private void OnEnable()
        {
            AudioManager.instance.PlayClipAt(AudioManager.instance.allAudio["UI Open Menu"], this.transform.position, AudioManager.instance.soundEffectMixer, true, false);
            backAction = playerInput.UI.Back; 
            backAction.Enable();
            backAction.performed += OnBackPressedAction;
        }

        private void OnDisable()
        {
            AudioManager.instance.PlayClipAt(AudioManager.instance.allAudio["UI Close Menu"], this.transform.position, AudioManager.instance.soundEffectMixer, true, false);
            backAction.performed -= OnBackPressedAction;
            backAction.Disable();
        }

        private void OnBackPressedAction(InputAction.CallbackContext context)
        {
            mainMenu.SetActive(true);
            gameObject.SetActive(false);
        }

        //public override void OnBackPressed()
        //{
        //    // add extra logic before
        //    base.OnBackPressed();
        //    // add extra logic after
        //}
    }
}
