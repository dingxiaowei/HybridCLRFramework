using Google.Protobuf;
using Protoc;

[MessageHandler]
public class S2C_RegisterUserInfoResponseHandler : AMHandler<S2C_RegisterUserInfoResponse>
{
    protected override void Run(ByteString content)
    {
        UnityEngine.Debug.Log("收到服务器返回消息S2C_RegisterUserInfoResponse");
        var registerUserInfoResponse = S2C_RegisterUserInfoResponse.Parser.ParseFrom(content);
        if (registerUserInfoResponse.Error != 0)
        {
            UnityEngine.Debug.LogError("S2C_RegisterUserInfoResponse返回消息异常");
        }
        else
        {
            SystemEventManager.Instance.RaiseEvent(EventType.EUserRegister, new UserRegisterEvent()
            {
                UserStateInfo = registerUserInfoResponse.UserStateInfo
            });
        };
    }
}