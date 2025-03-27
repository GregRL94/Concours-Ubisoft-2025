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
    public static int p1Point;
    public static int p2Point;
    public static bool FirstRound = true;

    public static void ResetPlayerPoints()
    {
        p1Point = 0;
        p2Point = 0;
        FirstRound = true;
    }
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

    [Header("Reputation Board UI")]
    [SerializeField] private GameObject ReputationUIBoard;
    [SerializeField] private GameObject NextRound;
    [SerializeField] private GameObject Draw;
    [SerializeField] private GameObject FinalResult;
    [SerializeField] private GameObject Player1Win;
    [SerializeField] private GameObject Player2Win;
    [SerializeField] private GameObject FinalResultOptions;

    [Header("Round UI")]
    public TextMeshProUGUI roundCountdownText;

    private bool Confirmed;
    private bool _nextRound = false;
    private int startingIndex; // starts from previous value index between rounds

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
    }
    #region UI Show Round Time
    public void ShowUIRoundCountdown(int timeCountdown)
    {
        roundCountdownText.text = timeCountdown.ToString();
    }
    #endregion

    #region UI Animation Score Board 

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


    #endregion

    #region UI Restore Last Score Board
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

    #region Update Players Score Board UI
    public void CreatePlayersReputationUI(int maxPlayerReputation, int minPlayerReputation, PlayerReputation[] _playersReputation)
    {
        CreatePlayerReputationUI(player1,maxPlayerReputation, minPlayerReputation);
        CreatePlayerReputationUI(player2,maxPlayerReputation, minPlayerReputation);
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
        StartCoroutine(UpdatePlayersReputationUI(_playersReputation, maxPlayerReputation, minPlayerReputation)); // todo: Ajmal - max,min test fait le !
    }

    IEnumerator UpdatePlayersReputationUI(PlayerReputation[] _playersReputation, int maxPlayerReputation, int minPlayerReputation)
    {
        UpdatePreviousBoardResult(_playersReputation, maxPlayerReputation, minPlayerReputation);
        yield return StartCoroutine(AnimateCurrentBoardResult(_playersReputation, maxPlayerReputation, minPlayerReputation));
    }

    IEnumerator AnimateCurrentBoardResult(PlayerReputation[] _playersReputation, int maxPlayerReputation, int minPlayerReputation)
    {
        // Animate Results from current Round
        //todo: Ajmal - Small delay before show board || animation
        ReputationUIBoard?.SetActive(true);
        yield return new WaitForSeconds(1.5f);

        if (ReputationUIBoard.activeInHierarchy)
        {
            for (int i = 0; i < _playersReputation.Length; i++)
            {
                // Each Player UI ref
                int reputationValue = (int)_playersReputation[i].reputationValue;

                PlayerReputationUI currentPlayer;
                if (i == 0)
                {
                    currentPlayer = player1;
                    startingIndex = maxPlayerReputation - GameData.p1Point;
                }
                else
                {
                    currentPlayer = player2;
                    startingIndex = maxPlayerReputation - GameData.p2Point;
                }

                //print($"Player {i + 1}: PreviousIndexValue {startingIndex} Reputation {reputationValue} | Max: {maxPlayerReputation}");

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

    private void UpdatePreviousBoardResult(PlayerReputation[] _playersReputation, int maxPlayerReputation, int minPlayerReputation)
    {
        int pointsToRemove = 0; // remove point that has already been depleted after first round

        if (!GameData.FirstRound) // Skip First Round, no point has been depleted 
        {
            for (int i = 0; i < _playersReputation.Length; i++)
            {
                int reputationValue = (int)_playersReputation[i].reputationValue;

                // 
                PlayerReputationUI currentPlayer;
                if (i == 0)
                {
                    currentPlayer = player1;
                    pointsToRemove = maxPlayerReputation - GameData.p1Point;
                }
                else
                {
                    currentPlayer = player2;
                    pointsToRemove = maxPlayerReputation - GameData.p2Point;
                }

                // Positive Bar
                if (reputationValue >= 0)
                {
                    HideReputationPointsInstantly(currentPlayer, pointsToRemove);
                }
                // Negative bar 
                else
                {
                    HideReputationPointsInstantly(currentPlayer, pointsToRemove);

                    if (reputationValue > 0) // show negative point only if previous has been animated 
                        ShowExtraReputationPointsInstantly(currentPlayer, reputationValue, minPlayerReputation);
                }

            }
        }
        else GameData.FirstRound = false;
    }

    IEnumerator CheckWinOrNextRound(PlayerReputation[] _playersReputation)
    {

        for (int i = 0; i < _playersReputation.Length; i++)
        {
            // Next Round Condition
            if(!Confirmed)
            {
                _nextRound = _playersReputation[i].reputationValue > 0;
                if (!_nextRound) Confirmed = true; 
            }
            if (i == (int)PlayerEnum.PLAYER1 - 1)
            {
                GameData.p1Point = (int)_playersReputation[(int)PlayerEnum.PLAYER1 - 1].reputationValue;
            }
            else if (i == (int)PlayerEnum.PLAYER2 - 1)
            {
                GameData.p2Point = (int)_playersReputation[(int)PlayerEnum.PLAYER2 - 1].reputationValue;
            }

        }

        //Debug.Log($"p1Point: {GameData.p1Point}, p2Point: {GameData.p2Point}");
        
        if (_nextRound)
        {
            Debug.Log("Next Round !");
            yield return new WaitForSeconds(1);
            NextRound?.SetActive(true);

            // todo: Thomas - Change avec ton round scene loop
            yield return new WaitUntil(() => Input.anyKeyDown); // Wait until any Input Pressed
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
                    //Debug.Log("Player 1 Won !");
                    Player1Win?.SetActive(true);
                    //todo : Ajmal - trophy animation for winner 
                }
                else if (GameData.p1Point < GameData.p2Point)
                {
                    //Debug.Log("Player 2 Won !");
                    Player2Win?.SetActive(true);

                }
                // todo: Ajmal - animate restart game or back to menu
                FinalResultOptions?.SetActive(true);
            }
            else if (GameData.p1Point == GameData.p2Point)
            {
                // todo: Ajmal - Draw ! Golden Point Match
                Draw?.SetActive(true);
            }
        }
    }
    #endregion

    #region Animations UI
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

    // todo: Ajmal - Create a script for captureThief
    public void UpdateCaptureThiefGauge(int amount, PlayerEnum playerID)
    {
        print("####### UpdateCaptureThiefGauge ####### ");
        float previousAmount = currentCaptureThiefAmount;
        currentCaptureThiefAmount = Mathf.Clamp(currentCaptureThiefAmount + amount, 0, maxCaptureThiefAmount);

        StartCoroutine(UpdateCaptureThiefUI(previousAmount, currentCaptureThiefAmount));

        // Determine which player captured the thief at the end of the round
        if (currentCaptureThiefAmount >= maxCaptureThiefAmount)
        {
            if (playerID == PlayerEnum.NONE)
            {
                currentCaptureThiefAmount--;
                StartCoroutine(UpdateCaptureThiefUI(previousAmount, currentCaptureThiefAmount));
                return;
            }
            GameManager.Instance.LosePlayerReputationByCapturingThief(playerID, 1);
        }
        print("####### UpdateCaptureThiefGauge End ####### ");
    }

    public IEnumerator UpdateCaptureThiefUI(float startAmount, float targetAmount)
    {
        float elapsedTime = 0f;
        float duration = 0.5f;

        // Get initial fill amounts
        float startFill = fillCaptureGaugeImage.fillAmount;
        float targetFill = targetAmount / maxCaptureThiefAmount;

        float startIconFill = fillCaptureGaugeIconImage.fillAmount;
        float targetIconFill = targetAmount / maxCaptureThiefAmount;

        // Initialize the current amount for number animation
        float currentAmount = startAmount;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Smoothly interpolate the fill amounts
            fillCaptureGaugeImage.fillAmount = Mathf.Lerp(startFill, targetFill, t);
            fillCaptureGaugeIconImage.fillAmount = Mathf.Lerp(startIconFill, targetIconFill, t);

            // Smoothly interpolate the displayed number
            currentAmount = Mathf.Lerp(startAmount, targetAmount, t);
            captureThiefText.text = Mathf.RoundToInt(currentAmount) + "/" + maxCaptureThiefAmount;

            yield return null;
        }

        // Ensure final values are correctly set
        fillCaptureGaugeImage.fillAmount = targetFill;
        fillCaptureGaugeIconImage.fillAmount = targetFill;
        captureThiefText.text = Mathf.RoundToInt(targetAmount) + "/" + maxCaptureThiefAmount;
    }
    #endregion

}
