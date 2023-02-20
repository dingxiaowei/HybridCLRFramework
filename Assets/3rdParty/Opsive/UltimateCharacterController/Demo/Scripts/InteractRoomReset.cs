/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Utility;
using Opsive.UltimateCharacterController.Objects.CharacterAssist;

namespace Opsive.UltimateCharacterController.Demo
{
    /// <summary>
    /// Resets the specified interactable objects when the character leaves the room.
    /// </summary>
    public class InteractRoomReset : MonoBehaviour
    {
        [Tooltip("A list of objects that should be reset when the character leaves the room.")]
        public GameObject[] m_InteractObjects;

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

            ResetInteractObjects();
        }

        /// <summary>
        /// Resets the interact room objects.
        /// </summary>
        private void ResetInteractObjects()
        {
            for (int i = 0; i < m_InteractObjects.Length; ++i) {
                var animatedInteractables = m_InteractObjects[i].GetComponents<AnimatedInteractable>();
                for (int j = 0; j < animatedInteractables.Length; ++j) {
                    animatedInteractables[j].ResetInteract();
                }
            }
        }
    }
}