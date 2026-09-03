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
    [SerializeField] private AudioClip correctPlacementClip, wrongPlacementClip;
    [SerializeField] private AudioSource source;
    [SerializeField] private PreviewSystem preview;
    [SerializeField] private ObjectPlacer objectPlacer;

    private GridData furnitureData;
    private GridData floorData;
    private Vector3Int lastDetectedPosition = Vector3Int.zero;
    private Vector3Int confirmedGridPosition = Vector3Int.zero; // last dragged position
    private IBuildingState buildingState;

    private int currentRotationIndex = 0;
    private readonly float[] rotationAngles = { 0f, 90f, 180f, 270f }; // 90 degree steps
    private int currentPlacementID = -1;
    private Vector2Int currentObjectSize = Vector2Int.one;

    private void Start()
    {
        gridVisualization.SetActive(false);
        StopPlacement();
        floorData = new();
        furnitureData = new();
    }

    public void StartPlacement(int ID)
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

    public void StartRemoving()
    {
        StopPlacement();
        gridVisualization.SetActive(true);
        buildingState = new RemovingState(grid, preview, floorData, furnitureData, objectPlacer);

        inputManager.EnterBuildMode();
        inputManager.OnClicked += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    // Called by Place button — uses last dragged position, not pointer position
    public void PlaceCurrentItem()
    {
        if (buildingState == null) return;
        buildingState.OnAction(confirmedGridPosition);
    }

    // Called by Rotate button
    public void RotateCurrentItem()
    {
        if (buildingState == null || currentPlacementID == -1) return;

        currentRotationIndex = (currentRotationIndex + 1) % rotationAngles.Length;
        float angle = rotationAngles[currentRotationIndex];

        buildingState.EndState();
        buildingState = new PlacementState(currentPlacementID, grid, preview, database, floorData, furnitureData, objectPlacer);

        // Rotate around centre by offsetting by half size
        preview.SetPreviewRotation(Quaternion.Euler(0f, angle, 0f), currentObjectSize);
    }

    public void ForceStop() => StopPlacement();

    private void PlaceStructure()
    {
        if (inputManager.IsPointerOverUI()) return;

        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        buildingState.OnAction(gridPosition);
    }

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

        if (inputManager.IsBuildModeActive)
            inputManager.ExitBuildMode();
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
            confirmedGridPosition = gridPosition; // save last dragged position
        }
    }
}