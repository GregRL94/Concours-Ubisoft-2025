using UnityEngine;

[ExecuteInEditMode]
public class GameGridGizmosHelper : MonoBehaviour
{
    private GameGrid gameGrid;
    private int gridWorldSizeX;
    private int gridWorldSizeZ;
    private float cellRadius;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        gameGrid = GetComponent<GameGrid>();
        gridWorldSizeX = gameGrid.gameGridWorldSizeX;
        gridWorldSizeZ = gameGrid.gameGridWorldSizeZ;
        cellRadius = gameGrid.nodeRadius;

        int gizmosGridX = Mathf.RoundToInt(gridWorldSizeX / (2 * cellRadius));
        int gizmosGridZ = Mathf.RoundToInt(gridWorldSizeZ / (2 * cellRadius));

        Gizmos.DrawWireCube(new Vector3(gridWorldSizeX / 2, 0, gridWorldSizeZ / 2), new Vector3(gridWorldSizeX, 1, gridWorldSizeZ));

        for (int i = 0; i < gizmosGridX; i++)
        {
            for (int j = 0; j < gizmosGridZ; j++)
            {
                Vector3 wireCubePos = new Vector3(i * 2 * cellRadius + cellRadius, 0, j * 2 * cellRadius + cellRadius);
                Gizmos.DrawWireCube(wireCubePos, new Vector3(2 * cellRadius, 1, 2 * cellRadius));
            }
        }
    }
#endif
}
