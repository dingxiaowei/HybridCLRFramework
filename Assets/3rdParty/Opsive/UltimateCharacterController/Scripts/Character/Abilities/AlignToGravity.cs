/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using Opsive.Shared.Game;
    using UnityEngine;

    /// <summary>
    /// The AlignToGravity ability provides a base class for any abilities that want to change the character's up rotation.
    /// </summary>
    public abstract class AlignToGravity : Ability
    {
        [Tooltip("Specifies the speed that the character can rotate to align to the ground.")]
        [SerializeField] protected float m_RotationSpeed = 10;
        [Tooltip("The direction of gravit that should be set when the ability stops. Set to Vector3.zero to disable.")]
        [SerializeField] protected Vector3 m_StopGravityDirection = Vector3.zero;

        public float RotationSpeed { get { return m_RotationSpeed; } set { m_RotationSpeed = value; } }
        public Vector3 StopGravityDirection { get { return m_StopGravityDirection; } set { m_StopGravityDirection = value; } }

        public override bool Enabled { get { return base.Enabled; } set { m_Enabled = value; if (!m_Enabled && IsActive) { StopAbility(); } } }
        public override bool IsConcurrent { get { return true; } }
        public override bool CanStayActivatedOnDeath { get { return true; } }

        protected bool m_Stopping;
        private bool m_StoppingFromUpdate;
        private float m_Epsilon = 1f - Mathf.Epsilon;
        private ScheduledEventBase m_AlignToGravityReset;

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            if (m_AlignToGravityReset != null) {
                Scheduler.Cancel(m_AlignToGravityReset);
            }
            m_CharacterLocomotion.AlignToGravity = true;
            m_Stopping = false;
            m_StoppingFromUpdate = false;
        }

        /// <summary>
        /// Rotates the character to be oriented with the specified normal.
        /// </summary>
        /// <param name="targetNormal">The direction that the character should be oriented towards on the vertical axis.</param>
        protected void Rotate(Vector3 targetNormal)
        {
            var deltaRotation = Quaternion.Euler(m_CharacterLocomotion.DeltaRotation);
            var rotation = m_Transform.rotation * deltaRotation;
            var proj = (rotation * Vector3.forward) - (Vector3.Dot((rotation * Vector3.forward), targetNormal)) * targetNormal;
            if (proj.sqrMagnitude > 0.0001f) {
                Quaternion targetRotation;
                if (m_CharacterLocomotion.Platform == null && !m_Stopping) {
                    var alignToGroundSpeed = m_RotationSpeed * m_CharacterLocomotion.TimeScale * Time.timeScale * Time.deltaTime;
                    targetRotation = Quaternion.Slerp(rotation, Quaternion.LookRotation(proj, targetNormal), alignToGroundSpeed);
                } else {
                    targetRotation = Quaternion.LookRotation(proj, targetNormal);
                }
                deltaRotation = deltaRotation * (Quaternion.Inverse(rotation) * targetRotation);
                m_CharacterLocomotion.DeltaRotation = deltaRotation.eulerAngles;
            }
        }

        /// <summary>
        /// Stops the ability if it needs to be stopped.
        /// </summary>
        public override void LateUpdate()
        {
            base.LateUpdate();

            // The ability should be stopped within LateUpdate so the character has a chance to be rotated.
            if (m_Stopping) {
                m_StoppingFromUpdate = true;
                StopAbility();
                m_StoppingFromUpdate = false;
            }
        }

        /// <summary>
        /// The ability is trying to stop. Ensure the character ends at the correct orientation.
        /// </summary>
        public override void WillTryStopAbility()
        {
            base.WillTryStopAbility();

            if (m_StopGravityDirection.sqrMagnitude > 0) {
                m_CharacterLocomotion.GravityDirection = m_StopGravityDirection.normalized;
            }
            m_Stopping = true;
            m_CharacterLocomotion.SmoothGravityYawDelta = false;
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility()
        {
            // Don't stop until the character is oriented in the correct direction.
            if (m_StoppingFromUpdate && (m_StopGravityDirection.sqrMagnitude == 0 || Vector3.Dot(m_Transform.rotation * Vector3.up, -m_StopGravityDirection) >= m_Epsilon)) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Resets the gravity direction and align to gravity to their stopping values.
        /// </summary>
        protected void ResetAlignToGravity()
        {
            if (m_StopGravityDirection.sqrMagnitude > 0) {
                m_CharacterLocomotion.GravityDirection = m_StopGravityDirection.normalized;
            }
            // Wait a frame to allow the camera to reset its rotation. This is useful if the ability is stopped in a single frame.
            m_AlignToGravityReset = Scheduler.Schedule(Time.deltaTime * 2, DoAlignToGravityReset);
        }

        /// <summary>
        /// Resets the AlignToGravity parameter.
        /// </summary>
        private void DoAlignToGravityReset()
        {
            m_CharacterLocomotion.AlignToGravity = false;
            m_AlignToGravityReset = null;
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            m_CharacterLocomotion.SmoothGravityYawDelta = true;
        }
    }
}