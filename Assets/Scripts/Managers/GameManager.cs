using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public struct Rounds
    {
        public float timeBeforeRoundStart;
        public float roundTime;
        public bool hasRoundStarted;
    }
    [Header("Metrics")]
    [SerializeField]
    private Rounds _roundsParameter;
    public Rounds RoundParameter => _roundsParameter;
    //stock all players at start of game
    [SerializeField]
    private GameObject[] _players;
    public GameObject[] Players => _players;
    [SerializeField]
    private MuseumObjectsManager _museumObjectManager;
    [SerializeField]
    private RobberManager _robberManager;
    [SerializeField]
    private TrapManager _trapManager;

    public MuseumObjectsManager MuseumObjectsManager => _museumObjectManager;
    public RobberManager RobberManager => _robberManager;
    public TrapManager TrapManager => _trapManager;

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
        _players = GameObject.FindGameObjectsWithTag("Player");
        _preStartRoundCoroutine = StartCoroutine(PreStartRound(_roundsParameter.timeBeforeRoundStart));
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
        _preStartRoundCoroutine = StartCoroutine(PreStartRound(_roundsParameter.timeBeforeRoundStart));
    }
    #endregion
}
