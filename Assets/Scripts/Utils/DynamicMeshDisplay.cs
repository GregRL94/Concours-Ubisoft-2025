using UnityEngine;

public class DynamicMeshDisplay : MonoBehaviour
{
    MeshRenderer _meshRenderer;

    void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public void ShowMesh(bool show)
    {
        if (show) { _meshRenderer.enabled = true; return; }
        _meshRenderer.enabled = false;
    }
}
