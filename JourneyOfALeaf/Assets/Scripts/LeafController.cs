using UnityEngine;

public class LeafController : MonoBehaviour
{
    [Header("Leaf")]
    [SerializeField] private Transform leafModel;

    [Header("Curl Settings")]
    [SerializeField] private float maxCurlAngle = 45f;
    [SerializeField] private float curlSpeed = 5f;

    [Header("Item Placement")]
    [SerializeField] private Transform[] itemPlacementPoints;

    private float currentCurl;

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
        // Temporary test
        // We'll replace this with touch input later.

        float targetCurl = 0f;

        currentCurl = Mathf.Lerp(
            currentCurl,
            targetCurl,
            curlSpeed * Time.deltaTime
        );
    }
}