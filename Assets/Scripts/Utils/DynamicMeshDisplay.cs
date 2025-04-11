using System.Collections.Generic;
using UnityEngine;

public class DynamicMeshDisplay : MonoBehaviour
{
    List<MeshRenderer> _meshes = new List<MeshRenderer>();
    List<SkinnedMeshRenderer> _skinnedMeshes = new List<SkinnedMeshRenderer>();

    void Start()
    {
        foreach (Transform child in GetFirstLevelChidren(transform))
        {
            foreach (Transform child2 in GetFirstLevelChidren(child))
            {
                if (child2.GetComponent<MeshRenderer>() != null)
                {
                    _meshes.Add(child2.GetComponent<MeshRenderer>());
                }
                if (child2.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    _skinnedMeshes.Add(child2.GetComponent<SkinnedMeshRenderer>());
                }
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
            for (int i = 0; i <  _skinnedMeshes.Count; i++)
            {
                _skinnedMeshes[i].enabled = show;
            }
        }

        if (gameObject.CompareTag("MUSEUMOBJECT"))
        {
            MuseumObjects museumObject = gameObject.GetComponent<MuseumObjects>();
            if (museumObject.IsStolen)
            {
                Debug.Log(gameObject.name + "set inactive");
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
