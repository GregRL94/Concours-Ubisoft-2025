using AkuroTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelManagement
{
    public class MainMenu : Menu<MainMenu>
    {
        [SerializeField]
        private float _playDelay = 0.5f;

        [SerializeField]
        private TransitionFader startTransitionPrefab;

        public void OnPlayPressed()
        {
            AudioManager.instance.PlayClipAt(AudioManager.instance.allAudio["UI Confirm"], this.transform.position, AudioManager.instance.soundEffectMixer, true, false);
            StartCoroutine(OnPlayPressedRoutine());
        }

        private IEnumerator OnPlayPressedRoutine()
        {
            if(startTransitionPrefab)
                TransitionFader.PlayTransition(startTransitionPrefab);
            
            yield return new WaitForSeconds(_playDelay);
            LevelLoader.LoadNextLevel();
        }

        public void OnSettingsPressed()
        {
            AudioManager.instance.PlayClipAt(AudioManager.instance.allAudio["UI Open Menu"], this.transform.position, AudioManager.instance.soundEffectMixer, true, false);
            SettingsMenu.Open();
        }

        public void OnCreditsPressed()
        {
            AudioManager.instance.PlayClipAt(AudioManager.instance.allAudio["UI Open Menu"], this.transform.position, AudioManager.instance.soundEffectMixer, true, false);
            CreditsScreen.Open();
        }

        public override void OnBackPressed()
        {
            AudioManager.instance.PlayClipAt(AudioManager.instance.allAudio["UI Close Menu"], this.transform.position, AudioManager.instance.soundEffectMixer, true, false);
            Application.Quit();
            #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false; // Exit option for editor 
            #endif
        }

        private void Start()
        {
            AudioManager.instance.PlayClipAt(AudioManager.instance.allAudio["Music Menu"], this.transform.position, AudioManager.instance.ostMixer, false, true);
        }
    }
}