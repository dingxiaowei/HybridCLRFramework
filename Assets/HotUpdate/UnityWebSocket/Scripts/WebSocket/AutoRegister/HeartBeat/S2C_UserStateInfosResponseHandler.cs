using Google.Protobuf;
using Protoc;

[MessageHandler]
public class S2C_UserStateInfosResponseHandler : AMHandler<S2C_UserStateInfosResponse>
{
    protected override void Run(ByteString content)
    {
        UnityEngine.Debug.Log("�յ�������������ϢS2C_UserStateInfosResponse");
        var userStateInfosResponse = S2C_UserStateInfosResponse.Parser.ParseFrom(content);
        if (userStateInfosResponse.Error != 0)
        {
            UnityEngine.Debug.LogError("S2C_UserStateInfosResponse������Ϣ�쳣");
        }
        else
        {
            var userStateInfosEvent = new UserStateInfosEvent();
            userStateInfosEvent.EventType = EventType.EUserStateInfos;
            userStateInfosEvent.UserStateInfos.AddRange(userStateInfosResponse.UserStateInfos);
            SystemEventManager.Instance.RaiseEvent(userStateInfosEvent);
        }
    }
}