using UnityEngine;

public class LeafItem : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private ItemData itemData;
    public ItemData Data => itemData;

    private Rigidbody rb;
    private Transform currentPlacementPoint;
    private ParticleSystem[] particles;

    public bool IsOnLeaf => currentPlacementPoint != null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Get all particle systems on children
        particles = GetComponentsInChildren<ParticleSystem>();
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

        // Stop and hide particles when collected
        SetParticlesActive(false);
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

        // Re-enable particles when dropped back into world
        SetParticlesActive(true);
    }

    private void SetParticlesActive(bool active)
    {
        foreach (var ps in particles)
        {
            if (active)
                ps.Play();
            else
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}