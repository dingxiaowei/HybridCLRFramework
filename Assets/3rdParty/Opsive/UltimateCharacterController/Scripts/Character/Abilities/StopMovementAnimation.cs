/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    /// Prevents the movement animation from playing when the character would run into a solid object. The move direction is predicted if the character is using
    /// root motion because with root motion the movement is applied after the animation plays.
    /// </summary>
    [DefaultStartType(AbilityStartType.Manual)]
    public class StopMovementAnimation : Ability
    {
        [Tooltip("The distance that is checked to determine if there is a collision.")]
        [SerializeField] protected float m_CollisionCheckDistance = 0.1f;
        [Tooltip("The maximum Character Locomotion Wall Glide Curve value that the ability should start with. The higher the value to more the ability will ignore the Wall Glide Curve value.")]
        [SerializeField] protected float m_WallGlideCurveThreshold = 0.1f;

        public float CollisionCheckDistance { get { return m_CollisionCheckDistance; } set { m_CollisionCheckDistance = value; } }
        public float WallGlideCurveThreshold { get { return m_WallGlideCurveThreshold; } set { m_WallGlideCurveThreshold = value; } }
        public override bool IsConcurrent { get { return true; } }

        private RaycastHit m_RaycastHit;
        private RestrictPosition m_RestrictPosition;

        /// <summary>
        /// Cache the Restrict Position ability.
        /// </summary>
        public override void Start()
        {
            base.Start();

            m_RestrictPosition = m_CharacterLocomotion.GetAbility<RestrictPosition>();
        }

        /// <summary>
        /// Called when another ability is attempting to start and the current ability is active.
        /// Returns true or false depending on if the new ability should be blocked from starting.
        /// </summary>
        /// <param name="startingAbility">The ability that is starting.</param>
        /// <returns>True if the ability should be blocked.</returns>
        public override bool ShouldBlockAbilityStart(Ability startingAbility)
        {
            return startingAbility is StoredInputAbilityBase;
        }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        /// <returns>True if the ability should be stopped.</returns>
        public override bool ShouldStopActiveAbility(Ability activeAbility)
        {
            return activeAbility is StoredInputAbilityBase;
        }

        /// <summary>
        /// Starts the ability if there will be a collision.
        /// </summary>
        public override void InactiveUpdate()
        {
            if (PredicatedCollision()) {
                m_CharacterLocomotion.InputVector = Vector2.zero;
                StartAbility();
            }
        }

        /// <summary>
        /// Predicts if there will be a collision based on the input vector. If using root motion this isn't completely accurate because the animation is
        /// updated before the movement but it still works well.
        /// </summary>
        /// <returns>True if the character would collide with an object based on the input vector.</returns>
        private bool PredicatedCollision()
        {
            if (m_CharacterLocomotion.InputVector.sqrMagnitude < m_CharacterLocomotion.ColliderSpacingSquared) {
                return false;
            }

            // If restrict position is stopping the move then the animation should also stop.
            var direction = m_Transform.TransformDirection(new Vector3(m_CharacterLocomotion.InputVector.x, 0, m_CharacterLocomotion.InputVector.y)).normalized * m_CollisionCheckDistance;
            var targetPosition = m_Transform.position + direction;
            if (m_RestrictPosition != null && m_RestrictPosition.IsActive && m_RestrictPosition.RestrictedPosition(ref targetPosition)) {
                return true;
            }

            // Stop the animation if the character would collide with a solid object.
            if (m_CharacterLocomotion.SingleCast(direction, Vector3.zero, m_CharacterLayerManager.SolidObjectLayers, ref m_RaycastHit)) {
                if (m_RaycastHit.rigidbody != null && !m_RaycastHit.rigidbody.isKinematic) {
                    return false;
                }

                var slope = Vector3.Angle(m_CharacterLocomotion.Up, m_RaycastHit.normal);
                if (slope <= m_CharacterLocomotion.SlopeLimit + m_CharacterLocomotion.SlopeLimitSpacing) {
                    return false;
                }

                // The character may be able to glide across the wall.
                var hitStrength = 1 - Vector3.Dot(direction.normalized, -m_RaycastHit.normal);
                if (m_CharacterLocomotion.WallGlideCurve.Evaluate(hitStrength) > m_WallGlideCurveThreshold) {
                    return false;
                }

                // The character will stop because of the hit object.
                return true;
            }

            return false;
        }

        /// <summary>
        /// Stops the movement animation if there is a collision.
        /// </summary>
        public override void Update()
        {
            // Stop the ability as soon as there are no more collisions.
            if (!PredicatedCollision()) {
                StopAbility();
                return;
            }

            m_CharacterLocomotion.InputVector = Vector3.zero;
        }

        /// <summary>
        /// Verify the position values.
        /// </summary>
        public override void ApplyPosition()
        {
            var localMoveDirection = m_Transform.InverseTransformDirection(m_CharacterLocomotion.MoveDirection);
            localMoveDirection.x = localMoveDirection.z = 0;
            m_CharacterLocomotion.MoveDirection = m_Transform.TransformDirection(localMoveDirection);
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            EventHandler.ExecuteEvent(m_GameObject, "OnStoredInputAbilityResetStoredInputs");
        }
    }
}