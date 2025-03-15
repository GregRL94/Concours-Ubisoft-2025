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

    public KeyCode stealInput;

    public UIManager uiManager;


    void Start()
    {
        //we get all museum objects
        _allMuseumObjects = FindObjectsOfType<MuseumObjects>();

        //We sort all objects
        //and we assign them to dictionnary
        var objectTypes = Enum.GetValues(typeof(MuseumObjects.ObjectType));
        foreach (MuseumObjects.ObjectType objectType in objectTypes)
        {
            List<MuseumObjects> MuseumObjects = new List<MuseumObjects>();
            for (int j = 0; j < _allMuseumObjects.Length; j++)
            {
                if (_allMuseumObjects[j].MuseumObjectType != objectType)
                    continue;
                MuseumObjects.Add(_allMuseumObjects[j]);
            }
            MuseumObjects[] MuseumObjectsArray = MuseumObjects.ToArray();
            MuseumObjectsArray.ToList();
            _sortedObjects.Add(objectType, MuseumObjectsArray);
        }

        uiManager.CreateListOfMuseumArtefactsUI(_sortedObjects);
    }

    void Update()
    {
        // todo: Debug - Thomas for thief stealing artefacts (Randomizer)
        if (Input.GetKeyDown(stealInput))
        {
            CheckArtefactStolen();
        }
    }

    public void CheckArtefactStolen()
    {
        if (_sortedObjects.Count == 0)
        {
            Debug.Log("Zero artefact to steal !");
            return;
        }

        // Select a random key in dict -  A CHANGER POUR THOMAS (peut passer un type a laide d'un de tes AI script)!!!
        MuseumObjects.ObjectType randomKey = _sortedObjects.Keys.ElementAt(UnityEngine.Random.Range(0, _sortedObjects.Count));

        // Get objects[] of this category key
        List<MuseumObjects> objectList = _sortedObjects[randomKey].ToList();

        if (objectList.Count == 0)
        {
            Debug.Log($"No more artefact to steal in category (key) {randomKey}.");

            return;
        }

        // Select a random index in the List 
        int randomIndex = UnityEngine.Random.Range(0, objectList.Count);
        MuseumObjects stolenObject = objectList[randomIndex];

        // Remove oject to the List
        objectList.RemoveAt(randomIndex);

        // Update dictionary with List 
        if (objectList.Count > 0)
        {
            _sortedObjects[randomKey] = objectList.ToArray();
        }
        else
        {
            // If List empty, delete key
            _sortedObjects.Remove(randomKey);
        }

        Debug.Log($"Successfully ! Object Stolen : {stolenObject.name} in the  {randomKey} category.");

        // Update UI List of Museum Artefacts
        uiManager.UpdateListOfMuseumArtefacts(_sortedObjects);
    }



    public MuseumObjects[] GetObjectList(MuseumObjects.ObjectType objectType)
    {
        return _sortedObjects.GetValueOrDefault(objectType);
    }
}
