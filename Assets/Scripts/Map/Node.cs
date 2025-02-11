using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector2 gridPos;
    public Vector3 worldPos;
    public bool isOccupied;
    public PlayerEnum playerZone;

    public Node(Vector2 gridPos, Vector3 worldPos, bool isOccupied = false, PlayerEnum playerZone = PlayerEnum.NONE)
    {
        this.gridPos = gridPos;
        this.worldPos = worldPos;
        this.isOccupied = isOccupied;
        this.playerZone = playerZone;
    }
}
