using Dypsloom.DypThePenguin.Scripts.Character;
using Protoc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActDemo
{
    enum EMoveActionType
    {
        Idle = 0,
        Move = 1,
        Jump = 2
    }

    public class CharacterNetMoveController : MonoBehaviour
    {
        private Character _Character;
        private Vector3 _lastPos;
        private Vector3 _lastRot;
        private bool isMove = false;
        private bool isJump = false;
        private float deltaTime = 0f;
        private EMoveActionType currentAction = EMoveActionType.Idle;
        private float jumpStartTimestap = 0f;
        private void Awake()
        {
            _Character = GetComponent<Character>();
        }
        void Start()
        {

        }

        void Update()
        {
            if (isMove)
            {
                deltaTime += Time.deltaTime;
                if (deltaTime > GameConfig.SyncTime)
                {
                    MoveMsg(deltaTime);
                    deltaTime = 0;
                }
                currentAction = EMoveActionType.Move;
            }
            else if (isJump)
            {
                deltaTime += Time.deltaTime;
                if(deltaTime > GameConfig.SyncTime)
                {
                    MoveMsg(deltaTime, EMoveActionType.Jump);
                    deltaTime = 0;
                }
                currentAction = EMoveActionType.Jump;
            }
        }

        void MoveMsg(float deltaTime, EMoveActionType actionType = EMoveActionType.Move)
        {
            CMoveData moveData = new CMoveData();
            moveData.UserId = CharactersManager.Instance.MainCharUid;
            _lastPos = transform.position;
            _lastRot = transform.localEulerAngles;
            moveData.Pos = _lastPos.ToVec3Data();
            moveData.Rotate = _lastRot.ToVec3Data();
            moveData.MoveSpeed = 0;
            moveData.SyncDeltaTime = deltaTime;
            moveData.ActionType = (int)actionType;
            NetworkManager.Instance.SendMsg((int)OuterOpcode.CMoveDataMsg, moveData);
        }
    }
}
