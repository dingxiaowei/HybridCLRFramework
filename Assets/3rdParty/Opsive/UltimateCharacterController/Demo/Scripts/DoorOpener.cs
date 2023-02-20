/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Demo.Objects;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Demo
{
    /// <summary>
    /// Opens the doors when the character enters the trigger.
    /// </summary>
    public class DoorOpener : MonoBehaviour
    {
        [Tooltip("The doors that should be opened.")]
        [SerializeField] protected Door[] m_Doors;

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

            // Only enter the trigger if the object is a character.
            if (other.gameObject.GetCachedParentComponent<UltimateCharacterLocomotion>() == null) {
                return;
            }

            // Open all of the doors.
            for (int i = 0; i < m_Doors.Length; ++i) {
                m_Doors[i].OpenClose(true, true, false);
            }

            // The GameObject is no longer necessary - the doors will stay open.
            Destroy(gameObject);
        }
    }
}