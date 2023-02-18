/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo
{
    using Opsive.UltimateCharacterController.Character;
    using UnityEngine;

    /// <summary>
    /// Disables the specified Collider when the character enters the trigger.
    /// </summary>
    public class ColliderDisabler : MonoBehaviour
    {
        [Tooltip("The Collider that should be disabled.")]
        [SerializeField] protected Collider m_DisableCollider;

        private DemoManager m_DemoManager;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_DemoManager = FindObjectOfType<DemoManager>();
        }

        /// <summary>
        /// An object has entered the trigger.
        /// </summary>
        /// <param name="other">The object that entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            var characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            if (characterLocomotion.gameObject != m_DemoManager.Character) {
                return;
            }

            m_DisableCollider.enabled = false;
        }

        /// <summary>
        /// An object has left the trigger.
        /// </summary>
        /// <param name="other">The object that exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            var characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            if (characterLocomotion.gameObject != m_DemoManager.Character) {
                return;
            }

            m_DisableCollider.enabled = true;
        }
    }
}