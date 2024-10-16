using ActDemo;
using Protoc;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


//ActDemo入口脚本
//模块初始化
public class ActDemoRoot : MonoBehaviour
{
    private List<IMgr> mMgrList;

    void Start()
    {
        RegisterAllMgr();
        foreach (var mgr in mMgrList)
        {
            mgr.Start();
        }
        MessageDispatcher.sInstance.AutoRegistHandlers();
    }

    private void RegisterAllMgr()
    {
        mMgrList = new List<IMgr>();

        mMgrList.Add(SystemEventManager.Instance);
        mMgrList.Add(ActGameManager.Instance);
        mMgrList.Add(CharactersManager.Instance);
        mMgrList.Add(VoiceManager.Instance);
        mMgrList.Add(NetworkManager.Instance);
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 50), "连接Socket"))
        {
            NetworkManager.Instance.Connect();
        }
        if (GUI.Button(new Rect(10, 100, 150, 50), "连发100消息"))
        {
            for (int i = 0; i < 100; i++)
            {
                S2C_EnterMap msg = new S2C_EnterMap();
                msg.Message = "服务器向客户端发送的进入地图消息";
                msg.RpcId = 1;
                msg.UnitId = i;
                msg.Error = 0;
                List<Protoc.UnitInfo> unitInfos = new List<UnitInfo>();
                unitInfos.Add(new UnitInfo() { UnitId = 222, X = 1, Y = 1, Z = 1 });
                msg.Units.AddRange(unitInfos);
                //socketSession.SendAsync((int)OuterOpcode.S2C_EnterMapResponse, msg);
                NetworkManager.Instance.SendMsg((int)OuterOpcode.S2C_EnterMapResponse, msg);
                Debug.Log("-----------客户端向服务器发送消息");
                msg.Debug();
            }
        }
        if (GUI.Button(new Rect(10, 190, 150, 50), "发送消息"))
        {
            S2C_EnterMap msg = new S2C_EnterMap();
            msg.Message = "服务器向客户端发送的进入地图消息";
            msg.RpcId = 1;
            msg.UnitId = 0;
            msg.Error = 0;
            List<Protoc.UnitInfo> unitInfos = new List<UnitInfo>();
            unitInfos.Add(new UnitInfo() { UnitId = 222, X = 1, Y = 1, Z = 1 });
            msg.Units.AddRange(unitInfos);
            msg.Voice = VoiceManager.Instance.AudioClipByteString;
            NetworkManager.Instance.SendMsg((int)OuterOpcode.S2C_EnterMapResponse, msg);
        }
        if (GUI.Button(new Rect(210, 10, 150, 50), "开始录音"))
        {
            VoiceManager.Instance.BeginRecord();
        }
        if (GUI.Button(new Rect(210, 100, 150, 50), "停止录音"))
        {
            VoiceManager.Instance.StopRecord();
        }
        if (GUI.Button(new Rect(210, 190, 150, 50), "播放录音"))
        {
            VoiceManager.Instance.PlayRecord();
        }
        if (GUI.Button(new Rect(210, 270, 150, 50), "发送录音"))
        {
            var msg = new BroadCastVoice();
            msg.Voice = VoiceManager.Instance.AudioClipByteString;
            //NetworkManager.Instance.SendMsg<BroadCastVoice>(msg);
            NetworkManager.Instance.SendMsg((int)OuterOpcode.BroadCastVoice, msg);
        }
    }

    private void Update()
    {
        foreach (var mgr in mMgrList)
        {
            mgr.Update();
        }
    }

    private void LateUpdate()
    {
        foreach (var mgr in mMgrList)
        {
            mgr.LateUpdate();
        }
    }

    private void FixedUpdate()
    {
        foreach (var mgr in mMgrList)
        {
            mgr.FixedUpdate();
        }
    }

    private void OnDestroy()
    {
        MessageDispatcher.sInstance.Dispose();
        foreach (var mgr in mMgrList)
        {
            mgr.OnDestroy();
        }
    }
}
