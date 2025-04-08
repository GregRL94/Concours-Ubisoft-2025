using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;



using UnityEngine.UI;

namespace AkuroTools
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;
        public AudioMixerGroup soundEffectMixer;
        private float soundEffectVolume;

        public AudioMixerGroup ostMixer;
        public AudioMixerGroup adaptativeOSTMixer;
        private float ostMixerVolume;

        [System.Serializable]
        public class KeyValue
        {
            public string audioName;
            public AudioClip audio;
        }
        [HideInInspector]
        public List<KeyValue> audioList = new List<KeyValue>();
        public Dictionary<string, AudioClip> allAudio = new Dictionary<string, AudioClip>();
        
        [SerializeField]
        private List<GameObject> playingSounds = new List<GameObject>();

        private List<AudioSource> adaptativeSounds = new List<AudioSource>();

        private List<string> audioCoroutines = new List<string>();

        public float maxFadeInTime = 2f;
        public float maxFadeOutTime = 2f;
        private float fadeInTime;
        private float fadeOutTime;

        //petit kiff
        public Image omoriBG;
        public Image iromoBG;

        [SerializeField]
        private float _speedupMusic = 1.25f;

        public enum TransitionType
        {
            ADAPTATIVE,
            BRUTE,
            FADE
        }

        private void Awake()
        {
            if (instance != null)
            {
                instance.playingSounds.Clear();
                Destroy(this.gameObject);
            }
            instance = this;

            DontDestroyOnLoad(this.gameObject);

            foreach(var audio in audioList)
            {
                allAudio[audio.audioName] = audio.audio;
            }

            //ostMixerVolume = ostMixer.audioMixer.
            
        }

        private void Start()
        {
            fadeInTime = maxFadeInTime;
            fadeOutTime = maxFadeOutTime;
        }


        public AudioSource PlayClipAt(AudioClip clip, Vector3 pos, AudioMixerGroup whatMixer, bool isSFX, bool islooping)
        {
            //Create GameObject
            GameObject tempGO = new GameObject("TempAudio");
            //pos of GO
            tempGO.transform.position = pos;
            //Add an audiosource
            AudioSource audioSource = tempGO.AddComponent<AudioSource>();
            audioSource.clip = clip;
            //Get the audio mixer
            audioSource.outputAudioMixerGroup = whatMixer;
            audioSource.loop = islooping;
            if(isSFX)audioSource.PlayOneShot(audioSource.clip);
            else
            {
                    Debug.Log(playingSounds.Count);
                if(SearchSound(clip.name))
                {
                    Destroy((tempGO));
                    return null;
                }
                playingSounds.Add(tempGO);
                audioSource.Play();
            }
            //Destroy at the lenght of the clip
            if(!audioSource.loop)Destroy(tempGO, clip.length);
            return audioSource;
        }


        public void SetOSTVolume(float volume)
        {
            ostMixer.audioMixer.SetFloat("OST", volume);
        }

        public void SetSFXVolume(float volume)
        {
            soundEffectMixer.audioMixer.SetFloat("SFX", volume);
        }

        /*
            Change la musique selon le type de transition
        */
        public void ChangeMusic(string nextMusic, AudioMixerGroup musicMixer, TransitionType transitionType)
        {
            switch(transitionType)
            {
                case TransitionType.ADAPTATIVE :
                    /*
                            AUDIOMIXER
                        - fade out l'audiomixer de la musique
                        - fade in l'audiomixer adaptative
                        - stop la musique actuel quand l'audiomixer actuel arrive à 0
                        - transfere la musique adaptative quand on entend l'audiomixer adaptative normalement
                        - reset les paramètres d'audiomixer comme avant

                            CLIP
                        - met la musique choisi dans l'audiomixer adaptative
                        - lance la musique adaptative à partir du même point que celle de la musique actuel
                    */
                
                    //prototype
                    StartCoroutine(MusicFadeOut(ostMixer, false, 0f, -40f, nextMusic));
                    AudioSource adaptativeMusic = PlayNextMusic(nextMusic, adaptativeOSTMixer, true, maxFadeInTime);
                    StartCoroutine(MusicFadeIn(adaptativeOSTMixer, true, adaptativeMusic, -20f, 0f, nextMusic));
                    //Debug.Log(previousMusicTime);
                break;

                case TransitionType.BRUTE :
                    PlayNextMusic(nextMusic, ostMixer, true);



                    StartCoroutine(MusicFadeOut(ostMixer, false, 0, 0, nextMusic));
                    StartCoroutine(MusicFadeIn(adaptativeOSTMixer, false, null, 0, 0f, nextMusic));
                    //StartCoroutine(MusicFadeOut(nextMusic, musicMixer, fadeTime));
                break;

                case TransitionType.FADE :
                    StartCoroutine(MusicFadeOut(ostMixer, true, 0, -40, nextMusic));
                    StartCoroutine(WaitBeforePlayingNext(nextMusic, maxFadeOutTime, ostMixer, true));
                    

                break;
            }

        }

        /*
            Joue la prochaine musique
        */
        private AudioSource PlayNextMusic(string nextMusicName, AudioMixerGroup audioMixer, bool isAdaptative = false, float destoyTime = 0f)
        {
            AudioSource nextMusic;
            float adaptativeTime = 0f;
            switch(playingSounds.Count)
            {
                case > 0 :
                    for(int i = 0; i < playingSounds.Count; i++)
                    {
                        AudioSource musicAS = playingSounds[i].GetComponent<AudioSource>();
                        if(musicAS.outputAudioMixerGroup != ostMixer)continue;
                        //Debug.Log("previous music time : " + musicAS.time);
                        if(isAdaptative)adaptativeTime = musicAS.time;
                        playingSounds.RemoveAt(i);
                        Destroy(musicAS.gameObject, destoyTime);
                    }
                    nextMusic = PlayClipAt(allAudio[nextMusicName], this.transform.position, audioMixer, false, true);
                    
                    if(isAdaptative)nextMusic.time = adaptativeTime;
                    //Debug.Log($"{playingSounds.Count} MUSICS, PLAYING NEXT MUSIC : {nextMusicName}");
                break;
            
                case <= 0 :
                    nextMusic = PlayClipAt(allAudio[nextMusicName], this.transform.position, audioMixer, false, true);
                    //Debug.Log($"NO MUSIC, PLAYING NEXT MUSIC {nextMusicName}");
                break;

            }

            return nextMusic;
        }

        private IEnumerator WaitBeforePlayingNext(string nextMusicName, float waitingTime, AudioMixerGroup audioMixer, bool isAdaptative = false)
        {
            if(audioCoroutines.Contains("WaitBeforePlayingNext"))yield return null;
            audioCoroutines.Add("WaitBeforePlayingNext");
            yield return new WaitForSeconds(waitingTime);
            PlayNextMusic(nextMusicName, audioMixer, isAdaptative);
            audioCoroutines.Remove("WaitBeforePlayingNext");
        }

        /*
            Fade out la musique
        */
        private IEnumerator MusicFadeOut(AudioMixerGroup musicMixer, bool fadeInAfter = false, float initialVolume = 0f, float maxFadeOut = -40f, string ui = "")
        {
            if(audioCoroutines.Contains("MusicFadeOut"))yield return null;
            audioCoroutines.Add("MusicFadeOut");
            //Debug.Log("FADE OUT");
            float elapsedTime = 0f;
            float volumeOST;
            musicMixer.audioMixer.GetFloat("VolumeOST", out volumeOST);
            Debug.Log(ui);
            while(true)
            {
                if(fadeOutTime <= 0f)
                {
                    fadeOutTime = maxFadeOutTime;
                    if(fadeInAfter)StartCoroutine(MusicFadeIn(musicMixer, false, null, maxFadeOut, 0, ui));
                    audioCoroutines.Remove("MusicFadeOut");
                    StopCoroutine("MusicFadeOut");
                    break;
                }
                //Debug.Log(fadeOutTime);
                fadeOutTime -= Time.deltaTime;
                elapsedTime += Time.deltaTime;
                //Debug.Log(elapsedTime);
                volumeOST = Mathf.Lerp(initialVolume, maxFadeOut, elapsedTime / maxFadeOutTime);
                musicMixer.audioMixer.SetFloat("VolumeOST", volumeOST);


                if(ui == "Musique1")iromoBG.color = new Color(iromoBG.color.r, iromoBG.color.g, iromoBG.color.b, Mathf.Lerp(iromoBG.color.a, 0, elapsedTime / (maxFadeOutTime * 20)));
                if(ui == "Musique2")omoriBG.color = new Color(omoriBG.color.r, omoriBG.color.g, omoriBG.color.b, Mathf.Lerp(omoriBG.color.a, 0, elapsedTime / (maxFadeOutTime * 20)));
                yield return null;
            }
        }

        /*
            Fade in la musique
        */

        private IEnumerator MusicFadeIn(AudioMixerGroup musicMixer, bool isAdaptative = false, AudioSource adaptativeMusic = null, float initialVolume = -80f, float maxFadeIn = 0, string ui = "")
        {
            if(audioCoroutines.Contains("MusicFadeIn"))yield return null;
            audioCoroutines.Add("MusicFadeIn");
            //Debug.Log("FADE IN");
            float elapsedTime = 0f;
            float volOST;
            string volumeName = "Volume";
            if(musicMixer == ostMixer)volumeName = "VolumeOST";
            else if(musicMixer == adaptativeOSTMixer)volumeName = "VolumeAdapt";
            musicMixer.audioMixer.GetFloat(volumeName, out volOST);
            while(true)
            {
                if(fadeInTime <= 0f)
                {
                    fadeInTime = maxFadeInTime;
                    if(isAdaptative)
                    {
                        ostMixer.audioMixer.SetFloat("VolumeOST", 0f);
                        adaptativeOSTMixer.audioMixer.SetFloat("VolumeAdapt", -40f);
                        adaptativeMusic.outputAudioMixerGroup = ostMixer;
                    }
                    //Debug.Log("NEW MUSIC PLAY");
                    StopCoroutine("MusicFadeIn");
                    yield return null;
                    break;
                }
                //Debug.Log(fadeInTime);
                elapsedTime += Time.deltaTime;
                //Debug.Log(elapsedTime);
                fadeInTime -= Time.deltaTime;
                volOST = Mathf.Lerp(initialVolume, maxFadeIn, elapsedTime / maxFadeInTime);
                musicMixer.audioMixer.SetFloat(volumeName, volOST);


                if(ui == "Musique2")iromoBG.color = new Color(iromoBG.color.r, iromoBG.color.g, iromoBG.color.b, Mathf.Lerp(iromoBG.color.a, 1, elapsedTime / (maxFadeInTime * 20)));
                if(ui == "Musique1")omoriBG.color = new Color(omoriBG.color.r, omoriBG.color.g, omoriBG.color.b, Mathf.Lerp(omoriBG.color.a, 1, elapsedTime / (maxFadeInTime * 20)));
                yield return null;
            }
        }

        /*
            Enlève un son s'il existe
        */

        public void RemoveSound(string name)
        {
            GameObject sound = SearchSound(name);
            if(sound == null)return;
            playingSounds.Remove(sound);
            Destroy(sound);
            
        }

        /*
            pause un son s'il existe
        */

        public void PauseSound(string name)
        {
            AudioSource sound = SearchSound(name).GetComponent<AudioSource>();
            if(sound == null)return;
            sound.Pause();
        }

        /*
            reprend la musique qui était en pause
        */

        public void UnPauseSound(string name)
        {
            AudioSource sound = SearchSound(name).GetComponent<AudioSource>();
            if(sound == null)return;
            sound.UnPause();
        }

        /*
            Cherche le son demandé dans les sons joués
        */
        private GameObject SearchSound(string soundName)
        {
            foreach(GameObject sound in playingSounds)
            {
                AudioSource soundAS = sound.GetComponent<AudioSource>();
                if(soundAS.clip.name == soundName)
                {
                    return soundAS.gameObject;
                }
            }
            return null;
        }


        public void AddAdaptativeSound(string soundName)
        {

        }

        public void RemoveAdaptativeSound(string soundName)
        {

        }

        public void ClearAllAdaptativeSound()
        {
            foreach(AudioSource aS in adaptativeSounds)
            {
                Destroy(aS.gameObject);
            }
            adaptativeSounds.Clear();
        }
        
        public void SpeedUpMusic() => playingSounds[0].GetComponent<AudioSource>().pitch = _speedupMusic;
        
        public void OriginalMusicSpeed() => playingSounds[0].GetComponent<AudioSource>().pitch = 1;
    }

}
