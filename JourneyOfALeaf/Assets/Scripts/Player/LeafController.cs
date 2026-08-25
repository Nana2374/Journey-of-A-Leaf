using UnityEngine;

public class LeafController : MonoBehaviour
{
    [Header("Leaf")]
    [SerializeField] private Transform leafModel;

    [Header("Curl Settings")]
    [SerializeField] private float maxCurlAngle = 45f;
    [SerializeField] private float curlSpeed = 5f;

    private float currentCurl;
    private float targetCurl;

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

    private void Update()
    {
        UpdateCurl();
        UpdateGlide();
    }

    // CURL
    private void UpdateCurl()
    {
        currentCurl = Mathf.Lerp(
            currentCurl,
            targetCurl,
            curlSpeed * Time.deltaTime
        );

        // Actual leaf bone/rig curling will be added later.
        // For now, currentCurl simply stores the value.
    }


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

    // CURL CONTROL
    public void SetCurl(float curlAmount)
    {
        targetCurl = Mathf.Clamp(
            curlAmount,
            -maxCurlAngle,
            maxCurlAngle
        );
    }

    public void ResetCurl()
    {
        targetCurl = 0f;
    }

    public float GetCurrentCurl()
    {
        return currentCurl;
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