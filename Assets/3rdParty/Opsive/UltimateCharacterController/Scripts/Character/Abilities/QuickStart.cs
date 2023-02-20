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
    /// The Start Movement ability allows the character to play a starting animation.
    /// </summary>
    [DefaultStopType(AbilityStopType.Automatic)]
    [DefaultAbilityIndex(6)]
    public class QuickStart : StoredInputAbilityBase
    {
        [Tooltip("The value which differentiates between a walk and a run.")]
        [SerializeField] protected float m_SpeedChangeThreshold = 1;

        public float SpeedChangeThreshold { get { return m_SpeedChangeThreshold; } set { m_SpeedChangeThreshold = value; } }

        private enum StartIndex { None, WalkForward, WalkForwardTurnLeft, WalkForwardTurnRight, WalkStrafeLeft, WalkStrafeRight, WalkBackward, WalkBackwardTurnLeft, WalkBackwardTurnRight, RunForward, RunForwardTurnLeft, RunForwardTurnRight, RunStrafeLeft, RunStrafeRight, RunBackward, RunBackwardTurnLeft, RunBackwardTurnRight }

        private bool m_CanStart;
        private int m_StartIndex;
        private bool m_EventStop;

        protected override bool UseRawInput { get { return true; } }
        protected override bool RequireInput { get { return false; } }
        public override int AbilityIntData { get { return m_StartIndex; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorStartMovementComplete", OnStartComplete);
            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterMoving", OnMoving);
        }

        /// <summary>
        /// The character has started to or stopped moving.
        /// </summary>
        /// <param name="moving">Is the character moving?</param>
        private void OnMoving(bool moving)
        {
            if (!moving) {
                // The ability can't start until the character is no longer moving. This will prevent the ability from starting repeatedly 
                // while the character is moving.
                m_CanStart = true;
            }
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

            // The ability can't start if the character is stopped.
            if (m_CharacterLocomotion.InputVector.sqrMagnitude == 0) {
                return false;
            }

            // If the input count is greater than zero then the character has been moving already so the ability should not be started.
            if (m_InputCount > 0) {
                m_CanStart = false;
            }

            return m_CanStart;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            // The start index is based on the input value.
            var inputValue = m_CharacterLocomotion.InputVector;
            m_StartIndex = (int)StartIndex.None;
            if (inputValue.x > m_SpeedChangeThreshold && inputValue.y > m_SpeedChangeThreshold) {
                m_StartIndex = (int)StartIndex.RunForwardTurnRight;
            } else if (inputValue.x > 0 && inputValue.y > 0) {
                m_StartIndex = (int)StartIndex.WalkForwardTurnRight;
            } else if (inputValue.x < -m_SpeedChangeThreshold && inputValue.y > m_SpeedChangeThreshold) {
                m_StartIndex = (int)StartIndex.RunForwardTurnLeft;
            } else if (inputValue.x < 0 && inputValue.y > 0) {
                m_StartIndex = (int)StartIndex.WalkForwardTurnLeft;
            } else if (inputValue.x < -m_SpeedChangeThreshold && inputValue.y < -m_SpeedChangeThreshold) {
                m_StartIndex = (int)StartIndex.RunBackwardTurnLeft;
            } else if (inputValue.x < 0 && inputValue.y < 0) {
                m_StartIndex = (int)StartIndex.WalkBackwardTurnLeft;
            } else if (inputValue.x > m_SpeedChangeThreshold && inputValue.y < -m_SpeedChangeThreshold) {
                m_StartIndex = (int)StartIndex.RunBackwardTurnRight;
            } else if (inputValue.x > 0 && inputValue.y < 0) {
                m_StartIndex = (int)StartIndex.WalkBackwardTurnRight;
            } else if (inputValue.y > m_SpeedChangeThreshold) {
                m_StartIndex = (int)StartIndex.RunForward;
            } else if (inputValue.y > 0) {
                m_StartIndex = (int)StartIndex.WalkForward;
            } else if (inputValue.y < -m_SpeedChangeThreshold) {
                m_StartIndex = (int)StartIndex.RunBackward;
            } else if (inputValue.y < 0) {
                m_StartIndex = (int)StartIndex.WalkBackward;
            } else if (inputValue.x > m_SpeedChangeThreshold) {
                m_StartIndex = (int)StartIndex.RunStrafeRight;
            } else if (inputValue.x > 0) {
                m_StartIndex = (int)StartIndex.WalkStrafeRight;
            } else if (inputValue.x < -m_SpeedChangeThreshold) {
                m_StartIndex = (int)StartIndex.RunStrafeLeft;
            } else if (inputValue.x < 0) {
                m_StartIndex = (int)StartIndex.WalkStrafeLeft;
            }

            m_EventStop = false;
            m_CanStart = false;

            base.AbilityStarted();
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility()
        {
            return m_EventStop || m_CharacterLocomotion.RawInputVector.sqrMagnitude == 0;
        }

        /// <summary>
        /// Animation event callback when the start animation has completed.
        /// </summary>
        private void OnStartComplete()
        {
            m_EventStop = true;
        }

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorStartMovementComplete", OnStartComplete);
            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterMoving", OnMoving);
        }
    }
}