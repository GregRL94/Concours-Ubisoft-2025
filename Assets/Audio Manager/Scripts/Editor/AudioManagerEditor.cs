using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AkuroTools
{
    #if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AudioManager))]
    public class AudioManagerEditor : Editor
    {
        private AudioManager _source;
        private GUIStyle _titleStyle;

        SerializedProperty maxFadeInTime;
        SerializedProperty maxFadeOutTime;
        SerializedProperty audioList;


        SerializedProperty soundEffectMixer;
        SerializedProperty musicMixer;
        SerializedProperty adaptativeMusicMixer;

        private void OnEnable()
        {
            _source = target as AudioManager;


            maxFadeInTime = serializedObject.FindProperty("maxFadeInTime");
            maxFadeOutTime = serializedObject.FindProperty("maxFadeOutTime");
            audioList = serializedObject.FindProperty("audioList");

            soundEffectMixer = serializedObject.FindProperty("soundEffectMixer");
            musicMixer = serializedObject.FindProperty("ostMixer");
            adaptativeMusicMixer = serializedObject.FindProperty("adaptativeOSTMixer");

            /*allAudioName.Clear();
            foreach(AudioManager.KeyValue audio in _source.audioList)
            {
                allAudioName.Add(audio.audioName);
            }*/
        }

        #region INSPECTOR
        public override void OnInspectorGUI()
        {
            InitStyle();

            DrawDatabasePanel();
            

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDatabasePanel()
        {
            EditorGUILayout.BeginVertical("box");

            DrawHeader();
            DrawBody();
            DrawFooter();

            EditorGUILayout.EndVertical();

            void DrawHeader()
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Sounds Database", _titleStyle);

                if (GUILayout.Button(new GUIContent("X", "Clear all sounds"), GUILayout.Width(30)))
                {
                    if (EditorUtility.DisplayDialog("Delete all sounds", "Delete all sounds ? \nYou can't redo this action.", "Yes, yes, yes, yes, YES !", "No, no, no, NO !"))
                        _source.audioList.Clear();
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField("Total Sounds : " + _source.audioList.Count.ToString(), _titleStyle);


            }

            void DrawBody()
            {
                SerializedObject so = new SerializedObject(this);
                if (_source.audioList.Count != 0)
                {
                    EditorGUILayout.BeginVertical("box");
                    for (int i = 0; i < _source.audioList.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        _source.audioList[i].audioName = EditorGUILayout.TextField(_source.audioList[i].audioName);
                        var elementProperty = audioList.GetArrayElementAtIndex(i);
                        //SerializedProperty audioClip = serializedObject.FindProperty("audioList");
                        EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative("audio"), new GUIContent());
                        //Object yes = _source.audioList[i].audio;
                        //yes = EditorGUILayout.ObjectField(yes, typeof(AudioClip), false);

                        if (GUILayout.Button(new GUIContent("X", "Remove Sound"), GUILayout.Width(30)))
                        {
                            _source.audioList.RemoveAt(i);
                            break;
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            void DrawFooter()
            {
                if (GUILayout.Button(new GUIContent("Add new sound", "Assigne lui un nom et le son correspondant !")))
                {
                    _source.audioList.Add(null);
                }

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal("box");

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Fade Out Time", _titleStyle);
                EditorGUILayout.PropertyField(maxFadeOutTime, new GUIContent(""));
                EditorGUILayout.EndVertical();


                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Fade In Time", _titleStyle);
                //EditorGUILayout.FloatField(_source.maxFadeInTime);
                EditorGUILayout.PropertyField(maxFadeInTime, new GUIContent());
                EditorGUILayout.EndVertical();


                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.PropertyField(soundEffectMixer, new GUIContent("Sound Effect Mixer"));
                EditorGUILayout.PropertyField(musicMixer, new GUIContent("Music Mixer"));
                EditorGUILayout.PropertyField(adaptativeMusicMixer, new GUIContent("Adaptative Music Mixer"));
                EditorGUILayout.EndVertical();

            }

            

        }



        #endregion


        #region STYLE
        private void InitStyle()
        {
            _titleStyle = GUI.skin.label;
            _titleStyle.alignment = TextAnchor.MiddleCenter;
            _titleStyle.fontStyle = FontStyle.Bold;
        }
        #endregion
    }
    #endif

}