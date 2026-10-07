using UnityEngine;

public class LeafController : MonoBehaviour
{
    [Header("Leaf")]
    [SerializeField] private Transform leafModel;
    [SerializeField] private SkinnedMeshRenderer leafRenderer;

    [Header("Curl Settings")]
    [SerializeField] private float curlSpeed = 5f;
    [SerializeField] private float attachThreshold = 0.95f;

    [Header("Blend Shape Names (must match Maya target names exactly)")]
    [SerializeField] private string nameLeft = "Leaf_States.Curl_L";
    [SerializeField] private string nameRight = "Leaf_States.Curl_R";
    [SerializeField] private string nameTop = "Leaf_States.Curl_Top";

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private int idxLeft = -1;
    private int idxRight = -1;
    private int idxTop = -1;

    private float currentLeft, targetLeft;
    private float currentRight, targetRight;
    private float currentTop, targetTop;

    public bool IsTipAttachedToHand { get; private set; }

    [Header("Glide Settings")]
    [SerializeField] private float glideTiltAmount = 30f;
    [SerializeField] private float glideTransitionSpeed = 5f;

    private float currentGlideAngle;

    public bool IsGliding { get; private set; }

    // ITEM PLACEMENT
    [Header("Item Placement")]
    [SerializeField] private Transform[] itemPlacementPoints;

    public Transform GetAvailablePlacementPoint()
    {
        foreach (Transform point in itemPlacementPoints)
        {
            if (point.childCount == 0)
            {
                return point;
            }
        }

        return null;
    }

    private void Awake()
    {
        ResolveBlendShapeIndices();
    }

    private void ResolveBlendShapeIndices()
    {
        if (leafRenderer == null || leafRenderer.sharedMesh == null)
        {
            Debug.LogError("[LeafController] Leaf Renderer is not assigned, or has no mesh. Curling will not work.");
            return;
        }

        int blendShapeCount = leafRenderer.sharedMesh.blendShapeCount;
        if (debugLogs)
        {
            Debug.Log($"[LeafController] Renderer '{leafRenderer.gameObject.name}' mesh '{leafRenderer.sharedMesh.name}' has {blendShapeCount} blend shapes.");

            if (blendShapeCount == 0)
            {
                Debug.LogError("[LeafController] This mesh has ZERO blend shapes. Either the wrong SkinnedMeshRenderer " +
                                "is assigned, or the FBX import/export didn't bring the blend shapes over.");
            }
            else
            {
                for (int i = 0; i < blendShapeCount; i++)
                {
                    Debug.Log($"[LeafController] Blend shape index {i}: \"{leafRenderer.sharedMesh.GetBlendShapeName(i)}\"");
                }
            }
        }

        idxLeft = FindBlendShape(nameLeft);
        idxRight = FindBlendShape(nameRight);
        idxTop = FindBlendShape(nameTop);
    }

    private int FindBlendShape(string shapeName)
    {
        int index = leafRenderer.sharedMesh.GetBlendShapeIndex(shapeName);

        if (index < 0)
        {
            Debug.LogWarning($"[LeafController] Blend shape '{shapeName}' NOT FOUND on mesh. " +
                              $"Check the exact name exported from Maya (case-sensitive, may have a prefix).");
        }
        else if (debugLogs)
        {
            Debug.Log($"[LeafController] Blend shape '{shapeName}' found at index {index}.");
        }

        return index;
    }

    private void Update()
    {
        UpdateCurl();
        UpdateGlide();
    }

    // CURL
    private void UpdateCurl()
    {
        currentLeft = Mathf.Lerp(currentLeft, targetLeft, curlSpeed * Time.deltaTime);
        currentRight = Mathf.Lerp(currentRight, targetRight, curlSpeed * Time.deltaTime);
        currentTop = Mathf.Lerp(currentTop, targetTop, curlSpeed * Time.deltaTime);

        ApplyCurl(idxLeft, currentLeft, "Left");
        ApplyCurl(idxRight, currentRight, "Right");
        ApplyCurl(idxTop, currentTop, "Top");

        bool wasAttached = IsTipAttachedToHand;
        IsTipAttachedToHand = currentTop >= attachThreshold;

        if (debugLogs && IsTipAttachedToHand != wasAttached)
            Debug.Log($"[LeafController] IsTipAttachedToHand changed to {IsTipAttachedToHand}");
    }

    private void ApplyCurl(int idx, float t, string axisName)
    {
        if (leafRenderer == null) return;
        if (idx < 0) return; // already warned in ResolveBlendShapeIndices

        leafRenderer.SetBlendShapeWeight(idx, Mathf.Clamp01(t) * 100f);
    }

    // CURL CONTROL
    public void SetLeftCurl(float t)
    {
        targetLeft = Mathf.Clamp01(t);
        if (debugLogs) Debug.Log($"[LeafController] SetLeftCurl({t:F2}) -> target {targetLeft:F2}");
    }

    public void SetRightCurl(float t)
    {
        targetRight = Mathf.Clamp01(t);
        if (debugLogs) Debug.Log($"[LeafController] SetRightCurl({t:F2}) -> target {targetRight:F2}");
    }

    public void SetTopCurl(float t)
    {
        targetTop = Mathf.Clamp01(t);
        if (debugLogs) Debug.Log($"[LeafController] SetTopCurl({t:F2}) -> target {targetTop:F2}");
    }

    public void ReleaseLeft()
    {
        targetLeft = 0f;
        if (debugLogs) Debug.Log("[LeafController] ReleaseLeft()");
    }

    public void ReleaseRight()
    {
        targetRight = 0f;
        if (debugLogs) Debug.Log("[LeafController] ReleaseRight()");
    }

    public void ReleaseTop()
    {
        targetTop = 0f;
        if (debugLogs) Debug.Log("[LeafController] ReleaseTop()");
    }

    public void ResetAllCurls()
    {
        targetLeft = 0f;
        targetRight = 0f;
        targetTop = 0f;
    }

    public float GetLeftCurl() => currentLeft;
    public float GetRightCurl() => currentRight;
    public float GetTopCurl() => currentTop;

    // GLIDE
    private void UpdateGlide()
    {
        if (!IsGliding)
        {
            currentGlideAngle = Mathf.Lerp(
                currentGlideAngle,
                0f,
                glideTransitionSpeed * Time.deltaTime
            );
        }
    }

    // GLIDE CONTROL
    public void StartGliding()
    {
        IsGliding = true;

        Debug.Log("Leaf: Glide started.");
    }

    public void StopGliding()
    {
        IsGliding = false;

        currentGlideAngle = 0f;

        Debug.Log("Leaf: Glide stopped.");
    }

    public bool IsInGlidePosition()
    {
        return Mathf.Abs(currentGlideAngle) > 10f;
    }

    public float GetGlideDirection()
    {
        if (glideTiltAmount == 0f)
            return 0f;

        return currentGlideAngle / glideTiltAmount;
    }

    public void SetGlideAngle(float angle)
    {
        currentGlideAngle = Mathf.Clamp(
            angle,
            -glideTiltAmount,
            glideTiltAmount
        );
    }
}