/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Objects.CharacterAssist
{
    /// <summary>
    /// A Gravity Zone represents a trigger area that adjusts the character's gravity direction when the character is within the trigger.
    /// </summary>
    public abstract class GravityZone : MonoBehaviour
    {
        /// <summary>
        /// Determines the direction of gravity that should be applied.
        /// </summary>
        /// <param name="position">The position of the character.</param>
        /// <returns>The direction of gravity that should be applied.</returns>
        public abstract Vector3 DetermineGravityDirection(Vector3 position);

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

            // The object must be a character.
            var characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            // With the Align To Gravity Zone ability.
            var alignToGravity = characterLocomotion.GetAbility<AlignToGravityZone>();
            if (alignToGravity == null) {
                return;
            }

            alignToGravity.RegisterGravityZone(this);
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


            // The object must be a character.
            var characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            // With the Align To Gravity Zone ability.
            var alignToGravity = characterLocomotion.GetAbility<AlignToGravityZone>();
            if (alignToGravity == null) {
                return;
            }

            alignToGravity.UnregisterGravityZone(this);
        }
    }
}