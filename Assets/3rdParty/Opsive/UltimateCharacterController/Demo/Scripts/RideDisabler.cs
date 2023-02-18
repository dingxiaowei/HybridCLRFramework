/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo
{
    using Opsive.Shared.Events;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Demo.Objects;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Disables or removes objects when the ride ability is active.
    /// </summary>
    public class RideDisabler : MonoBehaviour
    {
        [Tooltip("A reference to Blitz.")]
        [SerializeField] protected GameObject m_Blitz;
        [Tooltip("The doors that should be locked when the ride ability is active.")]
        [SerializeField] protected Door[] m_Doors;
        [Tooltip("The objects that should be deactivated when the ride ability is active.")]
        [SerializeField] protected GameObject[] m_Objects;

        private GameObject m_Nolan;
        private bool m_RideActive;
        private bool m_BlitzInTrigger;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            var demoManager = FindObjectOfType<DemoManager>();
            m_Nolan = demoManager.Character;

            EventHandler.RegisterEvent<Ability, bool>(m_Nolan, "OnCharacterAbilityActive", OnAbilityActive);
            EventHandler.RegisterEvent<Transform>(m_Blitz, "OnCharacterChangeMovingPlatforms", OnCharacterChangeMovingPlatforms);
        }

        /// <summary>
        /// The character's ability has been started or stopped.
        /// </summary>
        /// <param name="ability">The ability which was started or stopped.</param>
        /// <param name="active">True if the ability was started, false if it was stopped.</param>
        private void OnAbilityActive(Ability ability, bool active)
        {
            if (!(ability is Ride)) {
                return;
            }

            // Lock the doors and disable the objects if the ride ability starts to restrict the locations that the rideable object can move.
            for (int i = 0; i < m_Doors.Length; ++i) {
                m_Doors[i].Locked = active;
            }
            for (int i = 0; i < m_Objects.Length; ++i) {
                m_Objects[i].SetActive(!active && !m_BlitzInTrigger);
            }
            m_RideActive = active;
        }

        /// <summary>
        /// The character's moving platform object has changed.
        /// </summary>
        /// <param name="movingPlatform">The moving platform to set. Can be null.</param>
        private void OnCharacterChangeMovingPlatforms(Transform movingPlatform)
        {
            for (int i = 0; i < m_Objects.Length; ++i) {
                m_Objects[i].SetActive(movingPlatform != null);
            }
        }

        /// <summary>
        /// An object has entered the trigger.
        /// </summary>
        /// <param name="other">The object that entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            // A main character collider is required.
            if (!MathUtility.InLayerMask(other.gameObject.layer, 1 << LayerManager.Character)) {
                return;
            }

            var characterLocomotion = other.gameObject.GetComponentInParent<UltimateCharacterController.Character.UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            if (characterLocomotion.gameObject != m_Blitz) {
                return;
            }

            // The objects should not be activated when Blitz is in the trigger.
            m_BlitzInTrigger = true;
        }

        /// <summary>
        /// An object has exited the trigger.
        /// </summary>
        /// <param name="other">The collider that exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            // No actions are required if Blitz isn't in the trigger.
            if (!m_BlitzInTrigger) {
                return;
            }

            // A main character collider is required.
            if (!MathUtility.InLayerMask(other.gameObject.layer, 1 << LayerManager.Character)) {
                return;
            }

            var characterLocomotion = other.gameObject.GetComponentInParent<UltimateCharacterController.Character.UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            if (characterLocomotion.gameObject != m_Blitz) {
                return;
            }

            // Blitz is no longer in the trigger - reactivate the objects.
            m_BlitzInTrigger = false;
            for (int i = 0; i < m_Objects.Length; ++i) {
                m_Objects[i].SetActive(!m_RideActive && !m_BlitzInTrigger);
            }
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<Ability, bool>(m_Nolan, "OnCharacterAbilityActive", OnAbilityActive);
            EventHandler.UnregisterEvent<Transform>(m_Blitz, "OnCharacterChangeMovingPlatforms", OnCharacterChangeMovingPlatforms);
        }
    }
}