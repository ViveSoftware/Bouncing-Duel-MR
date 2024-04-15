using System;
using UnityEngine;

public class SocketManager : MonoBehaviour
{
    private static SocketManager instance = null;

    public static SocketManager Instance
    {
        get
        {
            if (instance == null)
                instance = new SocketManager();
            return instance;
        }
    }

    [SerializeField] private SocketUtils.SocketRole socketRole = SocketUtils.SocketRole.NULL;

    [SerializeField] public string serverIP = "127.0.0.1";

    [SerializeField] public bool isConnected = false;
    [SerializeField] public bool isDataTransferred = false;

    public event EventHandler disconnectEvent = null;
    private void Awake()
    {
        instance = this;
    }

    public void InitSocket(SocketUtils.SocketRole role)
    {
        socketRole = role;
        isConnected = false;
        isDataTransferred = false;

        if (role == SocketUtils.SocketRole.Server)
        {
            gameObject.AddComponent<SocketServer>();
            gameObject.GetComponent<SocketServer>().StartListen();
        }
        else if (role == SocketUtils.SocketRole.Client)
        {
            gameObject.AddComponent<SocketClient>();
            gameObject.GetComponent<SocketClient>().Connect();
        }
    }

    public void KillSocket()
    {
        if (socketRole == SocketUtils.SocketRole.Server &&
            gameObject.GetComponent<SocketServer>())
        {
            gameObject.GetComponent<SocketServer>().StopListen();
            Destroy(gameObject.GetComponent<SocketServer>());
            Debug.Log($"<color=red>[SocketServer] Killed.</color>");
        }
        if (socketRole == SocketUtils.SocketRole.Client &&
            gameObject.GetComponent<SocketClient>())
        {
            gameObject.GetComponent<SocketClient>().Disconnect();
            Destroy(gameObject.GetComponent<SocketClient>());
            Debug.Log($"<color=red>[SocketClient] Killed.</color>");
        }

        socketRole = SocketUtils.SocketRole.NULL;
        serverIP = "127.0.0.1";
        isConnected = false;
        isDataTransferred = false;
    }

    public void SocketDisconnect()
    {
        KillSocket();
        disconnectEvent(this, null);
    }

    private void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isConnected = true;
            isDataTransferred = true;
        }
        if (Input.GetKeyDown(KeyCode.F5))
            InitSocket(SocketUtils.SocketRole.Server);
        else if (Input.GetKeyDown(KeyCode.F6))
            InitSocket(SocketUtils.SocketRole.Client);
        else if (Input.GetKeyDown(KeyCode.F7))
            KillSocket();

        if (Input.GetKeyDown(KeyCode.F8))
        {
            if (gameObject.GetComponent<SocketServer>())
            {
                SocketUtils.AnchorData anchorData = new SocketUtils.AnchorData();
                SocketUtils.AnchorData.acVector3 acVector3Pos = new SocketUtils.AnchorData.acVector3();
                acVector3Pos.x = 100;
                acVector3Pos.y = 100;
                acVector3Pos.z = 100;
                SocketUtils.AnchorData.acVector4 acVector4Rot = new SocketUtils.AnchorData.acVector4();
                acVector4Rot.x = 90;
                acVector4Rot.y = 180;
                acVector4Rot.z = 90;
                acVector4Rot.w = 0;
                anchorData.anchorBytes = new byte[1024 * 1024 * 60];
                anchorData.anchorPos = acVector3Pos;
                anchorData.anchorRot = acVector4Rot;
                byte[] temp = SocketUtils.ToByteArray(anchorData);
                Debug.Log(temp.Length);
                gameObject.GetComponent<SocketServer>().SendDataToClient(temp);
            }
            if (gameObject.GetComponent<SocketClient>())
            {
                gameObject.GetComponent<SocketClient>().ReceiveDataFromServer(62914986);
            }
        }
#endif
    }
}