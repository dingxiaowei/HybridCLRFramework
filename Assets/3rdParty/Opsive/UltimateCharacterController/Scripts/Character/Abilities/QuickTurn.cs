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
    /// The Quick Turn ability allows the character to play an explicit animation when the character turns.
    /// </summary>
    [DefaultStopType(AbilityStopType.Automatic)]
    [DefaultAbilityIndex(8)]
    public class QuickTurn : StoredInputAbilityBase
    {
        [Tooltip("The minimum value of the input vector required for the ability to start.")]
        [SerializeField] protected float m_MinInputSqrMagnitude = 0.1f;
        [Tooltip("The value which differentiates between a walk and a run.")]
        [SerializeField] protected float m_SpeedChangeThreshold = 1;

        public float SpeedChangeThreshold { get { return m_SpeedChangeThreshold; } set { m_SpeedChangeThreshold = value; } }
        public float MinInputSqrMagnitude { get { return m_MinInputSqrMagnitude; } set { m_MinInputSqrMagnitude = value; } }

        private Vector2 m_AverageInput;
        private float m_StateIndex;
        private bool m_EventStop;

        protected override bool UseRawInput { get { return true; } }
        protected override bool RequireInput { get { return true; } }
        public override float AbilityFloatData { get { return m_StateIndex; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorQuickTurnComplete", OnQuickTurnComplete);
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            if (!base.CanStartAbility()) {
                return false;
            }

            // Don't start the ability if the input magnitude is below the threshold.
            if (m_CharacterLocomotion.RawInputVector.sqrMagnitude < m_MinInputSqrMagnitude) {
                return false;
            }

            m_AverageInput = Vector2.zero;
            for (int i = 0; i < m_InputCount; ++i) {
                m_AverageInput += m_Inputs[i];
            }
            m_AverageInput /= m_InputCount;

            // In order for turn to start the input vector has move in an opposite direction.
            if ((m_CharacterLocomotion.RawInputVector.x == 0 || m_AverageInput.x == 0 || Mathf.Sign(m_AverageInput.x) == Mathf.Sign(m_CharacterLocomotion.RawInputVector.x)) &&
                (m_CharacterLocomotion.RawInputVector.y == 0 || m_AverageInput.y == 0 || Mathf.Sign(m_AverageInput.y) == Mathf.Sign(m_CharacterLocomotion.RawInputVector.y))) {
                return false;
            }

            // Diagonal turns are not accepted.
            if (Vector2.Dot(m_CharacterLocomotion.RawInputVector, m_AverageInput) > -0.5f) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            if (Mathf.Abs(m_AverageInput.x) > m_SpeedChangeThreshold || Mathf.Abs(m_AverageInput.y) > m_SpeedChangeThreshold) {
                m_StateIndex = 1;
            } else {
                m_StateIndex = 0;
            }
            m_EventStop = false;
            m_CharacterLocomotion.ForceRootMotionRotation = true;

            base.AbilityStarted();
        }

        /// <summary>
        /// Animation event callback when the quick turn animation has completed.
        /// </summary>
        private void OnQuickTurnComplete()
        {
            m_EventStop = true;
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility()
        {
            return m_EventStop || !m_CharacterLocomotion.Moving;
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);
            m_CharacterLocomotion.ForceRootMotionRotation = false;
        }

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorQuickTurnComplete", OnQuickTurnComplete);
        }
    }
}