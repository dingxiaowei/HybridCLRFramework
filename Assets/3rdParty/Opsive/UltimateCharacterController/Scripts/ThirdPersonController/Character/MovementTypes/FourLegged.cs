/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character.MovementTypes;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Character.MovementTypes
{
    /// <summary>
    /// The FourLegged MovementType allows the character to move as if they are on four legs. The character cannot strafe and will rotate when trying to turn.
    /// </summary>
    public class FourLegged : MovementType
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
            var localEuler = MathUtility.InverseTransformQuaternion(m_Transform.rotation, Quaternion.LookRotation(m_Transform.forward, m_Transform.up)).eulerAngles;
            localEuler.y += characterHorizontalMovement * (characterForwardMovement < 0 ? -1 : 1);
            // If there is no horizontal movement and some forward movement then the character should turn in the direction of the camera.
            if (characterHorizontalMovement == 0 && characterForwardMovement != 0) {
                var cameraAngle = MathUtility.InverseTransformQuaternion(m_Transform.rotation, m_LookSource.Transform.rotation).eulerAngles;
                // The horizontal/forward movement input values will always be in a range between -1 and 1 so keep the camera angle within the same range
                // so the rotation doesn't move too quickly.
                localEuler.y += Mathf.Clamp(MathUtility.ClampInnerAngle(cameraAngle.y), -1, 1);
            }
            return localEuler.y;
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