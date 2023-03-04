using Protoc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ActDemo
{
    public class CharacterMoveSyncController : MonoBehaviour
    {
        private Queue<CMoveData> moveDataMsgQueue = new Queue<CMoveData>();
        private Animator animator;
        private Vector3 lastPos;
        private Vector3 lastRot;
        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void EnqueueMoveData(CMoveData data)
        {
            moveDataMsgQueue.Enqueue(data);
        }

        // Update is called once per frame
        void Update()
        {
            if (moveDataMsgQueue.Count > 0)
            {
                var moveMsg = moveDataMsgQueue.Dequeue();
                DealMoveData(moveMsg);
            }
        }

        void DealMoveData(CMoveData data)
        {
            if (data == null)
                return;

        }
    }
}
