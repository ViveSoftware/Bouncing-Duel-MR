using System;
using UnityEngine;
using UnityEngine.UI;
using Wave.Native;

namespace Wave.Essence.TrackableMarker.Sample
{
	public class TrackableMarkerDemo : MonoBehaviour
	{
		[SerializeField] private TrackableMarkerController trackableMarkerController;
		[SerializeField] private PassthroughHelper passthroughHelper;
		[SerializeField] private GameObject markerPrefab;
		[SerializeField] private Text observerStateText;

		private MarkerObserverHelper markerObserverHelper = null;

		private RaycastHit leftControllerRaycastHitInfo = new RaycastHit(), rightControllerRaycastHitInfo = new RaycastHit();

		[SerializeField] private GameObject leftController = null, rightController = null;

		private const string LOG_TAG = "TrackableMarkerDemo";

		private void OnEnable()
		{
			if (markerObserverHelper == null)
			{
				markerObserverHelper = new MarkerObserverHelper(trackableMarkerController, markerPrefab);
			}

			if (markerObserverHelper != null)
			{
				markerObserverHelper.OnEnable();

				trackableMarkerController.OnGetMarkerObserverState += OnMarkerObserverStateUpdate;
				OnClickSwitchDetectionModeEvent += markerObserverHelper.HandleSwitchDetectionMode;
				OnClickSwitchTrackingModeEvent += markerObserverHelper.HandleSwitchTrackingMode;

			}

			passthroughHelper.ShowPassthroughUnderlay(true);
		}
		private void OnDisable()
		{
			if (markerObserverHelper != null)
			{
				markerObserverHelper.OnDisable();

				trackableMarkerController.OnGetMarkerObserverState -= OnMarkerObserverStateUpdate;
				OnClickSwitchDetectionModeEvent -= markerObserverHelper.HandleSwitchDetectionMode;
				OnClickSwitchTrackingModeEvent -= markerObserverHelper.HandleSwitchTrackingMode;
			}

			passthroughHelper.ShowPassthroughUnderlay(false);
		}

		private void OnApplicationPause(bool pause)
		{
			if (pause)
			{
				markerObserverHelper.OnDisable();
			}
			else
			{
				markerObserverHelper.OnEnable();
			}
		}

		private void Update()
		{
			markerObserverHelper.OnUpdate();
		}

		private void OnMarkerObserverStateUpdate(WVR_MarkerObserverTarget observerTarget, WVR_MarkerObserverState observerState, WVR_Result result)
		{
			if (result == WVR_Result.WVR_Success)
			{
				observerStateText.text = observerTarget.ToString() + " : " + observerState.ToString();
			}
		}

		public Action<bool> OnClickSwitchDetectionModeEvent;
		public void OnClickSwitchDetectionMode(bool start)
		{
			OnClickSwitchDetectionModeEvent?.Invoke(start);
		}

		public Action<bool> OnClickSwitchTrackingModeEvent;
		public void OnClickSwitchTrackingMode(bool start)
		{
			OnClickSwitchTrackingModeEvent?.Invoke(start);
		}

		private static class ButtonFacade
		{
			public static bool AButtonPressed =>
				WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Right, WVR_InputId.WVR_InputId_Alias1_A);
			public static bool BButtonPressed =>
				WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Right, WVR_InputId.WVR_InputId_Alias1_B);
			public static bool XButtonPressed =>
				WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Left, WVR_InputId.WVR_InputId_Alias1_X);
			public static bool YButtonPressed =>
				WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Left, WVR_InputId.WVR_InputId_Alias1_Y);
		}
	}
}
