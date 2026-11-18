using UnityEngine;

public class SlopeAligner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform modelRoot;
    [SerializeField] private CharacterController controller;
    [SerializeField] private Player_Movement playerMovement;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float rayExtraLength = 0.4f;

    [Header("Tilt")]
    [SerializeField] private float maxTiltAngle = 30f;
    [SerializeField] private float alignSpeed = 8f;

    private readonly RaycastHit[] hits = new RaycastHit[8];

    private void LateUpdate()
    {
        // Gliding / in water: snap upright immediately, facing direction only
        if (playerMovement.IsGliding || playerMovement.IsInWater)
        {
            modelRoot.localRotation = Quaternion.identity;
            return;
        }

        Quaternion targetLocal = Quaternion.identity;

        if (TryGetGroundNormal(out Vector3 normal))
        {
            // Work in the Player's local space so yaw is never touched
            Vector3 localNormal = transform.InverseTransformDirection(normal);

            localNormal = Vector3.RotateTowards(
                Vector3.up, localNormal, maxTiltAngle * Mathf.Deg2Rad, 0f);

            targetLocal = Quaternion.FromToRotation(Vector3.up, localNormal);
        }

        float t = 1f - Mathf.Exp(-alignSpeed * Time.deltaTime);
        modelRoot.localRotation = Quaternion.Slerp(modelRoot.localRotation, targetLocal, t);
    }

    private bool TryGetGroundNormal(out Vector3 normal)
    {
        normal = Vector3.up;

        Bounds b = controller.bounds;
        float length = b.extents.y + rayExtraLength;

        int count = Physics.RaycastNonAlloc(
            b.center, Vector3.down, hits, length, groundMask,
            QueryTriggerInteraction.Ignore);

        float nearest = float.MaxValue;
        bool found = false;

        for (int i = 0; i < count; i++)
        {
            // Skip anything that belongs to the player (capsule, leaf, items...)
            if (hits[i].collider.transform.IsChildOf(transform))
                continue;

            if (hits[i].distance < nearest)
            {
                nearest = hits[i].distance;
                normal = hits[i].normal;
                found = true;
            }
        }

        return found;
    }
}