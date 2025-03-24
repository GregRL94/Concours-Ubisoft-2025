using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
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

    [Header("Metrics")]
    [SerializeField]
    private int _maxPlayersReputation = 10;
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


    public static GameManager Instance;

    private Coroutine _preStartRoundCoroutine;
    private Coroutine _startRoundCoroutine;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        InitializePlayers();

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
        _players = playersSorted;
    }

    #region Rounds management
    private IEnumerator PreStartRound(float time)
    {
        yield return new WaitForSeconds(time);
        if (_startRoundCoroutine != null)
        {
            StopCoroutine(_startRoundCoroutine);
            _startRoundCoroutine = null;
        }
        _startRoundCoroutine = StartCoroutine(StartRound(_roundsParameter.roundTime));
    }
    private IEnumerator StartRound(float time)
    {
        _roundsParameter.hasRoundStarted = true;
        _robberManager.SpawnRobber();
        yield return new WaitForSeconds(time);
        _roundsParameter.hasRoundStarted = false;
        _robberManager.DispawnRobber();
        if (_preStartRoundCoroutine != null)
        {
            StopCoroutine(_preStartRoundCoroutine);
            _preStartRoundCoroutine = null;
        }
        //To do : deplacer cette ligne dans l'UI apres l'attribution des points
        SceneLoading.Instance.LoadNextScene();
        _preStartRoundCoroutine = StartCoroutine(PreStartRound(_roundsParameter.timeBeforeRoundStart));
    }
    #endregion

    //make a player lose reputation
    //check if player is registered in player list
    //if one player lose all his reputation and the other is alone, end the game
    public void LosePlayerReputation(PlayerEnum playerIndex, int loseValue)
    {
        int index = (int)playerIndex;
        if (index < 0) return;
        if (index > _players.Length) return;
        _playersReputation[index - 1].reputationValue -= loseValue;
        Debug.Log($"{_players[index - 1].name} has lose {loseValue}, and is now at {_playersReputation[index - 1].reputationValue} reputation !");
        if (_playersReputation[index - 1].reputationValue > 0) return;
        _playersReputation[index - 1].isEliminated = true;
        CheckPlayersElimination();
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

    //start gameplay loop
    public void StartGameLoop() => _preStartRoundCoroutine = StartCoroutine(PreStartRound(_roundsParameter.timeBeforeRoundStart));
    
}
