using UnityEngine;
using UnityEngine.EventSystems;

public enum CurlAxis { Left, Right, Top }

public class LeafEdgeHitbox : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private LeafController leaf;
    [SerializeField] private CurlAxis axis;

    [Header("Tap-and-hold toggle")]
    [SerializeField] private float holdThreshold = 0.25f; // seconds pointer must stay down to count as a hold

    [Header("Top-only drag-to-hand")]
    [SerializeField] private float dragStartDistance = 20f;      // px of movement before it counts as a drag, not a hold
    [SerializeField] private float dragDistanceForFullCurl = 300f; // px of drag that maps to fully curled/attached

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private float pointerDownTime;
    private Vector2 pointerDownPos;
    private bool isDragging;
    private bool isCurled; // toggle state, used when NOT dragging

    private void Awake()
    {
        if (leaf == null)
            Debug.LogError($"[LeafEdgeHitbox:{axis}] 'Leaf' reference is not assigned in the Inspector. Curling will not work.");

        if (GetComponent<Collider>() == null)
            Debug.LogError($"[LeafEdgeHitbox:{axis}] No Collider found on this GameObject. Taps will never be detected.");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownTime = Time.time;
        pointerDownPos = eventData.position;
        isDragging = false;

        if (debugLogs) Debug.Log($"[LeafEdgeHitbox:{axis}] OnPointerDown at {eventData.position}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (axis != CurlAxis.Top)
        {
            if (debugLogs) Debug.Log($"[LeafEdgeHitbox:{axis}] OnDrag ignored (only Top supports dragging)");
            return;
        }

        float dragDistance = Vector2.Distance(eventData.position, pointerDownPos);

        if (!isDragging && dragDistance < dragStartDistance)
        {
            if (debugLogs) Debug.Log($"[LeafEdgeHitbox:{axis}] Movement {dragDistance:F1}px, below drag threshold {dragStartDistance}px");
            return;
        }

        isDragging = true;

        float t = dragDistance / dragDistanceForFullCurl;
        if (debugLogs) Debug.Log($"[LeafEdgeHitbox:{axis}] Dragging, distance {dragDistance:F1}px -> t={t:F2}");
        leaf.SetTopCurl(t);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (debugLogs) Debug.Log($"[LeafEdgeHitbox:{axis}] OnPointerUp, isDragging={isDragging}");

        if (axis == CurlAxis.Top && isDragging)
        {
            if (!leaf.IsTipAttachedToHand)
                leaf.ReleaseTop();

            isDragging = false;
            return;
        }

        float heldDuration = Time.time - pointerDownTime;
        if (debugLogs) Debug.Log($"[LeafEdgeHitbox:{axis}] Held for {heldDuration:F2}s (threshold {holdThreshold}s)");

        if (heldDuration < holdThreshold)
            return; // too quick to count, ignore

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
}