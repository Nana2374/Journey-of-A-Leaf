using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthOfFieldFocus : MonoBehaviour
{
    [SerializeField] private Transform target;  // drag your Player here
    [SerializeField] private Volume volume;     // drag your PostProcessing volume here

    private DepthOfField dof;

    void Start()
    {
        volume.profile.TryGet(out dof);
    }

    void Update()
    {
        if (dof == null || target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);
        dof.focusDistance.value = distance;
    }
}