using UnityEngine;

public class LeafController : MonoBehaviour
{
    [Header("Leaf")]
    [SerializeField] private Transform leafModel;

    [Header("Glide Settings")]
    [SerializeField] private float glideTiltAmount = 30f;
    [SerializeField] private float glideTransitionSpeed = 5f;

    private float currentGlideAngle;

    public bool IsGliding { get; private set; }

    [Header("Item Placement")]
    [SerializeField] private Transform[] itemPlacementPoints;

    public Transform GetAvailablePlacementPoint()
    {
        foreach (Transform point in itemPlacementPoints)
        {
            if (point.childCount == 0)
                return point;
        }
        return null;
    }

    private void Update()
    {
        UpdateGlide();
    }

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

    public void StartGliding()
    {
        IsGliding = true;
    }

    public void StopGliding()
    {
        IsGliding = false;
        currentGlideAngle = 0f;
    }

    public bool IsInGlidePosition()
    {
        return Mathf.Abs(currentGlideAngle) > 10f;
    }

    public float GetGlideDirection()
    {
        if (glideTiltAmount == 0f) return 0f;
        return currentGlideAngle / glideTiltAmount;
    }

    public void SetGlideAngle(float angle)
    {
        currentGlideAngle = Mathf.Clamp(angle, -glideTiltAmount, glideTiltAmount);
    }
}