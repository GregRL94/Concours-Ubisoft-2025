using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;

public class GameGrid : MonoBehaviour
{
    public int gameGridWorldSizeX;
    public int gameGridWorldSizeZ;
    public float nodeRadius;
    [Range(0f, 1f)] public float nodeFreeTolerance;
    [Range(10f, 50f)] public float raycastCheckHeight;
    public LayerMask playerZonesMask;
    public LayerMask gameAgentsMask;
    public bool showNodesInfos;

    private static GameGrid instance;
    public static GameGrid Instance {  get { return instance; } }
    private Node[,] gameGrid;
    private int gameGridSizeX;
    private int gameGridSizeY;
    private float nodeDiameter;
    public Node[,] Grid => gameGrid;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            instance = null;
            Destroy(gameObject);
        }

        nodeDiameter = 2 * nodeRadius;
        gameGridSizeX = Mathf.RoundToInt(gameGridWorldSizeX / nodeDiameter);
        gameGridSizeY = Mathf.RoundToInt(gameGridWorldSizeZ / nodeDiameter);
        CreateGrid(gameGridSizeX, gameGridSizeY);
    }

    private void CreateGrid(int gridSizeX, int grideSizeZ)
    {
        gameGrid = new Node[gridSizeX, grideSizeZ];

        for (int i = 0; i < gridSizeX; i++)
        {
            for (int j = 0; j < grideSizeZ; j++)
            {
                Vector3 nodeWorldPos = WorldPosFromGridPos(new Vector2(i, j));
                gameGrid[i, j] = new Node(new Vector2(i, j), nodeWorldPos, IsNodeFree(nodeWorldPos), NodeInPlayerZone(nodeWorldPos));
            }
        }
    }

    public Vector3 WorldPosFromGridPos(Vector2 gridPos)
    {
        return new Vector3(gridPos.x * nodeDiameter + nodeRadius, this.transform.position.y, gridPos.y * nodeDiameter + nodeRadius);
    }

    public Node NodeFromWorldPos(Vector3 worldPos)
    {
        int i = Mathf.FloorToInt(worldPos.x) / (int)nodeDiameter;
        int j = Mathf.FloorToInt(worldPos.z) / (int)nodeDiameter;

        return gameGrid[i, j];
    }

    public Vector3 SnapToGridPos(Vector3 worldPos)
    {
        float snappedPosX = Mathf.FloorToInt(worldPos.x / nodeDiameter) * nodeDiameter + nodeRadius;
        float snappedPosZ = Mathf.FloorToInt(worldPos.z / nodeDiameter) * nodeDiameter + nodeRadius;
        return new Vector3(snappedPosX, worldPos.y, snappedPosZ);
    }

    private bool IsNodeFree(Vector3 worldPos)
    {        
        if (Physics.OverlapBox(worldPos, Vector3.one * (nodeRadius - nodeFreeTolerance * nodeRadius), Quaternion.identity, gameAgentsMask).Length > 0)
        {
            return false;
        }
        return true;
    }

    private PlayerEnum NodeInPlayerZone(Vector3 worldPos)
    {
        Ray ray = new Ray(worldPos + Vector3.up * raycastCheckHeight, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 2 * raycastCheckHeight, playerZonesMask))
        {
            PlayerZone zone = hit.collider.gameObject.GetComponent<PlayerZone>();
            if (zone != null)
            {
                return zone.playerZone;
            }
        }
        return PlayerEnum.NONE;
    }

    private void OnDrawGizmos()
    {
        if (showNodesInfos)
        {
            if (gameGrid != null)
            {
                foreach (Node node in gameGrid)
                {
                    switch (node.playerZone)
                    {
                        case PlayerEnum.NONE:
                            Gizmos.color = Color.black;
                            break;

                        case PlayerEnum.PLAYER1:
                            Gizmos.color = Color.blue;
                            break;

                        case PlayerEnum.PLAYER2:
                            Gizmos.color = Color.white;
                            break;
                    }

                    if (!node.isFree)
                    {
                        Gizmos.color = Color.red;
                    }

                    Gizmos.DrawCube(node.worldPos, Vector3.one * (nodeDiameter - nodeDiameter / 10));
                }
            }
        }
    }
}
