using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLayout : MonoBehaviour
{
    [SerializeField]
    private UITutorialStep _tutorialType;
    [SerializeField]
    private GameObject _buttonPrefab;
    private void OnEnable()
    {

        GameManager.Instance.TutorialManager.ValidateButtons.Clear();
        GameManager.Instance.TutorialManager.CurrentTutorialType = _tutorialType;
        for (int i = 0; i < GameManager.Instance.Players.Length; i++)
        {
            GameObject button = Instantiate(_buttonPrefab, this.transform);
            TutorialGeneratedButton tutorialButton = button.GetComponent<TutorialGeneratedButton>();
            tutorialButton.Image.color = GameManager.Instance.TutorialManager.PlayerColor[i];
        }
    }
    
}
