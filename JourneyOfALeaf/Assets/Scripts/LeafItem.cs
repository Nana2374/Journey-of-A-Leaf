using UnityEngine;

public class LeafItem : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private ItemData itemData;
    public ItemData Data => itemData;

    private Rigidbody rb;
    private Transform currentPlacementPoint;
    public bool IsOnLeaf => currentPlacementPoint != null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void PlaceOnLeaf(Transform placementPoint)
    {
        currentPlacementPoint = placementPoint;
        transform.SetParent(placementPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public void RemoveFromLeaf()
    {
        currentPlacementPoint = null;
        transform.SetParent(null);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
}