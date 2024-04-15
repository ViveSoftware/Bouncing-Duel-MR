using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.TrackableMarker.Sample
{
	public class ArucoMarkerContainer : BaseMarkerContainer
	{
		private Dictionary<WVR_Uuid, ArucoMarker> arucoMarkers;

		private const string LOG_TAG = "ArucoMarkerContainer";

		public ArucoMarkerContainer(MarkerObserverHelper inObserverHelper, TrackableMarkerController inMarkerController, GameObject inAxisPrefab) : base(inObserverHelper, inMarkerController, inAxisPrefab)
		{
			arucoMarkers = new Dictionary<WVR_Uuid, ArucoMarker>(new WVRStructCompare.MarkerIdComparer());
		}

		public void ClearMarkerDictionary()
		{
			foreach(ArucoMarker arucoMarker in arucoMarkers.Values)
			{
				arucoMarker.Dispose();
			}
			arucoMarkers.Clear();
		}

		enum ArucoMarkerAction { NONE, ADD, REMOVE, UPDATE }
		IEnumerable<Tuple<ArucoMarkerAction, WVR_ArucoMarker, ArucoMarker>> ArucoMarkerActionEnumerator()
		{
			WVR_Result result = trackableMarkerController.GetArucoMarkers(TrackableMarkerController.GetCurrentPoseOriginModel(), out WVR_ArucoMarker[] latestArucoMarkers);

			if (result != WVR_Result.WVR_Success)
			{
				//Log.e(LOG_TAG, "Failed to get aruco markers");
				yield break;
			}

			//Check if generated plane still exsits
			List<WVR_Uuid> arucoMarkerIdToRemove = new List<WVR_Uuid>();
			foreach (WVR_Uuid arucoMarkerId in arucoMarkers.Keys)
			{
				bool arucoMarkerExists = false;
				foreach (WVR_ArucoMarker marker in latestArucoMarkers)
				{
					if (WVRStructCompare.IsUUIDEqual(arucoMarkerId, marker.uuid)) //plane still exists
					{
						arucoMarkerExists = true;
						break;
					}
				}

				if (!arucoMarkerExists)
				{
					arucoMarkerIdToRemove.Add(arucoMarkerId);
				}
			}

			foreach (WVR_Uuid markerId in arucoMarkerIdToRemove) //Remove all planes that no longer exists
			{
				yield return new Tuple<ArucoMarkerAction, WVR_ArucoMarker, ArucoMarker>(ArucoMarkerAction.REMOVE, default, arucoMarkers[markerId]);
			}

			//Process retrieved scene planes
			for (var index = 0; index < latestArucoMarkers.Length; index++)
			{
				WVR_ArucoMarker currentArucoMarker = latestArucoMarkers[index];
				if (!arucoMarkers.ContainsKey(currentArucoMarker.uuid))
				{
					yield return new Tuple<ArucoMarkerAction, WVR_ArucoMarker, ArucoMarker>(ArucoMarkerAction.ADD, currentArucoMarker, null);
				}
				else
				{
					ArucoMarker existingArucoMarker = arucoMarkers[currentArucoMarker.uuid];
					if (currentArucoMarker.size != existingArucoMarker.size || !WVRStructCompare.WVRPoseEqual(currentArucoMarker.pose, existingArucoMarker.pose) || currentArucoMarker.state != existingArucoMarker.state)
					{
						yield return new Tuple<ArucoMarkerAction, WVR_ArucoMarker, ArucoMarker>(ArucoMarkerAction.UPDATE, currentArucoMarker, existingArucoMarker);
					}
					else
					{
						yield return new Tuple<ArucoMarkerAction, WVR_ArucoMarker, ArucoMarker>(ArucoMarkerAction.NONE, currentArucoMarker, existingArucoMarker);
					}
				}
			}
		}

		public void UpdateArucoMarkers()
		{
			foreach (Tuple<ArucoMarkerAction, WVR_ArucoMarker, ArucoMarker> arucoMarkerAction in ArucoMarkerActionEnumerator())
			{
				ArucoMarkerAction action = arucoMarkerAction.Item1;
				WVR_ArucoMarker wvrArucoMarker = arucoMarkerAction.Item2;
				ArucoMarker arucoMarker = arucoMarkerAction.Item3;

				switch (action)
				{
					case ArucoMarkerAction.ADD:
						{
							Log.d(LOG_TAG, "ArucoMarkerAction.ADD");
							ArucoMarker newArucoMarker = new ArucoMarker(trackableMarkerController, markerPrefab, wvrArucoMarker);
							arucoMarkers[wvrArucoMarker.uuid] = newArucoMarker;
							break;
						}
					case ArucoMarkerAction.REMOVE:
						{
							Log.d(LOG_TAG, "ArucoMarkerAction.REMOVE");
							arucoMarkers.Remove(arucoMarker.markerId);
							arucoMarker.Dispose();
							break;
						}
					case ArucoMarkerAction.UPDATE:
						{
							Log.d(LOG_TAG, "ArucoMarkerAction.UPDATE");
							arucoMarker.UpdateArucoMarker(wvrArucoMarker);
							break;
						}
					case ArucoMarkerAction.NONE:
						//Log.d(LOG_TAG, "ArucoMarkerAction.NONE");
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public override BaseMarker FindMarkerWithId(WVR_Uuid markerId)
		{
			return arucoMarkers[markerId];
		}
	}
}
