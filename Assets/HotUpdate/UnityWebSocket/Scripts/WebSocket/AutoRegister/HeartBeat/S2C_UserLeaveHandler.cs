using Google.Protobuf;
using Protoc;

[MessageHandler]
public class S2C_UserLeaveHandler : AMHandler<S2C_UserLeave>
{
    protected override void Run(ByteString content)
    {
        UnityEngine.Debug.Log("�յ�������������ϢUserLeave");
        var s2c_UserLeave = S2C_UserLeave.Parser.ParseFrom(content);
        SystemEventManager.Instance.RaiseEvent(new UserLeaveEvent()
        {
            EventType = EventType.EUserLeave,
            Uid = s2c_UserLeave.UserId
        });
    }
}