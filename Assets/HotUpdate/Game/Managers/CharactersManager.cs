using Dypsloom.DypThePenguin.Scripts.Character;
using Dypsloom.DypThePenguin.Scripts.Items;
using libx;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace ActDemo
{
    public class CharactersManager : ManagerBase<CharactersManager>
    {
        public Action<Character> OnMainPlayerLoadedEvent;
        //Dictionary<int, PlayerBase> PlayersMap = new Dictionary<int, PlayerBase>();
        Dictionary<int, OtherPlayer> OtherPlayersMap = new Dictionary<int, OtherPlayer>();
        public int MainCharUid
        {
            get
            {
                return mainPlayer.Uid;
            }
        }
        MyPlayer mainPlayer { get; set; }

        public override void Start()
        {
            OnMainPlayerLoadedEvent -= OnMainPlayerLoaded;
            OnMainPlayerLoadedEvent += OnMainPlayerLoaded;
            //LoadMainPlayer();
            RegisterEvents();
        }

        void OnCharMove(SystemEventBase eventArg)
        {
            var arg = eventArg as CharMoveEvent;
            var uid = arg.MoveData.UserId;
            if (OtherPlayersMap.ContainsKey(uid))
            {
                OtherPlayersMap[uid].CharMoveSyncController.EnqueueMoveData(arg.MoveData);
            }
            else
            {
                Debug.LogError($"��ǰλ��ͬ����Ϣȱ�ٶ�Ӧ{uid}���");
            }
        }

        void RegisterEvents()
        {
            SystemEventManager.Instance.RegisterEvent(EventType.EUserLeave, OnUserLeave);
            SystemEventManager.Instance.RegisterEvent(EventType.ECharMove, OnCharMove);
        }

        void UnRegisterEvents()
        {
            SystemEventManager.Instance.UnRegisterEvent(EventType.EUserLeave, OnUserLeave);
            SystemEventManager.Instance.UnRegisterEvent(EventType.ECharMove, OnCharMove);
        }

        void OnUserLeave(SystemEventBase eventArg)
        {
            var arg = eventArg as UserLeaveEvent;
            int uid = arg.Uid;
            RemovePlayer(uid);
            Debug.Log($"���:{uid}�뿪");
        }

        //TODO:��Ҫ��ɫ������ɫ���������Ի���ģ�ͣ�����Request
        public void LoadOtherPlayer(Protoc.CUserStateInfo userStateInfo)
        {
            var tempUserStateInfo = userStateInfo;
            if (tempUserStateInfo == null)
            {
                return;
            }
            var request = Assets.LoadAsset(OtherPlayer.PrefabPath, typeof(GameObject));
            var go = GameObject.Instantiate(request.asset) as GameObject;
            if (go != null)
            {
                var pos = tempUserStateInfo.Pos.ToVector3();
                var dir = tempUserStateInfo.Rotate.ToVector3();
                var userInfo = tempUserStateInfo.UserInfo;
                go.transform.localPosition = pos;
                go.transform.localRotation = Quaternion.Euler(dir.x, dir.y, dir.z);
                go.name = userInfo.UserId.ToString();

                var charMoveSyncController = go.AddComponent<CharacterMoveSyncController>();

                int uid = tempUserStateInfo.UserInfo.UserId;
                var otherPlayer = new OtherPlayer(charMoveSyncController, uid, go, request);
                //PlayersMap.Add(uid, otherPlayer);
                OtherPlayersMap.Add(uid, otherPlayer);
            }
            //TODO:�첽��������ֻ���س�����һ��
            //var request = Assets.LoadAssetAsync(OtherPenguinPrefab, typeof(GameObject), (rq) =>
            //{
            //    var go = GameObject.Instantiate(rq.asset) as GameObject;
            //    if (go != null)
            //    {
            //        var pos = tempUserStateInfo.Pos.ToVector3();
            //        var dir = tempUserStateInfo.Rotate.ToVector3();
            //        var userInfo = tempUserStateInfo.UserInfo;
            //        go.transform.localPosition = pos;
            //        go.transform.localRotation = Quaternion.Euler(dir.x, dir.y, dir.z);
            //        go.name = userInfo.UserName;
            //        Debug.LogError("���ؽ�ɫ:" + userInfo.UserName);
            //    }
            //});
        }

        //��������
        public void LoadMainPlayer(Protoc.CUserStateInfo userStateInfo = null, Action<int> OnPlaerLoaded = null)
        {
            int uid = 0;
            var request = Assets.LoadAsset(MyPlayer.PrefabPath, typeof(GameObject));
            var go = GameObject.Instantiate(request.asset) as GameObject;
            if (go != null)
            {
                go.transform.localScale = Vector3.one;
                if (userStateInfo == null)
                {
                    go.name = request.asset.name;
                    go.transform.localPosition = MyPlayer.MainPlayerBornVector;
                    go.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    uid = userStateInfo.UserInfo.UserId;
                    var pos = userStateInfo.Pos.ToVector3();
                    var dir = userStateInfo.Rotate.ToVector3();
                    var userInfo = userStateInfo.UserInfo;
                    go.transform.localPosition = pos;
                    go.transform.localRotation = Quaternion.Euler(dir.x, dir.y, dir.z);
                    go.name = userInfo.UserId.ToString();
                }
                var charMoveController = go.AddComponent<CharacterNetMoveController>();
                var characterController = go.transform.GetComponent<Character>();
                ActDemoLoader.Instance.CameraFollow.target = go.transform;
                var myPlayer = new MyPlayer(characterController, charMoveController, uid, go, request);
                mainPlayer = myPlayer;
                //PlayersMap.Add(uid, myPlayer);
                OnMainPlayerLoadedEvent?.Invoke(characterController);
                OnPlaerLoaded?.Invoke(userStateInfo.UserInfo.UserId);
                mainPlayer.SetId(userStateInfo.UserInfo.UserId);
            }
        }

        void OnMainPlayerLoaded(Character character)
        {
            //���pico���¼��ı�  ��ı������ֵ
            XRInputManager.Instance.OnRightPrimary2DAxisValueEvent += On2DAxisValueChange;
            JoyStickMove.Instance.onMoving += On2DAxisValueChange;
            JoyStickMove.Instance.onMoveEnd += On2DAxisValueStop;
            XRInputManager.Instance.OnAButtonDown += OnJump;
            XRInputManager.Instance.OnAButtonUp += OnJumpReset;

            XRInputManager.Instance.OnBButtonDown += () => { VoiceManager.Instance.BeginRecord(); };
            XRInputManager.Instance.OnBButtonUp += () =>
            {
                VoiceManager.Instance.StopRecord();
                //VoiceManager.Instance.PlayRecord();
                var msg = new Protoc.BroadCastVoice();
                msg.Voice = VoiceManager.Instance.AudioClipByteString;
                //NetworkManager.Instance.SendMsg<Protoc.BroadCastVoice>(msg);
                NetworkManager.Instance.SendMsg((int)Protoc.OuterOpcode.BroadCastVoice, msg);
            };
        }

        void On2DAxisValueChange(Vector2 value)
        {
            if (mainPlayer.Character)
            {
                (mainPlayer.Character.CharacterInput as CharacterInputs).keyCodeCharacterInput.FirstHorizontal = value.x;
                (mainPlayer.Character.CharacterInput as CharacterInputs).keyCodeCharacterInput.FristVertical = value.y;
            }
        }

        void On2DAxisValueStop()
        {
            if (mainPlayer.Character)
            {
                (mainPlayer.Character.CharacterInput as CharacterInputs).keyCodeCharacterInput.FirstHorizontal = 0;
                (mainPlayer.Character.CharacterInput as CharacterInputs).keyCodeCharacterInput.FristVertical = 0;
            }
        }

        void OnJump()
        {
            if (mainPlayer.Character)
            {
                (mainPlayer.Character.CharacterInput as CharacterInputs).keyCodeCharacterInput.FirstJump = true;
            }
        }

        void OnJumpReset()
        {
            if (mainPlayer.Character)
            {
                (mainPlayer.Character.CharacterInput as CharacterInputs).keyCodeCharacterInput.FirstJump = false;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            OnMainPlayerLoadedEvent -= OnMainPlayerLoaded;
            JoyStickMove.Instance.onMoving -= On2DAxisValueChange;
            XRInputManager.Instance.OnRightPrimary2DAxisValueEvent -= On2DAxisValueChange;
            JoyStickMove.Instance.onMoveEnd -= On2DAxisValueStop;
            UnRegisterEvents();
        }

        void RemovePlayer(int uid)
        {
            //if (!PlayersMap.ContainsKey(uid))
            //{
            //    Debug.LogError($"��ǰ��ɫ uid:{uid}�����ڣ�û���Ƴ�");
            //    return;
            //}
            //var player = PlayersMap[uid];
            //player.Destroy();
            //if (PlayersMap.ContainsKey(uid))
            //    PlayersMap.Remove(uid);
            if (uid == mainPlayer.Uid)
            {
                Debug.LogError("�����Ƴ��Լ��Ľ�ɫ");
                return;
            }
            if (OtherPlayersMap.ContainsKey(uid))
                OtherPlayersMap.Remove(uid);
            else
                Debug.LogError($"û�е�ǰҪ�Ƴ��Ľ�ɫ:{uid}");
        }
    }
}
