using Google.Protobuf;
using Protoc;

[MessageHandler]
public class S2C_MoveSyncHandler : AMHandler<CMoveData>
{
    protected override void Run(ByteString content)
    {
        var s2c_MoveMsg = CMoveData.Parser.ParseFrom(content);
        SystemEventManager.Instance.RaiseEvent(new CharMoveEvent()
        {
            EventType = EventType.ECharMove,
            MoveData = s2c_MoveMsg
        });
    }
}