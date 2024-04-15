using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PhotonCallbacks))]
public sealed class PhotonManager : MonoBehaviour
{
    public enum Region
    {
        asia, au, eu, hk, jp, za, sa, kr, uae, us, usw,
    }

    [SerializeField] public Region region = Region.jp;

    #region DECLARE
    private static PhotonManager instance;

    public static PhotonManager Instance
    {
        get
        {
            if (instance == null)
                instance = new PhotonManager();
            return instance;
        }
    }

    private readonly TypedLobby DEFAULT_LOBBY = new TypedLobby("AnchorSharingLobby", LobbyType.SqlLobby);

    public readonly int MaxPlayersNum = 2;

    public List<RoomInfo> roomList = new List<RoomInfo>();
    #endregion

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        //Register
        PhotonCallbacks.Instance.punEvent += PhotonCB;

        //this.InitServer();
        //this.Connect();
    }

    private void OnApplicationQuit()
    {
        this.Disconnect();
    }


    #region PUBLIC API
    public void InitServer(string strAppID, string strRegion)
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = strAppID;

        PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = "1";
        PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = true;

        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = strRegion;
        PhotonNetwork.PhotonServerSettings.DevRegion = strRegion;

        PhotonNetwork.PhotonServerSettings.AppSettings.Protocol = ConnectionProtocol.Udp;
        PhotonNetwork.PhotonServerSettings.AppSettings.NetworkLogging = DebugLevel.ERROR;
        PhotonNetwork.PhotonServerSettings.PunLogging = PunLogLevel.ErrorsOnly;
        PhotonNetwork.PhotonServerSettings.EnableSupportLogger = false;

        PhotonNetwork.PhotonServerSettings.RunInBackground = true;
    }

    public void Connect()
    {
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.QuickResendAttempts = 3;
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.SentCountAllowance = 7;
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = 5000;
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.CrcEnabled = true;
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.MaximumTransferUnit = 520;

        PhotonNetwork.Disconnect();
        PhotonNetwork.ConnectUsingSettings();
    }

    public void CreateRoom(string roomName)
    {
        if (PhotonNetwork.InRoom)
            return;

        Debug.Log($"<color=lime>[PhotonPUN] {roomName}.</color>");
        RoomOptions roomOptions = new RoomOptions
        {
            //MaxPlayers = MaxPlayersNum,
            MaxPlayers = 2,
            IsOpen = true,
            IsVisible = true,
            PublishUserId = true,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable { { "C0", "RoomCode" } },
            CustomRoomPropertiesForLobby = new string[] { "C0" }
        };
        PhotonNetwork.CreateRoom(roomName, roomOptions, DEFAULT_LOBBY);
    }

    public void JoinRoom(string roomName)
    {
        if (PhotonNetwork.InRoom)
            return;
        PhotonNetwork.JoinRoom(roomName);
    }

    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
    }

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    public GameObject InitiateObject(string path, Vector3 position, Quaternion rotation, byte group, object[] dataStructSets)
    {
        var photonGameObject = PhotonNetwork.Instantiate(path, position, rotation, group, dataStructSets);
        return photonGameObject;
    }

    public void DestoryObject(GameObject obj)
    {
        PhotonNetwork.Destroy(obj);
    }

    public void LoadLevel(string levelName)
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(levelName, LoadSceneMode.Single);
        Destroy(gameObject);
    }

    public void LoadLevel_Photon(string levelName)
    {
        PhotonNetwork.LoadLevel(levelName);
    }

    public bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }

    public bool InRoom()
    {
        return PhotonNetwork.InRoom;
    }

    public bool InLobby()
    {
        return PhotonNetwork.InLobby;
    }

    public void SetRoomProperty(string key, object value)
    {
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { key, value } });
    }

    public T GetRoomProperty<T>(string key)
    {
        return (T)PhotonNetwork.CurrentRoom.CustomProperties[key];
    }

    public bool TryGetRoomProperty(string key, out object value)
    {
        return PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out value);
    }

    public void RefreshRoomList()
    {
        if (PhotonNetwork.InLobby)
        {
            string ROOM_CODE = "C0";
            string strRoomCode = "RoomCode";
            PhotonNetwork.GetCustomRoomList(DEFAULT_LOBBY, ROOM_CODE + " = '" + strRoomCode + "'");
        }
    }
    #endregion

    //----------------------------------------------------------------
    private void PhotonCB(object sender, PhotonCallbacks.PhotonCallbackEvent args)
    {
        switch (args.state)
        {
            case State.sOnConnected:
                OnConnected();
                break;
            case State.sOnConnectedToMaster:
                OnConnectedToMaster();
                break;
            case State.sOnDisconnected:
                OnDisconnected((DisconnectCause)args.data);
                break;
            case State.sOnJoinedLobby:
                OnJoinedLobby();
                break;
            case State.sOnCreatedRoom:
                OnCreatedRoom();
                break;
            case State.sOnJoinedRoom:
                OnJoinedRoom();
                break;
            case State.sOnLeftRoom:
                OnLeftRoom();
                break;
            case State.sOnCreateRoomFailed:
                OnCreateRoomFailed((string)args.data);
                break;
            case State.sOnJoinRoomFailed:
                OnJoinRoomFailed((string)args.data);
                break;
            case State.sOnPlayerEnteredRoom:
                OnPlayerEnteredRoom((Player)args.data);
                break;
            case State.sOnPlayerLeftRoom:
                OnPlayerLeftRoom((Player)args.data);
                break;
            case State.sOnRoomListUpdate:
                OnRoomListUpdate((List<RoomInfo>)args.data);
                break;
            case State.sOnRoomPropertiesUpdate:
                OnRoomPropertiesUpdate((ExitGames.Client.Photon.Hashtable)args.data);
                break;
        }
    }

    #region Callbacks
    private void OnConnected()
    {
        Debug.Log($"<color=lime>[PhotonPUN] OnConnected.</color>");
    }
    private void OnConnectedToMaster()
    {
        Debug.Log($"<color=lime>[PhotonPUN] OnConnectedToMaster.</color>");
        //PhotonNetwork.JoinLobby();
    }
    private void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"<color=red>[PhotonPUN] OnDisconnected.</color>");
        //this.LoadLevel(SceneManager.GetActiveScene().name);
        //this.Connect();
    }
    private void OnJoinedLobby()
    {
        Debug.Log($"<color=lime>[PhotonPUN] OnJoinedLobby.</color>");
        string ROOM_CODE = "C0";
        string strRoomCode = "RoomCode";
        PhotonNetwork.GetCustomRoomList(DEFAULT_LOBBY, ROOM_CODE + " = '" + strRoomCode + "'");
    }
    private void OnCreatedRoom()
    {
        Debug.Log($"<color=yellow>[PhotonPUN] OnCreatedRoom.</color>");
    }
    private void OnJoinedRoom()
    {
        Debug.Log($"<color=yellow>[PhotonPUN] OnJoinedRoom, Current Player: {PhotonNetwork.PlayerList.Length}.</color>");
    }
    private void OnLeftRoom()
    {
        Debug.Log($"<color=yellow>[PhotonPUN] OnLeftRoom.</color>");
        Debug.Log($"<color=yellow>[PhotonPUN] ---------------</color>");
    }
    private void OnCreateRoomFailed(string message)
    {
        Debug.Log($"<color=red>[PhotonPUN] OnCreateRoomFailed: {message}.</color>");
        //this.LoadLevel(SceneManager.GetActiveScene().name);
    }
    private void OnJoinRoomFailed(string message)
    {
        Debug.Log($"<color=red>[PhotonPUN] OnJoinRoomFailed: {message}.</color>");
        // this.LoadLevel(SceneManager.GetActiveScene().name);
    }
    private void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"<color=yellow>[PhotonPUN] OnPlayerEnteredRoom: {newPlayer.NickName}.</color>");
    }
    private void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"<color=yellow>[PhotonPUN] OnPlayerLeftRoom: {otherPlayer.NickName}.</color>");
    }
    private void OnRoomListUpdate(List<RoomInfo> rooms)
    {
        if (rooms.Count <= 0)
            return;
        roomList.Clear();
        foreach (var i in rooms)
        {
            roomList.Add(i);
            Debug.Log($"<color=yellow>[PhotonPUN] OnRoomListUpdate: {i.Name}, {i.PlayerCount} / {i.MaxPlayers}.</color>");
        }
    }
    private void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        foreach (var i in propertiesThatChanged)
            Debug.Log($"<color=yellow>[PhotonPUN] OnRoomPropertiesUpdate: [Key]: {i.Key}, [Value]: {i.Value}.</color>");
    }
    #endregion
}