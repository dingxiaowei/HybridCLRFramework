/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using Opsive.UltimateCharacterController.Objects.CharacterAssist;
    using UnityEngine;

    /// <summary>
    /// The AlignToGravityZone ability will orient the character to the direction of the gravity zones.
    /// </summary>
    [DefaultStartType(AbilityStartType.Manual)]
    public class AlignToGravityZone : AlignToGravity
    {
        private GravityZone[] m_GravityZones;
        private int m_GravityZoneCount;

        /// <summary>
        /// Registers a GravityZone with the ability.
        /// </summary>
        /// <param name="gravityZone">The GravityZone that should be registered.</param>
        public void RegisterGravityZone(GravityZone gravityZone)
        {
            if (m_GravityZones == null) {
                m_GravityZones = new GravityZone[1];
            } else if (m_GravityZones.Length == m_GravityZoneCount) {
                System.Array.Resize(ref m_GravityZones, m_GravityZoneCount + 1);
            }

            m_GravityZones[m_GravityZoneCount] = gravityZone;
            m_GravityZoneCount++;

            if (!IsActive) {
                StartAbility();
            }
        }

        /// <summary>
        /// Unregisters a GravityZone with the ability.
        /// </summary>
        /// <param name="gravityZone">The GravityZone that should be unregistered.</param>
        public void UnregisterGravityZone(GravityZone gravityZone)
        {
            for (int i = 0; i < m_GravityZoneCount; ++i) {
                if (m_GravityZones[i] != gravityZone) {
                    continue;
                }

                // Shift all of the array elements down one.
                for (int j = i; j < m_GravityZoneCount - 1; ++j) {
                    m_GravityZones[j] = m_GravityZones[j + 1];
                }
                m_GravityZoneCount--;
                m_GravityZones[m_GravityZoneCount] = null;
                break;
            }

            if (m_GravityZoneCount == 0) {
                StopAbility();
            }
        }

        /// <summary>
        /// Update the rotation forces.
        /// </summary>
        public override void UpdateRotation()
        {
            var targetNormal = m_Stopping ? (m_StopGravityDirection.sqrMagnitude > 0 ? -m_StopGravityDirection : -m_CharacterLocomotion.GravityDirection) : Vector3.zero;
            if (!m_Stopping) {
                var position = m_Transform.position;
                for (int i = 0; i < m_GravityZoneCount; ++i) {
                    // If the character is on the ground then only one gravity zone can influence the character. This will prevent the character from orienting to a different direction
                    // while on the ground.
                    if (m_CharacterLocomotion.Grounded) {
                        var normal = m_GravityZones[i].DetermineGravityDirection(position);
                        if (normal.sqrMagnitude > targetNormal.sqrMagnitude) {
                            targetNormal = normal;
                        }
                    } else {
                        // The character is not on the ground - use the average of all of the directions.
                        targetNormal += m_GravityZones[i].DetermineGravityDirection(position);
                    }
                }

                if (targetNormal.sqrMagnitude == 0) {
                    return;
                }
                targetNormal.Normalize();
                m_CharacterLocomotion.GravityDirection = -targetNormal;
            }

            Rotate(targetNormal);
        }

        /// <summary>
        /// The ability is trying to stop. Ensure the character ends at the correct orientation.
        /// </summary>
        public override void WillTryStopAbility()
        {
            // If the gravity zone count isn't 0 then the ability will not be able to stop.
            if (m_GravityZoneCount > 0) {
                return;
            }

            base.WillTryStopAbility();
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility()
        {
            if (!base.CanStopAbility()) {
                return false;
            }
            return m_GravityZoneCount == 0;
        }

        /// <summary>
        /// Can the ability be force stopped?
        /// </summary>
        /// <returns>True if the ability can be force stopped.</returns>
        public override bool CanForceStopAbility()
        {
            if (!base.CanForceStopAbility()) {
                return false;
            }
            return m_GravityZoneCount == 0;
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            if (!m_CharacterLocomotion.IsAbilityTypeActive<AlignToGround>()) {
                ResetAlignToGravity();
            }
        }
    }
}