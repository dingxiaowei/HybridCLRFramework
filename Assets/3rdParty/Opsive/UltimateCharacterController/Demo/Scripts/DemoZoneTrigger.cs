/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Notifies the DemoManager when the character enters a trigger.
    /// </summary>
    public class DemoZoneTrigger : MonoBehaviour
    {
        private int m_EnterExitCount;
        private DemoManager m_DemoManager;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_DemoManager = GetComponentInParent<DemoManager>();    
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

            // AI and remote networked characters should not trigger the zone.
            var characterLocomotion = other.gameObject.GetCachedParentComponent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null || characterLocomotion.gameObject != m_DemoManager.Character) {
                return;
            }

            m_EnterExitCount++;
            m_DemoManager.EnteredTriggerZone(this, other.gameObject);
        }

        /// <summary>
        /// An object has exited the trigger.
        /// </summary>
        /// <param name="other">The collider that exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            // A main character collider is required.
            if (!MathUtility.InLayerMask(other.gameObject.layer, 1 << LayerManager.Character)) {
                return;
            }

            // AI and remote networked characters should not trigger the zone.
            var characterLocomotion = other.gameObject.GetCachedParentComponent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null || characterLocomotion.gameObject != m_DemoManager.Character) {
                return;
            }

            m_EnterExitCount--;
            if (m_EnterExitCount == 0) {
                m_DemoManager.ExitedTriggerZone(this);
            }
        }
    }
}