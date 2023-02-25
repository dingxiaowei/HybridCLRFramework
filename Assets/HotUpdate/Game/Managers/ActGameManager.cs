using libx;
using Protoc;
using System;
using UnityEngine;

namespace ActDemo
{
    public class ActGameManager : ManagerBase<ActGameManager>
    {
        public override void Start()
        {
            base.Start();
            RegisterMsg();
            CreateOrInitMainChar();
        }

        void RegisterMsg()
        {
            //SystemEventManager.Instance.RegisterEvent(EventType.EUserRegister,)
        }

        public void CreateOrInitMainChar(bool force = false)
        {
            if (force)
            {
                InitUser();
                return;
            }
            //查看本地id和name，如果没有则创建，如果有则获取列表请求，如果服务器获取列表请求失败则重新注册
            var id = PlayerPrefs.GetInt("UserId", 0);
            var name = PlayerPrefs.GetString("UserName", "");
            if (id == 0 && string.IsNullOrEmpty(name))
            {
                InitUser();
            }
            else
            {
                RegisterUserInfoRequest(id, name);
            }
        }

        void InitUser()
        {
            var name = $"{SystemInfo.deviceUniqueIdentifier}_{System.DateTime.Now.ToLongDateString()}";
            RegisterUserInfoRequest(0, name);
        }

        void RegisterUserInfoRequest(int uid, string name)
        {
            CUserInfo userInfo = new CUserInfo()
            {
                UserId = uid,
                UserName = name
            };
            var msg = new C2S_RegisterUserInfoRequest();
            msg.UserInfo = userInfo;
            NetworkManager.Instance.SendMsg((int)OuterOpcode.C2S_RegisterUserInfoRequest, msg);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}