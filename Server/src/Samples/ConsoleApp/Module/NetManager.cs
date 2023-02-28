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
        Dictionary<int, Session> sessionMap = new Dictionary<int, Session>();
        WebSocketServer server = null;
        int socketIndex = 0;

        public void Connect()
        {
            server = new WebSocketServer("ws://127.0.0.1:8081");
            server.Start(socket =>
            {
                int tempIndex = ++socketIndex;
                Session session = new Session(tempIndex, socket, (s) =>
                {
                    sessionMap.Add(tempIndex, s);
                }, (s) =>
                {
                    sessionMap.Remove(tempIndex);
                });
            });
        }

        public void BroadCastMsg(NetMessage msg)
        {
            foreach (var pair in sessionMap)
            {
                pair.Value.Send(msg.ToByteArray());
            }
        }

        public void BroadCastMsg(NetMessage msg, int sid)
        {
            foreach (var pair in sessionMap)
            {
                if (pair.Key != sid)
                {
                    pair.Value.Send(msg.ToByteArray());
                }
            }
        }

        public void BroadCastMsg(byte[] bytes)
        {
            foreach (var pair in sessionMap)
            {
                pair.Value.Send(bytes);
            }
        }
        public void BroadCastMsg(string msg)
        {
            foreach (var pair in sessionMap)
            {
                pair.Value.Send(msg);
            }
        }

        public void BroadCastMsg(byte[] bytes, int sid)
        {
            foreach (var pair in sessionMap)
            {
                if (pair.Key != sid)
                {
                    pair.Value.Send(bytes);
                }
            }
        }

        public void BroadCastMsg(string msg, int sid)
        {
            foreach (var pair in sessionMap)
            {
                if (pair.Key != sid)
                {
                    pair.Value.Send(msg);
                }
            }
        }

        public void BraocastBinaryMsg(byte[] bytes)
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
            }
        }

        /*
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
        */
    }
}
