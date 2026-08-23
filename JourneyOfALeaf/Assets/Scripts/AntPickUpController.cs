using UnityEngine;
using UnityEngine.EventSystems;

public class AntPickupController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LeafController leaf;

    [Header("Pickup Settings")]
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private LayerMask interactionLayerMask;

    private Camera mainCamera;
    private LeafItem selectedItem;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // Testing in Unity Editor
        if (Input.GetMouseButtonDown(0))
        {
            TryInteract(Input.mousePosition);
        }
    }

    // Called by mobile touch system
    public void OnScreenTap(Vector2 screenPosition)
    {
        TryInteract(screenPosition);
    }

    private void TryInteract(Vector2 screenPosition)
    {
        // Don't let taps on UI buttons/HUD leak through to the world
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactionLayerMask))
            return;

        // ==========================================
        // TAPPED AN NPC
        // ==========================================
        NPCController npc = hit.collider.GetComponentInParent<NPCController>();
        if (npc != null)
        {
            if (selectedItem == null)
            {
                Debug.Log("No item selected to give.");
                return;
            }

            if (!npc.CanAccept(selectedItem))
            {
                Debug.Log(npc.name + " doesn't need this item right now.");
                return;
            }

            npc.ReceiveItem(selectedItem);
            selectedItem = null;
            return;
        }

        // ==========================================
        // TAPPED AN ITEM
        // ==========================================
        LeafItem item = hit.collider.GetComponentInParent<LeafItem>();
        if (item == null)
            return;

        float distance = Vector3.Distance(transform.position, item.transform.position);
        if (distance > pickupRange)
        {
            Debug.Log("Item is too far away.");
            return;
        }

        if (item.IsOnLeaf)
        {
            // Tapping the currently selected item again deselects it
            if (selectedItem == item)
            {
                selectedItem = null;
                Debug.Log("Deselected " + item.name);
                return;
            }

            selectedItem = item;
            Debug.Log("Selected " + item.name + " to give.");
            return;
        }

        // Item is on the ground -> pick it up onto the leaf
        Transform placementPoint = leaf.GetAvailablePlacementPoint();
        if (placementPoint == null)
        {
            Debug.Log("No available space on leaf.");
            return;
        }

        item.PlaceOnLeaf(placementPoint);
        Debug.Log("Placed " + item.name + " on " + placementPoint.name);
    }
}