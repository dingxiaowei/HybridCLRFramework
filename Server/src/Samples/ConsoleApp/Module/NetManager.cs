using Fleck;
using System;
using System.Collections.Generic;

namespace ServerDemo
{
    class NetManager : Singleton<NetManager>
    {
        List<IWebSocketConnection> allSockets = new List<IWebSocketConnection>();
        WebSocketServer server = null;

        //protected override void InitSingleton()
        //{
        //    base.InitSingleton();
        //    Connect();
        //}

        public void Connect()
        {
            server = new WebSocketServer("ws://127.0.0.1:8082");
            server.Start(socket =>
            {
                var id = socket.ConnectionInfo.Id;
                socket.OnOpen = () =>
                {
                    allSockets.Add(socket);
                    Console.WriteLine(id + ": Connected");
                };
                socket.OnClose = () =>
                {
                    allSockets.Remove(socket);
                    Console.WriteLine(id + ": Closed");
                };
                socket.OnBinary += BroacastBinaryMsg;
                socket.OnMessage += BroacastStringMsg;
                socket.OnError = error =>
                {
                    Console.WriteLine(socket.ConnectionInfo.Id + ": " + error.Message);
                };
            });
        }

        public void BroacastBinaryMsg(byte[] bytes)
        {
            foreach (var socket in allSockets)
            {
                Console.WriteLine($"======给{socket.ConnectionInfo.Id}广播消息");
                socket.Send(bytes);
            }
        }

        public void BroacastStringMsg(string message)
        {
            foreach (var s in allSockets)
            {
                s.Send(message);
            }
        }
    }
}
