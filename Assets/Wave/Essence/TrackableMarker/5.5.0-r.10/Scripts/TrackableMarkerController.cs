using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR;
using Wave.Essence.TrackableMarker.Wrapper;
using Wave.Native;
using Wave.XR;

namespace Wave.Essence.TrackableMarker
{
	public class TrackableMarkerController : MonoBehaviour
	{
		[SerializeField]
		public GameObject trackingOrigin = null;

		private const string LOG_TAG = "TrackableMarkerController";

		#region Marker Service
		public Action<WVR_Result> OnStartMarkerService;
		/// <summary>
		/// Start the Marker Service.
		/// Should be called before using other trackable marker related APIs.
		/// </summary>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if Marker Service is started successfully.
		/// </returns>
		public WVR_Result StartMarkerService()
		{
			WVR_Result result = TrackableMarkerWrapper.StartMarkerService();
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "StartMarkerService failed with result " + result.ToString());
			}
			else
			{
				//Log.d(LOG_TAG, "StartMarkerService successful.");
			}

			OnStartMarkerService?.Invoke(result);

			return result;
		}

		public Action OnStopMarkerService;
		/// <summary>
		/// Stop the Marker Service.
		/// Should be called when Marker related features are no longer in use.
		/// </summary>
		public void StopMarkerService()
		{
			TrackableMarkerWrapper.StopMarkerService();

			//Log.d(LOG_TAG, "StopMarkerService.");

			OnStopMarkerService?.Invoke();
		}
		#endregion

		#region Marker Observer
		public Action<WVR_MarkerObserverTarget, WVR_Result> OnStartMarkerObserver;
		/// <summary>
		/// Start observering markers of a specific target.
		/// Should be called after <see cref="StartMarkerService()"/> is called successfully. 
		/// See <see cref="WVR_MarkerObserverTarget"/> for the supported target types.
		/// </summary>
		/// <param name="target">
		/// The target to be observed.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the observer is started successfully.
		/// </returns>
		public WVR_Result StartMarkerObserver(WVR_MarkerObserverTarget target)
		{
			WVR_Result result = TrackableMarkerWrapper.StartMarkerObserver(target);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "StartMarkerObserver failed with result " + result.ToString());
			}
			else
			{
				//Log.d(LOG_TAG, "StartMarkerObserver successful.");
			}

			OnStartMarkerObserver?.Invoke(target, result);

			return result;
		}

		public Action<WVR_MarkerObserverTarget, WVR_MarkerObserverState, WVR_Result> OnGetMarkerObserverState;
		/// <summary>
		/// Get the current state of the observer of a specific target type.
		/// </summary>
		/// <param name="target">
		/// The target of the observer. 
		/// See <see cref="WVR_MarkerObserverTarget"/> for the supported target types.
		/// </param>
		/// <param name="state">
		/// The state of the observer of the target type. See <see cref="WVR_MarkerObserverState"/> for the types of states.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the state is retrieved successfully.
		/// </returns>
		public WVR_Result GetMarkerObserverState(WVR_MarkerObserverTarget target, out WVR_MarkerObserverState state)
		{
			WVR_Result result = TrackableMarkerWrapper.GetMarkerObserverState(target, out state);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "GetMarkerObserverState failed with result " + result.ToString());
			}
			else
			{
				//Log.d(LOG_TAG, "GetMarkerObserverState successful.");
			}

			OnGetMarkerObserverState?.Invoke(target, state, result);

			return result;
		}

		public Action<WVR_MarkerObserverTarget, WVR_Result> OnStopMarkerObserver;
		/// <summary>
		/// Stop observering markers of a specific target.
		/// See <see cref="WVR_MarkerObserverTarget"/> for the supported target types.
		/// </summary>
		/// <param name="target">
		/// The target type that should no longer be observed.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the observer is stopped successfully.
		/// </returns>
		public WVR_Result StopMarkerObserver(WVR_MarkerObserverTarget target)
		{
			WVR_Result result = TrackableMarkerWrapper.StopMarkerObserver(target);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "StopMarkerObserver failed with result " + result.ToString());
			}
			else
			{
				//Log.d(LOG_TAG, "StopMarkerObserver successful.");
			}

			OnStopMarkerObserver?.Invoke(target, result);

			return result;
		}
		#endregion

		#region Marker Detection
		public Action<WVR_MarkerObserverTarget, WVR_Result> OnStartMarkerDetection;
		/// <summary>
		/// Start detecting markers of a specific target.
		/// Should be called after <see cref="StartMarkerObserver(WVR_MarkerObserverTarget)()"/> is called successfully. 
		/// See <see cref="WVR_MarkerObserverTarget"/> for the supported target types.
		/// </summary>
		/// <param name="target">
		/// The target to be detected.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if marker detection is started successfully.
		/// </returns>
		public WVR_Result StartMarkerDetection(WVR_MarkerObserverTarget target)
		{
			WVR_Result result = TrackableMarkerWrapper.StartMarkerDetection(target);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "StartMarkerDetection failed with result " + result.ToString());
			}
			else
			{
				//Log.d(LOG_TAG, "StartMarkerDetection successful.");
			}

			OnStartMarkerDetection?.Invoke(target, result);

			return result;
		}

		public Action<WVR_MarkerObserverTarget, WVR_Result> OnStopMarkerDetection;
		/// <summary>
		/// Stop detecting markers of a specific target.
		/// See <see cref="WVR_MarkerObserverTarget"/> for the supported target types.
		/// </summary>
		/// <param name="target">
		/// The target that should no longer be detected.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the mark detection is stopped successfully.
		/// </returns>
		public WVR_Result StopMarkerDetection(WVR_MarkerObserverTarget target)
		{
			WVR_Result result = TrackableMarkerWrapper.StopMarkerDetection(target);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "StopMarkerDetection failed with result " + result.ToString());
			}
			else
			{
				//Log.d(LOG_TAG, "StopMarkerDetection successful.");
			}

			OnStopMarkerDetection?.Invoke(target, result);

			return result;
		}
		#endregion

		#region Aruco Marker
		public Action<WVR_ArucoMarker[], WVR_Result> OnGetArucoMarkers;
		/// <summary>
		/// Get all of the Aruco Markers that observed by the marker observer.
		/// </summary>
		/// <param name="originModel">
		/// Origin Model used for the pose data of the aruco markers.
		/// You can use <see cref="GetCurrentPoseOriginModel"/> to get the <see cref="WVR_PoseOriginModel"/> that matches the <see cref="TrackingOriginModeFlags">Tracking Origin Mode</see> in Unity.
		/// </param>
		/// <param name="markers">
		/// An array of <see cref="WVR_ArucoMarker">Aruco Marker</see> retrieved from the device.
		/// When the marking observer is in <see cref="WVR_MarkerObserverState.WVR_MarkerObserverState_Detecting"> state, all detected Aruco markers will be retrieved.
		/// When the marking observer is in <see cref="WVR_MarkerObserverState.WVR_MarkerObserverState_Tracking"> state, 
		/// only the Aruco markers that have been set to Trackable Markers by calling <see cref="CreateTrackableMarker(WVR_Uuid, char[])"/> will be retrieved.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the Aruco Markers are retrieved successfully.
		/// </returns>
		public WVR_Result GetArucoMarkers(WVR_PoseOriginModel originModel, out WVR_ArucoMarker[] markers)
		{
			UInt32 markerCount = 0;
			markers = new WVR_ArucoMarker[1]; //Empty array

			WVR_Result result = TrackableMarkerWrapper.GetArucoMarkers(0, out markerCount, originModel, IntPtr.Zero); //Get ArUco Marker Count
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "GetArucoMarkers 1 failed with result " + result.ToString());
			}
			else
			{
				//Log.d(LOG_TAG, "GetArucoMarkers 1 successful. Marker Count Output: " + markerCount);
			}

			Array.Resize(ref markers, (int)markerCount);
			if (markerCount <= 0) return result; //No need to further get markers if there are no markers.

			WVR_ArucoMarker defaultArucoMarker = default(WVR_ArucoMarker);
			WVR_ArucoMarker[] outMarkers = new WVR_ArucoMarker[markers.Length];
			IntPtr markersPtr = Marshal.AllocHGlobal(Marshal.SizeOf(defaultArucoMarker) * outMarkers.Length);

			long offset = 0;
			if (IntPtr.Size == 4)
				offset = markersPtr.ToInt32();
			else
				offset = markersPtr.ToInt64();

			for (int i = 0; i < outMarkers.Length; i++)
			{
				IntPtr markerPtr = new IntPtr(offset);

				Marshal.StructureToPtr(outMarkers[i], markerPtr, false);

				offset += Marshal.SizeOf(defaultArucoMarker);
			}

			result = TrackableMarkerWrapper.GetArucoMarkers(markerCount, out markerCount, originModel, markersPtr); //Get ArUco Markers
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "GetArucoMarkers 2 failed with result " + result.ToString());
			}
			else
			{
				//Log.d(LOG_TAG, "GetArucoMarkers 2 successful.");
			}

			if (IntPtr.Size == 4)
				offset = markersPtr.ToInt32();
			else
				offset = markersPtr.ToInt64();

			for (int i = 0; i < outMarkers.Length; i++)
			{
				IntPtr markerPtr = new IntPtr(offset);

				outMarkers[i] = (WVR_ArucoMarker)Marshal.PtrToStructure(markerPtr, typeof(WVR_ArucoMarker));

				offset += Marshal.SizeOf(defaultArucoMarker);
			}

			markers = outMarkers;

			Marshal.FreeHGlobal(markersPtr);

			OnGetArucoMarkers?.Invoke(markers, result);

			return result;
		}
		#endregion

		#region Trackable Markers
		public Action<WVR_MarkerObserverTarget, WVR_Uuid[], WVR_Result> OnEnumerateTrackableMarkers;
		/// <summary>
		/// Get the uuids of the existing Trackable Markers.
		/// </summary>
		/// <param name="target">
		/// The target type of the Trackable Markers.
		/// </param>
		/// <param name="markerIds">
		/// An array of <see cref="WVR_Uuid">uuid</see> which represents the Trackable Markers.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the uuids of the Trackable Markers are retrieved successfully.
		/// </returns>
		public WVR_Result GetTrackableMarkers(WVR_MarkerObserverTarget target, out WVR_Uuid[] markerIds)
		{
			UInt32 markerCount = 0;
			markerIds = new WVR_Uuid[1]; //Empty array

			WVR_Result result = TrackableMarkerWrapper.EnumerateTrackableMarkers(target, 0, out markerCount, IntPtr.Zero); //Get Trackable Marker Count
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "GetTrackableMarkers 1 failed with result " + result.ToString());
			}
			else
			{
				//Log.d(LOG_TAG, "GetTrackableMarkers 1 successful. Marker Count Output: " + markerCount);
			}

			Array.Resize(ref markerIds, (int)markerCount);
			if (markerCount <= 0) return result; //No need to further get markers if there are no markers.

			WVR_Uuid defaultTrackableMarkerId = default(WVR_Uuid);
			WVR_Uuid[] outMarkerIds = new WVR_Uuid[markerIds.Length];
			IntPtr markerIdsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(defaultTrackableMarkerId) * outMarkerIds.Length);

			long offset = 0;
			if (IntPtr.Size == 4)
				offset = markerIdsPtr.ToInt32();
			else
				offset = markerIdsPtr.ToInt64();

			for (int i = 0; i < outMarkerIds.Length; i++)
			{
				IntPtr markerPtr = new IntPtr(offset);

				Marshal.StructureToPtr(outMarkerIds[i], markerPtr, false);

				offset += Marshal.SizeOf(defaultTrackableMarkerId);
			}

			result = TrackableMarkerWrapper.EnumerateTrackableMarkers(target, markerCount, out markerCount, markerIdsPtr); //Get trackable Markers
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "GetTrackableMarkers 2 failed with result " + result.ToString());
			}
			else
			{
				//Log.d(LOG_TAG, "GetTrackableMarkers 2 successful.");
			}

			if (IntPtr.Size == 4)
				offset = markerIdsPtr.ToInt32();
			else
				offset = markerIdsPtr.ToInt64();

			for (int i = 0; i < outMarkerIds.Length; i++)
			{
				IntPtr markerIdPtr = new IntPtr(offset);

				outMarkerIds[i] = (WVR_Uuid)Marshal.PtrToStructure(markerIdPtr, typeof(WVR_Uuid));

				offset += Marshal.SizeOf(defaultTrackableMarkerId);
			}

			markerIds = outMarkerIds;

			Marshal.FreeHGlobal(markerIdsPtr);

			OnEnumerateTrackableMarkers?.Invoke(target, markerIds, result);

			return result;
		}

		public Action<WVR_Uuid, char[], WVR_Result> OnCreateTrackableMarker;
		/// <summary>
		/// Create a Trackable Marker from a detected Marker (e.g. an Aruco Marker).
		/// </summary>
		/// <param name="markerId">
		/// The uuid of a detected Marker.
		/// </param>
		/// <param name="markerName">
		/// The name of the Trackable Marker, must be within 256 characters including the null terminator.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the Trackable Marker is created successfully.
		/// </returns>
		public WVR_Result CreateTrackableMarker(WVR_Uuid markerId, char[] markerName)
		{
			if (markerName.Length > 256)
			{
				Log.e(LOG_TAG, "CreateTrackableMarker: marker name should be under 256 characters.");
				OnCreateTrackableMarker?.Invoke(markerId, markerName, WVR_Result.WVR_Error_InvalidArgument);
				return WVR_Result.WVR_Error_InvalidArgument;
			}

			WVR_MarkerName markerNameWVR = default(WVR_MarkerName);
			markerNameWVR.name = new char[256];
			markerName.CopyTo(markerNameWVR.name, 0);

			WVR_TrackableMarkerCreateInfo trackableMarkerCreateInfo = default(WVR_TrackableMarkerCreateInfo);
			trackableMarkerCreateInfo.uuid = markerId;
			trackableMarkerCreateInfo.markerName = markerNameWVR;

			WVR_TrackableMarkerCreateInfo[] trackableMarkerCreateInfoArray = { trackableMarkerCreateInfo };

			WVR_Result result = TrackableMarkerWrapper.CreateTrackableMarker(trackableMarkerCreateInfoArray);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "CreateTrackableMarker failed with result " + result.ToString());
				return result;
			}
			else
			{
				//Log.d(LOG_TAG, "CreateTrackableMarker successful");
			}

			OnCreateTrackableMarker?.Invoke(markerId, markerName, result);

			return result;
		}

		public Action<WVR_Uuid, WVR_Result> OnDestroyTrackableMarker;
		/// <summary>
		/// Destroy a Trackable Marker.
		/// </summary>
		/// <param name="markerId">
		/// The uuid of the Trackable Marker to be destroyed.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the Trackable Marker is destroyed successfully.
		/// </returns>
		public WVR_Result DestroyTrackableMarker(WVR_Uuid markerId)
		{
			WVR_Result result = TrackableMarkerWrapper.DestroyTrackableMarker(markerId);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "DestroyTrackableMarker failed with result " + result.ToString());
			}
			else
			{
				//Log.d(LOG_TAG, "DestroyTrackableMarker successful.");
			}

			OnDestroyTrackableMarker?.Invoke(markerId, result);

			return result;
		}

		public Action<WVR_Uuid, WVR_Result> OnStartTrackableMarkerTracking;
		/// <summary>
		/// Start tracking a Trackable Marker.
		/// </summary>
		/// <param name="markerId">
		/// The uuid of the Trackable Marker to be tracked.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the Trackable Marker is being tracked successfully.
		/// </returns>
		public WVR_Result StartTrackableMarkerTracking(WVR_Uuid markerId)
		{
			WVR_Result result = TrackableMarkerWrapper.StartTrackableMarkerTracking(markerId);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "StartTrackableMarkerTracking failed with result " + result.ToString());
			}
			else
			{
				//Log.d(LOG_TAG, "StartTrackableMarkerTracking successful.");
			}

			OnStartTrackableMarkerTracking?.Invoke(markerId, result);

			return result;
		}

		public Action<WVR_Uuid, WVR_Result> OnStopTrackableMarkerTracking;
		/// <summary>
		/// Stop tracking a Trackable Marker.
		/// </summary>
		/// <param name="markerId">
		/// The uuid of the Trackable Marker which should no longer be tracked.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the tracking of the Trackable Marker is stopped successfully.
		/// </returns>
		public WVR_Result StopTrackableMarkerTracking(WVR_Uuid markerId)
		{
			WVR_Result result = TrackableMarkerWrapper.StopTrackableMarkerTracking(markerId);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "StopTrackableMarkerTracking failed with result " + result.ToString());
			}
			else
			{
				//Log.d(LOG_TAG, "StopTrackableMarkerTracking successful.");
			}

			OnStopTrackableMarkerTracking?.Invoke(markerId, result);

			return result;
		}

		public Action<WVR_Uuid, WVR_TrackableMarkerState, WVR_Result> OnGetTrackableMarkerState;
		/// <summary>
		/// Get the state of a Trackable Marker.
		/// </summary>
		/// <param name="markerId">
		/// The uuid of the Trackable Marker.
		/// </param>
		/// <param name="originModel">
		/// Origin Model used for the pose data of the Trackable Marker.
		/// You can use <see cref="GetCurrentPoseOriginModel"/> to get the <see cref="WVR_PoseOriginModel"/> that matches the <see cref="TrackingOriginModeFlags">Tracking Origin Mode</see> in Unity.
		/// </param>
		/// <param name="state">
		/// The state of the Trackable Marker.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the state of the Trackable Marker is retrieved successfully.
		/// </returns>
		public WVR_Result GetTrackableMarkerState(WVR_Uuid markerId, WVR_PoseOriginModel originModel, out WVR_TrackableMarkerState state)
		{
			WVR_Result result = TrackableMarkerWrapper.GetTrackableMarkerState(markerId, originModel, out state);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "GetTrackableMarkerState failed with result " + result.ToString());
			}
			else
			{
				//Log.d(LOG_TAG, "GetTrackableMarkerState successful.");
			}

			OnGetTrackableMarkerState?.Invoke(markerId, state, result);

			return result;
		}

		public Action<WVR_Uuid, WVR_ArucoMarkerData, WVR_Result> OnGetArucoMarkerData;
		/// <summary>
		/// Get the data of a Aruco Marker.
		/// This API is designed to be used for retrieving additional data of a Trackable Marker if it is created from a Aruco Marker.
		/// </summary>
		/// <param name="markerId">
		/// The uuid of the Aruco Marker.
		/// </param>
		/// <param name="data">
		/// The data of the Aruco Marker.
		/// </param>
		/// <returns>
		/// The result of the function call. Will return <see cref="WVR_Result.WVR_Success"></see> if the data of the Aruco Marker is retrieved successfully.
		/// </returns>
		public WVR_Result GetArucoMarkerData(WVR_Uuid markerId, out WVR_ArucoMarkerData data)
		{
			WVR_Result result = TrackableMarkerWrapper.GetArucoMarkerData(markerId, out data);
			if (result != WVR_Result.WVR_Success)
			{
				Log.e(LOG_TAG, "GetArucoMarkerData failed with result " + result.ToString());
			}
			else
			{
				//Log.d(LOG_TAG, "GetArucoMarkerData successful.");
			}

			OnGetArucoMarkerData?.Invoke(markerId, data, result);

			return result;
		}
		#endregion

		#region Utility Functions
		/// <summary>
		/// A helper function which outputs the world space position and rotation of a <see cref="WVR_SpatialAnchorState"/>.
		/// The <see cref="trackingOrigin">Tracking Origin reference</see> also needs to be assigned to the <see cref="TrackableMarkerController"/> instance in order for this function to work as intended.
		/// </summary>
		/// <param name="markerPose">
		/// The target <see cref="WVR_Pose_t"/> of a marker which will be used in the conversion.
		/// </param>
		/// <param name="markerPosition">
		/// The world space position of the marker.
		/// The tracking space position will be returned instead if the <see cref="trackingOrigin">Tracking Origin reference</see> is not assigned to the <see cref="TrackableMarkerController"/> instance.
		/// </param>
		/// <param name="markerRotation">
		/// The world space rotation of the marker.
		/// The tracking space rotation will be returned instead if the <see cref="trackingOrigin">Tracking Origin reference</see> is not assigned to the <see cref="TrackableMarkerController"/> instance.
		/// </param>
		public void ApplyTrackingOriginCorrectionToMarkerPose(WVR_Pose_t markerPose, out Vector3 markerPosition, out Quaternion markerRotation)
		{
			Coordinate.GetVectorFromGL(markerPose.position, out markerPosition);
			Coordinate.GetQuaternionFromGL(markerPose.rotation, out markerRotation);

			markerRotation *= Quaternion.Euler(0, 180f, 0);

			if (trackingOrigin != null)
			{
				Matrix4x4 trackingSpaceOriginTRS = Matrix4x4.TRS(trackingOrigin.transform.position, trackingOrigin.transform.rotation, Vector3.one);

				Matrix4x4 trackingSpaceMarkerPoseTRS = Matrix4x4.TRS(markerPosition, markerRotation, Vector3.one);
				Matrix4x4 worldSpaceMarkerPoseTRS = trackingSpaceOriginTRS * trackingSpaceMarkerPoseTRS;

				markerPosition = worldSpaceMarkerPoseTRS.GetColumn(3); //4th Column of TRS Matrix is the position
				markerRotation = Quaternion.LookRotation(worldSpaceMarkerPoseTRS.GetColumn(2), worldSpaceMarkerPoseTRS.GetColumn(1));
			}
		}

		/// <summary>
		/// Get the <see cref="WVR_PoseOriginModel"/> in respect to the current <see cref="TrackingOriginModeFlags"/> in Unity.
		/// </summary>
		/// <returns>
		/// The <see cref="WVR_PoseOriginModel"/> in respect to the current <see cref="TrackingOriginModeFlags"/> in Unity.
		/// </returns>
		public static WVR_PoseOriginModel GetCurrentPoseOriginModel()
		{
			XRInputSubsystem subsystem = Utils.InputSubsystem;
			WVR_PoseOriginModel currentPoseOriginModel = WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnGround;

			if (subsystem != null)
			{
				TrackingOriginModeFlags trackingOriginMode = subsystem.GetTrackingOriginMode();


				bool getOriginSuccess = ClientInterface.GetOrigin(trackingOriginMode, ref currentPoseOriginModel);

				if (getOriginSuccess)
				{
					return currentPoseOriginModel;
				}
			}

			return currentPoseOriginModel;
		}

		/// <summary>
		/// A helper function for comparing two <see cref="WVR_Uuid"/>.
		/// </summary>
		/// <param name="uuid1">A <see cref="WVR_Uuid"/> of which will be in the comparison.</param>
		/// <param name="uuid2">A <see cref="WVR_Uuid"/> of which will be in the comparison.</param>
		/// <returns>
		/// true if the Uuids are the identical, false if they are not.
		/// </returns>
		public static bool IsUUIDEqual(WVR_Uuid uuid1, WVR_Uuid uuid2)
		{
			return WVRStructCompare.IsUUIDEqual(uuid1, uuid2);
		}

		#endregion
	}
}
