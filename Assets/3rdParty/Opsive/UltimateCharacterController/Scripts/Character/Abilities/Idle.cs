/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Game;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    /// Plays a random idle animation based off of the AbilityFloatData animator parameter.
    /// </summary>
    [DefaultStopType(AbilityStopType.Automatic)]
    public class Idle : Ability
    {
        [Tooltip("Specifies how long the ability should wait until it is started.")]
        [SerializeField] protected float m_StartDelay = 15;
        [Tooltip("The maximum AbilityFloatData animator parameter.")]
        [SerializeField] protected float m_MaxAbilityFloatDataValue;
        [Tooltip("Should a random int between 0 and MaxAbilityFloatDataValue be used? If false AbilityFloatData will be increased sequentially.")]
        [SerializeField] protected bool m_RandomValue = true;
        [Tooltip("The minimum amount of time that the current AbilityFloatData value should be set.")]
        [SerializeField] protected float m_MinDuration = 5;
        [Tooltip("The maximum amount of time that the current AbilityFloatData value should be set.")]
        [SerializeField] protected float m_MaxDuration = 10;

        public float MaxAbilityFloatDataValue { get { return m_MaxAbilityFloatDataValue; } set { m_MaxAbilityFloatDataValue = value; } }
        public bool RandomValue { get { return m_RandomValue; } set { m_RandomValue = value; } }
        public float MinDuration { get { return m_MinDuration; } set { m_MinDuration = value; } }
        public float MaxDuration { get { return m_MaxDuration; } set { m_MaxDuration = value; } }

        private float m_CanStartTime = -1;
        private float m_AbilityFloatDataValue;
        private ScheduledEventBase m_FloatChangeEvent;

        public override float AbilityFloatData { get { return m_AbilityFloatDataValue; } }

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

            // The ability can be started when the character is not moving and is on the ground.
            if (!m_CharacterLocomotion.Moving && m_CharacterLocomotion.Grounded) {
                if (m_CanStartTime == -1) {
                    m_CanStartTime = Time.time;
                    return false;
                }
                // A delay can be added to prevent the more extreme idle animations from playing immediately.
                return m_CanStartTime + m_StartDelay < Time.time;
            }
            if (m_CanStartTime != -1) {
                m_CanStartTime = -1;
            }
            return false;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            // Determine a new AbilityFloatData.
            DetermineAbilityFloatDataValue();
        }

        private void DetermineAbilityFloatDataValue()
        {
            if (m_RandomValue) {
                m_AbilityFloatDataValue = Random.Range(0, (int)(m_MaxAbilityFloatDataValue + 1));
            } else { // Sequence.
                m_AbilityFloatDataValue = (int)(m_AbilityFloatDataValue + 1) % (int)(m_MaxAbilityFloatDataValue + 1);
            }
            m_CharacterLocomotion.UpdateAbilityAnimatorParameters();

            // A new value should be chosen between the min and max duration.
            m_FloatChangeEvent = Scheduler.ScheduleFixed(Random.Range(m_MinDuration, m_MaxDuration), DetermineAbilityFloatDataValue);
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility()
        {
            // The ability should be stopped when the character moves or is no longer grounded.
            return m_CharacterLocomotion.Moving || !m_CharacterLocomotion.Grounded;
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            m_CanStartTime = -1;

            // DetermineAbilityFloatDataValue no longer needs to be called.
            Scheduler.Cancel(m_FloatChangeEvent);
            m_FloatChangeEvent = null;
        }
    }
}