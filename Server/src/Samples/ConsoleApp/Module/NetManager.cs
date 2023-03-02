using Fleck;
using Google.Protobuf;
using Protoc;
using System.Collections.Generic;

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
    }
}
