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
    /// Acts as the parent for any ability which activates based on stored input values.
    /// </summary>
    public abstract class StoredInputAbilityBase : Ability
    {
        [Tooltip("The number of inputs to store when determining if the ability can start.")]
        [SerializeField] protected int m_MaxInputCount = 10;

        public int MaxInputCount { get { return m_MaxInputCount; } }

        protected Vector2[] m_Inputs;
        protected int m_InputCount;
        protected int m_InputIndex = -1;
        protected int m_LastCanStartFrame = -1;
        private bool m_AccumulateInputs = true;

        protected abstract bool UseRawInput { get; }
        protected abstract bool RequireInput { get; }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_Inputs = new Vector2[m_MaxInputCount];
            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterGrounded", OnGrounded);
            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterImmediateTransformChange", OnImmediateTransformChange);
            EventHandler.RegisterEvent(m_GameObject, "OnStoredInputAbilityResetStoredInputs", ResetStoredInputs);
        }

        /// <summary>
        /// Store the character's input values.
        /// </summary>
        public override void InactiveUpdate()
        {
            base.InactiveUpdate();

            // When the ability first stops InactiveUpdate will run immediately after CanStopAbility if the AbilityStopType is Automatic. This first
            // call should be ignored.
            if (!m_AccumulateInputs) {
                m_AccumulateInputs = true;
                return;
            }

            if (!m_CharacterLocomotion.Grounded) {
                return;
            }

            if (!m_CharacterLocomotion.Moving) {
                if (m_InputCount > 0) {
                    m_InputCount--;
                }
                return;
            }

            m_InputIndex = (m_InputIndex + 1) % m_MaxInputCount;
            m_Inputs[m_InputIndex] = UseRawInput ? m_CharacterLocomotion.RawInputVector : m_CharacterLocomotion.InputVector;
            // The count will increase until the max value count.
            if (m_InputIndex == m_InputCount) {
                m_InputCount++;
            }
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            // The character is updated during FixedUpdate but the input is only updated within Update so it's possible for duplicate inputs to occur.
            // Return early if the current frame is the last checked frame to prevent the ability starting from based on duplicate input.
            if (m_LastCanStartFrame == Time.frameCount) {
                return false;
            }
            m_LastCanStartFrame = Time.frameCount;

            // An attribute may prevent the ability from starting.
            if (!base.CanStartAbility()) {
                return false;
            }

            // The ability can't be started if the character isn't on the ground or there are no values.
            if (!m_CharacterLocomotion.Grounded || (RequireInput && m_InputCount == 0)) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            m_AccumulateInputs = false;
            ResetStoredInputs();
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            AbilityStopped(force, true);
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        /// <param name="resetStoredInputs">Should the stored inputs be reset?</param>
        protected void AbilityStopped(bool force, bool resetStoredInputs)
        {
            base.AbilityStopped(force);

            if (resetStoredInputs) {
                // Reset the inputs for all of the stored input abilities to prevent the other ability from starting so soon after the initial ability stopped.
                EventHandler.ExecuteEvent(m_GameObject, "OnStoredInputAbilityResetStoredInputs");
            }
        }

        /// <summary>
        /// The character has changed grounded states. 
        /// </summary>
        /// <param name="grounded">Is the character on the ground?</param>
        private void OnGrounded(bool grounded)
        {
            if (!grounded) {
                ResetStoredInputs();
            }
        }

        /// <summary>
        /// The character's position or rotation has been teleported.
        /// </summary>
        /// <param name="snapAnimator">Should the animator be snapped?</param>
        private void OnImmediateTransformChange(bool snapAnimator)
        {
            m_AccumulateInputs = false;
            ResetStoredInputs();
        }

        /// <summary>
        /// Resets the internal input variables back to their starting values.
        /// </summary>
        protected void ResetStoredInputs()
        {
            m_InputIndex = -1;
            m_InputCount = 0;
        }

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterGrounded", OnGrounded);
            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterImmediateTransformChange", OnImmediateTransformChange);
            EventHandler.UnregisterEvent(m_GameObject, "OnStoredInputAbilityResetStoredInputs", ResetStoredInputs);
        }
    }
}