using UnityEngine;

public class FurnitureInstance : MonoBehaviour
{
    public int FurnitureID;
    public Vector3Int GridPosition;
    public int RotationIndex;

    [Tooltip("Drag the child mesh object here — the one with the centred pivot")]
    public Transform modelTransform; // drag your child model here in Inspector

    public void Initialize(int id, Vector3Int gridPos, int rotation)
    {
        FurnitureID = id;
        GridPosition = gridPos;
        RotationIndex = rotation;
    }

    public void ApplyRotation(int rotationIndex)
    {
        Transform target = modelTransform != null ? modelTransform : transform;
        target.localRotation = Quaternion.Euler(0f, rotationIndex * 90f, 0f);
    }
}