using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Serializable]
    public struct Rounds
    {
        public float timeBeforeRoundStart;
        public float roundTime;
        public bool hasRoundStarted;
    }

    [Serializable]
    public struct PlayerReputation
    {
        public float reputationValue;
        public bool isEliminated;
    }

    [Serializable]
    public class TrapData
    {
        public GameObject trapPrefab;
        [Range(0f, 10f)] public float setupTime;
        [Range(0, 5)] public int initialCount;
        [Range(0f, 10f)] public float cooldown;
        [HideInInspector] public TextMeshProUGUI countText;
        [HideInInspector] public Image fillImage;
        [HideInInspector] public int currentCount;
        [HideInInspector] public float timer;

        public TrapData() {}

        public TrapData(GameObject trapPrefab, float setupTime, int initialCount, float cooldown, TextMeshProUGUI countText, Image fillImage)
        {
            this.trapPrefab = trapPrefab;
            this.setupTime = setupTime;
            this.initialCount = initialCount;
            this.cooldown = cooldown;
            this.countText = countText;
            this.fillImage = fillImage;
        }
    }

    [Serializable]
    public class WhistleData
    {
        [Range(0f, 5f)] public float whistleFleeRange;
        [Range(0f, 5f)] public float whistleCaptureRange;
        [Range(0f, 100f)] public int whistleCapturePower;
        [Range(0f, 10f)] public float whistleCooldown;
        [HideInInspector] public Image fillImage;
        [HideInInspector] public float timer;

        public WhistleData() {}

        public WhistleData(float whistleFleeRange, float whistleCaptureRange, int whistleCapturePower, float whistleCooldown, Image fillImage)
        {
            this.whistleFleeRange = whistleFleeRange;
            this.whistleCaptureRange = whistleCaptureRange;
            this.whistleCapturePower = whistleCapturePower;
            this.whistleCooldown = whistleCooldown;
            this.fillImage = fillImage;
        }
    }
    [Header("ALL MANAGERS")]
    [SerializeField]
    private MuseumObjectsManager _museumObjectManager;
    [SerializeField]
    private RobberManager _robberManager;
    [SerializeField]
    private TrapManager _trapManager;
    [SerializeField]
    private UIManager _uiManager;
    [SerializeField]
    private TutorialManager _tutorialManager;

    [Header("Metrics")]
    [Range(0,13)][SerializeField]
    private int _maxPlayersReputation = 10;
    const int _minPlayersReputation = -4;
    [SerializeField]
    private Rounds _roundsParameter;
    public Rounds RoundParameter => _roundsParameter;

    [SerializeField]
    private Color[] _playerColor;
    public Color[] PlayerColor => _playerColor;

    [Header("-- ABILITIES --")]
    [Header("Whistle")]
    [SerializeField] private WhistleData whistleBase;
    public WhistleData WhistleBase => whistleBase;

    [Header("Traps")]
    [SerializeField] private TrapData alarmTrapBase;
    public TrapData AlarmTrapBase => alarmTrapBase;

    [SerializeField] private TrapData pushTrapBase;
    public TrapData PushTrapBase => pushTrapBase;

    [SerializeField] private TrapData captureTrapBase;
    public TrapData CaptureTrapBase => captureTrapBase;

    

    [Header("Debug")]
    //stock all players at start of game
    [SerializeField]
    private PlayerControls[] _players;
    [SerializeField]
    private PlayerReputation[] _playersReputation;
    [SerializeField]
    private TextMeshProUGUI _timerText;

    //getter
    public PlayerControls[] Players => _players;
    public MuseumObjectsManager MuseumObjectsManager => _museumObjectManager;
    public RobberManager RobberManager => _robberManager;
    public TrapManager TrapManager => _trapManager;
    public UIManager UIManager => _uiManager;
    public TutorialManager TutorialManager => _tutorialManager;


    [Header("Round Metrics")]
    [SerializeField]
    private int showTimeBeforeRoundEnd = 5;
    private Coroutine _preStartRoundCoroutine;
    private Coroutine _startRoundCoroutine;
    private bool _endGame = false;


    public static GameManager Instance { get; private set; }
    void Awake()
    {
        DetachGameManager();

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(this.gameObject);

    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnApplicationQuit()
    {
        print("Reset Player Points on Quit");
        GameData.ResetPlayerPoints();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        AssignManagers();
        InitializePlayers();

        UIManager.CreatePlayersReputationUI(_maxPlayersReputation, _minPlayersReputation, _playersReputation);
    }
    public void StartGameLoop()
    {
        // Start gameplay loop
        _endGame = false;
        _preStartRoundCoroutine = StartCoroutine(PreStartRound(_roundsParameter.timeBeforeRoundStart));
    }

    void DetachGameManager()
    {
        if (transform.childCount > 0)
        {
            // Store all children before detaching to avoid modification issues during iteration
            List<Transform> children = new List<Transform>();

            foreach (Transform child in transform)
            {
                children.Add(child);
            }

            // Detach each child
            foreach (Transform child in children)
            {
                child.SetParent(null); // Removes the parent-child relationship
            }
        }
    }

    private void AssignManagers()
    {
        if(!_museumObjectManager) _museumObjectManager = FindAnyObjectByType<MuseumObjectsManager>();
        if(!_robberManager)_robberManager = FindAnyObjectByType<RobberManager>();
        if(!_trapManager)_trapManager = FindAnyObjectByType<TrapManager>();
        if(!_uiManager)_uiManager = FindAnyObjectByType<UIManager>();
        if(!_tutorialManager) _tutorialManager = FindAnyObjectByType<TutorialManager>();
        _timerText = _uiManager.roundCountdownText;
    }

    private void InitializePlayers()
    {
        _players = FindObjectsOfType<PlayerControls>();
        int playerCount = _players.Length;
        _playersReputation = new PlayerReputation[playerCount];

        PlayerControls[] playersSorted = new PlayerControls[playerCount];

        for (int i = 0; i < playerCount; i++)
        {
            int playerIndex = (int)_players[i].PlayerID - 1;
            if (playerIndex < 0 || playerIndex >= playerCount) continue;

            playersSorted[playerIndex] = _players[i];

            _playersReputation[i] = new PlayerReputation
            {
                reputationValue = GameData.FirstRound ? _maxPlayersReputation :
                                (playerIndex == 0 ? GameData.p1Point : GameData.p2Point)
            };

            if (GameData.FirstRound)
            {
                if (playerIndex == 0) GameData.p1Point = _maxPlayersReputation;
                else GameData.p2Point = _maxPlayersReputation;
            }
        }

        _players = playersSorted;

        //print($"P1 {_playersReputation[0].reputationValue} | P2 {_playersReputation[1].reputationValue} in scene on Loaded!");
    }


    #region Rounds Management
    private IEnumerator PreStartRound(float time)
    {
        float timer = time;
        while (timer > 0)
        {
            _timerText.text = timer.ToString();
            yield return new WaitForSeconds(1);
            timer--;
        }
        if (_startRoundCoroutine != null)
        {
            StopCoroutine(_startRoundCoroutine);
            _startRoundCoroutine = null;
        }
        _timerText.text = "";
        _startRoundCoroutine = StartCoroutine(StartRound(_roundsParameter.roundTime, _roundsParameter.roundTime - showTimeBeforeRoundEnd)); 
    }
    private IEnumerator StartRound(float time, float timeLeftWarning)
    {
        _roundsParameter.hasRoundStarted = true;
        _robberManager.SpawnRobber();
        yield return new WaitForSeconds(timeLeftWarning);

        for (int i = 0; i < time - timeLeftWarning ; i++)
        {
            int timeLeft = (int)((int)time - timeLeftWarning - i);
            _uiManager.ShowUIRoundCountdown(timeLeft);
            yield return new WaitForSeconds(1);
            print(Time.time);
        }

        //_roundsParameter.hasRoundStarted = false;
        //_robberManager.DispawnRobber();

        //todo: Audio - For Round Finished
        _uiManager.ShowReputationBoard(_playersReputation, _maxPlayersReputation, _minPlayersReputation);
        _roundsParameter.hasRoundStarted = false;
        _robberManager.DispawnRobber();
    

        //if (_preStartRoundCoroutine != null)
        //{
        //    StopCoroutine(_preStartRoundCoroutine);
        //    _preStartRoundCoroutine = null;
        //}
        //_preStartRoundCoroutine = StartCoroutine(PreStartRound(_roundsParameter.timeBeforeRoundStart));
    }
    #endregion
    void Update()
    {
        // todo: Thomas - Pause(robber) everything in game except ui Update for next round
        // todo: Thomas - Check robber amount object left
        CheckEndRound();
    }

    void CheckEndRound()
    {
        if ((ValidateMuseumEmpty() || UIManager.GetCurrentCaptureThiefAmount >= UIManager.GetmaxCaptureThiefAmount) && !_endGame)
        {
            _endGame = true;
            UIManager.ShowReputationBoard(_playersReputation, _maxPlayersReputation, _minPlayersReputation);
        }
    }


    //make a player lose reputation
    //check if player is registered in player list
    //if one player lose all his reputation and the other is alone, end the game
    public void LosePlayerReputation(PlayerEnum playerIndex, int loseValue)
    {
        int index = (int)playerIndex;
        if (index < 0) return;
        if (index > _players.Length) return;
        _playersReputation[index - 1].reputationValue -= loseValue;
        //Debug.Log($"{_players[index - 1].name} has lose {loseValue}, and is now at {_playersReputation[index - 1].reputationValue} reputation !");

        //if (_playersReputation[index - 1].reputationValue > 0) return;
        //_playersReputation[index - 1].isEliminated = true;
        //CheckPlayersElimination();
    }


    // Determine which player did catch the thief at 100%
    // Opposite player loses a point at the end of the round
    public void LosePlayerReputationByCapturingThief(PlayerEnum playerIndex, int loseValue)
    {
        int index = (int)playerIndex;
        if (index < 0 || index > _players.Length) return;

        // Oppose player -1 point to its reputation if the adversary captured at last the thief
        if (index == (int)PlayerEnum.PLAYER1) index++;
        else if (index == (int)PlayerEnum.PLAYER2) index--;
        else Debug.Log("Player ID NULL");
        _playersReputation[index - 1].reputationValue -= loseValue;
        
    }


    //make all other players lose reputation
    //using this with robber capture
    public void AllOtherPlayersLoseReputation(PlayerEnum playerIndex, int loseValue)
    {
        if(playerIndex < 0) return;
        for (int i = 0; i < _players.Length; i++)
        {
            if ((int)playerIndex - 1 == i) continue;
            LosePlayerReputation((PlayerEnum)i + 1, loseValue);
        }
    }


    //check all players reputation
    //when there is only one player not eliminated, make that one player win
    //when everyone is eliminated, make it a draw
    private void CheckPlayersElimination()
    {
        int index = 0;
        int eliminatedValue = 0;
       
        for (int i = 0; i < _playersReputation.Length; i++)
        {
            if (!_playersReputation[i].isEliminated)
            {
                index = i;
                continue;
            }
            
            eliminatedValue++;
        }

        if (eliminatedValue == _playersReputation.Length - 1)
            Debug.Log($"{_players[index]} win !");

        if (eliminatedValue >= _playersReputation.Length)
            Debug.Log("It's a DRAW !");
    }


    //Check if museum has any remaining  artefacts
    public bool ValidateMuseumEmpty()
    {
        // Checks Museum List is Empty or Not
        if (_museumObjectManager.CountAllMuseumObjects() == 0)
        {
            return true;
        }
        return false;
    }
}
