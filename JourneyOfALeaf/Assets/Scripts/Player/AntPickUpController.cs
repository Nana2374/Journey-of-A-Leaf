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
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                TryInteract(touch.position, touch.fingerId);
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            TryInteract(Input.mousePosition, -1);
        }
    }

    // Called by mobile touch system
    public void OnScreenTap(Vector2 screenPosition, int fingerId)
    {
        TryInteract(screenPosition, fingerId);
    }

    private void TryInteract(Vector2 screenPosition, int pointerId)
    {
        // Don't let taps on UI buttons/HUD leak through to the world
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId))
            return;

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactionLayerMask))
        {
            ItemDropPromptUI.Instance?.Hide();
            return;
        }

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