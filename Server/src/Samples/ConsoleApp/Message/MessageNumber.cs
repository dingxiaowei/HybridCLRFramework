using System;
using System.Collections.Generic;
using System.Text;

namespace ServerDemo
{
    public class MessageNumber
    {
        public const int C2S_HeartBeatRequest = 1;
        public const int S2C_HeartBeatResponse = 2;

        public const int C2S_EnterMapRequest = 101;
        public const int S2C_EnterMapResponse = 102;
        public const int BroadCastVoice = 103;

        public const int C2S_RegisterUserInfoResquest = 110;
        public const int S2C_RegisterUserInfoResponse = 111;
        public const int C2S_UserStateInfosRequest = 112;
        public const int S2C_UserStateInfosResponse = 113;
        public const int S2C_UserLeave = 114;
        public const int CMoveDataMsg = 115;


        public const int CMD_JOIN_ROOM = 1000;
        public const int CMD_EXIT_ROOM = 1001;
        public const int MSG_ROOM_INFO = 1002;
        public const int MSG_START_GAME = 1003;
        public const int CMD_FSP_MESSAGE = 1004;
        public const int MSG_FRAME_MESSAGE = 1005;
    }
}
