using UnityEngine;

public class AntPickupController : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private Transform carryPoint;
    [SerializeField] private float pickupRange = 2f;

    private PickupItem currentItem;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // For testing in the Unity Editor
        if (Input.GetMouseButtonDown(0))
        {
            TryInteract(Input.mousePosition);
        }
    }

    public void OnScreenTap(Vector2 screenPosition)
    {
        TryInteract(screenPosition);
    }

    private void TryInteract(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            PickupItem item = hit.collider.GetComponentInParent<PickupItem>();

            if (item == null)
                return;

            // If already carrying something, drop it
            if (currentItem != null)
            {
                DropItem();
                return;
            }

            // Check if the ant is close enough
            float distance = Vector3.Distance(transform.position, item.transform.position);

            if (distance <= pickupRange)
            {
                PickUpItem(item);
            }
        }
    }

    private void PickUpItem(PickupItem item)
    {
        currentItem = item;

        Rigidbody rb = item.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        item.transform.SetParent(carryPoint);

        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
    }

    public void DropItem()
    {
        if (currentItem == null)
            return;

        currentItem.transform.SetParent(null);

        Rigidbody rb = currentItem.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        currentItem = null;
    }

    public bool IsCarrying()
    {
        return currentItem != null;
    }
}