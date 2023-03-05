using Protoc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dypsloom.DypThePenguin.Scripts.Damage;


namespace ActDemo
{
    public class CharacterMoveSyncController : MonoBehaviour
    {
        private Queue<CMoveData> moveDataMsgQueue = new Queue<CMoveData>();
        private Animator m_Animator;
        private Vector3 lastPos;
        private Quaternion lastRot;
        private Vector3 newPos;
        private Quaternion newRot;
        private bool firstTime = true;
        private float syncTimeScale = 1.0f;
        private float deltaTime = 0;
        private bool canMove = false;
        private float syncTime = 0;
        private EMoveActionType currentAction = EMoveActionType.Idle;
        private static readonly int m_HorizontalSpeedAnimHash = Animator.StringToHash("Horizontal Speed");
        private static readonly int m_VerticalSpeedAnimHash = Animator.StringToHash("Vertical Speed");
        private static readonly int m_GroundedAnimHash = Animator.StringToHash("Grounded");
        private static readonly int m_DamagedAnimHash = Animator.StringToHash("Damaged");
        private static readonly int m_ItemActionIndexAnimHash = Animator.StringToHash("ItemActionIndex");
        private static readonly int m_ItemActionAnimHash = Animator.StringToHash("ItemAction");
        private static readonly int m_ItemAnimHash = Animator.StringToHash("Item");
        private static readonly int m_DieAnimHash = Animator.StringToHash("Die");
        private static readonly int m_InteractAnimHash = Animator.StringToHash("Interact");
        private static readonly int m_EquippedItemAnimHash = Animator.StringToHash("EquippedItem");

        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
            if (m_Animator == null)
            {
                Debug.LogError("其他玩家没有正确获取到Animator组件");
            }
        }

        public void HorizontalMove(float speed)
        {
            m_Animator.SetFloat(m_HorizontalSpeedAnimHash, speed, 0f, Time.deltaTime);
        }

        public void VerticalMove(float speed)
        {
            m_Animator.SetFloat(m_VerticalSpeedAnimHash, speed, 0f, Time.deltaTime);
        }

        public void Grounded(bool grounded)
        {
            m_Animator.SetBool(m_GroundedAnimHash, grounded);
        }

        public void ItemAction(int item, int itemAction)
        {
            m_Animator.SetInteger(m_ItemAnimHash, item);
            m_Animator.SetInteger(m_ItemActionIndexAnimHash, itemAction);
            m_Animator.SetTrigger(m_ItemActionAnimHash);
        }

        public void EquipWeapon(int item)
        {
            m_Animator.SetInteger(m_EquippedItemAnimHash, item);
        }

        public void UnequipWeapon()
        {
            m_Animator.SetInteger(m_EquippedItemAnimHash, -1);
        }

        public void Die(bool dead)
        {
            m_Animator.SetBool(m_DieAnimHash, dead);
        }

        public void Damaged(Damage damage)
        {
            m_Animator.SetTrigger(m_DamagedAnimHash);
        }

        public void EnqueueMoveData(CMoveData data)
        {
            moveDataMsgQueue.Enqueue(data);
        }

        void Update()
        {
            if (!canMove && moveDataMsgQueue.Count > 0)
            {
                var moveMsg = moveDataMsgQueue.Dequeue();
                DealMoveData(moveMsg);
                syncTimeScale = moveDataMsgQueue.Count > 3 ? 0.9f : 1.0f;
            }
            if (canMove)
            {
                deltaTime += Time.deltaTime;
                var t = syncTime > 0 ? deltaTime / (syncTime * syncTimeScale) : 1;
                var currentPos = Vector3.Slerp(lastPos, newPos, t);
                transform.position = currentPos;
                transform.rotation = Quaternion.Slerp(lastRot, newRot, t);
                if(deltaTime >= syncTime)
                {
                    deltaTime -= syncTime;
                    canMove = false;
                }
            }
        }

        void DealMoveData(CMoveData data)
        {
            if (data == null)
                return;
            if (firstTime)
            {
                firstTime = false;
                lastPos = data.Pos.ToVector3();
                lastRot = Quaternion.Euler(data.Rotate.ToVector3());
                currentAction = EMoveActionType.Idle;
            }
            else
            {
                newPos = data.Pos.ToVector3();
                newRot = Quaternion.Euler(data.Rotate.ToVector3());
            }
            Grounded(!data.IsJump);
            //HorizontalMove(data.)
            HorizontalMove(data.MoveSpeed);
            syncTime = data.SyncDeltaTime;
            canMove = true;
        }
    }
}
