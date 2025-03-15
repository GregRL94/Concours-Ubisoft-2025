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
    public void CreateListOfMuseumArtefactsUI(Dictionary<MuseumObjects.ObjectType, MuseumObjects[]> museumArtefactsDict)
    {
        // Empty Parent Childs if not empty
        foreach (Transform child in Parent)
        {
            Destroy(child.gameObject);
        }

        // Extract all keys found in the dictionnary and showcase it in UI List
        foreach (var kvp in museumArtefactsDict)
        {
            GameObject newObject = Instantiate(artefactText, Parent); 
            newObject.name = kvp.Key.ToString(); 
            artefactNameCollection.Add(newObject);
            TextMeshProUGUI textComponent = newObject.GetComponent<TextMeshProUGUI>();
            if (textComponent != null && kvp.Value.Length > 0)
            {
                textComponent.text = $"- {kvp.Key} {kvp.Value.Length}X";
            }
        }
    }

    public void UpdateListOfMuseumArtefacts(Dictionary<MuseumObjects.ObjectType, MuseumObjects[]> museumArtefactsDict)
    {
        foreach (var artefactObj in artefactNameCollection) // Looping through all the gameobject artefacts created
        {
            string artefactName = artefactObj.name;

            // Verify if artifact name exist in the enum category
            if (Enum.TryParse(artefactName, out MuseumObjects.ObjectType artefactType)) 
            {
                int amount = 0; // Consider that amount = 0 for each artefact name

                // Verify if key exist and get -> MuseumObjectsTest[] (ex: PAINTING, 3 Painting objects in MuseumObjectsTest[])
                if (museumArtefactsDict.TryGetValue(artefactType, out MuseumObjects[] artefacts)) 
                {
                    amount = artefacts.Length; // amount of MuseumObjects for each key
                }

                TextMeshProUGUI textComponent = artefactObj.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {

                    if (amount == 0)
                    {
                        textComponent.text = $"- {artefactType}";
                        artefactObj.GetComponentInChildren<Image>().enabled = true; // Red line cross
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
        // Todo: Debug - Thomas Update Thief Bar UI after falling into a trap
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
