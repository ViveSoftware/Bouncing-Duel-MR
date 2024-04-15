using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.TrackableMarker.Sample
{
	public class MarkerDetectionHelper
	{
		private MarkerObserverHelper markerObserverHelper;
		private TrackableMarkerController trackableMarkerController;

		private readonly WVR_MarkerObserverTarget markerObserverTarget = WVR_MarkerObserverTarget.WVR_MarkerObserverTarget_Max;

		public MarkerDetectionHelper(MarkerObserverHelper inObserverHelper, TrackableMarkerController inMarkerController, WVR_MarkerObserverTarget inObserverTarget)
		{
			markerObserverHelper = inObserverHelper;
			trackableMarkerController = inMarkerController;
			markerObserverTarget = inObserverTarget;
		}

		public void StartMarkerDetection()
		{
			if (markerObserverHelper.isMarkerServiceRunning && markerObserverHelper.isMarkerObserverRunning)
			{
				trackableMarkerController.StartMarkerDetection(markerObserverTarget);
			}
		}

		public void StopMarkerDetection()
		{
			if (markerObserverHelper.isMarkerServiceRunning && markerObserverHelper.isMarkerObserverRunning)
			{
				trackableMarkerController.StopMarkerDetection(markerObserverTarget);
			}
		}

		public void UpdateDetectedMarkers(BaseMarkerContainer markerContainer)
		{
			if (markerObserverHelper.isMarkerServiceRunning && markerObserverHelper.isMarkerObserverRunning)
			{
				switch (markerObserverTarget)
				{
					case WVR_MarkerObserverTarget.WVR_MarkerObserverTarget_Aruco:
						{
							((ArucoMarkerContainer)markerContainer).UpdateArucoMarkers();
							break;
						}
					default:
						{
							break;
						}
				}
			}
		}
	}
}
