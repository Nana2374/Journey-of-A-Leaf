using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Manual-raycast input for the leaf's 3 curl hitboxes, mirroring
/// AntPickupController's pattern instead of relying on PhysicsRaycaster +
/// EventSystem pointer interfaces. This avoids EventSystem.IsPointerOverGameObject
/// treating every leaf hitbox hit as "UI", which was blocking item pickup.
/// </summary>
public class LeafCurlInputController : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask leafHitboxLayerMask;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private Camera mainCamera;

    // Tracks which hitbox each active touch/mouse pointer started on,
    // so drag/up events route back to the same hitbox even if the
    // finger moves off its collider mid-gesture.
    private LeafEdgeHitbox mouseActiveHitbox;
    private readonly System.Collections.Generic.Dictionary<int, LeafEdgeHitbox> touchActiveHitboxes
        = new System.Collections.Generic.Dictionary<int, LeafEdgeHitbox>();

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                HandleTouchPhase(touch.fingerId, touch.position, touch.phase);
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
                HandleTouchPhase(-1, Input.mousePosition, TouchPhase.Began);
            else if (Input.GetMouseButton(0))
                HandleTouchPhase(-1, Input.mousePosition, TouchPhase.Moved);
            else if (Input.GetMouseButtonUp(0))
                HandleTouchPhase(-1, Input.mousePosition, TouchPhase.Ended);
        }
    }

    private void HandleTouchPhase(int pointerId, Vector2 screenPos, TouchPhase phase)
    {
        // Same UI guard as AntPickupController. Safe here because we are NOT
        // using PhysicsRaycaster, so this only flags genuine UI (joystick, buttons).
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId))
        {
            if (debugLogs) Debug.Log($"[LeafCurlInput] Pointer {pointerId} over UI, ignoring.");
            return;
        }

        switch (phase)
        {
            case TouchPhase.Began:
                {
                    LeafEdgeHitbox hit = RaycastForHitbox(screenPos);
                    SetActiveHitbox(pointerId, hit);

                    if (hit != null)
                        hit.HandlePointerDown(screenPos);

                    break;
                }

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                {
                    LeafEdgeHitbox active = GetActiveHitbox(pointerId);
                    active?.HandleDrag(screenPos);
                    break;
                }

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                {
                    LeafEdgeHitbox active = GetActiveHitbox(pointerId);
                    active?.HandlePointerUp();
                    SetActiveHitbox(pointerId, null);
                    break;
                }
        }
    }

    private LeafEdgeHitbox RaycastForHitbox(Vector2 screenPos)
    {
        if (mainCamera == null) return null;

        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, leafHitboxLayerMask))
        {
            LeafEdgeHitbox hitbox = hitInfo.collider.GetComponentInParent<LeafEdgeHitbox>();
            if (debugLogs && hitbox != null)
                Debug.Log($"[LeafCurlInput] Hit hitbox: {hitbox.Axis}");
            return hitbox;
        }

        return null;
    }

    private LeafEdgeHitbox GetActiveHitbox(int pointerId)
    {
        if (pointerId == -1)
            return mouseActiveHitbox;

        touchActiveHitboxes.TryGetValue(pointerId, out LeafEdgeHitbox hitbox);
        return hitbox;
    }

    private void SetActiveHitbox(int pointerId, LeafEdgeHitbox hitbox)
    {
        if (pointerId == -1)
        {
            mouseActiveHitbox = hitbox;
            return;
        }

        if (hitbox == null)
            touchActiveHitboxes.Remove(pointerId);
        else
            touchActiveHitboxes[pointerId] = hitbox;
    }
}