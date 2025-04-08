using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AkuroTools
{
    public class AudioAction : MonoBehaviour
    {
        private enum CollideType
        {
            ENTERTRIGGER2D,
            ENTERTRIGGER3D,
            EXITTRIGGER2D,
            EXITTRIGGER3D,
            ENTERCOLLISION2D,
            ENTERCOLLISION3D,
            EXITCOLLISION2D,
            EXITCOLLISION3D,
            UIBUTTON
        }

        private enum ActionType
        {
            PLAYOST,
            PLAYSOUND,
            STOP,
            PAUSE,
            UNPAUSE,
            BRUTETRANSION,
            FADETRANSITION,
            ADAPTATIVETRANSITION,
            ADDADAPTATIVE,
            REMOVEADAPTATIVE,
            CLEARALLADAPTATIVE

        }

        [SerializeField]
        private string musicName;
        [SerializeField]
        private string _tagName;
        
        [SerializeField]
        private CollideType collideType;
        [SerializeField]
        private ActionType actionType;
        

        #region Trigger
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if(collideType != CollideType.ENTERTRIGGER2D)return;
            if(!collider.CompareTag(_tagName))return;
            AudioAct();
        }

        private void OnTriggerEnter(Collider collider)
        {
            if(collideType != CollideType.ENTERTRIGGER3D)return; 
            if(!collider.CompareTag(_tagName))return;
            AudioAct();
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if(collideType != CollideType.EXITTRIGGER2D)return; 
            if(!collider.CompareTag(_tagName))return;
            AudioAct();
        }

        private void OnTriggerExit(Collider collider)
        {
            if(collideType != CollideType.EXITTRIGGER3D)return; 
            if(!collider.CompareTag(_tagName))return;
            AudioAct();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if(collideType != CollideType.ENTERCOLLISION2D)return;
            if(collision.gameObject.CompareTag(_tagName))return;
            AudioAct();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(collideType != CollideType.ENTERCOLLISION3D)return;
            if(collision.gameObject.CompareTag(_tagName))return;
            AudioAct();
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if(collideType != CollideType.EXITCOLLISION2D)return;
            if(collision.gameObject.CompareTag(_tagName))return;
            AudioAct();
        }

        private void OnCollisionExit(Collision collision)
        {
            if(collideType != CollideType.EXITCOLLISION3D)return;
            if(collision.gameObject.CompareTag(_tagName))return;
            AudioAct();
        }

        public void OnButtonPerformed()
        {
            if(collideType != CollideType.UIBUTTON)return;
            AudioAct();
        }


        #endregion

        #region Action
        private void AudioAct()
        {
            switch(actionType)
            {
                case ActionType.PLAYOST :
                    AudioManager.instance.PlayClipAt(AudioManager.instance.allAudio[musicName], this.transform.position, AudioManager.instance.ostMixer, false, true);
                break;

                case ActionType.PLAYSOUND :
                    AudioManager.instance.PlayClipAt(AudioManager.instance.allAudio[musicName], this.transform.position, AudioManager.instance.soundEffectMixer, true, false);
                break;

                case ActionType.STOP :
                    AudioManager.instance.RemoveSound(musicName);
                break;

                case ActionType.PAUSE :
                    AudioManager.instance.PauseSound(musicName);
                break;

                case ActionType.UNPAUSE :
                    AudioManager.instance.UnPauseSound(musicName);
                break;

                case ActionType.BRUTETRANSION :
                    AudioManager.instance.ChangeMusic(musicName, AudioManager.instance.ostMixer, AudioManager.TransitionType.BRUTE);
                break;

                case ActionType.FADETRANSITION :
                    AudioManager.instance.ChangeMusic(musicName, AudioManager.instance.ostMixer, AudioManager.TransitionType.FADE);
                break;

                case ActionType.ADAPTATIVETRANSITION :
                    AudioManager.instance.ChangeMusic(musicName, AudioManager.instance.ostMixer, AudioManager.TransitionType.ADAPTATIVE);
                break;
            }
        }
        #endregion
    }

}
