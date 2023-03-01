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
            //���ע����Ϣ�ص�
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
                Debug.LogError("UserRegisterEvent��UserStateInfo������");
            }
        }
        void OnUserStateInfos(SystemEventBase eventArg)
        {
            Debug.Log("�յ�����������ɫ����Ϣ");
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

        //�������ʧ�������ǿ��ע��
        public void CreateOrInitMainChar(bool force = false)
        {
            if (force)
            {
                InitUser();
                return;
            }
            //�鿴����id��name�����û���򴴽�����������ȡ�б����������������ȡ�б�����ʧ��������ע��
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