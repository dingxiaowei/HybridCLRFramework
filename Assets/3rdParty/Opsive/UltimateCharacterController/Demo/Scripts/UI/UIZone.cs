/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Demo.UI
{
    /// <summary>
    /// Abstract class which manages the UI for the demo zones.
    /// </summary>
    public abstract class UIZone : MonoBehaviour
    {
        [Tooltip("A reference to the UI parent GameObject.")]
        [SerializeField] protected GameObject m_UIParent;
        [Tooltip("A reference to the buttons that correspond to the InputTypes. These buttons must be in the same order as the enum.")]
        [SerializeField] protected Image[] m_ButtonImages;

        protected Color m_NormalColor;
        protected Color m_PressedColor;
        protected Button[] m_Buttons;

        protected GameObject m_ActiveCharacter;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected virtual void Awake()
        {
            m_Buttons = new Button[m_ButtonImages.Length];
            var firstIndex = -1;
            for (int i = 0; i < m_Buttons.Length; ++i) {
                if (m_ButtonImages[i] == null) {
                    continue;
                }
                if (firstIndex == -1) {
                    firstIndex = i;
                }
                m_Buttons[i] = m_ButtonImages[i].GetComponent<Button>();
            }
            m_NormalColor = m_Buttons[firstIndex].colors.normalColor;
            m_PressedColor = m_Buttons[firstIndex].colors.pressedColor;
            m_UIParent.SetActive(false);
        }

        /// <summary>
        /// An object has entered the trigger.
        /// </summary>
        /// <param name="other">The object that entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            // If the active character GameObject isn't null then the character is already within the trigger (and may just be activated again).
            if (m_ActiveCharacter != null || !MathUtility.InLayerMask(other.gameObject.layer, 1 << LayerManager.Character)) {
                return;
            }

            var characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            // The other collider is the main character.
            m_ActiveCharacter = characterLocomotion.gameObject;

            // The subclass can handle initializing the character. 
            CharacterEnter(characterLocomotion);

            // Enable the UI that is specific for the zone.
            m_UIParent.SetActive(true);
        }

        /// <summary>
        /// The character has entered from the zone.
        /// </summary>
        /// <param name="characterLocomotion">The character that entered the zone.</param>
        protected virtual void CharacterEnter(UltimateCharacterLocomotion characterLocomotion) { }

        /// <summary>
        /// Enables the input after a button has been selected.
        /// </summary>
        protected void EnableInput()
        {
            if (m_ActiveCharacter == null) {
                return;
            }

            // Give control back to the player and lock the cursor after a selection. It can be unlocked again by pressing escape.
            EventHandler.ExecuteEvent(m_ActiveCharacter, "OnEnableGameplayInput", true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>
        /// An object has exited the trigger.
        /// </summary>
        /// <param name="other">The collider that exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, 1 << LayerManager.Character)) {
                return;
            }

            var characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            // The subclass can handle resetting the states.
            CharacterExit(characterLocomotion);

            // Reset the UI and active character.
            m_UIParent.SetActive(false);
            m_ActiveCharacter = null;
        }

        /// <summary>
        /// The character has exited from the zone.
        /// </summary>
        /// <param name="characterLocomotion">The character that exited the zone.</param>
        protected virtual void CharacterExit(UltimateCharacterLocomotion characterLocomotion) { }
    }
}