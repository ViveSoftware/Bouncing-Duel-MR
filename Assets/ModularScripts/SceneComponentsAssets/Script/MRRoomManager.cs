using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Wave.Essence.ScenePerception;
using Wave.Native;

namespace AnchorSharing
{
    public class MRRoomManager : MonoBehaviour
    {
        private static MRRoomManager instance = null;

        public static MRRoomManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MRRoomManager();
                }
                return instance;
            }
        }

        public bool IsUseScenePerception { get; private set; }
        public bool IsLoadSceneComponentEnabled { get; private set; } = false;
        public bool IsLoadColliderMeshEnabled { get; private set; } = false;
        public bool IsLoadVisualMeshEnabled { get; private set; } = false;
        public string GameVersion { get; private set; } = "1";
        public string RoomNumber { get; private set; } = string.Empty;
        public byte MaxPlayersPerRoom { get; private set; } = 4;
        public bool InitializationComplete { get; private set; } = false;
        public List<string> ScenesName { get; private set; } = new List<string>() {
            "ShareAnchorGame-Launcher",
            "ShareAnchorGame-RoomSetup&Gaming" };

        [Header("Game system reference")]
        [SerializeField] private ScenePerceptionHelper scenePerceptionHelper;
        [SerializeField] private SceneComponentManager sceneComponentManager;
        [SerializeField] private ScenePerceptionManager scenePerceptionManager;

        [Header("Game setting")]
        [SerializeField] private bool isUseScenePerception = false;
        [SerializeField] private bool isPassthroughEnabled = false;
        [SerializeField] private bool isLoadSceneComponentEnabled = true;
        [SerializeField] private bool isLoadColliderMeshEnabled = true;
        [SerializeField] private bool isLoadVisualMeshEnabled = true;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            //Initialize constant
            IsLoadSceneComponentEnabled = isLoadSceneComponentEnabled;
            IsLoadColliderMeshEnabled = isLoadColliderMeshEnabled;
            IsLoadVisualMeshEnabled = isLoadVisualMeshEnabled;

#if UNITY_EDITOR
            IsUseScenePerception = false;
#else
            IsUseScenePerception = isUseScenePerception;
#endif
            scenePerceptionHelper.IsUseSavedSceneData = false;

            //Show passthrough
            if (isPassthroughEnabled)
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = new Color(0, 0, 0, 0);
                Interop.WVR_SetPassthroughImageFocus(WVR_PassthroughImageFocus.Scale);
                Interop.WVR_ShowPassthroughUnderlay(true);
            }
            Debug.Log("[MRRoomManager][Start]");
            StartCoroutine(InitializeCoroutine());
        }

        private void OnDestroy()
        {
            scenePerceptionManager.ClearPersistedSpatialAnchors();
            scenePerceptionManager.StopScene();
        }

        private void OnApplicationPause(bool pause)
        {

        }

        private IEnumerator InitializeCoroutine()
        {
            if (IsUseScenePerception)
            {
                Debug.Log("[MRRoomManager][InitializeCoroutine] Check scene perception is supported");
                if (!isScenePerceptionSupported())
                {
                    Debug.LogError("[MRRoomManager][InitializeCoroutine] ScenePerception is not supported");
                    NotifyDeviceUnsupported();
                    yield break;
                }

                Debug.Log("[MRRoomManager][InitializeCoroutine] Check permission");
                yield return StartCoroutine(RequestPermission());
                if (!SceneMeshPermissionHelper.IsPermissionGranted)
                {
                    Debug.LogError("[MRRoomManager][InitializeCoroutine] Scene mesh permission is denied");
                    NotifyPermissionDenied();
                    yield break;
                }
            }
            Debug.Log("[MRRoomManager][InitializeCoroutine]");

            scenePerceptionHelper.StartScene();
            scenePerceptionManager.ClearPersistedSpatialAnchors(); ///之後要改成指定anchor
            InitializationComplete = true;
            GenerateMRRoom();
            //scenePerceptionHelper.IsUseSavedSceneData = !PhotonNetwork.IsMasterClient;
        }

        public void GenerateMRRoom() 
        {
            StartCoroutine(GenerateMRRoomCoroutine());
        }

        private IEnumerator GenerateMRRoomCoroutine() 
        {
            if (InitializationComplete != true)
            {
                Debug.LogError("[MRRoomManager][GenerateMRRoomCoroutine] Initialization Not Complete!");
                yield break;
            }

            SceneStatus sceneStatus = new SceneStatus();
            WVRUtility.GetMapStatus(out sceneStatus.mapStatus);
            yield return StartCoroutine(LoadSceneComponents());
            Debug.Log(GameDataCenter.currentSceneStatus.ToString());
            sceneComponentManager.Clear();
            sceneComponentManager.GenerateScenePlanes(GameDataCenter.currentSceneStatus.scenePlanes);
            sceneComponentManager.GenerateSceneMeshes(GameDataCenter.currentSceneStatus.colliderMeshes, GameDataCenter.currentSceneStatus.visualMeshes);
            transform.parent = scenePerceptionManager.trackingOrigin.transform;
        }

        private IEnumerator LoadSceneComponents()
        {
            SceneStatus sceneStatus = new SceneStatus();

            //load scene planes
            bool isLoadScenePlaneCompleted = false;
            if (IsLoadSceneComponentEnabled)
            {
                scenePerceptionHelper.LoadScenePlanes((ScenePlaneData[] planes) =>
                {
                    isLoadScenePlaneCompleted = true;
                    sceneStatus.scenePlanes = planes;
                });
            }
            else
            {
                sceneStatus.scenePlanes = new ScenePlaneData[0];
                isLoadScenePlaneCompleted = true;
            }
            yield return new WaitUntil(() => isLoadScenePlaneCompleted == true);

            //load collision meshes
            bool isLoadColliderMeshCompleted = false;
            if (IsLoadColliderMeshEnabled)
            {
                scenePerceptionHelper.LoadSceneMeshes(WVR_SceneMeshType.WVR_SceneMeshType_ColliderMesh, (SceneMeshData[] meshes) =>
                {
                    isLoadColliderMeshCompleted = true;
                    sceneStatus.colliderMeshes = meshes;
                });
            }
            else
            {
                isLoadColliderMeshCompleted = true;
                sceneStatus.colliderMeshes = new SceneMeshData[0];
            }
            yield return new WaitUntil(() => isLoadColliderMeshCompleted == true);

            //load visual meshes
            bool isLoadVisualMeshCompleted = false;
            if (IsLoadVisualMeshEnabled)
            {
                scenePerceptionHelper.LoadSceneMeshes(WVR_SceneMeshType.WVR_SceneMeshType_VisualMesh, (SceneMeshData[] meshes) =>
                {
                    isLoadVisualMeshCompleted = true;
                    sceneStatus.visualMeshes = meshes;
                });
            }
            else
            {
                isLoadVisualMeshCompleted = true;
                sceneStatus.visualMeshes = new SceneMeshData[0];
            }
            yield return new WaitUntil(() => isLoadVisualMeshCompleted == true);

            GameDataCenter.currentSceneStatus = sceneStatus;

            //SaveCurrentSceneStatusForTest();

            Debug.Log($"[MRRoomManager][LoadSceneComponents] scene status: {GameDataCenter.currentSceneStatus}");
        }

        private bool isScenePerceptionSupported()
        {
            return (Interop.WVR_GetSupportedFeatures() &
                    (ulong)WVR_SupportedFeature.WVR_SupportedFeature_ScenePerception) != 0;
        }

        private IEnumerator RequestPermission()
        {
            bool isPermissionRequestCompleted = false;
            bool isPermissionGranted = false;
            SceneMeshPermissionHelper.RequestSceneMeshPermission((bool isGranted) =>
            {
                isPermissionGranted = isGranted;
                isPermissionRequestCompleted = true;
            });
            yield return new WaitUntil(() => isPermissionRequestCompleted == true);
        }

        private void NotifyDeviceUnsupported()
        {
            //TODO
        }

        private void NotifyPermissionDenied()
        {
            //TODO
        }
        private void SaveCurrentSceneStatusForTest()
        {
            string jsonStr = JsonUtility.ToJson(GameDataCenter.currentSceneStatus, true);
            string path = Path.Combine(Application.persistentDataPath, "sceneStatus.json");
            File.WriteAllText(path, jsonStr);
            Debug.Log($"[MRRoomManager][SaveCurrentSceneStatusForTest] Save current scene status to [{path}]");
        }

        public void SaveSceneStatusForTest(byte[] sceneStatus)
        {
            string path = Path.Combine(Application.persistentDataPath, "sceneStatusImport.txt");
            File.WriteAllBytes(path, sceneStatus);
            Debug.Log($"[MRRoomManager][SaveSceneStatusForTest] Save current scene status to [{path}]");
        }
    }
}