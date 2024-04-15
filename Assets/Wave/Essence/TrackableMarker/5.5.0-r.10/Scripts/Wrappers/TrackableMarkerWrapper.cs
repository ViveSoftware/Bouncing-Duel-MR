using System;
using System.Runtime.InteropServices;
using Wave.Native;

namespace Wave.Essence.TrackableMarker.Wrapper
{
	public static class TrackableMarkerWrapper
	{
		#region Marker Service
		public static WVR_Result StartMarkerService()
		{
			return Interop.WVR_StartMarker();
		}

		public static void StopMarkerService()
		{
			Interop.WVR_StopMarker();
		}
		#endregion

		#region Marker Observer
		public static WVR_Result StartMarkerObserver(WVR_MarkerObserverTarget target)
		{
			return Interop.WVR_StartMarkerObserver(target);
		}

		public static WVR_Result GetMarkerObserverState(WVR_MarkerObserverTarget target, out WVR_MarkerObserverState state)
		{
			return Interop.WVR_GetMarkerObserverState(target, out state);
		}

		public static WVR_Result StopMarkerObserver(WVR_MarkerObserverTarget target)
		{
			return Interop.WVR_StopMarkerObserver(target);
		}
		#endregion

		#region Marker Detection
		public static WVR_Result StartMarkerDetection(WVR_MarkerObserverTarget target)
		{
			return Interop.WVR_StartMarkerDetection(target);
		}

		public static WVR_Result StopMarkerDetection(WVR_MarkerObserverTarget target)
		{
			return Interop.WVR_StopMarkerDetection(target);
		}
		#endregion

		#region Aruco Marker
		public static WVR_Result GetArucoMarkers(UInt32 markerCapacityInput, out UInt32 markerCountOutput /* uint32_t* */, WVR_PoseOriginModel originModel, IntPtr markers /* WVR_ArucoMarker* */)
		{
			return Interop.WVR_GetArucoMarkers(markerCapacityInput, out markerCountOutput, originModel, markers);
		}
		#endregion

		#region Trackable Markers
		public static WVR_Result EnumerateTrackableMarkers(WVR_MarkerObserverTarget target, UInt32 markerCapacityInput, out UInt32 markerCountOutput /* uint32_t* */, IntPtr markerIds /* WVR_Uuid* */)
		{
			return Interop.WVR_EnumerateTrackableMarkers(target, markerCapacityInput, out markerCountOutput, markerIds);
		}

		public static WVR_Result CreateTrackableMarker([In, Out] WVR_TrackableMarkerCreateInfo[] createInfo /* WVR_TrackableMarkerCreateInfo* */)
		{
			return Interop.WVR_CreateTrackableMarker(createInfo);
		}

		public static WVR_Result DestroyTrackableMarker(WVR_Uuid markerId)
		{
			return Interop.WVR_DestroyTrackableMarker(markerId);
		}

		public static WVR_Result StartTrackableMarkerTracking(WVR_Uuid markerId)
		{
			return Interop.WVR_StartTrackableMarkerTracking(markerId);
		}

		public static WVR_Result StopTrackableMarkerTracking(WVR_Uuid markerId)
		{
			return Interop.WVR_StopTrackableMarkerTracking(markerId);
		}

		public static WVR_Result GetTrackableMarkerState(WVR_Uuid markerId, WVR_PoseOriginModel originModel, out WVR_TrackableMarkerState state /* WVR_TrackableMarkerState* */)
		{
			return Interop.WVR_GetTrackableMarkerState(markerId, originModel, out state);
		}

		public static WVR_Result GetArucoMarkerData(WVR_Uuid markerId, out WVR_ArucoMarkerData data /* WVR_ArucoMarkerData* */)
		{
			return Interop.WVR_GetArucoMarkerData(markerId, out data);
		}
		#endregion

	}
}
