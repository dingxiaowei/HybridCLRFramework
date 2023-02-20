﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.StateSystem;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.UI
{
    /// <summary>
    /// The CameraMonitor component allows for UI elements to mapped to a specific character (allowing for split screen and coop).
    /// </summary>
    public abstract class CharacterMonitor : StateBehavior
    {
        [Tooltip("The character that uses the UI represents. Can be null.")]
        [SerializeField] protected GameObject m_Character;

        public GameObject Character { get { return m_Character; } set { OnAttachCharacter(value); } }

        protected bool m_ShowUI = true;
        protected GameObject m_CameraGameObject;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            var camera = UnityEngineUtility.FindCamera(m_Character);
            if (camera != null) {
                m_CameraGameObject = camera.gameObject;
                if (m_Character == null) {
                    m_Character = m_CameraGameObject.GetCachedComponent<Camera.CameraController>().Character;
                }
                EventHandler.RegisterEvent<GameObject>(m_CameraGameObject, "OnCameraAttachCharacter", OnAttachCharacter);
            }

            // Start disabled - attaching the character will enabe the component.
            enabled = false;

            if (m_Character != null) {
                var character = m_Character;
                m_Character = null;
                OnAttachCharacter(character);
            }
        }

        /// <summary>
        /// Attaches the monitor to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the monitor to.</param>
        protected virtual void OnAttachCharacter(GameObject character)
        {
            if (m_Character == character) {
                return;
            }

            if (m_Character != null) {
                StateManager.LinkGameObjects(m_Character, gameObject, false);
                EventHandler.UnregisterEvent<bool>(m_Character, "OnShowUI", ShowUI);
            }

            m_Character = character;

            if (m_Character != null) {
                StateManager.LinkGameObjects(m_Character, gameObject, true);
                EventHandler.RegisterEvent<bool>(m_Character, "OnShowUI", ShowUI);
            }

            // The monitor may be in the process of being destroyed.
            enabled = m_Character != null;
        }

        /// <summary>
        /// Shows or hides the UI.
        /// </summary>
        /// <param name="show">Should the UI be shown?</param>
        private void ShowUI(bool show)
        {
            m_ShowUI = show;
            gameObject.SetActive(show && CanShowUI());
        }

        /// <summary>
        /// Can the UI be shown?
        /// </summary>
        /// <returns>True if the UI can be shown.</returns>
        protected virtual bool CanShowUI()
        {
            return true;
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (m_CameraGameObject != null) {
                EventHandler.UnregisterEvent<GameObject>(m_CameraGameObject, "OnCameraAttachCharacter", OnAttachCharacter);
            }
            OnAttachCharacter(null);
        }
    }
}