/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Traits;
    using Opsive.UltimateCharacterController.StateSystem;
    using UnityEngine;

    /// <summary>
    /// Allows the character to switch between movement types.
    /// </summary>
    public class MovementTypeSwitcher : MonoBehaviour
    {
        [Tooltip("The keycode that should trigger the switch.")]
        [SerializeField] protected KeyCode m_SwitchKeycode = KeyCode.Return;
        [Tooltip("Can the top down and 2.5D movement type be switched to with a third person perspective?")]
        [SerializeField] protected bool m_IncludeTopDownPseudo3D;

        private string[] m_FirstPersonMovementStates = new string[] { "FirstPersonCombat", "FreeLook" };
        private string[] m_ThirdPersonMovementStates = new string[] { "Adventure", "ThirdPersonCombat", "RPG", "TopDown", "2.5D" };

        private GameObject m_Character;
        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private CharacterHealth m_CharacterHealth;

        private int m_ActiveFirstPersonIndex;
        private int m_ActiveThirdPersonIndex;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Awake()
        {
            m_Character = FindObjectOfType<DemoManager>().Character;
            m_CharacterLocomotion = m_Character.GetComponent<UltimateCharacterLocomotion>();
            m_CharacterHealth = m_Character.GetComponent<CharacterHealth>();

            EventHandler.RegisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);

            // The character may start out with the First Person Combat / Third Person Adventure Movement Type.
            if (m_CharacterLocomotion.FirstPersonMovementTypeFullName.Contains("FreeLook")) {
                m_ActiveFirstPersonIndex = 1;
            }
            if (m_CharacterLocomotion.ThirdPersonMovementTypeFullName.Contains("Combat")) {
                m_ActiveThirdPersonIndex = 1;
            } else if (m_CharacterLocomotion.ThirdPersonMovementTypeFullName.Contains("RPG")) {
                m_ActiveThirdPersonIndex = 2;
            }

            if (m_CharacterLocomotion.FirstPersonPerspective) {
                StateManager.SetState(m_Character, m_FirstPersonMovementStates[m_ActiveFirstPersonIndex], true);
            } else {
                StateManager.SetState(m_Character, m_ThirdPersonMovementStates[m_ActiveThirdPersonIndex], true);
            }
        }

        /// <summary>
        /// Switches the movement type when the specified keycode is pressed.
        /// </summary>
        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(m_SwitchKeycode)) {
                // The character needs to be alive to switch movement types.
                if (!m_CharacterHealth.IsAlive()) {
                    return;
                }

                // The movement type cannot be switched if ride or drive is active.
                if (m_CharacterLocomotion.IsAbilityTypeActive<UltimateCharacterController.Character.Abilities.Ride>() ||
                    m_CharacterLocomotion.IsAbilityTypeActive<UltimateCharacterController.Character.Abilities.Drive>()) {
                    return;
                }

                if (m_CharacterLocomotion.FirstPersonPerspective) {
                    UpdateMovementType(true, (m_ActiveFirstPersonIndex + 1) % m_FirstPersonMovementStates.Length);
                } else {
#if THIRD_PERSON_CONTROLLER
                    if (!m_IncludeTopDownPseudo3D) {
                        // The state cannot be switched if the top down or 2.5D movement type is active.
                        if (m_CharacterLocomotion.ActiveMovementType is ThirdPersonController.Character.MovementTypes.TopDown ||
                            m_CharacterLocomotion.ActiveMovementType is ThirdPersonController.Character.MovementTypes.Pseudo3D) {
                            return;
                        }
                    }
#endif
                    UpdateMovementType(false, (m_ActiveThirdPersonIndex + 1) % (m_ThirdPersonMovementStates.Length - (m_IncludeTopDownPseudo3D ? 0 : 2)));
                }
            }
        }

        /// <summary>
        /// Updates the movement type's index with the specified perspective.
        /// </summary>
        /// <param name="firstPersonPerspective">Should the first person perspective's movement type be updated?</param>
        /// <param name="index">The index to update the perspective to.</param>
        public void UpdateMovementType(bool firstPersonPerspective, int index)
        {
            if (firstPersonPerspective) {
                StateManager.SetState(m_Character, m_FirstPersonMovementStates[m_ActiveFirstPersonIndex], false);
                m_ActiveFirstPersonIndex = index;
                StateManager.SetState(m_Character, m_FirstPersonMovementStates[m_ActiveFirstPersonIndex], true);
            } else {
                StateManager.SetState(m_Character, m_ThirdPersonMovementStates[m_ActiveThirdPersonIndex], false);
                m_ActiveThirdPersonIndex = index;
                StateManager.SetState(m_Character, m_ThirdPersonMovementStates[m_ActiveThirdPersonIndex], true);
            }
        }

        /// <summary>
        /// The camera perspective between first and third person has changed.
        /// </summary>
        /// <param name="inFirstPerson">Is the camera in a first person view?</param>
        private void OnChangePerspectives(bool firstPersonPerspective)
        {
            // Wait a frame before changing states to prevent the movement type from switching the same frame the movement type is currently being switched.
            Scheduler.ScheduleFixed(Time.fixedDeltaTime, UpdateStates, firstPersonPerspective);
        }

        /// <summary>
        /// Updates the states depending on the perspective that was switched.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the camera in a first person view?</param>
        private void UpdateStates(bool firstPersonPerspective)
        {
            if (firstPersonPerspective) {
                StateManager.SetState(m_Character, m_ThirdPersonMovementStates[m_ActiveThirdPersonIndex], false);
                StateManager.SetState(m_Character, m_FirstPersonMovementStates[m_ActiveFirstPersonIndex], true);
            } else {
                StateManager.SetState(m_Character, m_FirstPersonMovementStates[m_ActiveFirstPersonIndex], false);
                StateManager.SetState(m_Character, m_ThirdPersonMovementStates[m_ActiveThirdPersonIndex], true);
            }
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (m_Character != null) {
                EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
            }
        }
    }
}