using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using Unity.PlasticSCM.Editor.WebApi;
using static GameManager;
using UnityEditor.Rendering;
using System.Linq;

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

    [Header("Player 1 Reputation Points UI")]
    public Transform player1ParentPoints;
    public GameObject player1UIPoint;
    public GameObject[] player1PointsCollection;
    
    public Transform player1ParentExtraPoints;
    public GameObject player1UIExtraPoint;
    public GameObject[] player1ExtraPointsCollection;
    
    [Header("Player 2 Reputation Points UI")]
    public Transform player2ParentPoints;
    public GameObject player2UIPoint;
    public GameObject[] player2PointsCollection;

    public Transform player2ParentExtraPoints;
    public GameObject player2UIExtraPoint;
    public GameObject[] player2ExtraPointsCollection;

    [Header("Metrics")]
    [SerializeField]
    const int maxCaptureThiefAmount = 30;
    [Header("Debug")]
    [SerializeField]
    int currentCaptureThiefAmount;
    public int GetCurrentCaptureThiefAmount => currentCaptureThiefAmount;
    public int GetmaxCaptureThiefAmount => maxCaptureThiefAmount;

    private Coroutine finishUpdateCaptureThiefUIRoutine;

    void Start() { captureThiefText.text = currentCaptureThiefAmount + "/" + maxCaptureThiefAmount; }

    #region Update Museum Actefacts Checklist UI 
    public void CreateListOfMuseumArtefactsUI(Dictionary<ObjectType, MuseumObjects[]> museumArtefactsDict)
    {
        // Empty Parent Childs if not empty
        foreach (Transform child in Parent)
        {
            Destroy(child.gameObject);
        }

        // Extract all keys found in the dictionnary and showcase it in UI List
        foreach (var kvp in museumArtefactsDict)
        {
            if(kvp.Value.Length > 0)
            {
                GameObject newObject = Instantiate(artefactText, Parent); 
                newObject.name = kvp.Key.ToString(); 
                artefactNameCollection.Add(newObject);
                TextMeshProUGUI textComponent = newObject.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = $"- {kvp.Key} {kvp.Value.Length}X";
                }
            }
        }
    }

    public void UpdateListOfMuseumArtefacts(Dictionary<ObjectType, MuseumObjects[]> museumArtefactsDict)
    {
        foreach (var artefactObj in artefactNameCollection) // Looping through all the gameobject artefacts created
        {
            string artefactName = artefactObj.name;

            // Verify if artifact name exist in the enum category
            if (Enum.TryParse(artefactName, out ObjectType artefactType)) 
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
    public void UpdateCaptureThiefGauge(int amount)
    {
        print("####### UpdateCaptureThiefGauge ####### ");
        float previousAmount = currentCaptureThiefAmount;
        currentCaptureThiefAmount = Mathf.Clamp(currentCaptureThiefAmount + amount, 0, maxCaptureThiefAmount);

        // put a couritne so it need to fill before ui 
        //if(finishUpdateCaptureThiefUIRoutine == null)
        //{
            finishUpdateCaptureThiefUIRoutine = StartCoroutine(UpdateCaptureThiefUI(previousAmount,currentCaptureThiefAmount));
        //}

        if(currentCaptureThiefAmount >= maxCaptureThiefAmount)
        {
            //Todo: Game Manager Show UI Result  
            // Invoke();
            print("Game Finish \n Show the Score Board P1 && P2");
        }
        print("####### UpdateCaptureThiefGauge End ####### ");
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

    #region Update Players Reputation UI
    public void CreatePlayersReputationUI(int maxPlayerReputation)
    {
        /////////// P1 
        // Create for both players UI Reputation max
        player1PointsCollection = new GameObject[maxPlayerReputation];

        // Empty Parent Childs if not empty
        foreach (Transform child in player1ParentPoints)
        {
            Destroy(child.gameObject);
        }

        // Extract all keys found in the dictionnary and showcase it in UI List
        for (int i = 0; i < player1PointsCollection.Length; i++)
        {
            GameObject newObject = Instantiate(player1UIPoint, player1ParentPoints);
            player1PointsCollection[i] = newObject;
            print(player1PointsCollection[i].name);
        }

        //////////// CROSS POINTS

        player1ExtraPointsCollection = new GameObject[4];

        foreach (Transform child in player1ParentExtraPoints)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < player1ExtraPointsCollection.Length; i++)
        {
            GameObject newObject = Instantiate(player1UIExtraPoint, player1ParentExtraPoints);
            player1ExtraPointsCollection[i] = newObject;
            print(player1ExtraPointsCollection[i].name);
            player1ExtraPointsCollection[i].SetActive(false);
        }
        
        /////////// P2
        // Create for both players UI Reputation max
        player2PointsCollection = new GameObject[maxPlayerReputation];

        // Empty Parent Childs if not empty
        foreach (Transform child in player2ParentPoints)
        {
            Destroy(child.gameObject);
        }

        // Extract all keys found in the dictionnary and showcase it in UI List
        for (int i = 0; i < player2PointsCollection.Length; i++)
        {
            GameObject newObject = Instantiate(player2UIPoint, player2ParentPoints);
            player2PointsCollection[i] = newObject;
            print(player2PointsCollection[i].name);
        }

        //////////// CROSS POINTS

        player2ExtraPointsCollection = new GameObject[4];

        foreach (Transform child in player2ParentExtraPoints)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < player2ExtraPointsCollection.Length; i++)
        {
            GameObject newObject = Instantiate(player2UIExtraPoint, player2ParentExtraPoints);
            player2ExtraPointsCollection[i] = newObject;
            print(player2ExtraPointsCollection[i].name);
            player2ExtraPointsCollection[i].SetActive(false);
        }

    }

    public void UpdatePlayersReputationUI(int playerIndex, PlayerReputation[] _playersReputation, int maxPlayerReputation)
    {
        print("########### UpdatePlayersReputationUI ########### ");
        print("playerIndex " + playerIndex);
        print("reputationValue " +
            _playersReputation[playerIndex - 1].reputationValue + 
            " isEliminated " + 
            _playersReputation[playerIndex - 1].isEliminated);
        print(" maxPlayerReputation " + maxPlayerReputation);

        // 

        if(playerIndex == 1)
        {
            if(_playersReputation[playerIndex - 1].reputationValue >= 0)
            {
                print("Player 1 UI - VISIBLE BAR");
                int reputationIndexRemove = (maxPlayerReputation - (int)_playersReputation[playerIndex - 1].reputationValue) - 1;
                player1PointsCollection[reputationIndexRemove].transform.GetComponent<Image>().fillAmount = 0f;
            }
            else if(_playersReputation[playerIndex - 1].reputationValue < 0)
            {
                print("Player 1 UI - INVISIBLE BAR");
                int reputationIndexRemove = (maxPlayerReputation + (int)_playersReputation[playerIndex - 1].reputationValue) + 1;
                print(reputationIndexRemove);
                player1ExtraPointsCollection[reputationIndexRemove].SetActive(true);
            }


        }
        else if(playerIndex == 2)
        {
            if (_playersReputation[playerIndex - 1].reputationValue >= 0)
            {
                print("Player 2 UI - VISIBLE BAR");
                int reputationIndexRemove = (maxPlayerReputation - (int)_playersReputation[playerIndex - 1].reputationValue) - 1;
                player2PointsCollection[reputationIndexRemove].transform.GetComponent<Image>().fillAmount = 0f;
            }
            else if (_playersReputation[playerIndex - 1].reputationValue < 0)
            {
                print("Player 2 UI - INVISIBLE BAR");
                int reputationIndexRemove = (maxPlayerReputation + (int)_playersReputation[playerIndex - 1].reputationValue) + 1;
                print(reputationIndexRemove);
                player2ExtraPointsCollection[reputationIndexRemove].SetActive(true);
            }
        }
        print("########### UpdatePlayersReputationUI END ########### ");
    }
    #endregion

}
