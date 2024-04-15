using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.TrackableMarker.Sample
{
	public class BaseMarker : IDisposable
	{
		public WVR_Uuid markerId;
		public WVR_MarkerObserverTarget target;
		public WVR_MarkerTrackingState state;
		public WVR_Pose_t pose;
		public WVR_MarkerName markerName;

		protected GameObject generatedMarkerGO;
		protected MarkerPrefab markerPrefabInstance;

		protected TrackableMarkerController trackableMarkerController;

		protected BaseMarker(TrackableMarkerController inTrackableMarkerController, GameObject inMarkerPrefab, WVR_Uuid inMarkerID, WVR_MarkerObserverTarget inTarget, WVR_MarkerTrackingState inState, WVR_Pose_t inPose, WVR_MarkerName inName)
		{
			markerId = inMarkerID;
			target = inTarget;
			state = inState;
			pose = inPose;
			markerName = inName;

			trackableMarkerController = inTrackableMarkerController;
		}

		public void UpdateTrackableMarkerState(WVR_TrackableMarkerState inMarkerState) //Assumes that uuid is check beforehand
		{
			UpdateTrackingState(inMarkerState.state);
			UpdatePose(inMarkerState.pose);
			markerName = inMarkerState.markerName;
		}

		protected GameObject GenerateNewMarkerGO(GameObject markerPrefab)
		{
			if (generatedMarkerGO != null) DestroyGameObject();

			generatedMarkerGO = GameObject.Instantiate(markerPrefab);
			markerPrefabInstance = generatedMarkerGO.GetComponent<MarkerPrefab>();

			markerPrefabInstance.markerRef = this;

			markerPrefabInstance.InitMarkerPrefab();

			trackableMarkerController.ApplyTrackingOriginCorrectionToMarkerPose(pose, out Vector3 worldSpaceMarkerPosition, out Quaternion worldSpaceMarkerRotation);
			markerPrefabInstance.UpdateMarkerPrefabPose(worldSpaceMarkerPosition, worldSpaceMarkerRotation);

			markerPrefabInstance.UpdateMarkerPrefabTrackingState(state);

			return generatedMarkerGO;
		}

		protected void UpdatePose(WVR_Pose_t newPose)
		{
			if (!WVRStructCompare.WVRPoseEqual(pose, newPose))
			{
				if (markerPrefabInstance != null)
				{
					trackableMarkerController.ApplyTrackingOriginCorrectionToMarkerPose(newPose, out Vector3 worldSpaceMarkerPosition, out Quaternion worldSpaceMarkerRotation);
					markerPrefabInstance.UpdateMarkerPrefabPose(worldSpaceMarkerPosition, worldSpaceMarkerRotation);
				}
				pose = newPose;
			}
		}

		protected void UpdateTrackingState(WVR_MarkerTrackingState newState)
		{
			if (state != newState)
			{
				if (markerPrefabInstance != null) markerPrefabInstance.UpdateMarkerPrefabTrackingState(newState);
				state = newState;
			}
		}

		public void Dispose()
		{
			DestroyGameObject();
		}

		public void DestroyGameObject()
		{
			if (generatedMarkerGO != null)
			{
				UnityEngine.Object.Destroy(generatedMarkerGO);

				generatedMarkerGO = null;
				markerPrefabInstance = null;
			}
		}
	}
}
