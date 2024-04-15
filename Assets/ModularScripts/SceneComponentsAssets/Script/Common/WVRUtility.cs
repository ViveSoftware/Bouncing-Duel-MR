using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using Wave.Native;

namespace AnchorSharing
{
    public static class WVRUtility
    {
        #if UNITY_EDITOR
        private const bool USE_WVR = false;
        #else
        private const bool USE_WVR = true;
        #endif

        public const string INVALID_RESULT = "unknown";

        private const uint MAX_RESULT_BUFFER_SIZE = 2560;

        public static async Task<string> WVR_GetParameterDynamicSize(string encodedParam)
        {
            if (!USE_WVR)
            {
                return null;
            }

            Debug.LogFormat("Pass command: {0}", encodedParam);

            IntPtr ptrParam = Marshal.StringToHGlobalAnsi(encodedParam);
            string result = string.Empty;

            uint return_str_length = Interop.WVR_GetParameters(WVR_DeviceType.WVR_DeviceType_HMD, ptrParam, (IntPtr)null, 0);
            Debug.LogFormat("Result str length = {0}", return_str_length);

            if (return_str_length > 0)
            {
                IntPtr ptrResult = Marshal.AllocHGlobal(new IntPtr(return_str_length));

                return_str_length = Interop.WVR_GetParameters(WVR_DeviceType.WVR_DeviceType_HMD, ptrParam, ptrResult, return_str_length);
                await Task.Yield();

                result = Marshal.PtrToStringAnsi(ptrResult);
                Debug.LogFormat("Result str = {0}", result);

                Marshal.FreeHGlobal(ptrResult);
            }

            Marshal.FreeHGlobal(ptrParam);
            return result;
        }

        public static string WVR_GetParameter(string encodedParam)
        {
            if (!USE_WVR)
            {
                return null;
            }

            Debug.LogFormat("Pass command: {0}", encodedParam);

            IntPtr ptrParam = Marshal.StringToHGlobalAnsi(encodedParam);
            IntPtr ptrResult = Marshal.AllocHGlobal(new IntPtr(MAX_RESULT_BUFFER_SIZE));
            string result = string.Empty;

            uint return_str_length = Interop.WVR_GetParameters(WVR_DeviceType.WVR_DeviceType_HMD, ptrParam, ptrResult, MAX_RESULT_BUFFER_SIZE);
            Debug.LogFormat("Result str length = {0}", return_str_length);

            if (return_str_length > 0)
            {
                result = Marshal.PtrToStringAnsi(ptrResult);
                Debug.LogFormat("Result str = {0}", result);
            }

            Marshal.FreeHGlobal(ptrResult);
            Marshal.FreeHGlobal(ptrParam);

            return result;
        }

        public static void WVR_SetParameter(string encodedParam)
        {
            if (!USE_WVR)
            {
                return;
            }

            IntPtr ptrParam = Marshal.StringToHGlobalAnsi(encodedParam);
            Interop.WVR_SetParameters(WVR_DeviceType.WVR_DeviceType_HMD, ptrParam);
            Marshal.FreeHGlobal(ptrParam);
        }

        public static bool GetMapStatus(out MapStatus mapStatus)
        {
            string mapStatusJsonStr = WVR_GetParameter("MapState");

            //Normally, if the new map is invalid, system will force user to RoomSetup, this part should be never enter.
            if (string.IsNullOrEmpty(mapStatusJsonStr))
            {
                Debug.LogError("[WVRUtility.GetMapStatus] MapState is null");
                mapStatus = MapStatus.INVALID;
                return false;
            }

            mapStatus = new MapStatus();
            JsonUtility.FromJsonOverwrite(mapStatusJsonStr, mapStatus);
            return true;
        }
    }
}
