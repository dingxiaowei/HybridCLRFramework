using libx;
using UnityEngine;

public class PlayerBase
{
    private int uid;
    public int Uid { get { return uid; } }
    protected GameObject PlayerRoot;
    protected AssetRequest AssetRequest;

    public PlayerBase(int uid, GameObject obj, AssetRequest request)
    {
        this.uid = uid;
        this.PlayerRoot = obj;
        this.AssetRequest = request;
    }

    public void SetId(int uid)
    {
        this.uid = uid;
        PlayerRoot.name = uid.ToString();
    }

    public void Destroy()
    {
        GameObject.Destroy(PlayerRoot);
        this.AssetRequest?.Release();
    }
}
