/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Camera;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.Abilities.Items;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.StateSystem;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Demo.UI
{
    /// <summary>
    /// Manages the first person springs zone. Allows switching between various spring types.
    /// </summary>
    public class FirstPersonSpringZone : UIZone
    {
        private enum SpringType { Modern, OldSchool, CrazyCowboy, Astronaut, DrunkPerson, Giant, SpringsOff, None }

        [Tooltip("A reference to the character used for Astronaut and Drunk Person.")]
        [SerializeField] protected GameObject m_DrunkAstronautCharacter;
        [Tooltip("A reference to the character used for Giant.")]
        [SerializeField] protected GameObject m_GiantCharacter;
        [Tooltip("The ItemTypes that the character should have before entering the room.")]
        [SerializeField] protected ItemType[] m_ItemTypes;
        [Tooltip("The index of the primary category.")]
        [SerializeField] protected int m_CategoryIndex = 0;
        [Tooltip("The ItemSet index that the character should switch to.")]
        [SerializeField] protected int m_ItemSetIndex = 0;

        private GameObject m_Character;
        private CameraController m_CameraController;

        private SpringType m_SpringType = SpringType.None;

        private Vector3 m_DrunkAstronautPosition;
        private Quaternion m_DrunkAstronautRotation;
        private Vector3 m_GiantPosition;
        private Quaternion m_GiantRotation;

        private ScheduledEventBase m_EnableStateEvent;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Remember the positions/rotations so they can be restored when the character leaves the zone.
            m_DrunkAstronautPosition = m_DrunkAstronautCharacter.transform.position;
            m_DrunkAstronautRotation = m_DrunkAstronautCharacter.transform.rotation;
            m_GiantPosition = m_GiantCharacter.transform.position;
            m_GiantRotation = m_GiantCharacter.transform.rotation;

            ChangeSpringType(SpringType.Modern);
            m_DrunkAstronautCharacter.SetActive(false);
            m_GiantCharacter.SetActive(false);
        }

        /// <summary>
        /// Change the spring types to the specified type.
        /// </summary>
        /// <param name="type">The type to change the value to.</param>
        public void ChangeSpringType(int type)
        {
            ChangeSpringType((SpringType)type);
        }

        /// <summary>
        /// Change the spring types to the specified type.
        /// </summary>
        /// <param name="type">The type to change the value to.</param>
        private void ChangeSpringType(SpringType type)
        {
            // Don't switch types if the type is equal to the current type or if the enable state event is active. This will occur if a new button is pressed
            // within the time that it takes to invoke the scheduled event.
            if (m_SpringType == type || m_EnableStateEvent != null) {
                return;
            }

            // Reset the button color and deactivate the previous character. The same character may be activated again depending on the spring type.
            UnityEngine.UI.ColorBlock buttonColors;
            if (m_SpringType != SpringType.None) {
                m_ButtonImages[(int)m_SpringType].color = m_NormalColor;
                buttonColors = m_Buttons[(int)m_SpringType].colors;
                buttonColors.normalColor = m_NormalColor;
                m_Buttons[(int)m_SpringType].colors = buttonColors;
                if (m_ActiveCharacter != null) {
                    m_ActiveCharacter.SetActive(false);
                    StateManager.SetState(m_ActiveCharacter, System.Enum.GetName(typeof(SpringType), m_SpringType), false);
                }
            }

            // Remember the old spring type and activate the new. The button should reflect the selected spring type.
            var prevSpringType = m_SpringType;
            m_SpringType = type;
            m_ButtonImages[(int)m_SpringType].color = m_PressedColor;
            buttonColors = m_Buttons[(int)m_SpringType].colors;
            buttonColors.normalColor = m_PressedColor;
            m_Buttons[(int)m_SpringType].colors = buttonColors;

            // If the previous spring type isn't None then a button was pressed. Activate the new character.
            if (prevSpringType != SpringType.None) {
                var prevCharacter = m_ActiveCharacter;
                
                // The active character depends on the spring type.
                if (m_SpringType == SpringType.Astronaut || m_SpringType == SpringType.DrunkPerson) {
                    m_ActiveCharacter = m_DrunkAstronautCharacter;
                } else if (m_SpringType == SpringType.Giant) {
                    m_ActiveCharacter = m_GiantCharacter;
                } else {
                    m_ActiveCharacter = m_Character;
                }

                // Activate the correct character and set the camera to the character if that character changed. This shouldn't be done if the character didn't
                // change so the camera doesn't snap into position.
                var characterChange = m_ActiveCharacter != prevCharacter;
                m_ActiveCharacter.SetActive(true);
                if (characterChange) {
                    m_CameraController.Character = m_ActiveCharacter;
                }

                // Wait a small amount of time if the springs are off so the item can get back into the correct position while the springs are enabled.
                m_EnableStateEvent = Scheduler.Schedule(m_SpringType == SpringType.SpringsOff ? 0.4f : 0, EnableSpringState, characterChange);
            }

            EnableInput();
        }

        /// <summary>
        /// Enables the current spring type on the active character.
        /// </summary>
        /// <param name="characterChange">Did the character GameObject changed?</param>
        private void EnableSpringState(bool characterChange)
        {
            StateManager.SetState(m_ActiveCharacter, System.Enum.GetName(typeof(SpringType), m_SpringType), true);
            m_EnableStateEvent = null;
            // If the character changed then the camera should adjust immediately to the new position. This will allow the camera to jump to the correct position
            // when switching to the giant.
            if (characterChange) {
                m_CameraController.PositionImmediately();
            }
        }

        /// <summary>
        /// The character has entered from the zone.
        /// </summary>
        /// <param name="characterLocomotion">The character that entered the zone.</param>
        protected override void CharacterEnter(UltimateCharacterLocomotion characterLocomotion)
        {
            // The other collider is the main character.
            m_Character = characterLocomotion.gameObject;
            m_CameraController = UnityEngineUtility.FindCamera(m_Character).GetComponent<CameraController>();

            // The character must have the primary item in order for it to be equipped.
            var inventory = m_Character.GetCachedComponent<InventoryBase>();
            for (int i = 0; i < m_ItemTypes.Length; ++i) {
                if (m_ItemTypes[i] == null) {
                    continue;
                }
                inventory.PickupItemType(m_ItemTypes[i], 1, 0, false, false);
            }

            // Ensure the primary weapon is equipped.
            var equipUnequipAbilities = characterLocomotion.GetAbilities<EquipUnequip>();
            for (int i = 0; i < equipUnequipAbilities.Length; ++i) {
                if (equipUnequipAbilities[i].ItemSetCategoryIndex == m_CategoryIndex) {
                    equipUnequipAbilities[i].StartEquipUnequip(m_ItemSetIndex, true);
                    break;
                }
            }

            // Setup the character for the zone.
            StateManager.SetState(m_Character, "FirstPersonSpringZone", true);
            EventHandler.ExecuteEvent(m_Character, "OnShowUI", false);

            // First person perspective is required.
            m_CameraController.SetPerspective(true);
        }

        /// <summary>
        /// The character has exited from the zone.
        /// </summary>
        /// <param name="characterLocomotion">The character that exited the zone.</param>
        protected override void CharacterExit(UltimateCharacterLocomotion characterLocomotion)
        {
            // The drunk astronaut or giant GameObject may have left the trigger. Set the normal character to the other character's position
            // so when switching back to the normal character they will be in the same position.
            if (characterLocomotion.gameObject != m_Character) {
                m_Character.GetComponent<UltimateCharacterLocomotion>().SetPositionAndRotation(characterLocomotion.transform.position, characterLocomotion.transform.rotation);
            }
            // The modern type should activate when leaving the zone.
            ChangeSpringType(SpringType.Modern);

            // Disable the states that were enabled when entering the zone.
            StateManager.SetState(m_Character, "FirstPersonSpringZone", false);
            EventHandler.ExecuteEvent(m_Character, "OnShowUI", true);

            // Restore the other character positions/rotations.
            m_DrunkAstronautCharacter.GetComponent<UltimateCharacterLocomotion>().SetPositionAndRotation(m_DrunkAstronautPosition, m_DrunkAstronautRotation);
            m_GiantCharacter.GetComponent<UltimateCharacterLocomotion>().SetPositionAndRotation(m_GiantPosition, m_GiantRotation);
        }
    }
}