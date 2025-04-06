using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RobberManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _robber;
    private GameObject _currentRobber;
    [Header("Metrics")]

    [SerializeField, Tooltip("Number of attempt to spawn robber")]
    private float _maxSpawnRobberAttempt = 10;
    [SerializeField, Tooltip("Distance between robber and all players")]
    private float _minDistanceToPlayers = 0;
    
    [SerializeField, Tooltip("Distance between robber and all museum objects")]
    private float _minDistanceToMuseumObjects = 0;

    [SerializeField, Tooltip("Distance between robber and all traps")]
    private float _minDistanceToTraps = 0;
    [SerializeField, Tooltip("Distance between robber and all traps")]
    private List<ObjectType> _stealObjectList;

    private MuseumObjects[] _museumObjects;
    private GameObject[] _traps;

    // Start is called before the first frame update
    void Start()
    {
        //get all museum objects
        _museumObjects = FindObjectsOfType<MuseumObjects>();
        //get all traps, use before round start
        GetAllTraps();
    }

    public void GetAllTraps() => _traps = GameObject.FindGameObjectsWithTag("TRAP");

    public void SpawnRobber()
    {
        Vector3 spawnPosition = Vector3.zero;
        for (int i = 0; i < GameGrid.Instance.Grid.GetLength(0); i++)
        {
            for (int j = 0; j < GameGrid.Instance.Grid.GetLength(1); j++)
            {
                spawnPosition = GameGrid.Instance.Grid[i, j].worldPos;
                if (!GameGrid.Instance.Grid[i, j].isFree) continue;
                if (!IsValidSpawnPosition(spawnPosition)) continue;
                _currentRobber = Instantiate(_robber, spawnPosition, Quaternion.identity);
                SetupRobber();
                return;
            }
        }

        //random spawn in game map when no grid are suitable to spawn
        int gameGridSizeX = GameGrid.Instance.GameGridSizeX;
        int gameGridSizeY = GameGrid.Instance.GameGridSizeY;
        Vector3 mapSize = new Vector3(gameGridSizeX, 0, gameGridSizeY);
        Vector3 mapMiddlePoint = GameGrid.Instance.Grid[gameGridSizeX / 2, gameGridSizeY / 2].worldPos;
        spawnPosition = new Vector3(
                Random.Range(mapMiddlePoint.x - mapSize.x, mapMiddlePoint.x + mapSize.x),
                mapMiddlePoint.y,
                Random.Range(mapMiddlePoint.z - mapSize.z, mapMiddlePoint.z + mapSize.z));
        _currentRobber = Instantiate(_robber, spawnPosition, Quaternion.identity);
        SetupRobber();

    }
    private void SetupRobber()
    {
        GameManager.Instance.UIManager.CreateListOfMuseumArtefactsUI(_stealObjectList);
        RobberBehaviour robber = _currentRobber.GetComponent<RobberBehaviour>();
        if (robber == null) return;
        robber.StealingList.Clear();
        for (int i = 0; i < _stealObjectList.Count; i++) 
            robber.StealingList.Add(_stealObjectList[i]);
    }

    private bool IsValidSpawnPosition(Vector3 position)
    {
        for (int i = 0; i < GameManager.Instance.Players.Length; i++)
        {
            if (GameManager.Instance.Players[i] != null)
                if (Vector3.Distance(position, GameManager.Instance.Players[i].transform.position) < _minDistanceToPlayers)
                    return false;
        }

        for (int i = 0; i < _museumObjects.Length; i++)
        {
            if (_museumObjects[i] != null)
                if (Vector3.Distance(position, _museumObjects[i].transform.position) < _minDistanceToMuseumObjects)
                    return false;
        }

        for (int i = 0; i < _traps.Length; i++)
        {
            if (_traps[i] != null)
                if (Vector3.Distance(position, _traps[i].transform.position) < _minDistanceToTraps)
                    return false;
            
        }
        return true;
    }

    public void DispawnRobber() 
    {
        RobberBehaviour robber = _currentRobber.GetComponent<RobberBehaviour>();
        robber.StopAllCoroutines();
        robber.enabled = false;
        robber.gameObject.SetActive(false);
        //Destroy(_currentRobber);
    }
}
