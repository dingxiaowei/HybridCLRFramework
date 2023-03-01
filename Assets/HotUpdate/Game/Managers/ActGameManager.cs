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
        }

        void RegisterMsg()
        {
            //玩家注册消息回调
            SystemEventManager.Instance.RegisterEvent(EventType.EUserRegister, OnUserRegist);
            SystemEventManager.Instance.RegisterEvent(EventType.EUserStateInfos, OnUserStateInfos);
            SystemEventManager.Instance.RegisterEvent(EventType.EForceRegisterUser, ForceRegisterUser);
            SystemEventManager.Instance.RegisterEvent(EventType.ESocketConnectState, OnConnectState);
        }

        void OnUserRegist(SystemEventBase eventArg)
        {
            var arg = eventArg as UserRegisterEvent;
            var userStateInfo = arg.UserStateInfo;
            if (userStateInfo != null)
            {
                SaveUser(userStateInfo.UserInfo);
                CharactersManager.Instance.LoadMainPlayer(userStateInfo, (uid) =>
                 {
                     NetworkManager.Instance.SendMsg((int)OuterOpcode.C2S_UserStateInfosRequest, new C2S_UserStateInfosRequest()
                     {
                         MyUserId = uid
                     });
                 });
            }
            else
            {
                Debug.LogError("UserRegisterEvent中UserStateInfo不存在");
            }
        }
        void OnUserStateInfos(SystemEventBase eventArg)
        {
            Debug.Log("收到加载其他角色的消息");
            var arg = eventArg as UserStateInfosEvent;
            var userInfos = arg.UserStateInfos;
            foreach (var userInfo in userInfos)
            {
                CharactersManager.Instance.LoadOtherPlayer(userInfo);
            }
        }

        void ForceRegisterUser(SystemEventBase eventArg)
        {
            //var arg = eventArg as ForceRegisterUserEvent;
            CreateOrInitMainChar(true);
        }

        void OnConnectState(SystemEventBase eventArg)
        {
            var arg = eventArg as ConnectStateEvent;
            if (arg.ConnectState)
            {
                CreateOrInitMainChar();
            }
        }

        void SaveUser(CUserInfo userInfo)
        {
            PlayerPrefs.SetInt("UserId", userInfo.UserId);
            PlayerPrefs.SetString("UserName", userInfo.UserName);
            PlayerPrefs.Save();
        }

        //如果返回失败则进行强制注册
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
            SystemEventManager.Instance.UnRegisterEvent(EventType.EUserRegister, OnUserRegist);
            SystemEventManager.Instance.UnRegisterEvent(EventType.EUserStateInfos, OnUserStateInfos);
            SystemEventManager.Instance.UnRegisterEvent(EventType.EForceRegisterUser, ForceRegisterUser);
            SystemEventManager.Instance.UnRegisterEvent(EventType.ESocketConnectState, OnConnectState);
        }
    }
}