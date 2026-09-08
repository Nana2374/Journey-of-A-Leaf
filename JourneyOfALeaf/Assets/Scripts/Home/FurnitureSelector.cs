using UnityEngine;
using UnityEngine.InputSystem;

public class FurnitureSelector : MonoBehaviour
{
    [Header("References")]
    public InputManager inputManager;
    public BuildUIManager buildUIManager;
    public PlacementSystem placementSystem;
    public LayerMask furnitureLayerMask;

    private GameObject selectedFurniture = null;
    private bool isSelectionMode = false;
    private bool isDragMoving = false;

    public GameObject SelectedFurniture => selectedFurniture;

    public void EnterSelectionMode()
    {
        isSelectionMode = true;
        selectedFurniture = null;
        isDragMoving = false;
    }

    public void ExitSelectionMode()
    {
        isSelectionMode = false;
        isDragMoving = false;
        Deselect();
    }

    void Update()
    {
        if (!isSelectionMode) return;
        if (placementSystem.IsPlacing()) return;

        // Debug every frame when furniture is selected
        if (selectedFurniture != null)
        {
            Debug.Log($"Furniture selected: {selectedFurniture.name}, IsDragging: {inputManager.IsDragging()}, isDragMoving: {isDragMoving}");
        }

        // If furniture is selected and player starts dragging, auto-enter move mode
        if (selectedFurniture != null && inputManager.IsDragging() && !isDragMoving)
        {
            Debug.Log("Drag detected on selected furniture — entering move mode");
            isDragMoving = true;
            MoveSelected();
            return;
        }

        // Reset drag flag when finger lifts
        if (Pointer.current != null && Pointer.current.press.wasReleasedThisFrame)
        {
            Debug.Log($"Pointer released. isDragging={inputManager.IsDragging()}, isDragMoving={isDragMoving}");
            isDragMoving = false;

            if (!inputManager.IsDragging())
                TrySelectFurniture();
        }
    }

    private void TrySelectFurniture()
    {
        if (Pointer.current == null) return;

        Vector2 screenPos = Pointer.current.position.ReadValue();
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
        isDragMoving = false;
        buildUIManager.HideActionBar();
    }

    public void StoreSelected()
    {
        Debug.Log($"StoreSelected called. selectedFurniture={selectedFurniture?.name ?? "null"}");

        if (selectedFurniture == null)
        {
            Debug.Log("StoreSelected: selectedFurniture is null!");
            return;
        }

        FurnitureInstance instance = selectedFurniture.GetComponent<FurnitureInstance>();

        if (instance == null)
        {
            Debug.Log($"StoreSelected: No FurnitureInstance on {selectedFurniture.name}!");
            Destroy(selectedFurniture);
            selectedFurniture = null;
            buildUIManager.HideActionBar();
            return;
        }

        Debug.Log($"StoreSelected: ID={instance.FurnitureID}, GridPos={instance.GridPosition}");
        Debug.Log($"Inventory before: {FurnitureInventory.Instance.GetQuantity(instance.FurnitureID)}");

        placementSystem.RemoveFurnitureFromGrid(instance.GridPosition, instance.FurnitureID);
        FurnitureInventory.Instance.AddItem(instance.FurnitureID);

        Debug.Log($"Inventory after: {FurnitureInventory.Instance.GetQuantity(instance.FurnitureID)}");

        Destroy(selectedFurniture);
        selectedFurniture = null;
        isDragMoving = false;
        buildUIManager.HideActionBar();
        buildUIManager.RefreshFurnitureButtons();
    }

    public void MoveSelected()
    {
        if (selectedFurniture == null)
        {
            Debug.Log("MoveSelected: selectedFurniture is null!");
            return;
        }

        FurnitureInstance instance = selectedFurniture.GetComponent<FurnitureInstance>();
        if (instance == null)
        {
            Debug.Log("MoveSelected: No FurnitureInstance found!");
            return;
        }

        int id = instance.FurnitureID;
        placementSystem.RemoveFurnitureFromGrid(instance.GridPosition, id);
        Destroy(selectedFurniture);
        selectedFurniture = null;
        isDragMoving = false;

        // Enter free placement — no inventory cost since already owned
        Debug.Log($"Moving furniture ID={instance.FurnitureID} from GridPos={instance.GridPosition}");
        placementSystem.StartPlacementFree(id);

        // Update action bar to show Place button
        var placeText = buildUIManager.placeButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (placeText != null) placeText.text = "Place";

        buildUIManager.placeButton.gameObject.SetActive(true);
        buildUIManager.rotateButton.gameObject.SetActive(true);
        buildUIManager.storeButton.gameObject.SetActive(true);
        buildUIManager.actionBarFollower.StopTracking();
        buildUIManager.StartCoroutine_ShowActionBarNextFrame();
    }

    public void RotateSelected()
    {
        if (selectedFurniture == null) return;

        Bounds bounds = new Bounds(selectedFurniture.transform.position, Vector3.zero);
        Renderer[] renderers = selectedFurniture.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);
        Vector3 centre = bounds.center;

        selectedFurniture.transform.RotateAround(centre, Vector3.up, 90f);

        // Snap back to grid
        selectedFurniture.transform.position = placementSystem.SnapToGrid(
            selectedFurniture.transform.position);

        FurnitureInstance instance = selectedFurniture.GetComponent<FurnitureInstance>();
        if (instance != null)
            instance.RotationIndex = (instance.RotationIndex + 1) % 4;

        buildUIManager.ShowActionBarOnFurniture(selectedFurniture);
    }
}