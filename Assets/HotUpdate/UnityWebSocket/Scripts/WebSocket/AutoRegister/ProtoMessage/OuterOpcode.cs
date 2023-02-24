using Protoc.AutoRegister.HeartBeat;

namespace Protoc
{
    public static partial class OuterOpcode
    {
        public const int C2S_HeartBeatRequest = 1;
        public const int S2C_HeartBeatResponse = 2;

        public const int C2S_EnterMapRequest = 101;
        public const int S2C_EnterMapResponse = 102;
        public const int BroadCastVoice = 103;
    }

    [Message(OuterOpcode.BroadCastVoice)]
    public partial class BroadCastVoice
    {

    }


    [Message(OuterOpcode.C2S_HeartBeatRequest)]
    public partial class C2S_HeartBeatRequest
    {
    }

    [Message(OuterOpcode.S2C_HeartBeatResponse)]  //TODO:这里编号可以写proto里面的id编号
    public partial class S2C_HeartBeatResponse
    {
        public void Debug()
        {
            UnityEngine.Debug.Log($"<color=yellow>服务器心跳回复:{HeartBeatManager.sInstance.ToLocalDateTime(this.ClientTimestamp * 1000).ToString("yyyy-MM-dd HH:mm:ss")}</color>");
        }
    }

    [Message(OuterOpcode.C2S_EnterMapRequest)]
    public partial class C2S_EnterMap
    {
    }

    [Message(OuterOpcode.S2C_EnterMapResponse)]  //TODO:这里编号可以写proto里面的id编号
    public partial class S2C_EnterMap
    {
        public void Debug()
        {
            UnityEngine.Debug.Log($"Message:{this.Message}  UnitId:{this.UnitId}");
            foreach (var unit in this.Units)
            {
                UnityEngine.Debug.Log($"ID:{unit.UnitId} X:{unit.X} Y:{unit.Y} Z:{unit.Z}");
            }
        }
    }
}