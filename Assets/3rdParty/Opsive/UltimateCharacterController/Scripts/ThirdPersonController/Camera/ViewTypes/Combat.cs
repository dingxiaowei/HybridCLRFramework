/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Events;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes
{
    /// <summary>
    /// The Combat View Type will inherit the functionality from the Third Person View Type while keeping the camera rotated to the same local yaw value
    /// as the character.
    /// </summary>
    [UltimateCharacterController.Camera.ViewTypes.RecommendedMovementType(typeof(Character.MovementTypes.Combat))]
    [UltimateCharacterController.StateSystem.AddState("Zoom", "edafe89541fb59d4dba60703f5b1574a")]
    public class Combat : ThirdPerson
    {
        private bool m_RotateWithCharacter;

        /// <summary>
        /// Attaches the view type to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the camera to.</param>
        public override void AttachCharacter(GameObject character)
        {
            if (m_Character != null) {
                EventHandler.UnregisterEvent<Ability, bool>(m_Character, "OnCharacterAbilityActive", OnAbilityActive);
            }

            base.AttachCharacter(character);

            if (m_Character != null) {
                m_RotateWithCharacter = false;
                EventHandler.RegisterEvent<Ability, bool>(m_Character, "OnCharacterAbilityActive", OnAbilityActive);
            }
        }

        /// <summary>
        /// Rotates the camera according to the horizontal and vertical movement values.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        /// <param name="immediatePosition">Should the camera be positioned immediately?</param>
        /// <returns>The updated rotation.</returns>
        public override Quaternion Rotate(float horizontalMovement, float verticalMovement, bool immediatePosition)
        {
            // The character may be controlling the rotation rather than the camera.
            if (m_RotateWithCharacter) {
                m_CharacterRotation = m_CharacterTransform.rotation;
            } else {
                m_Yaw += horizontalMovement;
            }

            return base.Rotate(horizontalMovement, verticalMovement, immediatePosition);
        }

        /// <summary>
        /// The character's ability has been started or stopped.
        /// </summary>
        /// <param name="ability">The ability which was started or stopped.</param>
        /// <param name="active">True if the ability was started, false if it was stopped.</param>
        private void OnAbilityActive(Ability ability, bool active)
        {
            if (!(ability is MoveTowards)) {
                return;
            }

            // Rotate with the camera so the camera will follow the character's rotation when the character is getting into position for Move Towards.
            m_RotateWithCharacter = active;

            // When rotate with character is enabled the CharacterRotation quaternion will update to the character's current rotation so the camera moves with the
            // character rather than the character moving with the camera. Set to yaw to 0 to prevent a snapping when the CharacterRotation quaternion is updated.
            if (m_RotateWithCharacter) {
                m_Yaw = 0;
            }
        }
    }
}