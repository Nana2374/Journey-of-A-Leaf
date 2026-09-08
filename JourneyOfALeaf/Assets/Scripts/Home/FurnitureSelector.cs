using UnityEngine;
using UnityEngine.InputSystem;

public class FurnitureSelector : MonoBehaviour
{
    [Header("References")]
    public InputManager inputManager;
    public BuildUIManager buildUIManager;
    public PlacementSystem placementSystem;
    public LayerMask furnitureLayerMask;

    // No separate camera field needed — gets active camera automatically
    private GameObject selectedFurniture = null;
    private bool isSelectionMode = false;

    public GameObject SelectedFurniture => selectedFurniture;

    public void EnterSelectionMode()
    {
        isSelectionMode = true;
        selectedFurniture = null;
    }

    public void ExitSelectionMode()
    {
        isSelectionMode = false;
        Deselect();
    }

    void Update()
    {
        if (!isSelectionMode) return;
        if (inputManager.IsDragging()) return;

        // Don't try to select furniture while placement is active
        if (placementSystem.IsPlacing()) return;

        if (Pointer.current != null && Pointer.current.press.wasReleasedThisFrame)
            TrySelectFurniture();
    }

    private void TrySelectFurniture()
    {
        if (Pointer.current == null) return;

        Vector2 screenPos = Pointer.current.position.ReadValue();

        // Use Camera.main which will be the active Cinemachine brain camera
        // This correctly uses whichever virtual camera has highest priority
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, furnitureLayerMask))
            SelectFurniture(hit.collider.gameObject);
        else
            Deselect();
    }

    public void SelectFurniture(GameObject furniture)
    {
        selectedFurniture = furniture;
        buildUIManager.ShowActionBarOnFurniture(furniture);
    }

    public void Deselect()
    {
        selectedFurniture = null;
        buildUIManager.HideActionBar();
    }

    public void StoreSelected()
    {
        if (selectedFurniture == null)
        {
            Debug.Log("StoreSelected: selectedFurniture is null!");
            return;
        }

        FurnitureInstance instance = selectedFurniture.GetComponent<FurnitureInstance>();

        if (instance == null)
        {
            Debug.Log($"StoreSelected: No FurnitureInstance component found on {selectedFurniture.name}!");
            // Still destroy and return
            Destroy(selectedFurniture);
            selectedFurniture = null;
            buildUIManager.HideActionBar();
            return;
        }

        Debug.Log($"StoreSelected: Storing ID={instance.FurnitureID}");

        placementSystem.RemoveFurnitureFromGrid(instance.GridPosition, instance.FurnitureID);
        FurnitureInventory.Instance.AddItem(instance.FurnitureID);

        Destroy(selectedFurniture);
        selectedFurniture = null;
        buildUIManager.HideActionBar();
        buildUIManager.RefreshFurnitureButtons();
    }

    public void MoveSelected()
    {
        if (selectedFurniture == null) return;

        FurnitureInstance instance = selectedFurniture.GetComponent<FurnitureInstance>();
        if (instance == null) return;

        int id = instance.FurnitureID;
        placementSystem.RemoveFurnitureFromGrid(instance.GridPosition, id);
        Destroy(selectedFurniture);
        selectedFurniture = null;

        placementSystem.StartPlacementFree(id);
        buildUIManager.HideActionBar();
    }

    public void RotateSelected()
    {
        if (selectedFurniture == null) return;
        selectedFurniture.transform.Rotate(0f, 90f, 0f);

        FurnitureInstance instance = selectedFurniture.GetComponent<FurnitureInstance>();
        if (instance != null)
            instance.RotationIndex = (instance.RotationIndex + 1) % 4;

        buildUIManager.ShowActionBarOnFurniture(selectedFurniture);
    }
}