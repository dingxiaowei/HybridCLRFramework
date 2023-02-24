using Google.Protobuf;
using Protoc;

[MessageHandler]
public class BroadCastVoiceHandler : AMHandler<BroadCastVoice>
{
    protected override void Run(ByteString content)
    {
        UnityEngine.Debug.Log("�յ������������㲥��Ϣ");
        var voice = BroadCastVoice.Parser.ParseFrom(content);
        ActDemo.VoiceManager.Instance.PlayRecord(voice.Voice);
    }
}
