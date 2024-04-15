using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Essence;

namespace AnchorSharing
{
    public static class SceneMeshPermissionHelper
    {
#if UNITY_EDITOR
        public static bool IsPermissionGranted { get; private set; } = true;
#else
        public static bool IsPermissionGranted { get; private set; } = false;
#endif

        private static event Action<bool> onRequestCompleted; 

        private static PermissionManager pmInstance = null;

        private const string scenePerceptionPermissionString = "wave.permission.GET_SCENE_MESH";

        public static void RequestSceneMeshPermission(Action<bool> requestCompleteHanlder = null)
        {
            Debug.Log("Request scene mesh Permission");

            if(IsPermissionGranted)
            {
                requestCompleteHanlder?.Invoke(true);
                return;
            }

            string[] permArray = {
               scenePerceptionPermissionString
            };
            
            if(requestCompleteHanlder != null)
            {
                onRequestCompleted += requestCompleteHanlder;
            }

            pmInstance = PermissionManager.instance;
            pmInstance?.requestPermissions(permArray, requestDoneCallback);
        }

        private static void requestDoneCallback(List<PermissionManager.RequestResult> results)
        {
            foreach (PermissionManager.RequestResult permissionRequestResult in results)
            {
                if (permissionRequestResult.PermissionName.Equals(scenePerceptionPermissionString))
                {                                        
                    Debug.Log("Scene mesh permission granted = " + IsPermissionGranted);

                    IsPermissionGranted = permissionRequestResult.Granted;
                    break;
                }
            }
            onRequestCompleted?.Invoke(IsPermissionGranted);
            onRequestCompleted = delegate { };
        }
    }
}
