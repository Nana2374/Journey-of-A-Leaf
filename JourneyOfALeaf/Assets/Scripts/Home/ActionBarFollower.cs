using UnityEngine;

public class ActionBarFollower : MonoBehaviour
{
    [Header("References")]
    public PreviewSystem previewSystem;
    public RectTransform actionBarRect;
    public Canvas canvas;
    public CanvasGroup canvasGroup;
    public ObjectsDatabaseSO database;

    [Header("Offset")]
    [Tooltip("Base offset below furniture centre per grid unit")]
    public float yOffsetPerUnit = -30f;
    public float yOffsetBase = -40f;
    public float xOffset = 0f;

    private RectTransform canvasRect;
    private bool isVisible = false;
    private GameObject trackedObject = null;
    private int currentFurnitureID = -1;

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

        float scaleFactor = canvas.scaleFactor;

        // Calculate y offset based on furniture size
        float yOffset = CalculateYOffset();

        screenPos.y += yOffset * scaleFactor;
        screenPos.x += xOffset * scaleFactor;

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

    private float CalculateYOffset()
    {
        if (database == null || currentFurnitureID == -1)
            return yOffsetBase;

        var data = database.objectsData.Find(d => d.ID == currentFurnitureID);
        if (data == null) return yOffsetBase;

        // Use the larger of X or Z size to determine offset
        int maxSize = Mathf.Max(data.Size.x, data.Size.y);
        return yOffsetBase + (yOffsetPerUnit * (maxSize - 1));
    }

    public void SetFurnitureID(int id)
    {
        currentFurnitureID = id;
    }

    public void TrackWorldObject(GameObject obj)
    {
        trackedObject = obj;
    }

    public void StopTracking()
    {
        trackedObject = null;
        // Don't reset currentFurnitureID here — it's needed for preview mode offset
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
        currentFurnitureID = -1; // only reset when fully hidden
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}