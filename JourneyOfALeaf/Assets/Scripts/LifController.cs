using UnityEngine;

namespace Bitgem.VFX.StylisedWater
{
    public enum LifState
    {
        Normal,     // following Meep normally
        Boat,       // flat on water, Meep rides on top
        Glider      // above Meep, gliding
    }

    public class LifController : MonoBehaviour
    {
        [Header("References")]
        public Transform meep;
        public WaterVolumeFloater waterFloater;

        [Header("Offsets per state")]
        public Vector3 normalOffset = new Vector3(0.6f, 0.2f, -0.4f);
        public Vector3 boatOffset = new Vector3(0f, -0.1f, 0f);   // flat under Meep on water
        public Vector3 gliderOffset = new Vector3(0f, 1.5f, 0f);    // above Meep

        [Header("Scale per state (curl effect)")]
        public Vector3 normalScale = new Vector3(1f, 1f, 1f);
        public Vector3 boatScale = new Vector3(1.5f, 0.1f, 1.5f); // flat/uncurled
        public Vector3 gliderScale = new Vector3(2f, 0.05f, 1f);    // wide/flat for gliding

        [Header("Buoyancy")]
        public float boatBuoyancyForce = 20f;   // very buoyant as a boat
        public float normalBuoyancyForce = 15f;

        [Header("Follow")]
        public float followSpeed = 8f;
        public float scaleSpeed = 4f;

        private LifState currentState = LifState.Normal;
        private Transform playerParent;

        void Start()
        {
            playerParent = meep.parent; // the Player empty GO
        }

        void Update()
        {
            UpdatePositionAndScale();
        }

        void UpdatePositionAndScale()
        {
            Vector3 targetOffset;
            Vector3 targetScale;

            switch (currentState)
            {
                case LifState.Boat:
                    // Lif stays flat on water surface, Meep parents to Lif
                    targetOffset = boatOffset;
                    targetScale = boatScale;
                    break;

                case LifState.Glider:
                    targetOffset = gliderOffset;
                    targetScale = gliderScale;
                    break;

                default: // Normal
                    targetOffset = normalOffset;
                    targetScale = normalScale;
                    break;
            }

            // Smooth position relative to Meep
            Vector3 worldTarget = meep.position + meep.TransformDirection(targetOffset);
            transform.position = Vector3.Lerp(transform.position, worldTarget, followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, meep.rotation, followSpeed * Time.deltaTime);

            // Smooth scale (curl/uncurl effect)
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleSpeed * Time.deltaTime);
        }

        // Call this from your UI button
        public void OnInteractButton()
        {
            if (waterFloater.IsInWater)
            {
                SetState(currentState == LifState.Boat ? LifState.Normal : LifState.Boat);
            }
            else
            {
                SetState(currentState == LifState.Glider ? LifState.Normal : LifState.Glider);
            }
        }

        void SetState(LifState newState)
        {
            currentState = newState;

            // Adjust buoyancy based on state
            if (newState == LifState.Boat)
                waterFloater.buoyancyForce = boatBuoyancyForce;
            else
                waterFloater.buoyancyForce = normalBuoyancyForce;

            // TODO: trigger Animator here when animations are ready
            // animator.SetInteger("LifState", (int)newState);
        }
    }
}
