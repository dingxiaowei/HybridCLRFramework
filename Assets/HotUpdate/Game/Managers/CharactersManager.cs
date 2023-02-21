using Dypsloom.DypThePenguin.Scripts.Character;
using Dypsloom.DypThePenguin.Scripts.Items;
using libx;
using System;
using UnityEngine;

namespace ActDemo
{
    public class CharactersManager : ManagerBase<CharactersManager>
    {
        private const string PenguinPrefab = "Assets/Demo/ActDemo/Prefabs/Penguin.prefab";
        AssetRequest _mainPlayerRequest;
        Vector3 mainPlayerBornVector = new Vector3(3.12f, 4.17f, 17.71f);
        public Action<Character> OnMainPlayerLoadedEvent;
        Character mainChar;
        public override void Start()
        {
            OnMainPlayerLoadedEvent -= OnMainPlayerLoaded;
            OnMainPlayerLoadedEvent += OnMainPlayerLoaded;
            LoadMainPlayer();
        }

        void LoadMainPlayer()
        {
            _mainPlayerRequest = Assets.LoadAssetAsync(PenguinPrefab, typeof(GameObject), (rq) =>
            {
                var go = GameObject.Instantiate(rq.asset) as GameObject;
                if (go != null)
                {
                    go.name = rq.asset.name;
                    go.transform.localPosition = mainPlayerBornVector;
                    go.transform.localScale = Vector3.one;
                    go.transform.localRotation = Quaternion.identity;
                }
                ActDemoLoader.Instance.CameraFollow.target = go.transform;

                var characterController = go.transform.GetComponent<Character>();
                OnMainPlayerLoadedEvent?.Invoke(characterController);
            });
        }

        void OnMainPlayerLoaded(Character character)
        {
            mainChar = character;
            //如果pico的事件改变  则改变输入的值
            XRInputManager.Instance.OnRightPrimary2DAxisValueEvent += OnRight2DAxisValueChange;
            XRInputManager.Instance.OnAButtonDown += OnJump;
            XRInputManager.Instance.OnAButtonUp += OnJumpReset;
        }

        void OnRight2DAxisValueChange(Vector2 value)
        {
            if (mainChar)
            {
                (mainChar.CharacterInput as CharacterInputs).keyCodeCharacterInput.FirstHorizontal = value.x;
                (mainChar.CharacterInput as CharacterInputs).keyCodeCharacterInput.FristVertical = value.y;
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
        }
    }
}
