using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoading : MonoBehaviour
{
    [Header("Metrics")]
    [SerializeField]
    private List<SceneAsset> _scenesList;

    [Header("Debug Reading")]
    [SerializeField]
    private int _nextSceneIndex = 0;
    [SerializeField]
    private bool _isTutoCompleted = false;
    public bool IsTutoCompleted { get { return _isTutoCompleted; } set {  _isTutoCompleted = value; } }

    public static SceneLoading Instance;
    private void Awake()
    {
        if (Instance != null) 
            Destroy(this.gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    //Load the next scene in scene list, return to 0 when end of list reached
    public void LoadNextScene()
    {
        LoadScene(_scenesList[_nextSceneIndex]);
        _nextSceneIndex++;
        if(_nextSceneIndex >= _scenesList.Count)_nextSceneIndex = 0;
    }

    //load any scene in build settings
    public void LoadScene(SceneAsset scene) => SceneManager.LoadScene(scene.name);
}
