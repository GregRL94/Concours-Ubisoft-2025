using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLayout : DynamicsPlayersValidation
{
    [SerializeField]
    private UITutorialStep _tutorialType;
    protected override void SetupValidateButton()
    {
        GameManager.Instance.TutorialManager.CurrentTutorialType = _tutorialType;
        base.SetupValidateButton();
    }
    
}
