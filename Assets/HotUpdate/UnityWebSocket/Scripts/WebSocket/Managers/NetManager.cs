using System.Collections.Generic;
using Protoc.AutoRegister.HeartBeat;
using UnityEngine;

namespace Protoc.Managers
{
    public class NetManager : Singleton<NetManager>
    {
        //private string serverUrl = "ws://127.0.0.1:8081";
        private string serverUrl = "ws://124.223.54.98:8081";

        public string ServerURL
        {
            get => serverUrl;
            set => serverUrl = value;
        }


        private WSSocketSession socketSession;

        public WSSocketSession SocketSession
        {
            get => socketSession;
        }

        public void ConnectServer()
        {
            var headers = new Dictionary<string, string>();
            headers.Add("User", "dxw");
            socketSession = new WSSocketSession(serverUrl, "1001", headers, (res) =>
            {
                var connectState = res ? "连接成功" : "连接失败";
                Debug.Log($"websocket {connectState}");
            });
            socketSession?.ConnectAsync();
        }

        public void DisConnectServer()
        {
            socketSession?.Disconnect();
        }
        
        public void ReConnectServer()
        {
            socketSession?.ReConnect();
        }

        public void SendHeartBeat()
        {
            HeartBeatManager.sInstance.SendHeartBeatRequest();
        }
    }
}