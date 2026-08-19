using UnityEngine;

public class AntPickupController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LeafController leaf;

    [Header("Pickup Settings")]
    [SerializeField] private float pickupRange = 2f;

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
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        // Look for an item
        LeafItem item =
            hit.collider.GetComponentInParent<LeafItem>();

        if (item == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            item.transform.position
        );

        if (distance > pickupRange)
        {
            Debug.Log("Item is too far away.");
            return;
        }

        // ==========================================
        // ITEM IS ALREADY ON THE LEAF
        // ==========================================

        if (item.IsOnLeaf)
        {
            item.RemoveFromLeaf();

            Debug.Log("Removed item from leaf.");

            return;
        }

        // ==========================================
        // ITEM IS ON THE GROUND
        // ==========================================

        Transform placementPoint =
            leaf.GetAvailablePlacementPoint();

        if (placementPoint == null)
        {
            Debug.Log("No available space on leaf.");
            return;
        }

        item.PlaceOnLeaf(placementPoint);

        Debug.Log(
            "Placed " + item.name +
            " on " + placementPoint.name
        );
    }
}