/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Environment {
    using Dypsloom.DypThePenguin.Scripts.Character;
    using Dypsloom.Shared.Utility;
    using System.Collections.Generic;
    using UnityEngine;
    using CharacterController = UnityEngine.CharacterController;

    /// <summary>
    /// A component used as moving platform that a character can stand on.
    /// </summary>
    public class MovingPlatform : MonoBehaviour, IMover
    {
        [SerializeField] protected LayerMask m_AffectedLayers;
        [Tooltip("If the target index is -1 the platform will continue moving.")]
        [SerializeField] protected int m_TargetIndex = -1;
        [SerializeField] protected int m_MoveIndex;
        [SerializeField] protected float m_Speed = 1;
        [SerializeField] protected Transform[] m_MovePoints;

        protected Vector3 m_Movement;
        protected Transform m_CurrentPoint;
        protected Dictionary<Collider, Transform> m_PreviousParents;

        public Vector3 Movement => m_Movement;

        public bool IsMoving => m_TargetIndex != m_MoveIndex;

        /// <summary>
        /// Cache components.
        /// </summary>
        private void Awake()
        {
            m_PreviousParents = new Dictionary<Collider, Transform>();
            transform.position = m_MovePoints[m_MoveIndex].position;
        }

        /// <summary>
        /// Check if an object entered the moving platform.
        /// </summary>
        /// <param name="other"></param>
        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!m_AffectedLayers.Contains(other)) { return; }

            var result = AddMoverToCharacter(other);

            if (result == true) { return; }
                
            var childTransform = other.attachedRigidbody != null ?
                other.attachedRigidbody.transform :
                other.transform;

            m_PreviousParents[other] = childTransform.parent;
            childTransform.SetParent(transform);
        }

        /// <summary>
        /// Add a mover to the character that entered the trigger.
        /// </summary>
        /// <param name="other">The character collider.</param>
        /// <returns>True if the collider was a character.</returns>
        protected bool AddMoverToCharacter(Collider other)
        {
            Character character = null;

            if (other is CharacterController characterController) {
                character = characterController.GetComponent<Character>();
            }

            if (character == null && other.attachedRigidbody != null) {
                character = other.attachedRigidbody.GetComponent<Character>();
            }
            
            if (character != null) {
                character.CharacterMover.AddExternalMover(this);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove the mover when exiting the trigger.
        /// </summary>
        /// <param name="other"></param>
        protected virtual void OnTriggerExit(Collider other)
        {
            if (!m_AffectedLayers.Contains(other)) { return; }

            var success = RemoveMoverFromCharacter(other);
            
            if(success){return;}
            
            m_PreviousParents.TryGetValue(other, out var parent); 
            
            if (other.attachedRigidbody != null) {
                other.attachedRigidbody.transform.SetParent(parent);
            } else {
                other.transform.SetParent(parent);
            }
        }
        
        /// <summary>
        /// Remove the mover from a character.
        /// </summary>
        /// <param name="other">The other collider.</param>
        /// <returns>True if the collider had a character.</returns>
        protected bool RemoveMoverFromCharacter(Collider other)
        {
            Character character = null;

            if (other is CharacterController characterController) {
                character = characterController.GetComponent<Character>();
            }

            if (character == null && other.attachedRigidbody != null) {
                character = other.attachedRigidbody.GetComponent<Character>();
            }
            
            if (character != null) {
                character.CharacterMover.RemoveExternalMover(this);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the next destination index for the platform.
        /// </summary>
        /// <returns>The index.</returns>
        public virtual int GetNextIndex()
        {
            return m_MovePoints.Length > m_MoveIndex + 1 ? m_MoveIndex + 1: 0;
        }

        /// <summary>
        /// Set the next destination index.
        /// </summary>
        /// <param name="index">The index.</param>
        public virtual void SetTargetIndex(int index)
        {
            m_TargetIndex = index;
        }

        /// <summary>
        /// move the platform.
        /// </summary>
        protected virtual void Update()
        {
            if (m_TargetIndex == m_MoveIndex) {
                m_Movement = Vector3.zero;
                return;
            }

            var nextIndex = GetNextIndex();

            var nextPoint = m_MovePoints[nextIndex].position;

            var dir = nextPoint - transform.position;

            m_Movement = dir.normalized * m_Speed;
        
            transform.Translate(m_Movement* Time.deltaTime);

            if (Vector3.SqrMagnitude(dir) < 0.2f) { m_MoveIndex = nextIndex; }
        }

        /// <summary>
        /// No need to tick as we have an update.
        /// </summary>
        public void Tick()
        {
            //Nothing
        }
        
        /// <summary>
        /// No need to set a parent of a platform.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public void SetParentMover(IParentMover parent)
        {
            //Nothing
        }

    }
}
