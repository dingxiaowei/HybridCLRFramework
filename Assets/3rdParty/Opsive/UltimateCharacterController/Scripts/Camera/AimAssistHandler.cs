/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Input;
using Opsive.UltimateCharacterController.Utility;
using Opsive.UltimateCharacterController.StateSystem;

namespace Opsive.UltimateCharacterController.Camera
{
    /// <summary>
    /// Handles the input for the AimAssist component.
    /// </summary>
    [RequireComponent(typeof(AimAssist))]
    public class AimAssistHandler : StateBehavior
    {
        [Tooltip("Can the targets be switched?")]
        [SerializeField] protected bool m_CanSwitchTargets;
        [Tooltip("The name of the button mapping for switching targets.")]
        [SerializeField] protected string m_SwitchTargetInputName = "Horizontal";
        [Tooltip("The minimum magnitude required to switch targets.")]
        [SerializeField] protected float m_SwitchTargetMagnitude = 0.8f;

        public bool CanSwitchTargets { get { return m_CanSwitchTargets; } set { m_CanSwitchTargets = value;
                                            if (Application.isPlaying) { enabled = m_PlayerInput != null && m_CanSwitchTargets; m_AllowTargetSwitch = true; } } }
        public string SwitchTargetInputName { get { return m_SwitchTargetInputName; } set { m_SwitchTargetInputName = value; } }
        public float SwitchTargetMagnitude { get { return m_SwitchTargetMagnitude; } set { m_SwitchTargetMagnitude = value; } }

        private AimAssist m_AimAssist;
        private PlayerInput m_PlayerInput;
        private bool m_AllowTargetSwitch = true;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_AimAssist = gameObject.GetComponent<AimAssist>();

            EventHandler.RegisterEvent<GameObject>(gameObject, "OnCameraAttachCharacter", OnAttachCharacter);

            // Enable after the character has been attached.
            enabled = false;
        }

        /// <summary>
        /// Attaches the component to the specified character.
        /// </summary>
        /// <param name="character">The handler to attach the camera to.</param>
        protected virtual void OnAttachCharacter(GameObject character)
        {
            m_PlayerInput = character != null ? character.GetCachedComponent<PlayerInput>() : null;

            enabled = m_PlayerInput != null && m_CanSwitchTargets;
        }

        /// <summary>
        /// Tries to switch targets if the input value is large enough.
        /// </summary>
        private void Update()
        {
            var value = m_PlayerInput.GetAxisRaw(m_SwitchTargetInputName);
            if (m_AllowTargetSwitch && Mathf.Abs(value) > m_SwitchTargetMagnitude) {
                m_AimAssist.TrySwitchTargets(value > 0);
                m_AllowTargetSwitch = false;
            } else if (!m_AllowTargetSwitch && Mathf.Abs(value) < 0.01f) {
                // Don't allow another target switch until the value is reset. This will prevent the target from quickly switching.
                m_AllowTargetSwitch = true;
            }
        }

        /// <summary>
        /// The camera has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<GameObject>(gameObject, "OnCameraAttachCharacter", OnAttachCharacter);
        }
    }
}