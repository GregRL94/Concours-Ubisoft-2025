using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGrid : MonoBehaviour
{
    public int gameGridWorldSizeX;
    public int gameGridWorldSizeZ;
    public int nodeRadius;
    public bool drawGizmos;

    private static GameGrid instance;
    private Node[,] gameGrid;
    private int gameGridSizeX;
    private int gameGridSizeY;
    private int nodeDiameter;

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
        gameGridSizeX = gameGridWorldSizeX / nodeDiameter;
        gameGridSizeY = gameGridWorldSizeZ / nodeDiameter;
        CreateGrid(gameGridSizeX, gameGridSizeY);
    }

    private void CreateGrid(int gridSizeX, int grideSizeZ)
    {
        gameGrid = new Node[gridSizeX, grideSizeZ];

        for (int i = 0; i < gridSizeX; i++)
        {
            for (int j = 0; j < grideSizeZ; j++)
            {                
                Vector2 nodeGridPos = new Vector2(i, j);
                Debug.Log(nodeGridPos);
                gameGrid[i, j] = new Node(nodeGridPos, WorldPosFromGridPos(nodeGridPos));
            }
        }
    }

    private Vector3 WorldPosFromGridPos(Vector2 gridPos)
    {
        return new Vector3(gridPos.x * nodeDiameter + nodeRadius, 0, gridPos.y * nodeDiameter + nodeRadius);
    }

    public Node NodeFromWorldPos(Vector3 worldPos)
    {
        int i = Mathf.FloorToInt(worldPos.x) / nodeDiameter;
        int j = Mathf.FloorToInt(worldPos.z) / nodeDiameter;

        return gameGrid[i, j];
    }

    public Vector3 SnapToGridPos(Vector3 worldPos)
    {
        float snappedPosX = Mathf.FloorToInt(worldPos.x / nodeDiameter) * nodeDiameter + nodeRadius;
        float snappedPosZ = Mathf.FloorToInt(worldPos.z / nodeDiameter) * nodeDiameter + nodeRadius;
        return new Vector3(snappedPosX, worldPos.y, snappedPosZ);
    }

    private void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            Gizmos.DrawWireCube(new Vector3(gameGridWorldSizeX / 2, 0, gameGridWorldSizeZ / 2), new Vector3(gameGridWorldSizeX, 1, gameGridWorldSizeZ));
            if (gameGrid != null)
            {
                foreach (Node node in gameGrid)
                {
                    Gizmos.DrawCube(node.worldPos, Vector3.one * (nodeDiameter - nodeDiameter / 10));
                }
            }
        }
    }
}
