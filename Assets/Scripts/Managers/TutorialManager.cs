using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("Metrics")]
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
    public bool IsTutorialCompleted => _isTutorialCompleted;

    // Start is called before the first frame update
    void Start()
    {
        AutoGetTutorialPages();
        if (!_isTutorialCompleted)StartTutorial();
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

    public void PreviousTutorialPage() => GoToPage(_currentTutorialPageIndex - 1);

    public void NextTutorialPage() => GoToPage(_currentTutorialPageIndex + 1);

    public void SkipTutorial() => CompleteTutorial();
    private void GoToPage(int index)
    {
        if (_currentTutorialPageIndex < 0) return;

        if(index > _currentTutorialPageIndex)
            _tutorialPages[index - 1].SetActive(false);
        else if(index < _currentTutorialPageIndex)
            _tutorialPages[index + 1].SetActive(false);

        if (_currentTutorialPageIndex >= _tutorialPages.Count)
        {
            CompleteTutorial();
            return;
        }
        _tutorialPages[index].SetActive(true);
        _currentTutorialPage = _tutorialPages[index];
        _currentTutorialPageIndex = index;
    }

    private void CompleteTutorial()
    {

        _tutorialUI.SetActive(false);
        _isTutorialCompleted = true;
    }
}
