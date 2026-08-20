#region Using statements

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Bitgem.VFX.StylisedWater
{
    public class WateverVolumeFloater : MonoBehaviour
    {
        #region Public fields

        public WaterVolumeHelper WaterVolumeHelper = null;

        #endregion

        #region MonoBehaviour events

        void Update()
        {
            var instance = WaterVolumeHelper ? WaterVolumeHelper : WaterVolumeHelper.Instance;
            if (!instance)
            {
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

            transform.position = new Vector3(transform.position.x, height ?? transform.position.y, transform.position.z);
        }

        #endregion
    }
}