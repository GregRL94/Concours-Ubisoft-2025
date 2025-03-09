using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MuseumObjectsManagerTest : MonoBehaviour
{
    [SerializeField]
    private MuseumObjectsTest[] _allMuseumObjectsTest;

    [SerializeField]
    private Dictionary<MuseumObjectsTest.ObjectType, MuseumObjectsTest[]> _sortedObjects = new Dictionary<MuseumObjectsTest.ObjectType, MuseumObjectsTest[]>();

    public static MuseumObjectsManagerTest Instance;

    public MuseumObjectsTest[] AllMuseumObjectsTest => _allMuseumObjectsTest;

    public KeyCode stealInput;

    public UIManager uiManager;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    void Start()
    {
        //we get all museum objects
        _allMuseumObjectsTest = FindObjectsOfType<MuseumObjectsTest>();

        //We sort all objects
        //and we assign them to dictionnary
        var objectTypes = Enum.GetValues(typeof(MuseumObjectsTest.ObjectType));
        foreach (MuseumObjectsTest.ObjectType objectType in objectTypes)
        {
            List<MuseumObjectsTest> MuseumObjectsTest = new List<MuseumObjectsTest>();
            for(int j = 0; j < _allMuseumObjectsTest.Length; j++)
            {
                if (_allMuseumObjectsTest[j].MuseumObjectType != objectType)
                    continue;
                MuseumObjectsTest.Add(_allMuseumObjectsTest[j]);
            }
            MuseumObjectsTest[] MuseumObjectsTestArray = MuseumObjectsTest.ToArray();
            MuseumObjectsTestArray.ToList();
            _sortedObjects.Add(objectType, MuseumObjectsTestArray);
        }

        uiManager.CreateListOfMuseumArtefactsUI(_sortedObjects);
    }

    void Update()
    {

        // todo: Debug for thief stealing artefacts (Randomizer)
        if (Input.GetKeyDown(stealInput))
        {
            StealRandomArtifact();
        }
    }

    void StealRandomArtifact()
    {
        if (_sortedObjects.Count == 0)
        {
            Debug.Log("Zero artefact to steal !");
            return;
        }

        // Sélectionner une clé (catégorie) aléatoire A CHANGER POUR THOMAS!!!S
        var randomKey = _sortedObjects.Keys.ElementAt(UnityEngine.Random.Range(0, _sortedObjects.Count));

        // Récupérer la liste des objets de cette catégorie
        var objectList = _sortedObjects[randomKey].ToList();

        if (objectList.Count == 0)
        {
            Debug.Log($"Plus d'artefacts à voler dans la catégorie {randomKey}.");

            return;
        }

        // Sélectionner un objet aléatoire dans cette catégorie
        int randomIndex = UnityEngine.Random.Range(0, objectList.Count);
        MuseumObjectsTest stolenObject = objectList[randomIndex];

        // Retirer cet objet de la liste
        objectList.RemoveAt(randomIndex);

        // Mettre à jour le dictionnaire avec la nouvelle liste
        if (objectList.Count > 0)
        {
            _sortedObjects[randomKey] = objectList.ToArray();
        }
        else
        {
            // Si la catégorie devient vide, on la supprime
            _sortedObjects.Remove(randomKey);
        }

        // Afficher le résultat dans la console
        Debug.Log($"Vol réussi ! Objet volé : {stolenObject.name} de la catégorie {randomKey}.");

        uiManager.UpdateListOfMuseumArtefacts(_sortedObjects);
    }



    public MuseumObjectsTest[] GetObjectList(MuseumObjectsTest.ObjectType objectType)
    {
        return _sortedObjects.GetValueOrDefault(objectType);
    }
}
