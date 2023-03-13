using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using libx;

namespace ActDemo
{
    public class OtherPlayer : PlayerBase
    {
        public const string PrefabPath = "Assets/Demo/ACTDemo/Prefabs/Penguin1.prefab";
        private CharacterMoveSyncController charMoveSyncController;
        public CharacterMoveSyncController CharMoveSyncController { get { return charMoveSyncController; } }
        public OtherPlayer(CharacterMoveSyncController charMoveSync, int uid, GameObject obj, AssetRequest request) : base(uid, obj, request)
        {
            this.charMoveSyncController = charMoveSync;
        }
    }
}
