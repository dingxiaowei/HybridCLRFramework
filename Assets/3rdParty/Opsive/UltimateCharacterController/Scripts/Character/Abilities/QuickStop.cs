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
    /// The Stop Movement ability allows the character to perform a sudden stop animation.
    /// </summary>
    [DefaultStopType(AbilityStopType.Automatic)]
    [DefaultAbilityIndex(7)]
    public class QuickStop : StoredInputAbilityBase
    {
        [Tooltip("The value which differentiates between a walk and a run.")]
        [SerializeField] protected float m_SpeedChangeThreshold = 1;
        [Tooltip("The threshold which indicates when the character has stopped.")]
        [SerializeField] protected float m_StopThreshold = 0.01f;
        [Tooltip("The number of times CanStartAbility must return true before the ability actually starts. Will prevent the ability from starting too soon.")]
        [SerializeField] protected int m_RequiredStartSuccessCount = 2;

        public float SpeedChangeThreshold { get { return m_SpeedChangeThreshold; } set { m_SpeedChangeThreshold = value; } }
        public float StopThreshold { get { return m_StopThreshold; } set { m_StopThreshold = value; } }
        public int RequiredStartSuccessCount { get { return m_RequiredStartSuccessCount; } set { m_RequiredStartSuccessCount = value; } }

        private enum StopIndex { None, WalkForward, WalkForwardTurnLeft, WalkForwardTurnRight, WalkStrafeLeft, WalkStrafeRight, WalkBackward, WalkBackwardTurnLeft, WalkBackwardTurnRight, RunForward, RunForwardTurnLeft, RunForwardTurnRight, RunStrafeLeft, RunStrafeRight, RunBackward, RunBackwardTurnLeft, RunBackwardTurnRight }

        private int m_StartSuccessCount;
        private Vector2 m_AverageInput;
        private int m_StopIndex;
        private bool m_EventStop;

        protected override bool UseRawInput { get { return false; } }
        protected override bool RequireInput { get { return true; } }
        public override int AbilityIntData { get { return m_StopIndex; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_StartSuccessCount = m_RequiredStartSuccessCount;
            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorStopMovementComplete", OnStopComplete);
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

            // The input must be moving towards 0 in order to stop.
            var input = m_CharacterLocomotion.RawInputVector;
            if (Mathf.Abs(input.x) > Mathf.Abs(m_Inputs[m_InputIndex].x) ||
                Mathf.Abs(input.y) > Mathf.Abs(m_Inputs[m_InputIndex].y)) {
                return false;
            }

            // There should be minimal current input.
            if (m_CharacterLocomotion.RawInputVector.sqrMagnitude > m_StopThreshold) {
                return false;
            }

            // The character has to have been moving in order to stop.
            m_AverageInput = Vector2.zero;
            for (int i = 0; i < m_InputCount; ++i) {
                m_AverageInput += m_Inputs[i];
            }
            m_AverageInput /= m_InputCount;
            if (m_AverageInput.sqrMagnitude < 0.01f) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            // The stop index is based on the average input value.
            m_StopIndex = (int)StopIndex.None;
            if (m_StartSuccessCount == 0) {
                DeterminStopIndex();
            }

            m_EventStop = false;

            base.AbilityStarted();
        }

        /// <summary>
        /// Determines which stop index should be set based on the current input.
        /// </summary>
        private void DeterminStopIndex()
        {
            if (m_AverageInput.x > m_SpeedChangeThreshold && m_AverageInput.y > m_SpeedChangeThreshold) {
                m_StopIndex = (int)StopIndex.RunForwardTurnRight;
            } else if (m_AverageInput.x > 0 && m_AverageInput.y > 0) {
                m_StopIndex = (int)StopIndex.WalkForwardTurnRight;
            } else if (m_AverageInput.x < -m_SpeedChangeThreshold && m_AverageInput.y > m_SpeedChangeThreshold) {
                m_StopIndex = (int)StopIndex.RunForwardTurnLeft;
            } else if (m_AverageInput.x < 0 && m_AverageInput.y > 0) {
                m_StopIndex = (int)StopIndex.WalkForwardTurnLeft;
            } else if (m_AverageInput.x < -m_SpeedChangeThreshold && m_AverageInput.y < -m_SpeedChangeThreshold) {
                m_StopIndex = (int)StopIndex.RunBackwardTurnLeft;
            } else if (m_AverageInput.x < 0 && m_AverageInput.y < 0) {
                m_StopIndex = (int)StopIndex.WalkBackwardTurnLeft;
            } else if (m_AverageInput.x > m_SpeedChangeThreshold && m_AverageInput.y < -m_SpeedChangeThreshold) {
                m_StopIndex = (int)StopIndex.RunBackwardTurnRight;
            } else if (m_AverageInput.x > 0 && m_AverageInput.y < 0) {
                m_StopIndex = (int)StopIndex.WalkBackwardTurnRight;
            } else if (m_AverageInput.y > m_SpeedChangeThreshold) {
                m_StopIndex = (int)StopIndex.RunForward;
            } else if (m_AverageInput.y > 0) {
                m_StopIndex = (int)StopIndex.WalkForward;
            } else if (m_AverageInput.y < -m_SpeedChangeThreshold) {
                m_StopIndex = (int)StopIndex.RunBackward;
            } else if (m_AverageInput.y < 0) {
                m_StopIndex = (int)StopIndex.WalkBackward;
            } else if (m_AverageInput.x > m_SpeedChangeThreshold) {
                m_StopIndex = (int)StopIndex.RunStrafeRight;
            } else if (m_AverageInput.x > 0) {
                m_StopIndex = (int)StopIndex.WalkStrafeRight;
            } else if (m_AverageInput.x < -m_SpeedChangeThreshold) {
                m_StopIndex = (int)StopIndex.RunStrafeLeft;
            } else if (m_AverageInput.x < 0) {
                m_StopIndex = (int)StopIndex.WalkStrafeLeft;
            }
        }

        /// <summary>
        /// Updates the ability.
        /// </summary>
        public override void Update()
        {
            base.Update();

            // The ability may start before the character should actually start to play the stop animation. Keep the AbilityIntData set to 0 until the stop animation should actually stop.
            // There is a delay because the character may be doing a quick turn instead of actually stopping.
            if (m_StartSuccessCount > 0) {
                if (!CanStartAbility()) {
                    StopAbility();
                }
                m_StartSuccessCount--;
                if (m_StartSuccessCount == 0) {
                    DeterminStopIndex();
                    m_CharacterLocomotion.UpdateAbilityAnimatorParameters();
                }
            }
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility()
        {
            return m_EventStop || m_CharacterLocomotion.RawInputVector.sqrMagnitude > 0.001f;
        }

        /// <summary>
        /// Animation event callback when the stop animation has completed.
        /// </summary>
        private void OnStopComplete()
        {
            m_EventStop = true;
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            // If StartSuccessCount isn't 0 then the stop ability was never fully started.
            AbilityStopped(force, m_StartSuccessCount == 0);

            m_StartSuccessCount = m_RequiredStartSuccessCount;
        }

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorStopMovementComplete", OnStopComplete);
        }
    }
}