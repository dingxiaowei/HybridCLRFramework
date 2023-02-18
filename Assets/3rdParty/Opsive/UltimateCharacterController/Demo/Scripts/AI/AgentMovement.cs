/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.AI
{
    using Opsive.UltimateCharacterController.Character.Abilities.AI;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Moves the agent to random positions within a circle. No pathfinding is done.
    /// </summary>
    public class AgentMovement : PathfindingMovement
    {
        [Tooltip("The radius of the position that the character can traverse to.")]
        [SerializeField] protected float m_Radius = 3;
        [Tooltip("The agent has arrived at the destination when the distance is less than the stopping distance.")]
        [SerializeField] protected float m_StoppingDistance = 0.1f;
        [Tooltip("The amount of time that the agent should wait at each destination.")]
        [SerializeField] protected MinMaxFloat m_RandomWait = new MinMaxFloat(0.2f, 0.7f);

        private Use m_UseAbility;
        private Vector3 m_Center;
        private Vector3 m_Destination;
        private Vector2 m_InputVector;
        private float m_WaitTime;
        private float m_ArriveTime;

        public override Vector2 InputVector { get { return m_InputVector; } }
        public override Vector3 DeltaRotation { get { return Vector3.zero; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_UseAbility = m_CharacterLocomotion.GetAbility<Use>();
            m_Center = m_Transform.position;
            DetermineDestination();
        }

        /// <summary>
        /// Determines a new destination to move towards.
        /// </summary>
        private void DetermineDestination()
        {
            var randomPosition = Random.insideUnitSphere * m_Radius;
            randomPosition.y = 0;
            SetDestination(m_Center + randomPosition);
        }

        /// <summary>
        /// Sets the destination of the pathfinding agent.
        /// </summary>
        /// <param name="destination">The position to move towards.</param>
        /// <returns>True if the destination was set.</returns>
        public override bool SetDestination(Vector3 destination)
        {
            m_Destination = destination;
            m_ArriveTime = -1;
            return true;
        }

        /// <summary>
        /// Updates the ability.
        /// </summary>
        public override void Update()
        {
            // The Use ability uses root motion to control the movement so the pathfinding movement should not override that.
            if (m_UseAbility.IsActive) {
                return;
            }

            // Choose a new destination after the agent has arrived at the current destination.
            if (IsAtDestination()) {
                if (m_ArriveTime == -1) {
                    m_WaitTime = m_RandomWait.RandomValue;
                    m_ArriveTime = Time.time;
                }
                if (m_ArriveTime + m_WaitTime < Time.time) {
                    DetermineDestination();
                }
            }

            // The normalized velocity should be relative to the target rotation.
            var desiredMovement = (m_Destination - m_Transform.position).normalized;
            var velocity = Quaternion.Inverse(m_Transform.rotation) * desiredMovement;
            m_InputVector.x = velocity.x;
            m_InputVector.y = velocity.z;

            base.Update();
        }

        /// <summary>
        /// Ensure the move direction is valid.
        /// </summary>
        public override void ApplyPosition()
        {
            if (IsAtDestination()) {
                // Prevent the character from jittering back and forth to land precisely on the target.
                var direction = m_Transform.InverseTransformPoint(m_Destination);
                var moveDirection = m_Transform.InverseTransformDirection(m_CharacterLocomotion.MoveDirection);
                if (Mathf.Abs(moveDirection.x) > Mathf.Abs(direction.x)) {
                    moveDirection.x = direction.x;
                }
                if (Mathf.Abs(moveDirection.z) > Mathf.Abs(direction.z)) {
                    moveDirection.z = direction.z;
                }
                m_CharacterLocomotion.MoveDirection = m_Transform.TransformDirection(moveDirection);
            }
        }

        /// <summary>
        /// Is the agent at the destination?
        /// </summary>
        /// <returns>True if the agent is at the destination.</returns>
        private bool IsAtDestination()
        {
            return (m_Destination - m_Transform.position).sqrMagnitude < m_StoppingDistance * m_StoppingDistance;
        }
    }
}