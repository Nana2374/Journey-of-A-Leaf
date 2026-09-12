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
        {
            // Tapped empty space - dismiss any open drop prompt
            ItemDropPromptUI.Instance?.Hide();
            return;
        }

        // ==========================================
        // TAPPED AN ITEM
        // ==========================================
        LeafItem item = hit.collider.GetComponentInParent<LeafItem>();
        if (item == null)
        {
            ItemDropPromptUI.Instance?.Hide();
            return;
        }

        float distance = Vector3.Distance(transform.position, item.transform.position);
        if (distance > pickupRange)
        {
            Debug.Log("Item is too far away.");
            return;
        }

        if (item.IsOnLeaf)
        {
            // Tapping the item again while its Drop prompt is showing dismisses it;
            // otherwise show the Drop prompt for this item.
            if (ItemDropPromptUI.Instance != null && ItemDropPromptUI.Instance.IsShowingFor(item))
            {
                ItemDropPromptUI.Instance.Hide();
            }
            else
            {
                ItemDropPromptUI.Instance?.Show(item);
            }
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