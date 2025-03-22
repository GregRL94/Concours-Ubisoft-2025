using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static GameManager;

public static class GameData
{
    public static int p1Point = 10;
    public static int p1LastRoundPoint = 0;
    public static int p2Point = 10;
    public static int p2LastRoundPoint = 0;
    public static bool skipFirstRound = true;
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
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

    ////////
    [Serializable]
    public class PlayerReputationUI
    {
        public Transform parentPoints;
        public GameObject uiPoint;
        public GameObject[] pointsCollection;

        public Transform parentExtraPoints;
        public GameObject uiExtraPoint;
        public GameObject[] extraPointsCollection;

        public Image BG;
    }

    [Header("Player 1 Reputation Points UI")]
    public PlayerReputationUI player1;

    [Header("Player 2 Reputation Points UI")]
    public PlayerReputationUI player2;
    ////////

    [Header("Capture Thief")]
    [SerializeField]
    private int maxCaptureThiefAmount = 30;
    [SerializeField]
    int currentCaptureThiefAmount;
    public int GetCurrentCaptureThiefAmount => currentCaptureThiefAmount;
    public int GetmaxCaptureThiefAmount => maxCaptureThiefAmount;
    private Coroutine finishUpdateCaptureThiefUIRoutine;

    [Header("Reputation Board UI")]
    [SerializeField] private GameObject ReputationUIBoard;
    [SerializeField] private GameObject NextRound;
    [SerializeField] private GameObject Draw;
    [SerializeField] private GameObject FinalResult;
    [SerializeField] private GameObject Player1Win;
    [SerializeField] private GameObject Player2Win;
    [SerializeField] private GameObject FinalResultOptions;
    bool _nextRound = false;
    private bool Confirmed;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }


    void Start()
    {
        ReputationUIBoard?.SetActive(false);
        captureThiefText.text = currentCaptureThiefAmount + "/" + maxCaptureThiefAmount;

        Debug.Log($"Chargement des scores : P1 = {GameData.p1Point}, P2 = {GameData.p2Point}");

    }

    #region Update Instantly Player Score Board
    //public void UpdatePlayersReputationUIInstantly(PlayerReputation[] _playersReputation, int maxPlayerReputation, int minPlayerReputation)
    //{
    //    print("Sup INSTANTLY !");
    //    if (!ReputationUIBoard.activeInHierarchy)
    //    {
    //        ReputationUIBoard.SetActive(true);
    //        ReputationUIBoard.GetComponent<Canvas>().enabled = false;
    //        for (int i = 0; i < _playersReputation.Length; i++)
    //        {
    //            // Each Player UI ref
    //            PlayerReputationUI currentPlayer = (i == 0) ? player1 : player2;
    //            int reputationValue = (int)_playersReputation[i].reputationValue;

    //            print($"Player {i + 1}: Reputation {reputationValue} | Max: {maxPlayerReputation}");


    //            // Positive Bar
    //            if (reputationValue >= 0)
    //            {
    //                HideReputationPointsInstantly(currentPlayer, maxPlayerReputation - reputationValue);
    //            }
    //            // Negative bar 
    //            else
    //            {
    //                HideReputationPointsInstantly(currentPlayer, maxPlayerReputation);
    //                ShowExtraReputationPointsInstantly(currentPlayer, reputationValue, minPlayerReputation);
    //            }
    //            ReputationUIBoard.GetComponent<Canvas>().enabled = true;
    //            ReputationUIBoard.SetActive(false);
    //        }
    //    }
    //}

    void HideReputationPointsInstantly(PlayerReputationUI playerReputationUI, int pointsToRemove)
    {
        for (int j = 0; j < pointsToRemove; j++)
        {
            playerReputationUI.pointsCollection[j].transform.GetComponent<Image>().enabled = false;
        }
    }

    // Show negative extra points
    void ShowExtraReputationPointsInstantly(PlayerReputationUI playerReputationUI, int reputationValue, int minPlayerReputation)
    {
        for (int k = reputationValue; k < 0 && k >= minPlayerReputation; k++)
        {
            int index = -(k) - 1;
            playerReputationUI.extraPointsCollection[index].SetActive(true);
        }
    }
    #endregion


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

    #region Update Board Reputation UI
   
    public void CreatePlayersReputationUI(int maxPlayerReputation, int minPlayerReputation, PlayerReputation[] _playersReputation)
    {
        CreatePlayerReputationUI(player1,maxPlayerReputation, minPlayerReputation);
        CreatePlayerReputationUI(player2,maxPlayerReputation, minPlayerReputation);
        //UpdatePlayersReputationUIInstantly(_playersReputation, maxPlayerReputation, minPlayerReputation);
    }
    private void CreatePlayerReputationUI(PlayerReputationUI playerUI, int maxReputation, int minReputation)
    {
        // Clean Parent gameobjects for + rep points
        foreach (Transform child in playerUI.parentPoints)
        {
            Destroy(child.gameObject);
        }

        // Create new childs gameobject for positive reputation
        playerUI.pointsCollection = new GameObject[maxReputation];
        for (int i = 0; i < maxReputation; i++)
        {
            playerUI.pointsCollection[i] = Instantiate(playerUI.uiPoint, playerUI.parentPoints);
        }

        // Clean Parent gameobjects for - rep points
        foreach (Transform child in playerUI.parentExtraPoints)
        {
            Destroy(child.gameObject);
        }

        // Create new childs gameobject for negative reputation
        int extraPointsCount = -minReputation;
        playerUI.extraPointsCollection = new GameObject[extraPointsCount];
        for (int i = 0; i < extraPointsCount; i++)
        {
            playerUI.extraPointsCollection[i] = Instantiate(playerUI.uiExtraPoint, playerUI.parentExtraPoints);
            playerUI.extraPointsCollection[i].SetActive(false);
        }
    }

    public void ShowReputationBoard(PlayerReputation[] _playersReputation, int maxPlayerReputation, int minPlayerReputation)
    {
        //Audio For Round Finished !
        StartCoroutine(UpdatePlayersReputationUI(_playersReputation, maxPlayerReputation, minPlayerReputation)); // todo: max,min test fait le !
    }

    int startingIndex;
    IEnumerator UpdatePlayersReputationUI(PlayerReputation[] _playersReputation, int maxPlayerReputation, int minPlayerReputation)
    {
        int chunkToRemove = 0;
        if (!GameData.skipFirstRound) 
        {
            for (int i = 0; i < _playersReputation.Length; i++)
            {
                int reputationValue = (int)_playersReputation[i].reputationValue;

                PlayerReputationUI currentPlayer;
                if(i == 0)
                {
                    currentPlayer = player1;
                    chunkToRemove = maxPlayerReputation - GameData.p1Point; 
                }
                else
                {
                    currentPlayer = player2;
                    chunkToRemove = maxPlayerReputation - GameData.p2Point;
                }


                // Positive Bar
                if (reputationValue >= 0)
                {
                    HideReputationPointsInstantly(currentPlayer, chunkToRemove);
                }
                // Negative bar 
                else
                {
                    HideReputationPointsInstantly(currentPlayer, chunkToRemove);
                    if(reputationValue > 0)
                        ShowExtraReputationPointsInstantly(currentPlayer, reputationValue, minPlayerReputation);
                }
                
            }
        }
        else GameData.skipFirstRound = false;

        //todo: Small delay before show board || animation
        ReputationUIBoard?.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        
        if (ReputationUIBoard.activeInHierarchy)
        {
            for (int i = 0; i < _playersReputation.Length; i++)
            {
                //PlayerReputationUI currentPlayer = (i == 0) ? player1 : player2;
                // Each Player UI ref
                int reputationValue = (int)_playersReputation[i].reputationValue;

                PlayerReputationUI currentPlayer;
                if (i == 0)
                {
                    currentPlayer = player1;
                    startingIndex = maxPlayerReputation - GameData.p1Point; // 2
                }
                else
                {
                    currentPlayer = player2;
                    startingIndex = maxPlayerReputation - GameData.p2Point; // 2
                }

                print($"Player {i + 1}: PreviousIndexValue {startingIndex} Reputation {reputationValue} | Max: {maxPlayerReputation}");

                // Show each player BG
                if (currentPlayer.BG != null) yield return StartCoroutine(ShowPlayerBGColor(currentPlayer.BG, 0.3f));
                yield return new WaitForSeconds(1);

                // Positive Bar
                if (reputationValue >= 0)
                {
                    yield return StartCoroutine(HideReputationPoints(currentPlayer, maxPlayerReputation - reputationValue));
                }
                // Negative bar 
                else
                {
                    yield return StartCoroutine(HideReputationPoints(currentPlayer, maxPlayerReputation));
                    yield return StartCoroutine(ShowExtraReputationPoints(currentPlayer, reputationValue, minPlayerReputation));
                }

                yield return new WaitForSeconds(0.5f);
            }
            // Next Round || Win Condition 
            yield return StartCoroutine(CheckWinOrNextRound(_playersReputation));
        }
    }

    // Hides positive points
    IEnumerator HideReputationPoints(PlayerReputationUI playerReputationUI, int pointsToRemove)
    {
        for (int j = startingIndex; j < pointsToRemove; j++)
        {
            yield return StartCoroutine(AnimateReputationPointRemoval(playerReputationUI.pointsCollection[j].transform.GetComponent<Image>(), 0.5f));
            yield return new WaitForSeconds(0.5f);
        }
    }

    // Show negative extra points
    IEnumerator ShowExtraReputationPoints(PlayerReputationUI playerReputationUI, int reputationValue, int minPlayerReputation)
    {   
        for (int k = reputationValue; k < 0 && k >= minPlayerReputation; k++)
        {
            int index = -(k) - 1;
            playerReputationUI.extraPointsCollection[index].SetActive(true);
        }
        yield return null;
    }

    IEnumerator CheckWinOrNextRound(PlayerReputation[] _playersReputation)
    {
        /////////////// NEXT ROUND || WINNER CONDITION ///////////////////
        for (int i = 0; i < _playersReputation.Length; i++)
        {
            // Next Round Condition
            if(!Confirmed)
            {
                _nextRound = _playersReputation[i].reputationValue > 0;
                if (!_nextRound) Confirmed = true; 
                //_nextRound = _playersReputation[i].reputationValue > 0 ? _nextRound = true : _nextRound = false; // bad code cause it erases the last bool
            }
            if (i == (int)PlayerEnum.PLAYER1 - 1)
            {
                //p1Point = (int)_playersReputation[i].reputationValue;
                GameData.p1Point = (int)_playersReputation[(int)PlayerEnum.PLAYER1 - 1].reputationValue;
                GameData.p1LastRoundPoint = (int)_playersReputation[(int)PlayerEnum.PLAYER1 - 1].reputationValue;
                //GameManager.Instance.SetPlayerReputation((int)PlayerEnum.PLAYER1 - 1, (int)_playersReputation[i].reputationValue); // saves data for all rounds
            }
            else if (i == (int)PlayerEnum.PLAYER2 - 1)
            {
                //p2Point = (int)_playersReputation[i].reputationValue;
                GameData.p2Point = (int)_playersReputation[(int)PlayerEnum.PLAYER2 - 1].reputationValue;
                GameData.p2LastRoundPoint = (int)_playersReputation[(int)PlayerEnum.PLAYER2 - 1].reputationValue;
                //GameManager.Instance.SetPlayerReputation((int)PlayerEnum.PLAYER2 - 1, (int)_playersReputation[i].reputationValue); // saves data for all rounds
            }

        }

        Debug.Log($"p1Point: {GameData.p1Point}, p2Point: {GameData.p2Point}");
        
        if (_nextRound)
        {
            Debug.Log("Next Round !");
            yield return new WaitForSeconds(1);
            NextRound?.SetActive(true);

            // Wait until any Input Pressed
            yield return new WaitUntil(() => Input.anyKeyDown);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else // Winner
        {
            yield return new WaitForSeconds(1f);
            
            if (GameData.p1Point != GameData.p2Point) // NOT DRAW
            {
                FinalResult?.SetActive(true);
                if (GameData.p1Point > GameData.p2Point)
                {
                    Debug.Log("Player 1 Won !");
                    Player1Win?.SetActive(true);
                    //todo : trophy too!
                }
                else if (GameData.p1Point < GameData.p2Point)
                {
                    Debug.Log("Player 2 Won !");
                    Player2Win?.SetActive(true);

                }
                FinalResultOptions?.SetActive(true);
            }
            else if (GameData.p1Point == GameData.p2Point)
            {
                Debug.Log("Match Nul ! Crée un One Pointer Match");
                Draw?.SetActive(true);
            }
        }
    }

    IEnumerator ShowPlayerBGColor(Image playerBG,float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            playerBG.color = new Color(playerBG.color.r, playerBG.color.g, playerBG.color.b, Mathf.Lerp(0, 1, elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerBG.color = new Color(playerBG.color.r, playerBG.color.g, playerBG.color.b, 1f);
    }

    public IEnumerator AnimateReputationPointRemoval(Image fillImage, float cooldownTime)
    {
        float elapsedTime = 0f;
        while (elapsedTime < cooldownTime)
        {
            fillImage.fillAmount = Mathf.Lerp(1, 0, elapsedTime / cooldownTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        fillImage.fillAmount = 0;
    }

    #endregion

    #region Update Capture Thief UI

    //// todo: Create a script for captureThief
    public void UpdateCaptureThiefGauge(int amount, PlayerEnum playerID)
    {
        print("####### UpdateCaptureThiefGauge ####### ");
        float previousAmount = currentCaptureThiefAmount;
        currentCaptureThiefAmount = Mathf.Clamp(currentCaptureThiefAmount + amount, 0, maxCaptureThiefAmount);

        //todo: put a (coroutine) so ui capture fills up before show board 
        //if(finishUpdateCaptureThiefUIRoutine == null)
        //{
        finishUpdateCaptureThiefUIRoutine = StartCoroutine(UpdateCaptureThiefUI(previousAmount, currentCaptureThiefAmount));
        //}

        if (currentCaptureThiefAmount >= maxCaptureThiefAmount)
        {
            print("Game Finish \n Show the Score Board P1 && P2");
            print("The player who has capture the thief at 100% is Player ID : " + playerID);
            GameManager.Instance.LosePlayerReputationByCapturingThief(playerID, 1);
        }
        print("####### UpdateCaptureThiefGauge End ####### ");
    }

    public IEnumerator UpdateCaptureThiefUI(float startAmount,float targetAmount)
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
