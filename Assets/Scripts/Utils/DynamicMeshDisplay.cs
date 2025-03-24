using UnityEngine;

public class DynamicMeshDisplay : MonoBehaviour
{
    MeshRenderer _meshRenderer;

    void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public void OnGetVision(bool show)
    {
        if (gameObject.CompareTag("ENEMY"))
        {
            if (show)
            {
                _meshRenderer.enabled = true;
                return;
            }
            _meshRenderer.enabled = false;
            return;
        }

        if (gameObject.CompareTag("MUSEUMOBJECT"))
        {
            MuseumObjects museumObject = gameObject.GetComponent<MuseumObjects>();
            if (museumObject.IsStolen)
            {
                gameObject.SetActive(false);
                return;
            }
        }
    }
}
