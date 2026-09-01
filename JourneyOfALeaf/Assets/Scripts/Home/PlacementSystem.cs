using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class PlacementSystem : MonoBehaviour
{
    [SerializeField]
    private InputManager inputManager;

    [SerializeField]
    private GameObject mouseIndicator, cellIndicator;

    [SerializeField]
    private Grid grid;

    [SerializeField]
    private ObjectsDatabaseSO database;
    private int selectedObjectIndex = -1;

    [SerializeField]
    private GameObject gridVisualization;


    //[SerializeField]
    //private AudioClip correctPlacementClip, wrongPlacementClip;

    [SerializeField]
    private AudioSource source;

    private GridData furnitureData;
    private GridData floorData;

    private Renderer previewRenderer;

    private List<GameObject> placedGameObject = new();

    //[SerializeField]
    //private PreviewSystem preview;
    //private Vector3Int lastDetectedPosition = Vector3Int.zero;
    //[SerializeField]
    //private ObjectPlacer objectPlacer;
    //IBuildingState buildingState;
    //[SerializeField]
    //private SoundFeedback soundFeedback;


    private void Start()
    {
        StopPlacement();
        //   gridVisualization.SetActive(false);
        floorData = new();
        furnitureData = new();
        previewRenderer = cellIndicator.GetComponentInChildren<Renderer>();
    }

    public void StartPlacement(int ID)
    {
        StopPlacement();
        selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == ID);
        if (selectedObjectIndex < 0)
        {
            Debug.LogError($"No ID found {ID}");
            return;
        }

        gridVisualization.SetActive(true);
        cellIndicator.SetActive(true);

        //    buildingState = new PlacementState(ID,
        //                                       grid,
        //                                       preview,
        //                                       database,
        //                                       floorData,
        //                                       furnitureData,
        //                                       objectPlacer,
        //                                       soundFeedback);

        inputManager.EnterBuildMode();

        inputManager.OnClicked += PlaceStructure;
        inputManager.OnExit += StopPlacement;
        Debug.Log("Subscribed to OnClicked");
        //}
        //public void StartRemoving()
        //{
        //    StopPlacement();
        //    gridVisualization.SetActive(true);
        //    buildingState = new RemovingState(grid, preview, floorData, furnitureData, objectPlacer, soundFeedback);
        //    inputManager.OnClicked += PlaceStructure;
        //    inputManager.OnExit += StopPlacement;
    }

    private void PlaceStructure()
    {
        Debug.Log("PlaceStructure called");

        if (inputManager.IsPointerOverUI())
        {
            var pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            pointerData.position = UnityEngine.InputSystem.Pointer.current.position.ReadValue();

            var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);

            foreach (var r in results)
                Debug.Log($"Blocked by UI element: {r.gameObject.name}");

            Debug.Log("Blocked: pointer is over UI");
            return;
        }

        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        Debug.Log($"Grid position: {gridPosition}");

        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        Debug.Log($"Placement validity: {placementValidity}");
        if (placementValidity == false)
            return;

        source.Play();
        GameObject newObject = Instantiate(database.objectsData[selectedObjectIndex].Prefab);
        newObject.transform.position = grid.CellToWorld(gridPosition);
        Debug.Log($"Instantiated at {newObject.transform.position}");

        placedGameObject.Add(newObject);
        GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ?
                floorData :
                furnitureData;

        selectedData.AddObjectAt(gridPosition,
            database.objectsData[selectedObjectIndex].Size,
            database.objectsData[selectedObjectIndex].ID,
            placedGameObject.Count - 1);

        Debug.Log("item placed");



        //    buildingState.OnAction(gridPosition);
        //}
        //private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
        //{
        //    GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ? 
        //        floorData : 
        //        furnitureData;
        //    return selectedData.CanPlaceObejctAt(gridPosition, database.objectsData[selectedObjectIndex].Size);
    }
    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ?
                floorData :
                furnitureData;
        return selectedData.CanPlaceObejctAt(gridPosition, database.objectsData[selectedObjectIndex].Size);
    }

    private void StopPlacement()
    {
        Debug.Log("StopPlacement called");
        selectedObjectIndex = -1;

        //    soundFeedback.PlaySound(SoundType.Click);
        //    if (buildingState == null)
        //        return;
        gridVisualization.SetActive(false);
        cellIndicator.SetActive(false);
        //    buildingState.EndState();
        inputManager.OnClicked -= PlaceStructure;
        inputManager.OnExit -= StopPlacement;

        //    lastDetectedPosition = Vector3Int.zero;
        //    buildingState = null;

        if (inputManager.IsBuildModeActive)
            inputManager.ExitBuildMode();
    }
    private void Update()
    {
        //if (buildingState == null)
        //    return;

        if (selectedObjectIndex < 0)
            return;

        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        previewRenderer.material.color = placementValidity ? Color.green : Color.red;

        mouseIndicator.transform.position = mousePosition;
        cellIndicator.transform.position = grid.CellToWorld(gridPosition);
        //if (lastDetectedPosition != gridPosition)
        //{
        //    buildingState.UpdateState(gridPosition);
        //    lastDetectedPosition = gridPosition;
        //}

    }
}