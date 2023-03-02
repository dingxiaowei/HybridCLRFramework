﻿using Fleck;
using Google.Protobuf;
using Protoc;
using System;
using System.Collections.Generic;

namespace ServerDemo
{
    public class Session
    {
        private int sid = 0;
        public int Sid { get { return sid; } }
        private IWebSocketConnection socket;
        private Guid connectInfoId;
        private Action<Session> OnOpenEvent;
        private Action<Session> OnCloseEvent;
        public Session(int sid, IWebSocketConnection socket, Action<Session> onOpenEvent, Action<Session> onCloseEvent)
        {
            this.sid = sid;
            this.socket = socket;
            connectInfoId = socket.ConnectionInfo.Id;
            socket.OnOpen += OnOpen;
            socket.OnBinary += OnBinaryMsg;
            socket.OnMessage += OnMessage;
            socket.OnClose += OnClose;
            socket.OnError += OnError;
            OnOpenEvent = onOpenEvent;
            OnCloseEvent = onCloseEvent;
        }

        public void Release()
        {
            socket.OnOpen -= OnOpen;
            socket.OnBinary -= OnBinaryMsg;
            socket.OnMessage -= OnMessage;
            socket.OnClose -= OnClose;
            socket.OnError -= OnError;

            socket = null;
        }

        public void Send(byte[] msg)
        {
            socket.Send(msg);
        }

        public void Send(string msg)
        {
            socket.Send(msg);
        }

        void OnError(Exception ex)
        {
            Console.WriteLine(connectInfoId + ": " + ex.Message);
        }

        void OnClose()
        {
            Console.WriteLine(connectInfoId + ": Closed");
            OnCloseEvent?.Invoke(this);
        }

        void OnOpen()
        {
            Console.WriteLine(connectInfoId + ": Connected");
            OnOpenEvent?.Invoke(this);
        }

        void OnMessage(string msg)
        {
            NetManager.Instance.BroadCastMsg(msg);
        }

        void OnBinaryMsg(byte[] bytes)
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
                        UserId = sid
                    }
                };
                Send((int)MessageNumber.S2C_RegisterUserInfoResponse, msg);
            }
            else if (msgType == (int)MessageNumber.C2S_UserStateInfosRequest)
            {
                Console.WriteLine("收到客户端发来的C2S_UserStateInfosRequest");
                var c2s_UserStateInfosRequest = C2S_UserStateInfosRequest.Parser.ParseFrom(netMsg.Content);
                var uid = c2s_UserStateInfosRequest.MyUserId;
                Console.WriteLine("当前传过来的玩家id是:");
                Console.WriteLine(uid);
                Console.WriteLine(sid);
                var msg = new S2C_UserStateInfosResponse();
                msg.Error = 0;
                msg.Message = "";
                List<CUserStateInfo> userStateInfos = new List<CUserStateInfo>();
                //TODO:添加其他玩家，需要管理Player
                //userStateInfos.Add(new CUserStateInfo()
                //{
                //    Rotate = new Vec3Data() { X = 0, Y = 0, Z = 0 },
                //    Pos = new Vec3Data() { X = 4.12f, Y = 4.17f, Z = 10.71f },
                //    UserInfo = new CUserInfo()
                //    {
                //        UserName = "test1",
                //        UserId = 2
                //    }
                //});
                msg.UserStateInfos.AddRange(userStateInfos);
                Console.WriteLine("发送S2C_UserStateInfosResponse,数量:" + msg.UserStateInfos.Count);
                Send((int)MessageNumber.S2C_UserStateInfosResponse, msg);
            }
        }

        public void Send(int msgId, IMessage msg)
        {
            var returnMsg = new NetMessage()
            {
                Type = msgId,
                Content = msg.ToByteString(),
            };
            socket.Send(returnMsg.ToByteArray());
        }
    }
}
