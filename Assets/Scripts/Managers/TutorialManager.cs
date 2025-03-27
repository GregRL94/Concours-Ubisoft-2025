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
    [SerializeField]
    private List<TutorialGeneratedButton> _validateButtons;
    public List<TutorialGeneratedButton> ValidateButtons => _validateButtons;
    [SerializeField]
    private int _nbValidatedButton = 0;
    [SerializeField]
    private Color[] _playerColor;
    public Color[] PlayerColor => _playerColor;
    public bool IsTutorialCompleted => _isTutorialCompleted;
    public UITutorialStep CurrentTutorialType { get { return _currentTutorialType; } set { _currentTutorialType = value; } }

    // Start is called before the first frame update
    void Start()
    {
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
        _validateButtons.Clear();
    }

    private void CompleteTutorial()
    {

        _tutorialUI.SetActive(false);
        _isTutorialCompleted = true;
        GameManager.Instance.StartGameLoop();
        SceneLoading.Instance.IsTutoCompleted = _isTutorialCompleted;
    }

    //Validate button for player
    //if all players validated, go to next tutorial page
    public void DynamicValidatePage(PlayerEnum playerID)
    {
        int id = (int)playerID;
        if (id > _validateButtons.Count || id <= 0) return;
        if (_validateButtons[id - 1].IsValidated) return;
        for (int i = 0; i < _validateButtons.Count; i++)
        {
            if (i != id - 1) continue;
            _validateButtons[i].ValidateButton();
        }
        //SFX de bouton valide

        _nbValidatedButton++;
        if(_nbValidatedButton >= _validateButtons.Count)
        {
            _nbValidatedButton = 0;
            NextTutorialPage();
        }
    }
}
