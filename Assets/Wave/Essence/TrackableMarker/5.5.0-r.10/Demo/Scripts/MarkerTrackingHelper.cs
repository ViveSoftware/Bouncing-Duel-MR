using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.TrackableMarker.Sample
{
	public class MarkerTrackingHelper
	{
		private MarkerObserverHelper markerObserverHelper;
		private TrackableMarkerController trackableMarkerController;

		private readonly Dictionary<WVR_Uuid, BaseMarker> trackableMarkerDictionary;

		private const string LOG_TAG = "MarkerTrackingHelper";

		public MarkerTrackingHelper(MarkerObserverHelper inObserverHelper, TrackableMarkerController inMarkerController)
		{
			markerObserverHelper = inObserverHelper;
			trackableMarkerController = inMarkerController;

			//Initialize dictionaries
			trackableMarkerDictionary = new Dictionary<WVR_Uuid, BaseMarker>();

			//Register marker events
			MarkerPrefabEventMediator.OnClickTrackMarkerEvent += OnReceiveTrackMarkerEventFromPrefab;
			MarkerPrefabEventMediator.OnClickUntrackMarkerEvent += OnReceiveUntrackMarkerEventFromPrefab;
		}

		~MarkerTrackingHelper()
		{
			MarkerPrefabEventMediator.OnClickTrackMarkerEvent -= OnReceiveTrackMarkerEventFromPrefab;
			MarkerPrefabEventMediator.OnClickUntrackMarkerEvent -= OnReceiveUntrackMarkerEventFromPrefab;
		}

		public void ClearTrackableMarkerDictionary()
		{
			trackableMarkerDictionary.Clear();
		}

		public void StartTrackingTrackableMarkers()
		{
			if (markerObserverHelper.isMarkerServiceRunning && markerObserverHelper.isMarkerObserverRunning)
			{
				RefreshTrackableMarkerDictionary(); //Get the latest list of trackable markers

				foreach (WVR_Uuid trackableMarkerId in trackableMarkerDictionary.Keys)
				{
					trackableMarkerController.StartTrackableMarkerTracking(trackableMarkerId);
				}
			}
		}

		public void StopTrackingTrackableMarkers()
		{
			if (markerObserverHelper.isMarkerServiceRunning && markerObserverHelper.isMarkerObserverRunning)
			{ 
				RefreshTrackableMarkerDictionary(); //Get the latest list of trackable markers

				foreach (WVR_Uuid trackableMarkerId in trackableMarkerDictionary.Keys)
				{
					trackableMarkerController.StopTrackableMarkerTracking(trackableMarkerId);
				}
			}
		}

		public bool CreateTrackableMarker(WVR_Uuid targetMarkerId, WVR_MarkerObserverTarget observerTarget)
		{
			if (markerObserverHelper.isMarkerServiceRunning && markerObserverHelper.isMarkerObserverRunning)
			{
				string markerNameString = "TM_" + BitConverter.ToString(targetMarkerId.data);
				char[] markerNameCharArray = markerNameString.ToCharArray();

				WVR_Result result = trackableMarkerController.CreateTrackableMarker(targetMarkerId, markerNameCharArray);

				if (result == WVR_Result.WVR_Success)
				{
					RefreshTrackableMarkerDictionaryEntry(observerTarget); //Get the latest list of trackable markers of the current type
					return true;
				}
			}
			return false;
		}

		public bool DestroyTrackableMarker(WVR_Uuid targetMarkerId)
		{
			if (markerObserverHelper.isMarkerServiceRunning && markerObserverHelper.isMarkerObserverRunning)
			{
				if (GetTrackableMarkerState(targetMarkerId, out WVR_TrackableMarkerState markerState))
				{
					WVR_Result result = trackableMarkerController.DestroyTrackableMarker(targetMarkerId);

					if (result == WVR_Result.WVR_Success)
					{
						RefreshTrackableMarkerDictionaryEntry(markerState.target); //Get the latest list of trackable markers of the current type
						return true;
					}
				}
			}
			return false;
		}

		public bool GetTrackableMarkerState(WVR_Uuid targetMarkerId, out WVR_TrackableMarkerState markerState)
		{
			markerState = default(WVR_TrackableMarkerState);

			if (markerObserverHelper.isMarkerServiceRunning && markerObserverHelper.isMarkerObserverRunning)
			{
				WVR_Result result = trackableMarkerController.GetTrackableMarkerState(targetMarkerId, TrackableMarkerController.GetCurrentPoseOriginModel(), out markerState);

				if (result == WVR_Result.WVR_Success)
				{
					return true;
				}
			}

			return false;
		}

		public void UpdateTrackableMarkerState()
		{
			foreach (WVR_Uuid markerId in trackableMarkerDictionary.Keys)
			{
				if (GetTrackableMarkerState(markerId, out WVR_TrackableMarkerState markerState))
				{
					BaseMarker targetBaseMarker = trackableMarkerDictionary[markerId];
					targetBaseMarker.UpdateTrackableMarkerState(markerState);

					switch (targetBaseMarker.target)
					{
						case WVR_MarkerObserverTarget.WVR_MarkerObserverTarget_Aruco:
							{
								ArucoMarker targetArucoMarker = (ArucoMarker)targetBaseMarker;
								trackableMarkerController.GetArucoMarkerData(markerId, out WVR_ArucoMarkerData newArucoMarkerData);
								targetArucoMarker.UpdateArucoMarkerData(newArucoMarkerData);
								break;
							}
						default:
							break;
					}
				}
			}
		}

		private void RefreshTrackableMarkerDictionary()
		{
			foreach (WVR_MarkerObserverTarget observerTarget in Enum.GetValues(typeof(WVR_MarkerObserverTarget)))
			{
				if ((int)observerTarget < (int)WVR_MarkerObserverTarget.WVR_MarkerObserverTarget_Max)
				{
					RefreshTrackableMarkerDictionaryEntry(observerTarget);
				}
			}
		}

		private void RefreshTrackableMarkerDictionaryEntry(WVR_MarkerObserverTarget observerTarget)
		{
			WVR_Result result = trackableMarkerController.GetTrackableMarkers(observerTarget, out WVR_Uuid[] trackableMarkerIdArray);

			if (result == WVR_Result.WVR_Success)
			{
				List<WVR_Uuid> trackableMarkerIdList = new List<WVR_Uuid>(trackableMarkerIdArray);
				
				//Remove markers that no longer exist from the dictionary
				List<WVR_Uuid> trackableMarkerIdsToBeRemoved = new List<WVR_Uuid>();
				foreach (WVR_Uuid markerIdInDictionary in trackableMarkerDictionary.Keys)
				{
					WVR_MarkerObserverTarget observerTargetOfMarkerInDictionary = trackableMarkerDictionary[markerIdInDictionary].target;

					if (observerTargetOfMarkerInDictionary != observerTarget) continue; //ignore marker in dictionary if targets are different

					if (!trackableMarkerIdList.Exists(markerIdInList => WVRStructCompare.IsUUIDEqual(markerIdInList, markerIdInDictionary))) //id in dictionary no longer exists
					{
						trackableMarkerIdsToBeRemoved.Add(markerIdInDictionary);
					}
				}

				foreach(WVR_Uuid trackableMarkerIdToBeRemoved in trackableMarkerIdsToBeRemoved)
				{
					trackableMarkerDictionary.Remove(trackableMarkerIdToBeRemoved);
				}

				//Add marker to dictionary 
				foreach (WVR_Uuid retrievedMarkerId in trackableMarkerIdArray)
				{
					if (!trackableMarkerDictionary.ContainsKey(retrievedMarkerId)) trackableMarkerDictionary[retrievedMarkerId] = markerObserverHelper.GetMarkerObjectFromMarkerContainer(retrievedMarkerId, observerTarget);
				}
			}
		}

		private void OnReceiveTrackMarkerEventFromPrefab(MarkerPrefab eventSource)
		{
			CreateTrackableMarker(eventSource.markerRef.markerId, eventSource.markerRef.target);
		}

		private void OnReceiveUntrackMarkerEventFromPrefab(MarkerPrefab eventSource)
		{
			DestroyTrackableMarker(eventSource.markerRef.markerId);
		}
	}
}
