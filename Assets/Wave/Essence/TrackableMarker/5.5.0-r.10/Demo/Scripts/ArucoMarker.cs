using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.TrackableMarker.Sample
{
	public class ArucoMarker : BaseMarker
	{
		//Data copied from WVR_ArucoMarker
		public UInt64 trackerId;                /**< indicates the tracker id of the aruco marker */
		public float size;                      /**< indicates the size */

		public ArucoMarker(TrackableMarkerController inTrackableMarkerController, GameObject inMarkerPrefab, WVR_ArucoMarker inArucoMarker) : base(inTrackableMarkerController, inMarkerPrefab, inArucoMarker.uuid, WVR_MarkerObserverTarget.WVR_MarkerObserverTarget_Aruco, inArucoMarker.state, inArucoMarker.pose, inArucoMarker.markerName)
		{
			trackerId = inArucoMarker.trackerId;
			UpdateSize(inArucoMarker.size);

			GenerateNewMarkerGO(inMarkerPrefab);
			markerPrefabInstance.UpdateMarkerPrefabSize(size);

		}

		public void UpdateArucoMarker(WVR_ArucoMarker inArucoMarker)
		{
			if (TrackableMarkerController.IsUUIDEqual(markerId, inArucoMarker.uuid))
			{
				trackerId = inArucoMarker.trackerId;
				UpdateSize(inArucoMarker.size);
				UpdatePose(inArucoMarker.pose);
				markerName = inArucoMarker.markerName;
				UpdateTrackingState(inArucoMarker.state);
			}
		}

		public void UpdateArucoMarkerData(WVR_ArucoMarkerData inArucoMarkerData) //Assumes that uuid is check beforehand
		{
			trackerId = inArucoMarkerData.trackerId;
			UpdateSize(inArucoMarkerData.size);
		}

		private void UpdateSize(float newSize)
		{
			if (size != newSize)
			{
				if (markerPrefabInstance != null) markerPrefabInstance.UpdateMarkerPrefabSize(newSize);
				size = newSize;
			}
		}
	}
}
