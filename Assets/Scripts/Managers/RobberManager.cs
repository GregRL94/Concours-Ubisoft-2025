using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobberManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _robber;
    [Header("Metrics")]

    [SerializeField, Tooltip("Number of attempt to spawn robber")]
    private float _maxSpawnRobberAttempt = 10;
    [SerializeField, Tooltip("Distance between robber and all players")]
    private float _minDistanceToPlayers = 0;
    
    [SerializeField, Tooltip("Distance between robber and all museum objects")]
    private float _minDistanceToMuseumObjects = 0;

    [SerializeField, Tooltip("Distance between robber and all traps")]
    private float _minDistanceToTraps = 0;
    

    private MuseumObjects[] _museumObjects;

    // Start is called before the first frame update
    void Start()
    {
        _museumObjects = FindObjectsOfType<MuseumObjects>();

        SpawnRobber();
    }

    public void SpawnRobber()
    {
        Vector3 spawnPosition = Vector3.zero;
        for (int i = 0; i < GameGrid.Instance.Grid.GetLength(0); i++)
        {
            for (int j = 0; j < GameGrid.Instance.Grid.GetLength(1); j++)
            {
                spawnPosition = GameGrid.Instance.Grid[i, j].worldPos;
                if (!IsValidSpawnPosition(spawnPosition)) continue;
                Instantiate(_robber, spawnPosition, Quaternion.identity);
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
        Instantiate(_robber, spawnPosition, Quaternion.identity);
        
    }
    private bool IsValidSpawnPosition(Vector3 position)
    {
        for (int i = 0; i < GameManager.Instance.Players.Length; i++)
        {
            if(Vector3.Distance(position, GameManager.Instance.Players[i].transform.position) < _minDistanceToPlayers)
                return false;
        }

        for (int i = 0; i < _museumObjects.Length; i++)
        {
            if(Vector3.Distance(position, _museumObjects[i].transform.position) < _minDistanceToMuseumObjects)
                return false;
        }

        //TO DO : same with traps
        return true;
    }
}
