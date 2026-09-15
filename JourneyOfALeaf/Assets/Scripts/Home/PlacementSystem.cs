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

    private GridData furnitureData;
    private GridData floorData;
    private Vector3Int lastDetectedPosition = Vector3Int.zero;
    private Vector3Int confirmedGridPosition = Vector3Int.zero;
    private IBuildingState buildingState;

    private int currentRotationIndex = 0;
    private readonly float[] rotationAngles = { 0f, 90f, 180f, 270f };
    private int currentPlacementID = -1;
    private Vector2Int currentObjectSize = Vector2Int.one;
    private bool placementIsFree = false;

    private Dictionary<int, int> lastRotationPerID = new Dictionary<int, int>();

    private void Start()
    {
        gridVisualization.SetActive(false);
        StopPlacement();
        floorData = new();
        furnitureData = new();
    }

    public void StartPlacement(int ID)
    {
        if (!FurnitureInventory.Instance.HasItem(ID))
        {
            //Debug.Log($"No items of ID {ID} in inventory");
            return;
        }

        placementIsFree = false;
        BeginPlacement(ID);
    }

    public void StartPlacementFree(int ID)
    {
        placementIsFree = true;
        BeginPlacement(ID);
    }

    private void BeginPlacement(int ID)
    {
        StopPlacement();
        currentPlacementID = ID;

        // Restore last used rotation for this furniture ID
        currentRotationIndex = lastRotationPerID.ContainsKey(ID) ? lastRotationPerID[ID] : 0;

        int index = database.objectsData.FindIndex(data => data.ID == ID);
        if (index >= 0)
            currentObjectSize = database.objectsData[index].Size;

        Vector3Int startGridPos = GetCentreGridPosition();
        lastDetectedPosition = startGridPos;
        confirmedGridPosition = startGridPos;

        gridVisualization.SetActive(true);
        buildingState = new PlacementState(ID, grid, preview, database, floorData, furnitureData, objectPlacer, startGridPos);

        // Apply restored rotation to preview immediately
        if (currentRotationIndex != 0)
            preview.SetPreviewRotation(Quaternion.Euler(0f, currentRotationIndex * 90f, 0f), currentObjectSize);

        inputManager.EnterBuildMode();
    }

    private Vector3Int GetCentreGridPosition()
    {
        // Raycast from camera centre to find grid position
        Ray ray = Camera.main.ScreenPointToRay(
            new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            return grid.WorldToCell(hit.point);

        // Fallback — use camera position projected onto grid
        Vector3 camPos = Camera.main.transform.position;
        return grid.WorldToCell(new Vector3(camPos.x, 0f, camPos.z));
    }

    public void PlaceCurrentItem()
    {
        if (buildingState == null) return;

        if (!furnitureData.CanPlaceObjectAt(confirmedGridPosition, currentObjectSize))
            return;

        if (!placementIsFree)
            FurnitureInventory.Instance.RemoveItem(currentPlacementID);

        buildingState.OnAction(confirmedGridPosition);

        GameObject justPlaced = objectPlacer.LastPlacedObject;
        int idToTag = currentPlacementID;
        Vector3Int posToTag = confirmedGridPosition;
        int rotToTag = currentRotationIndex;
        bool hasMoreStock = !placementIsFree && FurnitureInventory.Instance.HasItem(currentPlacementID);
        int idToContinue = currentPlacementID;

        // Save rotation before stopping
        lastRotationPerID[currentPlacementID] = currentRotationIndex;

        if (justPlaced != null)
        {
            FurnitureInstance fi = justPlaced.GetComponent<FurnitureInstance>();
            if (fi != null)
                fi.ApplyRotation(rotToTag);
            else
                justPlaced.transform.rotation = Quaternion.Euler(0f, rotToTag * 90f, 0f);
        }

        StopPlacement();

        buildUIManager.OnItemPlaced();
        buildUIManager.RefreshFurnitureButtons();

        StartCoroutine(TagPlacedObject(justPlaced, idToTag, posToTag, rotToTag));

        if (hasMoreStock)
            buildUIManager.ContinuePlacement(idToContinue);
    }

    private IEnumerator TagPlacedObject(GameObject placed, int id, Vector3Int gridPos, int rotation)
    {
        yield return null;

        if (placed == null) yield break;

        FurnitureInstance fi = placed.GetComponent<FurnitureInstance>();
        if (fi == null) yield break;

        // Just store the data — rotation already applied in PlaceCurrentItem
        fi.FurnitureID = id;
        fi.GridPosition = gridPos;
        fi.RotationIndex = rotation;
    }

    public void RemoveFurnitureFromGrid(Vector3Int gridPosition, int furnitureID)
    {
        //Debug.Log($"RemoveFurnitureFromGrid: trying to remove at gridPosition={gridPosition}");

        if (!furnitureData.CanPlaceObjectAt(gridPosition, Vector2Int.one))
        {
            furnitureData.RemoveObjectAt(gridPosition);
            Debug.Log($"Successfully removed at {gridPosition}");
        }
        else
            Debug.Log($"Nothing found at {gridPosition}!");
    }

    public void RotateCurrentItem()
    {
        if (buildingState == null || currentPlacementID == -1) return;

        currentRotationIndex = (currentRotationIndex + 1) % rotationAngles.Length;
        float angle = rotationAngles[currentRotationIndex];

        // Save rotation for this furniture ID
        lastRotationPerID[currentPlacementID] = currentRotationIndex;

        buildingState.EndState();
        buildingState = new PlacementState(currentPlacementID, grid, preview, database,
            floorData, furnitureData, objectPlacer, confirmedGridPosition);
        preview.SetPreviewRotation(Quaternion.Euler(0f, angle, 0f), currentObjectSize);
    }

    public bool IsPlacing() => buildingState != null && currentPlacementID != -1;

    public void CancelPlacement()
    {
        if (buildingState == null) return;

        int idToReturn = currentPlacementID;

        StopPlacement();

        // Always return to inventory when cancelling — 
        // free placement means it was a move, item should go back to storage
        if (idToReturn != -1)
            FurnitureInventory.Instance.AddItem(idToReturn);
    }

    private void StopPlacement()
    {
        if (buildingState == null) return;

        buildingState.EndState();

        lastDetectedPosition = Vector3Int.zero;
        confirmedGridPosition = Vector3Int.zero;
        buildingState = null;
        currentPlacementID = -1;
        placementIsFree = false;

        if (inputManager.IsBuildModeActive)
            inputManager.ExitBuildMode();
    }
    public void ForceStop()
    {
        gridVisualization.SetActive(false);
        StopPlacement();
    }


    private void Update()
    {
        if (buildingState == null) return;
        if (!inputManager.IsDragging()) return;

        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        Vector3 cellWorldPos = grid.CellToWorld(gridPosition);

        //Debug.Log($"Mouse={mousePosition}, GridCell={gridPosition}, CellWorld={cellWorldPos}, PreviewPos={preview.GetPreviewObject()?.transform.position}");

        if (lastDetectedPosition != gridPosition)
        {
            buildingState.UpdateState(gridPosition);
            lastDetectedPosition = gridPosition;
            confirmedGridPosition = gridPosition;
        }
    }

    public Vector3 SnapToGrid(Vector3 worldPosition)
    {
        Vector3Int gridPos = grid.WorldToCell(worldPosition);
        return grid.CellToWorld(gridPos);
    }

    public void SaveRotationForID(int id, int rotationIndex)
    {
        lastRotationPerID[id] = rotationIndex;
    }
}