using HTC.UnityPlugin.PoseTracker;
using HTC.UnityPlugin.Vive;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Wave.Essence;
using Wave.Essence.Events;
using Wave.Essence.ScenePerception;
using Wave.Essence.ScenePerception.Sample;
using Wave.Native;
using Pose = UnityEngine.Pose;

namespace AnchorSharing
{
    public class AnchorAlignmentManager : MonoBehaviour
    {
        private static AnchorAlignmentManager instance = null;

        public static AnchorAlignmentManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AnchorAlignmentManager();
                }
                return instance;
            }
        }

        [SerializeField] private GameObject anchorPrefab;
        [SerializeField] private GameObject unusedAnchorPrefab;
        [SerializeField] private ScenePerceptionManager scenePerceptionManager = null;

        WVR_Result wvrResult;

        private bool needAnchorEnumeration = true;
        private bool needPersistedAnchorEnumeration = true;
        private ulong[] anchorHandles = null;
        private string[] persistedAnchorNames = null;
        private readonly Dictionary<ulong, GameObject> dictAnchorObject = new Dictionary<ulong, GameObject>();

        #region MasterClient variables
        public bool ExportAnchorDataInit { get; private set; } = false;
        public string Export_HostAnchorName { get; private set; }
        public Vector3 Export_HostAnchorPos { get; private set; }
        public Quaternion Export_HostAnchorRot { get; private set; }
        public byte[] Export_HostAnchorData { get; private set; } = null;

        private GameObject anchorObj;
        #endregion

        #region Client variables
        [HideInInspector] public UnityEvent OnFirstTimeLocateAnchorDone = new UnityEvent();

        [SerializeField] private GameObject trackingRig;

        private bool firstTimeLocateAnchorDone = false;

        private string anchorNameInHostSpace = "";
        private Vector3 anchorPosInHostSpace;
        private Quaternion anchorRotInHostSpace;
        private Vector3 anchorPosInClientSpace;
        private Quaternion anchorRotInClientSpace;
        private byte[] anchorDataInHostSpace = null;

        private Vector3 hostOriginPosInClientSpace;
        private Quaternion hostOriginRotInClientSpace;

        private Coroutine recheckAnchorUpdate;

        #endregion

        private void Awake()
        {
            instance = this;
        }

        private void OnEnable()
        {
            SystemEvent.Listen(WVR_EventType.WVR_EventType_SpatialAnchor_Changed, OnSpatialAnchorEvent, true);

            SystemEvent.Listen(WVR_EventType.WVR_EventType_PersistedSpatialAnchor_Changed, OnSpatialAnchorEvent, true);
        }

        private void OnDisable()
        {
            SystemEvent.Remove(WVR_EventType.WVR_EventType_SpatialAnchor_Changed, OnSpatialAnchorEvent);

            SystemEvent.Remove(WVR_EventType.WVR_EventType_PersistedSpatialAnchor_Changed, OnSpatialAnchorEvent);
        }

        public void ResetAnchorData() 
        {
            foreach (var anchorObject in dictAnchorObject)
            {
                Destroy(anchorObject.Value);
            }
            dictAnchorObject.Clear();
            scenePerceptionManager.ClearPersistedSpatialAnchors();
            needAnchorEnumeration = true;
            needPersistedAnchorEnumeration = true;
            anchorHandles = null;
            persistedAnchorNames = null;

            if (recheckAnchorUpdate != null) StopCoroutine(recheckAnchorUpdate);

            if (PhotonManager.Instance.IsMasterClient()) 
            {
                ExportAnchorDataInit = false;
                Export_HostAnchorName = "";
                Export_HostAnchorPos = Vector3.zero;
                Export_HostAnchorRot = Quaternion.identity;
                Export_HostAnchorData = null;
                if (anchorObj != null) Destroy(anchorObj);
            }
            else
            {
                firstTimeLocateAnchorDone = false;

                anchorNameInHostSpace = "";
                anchorPosInHostSpace = Vector3.zero;
                anchorRotInHostSpace = Quaternion.identity;
                anchorPosInClientSpace = Vector3.zero;
                anchorRotInClientSpace = Quaternion.identity;
                anchorDataInHostSpace = null;

                hostOriginPosInClientSpace = Vector3.zero;
                hostOriginRotInClientSpace = Quaternion.identity;
            }
        }

        public bool StartCreateAnchorAsHost(Vector3 anchorPos, Quaternion anchorRot)
        {
            if (PhotonNetwork.IsMasterClient && !ExportAnchorDataInit)
            {
                string anchorNameString = "SpatialAnchor_" + DateTime.UtcNow.ToString("HHmmss.fff");
                Debug.Log("[AnchorAlignmentManager] anchorNameString: " + anchorNameString);
                ulong newAnchor = 0;
                char[] anchorNameArray = anchorNameString.ToCharArray();

                wvrResult = scenePerceptionManager.CreateSpatialAnchor(anchorNameArray, anchorPos, anchorRot, ScenePerceptionManager.GetCurrentPoseOriginModel(), out newAnchor, true);

                // create on right controller
                //wvrResult = scenePerceptionManager.CreateSpatialAnchor(anchorNameArray, CameraController.Instance.RightHand.transform.position, CameraController.Instance.RightHand.transform.rotation, ScenePerceptionManager.GetCurrentPoseOriginModel(), out newAnchor, true);
                if (wvrResult != WVR_Result.WVR_Success) return false;

                string persistedAnchorNameString = "Persisted" + anchorNameString;
                wvrResult = scenePerceptionManager.PersistSpatialAnchor(persistedAnchorNameString, newAnchor);
                if (wvrResult != WVR_Result.WVR_Success)
                {
                    Debug.Log("[AnchorAlignmentManager] create persist anchor failed");
                    return false;
                }
                Debug.Log("[AnchorAlignmentManager] create persist anchor finish");

                byte[] data = null;
                wvrResult = scenePerceptionManager.ExportPersistedSpatialAnchor(persistedAnchorNameString, out data);
                if (wvrResult != WVR_Result.WVR_Success)
                {
                    Debug.Log("[AnchorAlignmentManager] export anchor failed");
                    return false;
                }
                Debug.Log("[AnchorAlignmentManager] export anchor finish");

                if (data == null)
                {
                    Debug.Log("[AnchorAlignmentManager] export data get null");
                    return false;
                }
                Debug.Log("[AnchorAlignmentManager] export anchor finish, get size: " + data.Length);

                var wvrOriginModel = ScenePerceptionManager.GetCurrentPoseOriginModel();
                // Get Origin Pose is engine side codes, so it need run in main thread.
                var trackingOriginPose = scenePerceptionManager.GetTrackingOriginPose();
                wvrResult = scenePerceptionManager.GetSpatialAnchorState(newAnchor, wvrOriginModel, out SpatialAnchorTrackingState trackingState, out Pose pose, out string name, trackingOriginPose);

                Debug.Log("[AnchorAlignmentManager] set new anchor pose");

                // draw anchor to game
                anchorObj = DrawNewAnchor(newAnchor, trackingState, pose, persistedAnchorNameString);

                Export_HostAnchorName = anchorNameString;
                Export_HostAnchorPos = pose.position;
                Export_HostAnchorRot = pose.rotation;
                Export_HostAnchorData = data;
                ExportAnchorDataInit = true;

                Debug.Log("[AnchorAlignmentManager] Create Anchor finish, start transfer");
                return true;
            }
            else
            {
                Debug.LogError($"[AnchorAlignmentManager] Unable to create anchor, due to IsMasterClient:{PhotonNetwork.IsMasterClient}, Data already created:{ExportAnchorDataInit}");
                return false;
            }
        }

        public void ConfirmFinalAnchorPoint()
        {
            if (recheckAnchorUpdate != null) StopCoroutine(recheckAnchorUpdate);
            AlignByPersistedSpatialAnchor(true, true);
        }

        IEnumerator RecheckAnchorUpdate() 
        {
            Debug.Log($"[AnchorAlignmentManager][RecheckAnchorUpdate] ");
            while (true)
            {
                yield return StartCoroutine(UpdateAnchorDictionaryCoroutine());
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void AlignByPersistedSpatialAnchor(bool correctYAxis, bool correctHeight)
        {
            if (anchorPosInClientSpace == null || anchorRotInClientSpace == null)
            {
                Debug.LogError("[AnchorAlignmentManager][AlignByPersist] missing Client variable");
                return;
            }
            if (anchorPosInHostSpace == null || anchorRotInHostSpace == null)
            {
                Debug.LogError("[AnchorAlignmentManager][AlignByPersist] missing Host variable");
                return;
            }
            hostOriginRotInClientSpace = Quaternion.identity;
            hostOriginPosInClientSpace = Vector3.zero;

            // compute host origin pose in client space, which will be used as new client origin
            hostOriginRotInClientSpace = anchorRotInClientSpace * Quaternion.Inverse(anchorRotInHostSpace);
            hostOriginPosInClientSpace = anchorPosInClientSpace - (hostOriginRotInClientSpace * anchorPosInHostSpace);

            // assume host and client have the same up vector (y axis)
            // correct marker pose to make the computed host y axis be the same as the client
            if (correctYAxis)
            {
                // adjust rotation
                Vector3 hostYAxisInClientSpace = hostOriginRotInClientSpace * Vector3.up;
                Quaternion adjustRot = Quaternion.FromToRotation(hostYAxisInClientSpace, Vector3.up);

                // rotate marker such that the recomputed host y axis matches client y axis
                Quaternion anchorRotInClientSpace_correctYAxis = adjustRot * anchorRotInClientSpace;
                // recompute host origin pose
                hostOriginRotInClientSpace = anchorRotInClientSpace_correctYAxis * Quaternion.Inverse(anchorRotInHostSpace);
                hostOriginPosInClientSpace = anchorPosInClientSpace - (hostOriginRotInClientSpace * anchorPosInHostSpace);
            }
            // assume the host and the client have the same floor height
            // this assumption depends on how well the room setup have been done
            if (correctHeight)
            {
                hostOriginPosInClientSpace = new Vector3(hostOriginPosInClientSpace.x, 0, hostOriginPosInClientSpace.z);
            }
            Debug.Log("[AnchorAlignmentManager][AlignByPersist] Align trans get!");

            // instead of moving the whole scene to the new origin
            // here we move the tracked devices in the opposite direction to get the same effect
            trackingRig.transform.position = Vector3.zero - (Quaternion.Inverse(hostOriginRotInClientSpace) * hostOriginPosInClientSpace);
            trackingRig.transform.rotation = Quaternion.identity * Quaternion.Inverse(hostOriginRotInClientSpace);
        }

        private GameObject DrawNewAnchor(ulong anchorHandle, SpatialAnchorTrackingState trackingState, Pose pose, string anchorName)
        {
            GameObject newAnchorGameObject;
            AnchorPrefab newAnchorPrefabInstance;
            Debug.Log($"[AnchorAlignmentManager][DrawNewAnchor] anchorName: {anchorName} anchorNameInHostSpace: {anchorNameInHostSpace}");
            if (PhotonManager.Instance.IsMasterClient() || anchorNameInHostSpace == anchorName)
            {
                newAnchorGameObject = UnityEngine.Object.Instantiate(anchorPrefab);
            }
            else
            {
                newAnchorGameObject = UnityEngine.Object.Instantiate(unusedAnchorPrefab);
            }
            newAnchorPrefabInstance = newAnchorGameObject.GetComponent<AnchorPrefab>();

            newAnchorPrefabInstance.SetAnchorHandle(anchorHandle);
            newAnchorPrefabInstance.SetAnchorName(anchorName);
            newAnchorPrefabInstance.SetAnchorState(trackingState, pose);

            SetAnchorPoseInScene(newAnchorPrefabInstance, pose, anchorName);

            return newAnchorGameObject;
        }

        private void AlignAnchorInScene() 
        {
            if (!firstTimeLocateAnchorDone && !PhotonNetwork.IsMasterClient)
            {
                UIController.Instance.ShowLog(PageName.ClientConfirmAnchorPage, "Please check and confirm your anchor position with the host.");
                firstTimeLocateAnchorDone = true;
                //AlignByPersistedSpatialAnchor(true, true);
                OnFirstTimeLocateAnchorDone.Invoke();
                Debug.Log($"[AnchorAlignmentManager][AlignAnchorInScene] First Time Locate Anchor Done");
            }
        }

        private void SetAnchorPoseInScene(AnchorPrefab anchorObj, Pose pose, string name)
        {
            anchorObj.transform.SetPositionAndRotation(pose.position, pose.rotation);

            if (anchorNameInHostSpace == name)
            {
                anchorPosInClientSpace = pose.position;
                anchorRotInClientSpace = pose.rotation;
            }

            Debug.Log($"[AnchorAlignmentManager][SetAnchorPoseInScene] set new anchor pose  {anchorPosInClientSpace} {anchorRotInClientSpace}");
        }

        void OnSpatialAnchorEvent(WVR_Event_t wvrEvent)
        {
            if (wvrEvent.common.type == WVR_EventType.WVR_EventType_SpatialAnchor_Changed)
            {
                Debug.Log("[AnchorAlignmentManager][OnSpatialAnchorEvent] Receive Spatial Anchor Changed event!!!");
            }
            else if (wvrEvent.common.type == WVR_EventType.WVR_EventType_PersistedSpatialAnchor_Changed)
            {
                Debug.Log("[AnchorAlignmentManager][OnSpatialAnchorEvent] Receive Persisted Spatial Anchor Changed event!!!");
            }
        }

        internal IEnumerator UpdateAnchorDictionaryCoroutine()
        {
            bool hasNewSpatial, hasNewPersist;

            hasNewSpatial = needAnchorEnumeration;
            hasNewPersist = needPersistedAnchorEnumeration;

            needAnchorEnumeration = false;
            needPersistedAnchorEnumeration = false;

            List<Tuple<ulong, SpatialAnchorTrackingState, Pose, string>> stateList = null;
            var wvrOriginModel = ScenePerceptionManager.GetCurrentPoseOriginModel();
            // Get Origin Pose is engine side codes, so it need run in main thread.
            var trackingOriginPose = scenePerceptionManager.GetTrackingOriginPose();

            var t = Task.Run(() => UpdateAnchorsTask(hasNewSpatial, hasNewPersist, wvrOriginModel, trackingOriginPose, out stateList));

            yield return new WaitUntil(() => t.IsCompleted);

            if (stateList != null)
            {
                foreach (var state in stateList)
                {
                    if (!dictAnchorObject.ContainsKey(state.Item1))
                    {
                        // Create Anchor Object
                        dictAnchorObject.Add(state.Item1, DrawNewAnchor(state.Item1, state.Item2, state.Item3, state.Item4));
                    }
                    CheckAnchorPose(state.Item1, state.Item2, state.Item3, state.Item4);
                }
            }
        }

        private void CheckAnchorPose(ulong anchorHandle, SpatialAnchorTrackingState trackingState, Pose pose, string name)
        {
            //Check anchor pose
            GameObject anchorObj;
            anchorObj = dictAnchorObject[anchorHandle];
            if (anchorObj == null) return;

            AnchorPrefab prefab = anchorObj.GetComponent<AnchorPrefab>();
            if (prefab == null)
            {
                needAnchorEnumeration = true;
                return;
            }

            bool needSetAnchorPose = prefab.GetPose() != pose;
            Debug.Log($"[AnchorAlignmentManager][CheckAnchorPose] isPosUpdate:{needSetAnchorPose} TrackingState:{prefab.GetTrackingState()}");

            if (needSetAnchorPose)
            {
                SetAnchorPoseInScene(prefab, pose, name);
            }

            if (prefab.GetTrackingState() == SpatialAnchorTrackingState.Tracking && anchorNameInHostSpace == name) 
            {
                AlignAnchorInScene();
            }

            if (needSetAnchorPose || prefab.GetTrackingState() != trackingState)
            {
                prefab.SetAnchorState(trackingState, pose);
            }
        }

        internal void UpdateAnchorsTask(bool hasNewSpatial, bool hasNewPersist, WVR_PoseOriginModel wvrOriginModel, Pose trackingOriginPose, out List<Tuple<ulong, SpatialAnchorTrackingState, Pose, string>> stateList)
        {
            if (hasNewSpatial || hasNewPersist)
                UpdateAnchorLists(hasNewSpatial, hasNewPersist);

            if (anchorHandles == null)
            {
                stateList = null;
                return;
            }

            stateList = new List<Tuple<ulong, SpatialAnchorTrackingState, Pose, string>>();
            foreach (ulong anchor in anchorHandles)
            {
                wvrResult = scenePerceptionManager.GetSpatialAnchorState(anchor, wvrOriginModel, out SpatialAnchorTrackingState trackingState, out Pose pose, out string name, trackingOriginPose);
                if (ResultProcess("GetSpatialAnchorState", false, true))
                {
                    stateList.Add(new Tuple<ulong, SpatialAnchorTrackingState, Pose, string>(anchor, trackingState, pose, name));
                }
                else
                {
                    Debug.Log("[AnchorAlignmentManager] GetSpatialAnchorState result fail");
                }
            }

            foreach (var persistedAnchorName in persistedAnchorNames)
            {
                bool found = false;
                foreach (var state in stateList)
                {
                    if (persistedAnchorName.Contains(state.Item4))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    // Remove anchorName's start string "Persisted"
                    string spatialAnchorName = persistedAnchorName.Substring(9);
                    wvrResult = scenePerceptionManager.CreateSpatialAnchorFromPersistenceName(persistedAnchorName, spatialAnchorName, out ulong _);
                    needAnchorEnumeration = (wvrResult == WVR_Result.WVR_Success);
                }
            }
        }

        private void UpdateAnchorLists(bool hasNewSpatial, bool hasNewPersist)
        {
            if (!hasNewSpatial && !hasNewPersist)
                return;

            ulong[] saHandles;
            string[] paNames;

            saHandles = anchorHandles;
            paNames = persistedAnchorNames;

            if (anchorHandles == null || hasNewSpatial)
            {
                wvrResult = scenePerceptionManager.GetSpatialAnchors(out saHandles);
                if (!ResultProcess("GetSpatialAnchors", false, true)) return;
            }

            if (hasNewPersist)
            {
                wvrResult = scenePerceptionManager.GetPersistedSpatialAnchorNames(out paNames);
                if (!ResultProcess("GetPersistedSpatialAnchorNames", false, true)) return;
            }

            anchorHandles = saHandles;
            persistedAnchorNames = paNames;
        }

        bool ResultProcess(string msg, bool successedLog = true, bool failedLog = true)
        {
            if (wvrResult == WVR_Result.WVR_Success)
            {
                if (successedLog)
                    Debug.Log("[AnchorAlignmentManager]" + msg + " successed");
                return true;
            }
            else
            {
                if (failedLog)
                    Debug.Log("[AnchorAlignmentManager]" + msg + " failed");
                return false;
            }
        }

        public void ImportReceivedAnchorData(string name, Vector3 pos, Quaternion rot, byte[] anchorData) 
        {
            Debug.Log("[AnchorAlignmentManager][ImportReceivedData]");
            anchorNameInHostSpace = name;
            anchorPosInHostSpace = pos;
            anchorRotInHostSpace = rot;
            anchorDataInHostSpace = anchorData;
            ImportAnchor(anchorDataInHostSpace);
            recheckAnchorUpdate = StartCoroutine(RecheckAnchorUpdate());
        }

        private void ImportAnchor(byte[] imported_data)
        {
            Debug.Log("[AnchorAlignmentManager][ImportAnchor] get imported data length:" + imported_data.Length);

            wvrResult = scenePerceptionManager.ImportPersistedSpatialAnchor(imported_data);
            if (wvrResult != WVR_Result.WVR_Success)
            {
                Debug.LogError("[AnchorAlignmentManager][ImportAnchor] Import Persisted Spatial Anchor Failed!");
                return;
            }
            Debug.Log("[AnchorAlignmentManager][ImportAnchor] import anchor finished");
            needPersistedAnchorEnumeration = true;
        }
    }
}

