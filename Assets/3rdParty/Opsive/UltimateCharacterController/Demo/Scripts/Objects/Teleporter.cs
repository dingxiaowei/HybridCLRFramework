/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Objects
{
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Teleports the character to the specified destination.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class Teleporter : MonoBehaviour
    {
        [Tooltip("The location that the character will teleport to.")]
        [SerializeField] protected Transform m_Destination;
        [Tooltip("Should the character's animator be snapped when teleporting to the destination?")]
        [SerializeField] protected bool m_SnapAnimator;
        [Tooltip("The LayerMask of the character.")]
        [SerializeField] protected LayerMask m_LayerMask = 1 << LayerManager.Character;
        [Tooltip("The AudioClip that should play when the character is teleported.")]
        [SerializeField] protected AudioClip m_TeleportAudioClip;
        [Tooltip("The name of the state that should activate when the character teleports.")]
        [SerializeField] protected string m_StateName;
#if UNITY_EDITOR
        [Tooltip("The color to draw the editor gizmo in (editor only).")]
        [SerializeField] protected Color m_GizmoColor = new Color(0, 0, 1, 0.3f);
#endif

        public Transform Destination { get { return m_Destination; } set { m_Destination = value; } }
#if UNITY_EDITOR
        public Color GizmoColor { get { return m_GizmoColor; } set { m_GizmoColor = value; } }
#endif

        private AudioSource m_AudioSource;

        private bool m_IgnoreCharacterEnter;

        public bool IgnoreCharacterEnter { set { m_IgnoreCharacterEnter = true; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_AudioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// Teleport the character to the specified destination.
        /// </summary>
        /// <param name="other">The collider that entered the trigger. May or may not be a character.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, m_LayerMask) || m_IgnoreCharacterEnter) {
                return;
            }

            UltimateCharacterLocomotion characterLocomotion;
            if ((characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>()) != null) {
                // Do not allow teleportation if the Drive or Ride abilities are active.
                if (characterLocomotion.IsAbilityTypeActive<UltimateCharacterController.Character.Abilities.Drive>() ||
                    characterLocomotion.IsAbilityTypeActive<UltimateCharacterController.Character.Abilities.Ride>()) {
                    return;
                }

                var destinationTeleporter = m_Destination.GetComponentInParent<Teleporter>();
                if (destinationTeleporter != null) {
                    destinationTeleporter.IgnoreCharacterEnter = true;
                }
                characterLocomotion.SetPositionAndRotation(m_Destination.position, m_Destination.rotation, m_SnapAnimator);

                if (m_AudioSource != null && m_TeleportAudioClip != null) {
                    m_AudioSource.clip = m_TeleportAudioClip;
                    m_AudioSource.Play();
                }

                if (!string.IsNullOrEmpty(m_StateName)) {
                    StateSystem.StateManager.SetState(characterLocomotion.gameObject, m_StateName, true);
                }
            }
        }

        /// <summary>
        /// An object has exited the trigger.
        /// </summary>
        /// <param name="other">The collider that entered the trigger. May or may not be a character.</param>
        private void OnTriggerExit(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, m_LayerMask)) {
                return;
            }

            m_IgnoreCharacterEnter = false;
        }
    }
}