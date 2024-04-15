
using HTC.UnityPlugin.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using Wave.Essence.ScenePerception;
using Wave.Native;
using static Wave.Essence.ScenePerception.ScenePerceptionManager;

namespace AnchorSharing
{
    //Start and Stop scenePerception
    //Load scene planes
    //Load scene meshes
    public class ScenePerceptionHelper : MonoBehaviour
    {
        [SerializeField] private ScenePerceptionManager scenePerceptionManager;

        [Header("Scene data source")]
        [SerializeField] private TextAsset sceneStatusData;

        [HideInInspector]
        public bool IsUseSavedSceneData;

        private SceneStatus sceneStatus;

        private void Awake()
        {
            sceneStatus = JsonUtility.FromJson<SceneStatus>(sceneStatusData.text);
        }

        public bool IsSceneStarted { get; private set; } = false;
        public bool IsPlanePerceptionStarted
        {
            get
            {
                if (scenePerceptionRunningMap.TryGetValue(WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane, out bool result))
                {
                    return result;
                }
                else
                {
                    return false;
                }
            }
            private set
            {
                scenePerceptionRunningMap[WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane] = value;
            }
        }
        public bool IsMeshPerceptionStarted
        {
            get
            {
                if (scenePerceptionRunningMap.TryGetValue(WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_SceneMesh, out bool result))
                {
                    return result;
                }
                else
                {
                    return false;
                }
            }
            private set
            {
                scenePerceptionRunningMap[WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_SceneMesh] = value;
            }
        }

        private Dictionary<WVR_ScenePerceptionTarget, bool> scenePerceptionRunningMap = new Dictionary<WVR_ScenePerceptionTarget, bool>();
        private Dictionary<WVR_ScenePerceptionTarget, WVR_ScenePerceptionState> scenePerceptionStateMap = new Dictionary<WVR_ScenePerceptionTarget, WVR_ScenePerceptionState>();
        private SceneMeshGenerationHelper meshGenerationHelper = new SceneMeshGenerationHelper();

        public void StartScene()
        {
            if (IsSceneStarted) return;

            if (IsUseSavedSceneData)
            {
                IsSceneStarted = true;
                return;
            }

            WVR_Result result = scenePerceptionManager.StartScene();
            if (result == WVR_Result.WVR_Success)
            {
                IsSceneStarted = true;
            }
            else
            {
                Debug.LogError("Start scene failed!");
            }
        }

        public void StopScene()
        {
            if (!IsSceneStarted) return;

            if (IsUseSavedSceneData)
            {
                IsSceneStarted = false;
                return;
            }

            if (!IsSceneStarted)
            {
                Debug.Log("Scene is not started.");
                return;
            }

            stopScenePerception();
            scenePerceptionManager.StopScene();
            IsSceneStarted = false;
        }

        public void LoadScenePlanes(Action<ScenePlaneData[]> onLoadCompletedHandler)
        {
            if (IsUseSavedSceneData)
            {
                onLoadCompletedHandler.Invoke(sceneStatus.scenePlanes);
                return;
            }

            startScenePerception(WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane);

            if (!IsPlanePerceptionStarted)
            {
                Debug.LogError($"Please start {WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane} before load scene planes");
                onLoadCompletedHandler.Invoke(null);
                return;
            }

            WVR_Result result = scenePerceptionManager.GetScenePlanes(ScenePerceptionManager.GetCurrentPoseOriginModel(), out WVR_ScenePlane[] currentScenePlanes);
            if (result != WVR_Result.WVR_Success)
            {
                Debug.LogError("Get scene plane error.");
                onLoadCompletedHandler.Invoke(null);
                return;
            }

            ScenePlaneData[] scenePlanes = new ScenePlaneData[currentScenePlanes.Length];
            for (int i = 0; i < currentScenePlanes.Length; ++i)
            {
                parseWVRPlaneToScenePlaneData(currentScenePlanes[i], out ScenePlaneData scenePlane);
                scenePlanes[i] = scenePlane;
            }

            onLoadCompletedHandler?.Invoke(scenePlanes);
        }

        public void LoadSceneMeshes(WVR_SceneMeshType meshType, Action<SceneMeshData[]> onLoadCompletedHandler)
        {
            if (IsUseSavedSceneData)
            {
                onLoadCompletedHandler.Invoke(sceneStatus.colliderMeshes);
                return;
            }

            startScenePerception(WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_SceneMesh);

            if (!IsMeshPerceptionStarted)
            {
                Debug.LogError($"Please start {WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_SceneMesh} before load scene meshes");
                onLoadCompletedHandler.Invoke(null);
                return;
            }

            if (scenePerceptionStateMap.TryGetValue(WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_SceneMesh, out WVR_ScenePerceptionState meshPerceptionState))
            {
                if (meshPerceptionState == WVR_ScenePerceptionState.WVR_ScenePerceptionState_Completed)
                {
                    WVR_Result meshResult = scenePerceptionManager.GetSceneMeshes(meshType, out WVR_SceneMesh[] sceneMeshes);
                    if (meshResult != WVR_Result.WVR_Success)
                    {
                        Debug.LogError("Get scene mesh error: GetSceneMeshes failed!");
                        onLoadCompletedHandler.Invoke(null);
                        return;
                    }

                    bool meshesValid = true;
                    foreach (WVR_SceneMesh sceneMesh in sceneMeshes)
                    {
                        if (sceneMesh.meshBufferId == 0)
                        {
                            meshesValid = false;
                            break;
                        }
                    }

                    if (!meshesValid)
                    {
                        Debug.LogError("Get scene mesh error: Scene meshes have invalid buffer ID");
                        onLoadCompletedHandler.Invoke(null);
                        return;
                    }


                    if (sceneMeshes.Length == 0)
                    {
                        Debug.Log("There is no valid scene mesh.");
                        onLoadCompletedHandler.Invoke(null);
                        return;
                    }

                    StartCoroutine(parseSceneMeshCoroutine(sceneMeshes, onLoadCompletedHandler));
                    return;
                }
                else if (meshPerceptionState == WVR_ScenePerceptionState.WVR_ScenePerceptionState_Empty) 
                {
                    Debug.LogError($"Get scene mesh error: scene mesh is empty");
                    onLoadCompletedHandler.Invoke(null);
                    return;
                }
                else
                {
                    Debug.LogError($"Get scene mesh error: state [{meshPerceptionState}] is invalid");
                    onLoadCompletedHandler.Invoke(null);
                    return;
                }
            }
        }

        private void stopScenePerception()
        {
            foreach (WVR_ScenePerceptionTarget target in scenePerceptionRunningMap.Keys)
            {
                if (scenePerceptionRunningMap[target])
                {
                    stopScenePerception(target);
                }
            }
        }

        private void startScenePerception(WVR_ScenePerceptionTarget target)
        {
            if (!IsSceneStarted)
            {
                Debug.LogError($"Start scenePerception [{target}] error, please call StartScene first.");
                return;
            }

            if (target == WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_SceneMesh && !SceneMeshPermissionHelper.IsPermissionGranted)
            {
                Debug.LogError($"Start scenePerception [{target}] error, please request permission first {WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_SceneMesh} {SceneMeshPermissionHelper.IsPermissionGranted}");
                return;
            }

            bool isRunning = false;
            if (!scenePerceptionRunningMap.TryGetValue(target, out isRunning) || isRunning == false)
            {
                WVR_Result result = scenePerceptionManager.StartScenePerception(target);

                if (result == WVR_Result.WVR_Success)
                {
                    Debug.Log($"Start scenePerception [{target}] success!");
                    scenePerceptionRunningMap[target] = true;
                    getScenePerceptionState(target);
                }
                else
                {
                    Debug.LogError($"Start scenePerception [{target}] error!");
                }
            }
            else
            {
                Debug.Log($"ScenePerception [{target}] is already started.");
            }
        }

        private void stopScenePerception(WVR_ScenePerceptionTarget target)
        {
            if (IsSceneStarted && scenePerceptionRunningMap[target])
            {
                WVR_Result result = scenePerceptionManager.StopScenePerception(target);

                if (result == WVR_Result.WVR_Success)
                {
                    scenePerceptionRunningMap[target] = false;
                }
            }
        }

        private void getScenePerceptionState(WVR_ScenePerceptionTarget target)
        {
            WVR_ScenePerceptionState latestPerceptionState = WVR_ScenePerceptionState.WVR_ScenePerceptionState_Empty;
            WVR_Result result = scenePerceptionManager.GetScenePerceptionState(WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane, ref latestPerceptionState);
            if (result == WVR_Result.WVR_Success)
            {
                scenePerceptionStateMap[target] = latestPerceptionState; //Update perception state for the perception target
            }
        }

        private void parseWVRPlaneToScenePlaneData(WVR_ScenePlane wvrPlane, out ScenePlaneData scenePlaneData)
        {
            scenePlaneData.uuID = wvrPlane.uuid.data;
            scenePlaneData.vertices = MeshGenerationHelper.GenerateQuadVertex(wvrPlane.extent);
            scenePerceptionManager.ApplyTrackingOriginCorrectionToPlanePose(wvrPlane, out scenePlaneData.position, out scenePlaneData.rotation);
            scenePlaneData.sceneLabel = wvrPlane.planeLabel;
        }

        private IEnumerator parseSceneMeshCoroutine(WVR_SceneMesh[] wvr_sceneMeshes, Action<SceneMeshData[]> onLoadCompletedHandler)
        {
            List<SceneMeshData> sceneMeshes = new List<SceneMeshData>();

            object resultCountLock = new object();
            int resultCount = 0;

            for (int i = 0; i < wvr_sceneMeshes.Length; i++)
            {
                meshGenerationHelper.GenerateMesh(wvr_sceneMeshes[i], (Vector3[] vertices, int[] indices, Vector2[] uvs, Vector4[] tangents) =>
                {
                    if (vertices != null && indices != null)
                    {
                        SceneMeshData sceneMeshData = new SceneMeshData();
                        sceneMeshData.vertices = vertices;
                        sceneMeshData.indices = indices;
                        sceneMeshData.uvs = uvs;
                        sceneMeshData.tangents = tangents;
                        sceneMeshes.Add(sceneMeshData);
                    }
                    lock (resultCountLock)
                    {
                        resultCount++;
                    }
                });
            }
            yield return new WaitUntil(() => resultCount == wvr_sceneMeshes.Length);
            //Back to main thread to prevent unexpected error

            onLoadCompletedHandler.Invoke(sceneMeshes.ToArray());
        }
    }
}
