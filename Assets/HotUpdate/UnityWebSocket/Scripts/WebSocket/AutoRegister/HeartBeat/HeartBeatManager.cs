using System;
using Protoc.Managers;
using UnityEngine;

namespace Protoc.AutoRegister.HeartBeat
{
    public class HeartBeatManager : Singleton<HeartBeatManager>
    {
        /// <summary>
        /// 心跳最大间隔时长
        /// </summary>
        private float HEART_BEAT_INTERVAL_TIME = 6;

        private long serverTimestamp;

        /// <summary>
        /// 服务器时间戳
        /// </summary>
        public long ServerTimestamp
        {
            get => serverTimestamp;
            set => serverTimestamp = value;
        }

        private DateTime serverDateTime;

        /// <summary>
        /// 服务器当前时间
        /// </summary>
        public DateTime ServerDateTime
        {
            get => ToLocalDateTime(serverTimestamp);
            set => serverDateTime = value;
        }

        /// <summary>
        /// 时间戳转换DateTime
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public DateTime ToLocalDateTime(long timeStamp)
        {
            long timeStampSecond = timeStamp / 1000 + GetZoneOffTimeSeconds();
            var serverTime = new DateTime(621355968000000000 + (long)timeStampSecond * (long)10000000, DateTimeKind.Utc);
            return serverTime;
        }

        /// <summary>
        /// 获取时区偏移总秒数
        /// </summary>
        /// <returns></returns>
        private long GetZoneOffTimeSeconds()
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;
            return (long)timeZoneInfo.BaseUtcOffset.TotalSeconds;
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public long GetClientTimestamp()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }

        /// <summary>
        /// 发送心跳请求
        /// </summary>
        /// <param name="socketSession"></param>
        public void SendHeartBeatRequest()
        {
            C2S_HeartBeatRequest heartBeatRequest = new C2S_HeartBeatRequest();
            heartBeatRequest.ClientTimestamp = GetClientTimestamp(); //客户端上发的时间戳为了服务器返回，应该是用的服务器自己的时间戳
            NetManager.sInstance.SocketSession?.SendAsync((int)OuterOpcode.S2C_HeartBeatResponse, heartBeatRequest);
            CheckNetCanReConnect();
        }

        /// <summary>
        /// 检查心跳是否触发断线重连
        /// </summary>
        private void CheckNetCanReConnect()
        {
            //网络连接正常
            if (NetManager.sInstance.SocketSession.IsConnected)
                return;

            //心跳超时
            if ((GetClientTimestamp() - ServerTimestamp) >= HEART_BEAT_INTERVAL_TIME)
            {
                if (NetManager.sInstance.SocketSession.IsConnected)
                {
                    NetManager.sInstance.ReConnectServer();
#if DEBUG_NETWORK
                    Debug.Log("客户断线重连");
#endif
                }
                else
                {
                    NetManager.sInstance.ConnectServer();
#if DEBUG_NETWORK
                    Debug.Log("客户断线网络重建");
#endif
                }
            }
            else
            {
#if DEBUG_NETWORK
                Debug.Log("客户端和服务器心跳未超时");
#endif
            }
        }
    }
}