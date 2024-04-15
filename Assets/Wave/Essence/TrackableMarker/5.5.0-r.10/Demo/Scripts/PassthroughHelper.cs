using System;
using System.Collections.Generic;
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.TrackableMarker.Sample
{
    [Serializable]
    public class PassthroughHelper
	{
        [SerializeField] private Camera hmd;
	    
        public void ShowPassthroughUnderlay(bool show)
        {
            if (show)
            {
                hmd.clearFlags = CameraClearFlags.SolidColor;
                hmd.backgroundColor = new Color(0, 0, 0, 0);
            }
            else
            {
                hmd.clearFlags = CameraClearFlags.Skybox;
            }

            Interop.WVR_ShowPassthroughUnderlay(show);
        }
    }
}
