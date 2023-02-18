/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.AI
{
    using Opsive.UltimateCharacterController.Character.MovementTypes;
    using UnityEngine;

    /// <summary>
    /// The Idle Movement Type will ensure the AI character doesn't move. A regular Movement Type could have also been used but there isn't a common Movement Type included
    /// with all of the different packages so it's easier to use the same Movement Type for demo purposes.
    /// </summary>
    public class Idle : MovementType
    {
        public override bool FirstPersonPerspective { get { return false; } }

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
    }
}