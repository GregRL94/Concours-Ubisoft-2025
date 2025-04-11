using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VsyncToggle : MonoBehaviour
{
    [SerializeField]
    private Toggle _vsyncToggle;
    void OnEnable()
    {
        if(_vsyncToggle == null)_vsyncToggle = GetComponent<Toggle>();
        _vsyncToggle.isOn = QualityManager.instance.IsVsyncOn;
    }

    public void SetVsyncToggle()
    {
        QualityManager.instance.SetVsync(_vsyncToggle.isOn);
    }
}
