using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Museum Actefacts Checklist UI")]
    public Transform Parent;
    public GameObject artefactText;
    public List<GameObject> artefactNameCollection = new List<GameObject>();

    [Header("Capture Thief UI")]
    public TextMeshProUGUI captureThiefText;
    public float captureGaugeFillTime;
    public Image fillCaptureGaugeImage;
    public Image fillCaptureGaugeIconImage;
    [HideInInspector] public Coroutine captureThiefRoutine;
    //todo: Debug - Thomas enleve et met tes var de reputation
    int currentCaptureThiefAmount;// enleve thomas
    const int maxCaptureThiefAmount = 100; // enleve thomas

    void Start() { captureThiefText.text = currentCaptureThiefAmount + "/" + maxCaptureThiefAmount; }

    #region Update Museum Actefacts Checklist UI 
    public void CreateListOfMuseumArtefactsUI(Dictionary<MuseumObjectsTest.ObjectType, MuseumObjectsTest[]> museumArtefactsDict)
    {
        foreach (Transform child in Parent) // Supprime les anciens objets pour éviter les doublons
        {
            Destroy(child.gameObject);
        }

        foreach (var kvp in museumArtefactsDict)
        {
            GameObject newObject = Instantiate(artefactText, Parent); // Instancie l'UI sous Parent directement
            newObject.name = kvp.Key.ToString(); // Change le nom du GameObject
            artefactNameCollection.Add(newObject);
            // Met à jour le texte affiché
            TextMeshProUGUI textComponent = newObject.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = $"- {kvp.Key} {kvp.Value.Length}X";
            }
        }
    }

    //public void UpdateListOfMuseumArtefacts(Dictionary<MuseumObjectsTest.ObjectType, MuseumObjectsTest[]> museumArtefactsDict)
    //{
    //    foreach (var artefactObj in artefactNameCollection) // Parcourir tous les GameObjects créés
    //    {
    //        string artefactName = artefactObj.name; // Nom du GameObject = clé Enum

    //        if (Enum.TryParse(artefactName, out MuseumObjectsTest.ObjectType artefactType)) // Vérifier si le nom est une clé valide
    //        {
    //            if (museumArtefactsDict.TryGetValue(artefactType, out MuseumObjectsTest[] artefacts)) // Vérifier si la clé existe dans le dictionnaire
    //            {
    //                int amount = artefacts.Length; // Nombre d'objets
    //                TextMeshProUGUI textComponent = artefactObj.GetComponent<TextMeshProUGUI>();

    //                if(amount == 0)
    //                {
    //                    if (textComponent != null)
    //                    {
    //                        textComponent.text = $"{artefactType} 0X"; // Mettre à jour le texte
    //                    }
    //                }
    //                else
    //                {
    //                    if (textComponent != null)
    //                    {
    //                        textComponent.text = $"{artefactType} {amount}X"; // Mettre à jour le texte
    //                    }
    //                }

    //            }
    //        }
    //    }
    //}
    public void UpdateListOfMuseumArtefacts(Dictionary<MuseumObjectsTest.ObjectType, MuseumObjectsTest[]> museumArtefactsDict)
    {
        foreach (var artefactObj in artefactNameCollection) // Parcourir tous les GameObjects créés
        {
            string artefactName = artefactObj.name; // Nom du GameObject = clé Enum

            if (Enum.TryParse(artefactName, out MuseumObjectsTest.ObjectType artefactType)) // Vérifier si le nom est une clé valide
            {
                int amount = 0; // Par défaut, considérer qu'il n'y a aucun objet

                if (museumArtefactsDict.TryGetValue(artefactType, out MuseumObjectsTest[] artefacts)) // Vérifier si la clé existe
                {
                    amount = artefacts.Length; // Nombre réel d'objets
                }

                TextMeshProUGUI textComponent = artefactObj.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {

                    if (amount == 0)
                    {
                        textComponent.text = $"- {artefactType}";
                        artefactObj.GetComponentInChildren<Image>().enabled = true; // Rayure rouge
                    }
                    else
                    {
                        textComponent.text = $"- {artefactType} {amount}X"; 
                    }
                }
            }
        }
    }
    #endregion
    
    #region Update Capture Thief UI
    void Update()
    {
        // Todo: Debug-> Update Thief Bar UI after falling into a trap
        if (Input.GetKeyDown(KeyCode.Z))
        {
            int randNumber = UnityEngine.Random.Range(1,16);
            UpdateCaptureThiefGauge(randNumber);
        }
    }

    public void UpdateCaptureThiefGauge(int amount)
    {
        float previousAmount = currentCaptureThiefAmount;
        currentCaptureThiefAmount = Mathf.Clamp(currentCaptureThiefAmount + amount, 0, maxCaptureThiefAmount);

        StartCoroutine(UpdateCaptureThiefUI(previousAmount,currentCaptureThiefAmount));

        if(currentCaptureThiefAmount >= maxCaptureThiefAmount)
        {
            //Todo: Game Manager Show UI Result  
            // Invoke();
            print("Game Finish \n Show the Score Board P1 && P2");
        }
    }

    IEnumerator UpdateCaptureThiefUI(float startAmount,float targetAmount)
    {
        captureThiefText.text = currentCaptureThiefAmount + "/" + maxCaptureThiefAmount;

        float elapsedTime = 0f;
        float duration = 0.5f;

        // Bar Fill
        float startFill = fillCaptureGaugeImage.fillAmount; // last amount filled
        float targetFill = targetAmount / maxCaptureThiefAmount;
        
        // Icon Fill
        float startIconFill = fillCaptureGaugeIconImage.fillAmount; // last amount filled
        float targetIconFill = targetAmount / maxCaptureThiefAmount;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            fillCaptureGaugeImage.fillAmount = Mathf.Lerp(startFill, targetFill, elapsedTime / duration);
            fillCaptureGaugeIconImage.fillAmount = Mathf.Lerp(startIconFill, targetIconFill, elapsedTime / duration);
            yield return null;
        }

        fillCaptureGaugeImage.fillAmount = targetFill;
        fillCaptureGaugeIconImage.fillAmount = targetFill;
    }
    #endregion

}
