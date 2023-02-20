﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    /// The Slide ability will apply a force to the character if the character is on a steep slope.
    /// </summary>
    [DefaultStopType(AbilityStopType.Automatic)]
    public class Slide : Ability
    {
        [Tooltip("Steepness (in degrees) above which the character can slide.")]
        [SerializeField] protected float m_MinSlideLimit = 40;
        [Tooltip("Steepness (in degrees) below which the character can slide.")]
        [SerializeField] protected float m_MaxSlideLimit = 89f;
        [Tooltip("Multiplier of the ground's slide value. The slide value is determined by (1 - dynamicFriction) of the ground's physic material.")]
        [SerializeField] protected float m_Multiplier = 0.4f;
        [Tooltip("The maximum speed that the character can slide.")]
        [SerializeField] protected float m_MaxSlideSpeed = 1;

        public float MinSlideLimit { get { return m_MinSlideLimit; } set { m_MinSlideLimit = value; } }
        public float MaxSlideLimit { get { return m_MaxSlideLimit; } set { m_MaxSlideLimit = value; } }
        public float Multiplier { get { return m_Multiplier; } set { m_Multiplier = value; } }
        public float MaxSlideSpeed { get { return m_MaxSlideSpeed; } set { m_MaxSlideSpeed = value; } }

        private float m_SlideSpeed;

        public override bool IsConcurrent { get { return true; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterGrounded", OnGrounded);
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            // An attribute may prevent the ability from starting.
            if (!base.CanStartAbility()) {
                return false;
            }

            return CanSlide();
        }

        /// <summary>
        /// Returns true if the character can slide on the ground.
        /// </summary>
        /// <returns>True if the character can slide on the ground.</returns>
        private bool CanSlide()
        {
            // The character cannot slide in the air.
            if (!m_CharacterLocomotion.Grounded) {
                return false;
            }

            // The character cannot slide if the slope isn't steep enough or is too steep.
            var slope = Vector3.Angle(m_CharacterLocomotion.Up, m_CharacterLocomotion.GroundRaycastHit.normal);
            if (slope < m_MinSlideLimit + m_CharacterLocomotion.SlopeLimitSpacing || slope > m_MaxSlideLimit) {
                return false;
            }

            // Don't slide if the character is moving and can step over the object.
            if (m_CharacterLocomotion.Moving) {
                var groundPoint = m_Transform.InverseTransformPoint(m_CharacterLocomotion.GroundRaycastHit.point);
                groundPoint.y = 0;
                groundPoint = m_Transform.TransformPoint(groundPoint);
                var direction = groundPoint - m_Transform.position;
                if (m_CharacterLocomotion.OverlapCount((direction.normalized * (direction.magnitude + m_CharacterLocomotion.Radius)) +
                    m_CharacterLocomotion.PlatformMovement + m_CharacterLocomotion.Up * (m_CharacterLocomotion.MaxStepHeight - m_CharacterLocomotion.ColliderSpacing)) == 0) {
                    return false;
                }
            }

            // The character can slide.
            return true;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            m_SlideSpeed = 0;
        }

        /// <summary>
        /// Updates the ability after the character movements have been applied.
        /// </summary>
        public override void LateUpdate()
        {
            var groundRaycastHit = m_CharacterLocomotion.GroundRaycastHit;
            // The slide value uses the ground's physic material to get the amount of friction of the material.
            var slide = (1 - groundRaycastHit.collider.material.dynamicFriction) * m_Multiplier;
            // Slide at a constant speed if the slope is within the slope limit.
            var slope = Vector3.Angle(groundRaycastHit.normal, m_CharacterLocomotion.Up);
            if (slope < m_CharacterLocomotion.SlopeLimit) {
                m_SlideSpeed = Mathf.Max(m_SlideSpeed, slide);
            } else { // The slope is steeper then the slope limit. Slide with an accelerating slide speed.
                m_SlideSpeed += slide * (slope / m_MinSlideLimit);
            }
            m_SlideSpeed = Mathf.Min(m_SlideSpeed, m_MaxSlideSpeed);

            // Add a force if the character should slide.
            if (m_SlideSpeed > 0) {
                var direction = Vector3.Cross(Vector3.Cross(groundRaycastHit.normal, -m_CharacterLocomotion.Up), groundRaycastHit.normal);
                AddForce(direction.normalized * m_SlideSpeed * m_CharacterLocomotion.TimeScale * Time.timeScale * m_CharacterLocomotion.DeltaTime, 1, false, true);
            }
        }

        /// <summary>
        /// Stop the ability from running.
        /// </summary>
        /// <returns>True if the ability was stopped.</returns>
        public override bool CanStopAbility()
        {
            return !CanSlide();
        }

        /// <summary>
        /// The character has changed grounded state. 
        /// </summary>
        /// <param name="grounded">Is the character on the ground?</param>
        private void OnGrounded(bool grounded)
        {
            if (!grounded) {
                StopAbility();
            }
        }

        /// <summary>
        /// The character has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterGrounded", OnGrounded);
        }
    }
}