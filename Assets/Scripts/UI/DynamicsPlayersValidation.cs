using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class DynamicsPlayersValidation : MonoBehaviour
{
    [Header("Metrics")]
    [SerializeField]
    private UnityEvent _eventToTrigger;
    [SerializeField]
    private bool _simpleValidate = true;

    [Header("Debug")]
    [SerializeField]
    private List<DynamicsButtonValidation> _currentValidateButtons;

    [SerializeField]
    private GameObject _buttonPrefab;
    [SerializeField]
    private int _nbValidatedButton = 0;
    public bool SimpleValidate => _simpleValidate;

    private void OnEnable()
    {
        SetupValidateButton();
    }

    protected virtual void SetupValidateButton()
    {
        GameManager.Instance.UIManager.CurrentPlayerValidation = this;
        _currentValidateButtons.Clear();
        for (int i = 0; i < GameManager.Instance.Players.Length; i++)
        {
            GameObject button = Instantiate(_buttonPrefab, this.transform);
            DynamicsButtonValidation validationButton = button.GetComponent<DynamicsButtonValidation>();
            validationButton.Image.color = GameManager.Instance.PlayerColor[i];
            _currentValidateButtons.Add(validationButton);
        }
    }

    //Validate button for player
    //if all players validated, go to next tutorial page
    public void DynamicValidatePage(PlayerEnum playerID)
    {
        int id = (int)playerID;
        if (_currentValidateButtons.Count <= 0 || id > _currentValidateButtons.Count || id <= 0) return;
        if (_currentValidateButtons[id - 1].IsValidated) return;
        for (int i = 0; i < _currentValidateButtons.Count; i++)
        {
            if (i != id - 1) continue;
            _currentValidateButtons[i].ValidateButton();
        }
        //SFX de bouton valide

        _nbValidatedButton++;
        if (_nbValidatedButton >= _currentValidateButtons.Count)
        {
            _nbValidatedButton = 0;
            //execute the assigned event when all button were validated
            _eventToTrigger.Invoke();
        }
    }
}
