using Google.Protobuf;
using Protoc;

[MessageHandler]
public class BroadCastVoiceHandler : AMHandler<BroadCastVoice>
{
    protected override void Run(ByteString content)
    {
        UnityEngine.Debug.Log("收到服务器语音广播消息");
        var voice = BroadCastVoice.Parser.ParseFrom(content);
        ActDemo.VoiceManager.Instance.PlayRecord(voice.Voice);
    }
}
