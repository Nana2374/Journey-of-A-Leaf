using UnityEngine;
using UnityEngine.InputSystem;

public class FurnitureSelector : MonoBehaviour
{
    [Header("References")]
    public InputManager inputManager;
    public BuildUIManager buildUIManager;
    public PlacementSystem placementSystem;
    public FurnitureInventory inventory;
    public LayerMask furnitureLayerMask;

    [SerializeField] private Camera sceneCamera;

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

        if (Pointer.current != null && Pointer.current.press.wasReleasedThisFrame)
            TrySelectFurniture();
    }

    private void TrySelectFurniture()
    {
        if (Pointer.current == null) return;

        Vector2 screenPos = Pointer.current.position.ReadValue();
        Ray ray = sceneCamera.ScreenPointToRay(screenPos);

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

    // Store selected furniture back to inventory
    public void StoreSelected()
    {
        if (selectedFurniture == null) return;

        FurnitureInstance instance = selectedFurniture.GetComponent<FurnitureInstance>();
        if (instance != null)
        {
            // Free up grid cells
            placementSystem.RemoveFurnitureFromGrid(instance.GridPosition, instance.FurnitureID);
            // Add back to inventory
            inventory.AddItem(instance.FurnitureID);
        }

        Destroy(selectedFurniture);
        selectedFurniture = null;
        buildUIManager.HideActionBar();
        buildUIManager.RefreshFurnitureButtons();
    }

    // Pick up to move — starts placement preview again
    public void MoveSelected()
    {
        if (selectedFurniture == null) return;

        FurnitureInstance instance = selectedFurniture.GetComponent<FurnitureInstance>();
        if (instance == null) return;

        int id = instance.FurnitureID;

        // Free grid cells and destroy old object
        placementSystem.RemoveFurnitureFromGrid(instance.GridPosition, id);
        Destroy(selectedFurniture);
        selectedFurniture = null;

        // Start placement again with same ID — no inventory cost since it's already placed
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
