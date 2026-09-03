using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera sceneCamera;
    [SerializeField] private LayerMask placementLayermask;

    private Vector3 lastPosition;

    public event Action OnClicked;
    public event Action OnExit;
    public event Action<bool> OnBuildModeChanged;

    public bool IsBuildModeActive { get; private set; }

    [Header("Drag Settings")]
    [Tooltip("How many pixels the finger must move before it counts as a drag")]
    public float dragThreshold = 10f;

    private Vector2 pressStartPos;
    private bool isDragging = false;
    private bool pressStartedOverUI = false;

    void Update()
    {
        if (!IsBuildModeActive) return;

        if (Pointer.current == null) return;

        // Finger/mouse just pressed down
        if (Pointer.current.press.wasPressedThisFrame)
        {
            pressStartPos = Pointer.current.position.ReadValue();
            isDragging = false;
            pressStartedOverUI = IsPointerOverInteractableUI();
        }

        // Finger/mouse is held down — check if dragging
        if (Pointer.current.press.isPressed && !pressStartedOverUI)
        {
            Vector2 currentPos = Pointer.current.position.ReadValue();
            float distance = Vector2.Distance(currentPos, pressStartPos);

            if (distance > dragThreshold)
                isDragging = true;
        }

        // Finger/mouse lifted
        if (Pointer.current.press.wasReleasedThisFrame)
        {
            isDragging = false;
            pressStartedOverUI = false;
        }
    }

    // PlacementSystem.Update() calls this to move the preview
    public bool IsDragging() => isDragging && !pressStartedOverUI;

    public void EnterBuildMode()
    {
        if (IsBuildModeActive) return;
        IsBuildModeActive = true;
        OnBuildModeChanged?.Invoke(true);
    }

    public void ExitBuildMode()
    {
        if (!IsBuildModeActive) return;
        IsBuildModeActive = false;
        OnExit?.Invoke();
        OnBuildModeChanged?.Invoke(false);
    }

    public bool IsPointerOverUI() => IsPointerOverInteractableUI();

    private bool IsPointerOverInteractableUI()
    {
        Vector2 pointerPos = Pointer.current != null
            ? Pointer.current.position.ReadValue()
            : Vector2.zero;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = pointerPos
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            Transform current = result.gameObject.transform;
            while (current != null)
            {
                if (current.GetComponent<Button>() != null) return true;
                if (current.GetComponent<Toggle>() != null) return true;
                if (current.GetComponent<Slider>() != null) return true;
                if (current.GetComponent<ScrollRect>() != null) return true;
                if (current.GetComponent<CanvasGroup>() != null &&
                    current.GetComponent<CanvasGroup>().blocksRaycasts &&
                    current.GetComponent<CanvasGroup>().interactable) return true;
                current = current.parent;
            }
        }

        return false;
    }

    public Vector3 GetSelectedMapPosition()
    {
        Vector2 pointerScreenPos = Pointer.current != null
            ? Pointer.current.position.ReadValue()
            : Vector2.zero;

        Ray ray = sceneCamera.ScreenPointToRay(pointerScreenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, placementLayermask))
            lastPosition = hit.point;

        return lastPosition;
    }
}