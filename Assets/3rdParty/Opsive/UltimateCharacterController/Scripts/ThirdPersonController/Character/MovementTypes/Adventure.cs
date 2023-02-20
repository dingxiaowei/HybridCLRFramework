/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character.MovementTypes;
using Opsive.UltimateCharacterController.Character.Abilities.Items;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Character.MovementTypes
{
    /// <summary>
    /// With the Adventure movement type the character will always move forward in the movement direction. This is relative to the camera direction and prevents the 
    /// character from playing a strafe or backwards animation.
    /// </summary>
    public class Adventure : MovementType
    {
        private bool m_AimActive;
        private int m_FaceUseTargetCount;

        public override bool FirstPersonPerspective { get { return false; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            EventHandler.RegisterEvent<bool, bool>(m_GameObject, "OnAimAbilityStart", OnAimStart);
            EventHandler.RegisterEvent<bool, Use>(m_GameObject, "OnUseAbilityStart", OnUseStart);
        }

        /// <summary>
        /// The Aim ability has started or stopped.
        /// </summary>
        /// <param name="start">Has the Aim ability started?</param>
        /// <param name="inputStart">Was the ability started from input?</param>
        private void OnAimStart(bool start, bool inputStart)
        {
            if (!inputStart) {
                return;
            }
            m_AimActive = start;
        }

        /// <summary>
        /// The Use ability has started or stopped using an item.
        /// </summary>
        /// <param name="start">Has the Use ability started?</param>
        /// <param name="useAbility">The Use ability that has started or stopped.</param>
        private void OnUseStart(bool start, Use useAbility)
        {
            if (useAbility.FaceTargetItem != null) {
                m_FaceUseTargetCount += start ? 1 : -1;
            }
        }

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
            if (characterHorizontalMovement != 0 || characterForwardMovement != 0) {
                var lookRotation = Quaternion.LookRotation(m_LookSource.Transform.rotation * 
                    new Vector3(characterHorizontalMovement, 0, characterForwardMovement).normalized, m_CharacterLocomotion.Up);
                return MathUtility.ClampInnerAngle(MathUtility.InverseTransformQuaternion(m_Transform.rotation, lookRotation).eulerAngles.y);
            }
            return 0;
        }

        /// <summary>
        /// Gets the controller's input vector relative to the movement type.
        /// </summary>
        /// <param name="inputVector">The current input vector.</param>
        /// <returns>The updated input vector.</returns>
        public override Vector2 GetInputVector(Vector2 inputVector)
        {
            // The character will be facing the target while the aim ability is active or the use ability is facing the target. During this time the character should be
            // able to strafe or move backwards.
            if (m_AimActive || m_FaceUseTargetCount > 0) {
                return inputVector;
            }

            // The Adventure Movement Type only uses the forward input value.
            // Clamp to a value higher then one if the x or y value is greater then one. This can happen if the character is sprinting.
            var clampValue = Mathf.Max(Mathf.Abs(inputVector.x), Mathf.Max(Mathf.Abs(inputVector.y), 1));
            inputVector.y = Mathf.Clamp(inputVector.magnitude, -clampValue, clampValue);
            inputVector.x = 0;
            return inputVector;
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<bool, bool>(m_GameObject, "OnAimAbilityStart", OnAimStart);
            EventHandler.UnregisterEvent<bool, Use>(m_GameObject, "OnUseAbilityStart", OnUseStart);
        }
    }
}