using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.TrackableMarker.Sample
{
	public static class MarkerPrefabEventMediator
	{
		//Invoked by marker prefab
		public static Action<MarkerPrefab> OnClickTrackMarkerEvent;
		public static Action<MarkerPrefab> OnClickUntrackMarkerEvent;
	}
}
