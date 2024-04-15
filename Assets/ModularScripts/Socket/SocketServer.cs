using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class SocketServer : MonoBehaviour
{
    private Socket serverSocket;
    private Socket clientSocket;

    public int targetSize = 0;
    public int currentLength = 0;

    public void StartListen()
    {
        StopListen();

        IPAddress ipAddress = IPAddress.Parse(SocketManager.Instance.serverIP);
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, SocketUtils.PORT);

        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //serverSocket.NoDelay = true;

        serverSocket.Bind(localEndPoint);
        serverSocket.Listen(SocketUtils.LISTEN);
        serverSocket.BeginAccept(new AsyncCallback(Accept_Callback), serverSocket);
    }

    public void StopListen()
    {
        if (serverSocket != null)
        {
            Debug.Log($"<color=yellow>[SocketServer] Server Socket Close.</color>");
            if (serverSocket.Connected)
            {
                serverSocket.Shutdown(SocketShutdown.Both);
                serverSocket.Disconnect(false);
            }
            if (clientSocket != null && clientSocket.Connected)
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Disconnect(false);
            }

            if (serverSocket != null)
                serverSocket.Close(0);
            if (clientSocket != null)
                clientSocket.Close(0);
            SocketManager.Instance.isConnected = false;
            SocketManager.Instance.isDataTransferred = false;
        }
    }

    public void SendDataToClient(byte[] bytes)
    {
        targetSize = bytes.Length;
        currentLength = 0;

        Debug.Log($"<color=lime>[SocketServer] Ready To Send.</color>");
        Debug.Log($"<color=lime>[SocketServer] {bytes.Length}.</color>");
        clientSocket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, new AsyncCallback(Send_Callback), clientSocket);
    }

    private void Accept_Callback(IAsyncResult ar)
    {
        try
        {
            if (ar.AsyncState != null)
            {
                // after client connect...
                Socket socket = ar.AsyncState as Socket;
                clientSocket = socket.EndAccept(ar);

                SocketManager.Instance.isConnected = true;
                Debug.Log($"<color=lime>[SocketServer] Client Has Connected.</color>");
            }
        }
        catch (Exception e)
        {
            Debug.Log("[SocketServer][Accept_Callback] server exception error:" + e.ToString());
            return;
        }
    }

    private void Send_Callback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            Socket socket = ar.AsyncState as Socket;

            // Complete sending the data to the remote device.
            int bytesSent = socket.EndSend(ar);

            if (bytesSent > 0)
            {
                Debug.Log($"<color=lime>[SocketServer][Send_Callback] Sent {bytesSent} bytes to client.</color>");
                currentLength = currentLength + bytesSent;

                if (currentLength != targetSize)
                {
                }
                else if (currentLength == targetSize)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    Debug.Log($"<color=lime>[SocketServer][Send_Callback] Socket Shutdown.</color>");
                    SocketManager.Instance.isDataTransferred = true;
                }
            }
        }
        catch (Exception e)
        {
            SocketManager.Instance.SocketDisconnect();
            Debug.Log("[SocketServer][Send_Callback] server exception error:" + e.ToString());
            return;
        }
    }
}
