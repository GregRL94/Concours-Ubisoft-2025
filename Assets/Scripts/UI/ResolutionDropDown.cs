using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionDropDown : MonoBehaviour
{
    private bool isFullScreen = true;

    // Start is called before the first frame update
    void Start()
    {
        Resolution[] resolutions = Screen.resolutions;
        List<string> options = new List<string>();
        int index = 0;
        foreach(Resolution res in resolutions) {
            options.Add(res.ToString());
            if (Screen.currentResolution.ToString().Equals(res.ToString())) {
                index = options.Count - 1;
            }
        }
        TMP_Dropdown dropdown = GetComponent<TMP_Dropdown>();
        dropdown.AddOptions(options);
        dropdown.value = index;
    }

    
    public void SetResolution(int index) {
        Resolution res = Screen.resolutions[index];
        Screen.SetResolution(res.width, res.height, isFullScreen);
    }


    public void IsFullScreen(bool isFullScreen) {
        this.isFullScreen = isFullScreen;
        Screen.fullScreen = isFullScreen;
    }
}
