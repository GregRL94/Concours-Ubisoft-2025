using System.Collections.Generic;
using UnityEngine;

public class DynamicMeshDisplay : MonoBehaviour
{
    List<MeshRenderer> _meshes = new List<MeshRenderer>();

    void Start()
    {
        foreach (Transform child in GetFirstLevelChidren(transform))
        {
            if(child.GetComponent<MeshRenderer>() != null)
            {
                _meshes.Add(child.GetComponent<MeshRenderer>());
            }
        }
    }

    public void OnGetVision(bool show)
    {
        if (gameObject.CompareTag("ENEMY"))
        {
            for (int i = 0; i < _meshes.Count; i++)
            {
                _meshes[i].enabled = show;
            }
        }

        if (gameObject.CompareTag("MUSEUMOBJECT"))
        {
            MuseumObjects museumObject = gameObject.GetComponent<MuseumObjects>();
            if (museumObject.IsStolen)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private List<Transform> GetFirstLevelChidren(Transform parent)
    {
        List<Transform> listOfChildren = new List<Transform>();

        foreach (Transform child in parent)
        {
            listOfChildren.Add(child);
        }

        return listOfChildren;
    }
}
