using Protoc;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
        private Vector3 endRotate;
        private bool firstTime = true;
        //private float syncTimeScale = 1.0f;//����
        private float deltaTime = 0;
        private float syncTime = 0;
        private bool delWithOnceMoveData = false;
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
                Debug.LogError("�������û����ȷ��ȡ��Animator���");
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
            if (!delWithOnceMoveData && moveDataMsgQueue.Count > 0)
            {
                delWithOnceMoveData = true;
                var moveMsg = moveDataMsgQueue.Dequeue();
                DealMoveData(moveMsg);
                //syncTimeScale = moveDataMsgQueue.Count > 3 ? 0.9f : 1.0f; //ͬ����ʱ��
            }
            if (delWithOnceMoveData)
            {
                deltaTime += Time.deltaTime;
                //var t = syncTime > 0 ? deltaTime / (syncTime * syncTimeScale) : 1;
                var t = (deltaTime / syncTime);
                transform.position = Vector3.Slerp(lastPos, newPos, t);
                transform.DORotate(endRotate, t);
                if (deltaTime >= syncTime)
                {
                    delWithOnceMoveData = false;
                    deltaTime = 0;
                }
            }
        }

        void DealMoveData(CMoveData data)
        {
            if (data == null)
                return;
            lastPos = transform.position;
            lastRot = Quaternion.Euler(transform.localPosition);

            newPos = data.Pos.ToVector3();
            newRot = Quaternion.Euler(data.Rotate.ToVector3());
            endRotate = data.Rotate.ToVector3();
            Grounded(!data.IsJump);
            HorizontalMove(data.MoveSpeed);
            syncTime = data.SyncDeltaTime; //�ƶ���ʱ��
            deltaTime = 0;
        }
    }
}
