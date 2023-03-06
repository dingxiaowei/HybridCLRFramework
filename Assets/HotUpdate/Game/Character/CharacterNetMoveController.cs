using Dypsloom.DypThePenguin.Scripts.Character;
using Protoc;
using UnityEngine;

namespace ActDemo
{
    enum EMoveActionType
    {
        Idle = 0,
        Move = 1,
        Jump = 2,
        JumpMove = 3
    }

    public class CharacterNetMoveController : MonoBehaviour
    {
        private Character _Character;
        private Vector3 _lastPos;
        private Vector3 _lastRot;
        private bool isMove = false; //ң���Ƿ���λ��
        private bool isJump = false;
        private float deltaTime = 0f;
        private EMoveActionType currentAction = EMoveActionType.Idle;
        private float realTimeSinceStartUp = 0f;
        //�ϴε��ƶ�ֵ
        private float lastHV = 0;
        private float lastVV = 0;
        private float moveSpeed = 0;
        private bool moveJumpFlag = false;
        private void Awake()
        {
            _Character = GetComponent<Character>();
        }
        void Start()
        {
            RegisterEvents();
        }

        void RegisterEvents()
        {
            _Character.IsGroundedEvent += OnGrounded;
            _Character.BeginJumpEvent += OnJump;
            _Character.MoveEvent += OnMove;
        }

        void UnRegisterEvents()
        {
            _Character.IsGroundedEvent -= OnGrounded;
            _Character.BeginJumpEvent -= OnJump;
            _Character.MoveEvent -= OnMove;
        }

        void OnGrounded()
        {
            //Debug.Log("��ʼ���");
            isJump = false;
            //TODO:���ҲҪ��һ���ؼ�֡��Ϣ
            MoveMsg(deltaTime, EMoveActionType.Idle);
        }

        void OnJump()
        {
            //Debug.Log("��ʼ��Ծ");
            isJump = true;
            MoveMsg(deltaTime, EMoveActionType.Jump);
        }

        void OnMove(float horizontalValue, float verticalValue)
        {
            if (lastHV == 0 && lastVV == 0 && lastHV == horizontalValue && lastVV == verticalValue)
                return;
            lastVV = verticalValue;
            lastHV = horizontalValue;
            moveSpeed = Mathf.Sqrt((verticalValue * verticalValue) + (horizontalValue * horizontalValue));
            //Debug.Log($"�ƶ�:{horizontalValue},{verticalValue}");
            if (horizontalValue == 0 && verticalValue == 0)
            {
                isMove = false;
                //TODO:�ƶ�ֹͣҲҪ��һ���ؼ�֡
                MoveMsg(deltaTime, EMoveActionType.Idle);
            }
            else
            {
                isMove = true;
            }
        }
        
        void Update()
        {
            if (isMove)
            {
                moveJumpFlag = true;
                deltaTime += Time.deltaTime;
                if (deltaTime > GameConfig.SyncTime)
                {
                    MoveMsg(deltaTime);
                    deltaTime = 0;
                    realTimeSinceStartUp = Time.realtimeSinceStartup;
                }
                currentAction = EMoveActionType.Move;
            }
            if (isJump)
            {
                if (!moveJumpFlag)
                    deltaTime += Time.deltaTime;
                if (deltaTime > GameConfig.SyncTime)
                {
                    MoveMsg(deltaTime, EMoveActionType.Jump);
                    deltaTime = 0;
                    realTimeSinceStartUp = Time.realtimeSinceStartup;
                }
                if (!moveJumpFlag)
                    currentAction = EMoveActionType.Jump;
                else
                    currentAction = EMoveActionType.JumpMove;
            }
            moveJumpFlag = false;
        }

        void MoveMsg(float deltaTime, EMoveActionType actionType = EMoveActionType.Move)
        {
            CMoveData moveData = new CMoveData();
            moveData.UserId = CharactersManager.Instance.MainCharUid;
            _lastPos = transform.localPosition;
            _lastRot = transform.localEulerAngles;
            moveData.Pos = _lastPos.ToVec3Data();
            moveData.Rotate = _lastRot.ToVec3Data();
            moveData.MoveSpeed = moveSpeed;
            moveData.SyncDeltaTime = deltaTime;
            moveData.ActionType = (int)actionType;
            moveData.RealtimeSinceStartUp = Time.realtimeSinceStartup;
            moveData.IsMove = isMove;
            moveData.IsJump = isJump;
            NetworkManager.Instance.SendMsg((int)OuterOpcode.CMoveDataMsg, moveData);
        }

        private void OnDestroy()
        {
            UnRegisterEvents();
        }
    }
}
