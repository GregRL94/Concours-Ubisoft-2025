using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    //stock all players at start of game
    [SerializeField]
    private GameObject[] _players;
    public GameObject[] Players => _players;
    [SerializeField, Tooltip("Middle of the map")]
    private Transform _mapMiddlePoint;
    public Transform MapMiddlePoint => _mapMiddlePoint;
    
    [SerializeField]
    private Transform[] _mapCorners;
    public Transform[] MapCorners => _mapCorners;

    public static GameManager Instance;
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
    }
}
