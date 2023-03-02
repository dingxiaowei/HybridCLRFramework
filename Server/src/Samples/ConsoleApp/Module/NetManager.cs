using Fleck;
using Google.Protobuf;
using Protoc;
using System.Collections.Generic;

namespace ServerDemo
{
    class NetManager : Singleton<NetManager>
    {
        public Dictionary<int, Session> SessionMap = new Dictionary<int, Session>();
        WebSocketServer server = null;
        int socketIndex = 0;

        public void Connect()
        {
            server = new WebSocketServer("ws://0.0.0.0:8081");
            server.Start(socket =>
            {
                int tempIndex = ++socketIndex;
                Session session = new Session(tempIndex, socket, (s) =>
                {
                    SessionMap.Add(tempIndex, s);
                }, (s) =>
                {
                    SessionMap.Remove(tempIndex);
                });
            });
        }

        public void BroadCastMsg(NetMessage msg)
        {
            foreach (var pair in SessionMap)
            {
                pair.Value.Send(msg.ToByteArray());
            }
        }

        public void BroadCastMsg(NetMessage msg, int sid)
        {
            foreach (var pair in SessionMap)
            {
                if (pair.Key != sid)
                {
                    pair.Value.Send(msg.ToByteArray());
                }
            }
        }

        public void BroadCastMsg(byte[] bytes)
        {
            foreach (var pair in SessionMap)
            {
                pair.Value.Send(bytes);
            }
        }
        public void BroadCastMsg(string msg)
        {
            foreach (var pair in SessionMap)
            {
                pair.Value.Send(msg);
            }
        }

        public void BroadCastMsg(byte[] bytes, int sid)
        {
            foreach (var pair in SessionMap)
            {
                if (pair.Key != sid)
                {
                    pair.Value.Send(bytes);
                }
            }
        }

        public void BroadCastMsg(string msg, int sid)
        {
            foreach (var pair in SessionMap)
            {
                if (pair.Key != sid)
                {
                    pair.Value.Send(msg);
                }
            }
        }
    }
}
