using UnityEngine;

public class Node
{
    public Vector2 gridPos;
    public Vector3 worldPos;
    public bool isFree;
    public PlayerEnum playerZone;

    public Node(Vector2 gridPos, Vector3 worldPos, bool isFree = true, PlayerEnum playerZone = PlayerEnum.NONE)
    {
        this.gridPos = gridPos;
        this.worldPos = worldPos;
        this.isFree = isFree;
        this.playerZone = playerZone;
    }
}
