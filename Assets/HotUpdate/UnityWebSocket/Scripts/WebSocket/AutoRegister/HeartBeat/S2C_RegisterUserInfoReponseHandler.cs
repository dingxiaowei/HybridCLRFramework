using Google.Protobuf;
using Protoc;

[MessageHandler]
public class S2C_RegisterUserInfoResponseHandler : AMHandler<S2C_RegisterUserInfoResponse>
{
    protected override void Run(ByteString content)
    {
        UnityEngine.Debug.Log("�յ�������������ϢS2C_RegisterUserInfoResponse");
        var registerUserInfoResponse = S2C_RegisterUserInfoResponse.Parser.ParseFrom(content);
        if (registerUserInfoResponse.Error != 0)
        {
            UnityEngine.Debug.LogError("S2C_RegisterUserInfoResponse������Ϣ�쳣");
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