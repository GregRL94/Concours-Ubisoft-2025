using AkuroTools;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [Header("Metrics")]
    [SerializeField]
    private float _timeBeforeChangingTutorialPages = 2f;
    [SerializeField]
    private List<GameObject> _tutorialPages;
    [SerializeField]
    private GameObject _tutorialUI;

    [Header("Debug")]
    [SerializeField]
    private bool _isTutorialCompleted;
    [SerializeField]
    private GameObject _currentTutorialPage;
    [SerializeField]
    private int _currentTutorialPageIndex;
    [SerializeField]
    private UITutorialStep _currentTutorialType;
    
    public bool IsTutorialCompleted => _isTutorialCompleted;
    public UITutorialStep CurrentTutorialType { get { return _currentTutorialType; } set { _currentTutorialType = value; } }

    // Start is called before the first frame update
    void Start()
    {
        AudioManager.instance.PlayClipAt(AudioManager.instance.allAudio["Music Ingame"], this.transform.position, AudioManager.instance.ostMixer, false, true);
        _isTutorialCompleted = SceneLoading.Instance.IsTutoCompleted;
        AutoGetTutorialPages();
        if (!_isTutorialCompleted) StartTutorial();
        else CompleteTutorial();
    }

    private void AutoGetTutorialPages()
    {
        if (_tutorialPages.Count == 0)
        {
            _tutorialPages = new List<GameObject>();
            for (int i = 0; i < _tutorialUI.transform.childCount; i++)
            {
                _tutorialPages.Add(_tutorialUI.transform.GetChild(i).gameObject);
            }
        }
    }

    private void StartTutorial()
    {
        if (_isTutorialCompleted) return;
        _tutorialUI.SetActive(true);
        _tutorialPages[0].SetActive(true);
        _currentTutorialPage = _tutorialPages[0];
        _currentTutorialPageIndex = 0;
    }

    public void PreviousTutorialPage() => StartCoroutine(GoToPage(_currentTutorialPageIndex - 1));

    public void NextTutorialPage() => StartCoroutine(GoToPage(_currentTutorialPageIndex + 1));

    public void SkipTutorial() => CompleteTutorial();
    private IEnumerator GoToPage(int index)
    {
        if (_currentTutorialPageIndex < 0) yield return null;
        yield return new WaitForSeconds(_timeBeforeChangingTutorialPages);
        if(index > _currentTutorialPageIndex)
            _tutorialPages[index - 1].SetActive(false);
        else if(index < _currentTutorialPageIndex)
            _tutorialPages[index + 1].SetActive(false);

        if (index >= _tutorialPages.Count)
        {
            CompleteTutorial();
            yield return null;
        }
        _tutorialPages[index].SetActive(true);
        _currentTutorialPage = _tutorialPages[index];
        _currentTutorialPageIndex = index;
    }

    private void CompleteTutorial()
    {

        _tutorialUI.SetActive(false);
        _isTutorialCompleted = true;
        GameManager.Instance.StartGameLoop();
        SceneLoading.Instance.IsTutoCompleted = _isTutorialCompleted;
    }

    public void NextScene() => SceneLoading.Instance.LoadNextScene();
    public void GoToScene(string sceneName) => SceneManager.LoadScene(sceneName);

}
