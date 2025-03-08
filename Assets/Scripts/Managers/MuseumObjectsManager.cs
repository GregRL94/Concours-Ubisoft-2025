using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MuseumObjectsManager : MonoBehaviour
{
    [SerializeField]
    private MuseumObjects[] _allMuseumObjects;

    [SerializeField]
    private Dictionary<MuseumObjects.ObjectType, MuseumObjects[]> _sortedObjects = new Dictionary<MuseumObjects.ObjectType, MuseumObjects[]>();

    public MuseumObjects[] AllMuseumObjects => _allMuseumObjects;

    // Start is called before the first frame update
    void Start()
    {
        //we get all museum objects
        _allMuseumObjects = FindObjectsOfType<MuseumObjects>();

        //We sort all objects
        //and we assign them to dictionnary
        var objectTypes = Enum.GetValues(typeof(MuseumObjects.ObjectType));
        foreach (MuseumObjects.ObjectType objectType in objectTypes)
        {
            //Debug.Log(objectType);
            List<MuseumObjects> museumObjects = new List<MuseumObjects>();
            for(int j = 0; j < _allMuseumObjects.Length; j++)
            {
                if (_allMuseumObjects[j].MuseumObjectType != objectType)
                    continue;
                /*{
                    Debug.Log($"not {objectType} in index {j}, continue");
                    continue;
                }*/
                
                museumObjects.Add(_allMuseumObjects[j]);
            }
            MuseumObjects[] museumObjectsArray = museumObjects.ToArray();
            museumObjectsArray.ToList();
            _sortedObjects.Add(objectType, museumObjectsArray);
            /*Debug.Log($"object type : {objectType} and list : {_sortedObjects.GetValueOrDefault(objectType)}");
            Debug.Log($"all object in list : ");
            foreach (MuseumObjects yes in _sortedObjects.GetValueOrDefault(objectType))
            {
                Debug.Log(yes.gameObject.name);
            }
            */
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public MuseumObjects[] GetObjectList(MuseumObjects.ObjectType objectType)
    {
        return _sortedObjects.GetValueOrDefault(objectType);
    }
}
