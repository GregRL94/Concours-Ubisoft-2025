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
    private CaptureThiefManager _captureThiefManager;

    [Header("Metrics")]
    [Range(0,13)][SerializeField]
    private int _maxPlayersReputation = 10;
    public int MaxPlayersReputation => _maxPlayersReputation;
    const int _minPlayersReputation = -4;
    [SerializeField]
    private Rounds _roundsParameter;
    public Rounds RoundParameter => _roundsParameter;

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
    
    //getter
    public PlayerControls[] Players => _players;
    public MuseumObjectsManager MuseumObjectsManager => _museumObjectManager;
    public RobberManager RobberManager => _robberManager;
    public TrapManager TrapManager => _trapManager;
    public UIManager UIManager => _uiManager;
    public CaptureThiefManager CaptureThiefManager => _captureThiefManager;


    [Header("Round Metrics")]
    private Coroutine _preStartRoundCoroutine;
    private Coroutine _startRoundCoroutine;
    public TextMeshProUGUI countdown;

    public static GameManager Instance { get; private set; }
    void Awake()
    {
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

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene Loaded: {scene.name}. Reassigning references...");

        endGame = false;
        _museumObjectManager = FindAnyObjectByType<MuseumObjectsManager>();
        _robberManager = FindAnyObjectByType<RobberManager>();
        _trapManager = FindAnyObjectByType<TrapManager>();
        _uiManager = FindAnyObjectByType<UIManager>();
        _captureThiefManager = FindAnyObjectByType<CaptureThiefManager>();

        ReInitializePlayers();

        UIManager.CreatePlayersReputationUI(_maxPlayersReputation, _minPlayersReputation,_playersReputation);

        // start gameplay loop
        _preStartRoundCoroutine = StartCoroutine(PreStartRound(_roundsParameter.timeBeforeRoundStart));

    }
    void Start()
    {
        print("GameManager Hey");
        InitializePlayers();

        //TODO: UI - Time before round starts
    }

    private void InitializePlayers()
    {
        _players = FindObjectsOfType<PlayerControls>();
        _playersReputation = new PlayerReputation[_players.Length];

        //sort player depending on its id
        PlayerControls[] playersSorted = new PlayerControls[_players.Length];
        for (int i = 0; i < _players.Length; i++)
        {
            if ((int)_players[i].PlayerID - 1 < 0) continue;
            playersSorted[(int)_players[i].PlayerID - 1] = _players[i];
            //initialize player reputation
            _playersReputation[i].reputationValue = _maxPlayersReputation;
        }

        GameData.p1Point = _maxPlayersReputation;
        GameData.p2Point = _maxPlayersReputation;
        Debug.Log($"Chargement des scores : P1 = {GameData.p1Point}, P2 = {GameData.p2Point}");
        print("P1 " + _playersReputation[0].reputationValue + "P2 " +
        _playersReputation[1].reputationValue + "in Initial Players !");

        _players = playersSorted;
    }

    private void ReInitializePlayers()
    {
        _players = FindObjectsOfType<PlayerControls>();
        _playersReputation = new PlayerReputation[_players.Length];

        //sort player depending on its id
        PlayerControls[] playersSorted = new PlayerControls[_players.Length];
        for (int i = 0; i < _players.Length; i++)
        {
            if ((int)_players[i].PlayerID - 1 < 0) continue;
            playersSorted[(int)_players[i].PlayerID - 1] = _players[i];
        }

         //initialize player reputation
        _playersReputation[0].reputationValue = GameData.p1Point;
        _playersReputation[1].reputationValue = GameData.p2Point;


        print("P1 " + _playersReputation[0].reputationValue + " P2 " +
        _playersReputation[1].reputationValue + " in scene on Loaded !");

        _players = playersSorted;

    }

    #region Rounds Management
    private IEnumerator PreStartRound(float time)
    {
        yield return new WaitForSeconds(time);
        if (_startRoundCoroutine != null)
        {
            StopCoroutine(_startRoundCoroutine);
            _startRoundCoroutine = null;
        }
        _startRoundCoroutine = StartCoroutine(StartRound(_roundsParameter.roundTime, _roundsParameter.roundTime - 5));
    }
    private IEnumerator StartRound(float time, float timeLeftWarning)
    {
        _roundsParameter.hasRoundStarted = true;
        _robberManager.SpawnRobber();
        yield return new WaitForSeconds(timeLeftWarning);

        for (int i = 0; i < time - timeLeftWarning ; i++)
        {
            int test = (int)((int)time - timeLeftWarning - i);
            countdown.text = test.ToString();
            yield return new WaitForSeconds(1);
            print(Time.time);
        }

        _roundsParameter.hasRoundStarted = false;
        _robberManager.DispawnRobber();

        // Audio For Round Finished !
        // todo: show board !
        _uiManager.Invoke("ShowReputationBoard", 1f);
     
        //_roundsParameter.hasRoundStarted = false;
        //_robberManager.DispawnRobber();
        //if (_preStartRoundCoroutine != null)
        //{
        //    StopCoroutine(_preStartRoundCoroutine);
        //    _preStartRoundCoroutine = null;
        //}
        //_preStartRoundCoroutine = StartCoroutine(PreStartRound(_roundsParameter.timeBeforeRoundStart));
    }
    #endregion
    public bool endGame = false;
    private void Update()
    {
        print("GameManager Update !");
        /*Time.time >= _roundsParameter.roundTime ||*/
        if ((ValidateMuseumEmpty() || UIManager.GetCurrentCaptureThiefAmount >= UIManager.GetmaxCaptureThiefAmount) && !endGame)
        {
            
            endGame = true;
            UIManager.ShowReputationBoard( _playersReputation, _maxPlayersReputation, _minPlayersReputation);
        }

    }

    //make a player lose reputation
    //check if player is registered in player list
    //if one player lose all his reputation and the other is alone, end the game
    public void LosePlayerReputation(PlayerEnum playerIndex, int loseValue)
    {
        print("############ LosePlayerReputation ############");
        int index = (int)playerIndex;
        print("PlayerEnum Index "+index);
        if (index < 0) return;
        if (index > _players.Length) return;
        _playersReputation[index - 1].reputationValue -= loseValue;
        Debug.Log($"{_players[index - 1].name} has lose {loseValue}, and is now at {_playersReputation[index - 1].reputationValue} reputation !");
        //UI Update for Player reputation
        
        //UIManager.UpdatePlayersReputationUI(index, _playersReputation, _maxPlayersReputation);

        //if (_playersReputation[index - 1].reputationValue > 0) return;
        //_playersReputation[index - 1].isEliminated = true;

        //CheckPlayersElimination();

        print("############ LosePlayerReputation End ############");
    }


    // PlayerEnum -> (int)PLAYER 1 == 1 -> index == 0
    // PlayerEnum -> (int)PLAYER 2 == 2 -> index == 1
    public void LosePlayerReputationByCapturingThief(PlayerEnum playerIndex, int loseValue)
    {
        print("############ LosePlayerReputationByCapturingThief ############");
        int index = (int)playerIndex;
        if (index < 0 || index > _players.Length) return;

        // Oppose player -1 point to its reputation if the adversary captured at last the thief
        if (index == (int)PlayerEnum.PLAYER1) index++;
        else if (index == (int)PlayerEnum.PLAYER2) index--;
        else Debug.Log("Player ID NULL");
        _playersReputation[index - 1].reputationValue -= loseValue;
        Debug.Log($"Captured Thief !!!!! {_players[index - 1].name} has lose {loseValue}, and is now at {_playersReputation[index - 1].reputationValue} reputation !");
        
        print("############ LosePlayerReputationByCapturingThief End ############");
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

    //CheckAndDisplayReputationBoard 
    public bool ValidateMuseumEmpty()
    {
        // Checks Museum List is Empty or Not
        if (_museumObjectManager.CountAllMuseumObjects() == 0)
        {
            //ShowReputationBoard();
            return true;
        }
        return false;
    }


    // Keeping Player Reputation data between scenes
    #region Players Reputation Get/Set
    //public PlayerReputation GetPlayerReputation(int playerIndex)
    //{
    //    return _playersReputation[playerIndex];
    //}

    //public void SetPlayerReputation(int playerIndex, int newReputation)
    //{
    //    _playersReputation[playerIndex].reputationValue = newReputation;

    //    Debug.Log($"Réputation sauvegardée: Joueur {playerIndex + 1} = {_playersReputation[playerIndex].reputationValue}");

    //}


    #endregion


}
