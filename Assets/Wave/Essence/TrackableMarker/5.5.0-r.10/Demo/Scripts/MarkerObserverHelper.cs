using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.TrackableMarker.Sample
{
	public class MarkerObserverHelper
	{
		//This class manages all trackable marker lifecycle related operations
		//1. Start/Stop Trackable Marker Service
		//2a. Start/Stop Trackable Marker Observer
		//2b. Get Trackable Marker Observer State
		//3a. Start/Stop Marker Detection

		private TrackableMarkerController trackableMarkerController;

		public bool isMarkerServiceRunning = false, isMarkerObserverRunning = false;

		private WVR_MarkerObserverState currentMarkerObserverState = WVR_MarkerObserverState.WVR_MarkerObserverState_Idle;
		private WVR_MarkerObserverTarget currentMarkerObserverTarget = WVR_MarkerObserverTarget.WVR_MarkerObserverTarget_Aruco;

		private readonly Dictionary<WVR_MarkerObserverTarget, BaseMarkerContainer> markerContainerMap;
		private readonly Dictionary<WVR_MarkerObserverTarget, MarkerDetectionHelper> markerDetectionHelperMap;
		private readonly MarkerTrackingHelper markerTrackingHelper;

		private readonly GameObject markerPrefab;

		private const string LOG_TAG = "MarkerObserverHelper";

		public MarkerObserverHelper(TrackableMarkerController inTrackableMarkerController, GameObject inMarkerPrefab)
		{
			trackableMarkerController = inTrackableMarkerController;
			markerPrefab = inMarkerPrefab;

			markerContainerMap = new Dictionary<WVR_MarkerObserverTarget, BaseMarkerContainer>();
			markerDetectionHelperMap = new Dictionary<WVR_MarkerObserverTarget, MarkerDetectionHelper>();

			markerTrackingHelper = new MarkerTrackingHelper(this, trackableMarkerController);

			InitializeMarkerComponents(currentMarkerObserverTarget);
		}

		private void InitializeMarkerComponents(WVR_MarkerObserverTarget observerTarget)
		{
			switch (observerTarget)
			{
				case WVR_MarkerObserverTarget.WVR_MarkerObserverTarget_Aruco:
					{
						markerContainerMap[observerTarget] = new ArucoMarkerContainer(this, trackableMarkerController, markerPrefab);
						markerDetectionHelperMap[observerTarget] = new MarkerDetectionHelper(this, trackableMarkerController, observerTarget);
						break;
					}
				default:
					{
						break;
					}
			}
		}

		private bool IsMarkerSupported()
		{
			return (Interop.WVR_GetSupportedFeatures() &
					(ulong)WVR_SupportedFeature.WVR_SupportedFeature_Marker) != 0;
		}

		public void OnEnable()
		{
			if (!IsMarkerSupported())
			{
				Log.e(LOG_TAG, "Marker Not Supported");
				throw new Exception("Marker is not supported on this device");
			}

			WVR_Result result = trackableMarkerController.StartMarkerService();
			if (result == WVR_Result.WVR_Success)
			{
				isMarkerServiceRunning = true;

				StartMarkerObserver();
			}
		}

		public void OnDisable()
		{
			if (!isMarkerServiceRunning) return;

			StopMarkerObserver();

			trackableMarkerController.StopMarkerService();
			isMarkerServiceRunning = false;
		}

		public void OnUpdate()
		{
			if (isMarkerServiceRunning && isMarkerObserverRunning)
			{
				UpdateMarkerObserverState();

				//Spawn/destroy/update detected markers -> e.g. Aruco markers
				markerDetectionHelperMap[currentMarkerObserverTarget].UpdateDetectedMarkers(markerContainerMap[currentMarkerObserverTarget]);

				switch (currentMarkerObserverState)
				{
					case WVR_MarkerObserverState.WVR_MarkerObserverState_Detecting:
						{
							break;
						}
					case WVR_MarkerObserverState.WVR_MarkerObserverState_Tracking:
						{
							//Update tracking state of all trackable markers
							markerTrackingHelper.UpdateTrackableMarkerState();
							break;
						}
					case WVR_MarkerObserverState.WVR_MarkerObserverState_Idle:
					default:
						break;
				}
			}
		}

		public void StartMarkerObserver()
		{
			if (isMarkerServiceRunning && !isMarkerObserverRunning)
			{
				WVR_Result result = trackableMarkerController.StartMarkerObserver(currentMarkerObserverTarget);

				if (result == WVR_Result.WVR_Success)
				{
					isMarkerObserverRunning = true;
				}
			}
		}

		public void StopMarkerObserver()
		{
			if (isMarkerServiceRunning && isMarkerObserverRunning)
			{
				WVR_Result result = trackableMarkerController.StopMarkerObserver(currentMarkerObserverTarget);

				if (result == WVR_Result.WVR_Success)
				{
					isMarkerObserverRunning = false;
				}
			}
		}

		public void UpdateMarkerObserverState()
		{
			if (isMarkerServiceRunning && isMarkerObserverRunning)
			{
				WVR_Result result = trackableMarkerController.GetMarkerObserverState(currentMarkerObserverTarget, out currentMarkerObserverState);
			}
		}

		public BaseMarker GetMarkerObjectFromMarkerContainer(WVR_Uuid markerId, WVR_MarkerObserverTarget observerTarget)
		{
			switch (observerTarget)
			{
				case WVR_MarkerObserverTarget.WVR_MarkerObserverTarget_Aruco:
					{
						return ((ArucoMarkerContainer)markerContainerMap[observerTarget]).FindMarkerWithId(markerId);
					}
				default:
					{
						break;
					}
			}

			return null;
		}

		//For handling mode change events
		public void HandleSwitchDetectionMode(bool start)
		{
			//Start Detecting
			if (start) markerDetectionHelperMap[currentMarkerObserverTarget].StartMarkerDetection();
			else markerDetectionHelperMap[currentMarkerObserverTarget].StopMarkerDetection();
			UpdateMarkerObserverState();
		}

		public void HandleSwitchTrackingMode(bool start)
		{
			//Start Tracking
			if (start) markerTrackingHelper.StartTrackingTrackableMarkers();
			else markerTrackingHelper.StopTrackingTrackableMarkers();
			UpdateMarkerObserverState();
		}
	}
}
