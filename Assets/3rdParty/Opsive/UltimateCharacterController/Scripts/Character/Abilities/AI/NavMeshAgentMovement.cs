/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEngine.AI;
using Opsive.UltimateCharacterController.Events;

namespace Opsive.UltimateCharacterController.Character.Abilities.AI
{
    /// <summary>
    /// Moves the character according to the NavMeshAgent desired velocity.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshAgentMovement : PathfindingMovement
    {
        private NavMeshAgent m_NavMeshAgent;
        private Jump m_JumpAbility;
        private Fall m_FallAbility;

        private Vector2 m_InputVector;
        private Vector3 m_DeltaRotation;
        private bool m_UpdateRotation;
        private int m_LastPathPendingFrame;

        public override Vector2 InputVector { get { return m_InputVector; } }
        public override Vector3 DeltaRotation { get { return m_DeltaRotation; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_NavMeshAgent = GetComponent<NavMeshAgent>();
            m_NavMeshAgent.autoTraverseOffMeshLink = false;
            m_NavMeshAgent.updatePosition = false;
            m_LastPathPendingFrame = int.MinValue;

            m_JumpAbility = m_CharacterLocomotion.GetAbility<Jump>();
            m_FallAbility = m_CharacterLocomotion.GetAbility<Fall>();

            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterGrounded", OnGrounded);
            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_GameObject, "OnRespawn", OnRespawn);
        }

        /// <summary>
        /// Updates the ability.
        /// </summary>
        public override void Update()
        {
            m_InputVector = Vector2.zero;
            var lookRotation = Quaternion.LookRotation(m_Transform.forward, m_CharacterLocomotion.Up);
            if (m_NavMeshAgent.isOnOffMeshLink) {
                UpdateOffMeshLink();
            } else {
                // When the path is pending the desired velocity isn't correct. Add a small buffer to ensure the path is valid.
                if (m_NavMeshAgent.pathPending) {
                    m_LastPathPendingFrame = Time.frameCount;
                }
                // Only move if a path exists.
                if (m_NavMeshAgent.desiredVelocity.sqrMagnitude > 0.01f && m_NavMeshAgent.remainingDistance > 0.01f && m_LastPathPendingFrame + 2 < Time.frameCount) {
                    lookRotation = Quaternion.LookRotation(m_NavMeshAgent.desiredVelocity, m_CharacterLocomotion.Up);
                    // The normalized velocity should be relative to the target rotation.
                    var velocity = Quaternion.Inverse(lookRotation) * m_NavMeshAgent.desiredVelocity;
                    // Only normalize if the magnitude is greater than 1. This will allow the character to walk.
                    if (velocity.sqrMagnitude > 1) {
                        velocity.Normalize();
                    }
                    m_InputVector.x = velocity.x;
                    m_InputVector.y = velocity.z;
                }
            }
            var rotation = lookRotation * Quaternion.Inverse(m_Transform.rotation);
            m_DeltaRotation.y = Utility.MathUtility.ClampInnerAngle(rotation.eulerAngles.y);

            base.Update();
        }

        /// <summary>
        /// Ensure the move direction is valid.
        /// </summary>
        public override void ApplyPosition()
        {
            if (m_NavMeshAgent.remainingDistance < m_NavMeshAgent.stoppingDistance) {
                // Prevent the character from jittering back and forth to land precisely on the target.
                var direction = m_Transform.InverseTransformPoint(m_NavMeshAgent.destination);
                var moveDirection = m_Transform.InverseTransformDirection(m_CharacterLocomotion.MoveDirection);
                if (Mathf.Abs(moveDirection.x) > Mathf.Abs(direction.x)) {
                    moveDirection.x = direction.x;
                }
                if (Mathf.Abs(moveDirection.z) > Mathf.Abs(direction.z)) {
                    moveDirection.z = direction.z;
                }
                m_CharacterLocomotion.MoveDirection = m_Transform.TransformDirection(moveDirection);
            }
            m_NavMeshAgent.nextPosition = m_Transform.position + m_CharacterLocomotion.MoveDirection;
        }

        /// <summary>
        /// Updates the velocity and look rotation using the off mesh link.
        /// </summary>
        protected virtual void UpdateOffMeshLink()
        {
            if (m_NavMeshAgent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeDropDown || m_NavMeshAgent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeJumpAcross) {
                // Ignore the y difference when determining a look direction and velocity.
                // This will give XZ distances a greater impact when normalized.
                var direction = m_NavMeshAgent.currentOffMeshLinkData.endPos - m_Transform.position;
                direction.y = 0;
                if (direction.sqrMagnitude > 0.1f || m_CharacterLocomotion.Grounded) {
                    var nextPositionDirection = m_Transform.InverseTransformPoint(m_NavMeshAgent.currentOffMeshLinkData.endPos);
                    nextPositionDirection.y = 0;
                    nextPositionDirection.Normalize();

                    m_InputVector.x = nextPositionDirection.x;
                    m_InputVector.y = nextPositionDirection.z;
                }

                // Jump if the agent hasn't jumped yet.
                if (m_JumpAbility != null && m_NavMeshAgent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeJumpAcross) {
                    if (!m_JumpAbility.IsActive && (m_FallAbility == null || !m_FallAbility.IsActive)) {
                        m_CharacterLocomotion.TryStartAbility(m_JumpAbility);
                    }
                }
            }
        }

        /// <summary>
        /// The character has changed grounded state. 
        /// </summary>
        /// <param name="grounded">Is the character on the ground?</param>
        protected virtual void OnGrounded(bool grounded)
        {
            if (grounded) {
                // The agent is no longer on an off mesh link if they just landed.
                if (m_NavMeshAgent.isOnOffMeshLink && (m_NavMeshAgent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeDropDown ||
                                                       m_NavMeshAgent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeJumpAcross)) {
                    m_NavMeshAgent.CompleteOffMeshLink();
                }
                // Warp the NavMeshAgent just in case the navmesh position doesn't match the transform position.
                var destination = m_NavMeshAgent.destination;
                m_NavMeshAgent.Warp(m_Transform.position);
                // Warp can change the destination so make sure that doesn't happen.
                if (m_NavMeshAgent.destination != destination) {
                    m_NavMeshAgent.SetDestination(destination);
                }
            }
        }

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            m_UpdateRotation = m_NavMeshAgent.updateRotation;
            m_NavMeshAgent.updateRotation = false;
        }

        /// <summary>
        /// The character has respawned. Start moving again.
        /// </summary>
        private void OnRespawn()
        {
            // Reset the NavMeshAgent to the new position.
            m_NavMeshAgent.Warp(m_Transform.position);
            if (m_NavMeshAgent.isOnOffMeshLink) {
                m_NavMeshAgent.ActivateCurrentOffMeshLink(false);
            }
            m_NavMeshAgent.updateRotation = m_UpdateRotation;
            m_LastPathPendingFrame = int.MinValue;
        }

        /// <summary>
        /// The character has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterGrounded", OnGrounded);
            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.UnregisterEvent(m_GameObject, "OnRespawn", OnRespawn);
        }
    }
}