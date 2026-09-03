using UnityEngine;

public class ActionBarFollower : MonoBehaviour
{
    [Header("References")]
    public PreviewSystem previewSystem;
    public RectTransform actionBarRect;
    public Canvas canvas;
    public CanvasGroup canvasGroup;

    [Header("Offset")]
    [Tooltip("Pixels above the furniture preview centre")]
    public float yOffset = 80f;
    public float xOffset = 80f;


    private RectTransform canvasRect;
    private bool isTracking = false;

    void Start()
    {
        canvasRect = canvas.GetComponent<RectTransform>();
        Hide();
    }

    void Update()
    {
        if (!isTracking) return;

        GameObject preview = previewSystem.GetPreviewObject();
        if (preview == null)
        {
            canvasGroup.alpha = 0f;
            return;
        }

        Vector3 worldPos = GetPreviewCentre(preview);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // Hide if behind camera
        if (screenPos.z < 0)
        {
            canvasGroup.alpha = 0f;
            return;
        }

        // Make sure it's visible (Show() sets interactable/blocksRaycasts,
        // but alpha can get clobbered — restore it here)
        canvasGroup.alpha = 1f;

        screenPos.y += yOffset;
        screenPos.x += xOffset;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            null,
            out Vector2 localPos
        );

        actionBarRect.anchoredPosition = localPos;
    }

    private Vector3 GetPreviewCentre(GameObject preview)
    {
        // Try to get the visual centre from renderers
        Renderer[] renderers = preview.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            foreach (var r in renderers)
                bounds.Encapsulate(r.bounds);
            return bounds.center;
        }
        // Fallback to transform position
        return preview.transform.position;
    }

    public void Show()
    {
        isTracking = true;
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        isTracking = false;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}