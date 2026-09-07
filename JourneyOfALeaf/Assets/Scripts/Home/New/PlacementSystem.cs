using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Grid grid;
    [SerializeField] private ObjectsDatabaseSO database;
    [SerializeField] private GameObject gridVisualization;
    [SerializeField] private AudioSource source;
    [SerializeField] private PreviewSystem preview;
    [SerializeField] private ObjectPlacer objectPlacer;
    [SerializeField] private BuildUIManager buildUIManager;
    [SerializeField] private FurnitureInventory inventory;

    private GridData furnitureData;
    private GridData floorData;
    private Vector3Int lastDetectedPosition = Vector3Int.zero;
    private Vector3Int confirmedGridPosition = Vector3Int.zero;
    private IBuildingState buildingState;

    private int currentRotationIndex = 0;
    private readonly float[] rotationAngles = { 0f, 90f, 180f, 270f };
    private int currentPlacementID = -1;
    private Vector2Int currentObjectSize = Vector2Int.one;
    private bool placementIsFree = false; // true when moving existing furniture

    private void Start()
    {
        gridVisualization.SetActive(false);
        StopPlacement();
        floorData = new();
        furnitureData = new();
    }

    // Place a new item — costs one from inventory
    public void StartPlacement(int ID)
    {
        if (!inventory.HasItem(ID))
        {
            Debug.Log($"No items of ID {ID} in inventory");
            return;
        }

        placementIsFree = false;
        BeginPlacement(ID);
    }

    // Place an existing item being moved — no inventory cost
    public void StartPlacementFree(int ID)
    {
        placementIsFree = true;
        BeginPlacement(ID);
    }

    private void BeginPlacement(int ID)
    {
        StopPlacement();
        currentPlacementID = ID;
        currentRotationIndex = 0;

        int index = database.objectsData.FindIndex(data => data.ID == ID);
        if (index >= 0)
            currentObjectSize = database.objectsData[index].Size;

        gridVisualization.SetActive(true);
        buildingState = new PlacementState(ID, grid, preview, database, floorData, furnitureData, objectPlacer);

        inputManager.EnterBuildMode();
        inputManager.OnExit += StopPlacement;
    }

    public void PlaceCurrentItem()
    {
        if (buildingState == null) return;

        // Check validity before placing
        if (!furnitureData.CanPlaceObjectAt(confirmedGridPosition, currentObjectSize))
        {
            Debug.Log("Cannot place here");
            return;
        }

        // Deduct from inventory only if not a move operation
        if (!placementIsFree)
            inventory.RemoveItem(currentPlacementID);

        buildingState.OnAction(confirmedGridPosition);

        // Tag the placed object with FurnitureInstance data
        // ObjectPlacer places the last object — find it and tag it
        StartCoroutine(TagLastPlacedObject(currentPlacementID, confirmedGridPosition, currentRotationIndex));

        buildUIManager.OnItemPlaced();
        buildUIManager.RefreshFurnitureButtons();
    }

    private System.Collections.IEnumerator TagLastPlacedObject(int id, Vector3Int gridPos, int rotation)
    {
        yield return null; // wait one frame for object to be instantiated
        // Find the most recently placed furniture by tag
        GameObject[] furniture = GameObject.FindGameObjectsWithTag("Furniture");
        foreach (var f in furniture)
        {
            FurnitureInstance fi = f.GetComponent<FurnitureInstance>();
            if (fi != null && fi.FurnitureID == 0) // uninitialized
            {
                fi.Initialize(id, gridPos, rotation);
                break;
            }
        }
    }

    // Called by FurnitureSelector when storing/moving furniture
    public void RemoveFurnitureFromGrid(Vector3Int gridPosition, int furnitureID)
    {
        int index = database.objectsData.FindIndex(d => d.ID == furnitureID);
        if (index < 0) return;

        Vector2Int size = database.objectsData[index].Size;

        // Remove all occupied cells
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                Vector3Int pos = gridPosition + new Vector3Int(x, 0, z);
                if (!furnitureData.CanPlaceObjectAt(pos, Vector2Int.one))
                    furnitureData.RemoveObjectAt(pos);
            }
        }
    }

    public void RotateCurrentItem()
    {
        if (buildingState == null || currentPlacementID == -1) return;

        currentRotationIndex = (currentRotationIndex + 1) % rotationAngles.Length;
        float angle = rotationAngles[currentRotationIndex];

        buildingState.EndState();
        buildingState = new PlacementState(currentPlacementID, grid, preview, database, floorData, furnitureData, objectPlacer);
        preview.SetPreviewRotation(Quaternion.Euler(0f, angle, 0f), currentObjectSize);
    }

    public void ForceStop() => StopPlacement();

    private void StopPlacement()
    {
        if (buildingState == null) return;

        gridVisualization.SetActive(false);
        buildingState.EndState();

        inputManager.OnClicked -= PlaceStructure;
        inputManager.OnExit -= StopPlacement;

        lastDetectedPosition = Vector3Int.zero;
        confirmedGridPosition = Vector3Int.zero;
        buildingState = null;
        currentPlacementID = -1;
        placementIsFree = false;

        if (inputManager.IsBuildModeActive)
            inputManager.ExitBuildMode();
    }

    private void PlaceStructure()
    {
        if (inputManager.IsPointerOverUI()) return;
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        buildingState.OnAction(gridPosition);
    }

    private void Update()
    {
        if (buildingState == null) return;
        if (!inputManager.IsDragging()) return;

        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        if (lastDetectedPosition != gridPosition)
        {
            buildingState.UpdateState(gridPosition);
            lastDetectedPosition = gridPosition;
            confirmedGridPosition = gridPosition;
        }
    }
}