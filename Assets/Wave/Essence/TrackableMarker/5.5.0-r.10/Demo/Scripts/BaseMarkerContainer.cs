using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Native;

namespace Wave.Essence.TrackableMarker.Sample
{
	public class BaseMarkerContainer
	{
		protected MarkerObserverHelper markerObserverHelper;
		protected TrackableMarkerController trackableMarkerController;

		protected GameObject markerPrefab;

		protected BaseMarkerContainer(MarkerObserverHelper inObserverHelper, TrackableMarkerController inMarkerController, GameObject inAxisPrefab)
		{
			markerObserverHelper = inObserverHelper;
			trackableMarkerController = inMarkerController;
			markerPrefab = inAxisPrefab;
		}

		public virtual BaseMarker FindMarkerWithId(WVR_Uuid markerId)
		{
			return null;
		}
	}
}
