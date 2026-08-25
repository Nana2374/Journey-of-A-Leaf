#region Using statements
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion

namespace Bitgem.VFX.StylisedWater
{
    // Attach this to the Player, below Player_Movement in
    // Edit > Project Settings > Script Execution Order.
    // While the player is over the water volume it floats the player at the
    // surface height and tells Player_Movement to stop applying gravity.
    // When not over water, it does nothing and normal ground/gravity logic
    // (and jumping) works exactly as before.
    [RequireComponent(typeof(CharacterController))]
    public class PlayerWaterFloater : MonoBehaviour
    {
        #region Public fields
        public WaterVolumeHelper WaterVolumeHelper = null;
        public Player_Movement PlayerMovement = null;

        [Tooltip("How high above the water surface the player floats.")]
        public float FloatOffset = 0.1f;

        [Tooltip("How quickly the player eases to the float height. Higher = snappier.")]
        public float FloatLerpSpeed = 8f;

        [Tooltip("The floater only engages once the player is within this distance of the water surface. Above this, falling/gliding is left alone so jumps and glides over water actually work.")]
        public float EngageDistance = 1.5f;
        #endregion

        #region Private fields
        private CharacterController _controller;
        #endregion

        #region Properties
        public bool IsFloating { get; private set; }
        #endregion

        #region MonoBehaviour events
        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (!PlayerMovement)
            {
                PlayerMovement = GetComponent<Player_Movement>();
            }
        }

        void Update()
        {
            // Gliding fully owns vertical movement — back off entirely so we
            // don't fight Player_Glide just because we're horizontally over
            // a water volume's bounds while airborne.
            if (PlayerMovement && PlayerMovement.IsGliding)
            {
                SetFloating(false);
                return;
            }

            var instance = WaterVolumeHelper ? WaterVolumeHelper : WaterVolumeHelper.Instance;
            if (!instance)
            {
                SetFloating(false);
                return;
            }

            float? height = null;
            try
            {
                height = instance.GetHeight(transform.position);
            }
            catch (System.NullReferenceException)
            {
                // Volume is mid-rebuild this frame — skip and try again next frame
                return;
            }

            if (!height.HasValue)
            {
                // Not over water — do nothing, let normal ground/gravity logic run
                SetFloating(false);
                return;
            }

            float targetY = height.Value + FloatOffset;

            // Still well above the surface (jumping, falling, or about to
            // glide) — leave gravity/gliding alone so a fall over water can
            // actually build up enough distance to trigger a glide.
            if (transform.position.y - targetY > EngageDistance)
            {
                SetFloating(false);
                return;
            }

            SetFloating(true);

            float newY = Mathf.Lerp(transform.position.y, targetY, FloatLerpSpeed * Time.deltaTime);
            float deltaY = newY - transform.position.y;

            // Move via the controller so it stays consistent with collision/grounding
            _controller.Move(new Vector3(0f, deltaY, 0f));
        }
        #endregion

        #region Private methods
        private void SetFloating(bool floating)
        {
            if (IsFloating == floating)
            {
                return;
            }

            IsFloating = floating;

            if (PlayerMovement)
            {
                PlayerMovement.SetInWater(floating);
            }
        }
        #endregion
    }
}