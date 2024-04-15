using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Wave.Native;
using HTC.UnityPlugin.Vive;
using static SocketUtils;

namespace AnchorSharing
{
    public sealed class GameManager : MonoBehaviour
    {
        private static GameManager instance;

        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new GameManager();
                return instance;
            }
        }

        [Header("Photon Objects [Default None]")]
        //Both MasterClient and Client can get these objects.
        [SerializeField] public PlayerController selfPlayerController = new PlayerController();
        [SerializeField] public PlayerController otherPlayerController = new PlayerController();

        //Only MasterClient can gets these objects.
        [SerializeField] private List<ReboundController> reboundControllers = new List<ReboundController>();
        [SerializeField] private List<GashaponController> gashaponControllers = new List<GashaponController>();
        [SerializeField] private CannonController cannonController = new CannonController();

        //Both MasterClient and Client can get these objects.
        [SerializeField] public ScoreBoardController scoreBoardController = null;

        [SerializeField] private ServerSettings settings = null;
        //-----------------------------------------------------------------------------------------

        //Game Events
        [HideInInspector] public UnityEvent OnGameStart = new UnityEvent();
        [HideInInspector] public UnityEvent OnGameEnd = new UnityEvent();

        [Header("----------------------------------------------------------------------------------------------------")]
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip audioClipBGM;

        [Header("Mesh Bounds For Generate Game Objects")]
        [SerializeField] private GameObject scenePerception = null;
        [SerializeField] private GameObject anchorManager = null;
        [SerializeField] private LayerMask meshAllowMask;
        [Range(-5, -1)]
        [SerializeField] private float meshBoundExpand = -3;

        [Header("----------------------------------------------------------------------------------------------------")]
        [Header("Release:       editorMode Off, anchorEnable On ")]
        [Header("Non Anchor:    editorMode Off, anchorEnable Off")]
        [Header("On Editor:     editorMode On , anchorEnable Off")]
        [SerializeField] private bool editorMode = true;
        [SerializeField] private bool anchorEnable = false;
        [SerializeField] private TextMesh regionText = null;

        private string LAN_SERVER_IP = "";

        //-----------------------------------------------------------------------------------------
        private void Awake()
        {
            instance = this;
            if (anchorEnable)
                anchorManager.SetActive(true);
            else
                anchorManager.SetActive(false);
        }

        private void Start()
        {
            Application.targetFrameRate = 90;

            //Register
            PhotonCallbacks.Instance.punEvent += PhotonCB;
            SocketManager.Instance.disconnectEvent += SocketCB;

            GameDefine.EnablePassthrough(true, WVR_PassthroughImageQuality.DefaultMode);
        }

        public void StartConnecting()
        {
            PhotonManager.Instance.InitServer(settings.AppSettings.AppIdRealtime, PhotonManager.Instance.region.ToString());
            PhotonManager.Instance.Connect();

            regionText.text = PhotonManager.Instance.region.ToString();
        }

        public void RestartGame()
        {
            StopAllCoroutines();
            StartCoroutine(Main_ReStartGame());
        }

        public void RecreateAnchor()
        {
            Debug.Log("[GameManager][RecreateAnchor]");
            StopAllCoroutines();
            StartCoroutine(MainGameFlow(false));
        }

        private void PhotonCB(object sender, PhotonCallbacks.PhotonCallbackEvent args)
        {
            switch (args.state)
            {
                case State.sOnJoinedRoom:
                    StartCoroutine(MainGameFlow(true));
                    break;
                case State.sOnLeftRoom:
                    StopAllCoroutines();
                    AnchorAlignmentManager.Instance.ResetAnchorData();
                    SocketManager.Instance.KillSocket();
                    break;
                case State.sOnDisconnected:
                    SocketManager.Instance.KillSocket();
                    break;
                case State.sOnCreateRoomFailed:
                    SocketManager.Instance.KillSocket();
                    break;
                case State.sOnJoinRoomFailed:
                    SocketManager.Instance.KillSocket();
                    break;
                case State.sOnPlayerLeftRoom:
                    SocketManager.Instance.KillSocket();
                    PhotonManager.Instance.LoadLevel(SceneManager.GetActiveScene().name);
                    break;
            }
        }

        private void SocketCB(object sender, System.EventArgs args)
        {
            PhotonManager.Instance.LoadLevel(SceneManager.GetActiveScene().name);
        }

        private IEnumerator MainGameFlow(bool checkServerIP)
        {
            if (checkServerIP)
                yield return PUN_SetRoomProperty_LAN_IP();

            yield return Socket_CheckSocketIsConnect();
            yield return PUN_DefineSceneObjects_ScoreBoard();

            if (anchorEnable)
            {
                yield return Socket_SyncAnchorData();
                yield return PUN_AnchorVerify();
            }

            yield return PUN_CreatPlayer();

            if (!anchorEnable)
                yield return Main_ReStartGame();
        }

        private IEnumerator Main_ReStartGame()
        {
            if (PhotonManager.Instance.IsMasterClient())
            {
                var photonViewsExceptPlayer = FindObjectsOfType<PhotonView>();
                foreach (var i in photonViewsExceptPlayer)
                {
                    if (!i.gameObject.GetComponent<PlayerController>() && !i.gameObject.GetComponent<ScoreBoardController>())
                        PhotonNetwork.Destroy(i.gameObject);
                }
            }

            if (cannonController != null)
                PhotonManager.Instance.DestoryObject(cannonController.gameObject);

            reboundControllers.Clear();
            gashaponControllers.Clear();
            cannonController = null;

            yield return PUN_DefineSceneObjects_Rebounder();
            yield return PUN_DefineSceneObjects_Gashapon();
            yield return PUN_DefineSceneObjects_Cannon();
            yield return PUN_DefineSceneObjects_ScoreBoard();
            yield return GAME_StartGame();
        }

        //-----------------------------------------------------------------------------------------
        private IEnumerator PUN_SetRoomProperty_LAN_IP()
        {
            LAN_SERVER_IP = SocketUtils.GetLocalIPAddress();
            if (!PhotonManager.Instance.TryGetRoomProperty(GameDefine.SOCKET_SERVER_IP, out object o))
            {
                PhotonManager.Instance.SetRoomProperty(GameDefine.SOCKET_SERVER_IP, LAN_SERVER_IP);
                Debug.Log($"<color=yellow>[PhotonPUN] SetRoomProperty: {GameDefine.SOCKET_SERVER_IP} : {LAN_SERVER_IP}.</color>");
            }
            else
            {
                LAN_SERVER_IP = PhotonManager.Instance.GetRoomProperty<string>(GameDefine.SOCKET_SERVER_IP);
                Debug.Log($"<color=yellow>[PhotonPUN] GetRoomProperty: {GameDefine.SOCKET_SERVER_IP} : {LAN_SERVER_IP}.</color>");
            }
            yield return null;
        }

        private IEnumerator Socket_CheckSocketIsConnect()
        {
            SocketManager.Instance.serverIP = LAN_SERVER_IP;
            if (PhotonManager.Instance.IsMasterClient())
                SocketManager.Instance.InitSocket(SocketUtils.SocketRole.Server);
            else
                SocketManager.Instance.InitSocket(SocketUtils.SocketRole.Client);

            int timerCountdown = 30;
            while (true)
            {
                if (PhotonManager.Instance.IsMasterClient())
                    UIController.Instance.ShowLog(PageName.LoadingForRoomPage, $"Waiting for Socket Client to connect... \n Timeout: {timerCountdown}");
                else
                    UIController.Instance.ShowLog(PageName.LoadingForRoomPage, $"Connecting... \n Timeout: {timerCountdown}");

                yield return new WaitForSeconds(1);
                timerCountdown--;

                if (timerCountdown <= 0)
                {
                    if (PhotonManager.Instance.IsMasterClient())
                        UIController.Instance.ShowLog(PageName.LoadingForRoomPage, $"Failed... Please confirm that the Client is on the same wifi!");
                    else
                        UIController.Instance.ShowLog(PageName.LoadingForRoomPage, $"Failed... Please confirm that the Host is on the same wifi!");

                    yield return new WaitForSeconds(3);

                    SocketManager.Instance.KillSocket();
                    PhotonManager.Instance.LoadLevel(SceneManager.GetActiveScene().name);
                }

                if (SocketManager.Instance.isConnected)
                    break;
            }

            UIController.Instance.ShowLog(PageName.LoadingForRoomPage, $"Connected!");
            yield return new WaitForSeconds(2);
            yield return null;
        }

        private IEnumerator Socket_SyncAnchorData()
        {
            byte[] data = null;

            UIController.Instance.ToPage(PageName.WaitingCreatAnchorPage);

            if (PhotonManager.Instance.IsMasterClient())
            {
                PhotonManager.Instance.SetRoomProperty(GameDefine.SOCKET_ANCHOR_SIZE, 0);
                PhotonManager.Instance.SetRoomProperty(GameDefine.SOCKET_ANCHOR_READY_TO_SYNC, false);
            }

            UIController.Instance.ShowLog(PageName.WaitingCreatAnchorPage, $"Initializing Anchor Data...");

            yield return new WaitUntil(() => PhotonManager.Instance.TryGetRoomProperty(GameDefine.SOCKET_ANCHOR_SIZE, out object o)
                                            && PhotonManager.Instance.TryGetRoomProperty(GameDefine.SOCKET_ANCHOR_READY_TO_SYNC, out object j));

            yield return new WaitUntil(() => PhotonManager.Instance.GetRoomProperty<int>(GameDefine.SOCKET_ANCHOR_SIZE) == 0
                                            && !PhotonManager.Instance.GetRoomProperty<bool>(GameDefine.SOCKET_ANCHOR_READY_TO_SYNC));

            if (SocketManager.Instance.GetComponent<SocketClient>())
            {
                UIController.Instance.ShowLog(PageName.WaitingCreatAnchorPage, $"Waiting for server to create the anchor point...");
            }

            if (SocketManager.Instance.GetComponent<SocketServer>())
            {
                UIController.Instance.ShowLog(PageName.WaitingCreatAnchorPage, $"Walk to a specific object in your play area, press left hand trigger in front of this object to set the anchor.");

                PhotonManager.Instance.SetRoomProperty(GameDefine.ANCHOR_ALIGNMENT_SUCCESS, false);

                //Prepare Anchor Data.
                Vector3 leftHandPos = Vector3.zero;
                Quaternion leftHandRot = Quaternion.identity;

                while (true)
                {
                    if (ViveInput.GetPressDown(HandRole.LeftHand, ControllerButton.Trigger))
                    {
                        leftHandPos = VivePose.GetPose(HandRole.LeftHand).pos;
                        leftHandRot = VivePose.GetPose(HandRole.LeftHand).rot;
                        break;
                    }
                    yield return null;
                }

                bool createAnchorFinish = false;
                while (!createAnchorFinish)
                {
                    if (AnchorAlignmentManager.Instance.StartCreateAnchorAsHost(leftHandPos, leftHandRot))
                    {
                        AnchorData anchorData = new AnchorData();
                        anchorData.anchorName = AnchorAlignmentManager.Instance.Export_HostAnchorName;
                        anchorData.anchorBytes = AnchorAlignmentManager.Instance.Export_HostAnchorData;
                        AnchorData.acVector3 acVector3Pos = new AnchorData.acVector3();
                        acVector3Pos.x = AnchorAlignmentManager.Instance.Export_HostAnchorPos.x;
                        acVector3Pos.y = AnchorAlignmentManager.Instance.Export_HostAnchorPos.y;
                        acVector3Pos.z = AnchorAlignmentManager.Instance.Export_HostAnchorPos.z;
                        anchorData.anchorPos = acVector3Pos;
                        AnchorData.acVector4 acVector4Rot = new AnchorData.acVector4();
                        acVector4Rot.x = AnchorAlignmentManager.Instance.Export_HostAnchorRot.x;
                        acVector4Rot.y = AnchorAlignmentManager.Instance.Export_HostAnchorRot.y;
                        acVector4Rot.z = AnchorAlignmentManager.Instance.Export_HostAnchorRot.z;
                        acVector4Rot.w = AnchorAlignmentManager.Instance.Export_HostAnchorRot.w;
                        anchorData.anchorRot = acVector4Rot;

                        byte[] temp = ToByteArray(anchorData);
                        data = temp;
                        PhotonManager.Instance.SetRoomProperty(GameDefine.SOCKET_ANCHOR_SIZE, temp.Length);

                        UIController.Instance.ShowLog(PageName.WaitingCreatAnchorPage, $"Anchor created");
                        Debug.Log("[GameManager][Socket_SyncAnchorData] Get anchor data");
                        createAnchorFinish = true;
                    }
                    else
                    {
                        UIController.Instance.ShowLog(PageName.WaitingCreatAnchorPage, $"Anchor creation failed");
                        Debug.LogError("[GameManager][Socket_SyncAnchorData] Get anchor data Failed!");
                    }
                    yield return null;
                }
            }

            yield return new WaitUntil(() => PhotonManager.Instance.GetRoomProperty<int>(GameDefine.SOCKET_ANCHOR_SIZE) != 0);
            int size = PhotonManager.Instance.GetRoomProperty<int>(GameDefine.SOCKET_ANCHOR_SIZE);
            Debug.Log($"<color=lime>[Anchor] Anchor Size is {size}.</color>");

            UIController.Instance.ToPage(PageName.SendingAnchorPage);

            if (SocketManager.Instance.GetComponent<SocketServer>())
            {
                UIController.Instance.ShowLog(PageName.SendingAnchorPage, $"Transferring Anchor data...");
                yield return new WaitUntil(() => PhotonManager.Instance.GetRoomProperty<bool>(GameDefine.SOCKET_ANCHOR_READY_TO_SYNC));
                SocketManager.Instance.GetComponent<SocketServer>().SendDataToClient(data);
            }
            else if (SocketManager.Instance.GetComponent<SocketClient>())
            {
                UIController.Instance.ShowLog(PageName.SendingAnchorPage, $"Receiving Anchor data...");
                PhotonManager.Instance.SetRoomProperty(GameDefine.SOCKET_ANCHOR_READY_TO_SYNC, true);
                SocketManager.Instance.GetComponent<SocketClient>().ReceiveDataFromServer(size);
            }

            while (true)
            {
                if (SocketManager.Instance.GetComponent<SocketServer>())
                {
                    UIController.Instance.ShowLog(PageName.SendingAnchorPage, $"Transferring Anchor data... \n" +
                                                          $"Package size: {SocketManager.Instance.GetComponent<SocketServer>().targetSize}");
                }
                else if (SocketManager.Instance.GetComponent<SocketClient>())
                {
                    float progerss = (float)SocketManager.Instance.GetComponent<SocketClient>().currentLength / (float)SocketManager.Instance.GetComponent<SocketClient>().targetSize;
                    UIController.Instance.ShowLog(PageName.SendingAnchorPage, $"Receiving Anchor data... \n" + $"{SocketManager.Instance.GetComponent<SocketClient>().currentLength} / {SocketManager.Instance.GetComponent<SocketClient>().targetSize}");
                    UIController.Instance.UpdateProgressBar(progerss);
                }
                yield return null;

                if (SocketManager.Instance.isDataTransferred)
                {
                    if (SocketManager.Instance.GetComponent<SocketServer>())
                    {
                        UIController.Instance.ShowLog(PageName.SendingAnchorPage, $"Anchor data transferred successfully!");
                    }
                    else if (SocketManager.Instance.GetComponent<SocketClient>())
                    {
                        UIController.Instance.ShowLog(PageName.SendingAnchorPage, $"Anchor data received successfully!");

                        byte[] d = SocketManager.Instance.GetComponent<SocketClient>().byteData;
                        AnchorData ad = FromByteArray<AnchorData>(d);
                        Debug.Log("[GameManager][Socket_SyncAnchorData] Name: " + ad.anchorName);
                        Debug.Log("[GameManager][Socket_SyncAnchorData] Bytes Length: " + ad.anchorBytes.Length);
                        Debug.Log($"[GameManager][Socket_SyncAnchorData] Pos: {ad.anchorPos.x},{ad.anchorPos.y},{ad.anchorPos.z}");
                        Debug.Log($"[GameManager][Socket_SyncAnchorData] Rot: {ad.anchorRot.x},{ad.anchorRot.y},{ad.anchorRot.z}");

                        Quaternion quaternion = new Quaternion(ad.anchorRot.x, ad.anchorRot.y, ad.anchorRot.z, ad.anchorRot.w);
                        AnchorAlignmentManager.Instance.ImportReceivedAnchorData(ad.anchorName, new Vector3(ad.anchorPos.x, ad.anchorPos.y, ad.anchorPos.z), quaternion, ad.anchorBytes);
                    }
                    yield return new WaitForSeconds(3);
                    break;
                }
            }
            yield return null;
        }

        private IEnumerator PUN_AnchorVerify()
        {
            PageName page;
            string msg;
            if (PhotonManager.Instance.IsMasterClient())
            {
                page = PageName.MasterConfirmAnchorPage;
                msg = "Waiting for another player to confirm anchor position...";
            }
            else
            {
                page = PageName.ClientConfirmAnchorPage;
                msg = "Anchor aligning...";
            }

            UIController.Instance.ToPage(page);
            UIController.Instance.ShowLog(page, msg);
            Debug.Log("[GameManager][PUN_AnchorVerify]");
            yield return new WaitUntil(() => PhotonManager.Instance.GetRoomProperty<bool>(GameDefine.ANCHOR_ALIGNMENT_SUCCESS));
            Debug.Log("[GameManager][PUN_AnchorVerify] End");
            UIController.Instance.ShowLog(page, $"Anchor alignment completed!");
            UIController.Instance.ToPage(PageName.SelectWeaponPage);
        }

        //-----------------------------------------------------------------------------------------
        private IEnumerator PUN_CreatPlayer()
        {
            object[] instantiationData = new object[] { PlayerController.WeaponType.GunShield };
            GameObject player = PhotonManager.Instance.InitiateObject(GameDefine.PLAYER, Vector3.zero, Quaternion.identity, 0, instantiationData);

            yield return new WaitUntil(() => player.activeSelf);
            selfPlayerController = player.GetComponent<PlayerController>();

            if (PhotonManager.Instance.IsMasterClient())
                player.GetPhotonView().Owner.NickName = GameDefine.PLAYER_1_NAME;
            else
                player.GetPhotonView().Owner.NickName = GameDefine.PLAYER_2_NAME;

            if (!editorMode)
            {
                while (true)
                {
                    yield return new WaitForEndOfFrame();
                    var allPlayers = FindObjectsOfType<PlayerController>();
                    foreach (var i in allPlayers)
                    {
                        if (i != selfPlayerController)
                        {
                            otherPlayerController = i;
                            break;
                        }
                    }
                    if (otherPlayerController != null)
                        break;
                }
                yield return new WaitUntil(() => FindObjectsOfType<PlayerController>().Length >= GameDefine.PLAYERS_COUNT);
            }
        }

        private IEnumerator PUN_DefineSceneObjects_Rebounder()
        {
            if (PhotonManager.Instance.IsMasterClient())
            {
                //Create Rebounder
                for (int i = 0; i < GameDefine.REBOUNDER_COUNT; i++)
                {
                    //Random Position
                    Vector3 randomPos = Vector3.zero;
                    Vector3 randomRot = Vector3.zero;

                    SceneMesh[] sceneMeshs = scenePerception.GetComponentsInChildren<SceneMesh>();
                    FloorDisplayer floorDisplayer = scenePerception.GetComponentInChildren<FloorDisplayer>();
                    SceneMesh sm = null;
                    foreach (var s in sceneMeshs)
                    {
                        if (s.GetComponent<MeshRenderer>() != null)
                            sm = s;
                    }

                    if (sm != null)
                    {
                        List<Vector3> vt3 = GetPositionsFromRendererBound(sm.GetComponent<MeshRenderer>(), scenePerception.transform, floorDisplayer.transform.position + new Vector3(0, 2, 0));

                        Vector3 point = new Vector3();
                        Vector3 dir = new Vector3();
                        Vector3 rota = new Vector3();
                        Color color = new Color();

                        if (i >= 0 && i <= 1)
                        {
                            color = Color.red;
                            if (i == 0)
                                point = GetRandomPointOnPlaneAreaA(vt3[0], vt3[1], vt3[4], vt3[5]);
                            else if (i == 1)
                                point = GetRandomPointOnPlaneAreaB(vt3[0], vt3[1], vt3[4], vt3[5]);
                            dir = GetNormalizedByRandomPointOnPlane(vt3[0], vt3[1], vt3[4], vt3[5]);
                            rota = new Vector3(0, Random.Range(0, 180), 0);
                        }
                        else if (i >= 2 && i <= 2)
                        {
                            color = Color.blue;
                            point = GetRandomPointOnPlane(vt3[1], vt3[3], vt3[5], vt3[7]);
                            dir = GetNormalizedByRandomPointOnPlane(vt3[1], vt3[3], vt3[5], vt3[7]);
                            rota = new Vector3(Random.Range(0, 180), 0, 90f);
                        }
                        else if (i >= 3 && i <= 3)
                        {
                            color = Color.green;
                            point = GetRandomPointOnPlane(vt3[0], vt3[2], vt3[4], vt3[6]);
                            dir = -GetNormalizedByRandomPointOnPlane(vt3[0], vt3[2], vt3[4], vt3[6]);
                            rota = new Vector3(Random.Range(0, 180), 180, 90f);
                        }
                        else if (i >= 4 && i <= 4)
                        {
                            color = Color.yellow;
                            point = GetRandomPointOnPlane(vt3[4], vt3[5], vt3[6], vt3[7]);
                            dir = GetNormalizedByRandomPointOnPlane(vt3[4], vt3[5], vt3[6], vt3[7]);
                            rota = new Vector3(Random.Range(0, 180), -90, 90f);
                        }
                        else if (i >= 5 && i <= 5)
                        {
                            color = Color.cyan;
                            point = GetRandomPointOnPlane(vt3[0], vt3[1], vt3[2], vt3[3]);
                            dir = -GetNormalizedByRandomPointOnPlane(vt3[0], vt3[1], vt3[2], vt3[3]);
                            rota = new Vector3(Random.Range(0, 180), 90, 90f);
                        }

                        Ray ray = new Ray(point, dir);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity, meshAllowMask))
                        {
                            Debug.DrawLine(point, point + dir, color, Mathf.Infinity);
                            randomPos = hit.point;
                            randomRot = rota + scenePerception.transform.eulerAngles;
                        }
                    }

                    GameObject rb = PhotonManager.Instance.InitiateObject(GameDefine.REBOUNDER, randomPos, Quaternion.Euler(randomRot), 0, null);

                    yield return new WaitUntil(() => rb.activeSelf);
                    yield return new WaitUntil(() => rb.GetPhotonView() != null);

                    int index = Random.Range(0, 4);
                    System.Array values = System.Enum.GetValues(typeof(ReboundController.Type));
                    int randomNum = Random.Range(0, values.Length);
                    rb.GetPhotonView().RPC("AS_Rebounder_ChangeType",
                                           RpcTarget.All,
                                           (ReboundController.Type)randomNum,
                                           index
                                           );
                    reboundControllers.Add(rb.GetComponent<ReboundController>());
                }
            }
            if (!editorMode)
                yield return new WaitUntil(() => FindObjectsOfType<ReboundController>().Length >= GameDefine.REBOUNDER_COUNT);
        }

        private IEnumerator PUN_DefineSceneObjects_Gashapon()
        {
            if (PhotonManager.Instance.IsMasterClient())
            {
                //Create Gashapon
                for (int i = 0; i < GameDefine.GASHAPON_COUNT; i++)
                {
                    //Random Position
                    Vector3 randomPos = Vector3.zero;
                    Vector3 randomRot = Vector3.zero;

                    SceneMesh[] sceneMeshs = scenePerception.GetComponentsInChildren<SceneMesh>();
                    FloorDisplayer floorDisplayer = scenePerception.GetComponentInChildren<FloorDisplayer>();
                    SceneMesh sm = null;
                    foreach (var s in sceneMeshs)
                    {
                        if (s.GetComponent<MeshRenderer>() != null)
                            sm = s;
                    }

                    if (sm != null)
                    {
                        List<Vector3> vt3 = GetPositionsFromRendererBound(sm.GetComponent<MeshRenderer>(), scenePerception.transform, floorDisplayer.transform.position + new Vector3(0, 2, 0));

                        Vector3 point = new Vector3();
                        Vector3 dir = new Vector3();
                        Vector3 rota = new Vector3();

                        if (i == 0)
                        {
                            point = vt3[4];
                            dir = Vector3.down;
                            rota = new Vector3(0, 150f, 0);
                        }
                        else if (i == 1)
                        {
                            point = vt3[1];
                            dir = Vector3.down;
                            rota = new Vector3(0, -45f, 0);
                        }

                        Ray ray = new Ray(point, dir);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity, meshAllowMask))
                        {
                            Debug.DrawLine(point, point + dir, Color.white, Mathf.Infinity);
                            randomPos = hit.point;
                            randomRot = rota + scenePerception.transform.eulerAngles;
                        }
                    }
                    GameObject gp = PhotonManager.Instance.InitiateObject(GameDefine.GASHAPON, randomPos, Quaternion.Euler(randomRot), 0, null);

                    yield return new WaitUntil(() => gp.activeSelf);
                    yield return new WaitUntil(() => gp.GetPhotonView() != null);

                    int spawnNum = 3;
                    float initSpeed = 1;
                    gp.GetPhotonView().RPC("AS_Gashapon_SpawnCapsule",
                                           RpcTarget.All,
                                           spawnNum,
                                           initSpeed
                                           );

                    gashaponControllers.Add(gp.GetComponent<GashaponController>());
                }
            }
            if (!editorMode)
                yield return new WaitUntil(() => FindObjectsOfType<GashaponController>().Length >= GameDefine.GASHAPON_COUNT);
        }

        private IEnumerator PUN_DefineSceneObjects_Cannon()
        {
            if (PhotonManager.Instance.IsMasterClient())
            {
                //Create Cannon
                for (int i = 0; i < GameDefine.CANNON_COUNT; i++)
                {
                    //Random Position
                    Vector3 randomPos = Vector3.zero;
                    Vector3 randomRot = Vector3.zero;

                    SceneMesh[] sceneMeshs = scenePerception.GetComponentsInChildren<SceneMesh>();
                    FloorDisplayer floorDisplayer = scenePerception.GetComponentInChildren<FloorDisplayer>();
                    SceneMesh sm = null;
                    foreach (var s in sceneMeshs)
                    {
                        if (s.GetComponent<MeshRenderer>() != null)
                            sm = s;
                    }

                    if (sm != null)
                    {
                        List<Vector3> vt3 = GetPositionsFromRendererBound(sm.GetComponent<MeshRenderer>(), scenePerception.transform, floorDisplayer.transform.position + new Vector3(0, 2, 0));

                        Vector3 point = new Vector3();
                        Vector3 dir = new Vector3();
                        Vector3 rota = new Vector3();

                        if (i == 0)
                        {
                            point = CalculateCubeCenter(vt3.ToArray()) + new Vector3(0, 0, 0.25f);
                            dir = Vector3.down;
                            rota = Vector3.zero;
                        }

                        Ray ray = new Ray(point, dir);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity, meshAllowMask))
                        {
                            Debug.DrawLine(point, point + dir, Color.white, Mathf.Infinity);
                            randomPos = hit.point;
                            //randomPos = new Vector3(0, 1.5f, 1);
                            randomRot = rota + scenePerception.transform.eulerAngles;
                        }
                    }
                    GameObject cn = PhotonManager.Instance.InitiateObject(GameDefine.CANNON, randomPos, Quaternion.Euler(randomRot), 0, null);

                    yield return new WaitUntil(() => cn.activeSelf);
                    yield return new WaitUntil(() => cn.GetPhotonView() != null);

                    cannonController = cn.GetComponent<CannonController>();
                }
            }
            else if (!PhotonManager.Instance.IsMasterClient())
            {
                while (true)
                {
                    yield return new WaitForEndOfFrame();
                    cannonController = FindObjectOfType<CannonController>();
                    if (cannonController != null)
                        break;
                }
            }
            if (!editorMode)
                yield return new WaitUntil(() => FindObjectsOfType<CannonController>().Length >= GameDefine.CANNON_COUNT);
        }

        private IEnumerator PUN_DefineSceneObjects_ScoreBoard()
        {
            if (scoreBoardController != null)
            {
                scoreBoardController.ResetScoreBoard();
            }
            else
            {
                if (PhotonManager.Instance.IsMasterClient())
                {
                    //Create ScoreBoard
                    FloorDisplayer ground = scenePerception.GetComponentInChildren<FloorDisplayer>();
                    GameObject sb = PhotonManager.Instance.InitiateObject(GameDefine.SCOREBOARD, ground.transform.position + new Vector3(0, 2.5f, 0), Quaternion.identity, 0, null);

                    yield return new WaitUntil(() => sb.activeSelf);
                    yield return new WaitUntil(() => sb.GetPhotonView() != null);

                    scoreBoardController = sb.GetComponent<ScoreBoardController>();
                    Debug.Log("[GameManager][PUN_DefineSceneObjects_ScoreBoard]");
                }
                else if (!PhotonManager.Instance.IsMasterClient())
                {
                    while (true)
                    {
                        yield return new WaitForEndOfFrame();
                        scoreBoardController = FindObjectOfType<ScoreBoardController>();
                        if (scoreBoardController != null)
                            break;
                    }
                }
                if (!editorMode)
                    yield return new WaitUntil(() => scoreBoardController != null);
            }
        }

        private IEnumerator GAME_StartGame()
        {
            audioSource.Stop();
            audioSource.clip = audioClipBGM;
            audioSource.Play();

            if (PhotonManager.Instance.IsMasterClient())
            {
                if (scoreBoardController != null)
                {
                    scoreBoardController.CallStartGameRPC();
                    while (true)
                    {
                        yield return new WaitForSeconds(4);
                        if (scoreBoardController.IsGameRunning &&
                            gashaponControllers.Count >= GameDefine.GASHAPON_COUNT)
                        {
                            var capsules = FindObjectsOfType<CapsuleController>();
                            if (capsules.Length < 10)
                            {
                                foreach (var i in gashaponControllers)
                                {
                                    int spawnNum = 1;
                                    float initSpeed = 1;
                                    i.gameObject.GetPhotonView().RPC("AS_Gashapon_SpawnCapsule",
                                                                     RpcTarget.All,
                                                                     spawnNum,
                                                                     initSpeed
                                                                     );
                                }
                            }
                        }
                    }
                }
            }
            yield return null;
        }

        //-----------------------------------------------------------------------------------------
        void OnDrawGizmos()
        {
            if (scenePerception != null)
            {
                SceneMesh[] sceneMeshs = scenePerception.GetComponentsInChildren<SceneMesh>();
                FloorDisplayer floorDisplayer = scenePerception.GetComponentInChildren<FloorDisplayer>();
                SceneMesh sm = null;
                foreach (var s in sceneMeshs)
                {
                    if (s.GetComponent<MeshRenderer>() != null)
                        sm = s;
                }

                if (sm != null)
                {
                    List<Vector3> vt3 = GetPositionsFromRendererBound(sm.GetComponent<MeshRenderer>(), scenePerception.transform, floorDisplayer.transform.position + new Vector3(0, 2, 0));
                }
            }
        }

        private List<Vector3> GetPositionsFromRendererBound(Renderer r, Transform ts, Vector3 offset)
        {
            Vector3[] sourcePoints = new Vector3[8];
            Vector3[] points = new Vector3[8];
            Vector3 center = Vector3.zero;
            Quaternion originalRotation = ts.rotation;
            ts.rotation = Quaternion.identity;

            Bounds bounds = r.bounds;
            sourcePoints[0] = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z) - ts.position; // Bot left near
            sourcePoints[1] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z) - ts.position; // Bot right near
            sourcePoints[2] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z) - ts.position; // Top left near
            sourcePoints[3] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z) - ts.position; // Top right near
            sourcePoints[4] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z) - ts.position; // Bot left far
            sourcePoints[5] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z) - ts.position; // Bot right far
            sourcePoints[6] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z) - ts.position; // Top left far
            sourcePoints[7] = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z) - ts.position; // Top right far

            ts.rotation = originalRotation;
            for (int s = 0; s < sourcePoints.Length; s++)
            {
                sourcePoints[s] = new Vector3(sourcePoints[s].x / ts.localScale.x,
                                              sourcePoints[s].y / ts.localScale.y,
                                              sourcePoints[s].z / ts.localScale.z);
            }

            for (int t = 0; t < points.Length; t++)
                points[t] = ts.TransformPoint(sourcePoints[t]);
            center = CalculateCubeCenter(points);

            Vector3 direction = (offset - center).normalized;
            float distance = Vector3.Distance(center, offset);
            for (int t = 0; t < points.Length; t++)
                points[t] = points[t] + (direction * distance);
            center = CalculateCubeCenter(points);

            for (int t = 0; t < points.Length; t++)
            {
                Vector3 d = (points[t] - center).normalized;
                Vector3 m = d * meshBoundExpand;
                points[t] += m;
            }

            if (points.Length == 0)
                return null;

            Color c = Color.red;

            Debug.DrawLine(points[0], points[1], c);
            Debug.DrawLine(points[2], points[3], c);
            Debug.DrawLine(points[0], points[2], c);
            Debug.DrawLine(points[1], points[3], c);

            Debug.DrawLine(points[4], points[5], c);
            Debug.DrawLine(points[6], points[7], c);
            Debug.DrawLine(points[4], points[6], c);
            Debug.DrawLine(points[5], points[7], c);

            Debug.DrawLine(points[0], points[4], c);
            Debug.DrawLine(points[1], points[5], c);
            Debug.DrawLine(points[2], points[6], c);
            Debug.DrawLine(points[3], points[7], c);

            return points.ToList();
        }

        private Vector3 CalculateCubeCenter(Vector3[] corners)
        {
            Vector3 center = Vector3.zero;
            foreach (Vector3 corner in corners)
                center += corner;
            center /= corners.Length;
            return center;
        }

        private Vector3 GetRandomPointOnPlane(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
        {
            Vector3 vector1 = point2 - point1;
            Vector3 vector2 = point3 - point1;
            float randomX = Random.value;
            float randomY = Random.value;
            Vector3 randomPoint = point1 + vector1 * randomX + vector2 * randomY;
            return randomPoint;
        }

        private Vector3 GetRandomPointOnPlaneAreaA(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
        {
            Vector3 newP1 = point1;
            Vector3 newP2 = PointTwoThirdsBetween(point1, point2, 2f, 3f);
            Vector3 newP3 = PointTwoThirdsBetween(point1, point3, 1f, 3f);

            Vector3 vector1 = newP2 - newP1;
            Vector3 vector2 = newP3 - newP1;
            float randomX = Random.value;
            float randomY = Random.value;
            Vector3 randomPoint = newP1 + vector1 * randomX + vector2 * randomY;
            return randomPoint;
        }

        private Vector3 GetRandomPointOnPlaneAreaB(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
        {
            Vector3 newP1 = new Vector3(PointTwoThirdsBetween(point1, point2, 1f, 3f).x, point1.y, PointTwoThirdsBetween(point1, point3, 2f, 3f).z);
            Vector3 newP2 = new Vector3(point2.x, point2.y, PointTwoThirdsBetween(point1, point3, 2f, 3f).z);
            Vector3 newP3 = new Vector3(PointTwoThirdsBetween(point1, point2, 1f, 3f).x, point3.y, point3.z);

            Vector3 vector1 = newP2 - newP1;
            Vector3 vector2 = newP3 - newP1;
            float randomX = Random.value;
            float randomY = Random.value;
            Vector3 randomPoint = newP1 + vector1 * randomX + vector2 * randomY;
            return randomPoint;
        }

        private Vector3 GetNormalizedByRandomPointOnPlane(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
        {
            Vector3 vector1 = point2 - point1;
            Vector3 vector2 = point3 - point1;
            Vector3 normal = Vector3.Cross(vector1, vector2).normalized;
            return normal;
        }

        private Vector3 PointTwoThirdsBetween(Vector3 a, Vector3 b, float numerator, float denominator)
        {
            float x1 = a.x;
            float y1 = a.y;
            float z1 = a.z;
            float x2 = b.x;
            float y2 = b.y;
            float z2 = b.z;

            // Calculate differences
            float dx = x2 - x1;
            float dy = y2 - y1;
            float dz = z2 - z1;

            // Scale differences by 2/3
            float dx_23 = (numerator / denominator) * dx;
            float dy_23 = (numerator / denominator) * dy;
            float dz_23 = (numerator / denominator) * dz;

            // Add scaled differences to point A
            float x_23 = x1 + dx_23;
            float y_23 = y1 + dy_23;
            float z_23 = z1 + dz_23;

            return new Vector3(x_23, y_23, z_23);
        }
    }
}