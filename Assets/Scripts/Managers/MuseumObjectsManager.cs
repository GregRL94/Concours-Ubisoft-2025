using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MuseumObjectsManager : MonoBehaviour
{
    [SerializeField]
    private List<MuseumObjects> _allMuseumObjects;

    [SerializeField]
    private Dictionary<ObjectType, MuseumObjects[]> _sortedObjects = new Dictionary<ObjectType, MuseumObjects[]>();

    public List<MuseumObjects> AllMuseumObjects => _allMuseumObjects;


    public UIManager uiManager;

    void Start()
    {
        //we get all museum objects
        _allMuseumObjects = FindObjectsOfType<MuseumObjects>().ToList();

        //We sort all objects
        //and we assign them to dictionnary
        var objectTypes = Enum.GetValues(typeof(ObjectType));
        foreach (ObjectType objectType in objectTypes)
        {
            List<MuseumObjects> MuseumObjects = new List<MuseumObjects>();
            for (int j = 0; j < _allMuseumObjects.Count; j++)
            {
                if (_allMuseumObjects[j].MuseumObjectType != objectType)
                    continue;
                MuseumObjects.Add(_allMuseumObjects[j]);
            }
            MuseumObjects[] MuseumObjectsArray = MuseumObjects.ToArray();
            MuseumObjectsArray.ToList();
            _sortedObjects.Add(objectType, MuseumObjectsArray);
        }

    }

    public void CheckArtefactStolen(MuseumObjects objectStolen)
    {
        if (_sortedObjects.Count == 0)
        {
            Debug.Log("Zero artefact to steal !");
            return;
        }

        List<MuseumObjects> objectList = _sortedObjects[objectStolen.MuseumObjectType].ToList();

        if (objectList.Count == 0)
        {
            Debug.Log($"No more artefact to steal in category (key) {objectStolen.MuseumObjectType}.");

            return;
        }

        // Remove object to the List
        objectList.Remove(objectStolen);
        _allMuseumObjects.Remove(objectStolen);
        // Update dictionary with List 
        if (objectList.Count > 0)
        {
            _sortedObjects[objectStolen.MuseumObjectType] = objectList.ToArray();
        }
        else
        {
            // If List empty, delete key
            _sortedObjects.Remove(objectStolen.MuseumObjectType);
        }

        Debug.Log($"Successfully ! Object Stolen : {objectStolen.name} in the  {objectStolen.MuseumObjectType} category.");
        UIManager.Instance.alreadyAssignedType[objectStolen.MuseumObjectType]--;
        // Update UI List of Museum Artefacts
        uiManager.UpdateListOfMuseumArtefacts();
    }



    public MuseumObjects[] GetObjectList(ObjectType objectType)
    {
        return _sortedObjects.GetValueOrDefault(objectType);
    }

    public int CountAllMuseumObjects()
    {
        int totalCount = 0;
        foreach (var kvp in _sortedObjects)
        {
            totalCount += kvp.Value.Length;
        }

        return totalCount;
    }

}
