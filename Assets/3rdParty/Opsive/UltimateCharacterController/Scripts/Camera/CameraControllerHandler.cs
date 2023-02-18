/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Camera
{
    using Opsive.Shared.Events;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Input;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The CameraControllerHandler manages player input and the CameraController.
    /// </summary>
    public class CameraControllerHandler : MonoBehaviour
    {
        [Tooltip("The name of the zoom input mapping.")]
        [SerializeField] protected string m_ZoomInputName = "Fire2";
        [Tooltip("Does the zoom button need to be held down in order for the camera to zoom?")]
        [SerializeField] protected bool m_ContinuousZoom = true;
#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
        [Tooltip("The name of the toggle perspective input mapping.")]
        [SerializeField] protected string m_TogglePerspectiveInputName = "Toggle Perspective";
#endif

        public string ZoomInputName { get { return m_ZoomInputName; } set { m_ZoomInputName = value; } }
        public bool ContinuousZoom { get { return m_ContinuousZoom; } set { m_ContinuousZoom = value; } }
#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
        public string TogglePerspectiveInputName { get { return m_TogglePerspectiveInputName; } set { m_TogglePerspectiveInputName = value; } }
#endif

        private GameObject m_GameObject;
        private CameraController m_CameraController;
        private GameObject m_Character;
        private PlayerInput m_PlayerInput;
        private bool m_AllowGameplayInput;
        private List<ActiveInputEvent> m_ActiveInputList;

        /// <summary>
        /// Initializes the handler.
        /// </summary>
        protected virtual void Awake()
        {
            m_GameObject = gameObject;
            m_CameraController = GetComponent<CameraController>();

            EventHandler.RegisterEvent<GameObject>(gameObject, "OnCameraAttachCharacter", OnAttachCharacter);

            // The component will become enabled after a character has been attached.
            enabled = false;
        }

        /// <summary>
        /// Attaches the handler to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the handler to.</param>
        private void OnAttachCharacter(GameObject character)
        {
            enabled = character != null;

            if (m_Character != null) {
                EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
                EventHandler.UnregisterEvent(m_Character, "OnRespawn", OnRespawn);
                EventHandler.UnregisterEvent<bool>(m_Character, "OnEnableGameplayInput", OnEnableGameplayInput);
                EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterActivate", OnActivate);
            }

            m_Character = character;

            if (character != null) {
                EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(character, "OnDeath", OnDeath);
                EventHandler.RegisterEvent(character, "OnRespawn", OnRespawn);
                EventHandler.RegisterEvent<bool>(character, "OnEnableGameplayInput", OnEnableGameplayInput);
                EventHandler.RegisterEvent<bool>(character, "OnCharacterActivate", OnActivate);
                m_AllowGameplayInput = true;
                m_PlayerInput = character.GetComponent<PlayerInput>();
                enabled = character.activeInHierarchy;
            }
        }

        /// <summary>
        /// The handler has been disabled.
        /// </summary>
        private void OnDisable()
        {
            if (m_CameraController.KinematicObjectIndex == -1) {
                return;
            }

            KinematicObjectManager.SetCameraLookVector(m_CameraController.KinematicObjectIndex, Vector2.zero);
        }

        /// <summary>
        /// Handles the player input.
        /// </summary>
        private void Update()
        {
            bool zoom;
            if (m_ContinuousZoom) {
                zoom = m_PlayerInput.GetButton(m_ZoomInputName);
            } else {
                zoom = m_PlayerInput.GetButtonDown(m_ZoomInputName) ? !m_CameraController.ZoomInput : m_CameraController.ZoomInput;
            }

            if (zoom != m_CameraController.ZoomInput) {
                m_CameraController.TryZoom(zoom);
            }

#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
            if (m_CameraController.CanChangePerspectives && m_PlayerInput.GetButtonDown(m_TogglePerspectiveInputName)) {
                m_CameraController.TogglePerspective();
            }
#endif

            // ViewTypes can listen for their own input.
            if (m_ActiveInputList != null) {
                for (int i = 0; i < m_ActiveInputList.Count; ++i) {
                    // Execute the event as soon as the input type becomes true.
                    if (m_ActiveInputList[i].HasButtonEvent(m_PlayerInput)) {
                        ExecuteInputEvent(m_ActiveInputList[i].EventName);
                    } else if (m_ActiveInputList[i].HasAxisEvent(m_PlayerInput)) {
                        ExecuteInputEvent(m_ActiveInputList[i].EventName, m_ActiveInputList[i].GetAxisValue(m_PlayerInput));
                    }
                }
            }
        }

        /// <summary>
        /// Sets the look vector of the camera.
        /// </summary>
        private void FixedUpdate()
        {
            var lookVector = m_PlayerInput.GetLookVector(true);
            KinematicObjectManager.SetCameraLookVector(m_CameraController.KinematicObjectIndex, lookVector);
        }

        /// <summary>
        /// Register an input event which allows the view type to receive button callbacks while it is active.
        /// </summary>
        /// <param name="inputEvent">The input event object to register.</param>
        public virtual void RegisterInputEvent(ActiveInputEvent inputEvent)
        {
            if (m_ActiveInputList == null) {
                m_ActiveInputList = new List<ActiveInputEvent>();
            }
            m_ActiveInputList.Add(inputEvent);
        }

        /// <summary>
        /// Unregister the specified input event.
        /// </summary>
        /// <param name="inputEvent">The input event object to unregister.</param>
        public void UnregisterAbilityInputEvent(ActiveInputEvent inputEvent)
        {
            m_ActiveInputList.Remove(inputEvent);
        }

        /// <summary>
        /// Executes the input event.
        /// </summary>
        /// <param name="eventName">The input event name.</param>
        protected virtual void ExecuteInputEvent(string eventName)
        {
            EventHandler.ExecuteEvent(m_GameObject, eventName);
        }

        /// <summary>
        /// Executes the axis input event.
        /// </summary>
        /// <param name="eventName">The input event name.</param>
        /// <param name="axisValue">The value of the axis.</param>
        protected virtual void ExecuteInputEvent(string eventName, float axisValue)
        {
            EventHandler.ExecuteEvent<float>(m_GameObject, eventName, axisValue);
        }

        /// <summary>
        /// The character has died. Disable the component.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            m_CameraController.TryZoom(false);
            enabled = false;
        }

        /// <summary>
        /// The character has respawned. Enable the component.
        /// </summary>
        private void OnRespawn()
        {
            enabled = true;
        }

        /// <summary>
        /// Enables or disables gameplay input. An example of when it will not be enabled is when there is a fullscreen UI over the main camera.
        /// </summary>
        /// <param name="enable">True if the input is enabled.</param>
        private void OnEnableGameplayInput(bool enable)
        {
            m_AllowGameplayInput = enable;
            enabled = m_AllowGameplayInput && m_Character != null && m_Character.activeInHierarchy;

            if (!enabled) {
                KinematicObjectManager.SetCameraLookVector(m_CameraController.KinematicObjectIndex, Vector2.zero);
            }
        }

        /// <summary>
        /// The character has been activated or deactivated.
        /// </summary>
        /// <param name="activate">True if the character has been activated.</param>
        private void OnActivate(bool activate)
        {
            enabled = m_AllowGameplayInput && m_Character != null && activate;
        }

        /// <summary>
        /// The camera has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            OnAttachCharacter(null);

            EventHandler.UnregisterEvent<GameObject>(gameObject, "OnCameraAttachCharacter", OnAttachCharacter);
        }
    }
}