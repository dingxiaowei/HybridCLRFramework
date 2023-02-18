/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.FirstPersonController.Character.MovementTypes
{
    using Opsive.UltimateCharacterController.Character.MovementTypes;
    using UnityEngine;

    /// <summary>
    /// Allows the character to rotate and move independently of the camera.
    /// </summary>
    public class FreeLook : MovementType
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
            return 0;
        }

        /// <summary>
        /// Gets the controller's input vector relative to the movement type.
        /// </summary>
        /// <param name="inputVector">The current input vector.</param>
        /// <returns>The updated input vector.</returns>
        public override Vector2 GetInputVector(Vector2 inputVector)
        {
            return inputVector;
        }

        /// <summary>
        /// Can the character look independently of the transform rotation?
        /// </summary>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <returns>True if the character should look independently of the transform rotation.</returns>
        public override bool UseIndependentLook(bool characterLookDirection)
        {
            return true;
        }
    }
}