/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Input;

namespace Opsive.UltimateCharacterController.Demo.UI
{
    /// <summary>
    /// Toggles the UI that shows the help for the control mappings.
    /// </summary>
    public class ToggleControlHelp : MonoBehaviour
    {
        [Tooltip("The UI that should be toggled.")]
        [SerializeField] protected GameObject m_ControlUI;
        [Tooltip("The keycode that should toggle the UI.")]
        [SerializeField] protected KeyCode m_ToggleKeyCode = KeyCode.F1;
        [Tooltip("A reference to the keyboard controls sprite.")]
        [SerializeField] protected GameObject m_KeyboardUI;
        [Tooltip("A reference to the controller controls sprite.")]
        [SerializeField] protected GameObject m_ControllerUI;
        [Tooltip("A reference to the in game Show Controls text.")]
        [SerializeField] protected Text m_InGameText;

        private GameObject m_Character;
        private PlayerInput m_PlayerInput;
        private bool m_Active;
        private float m_PrevTimeScale;

        /// <summary>
        /// The UI should start disabled.
        /// </summary>
        private void Awake()
        {
            m_ControlUI.SetActive(m_Active);
            var demoManager = GetComponent<DemoManager>();
            m_Character = demoManager.Character;
            m_PlayerInput = m_Character.GetComponent<PlayerInput>();
            OnControllerConnected(m_PlayerInput.ControllerConnected);
            EventHandler.RegisterEvent<bool>(m_Character, "OnInputControllerConnected", OnControllerConnected);
        }

        /// <summary>
        /// Toggles the UI.
        /// </summary>
        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(m_ToggleKeyCode) || UnityEngine.Input.GetKeyDown(KeyCode.Joystick1Button6) || (m_Active && UnityEngine.Input.GetKeyDown(KeyCode.Escape))) {
                m_Active = !m_Active;
                m_ControlUI.SetActive(m_Active);
                if (m_Active) {
                    m_PrevTimeScale = Time.timeScale;
                    Time.timeScale = 0;
                    m_KeyboardUI.SetActive(!m_PlayerInput.ControllerConnected);
                    m_ControllerUI.SetActive(m_PlayerInput.ControllerConnected);
                } else {
                    Time.timeScale = m_PrevTimeScale;
                }
            }
        }

        /// <summary>
        /// A controller has been connected or disconnected.
        /// </summary>
        /// <param name="controllerConnected">True if a controller has been connected.</param>
        private void OnControllerConnected(bool controllerConnected)
        {
            m_InGameText.text = string.Format("PRESS {0} FOR CONTROLS", controllerConnected ? "BACK" : "F1");

            if (!m_Active) {
                return;
            }

            m_KeyboardUI.SetActive(controllerConnected);
            m_ControllerUI.SetActive(controllerConnected);
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<bool>(m_Character, "OnInputControllerConnected", OnControllerConnected);
        }
    }
}