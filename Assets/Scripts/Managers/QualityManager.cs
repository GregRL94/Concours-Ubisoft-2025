using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class QualityManager : MonoBehaviour
{
    [SerializeField]
    private bool _isVsyncOn;
    public bool IsVsyncOn => _isVsyncOn;

    public static QualityManager instance;
    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        
    }

    public void SetVsync(Toggle toggle)
    {
        _isVsyncOn = toggle.isOn;
        QualitySettings.vSyncCount = _isVsyncOn ? 1 : 0;
    }

    public void SetVsync(bool vsync)
    {
        _isVsyncOn = vsync;
        QualitySettings.vSyncCount = vsync ? 1 : 0;
    }
}
