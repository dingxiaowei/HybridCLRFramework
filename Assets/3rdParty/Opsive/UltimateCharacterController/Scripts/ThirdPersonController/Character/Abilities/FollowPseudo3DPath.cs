/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Character.MovementTypes;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Character.Abilities
{
    /// <summary>
    /// Follows a path specified by the 2.5D movement type. Ensures the character stays at a consistant horizontal offset.
    /// </summary>
    [DefaultStartType(AbilityStartType.Manual)]
    public class FollowPseudo3DPath : Ability
    {
        public override bool IsConcurrent { get { return true; } }

        private MovementTypes.Pseudo3D m_Pseudo3DMovementType;
        private int m_PathIndex;
        private float m_Offset;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            EventHandler.RegisterEvent<MovementType, bool>(m_GameObject, "OnCharacterChangeMovementType", OnCharacterChangeMovementType);
        }

        /// <summary>
        /// The specified movement type has been activated or deactivated.
        /// </summary>
        /// <param name="movementType">The movement type that has been activated or deactivated.</param>
        /// <param name="active">Is the movement type active?</param>
        private void OnCharacterChangeMovementType(MovementType movementType, bool active)
        {
            if (!(movementType is MovementTypes.Pseudo3D)) {
                return;
            }

            if (!IsActive && active) {
                // If the movement type is active then the ability should start if a path exists.
                var pseudo3DMovementType = movementType as MovementTypes.Pseudo3D;
                if (pseudo3DMovementType.Path != null) {
                    StartAbility();
                    return;
                }
            } else if (IsActive && !active) {
                // If the movement type is no longer active then the ability should stop.
                StopAbility();
            }
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            m_Pseudo3DMovementType = m_CharacterLocomotion.ActiveMovementType as MovementTypes.Pseudo3D;
            
            // Store the horizontal offset between the character and the path. This offset will ensure the character stays at a constant horizontal position.
            var pathRotation = Quaternion.LookRotation(m_Pseudo3DMovementType.Path.GetTangent(m_Transform.position, ref m_PathIndex));
            var closestPoint = m_Pseudo3DMovementType.Path.GetClosestPoint(m_Transform.position, ref m_PathIndex);
            m_Offset = MathUtility.InverseTransformPoint(m_Transform.position, pathRotation, closestPoint).x;
        }

        /// <summary>
        /// Stop the ability if the path no longer exists.
        /// </summary>
        public override void Update()
        {
            if (m_Pseudo3DMovementType.Path == null) {
                StopAbility();
            }
        }

        /// <summary>
        /// Update the controller's position values.
        /// </summary>
        public override void UpdatePosition()
        {
            // Retireve the closest point and tangent of the path at the target position. This position will be used when determining if the character needs to change
            // horizontal offsets.
            var targetPosition = m_Transform.position + m_CharacterLocomotion.MoveDirection;
            var closestPoint = m_Pseudo3DMovementType.Path.GetClosestPoint(targetPosition, ref m_PathIndex);
            var pathRotation = Quaternion.LookRotation(m_Pseudo3DMovementType.Path.GetTangent(targetPosition, ref m_PathIndex));

            // Update the offset if the character can move along the relative x axis. The depth term is used in relation to the camera's depth axis.
            if (m_Pseudo3DMovementType.AllowDepthMovement && m_CharacterLocomotion.RawInputVector.y != 0) {
                m_Offset -= MathUtility.InverseTransformDirection(m_CharacterLocomotion.MotorThrottle, pathRotation).x;
            }

            // Update the target position based on the difference between the current offset and the stored offset.
            var offset = MathUtility.InverseTransformPoint(targetPosition, pathRotation, closestPoint).x;
            targetPosition = MathUtility.TransformPoint(targetPosition, pathRotation, Vector3.right * (offset - m_Offset));
            m_CharacterLocomotion.MoveDirection = targetPosition - m_Transform.position;
        }

        /// <summary>
        /// The character has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<MovementType, bool>(m_GameObject, "OnCharacterChangeMovementType", OnCharacterChangeMovementType);
        }
    }
}