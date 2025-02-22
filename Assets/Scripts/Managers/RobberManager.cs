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

    [SerializeField, Tooltip("Size of the map")]
    private Vector3 _MapSize = new Vector3(10f, 0f, 10f);

    

    private MuseumObjects[] _museumObjects;

    // Start is called before the first frame update
    void Start()
    {
        _museumObjects = FindObjectsOfType<MuseumObjects>();

        SpawnRobber();
    }

    public void SpawnRobber()
    {
        for(int i = 0; i < _maxSpawnRobberAttempt; i++)
        {
            Vector3 spawnPosition = new Vector3(
                Random.Range(GameManager.Instance.MapMiddlePoint.position.x - _MapSize.x, GameManager.Instance.MapMiddlePoint.position.x + _MapSize.x),
                GameManager.Instance.MapMiddlePoint.position.y,
                Random.Range(GameManager.Instance.MapMiddlePoint.position.z - _MapSize.z, GameManager.Instance.MapMiddlePoint.position.z + _MapSize.z));
            if (!IsValidSpawnPosition(spawnPosition) && i < _maxSpawnRobberAttempt - 1) continue;
            Instantiate(_robber, spawnPosition, Quaternion.identity);
            break;
        }
        
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
