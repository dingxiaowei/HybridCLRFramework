using Fleck;
using Google.Protobuf;
using Protoc;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ServerDemo
{
    class NetManager : Singleton<NetManager>
    {
        List<IWebSocketConnection> allSockets = new List<IWebSocketConnection>();
        Dictionary<int, IWebSocketConnection> idSocketMap = new Dictionary<int, IWebSocketConnection>();
        Dictionary<IWebSocketConnection, int> socketIdMap = new Dictionary<IWebSocketConnection, int>();
        WebSocketServer server = null;
        int socketIndex = 0;

        public void Connect()
        {
            server = new WebSocketServer("ws://127.0.0.1:8081");
            server.Start(socket =>
            {
                var id = socket.ConnectionInfo.Id;
                socket.OnOpen = () =>
                {
                    allSockets.Add(socket);
                    Console.WriteLine(id + ": Connected");
                    socketIndex++;
                    socketIdMap.Add(socket, socketIndex);
                    idSocketMap.Add(socketIndex, socket);
                };
                socket.OnClose = () =>
                {
                    idSocketMap.Remove(socketIdMap[socket]);
                    allSockets.Remove(socket);
                    Console.WriteLine(id + ": Closed");
                    socketIdMap.Remove(socket);
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
            var netMsg = NetMessage.Parser.ParseFrom(bytes);
            int msgType = netMsg.Type;
            //TODO:要做消息分发
            if (msgType == (int)MessageNumber.C2S_RegisterUserInfoResquest)
            {
                var c2s_RegisterUserInfoRequest = C2S_RegisterUserInfoRequest.Parser.ParseFrom(netMsg.Content);
                //给当前角色返回消息S2C_RegisterUserInfoResponse
                var msg = new S2C_RegisterUserInfoResponse();
                msg.Error = 0;
                msg.Message = "";
                msg.UserStateInfo = new CUserStateInfo()
                {
                    Rotate = new Vec3Data() { X = 0, Y = 0, Z = 0 },
                    Pos = new Vec3Data() { X = 3.12f, Y = 4.17f, Z = 17.71f },
                    UserInfo = new CUserInfo()
                    {
                        UserName = c2s_RegisterUserInfoRequest.UserInfo.UserName,
                        UserId = socketIndex
                    }
                };
                var returnMsg = new NetMessage()
                {
                    Type = (int)MessageNumber.S2C_RegisterUserInfoResponse,
                    Content = msg.ToByteString(),
                };
                foreach (var socket in allSockets)
                {
                    Console.WriteLine($"======给{socket.ConnectionInfo.Id}广播消息");
                    socket.Send(returnMsg.ToByteArray());
                }
            }
            //Console.WriteLine(netMsg.Type);
            //var c2s_RegisterMsg = C2S_RegisterUserInfoRequest.Parser.ParseFrom(netMsg.Content);
            //var user = c2s_RegisterMsg.UserInfo;
            //foreach (var socket in allSockets)
            //{
            //    Console.WriteLine($"======给{socket.ConnectionInfo.Id}广播消息");
            //    socket.Send(bytes);
            //}
        }

        public void BroacastStringMsg(string message)
        {
            //Console.WriteLine("接收到string的消息");
            foreach (var socket in allSockets)
            {
                Console.WriteLine($"-------给{socket.ConnectionInfo.Id}广播消息");
                socket.Send(message);
            }
        }
    }
}
