/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.FirstPersonController.Character.MovementTypes
{
    using Opsive.UltimateCharacterController.Character.MovementTypes;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// With the Combat movement type the character can strafe and move backwards, and is always facing in the direction of the camera.
    /// </summary>
    public class Combat : MovementType
    {
        public override bool FirstPersonPerspective { get { return true; } }

        /// <summary>
        /// Returns the delta yaw rotation of the character.
        /// </summary>
        /// <param name="characterHorizontalMovement">The character's horizontal movement.</param>
        /// <param name="characterForwardMovement">The character's forward movement.</param>
        /// <param name="cameraHorizontalMovement">The camera's horizontal movement.</param>
        /// <param name="cameraVerticalMovement">The camera's vertical movement.</param>
        /// <returns>The delta yaw rotation of the character.</returns>
        public override float GetDeltaYawRotation(float characterHorizontalMovement, float characterForwardMovement, float cameraHorizontalMovement, float cameraVerticalMovement)
        {
#if UNITY_EDITOR
            if (m_LookSource == null) {
                Debug.LogError("Error: There is no look source attached to the character. Ensure the character has a look source attached. For player characters the look source is the Camera Controller, and AI agents use the Local Look Source.");
                return 0;
            }
#endif
            var lookRotation = Quaternion.LookRotation(m_LookSource.LookDirection(true), m_CharacterLocomotion.Up);
            // Convert to a local character rotation and then only return the relative y rotation.
            return MathUtility.ClampInnerAngle(MathUtility.InverseTransformQuaternion(m_Transform.rotation, lookRotation).eulerAngles.y);
        }

        /// <summary>
        /// Gets the controller's input vector relative to the movement type.
        /// </summary>
        /// <param name="inputVector">The current input vector.</param>
        /// <returns>The updated input vector.</returns>
        public override Vector2 GetInputVector(Vector2 inputVector)
        {
            // No changes are necessary.
            return inputVector;
        }
    }
}