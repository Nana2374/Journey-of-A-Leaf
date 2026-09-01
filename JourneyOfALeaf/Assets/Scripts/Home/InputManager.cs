using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Handles tap/click input for the furniture-placement system.
// Only reports placement taps while Build Mode is active — call
// EnterBuildMode() from your "build" button's OnClick, and
// ExitBuildMode() from your "X" button's OnClick.
public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Camera sceneCamera;

    [SerializeField]
    private LayerMask placementLayermask;

    private Vector3 lastPosition;

    // Fired when the player taps/clicks somewhere valid to place/select
    // an item, but ONLY while build mode is active.
    public event Action OnClicked;

    // Fired the moment build mode is exited (via ExitBuildMode()).
    // Useful for e.g. deselecting the currently-held furniture item.
    public event Action OnExit;

    // Fired whenever build mode turns on or off, in case other scripts
    // (UI, player movement, etc.) want to react to the mode change.
    public event Action<bool> OnBuildModeChanged;

    public bool IsBuildModeActive { get; private set; }

    private void Update()
    {
        if (!IsBuildModeActive)
            return; // ignore all placement input outside build mode

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            //remove int and debug after checking if works

            int subCount = OnClicked?.GetInvocationList().Length ?? 0;
            Debug.Log($"Pointer press detected. OnClicked subscriber count: {subCount}");
            OnClicked?.Invoke();
        }
    }

    // Call this from your "Enter Build Mode" button's OnClick()
    public void EnterBuildMode()
    {
        if (IsBuildModeActive)
            return;

        IsBuildModeActive = true;
        OnBuildModeChanged?.Invoke(true);
    }

    // Call this from your "X" (exit build mode) button's OnClick()
    public void ExitBuildMode()
    {
        if (!IsBuildModeActive)
            return;

        IsBuildModeActive = false;
        OnExit?.Invoke();
        OnBuildModeChanged?.Invoke(false);
    }

    public bool IsPointerOverUI()
        => EventSystem.current.IsPointerOverGameObject();

    public Vector3 GetSelectedMapPosition()
    {
        Vector2 pointerScreenPos = Pointer.current != null
            ? Pointer.current.position.ReadValue()
            : Vector2.zero;

        Ray ray = sceneCamera.ScreenPointToRay(pointerScreenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, placementLayermask))
        {
            lastPosition = hit.point;
        }
        return lastPosition;
    }
}