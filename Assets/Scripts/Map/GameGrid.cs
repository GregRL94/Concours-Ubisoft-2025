using UnityEngine;

public class GameGrid : MonoBehaviour
{
    #region Variables
    private static GameGrid instance;

    [Header("GAME MAP SIZE")]
    [SerializeField] private int gameGridWorldSizeX;    
    [SerializeField] private int gameGridWorldSizeZ;    
    [Space]
    [Header("GRID CELL PARAMETERS")]
    [SerializeField] private float nodeRadius;
    [SerializeField, Range(0f, 1f)] private float nodeFreeTolerance;
    [SerializeField, Range(10f, 50f)] private float raycastCheckHeight;
    [SerializeField] private bool showNodesInfos;
    [Space]
    [Header("GAME OBJECTS LAYER MASK")]
    [SerializeField] private LayerMask playerZonesMask;
    [SerializeField] private LayerMask gameAgentsMask;

    private Node[,] gameGrid;
    private int gameGridSizeX;
    private int gameGridSizeY;
    private float nodeDiameter;
    #endregion

    #region Getters
    public static GameGrid Instance { get { return instance; } }
    public int GameGridWorldSizeX => gameGridWorldSizeX;
    public int GameGridWorldSizeZ => gameGridWorldSizeZ;
    public float NodeRadius => nodeRadius;
    public Node[,] Grid => gameGrid;
    public int GameGridSizeX => gameGridSizeX;
    public int GameGridSizeY => gameGridSizeY;
    #endregion

    #region MonoBehaviour Flow
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
    #endregion

    #region Grid Functions
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

    private void BakeGrid()
    {
        foreach(Node node in gameGrid)
        {
            node.isFree = IsNodeFree(node.worldPos);
        }
    }
    public Vector3 WorldPosFromGridPos(Vector2 gridPos)
    {
        return new Vector3(gridPos.x * nodeDiameter + nodeRadius, this.transform.position.y, gridPos.y * nodeDiameter + nodeRadius);
    }

    public Vector3 SnapToGridPos(Vector3 worldPos)
    {
        float snappedPosX = Mathf.FloorToInt(worldPos.x / nodeDiameter) * nodeDiameter + nodeRadius;
        float snappedPosZ = Mathf.FloorToInt(worldPos.z / nodeDiameter) * nodeDiameter + nodeRadius;
        return new Vector3(snappedPosX, worldPos.y, snappedPosZ);
    }

    public void UpdateAtWorldPos(Vector3 worldPos)
    {
        NodeFromWorldPos(worldPos).isFree = IsNodeFree(worldPos);
    }
    #endregion

    #region Node Functions
    public Node NodeFromWorldPos(Vector3 worldPos)
    {
        int i = Mathf.FloorToInt(worldPos.x) / (int)nodeDiameter;
        int j = Mathf.FloorToInt(worldPos.z) / (int)nodeDiameter;

        return gameGrid[i, j];
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

    public void UpdateNode(Node node)
    {
        node.isFree = IsNodeFree(node.worldPos);
    }
    #endregion

    #region Gizmos
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

                    Gizmos.DrawCube(node.worldPos, new Vector3(nodeDiameter - nodeDiameter / 10, 1, nodeDiameter - nodeDiameter / 10));
                }
            }
        }
    }
    #endregion
}
