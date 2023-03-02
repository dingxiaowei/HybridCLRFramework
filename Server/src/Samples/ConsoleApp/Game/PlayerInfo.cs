using Protoc;

public class PlayerInfo
{
    public CUserStateInfo UserStateInfo { get; set; }
    public PlayerInfo(int uid)
    {
        UserStateInfo = new CUserStateInfo();
        UserStateInfo.Pos = new Vec3Data() { X = 3.12f, Y = 4.17f, Z = 17.71f };
        UserStateInfo.Rotate = new Vec3Data();
        UserStateInfo.UserInfo = new CUserInfo()
        {
            UserId = uid,
            UserName = ""
        };
    }

    public void SetName(string name)
    {
        UserStateInfo.UserInfo.UserName = name;
    }
}

