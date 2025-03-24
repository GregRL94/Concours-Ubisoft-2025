using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialGeneratedButton : MonoBehaviour
{
    [SerializeField]
    private bool _isValidated;
    [SerializeField]
    private Sprite _validateSprite;
    [SerializeField]
    private Image _image;
    public Image Image => _image;
    public bool IsValidated => _isValidated;
    private void Start() => GameManager.Instance.TutorialManager.ValidateButtons.Add(this);
    public void ValidateButton()
    {
        _image.sprite = _validateSprite;
        _isValidated = true;
    }

}
