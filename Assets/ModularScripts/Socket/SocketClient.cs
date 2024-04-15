using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class SocketClient : MonoBehaviour
{
    private Socket clientSocket;

    public byte[] byteData = null;
    private byte[] tempData = null;

    public int targetSize = 0;
    public int currentLength = 0;

    private bool once = true;
    private DateTime start;
    private DateTime cost;

    public void Connect()
    {
        Disconnect();

        IPAddress ipAddress = IPAddress.Parse(SocketManager.Instance.serverIP);
        IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, SocketUtils.PORT);

        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //clientSocket.NoDelay = true;

        clientSocket.BeginConnect(remoteEndPoint, new AsyncCallback(Connect_Callback), clientSocket);
    }

    public void Disconnect()
    {
        if (clientSocket != null)
        {
            Debug.Log($"<color=yellow>[SocketClient] Client Socket Close.</color>");
            if (clientSocket.Connected)
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Disconnect(false);
            }
            clientSocket.Close(0);
            SocketManager.Instance.isConnected = false;
            SocketManager.Instance.isDataTransferred = false;
        }
    }

    public void ReceiveDataFromServer(int size)
    {
        targetSize = size;
        //Use "SocketUtils.PACKET_SIZE" if you want to transfer slowly.
        tempData = new byte[SocketUtils.PACKET_SIZE];
        byteData = new byte[targetSize];
        currentLength = 0;

        Debug.Log($"<color=lime>[SocketClient] Ready To Receive.</color>");
        clientSocket.BeginReceive(tempData, 0, tempData.Length, SocketFlags.None, new AsyncCallback(Receive_Callback), clientSocket);
    }

    private void Connect_Callback(IAsyncResult ar)
    {
        try
        {
            if (ar.AsyncState != null)
            {
                Socket socket = ar.AsyncState as Socket;
                socket.EndConnect(ar);
                if (clientSocket.Connected)
                {
                    SocketManager.Instance.isConnected = true;
                    Debug.Log($"<color=lime>[SocketClient] {socket.ReceiveBufferSize}</color>");
                    Debug.Log($"<color=lime>[SocketClient] Success Connect To Server.</color>");
                    //--------------------------------------------------------------------------
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("[SocketClient][Connect_Callback] client exception error:" + e.ToString());
            return;
        }
    }

    private void Receive_Callback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            Socket socket = ar.AsyncState as Socket;

            // Complete received the data to the remote device.
            int bytesSent = socket.EndReceive(ar);

            if (bytesSent > 0)
            {
                if (once)
                {
                    start = DateTime.Now;
                    once = false;
                }
                Debug.Log($"<color=lime>[SocketClient][Receive_Callback] Receive {bytesSent} bytes from server.</color>");
                currentLength = currentLength + bytesSent;
                Debug.Log(currentLength);

                System.Buffer.BlockCopy(tempData, 0, byteData, currentLength - bytesSent, bytesSent);

                if (currentLength != targetSize)
                {
                    //Use "SocketUtils.PACKET_SIZE" if you want to transfer slowly.
                    tempData = new byte[SocketUtils.PACKET_SIZE];
                    clientSocket.BeginReceive(tempData, 0, tempData.Length, SocketFlags.None, new AsyncCallback(Receive_Callback), clientSocket);
                }
                else if (currentLength == targetSize)
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    Debug.Log($"<color=lime>[SocketClient][Receive_Callback] Socket Shutdown.</color>");

                    cost = DateTime.Now;
                    Debug.Log(start);
                    Debug.Log(cost);
                    SocketManager.Instance.isDataTransferred = true;
                }
            }
        }
        catch (Exception e)
        {
            SocketManager.Instance.SocketDisconnect();
            Debug.Log("[SocketClient][Receive_Callback] client exception error:" + e.ToString());
            return;
        }
    }
}