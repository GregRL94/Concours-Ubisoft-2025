using AkuroTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LevelManagement
{
    public class SettingsMenu : Menu<SettingsMenu>
    {
        [SerializeField]
        private Slider _masterVolumeSlider;

        [SerializeField]
        private Slider _sfxVolumeSlider;

        [SerializeField]
        private Slider _musicVolumeSlider;


        protected override void Awake()
        {
            base.Awake();
        }

        public void OnMasterVolumeChanged(float volume)
        {
            _masterVolumeSlider.value = volume;
            AudioManager.instance.SetOSTVolume(_masterVolumeSlider.value);
            AudioManager.instance.SetSFXVolume(_masterVolumeSlider.value);
        }

        public void OnSFXVolumeChanged(float volume)
        {
            _sfxVolumeSlider.value = volume;
            AudioManager.instance.SetSFXVolume(_sfxVolumeSlider.value);

        }

        public void OnMusicVolumeChanged(float volume)
        {
            _musicVolumeSlider.value = volume;
            AudioManager.instance.SetOSTVolume(_musicVolumeSlider.value);
        }

        public override void OnBackPressed()
        {
            AudioManager.instance.PlayClipAt(AudioManager.instance.allAudio["UI Close Menu"], this.transform.position, AudioManager.instance.soundEffectMixer, true, false);
            base.OnBackPressed();
        }

    }
}
