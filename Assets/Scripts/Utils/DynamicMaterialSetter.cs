using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicMaterialSetter : MonoBehaviour
{
    [SerializeField] private Material _seeThroughMat;
    [SerializeField] private List<Renderer> _renderers;

    List<Material> _defaultMaterialsList;

    void Awake()
    {
        if (_renderers.Count > 0)
        {
            _defaultMaterialsList = new List<Material>();
            foreach (Renderer renderer in _renderers)
            {
                _defaultMaterialsList.Add(renderer.material);
            }
        }
    }

    public void SetSeeThrough(bool seeThrough)
    {
        if (seeThrough)
        {
            for (int i = 0;  i < _renderers.Count; i++)
            {
                _renderers[i].material = _seeThroughMat;
            }
            return;
        }

        for (int i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].material = _defaultMaterialsList[i];
        }
    }
}
