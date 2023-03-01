using Google.Protobuf;
using Protoc;

[MessageHandler]
public class S2C_UserStateInfosResponseHandler : AMHandler<S2C_UserStateInfosResponse>
{
    protected override void Run(ByteString content)
    {
        UnityEngine.Debug.Log("收到服务器返回消息S2C_UserStateInfosResponse");
        var userStateInfosResponse = S2C_UserStateInfosResponse.Parser.ParseFrom(content);
        if (userStateInfosResponse.Error != 0)
        {
            UnityEngine.Debug.LogError("S2C_UserStateInfosResponse返回消息异常");
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