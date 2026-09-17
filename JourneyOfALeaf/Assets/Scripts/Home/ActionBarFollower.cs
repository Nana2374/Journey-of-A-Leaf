using UnityEngine;

public class ActionBarFollower : MonoBehaviour
{
    [Header("References")]
    public PreviewSystem previewSystem;
    public RectTransform actionBarRect;
    public Canvas canvas;
    public CanvasGroup canvasGroup;

    [Header("Offset")]
    [Tooltip("Negative value moves it below the furniture centre")]
    public float yOffset = -80f;
    public float xOffset = 0f;

    private RectTransform canvasRect;
    private bool isVisible = false;
    private GameObject trackedObject = null;

    void Start()
    {
        canvasRect = canvas.GetComponent<RectTransform>();
        Hide();
    }

    void Update()
    {
        if (!isVisible) return;

        GameObject target = trackedObject != null
            ? trackedObject
            : previewSystem.GetPreviewObject();

        if (target == null) return;

        Vector3 worldPos = GetCentre(target);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0) return;

        // Apply offset in screen space scaled to canvas
        float scaleFactor = canvas.scaleFactor;
        screenPos.y += yOffset * scaleFactor;
        screenPos.x += xOffset * scaleFactor;

        // Use canvas camera for Screen Space - Camera, null for Overlay
        Camera canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvasCamera,
            out Vector2 localPos);

        actionBarRect.anchoredPosition = localPos;
    }

    public void TrackWorldObject(GameObject obj)
    {
        trackedObject = obj;
    }

    public void StopTracking()
    {
        trackedObject = null;
        actionBarRect.anchoredPosition = new Vector2(-9999f, -9999f);
    }

    private Vector3 GetCentre(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            return bounds.center;
        }
        return obj.transform.position;
    }

    public void Show()
    {
        isVisible = true;
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        isVisible = false;
        trackedObject = null;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}