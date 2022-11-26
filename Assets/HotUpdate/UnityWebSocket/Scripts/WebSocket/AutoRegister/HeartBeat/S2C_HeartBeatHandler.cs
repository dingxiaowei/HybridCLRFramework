using Google.Protobuf;


namespace Protoc.AutoRegister.HeartBeat
{
    [MessageHandler]
    public class S2C_HeartBeatHandler:AMHandler<S2C_HeartBeatResponse>
    {
        protected override void Run(ByteString content)
        {
        #if DEBUG_NETWORK
            UnityEngine.Debug.Log($"收到服务器心跳回复协议:S2C_HeartBeatResponse");
        #endif
            var heartResponse = S2C_HeartBeatResponse.Parser.ParseFrom(content);
            heartResponse.Debug();
            HeartBeatManager.sInstance.ServerTimestamp = heartResponse.ClientTimestamp;
        }
    }
}