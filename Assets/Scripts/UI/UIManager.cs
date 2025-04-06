using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static GameManager;
using System.Linq;
using AkuroTools;

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
    public RawImage radialRevealImage;
    private Material revealMat;

    [Header("Round UI")]
    public TextMeshProUGUI roundCountdownText;
    private bool Confirmed;
    private bool _nextRound = false;
    private int startingIndex; // starts from previous value index between rounds
    [SerializeField] private string timeUpText = "TIME UP";
    [SerializeField] private string capturedThiefText = "THIEF CAPTURED";
    [SerializeField] private string museumEmptyText = "MUSEUM IS EMPTY";
    
    //input for validation
    private DynamicsPlayersValidation _currentPlayerValidation;
    public DynamicsPlayersValidation CurrentPlayerValidation { get { return _currentPlayerValidation; } set { _currentPlayerValidation = value; } }
    
    public Dictionary<ObjectType, int> alreadyAssignedType = new Dictionary<ObjectType, int>();

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
        if(timeCountdown != 0)
        {
            roundCountdownText.text = timeCountdown.ToString();
        }
        else
        {
            roundCountdownText.text = "";
        }
    }
    #endregion

    #region UI Animation Score Board 
    // Anim - Hides positive points
    IEnumerator HideReputationPoints(PlayerReputationUI playerReputationUI, int pointsToRemove)
    {
        for (int j = startingIndex; j < pointsToRemove; j++)
        {
            yield return StartCoroutine(AnimateReputationPointRemoval(playerReputationUI.pointsCollection[j].transform.GetComponent<Image>(), 0.5f));
            yield return new WaitForSeconds(0.5f);
        }
    }

    // Anim - Show negative extra points
    IEnumerator ShowExtraReputationPoints(PlayerReputationUI playerReputationUI, int reputationValue, int minPlayerReputation)
    {
        int initialRep = reputationValue;
        for (int k = reputationValue; k < 0 && k >= minPlayerReputation; k++)
        {
            int index = -(-(k) + initialRep);
            playerReputationUI.extraPointsCollection[index].SetActive(true);

            yield return new WaitForSeconds(0.3f);
            Image[] cross = playerReputationUI.extraPointsCollection[index].transform.GetChild(1).GetComponentsInChildren<Image>().ToArray();
            for (int i = 0; i < cross.Length; i++)
            {
                yield return StartCoroutine(AnimateCross(cross[i], 0.5f));
                yield return new WaitForSeconds(0.3f);
            }
            yield return new WaitForSeconds(0.3f);
        }
    }
    #endregion

    #region UI Restore Last Score Board

    // Restore reputation points lost from previous rounds 
    void HideReputationPointsInstantly(PlayerReputationUI playerReputationUI, int pointsToRemove)
    {
        for (int j = 0; j < pointsToRemove; j++)
        {
            playerReputationUI.pointsCollection[j].transform.GetComponent<Image>().enabled = false;
        }
    }
    // Restore negative reputation points lost from previous rounds 
    void ShowExtraReputationPointsInstantly(PlayerReputationUI playerReputationUI, int reputationValue, int minPlayerReputation)
    {
        int initialRep = reputationValue;
        for (int k = reputationValue; k < 0 && k >= minPlayerReputation; k++)
        {
            int index = -(-(k) + initialRep);
            playerReputationUI.extraPointsCollection[index].SetActive(true);
        }
    }
    #endregion

    #region Update Museum Actefacts Checklist UI 
    public void CreateListOfMuseumArtefactsUI(List<ObjectType> museumArtefactsList)
    {
        // Empty Parent Childs if not empty
        foreach (Transform child in Parent)
        {
            Destroy(child.gameObject);
        }

        // Extract all keys found in the dictionnary and showcase it in UI List
        /*foreach (var kvp in museumArtefactsList)
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
        }*/
        alreadyAssignedType.Clear();
        for (int i = 0; i < museumArtefactsList.Count; i++)
        {
            bool skipObject = false;
            //Check if object already assigned
            for(int j = 0 ; j < alreadyAssignedType.Count; j++)
            {
                if (alreadyAssignedType.ContainsKey(museumArtefactsList[i]))
                {
                    alreadyAssignedType[museumArtefactsList[i]]++;
                    skipObject = true;
                    GameObject artefact = artefactNameCollection.Find(x => x.name == museumArtefactsList[i].ToString());
                    TextMeshProUGUI text = artefact.GetComponent<TextMeshProUGUI>();
                    if (text != null)
                    {
                        text.text = $"- {artefact.name} {alreadyAssignedType[museumArtefactsList[i]]}X";
                    }
                    break;
                }
            }

            if (skipObject) continue;
            alreadyAssignedType.Add(museumArtefactsList[i], 1);
            GameObject newObject = Instantiate(artefactText, Parent);
            newObject.name = museumArtefactsList[i].ToString();
            artefactNameCollection.Add(newObject);
            TextMeshProUGUI textComponent = newObject.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = $"- {museumArtefactsList[i].ToString()} {alreadyAssignedType[museumArtefactsList[i]]}X";
            }
        }

    }

    public void UpdateListOfMuseumArtefacts()
    {
        foreach (var artefactObj in artefactNameCollection) // Looping through all the gameobject artefacts created
        {
            string artefactName = artefactObj.name;

            // Verify if artifact name exist in the enum category
            if (Enum.TryParse(artefactName, out ObjectType artefactType)) 
            {
                int amount = 0; // Consider that amount = 0 for each artefact name

                // Verify if key exist and get -> MuseumObjectsTest[] (ex: PAINTING, 3 Painting objects in MuseumObjectsTest[])
                if (alreadyAssignedType.TryGetValue(artefactType, out int artefactsNb)) 
                {
                    amount = artefactsNb; // amount of MuseumObjects for each key
                }

                TextMeshProUGUI textComponent = artefactObj.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    if (amount == 0)
                    {
                        textComponent.text = $"- {artefactType}";
                        Image artefactImageCross = artefactObj.GetComponentInChildren<Image>();
                        artefactImageCross.enabled = true; // red line cross
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

    public void ShowReputationBoard(PlayerReputation[] _playersReputation, int maxPlayerReputation, int minPlayerReputation)
    {
        //Audio For Round Finished !
        StartCoroutine(UpdatePlayersReputationUI(_playersReputation, maxPlayerReputation, minPlayerReputation)); // todo: Ajmal - max,min test fait le !
    }

    IEnumerator UpdatePlayersReputationUI(PlayerReputation[] _playersReputation, int maxPlayerReputation, int minPlayerReputation)
    {
        UpdatePreviousBoardResult(_playersReputation, maxPlayerReputation, minPlayerReputation);

        yield return StartCoroutine(DelayBeforeShowingBoardResult());

        yield return StartCoroutine(AnimateCurrentBoardResult(_playersReputation, maxPlayerReputation, minPlayerReputation));
    }

    IEnumerator DelayBeforeShowingBoardResult()
    {
        if (currentCaptureThiefAmount >= maxCaptureThiefAmount) // THIEF CAPTURED TEXT
        {
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(ShowTextWithPopEffect(capturedThiefText));

            // Trouver tous les joueurs
            PlayerControls[] players = FindObjectsOfType<PlayerControls>();
            List<Animator> animators = new List<Animator>();

            foreach (var player in players)
            {
                Animator animator = player.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    // Réinitialiser toutes les animations
                    foreach (AnimatorControllerParameter param in animator.parameters)
                    {
                        if (param.type == AnimatorControllerParameterType.Bool)
                            animator.SetBool(param.name, false);
                    }

                    animator.SetTrigger("Capture");
                    animators.Add(animator);
                }
            }

            // Wait till capture anim finishes after showing the score board reputation 
            bool animationsFinished = false;
            while (!animationsFinished)
            {
                animationsFinished = true;
                foreach (var animator in animators)
                {
                    var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    if (stateInfo.IsTag("Capture") && stateInfo.normalizedTime < 1)
                    {
                        animationsFinished = false;
                        break;
                    }
                }
                yield return null;
            }
        }
        else if (GameManager.Instance.ValidateMuseumEmpty()) // MUSEUM IS EMPTY TEXT
        {
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(ShowTextWithPopEffect(museumEmptyText));

        }
        else // ROUND TIME ENDED TEXT
        {
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(ShowTextWithPopEffect(timeUpText));
        }
        yield return new WaitForSeconds(3f);
        roundCountdownText.text = "";
    }

    IEnumerator AnimateCurrentBoardResult(PlayerReputation[] _playersReputation, int maxPlayerReputation, int minPlayerReputation)
    {
        ReputationUIBoard.SetActive(true);
        radialRevealImage.gameObject.SetActive(true);
        yield return StartCoroutine(RevealReputationBoard(1.2f));

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
        }
        else // Winner
        {
            yield return new WaitForSeconds(1f);
            
            if (GameData.p1Point != GameData.p2Point) // NOT DRAW
            {
                FinalResult?.SetActive(true);
                if (GameData.p1Point > GameData.p2Point)
                {
                    Player1Win?.SetActive(true);
                    //todo : Ajmal - trophy animation for winner 
                }
                else if (GameData.p1Point < GameData.p2Point)
                {
                    Player2Win?.SetActive(true);

                }
                // todo: Ajmal - animate restart game or back to menu
                FinalResultOptions?.SetActive(true);
            }
            else if (GameData.p1Point == GameData.p2Point)
            {
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
    public IEnumerator AnimateCross(Image fillImage, float cooldownTime = 1f)
    {
        float elapsedTime = 0f;
        while (elapsedTime < cooldownTime)
        {
            fillImage.fillAmount = Mathf.Lerp(0, 1, elapsedTime / cooldownTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        fillImage.fillAmount = 1;
    }

    IEnumerator ShowTextWithPopEffect(string message, float duration = 1f)
    {
        roundCountdownText.text = message + " " + playerStrCapturedThief;

        CanvasGroup canvasGroup = roundCountdownText.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = roundCountdownText.gameObject.AddComponent<CanvasGroup>();

        // Initial setup
        canvasGroup.alpha = 0f;
        roundCountdownText.transform.localScale = Vector3.zero;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // Smooth pop effect using easing overshoot
            float scale = Mathf.SmoothStep(0f, 1.1f, t); // Slight overshoot
            if (t > 0.8f) scale = Mathf.Lerp(1.1f, 1f, (t - 0.8f) / 0.2f); // Come back to 1

            roundCountdownText.transform.localScale = new Vector3(scale, scale, scale);
            canvasGroup.alpha = Mathf.Clamp01(t);

            yield return null;
        }

        // Ensuring final values are set
        roundCountdownText.transform.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
    }

    string playerStrCapturedThief; 
    public void WhichPlayerCapturedThiefUI(string playerStrID)
    {
        playerStrCapturedThief = playerStrID;
    }

    IEnumerator ShowUIWithPopEffect(GameObject uiObject, float duration = 0.5f)
    {
        if (uiObject == null) yield break;

        uiObject.SetActive(true);

        // Ajouter CanvasGroup si manquant
        CanvasGroup canvasGroup = uiObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = uiObject.AddComponent<CanvasGroup>();

        // Initialiser les valeurs
        canvasGroup.alpha = 0f;
        uiObject.transform.localScale = Vector3.zero;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // Effet d'overshoot sur le scale
            float scale = Mathf.SmoothStep(0f, 1.1f, t);
            if (t > 0.8f)
                scale = Mathf.Lerp(1.1f, 1f, (t - 0.8f) / 0.2f);

            uiObject.transform.localScale = new Vector3(scale, scale, scale);
            canvasGroup.alpha = Mathf.Clamp01(t);

            yield return null;
        }

        // Set valeurs finales pour être sûr
        canvasGroup.alpha = 1f;
        uiObject.transform.localScale = Vector3.one;
    }
    IEnumerator RevealReputationBoard(float duration = 1f)
    {
        revealMat = radialRevealImage.material;
        revealMat.SetFloat("_Radius", 0f);
        revealMat.SetFloat("_Smoothness", 0.02f);

        revealMat.SetVector("_Center", new Vector4(0.5f, 0.5f, 0f, 0f));

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / duration);
            revealMat.SetFloat("_Radius", t);
            yield return null;
        }

        revealMat.SetFloat("_Radius", 1f);

        radialRevealImage.gameObject.SetActive(false);
    }
    #endregion

    #region Update Capture Thief UI
    // todo: Ajmal - Create a script for captureThief
    public void UpdateCaptureThiefGauge(int amount, PlayerEnum playerID)
    {
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
            AudioManager.instance.PlayClipAt(AudioManager.instance.allAudio["Robber Captured"], this.transform.position, AudioManager.instance.soundEffectMixer, true, false);
            GameManager.Instance.LosePlayerReputationByCapturingThief(playerID, 1);
            GameManager.Instance.EndRound();
        }
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

    public void ResetPlayerReput() => GameData.ResetPlayerPoints();


}