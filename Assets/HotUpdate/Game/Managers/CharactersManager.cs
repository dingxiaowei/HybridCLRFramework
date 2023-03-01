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
        private const string PenguinPrefab = "Assets/Demo/ActDemo/Prefabs/Penguin.prefab";
        AssetRequest mainPlayerRequest;
        List<AssetRequest> othersPlayerRequests = new List<AssetRequest>();
        Vector3 mainPlayerBornVector = new Vector3(3.12f, 4.17f, 17.71f);
        public Action<Character> OnMainPlayerLoadedEvent;
        Character mainChar;
        public override void Start()
        {
            OnMainPlayerLoadedEvent -= OnMainPlayerLoaded;
            OnMainPlayerLoadedEvent += OnMainPlayerLoaded;
            //LoadMainPlayer();
        }

        public void LoadOtherPlayer(Protoc.CUserStateInfo userStateInfo)
        {
            if (userStateInfo == null)
            {
                return;
            }
            var request = Assets.LoadAssetAsync(PenguinPrefab, typeof(GameObject), (rq) =>
            {
                var go = GameObject.Instantiate(rq.asset) as GameObject;
                if (go != null)
                {
                    var pos = userStateInfo.Pos.ToVector3();
                    var dir = userStateInfo.Rotate.ToVector3();
                    var userInfo = userStateInfo.UserInfo;
                    go.transform.localPosition = pos;
                    go.transform.localRotation = Quaternion.Euler(dir.x, dir.y, dir.z);
                    go.name = userInfo.UserName;
                }
            });
            othersPlayerRequests.Add(request);
        }

        //加载主角
        public void LoadMainPlayer(Protoc.CUserStateInfo userStateInfo = null, Action<int> OnPlaerLoaded = null)
        {
            mainPlayerRequest = Assets.LoadAssetAsync(PenguinPrefab, typeof(GameObject), (rq) =>
            {
                var go = GameObject.Instantiate(rq.asset) as GameObject;
                if (go != null)
                {
                    go.transform.localScale = Vector3.one;
                    if (userStateInfo == null)
                    {
                        go.name = rq.asset.name;
                        go.transform.localPosition = mainPlayerBornVector;
                        go.transform.localRotation = Quaternion.identity;
                    }
                    else
                    {
                        var pos = userStateInfo.Pos.ToVector3();
                        var dir = userStateInfo.Rotate.ToVector3();
                        var userInfo = userStateInfo.UserInfo;
                        go.transform.localPosition = pos;
                        go.transform.localRotation = Quaternion.Euler(dir.x, dir.y, dir.z);
                        go.name = userInfo.UserName;
                    }
                }
                ActDemoLoader.Instance.CameraFollow.target = go.transform;

                var characterController = go.transform.GetComponent<Character>();
                OnMainPlayerLoadedEvent?.Invoke(characterController);
                OnPlaerLoaded?.Invoke(userStateInfo.UserInfo.UserId);
            });
        }

        void OnMainPlayerLoaded(Character character)
        {
            mainChar = character;
            //如果pico的事件改变  则改变输入的值
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
                //msg.Voice = VoiceManager.Instance.AudioClipByteString;
                NetworkManager.Instance.SendMsg<Protoc.BroadCastVoice>(msg);
                NetworkManager.Instance.SendMsg((int)Protoc.OuterOpcode.BroadCastVoice, msg);
            };
        }

        void On2DAxisValueChange(Vector2 value)
        {
            if (mainChar)
            {
                (mainChar.CharacterInput as CharacterInputs).keyCodeCharacterInput.FirstHorizontal = value.x;
                (mainChar.CharacterInput as CharacterInputs).keyCodeCharacterInput.FristVertical = value.y;
            }
        }

        void On2DAxisValueStop()
        {
            if (mainChar)
            {
                (mainChar.CharacterInput as CharacterInputs).keyCodeCharacterInput.FirstHorizontal = 0;
                (mainChar.CharacterInput as CharacterInputs).keyCodeCharacterInput.FristVertical = 0;
            }
        }

        void OnJump()
        {
            if (mainChar)
            {
                (mainChar.CharacterInput as CharacterInputs).keyCodeCharacterInput.FirstJump = true;
            }
        }

        void OnJumpReset()
        {
            if (mainChar)
            {
                (mainChar.CharacterInput as CharacterInputs).keyCodeCharacterInput.FirstJump = false;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            OnMainPlayerLoadedEvent -= OnMainPlayerLoaded;
            JoyStickMove.Instance.onMoving -= On2DAxisValueChange;
            XRInputManager.Instance.OnRightPrimary2DAxisValueEvent -= On2DAxisValueChange;
            JoyStickMove.Instance.onMoveEnd -= On2DAxisValueStop;
        }
    }
}
