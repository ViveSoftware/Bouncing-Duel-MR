using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System;

public enum State
{
    sNULL,
    sOnConnected,
    sOnConnectedToMaster,
    sOnDisconnected,
    sOnJoinedLobby,
    sOnCreatedRoom,
    sOnJoinedRoom,
    sOnLeftRoom,
    sOnCreateRoomFailed,
    sOnJoinRoomFailed,
    sOnPlayerEnteredRoom,
    sOnPlayerLeftRoom,
    sOnRoomListUpdate,
    sOnRoomPropertiesUpdate
}

public sealed class PhotonCallbacks : MonoBehaviourPunCallbacks
{
    #region EVENTS
    public event EventHandler<PhotonCallbackEvent> punEvent = null;

    public sealed class PhotonCallbackEvent : EventArgs
    {
        public State state = State.sNULL;
        public object data = null;
        public PhotonCallbackEvent(State _state, object _data)
        {
            state = _state;
            data = _data;
        }
    }
    #endregion

    #region DECLARE
    private static PhotonCallbacks instance;
    public static PhotonCallbacks Instance
    {
        get
        {
            if (instance == null)
                instance = new PhotonCallbacks();
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }
    #endregion

    #region CALLBACKS
    public override void OnConnected()
    {
        base.OnConnected();
        punEvent(this, new PhotonCallbackEvent(State.sOnConnected, null));
    }
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        punEvent(this, new PhotonCallbackEvent(State.sOnConnectedToMaster, null));
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        punEvent(this, new PhotonCallbackEvent(State.sOnDisconnected, cause));
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        punEvent(this, new PhotonCallbackEvent(State.sOnJoinedLobby, null));
    }
    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        punEvent(this, new PhotonCallbackEvent(State.sOnCreatedRoom, null));
    }

    /// <summary>
    /// Local Player
    /// </summary>
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        punEvent(this, new PhotonCallbackEvent(State.sOnJoinedRoom, null));
    }
    /// <summary>
    /// Local Player
    /// </summary>
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        punEvent(this, new PhotonCallbackEvent(State.sOnLeftRoom, null));
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        punEvent(this, new PhotonCallbackEvent(State.sOnCreateRoomFailed, returnCode.ToString() + " - " + message));
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        punEvent(this, new PhotonCallbackEvent(State.sOnJoinRoomFailed, returnCode.ToString() + " - " + message));
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
    }

    /// <summary>
    /// Remote Player
    /// </summary>
    /// <param name="newPlayer"></param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        punEvent(this, new PhotonCallbackEvent(State.sOnPlayerEnteredRoom, newPlayer));
    }
    /// <summary>
    /// Remote Player
    /// </summary>
    /// <param name="otherPlayer"></param>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        punEvent(this, new PhotonCallbackEvent(State.sOnPlayerLeftRoom, otherPlayer));
    }

    public override void OnCustomAuthenticationFailed(string debugMessage)
    {
        base.OnCustomAuthenticationFailed(debugMessage);
    }
    public override void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        base.OnCustomAuthenticationResponse(data);
    }
    public override void OnErrorInfo(ErrorInfo errorInfo)
    {
        base.OnErrorInfo(errorInfo);
    }
    public override void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        base.OnFriendListUpdate(friendList);
    }
    public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        base.OnLobbyStatisticsUpdate(lobbyStatistics);
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
    }
    public override void OnRegionListReceived(RegionHandler regionHandler)
    {
        base.OnRegionListReceived(regionHandler);
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        punEvent(this, new PhotonCallbackEvent(State.sOnRoomListUpdate, roomList));
    }
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        punEvent(this, new PhotonCallbackEvent(State.sOnRoomPropertiesUpdate, propertiesThatChanged));
    }
    public override void OnWebRpcResponse(OperationResponse response)
    {
        base.OnWebRpcResponse(response);
    }
    #endregion
}