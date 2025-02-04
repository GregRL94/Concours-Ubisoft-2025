using LevelManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndScreen : Menu<EndScreen>
{

    [SerializeField]
    private float _playDelay = 1f;


    [SerializeField]
    private TransitionFader endTransitionPrefab;

    private IEnumerator OnPlayPressedRoutine()
    {
        TransitionFader.PlayTransition(endTransitionPrefab);
        LevelLoader.LoadStartMenu();
        yield return new WaitForSeconds(_playDelay);
        gameObject.SetActive(false);
    }

    public void OnStartMenuPressed()
    {
        StartCoroutine(OnPlayPressedRoutine());
    }

    public void OnQuitPressed()
    {
        Application.Quit();
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false; // Exit option for editor 
        #endif
    }
}
