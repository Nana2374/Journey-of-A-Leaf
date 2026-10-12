using UnityEngine;

public enum CurlAxis { Left, Right, Top }

/// <summary>
/// Lives on each of the 3 leaf edge hitboxes. Holds only the per-axis
/// curl logic; input detection/raycasting is driven externally by
/// LeafCurlInputController, matching the manual-raycast pattern used
/// by AntPickupController (no EventSystem/PhysicsRaycaster involved).
/// </summary>
public class LeafEdgeHitbox : MonoBehaviour
{
    [SerializeField] private LeafController leaf;
    [SerializeField] private CurlAxis axis;

    [Header("Tap-and-hold toggle")]
    [SerializeField] private float holdThreshold = 0.25f;

    [Header("Top-only drag-to-hand")]
    [SerializeField] private float dragStartDistance = 20f;
    [SerializeField] private float dragDistanceForFullCurl = 300f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    public CurlAxis Axis => axis;

    private float pointerDownTime;
    private Vector2 pointerDownPos;
    private bool isDragging;
    private bool isCurled;

    private void Awake()
    {
        if (leaf == null)
            Debug.LogError($"[LeafEdgeHitbox:{axis}] 'Leaf' reference is not assigned in the Inspector.");

        if (GetComponent<Collider>() == null)
            Debug.LogError($"[LeafEdgeHitbox:{axis}] No Collider found on this GameObject. Raycasts will never hit it.");
    }

    public void HandlePointerDown(Vector2 screenPos)
    {
        pointerDownTime = Time.time;
        pointerDownPos = screenPos;
        isDragging = false;

        if (debugLogs) Debug.Log($"[LeafEdgeHitbox:{axis}] Down at {screenPos}");
    }

    public void HandleDrag(Vector2 screenPos)
    {
        if (axis != CurlAxis.Top)
            return; // only Top supports drag-to-hand

        float dragDistance = Vector2.Distance(screenPos, pointerDownPos);

        if (!isDragging && dragDistance < dragStartDistance)
            return;

        isDragging = true;

        float t = dragDistance / dragDistanceForFullCurl;
        if (debugLogs) Debug.Log($"[LeafEdgeHitbox:{axis}] Dragging, distance {dragDistance:F1}px -> t={t:F2}");
        leaf.SetTopCurl(t);
    }

    public void HandlePointerUp()
    {
        if (debugLogs) Debug.Log($"[LeafEdgeHitbox:{axis}] Up, isDragging={isDragging}");

        if (axis == CurlAxis.Top && isDragging)
        {
            if (!leaf.IsTipAttachedToHand)
                leaf.ReleaseTop();

            isDragging = false;
            return;
        }

        float heldDuration = Time.time - pointerDownTime;
        if (heldDuration < holdThreshold)
            return;

        isCurled = !isCurled;
        float target = isCurled ? 1f : 0f;

        if (debugLogs) Debug.Log($"[LeafEdgeHitbox:{axis}] Toggled -> isCurled={isCurled}");

        switch (axis)
        {
            case CurlAxis.Left:
                if (isCurled) leaf.SetLeftCurl(target); else leaf.ReleaseLeft();
                break;
            case CurlAxis.Right:
                if (isCurled) leaf.SetRightCurl(target); else leaf.ReleaseRight();
                break;
            case CurlAxis.Top:
                if (isCurled) leaf.SetTopCurl(target); else leaf.ReleaseTop();
                break;
        }
    }

    // Called externally if a drag/hold gets interrupted (e.g. another system
    // claims the touch mid-gesture) so state doesn't get stuck.
    public void CancelInteraction()
    {
        isDragging = false;
    }
}