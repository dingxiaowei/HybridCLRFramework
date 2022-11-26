using Google.Protobuf;
using Protoc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityWebSocket;

public class WSSocketConnection
{
    private string mServerUrl;
    private string mServerIdStr = "1001";
    private Dictionary<string, string> mHeaders;
    private IWebSocket mSocket;
    private Action<bool> mOnConnected;
    private bool connectResReturn = false;
    private int reconnectTryTimes = 0;
    //private IMessagePacker mMessagePacker;
    public bool IsConnect { get { return mSocket != null ? mSocket.ReadyState == WebSocketState.Open : false; } }
    ObjectPoolWithReset<NetMessage> mNetMessagePool;
    public WSSocketConnection(string serverUrl, string serverIdStr, Dictionary<string, string> headers, Action<bool> onConnectedCallBack)
    {
        mServerUrl = serverUrl;
        mServerIdStr = serverIdStr;
        mHeaders = headers;
        mOnConnected = onConnectedCallBack;
        connectResReturn = false;
        reconnectTryTimes = 0;
        mNetMessagePool = new ObjectPoolWithReset<NetMessage>(10);
        //mMessagePacker = new ProtobufPacker();
    }

    public void SendAsync(int msgId, IMessage message)
    {
        if (mSocket.ReadyState == WebSocketState.Open)
        {
            var msg = mNetMessagePool.Get();
            msg.Type = msgId;
            msg.Xid = mServerIdStr;
            msg.Oid = "";
            msg.Content = message.ToByteString();
            //ProtobufHelper.ToBytes(msg);
            mSocket.SendAsync(msg.ToByteArray());
            mNetMessagePool.Return(msg);
        }
        else
        {
#if DEBUG_NETWORK
            Debug.LogError($"消息:{msgId}发送失败,当前网络连接状态:{mSocket.ReadyState}");
#endif
        }
    }

    public void ConnectAsync()
    {
        connectResReturn = false;
        Task.Delay(5000).ContinueWith(_ =>
        {
            Debug.Log("5s检测连接结果");
            if (!connectResReturn)
            {
                if (reconnectTryTimes++ < 3)
                {
                    Debug.LogError("没收到返回结果，并且尝试次数小于3，继续尝试重连");
                    ConnectAsync();
                }
                else
                {
                    Debug.Log("异常连接没有收到返回结果重新尝试次数大于3，不继续尝试重连");
                }
            }
            else
            {
                Debug.Log("socket连接结果正常返回");
            }
        });
        Debug.Log($"serverUrl:{mServerUrl}");
        mSocket = new WebSocket(mServerUrl);
        mSocket.OnOpen += Socket_OnOpen;
        mSocket.OnMessage += Socket_OnMessage;
        mSocket.OnClose += Socket_OnClose;
        mSocket.OnError += Socket_OnError;
#if DEBUG_NETWORK
        Debug.Log(string.Format("Connecting...\n"));
#endif
        Debug.Log($"header是否为空:{mHeaders == null}");
        mSocket.ConnectAsync(mHeaders);
    }

    public void ReConnect()
    {
        //if (mSocket != null)
        //    mSocket.ConnectAsync(mHeaders);
        DisConnect();
        ConnectAsync();
    }

    private void Socket_OnOpen(object sender, OpenEventArgs e)
    {
#if DEBUG_NETWORK
        Debug.Log(string.Format("Connected: {0}\n", mServerUrl));
#endif
        connectResReturn = true;
        if (mSocket.ReadyState == WebSocketState.Open)
        {
            mOnConnected?.Invoke(true);
        }
        else
        {
            mOnConnected?.Invoke(false);
        }
    }

    private void Socket_OnMessage(object sender, MessageEventArgs e)
    {
        if (e.IsBinary)
        {
#if DEBUG_NETWORK
            Debug.Log(string.Format("Receive Bytes ({1}): {0}\n", e.Data, e.RawData.Length));
#endif
            var netMsg = NetMessage.Parser.ParseFrom(e.RawData);
#if DEBUG_NETWORK
            Debug.Log($"Receive Msg ID:{netMsg.Type} xid:{netMsg.Xid}");
#endif
            MessageDispatcher.sInstance.ReceiveMessage(netMsg);
        }
        else if (e.IsText)
        {
#if DEBUG_NETWORK
            Debug.Log(string.Format("Receive Text: {0}\n", e.Data));
#endif
        }
    }

    private void Socket_OnClose(object sender, CloseEventArgs e)
    {
#if DEBUG_NETWORK
        Debug.Log(string.Format("WebSocket Closed: StatusCode: {0}, Reason: {1}\n", e.StatusCode, e.Reason));
#endif
    }

    private void Socket_OnError(object sender, ErrorEventArgs e)
    {
#if DEBUG_NETWORK
        Debug.LogError(string.Format("WebSocket Error: {0}\n", e.Message));
        mOnConnected?.Invoke(false);
#endif
        connectResReturn = true;
    }

    public void DisConnect()
    {
        if (mSocket != null && mSocket.ReadyState != WebSocketState.Closed)
        {
            mSocket.CloseAsync();
            mNetMessagePool?.Clear();
        }
    }
}
