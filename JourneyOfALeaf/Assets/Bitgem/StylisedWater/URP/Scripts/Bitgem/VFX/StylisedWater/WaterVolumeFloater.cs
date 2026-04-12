using UnityEngine;

namespace Bitgem.VFX.StylisedWater
{
    public class WaterVolumeFloater : MonoBehaviour
    {
        [Header("Water")]
        public WaterVolumeHelper WaterVolumeHelper = null;

        [Header("Buoyancy")]
        [Tooltip("Higher = bobs back to surface faster. Leaf: ~15, Main character: ~5")]
        public float buoyancyForce = 10f;

        [Tooltip("Dampens bobbing. Keep between 0.5 and 2")]
        public float damping = 1f;

        [Tooltip("How high above the water surface to float. 0 = flush, negative = slightly submerged")]
        public float floatOffset = 0f;

        public bool IsInWater => isInWater;

        private Rigidbody rb;
        private bool isInWater = false;
        private float waterHeight = 0f;

        void Start()
        {
            rb = GetComponentInParent<Rigidbody>();

            if (WaterVolumeHelper == null)
                WaterVolumeHelper = WaterVolumeHelper.Instance;

            if (rb == null)
                Debug.LogWarning($"WaterVolumeFloater on {gameObject.name}: no Rigidbody found in parent!");
            if (WaterVolumeHelper == null)
                Debug.LogWarning($"WaterVolumeFloater on {gameObject.name}: no WaterVolumeHelper assigned!");
        }

        void Update()
        {
            if (WaterVolumeHelper == null) return;

            float? height = WaterVolumeHelper.GetHeight(transform.position);

            if (height.HasValue && height.Value != 0f)
            {
                isInWater = true;
                // Subtract half a TileSize to sit at the visual surface rather than the top of the tile
                waterHeight = height.Value - (WaterVolumeHelper.WaterVolume.TileSize * 0.5f);
            }
            else
            {
                isInWater = false;
            }
        }

        void FixedUpdate()
        {
            if (!isInWater || rb == null) return;

            float targetY = waterHeight + floatOffset;
            float currentY = transform.position.y;

            if (currentY < targetY)
            {
                float distanceBelowSurface = targetY - currentY;
                float upwardForce = distanceBelowSurface * buoyancyForce;

                // Dampen vertical velocity to prevent endless bobbing
                upwardForce -= rb.velocity.y * damping;

                rb.AddForce(Vector3.up * upwardForce, ForceMode.Acceleration);
            }
        }
    }
}