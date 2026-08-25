using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

/// <summary>
/// Prevents the Cinemachine FreeLook camera from receiving look input
/// (via CinemachineInputProvider's LookAround action) when the current
/// touch/click gesture started over a UI element, like the on-screen dpad.
///
/// Attach this anywhere (e.g. on the same object as your camera rig),
/// and assign the same InputActionReference used as "LookAround" on
/// your CinemachineInputProvider component.
///
/// This does NOT modify any Cinemachine package files.
/// </summary>
public class CameraInputUIGate : MonoBehaviour
{
    [Tooltip("The same LookAround InputActionReference assigned on CinemachineInputProvider (Pointer/delta).")]
    public InputActionReference lookAroundAction;

    // Whether the gesture currently in progress started over UI
    private bool gestureBlockedByUI = false;
    private bool wasPressed = false;

    private void Update()
    {
        bool isPressed = IsPointerOrTouchPressed(out int pointerId);

        // Only re-evaluate "started on UI?" at the moment a new gesture begins,
        // so sliding off the dpad mid-drag doesn't suddenly start rotating the camera.
        if (isPressed && !wasPressed)
        {
            gestureBlockedByUI =
                EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject(pointerId);
        }
        else if (!isPressed)
        {
            gestureBlockedByUI = false;
        }

        wasPressed = isPressed;

        ApplyGateState();
    }

    private void ApplyGateState()
    {
        if (lookAroundAction == null || lookAroundAction.action == null)
            return;

        bool shouldBeEnabled = !gestureBlockedByUI;

        if (shouldBeEnabled && !lookAroundAction.action.enabled)
        {
            lookAroundAction.action.Enable();
        }
        else if (!shouldBeEnabled && lookAroundAction.action.enabled)
        {
            lookAroundAction.action.Disable();
        }
    }

    // Returns true if a mouse button or touch is currently pressed,
    // and outputs the pointer id to check against the UI EventSystem.
    // pointerId of -1 corresponds to the mouse pointer.
    private bool IsPointerOrTouchPressed(out int pointerId)
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            pointerId = Touchscreen.current.primaryTouch.touchId.ReadValue();
            return true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            pointerId = -1;
            return true;
        }

        pointerId = -1;
        return false;
    }

    private void OnDisable()
    {
        // Make sure we don't leave the action permanently disabled
        // if this gate component is turned off for some reason.
        if (lookAroundAction != null && lookAroundAction.action != null &&
            !lookAroundAction.action.enabled)
        {
            lookAroundAction.action.Enable();
        }
    }
}