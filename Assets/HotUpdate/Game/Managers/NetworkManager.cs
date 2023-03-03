using Google.Protobuf;
using Protoc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActDemo
{
    public class NetworkManager : ManagerBase<NetworkManager>
    {
        private const string ServerUrl = "ws://116.205.247.142:8082";
        //private const string ServerUrl = "ws://127.0.0.1:8081";
        private WSSocketSession socketSession;

        public override void Start()
        {
            base.Start();
            Connect();
        }

        public void Connect()
        {
            if (socketSession == null)
            {
                var headers = new Dictionary<string, string>();
                headers.Add("User", "dxw");
                socketSession = new WSSocketSession(ServerUrl, "1001", headers, (res) =>
                {
                    var connectState = res ? "连接成功" : "连接失败";
                    SystemEventManager.Instance.RaiseEvent(new ConnectStateEvent() { EventType = EventType.ESocketConnectState, ConnectState = res });
                    Debug.Log($"websocket {connectState}");
                });
                socketSession?.ConnectAsync();
            }
            else if (!socketSession.IsConnected)
            {
                socketSession.ConnectAsync();
            }
        }

        public override void Update()
        {
            base.Update();
            if (socketSession != null && socketSession.IsConnected)
            {
                socketSession.Update();
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            socketSession?.Disconnect();
        }

        public void SendMsg(int msgId, IMessage message)
        {
            Debug.Log($"发送消息{msgId},消息内容:{message}");
            socketSession.SendAsync(msgId, message);
        }

        public void SendMsg<T>(IMessage message) where T : IMessage
        {
            socketSession.SendAsync<T>(message);
        }
    }
}
