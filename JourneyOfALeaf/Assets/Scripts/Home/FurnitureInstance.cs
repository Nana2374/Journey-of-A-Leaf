using UnityEngine;

public class FurnitureInstance : MonoBehaviour
{
    public int FurnitureID;
    public Vector3Int GridPosition;
    public int RotationIndex;

    // Called when player collects/stores this piece
    public void Initialize(int id, Vector3Int gridPos, int rotation)
    {
        FurnitureID = id;
        GridPosition = gridPos;
        RotationIndex = rotation;
    }
}
