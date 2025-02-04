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
        }

        public void OnSFXVolumeChanged(float volume)
        {
            _sfxVolumeSlider.value = volume;
        }

        public void OnMusicVolumeChanged(float volume)
        {
            _musicVolumeSlider.value = volume;
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
        }

    }
}
