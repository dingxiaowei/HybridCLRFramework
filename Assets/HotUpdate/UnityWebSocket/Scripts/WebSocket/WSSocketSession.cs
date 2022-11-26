using Google.Protobuf;
using System;
using System.Collections.Generic;

public class WSSocketSession
{
    protected WSSocketConnection mConnection;
    protected readonly MessageDispatcher mDispatcher = MessageDispatcher.sInstance;

    public bool IsConnected { get { return mConnection.IsConnect; } }

    public WSSocketSession(string serverUrl, string userIdStr, Dictionary<string, string> headers, Action<bool> onConnectedCallBack)
    {
        mConnection = new WSSocketConnection(serverUrl, userIdStr, headers, onConnectedCallBack);
    }

    public void ConnectAsync()
    {
        mConnection?.ConnectAsync();
    }

    public void ReConnect()
    {
        mConnection?.ReConnect();
    }

    public void SendAsync(int msgId, IMessage message)
    {
        mConnection?.SendAsync(msgId, message);
    }

    public void SendAsync<T>(IMessage message) where T : IMessage
    {
        var t = typeof(T);
        int msgId = NetMessageIdList.TypeToMsgId(t);
        mConnection?.SendAsync(msgId, message);
#if DEBUG_NETWORK
        UnityEngine.Debug.Log("Send message : " + msgId.ToString() + "_" + NetMessageIdList.MsgIdToType(msgId));
#endif
    }
    public void RegisterOnMessageReceived<T>(Action<object> handler) where T : IMessage
    {
        mDispatcher?.RegisterOnMessageReceived<T>(handler);
    }

    public void UnregisterOnMessageReceived<T>(Action<object> handler) where T : IMessage
    {
        mDispatcher?.UnregisterOnMessageReceived<T>(handler);
    }


    public void Disconnect()
    {
        mConnection?.DisConnect();
        DisposeOnDisconnected();
    }

    public void Update()
    {
        if (IsConnected)
        {
            mDispatcher?.DispatchMessages();
        }
    }

    protected void DisposeOnDisconnected()
    {

    }
}
