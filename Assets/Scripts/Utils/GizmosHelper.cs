using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmosHelper : MonoBehaviour
{
    private GameGrid grid = GameGrid.Instance;

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < grid.GameGridSizeX; i++)
        {
            for (int j = 0; j < grid.GameGridSizeY; j++)
            {
                Vector3 wireCubePos = grid.WorldPosFromGridPos(new Vector2(i, j));
                Gizmos.DrawWireCube(wireCubePos, Vector3.one * grid.NodeDiameter);
            }
        }
    }
}
