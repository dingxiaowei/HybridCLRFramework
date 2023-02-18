/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.FirstPersonController.Camera.ViewTypes
{
    using Opsive.UltimateCharacterController.Utility;
    using Opsive.UltimateCharacterController.FirstPersonController.Character;
    using UnityEngine;

    /// <summary>
    /// The FreeLook ViewType is a first person view type that allows the camera to rotate independently of the character's direction.
    /// </summary>
    [UltimateCharacterController.Camera.ViewTypes.RecommendedMovementType(typeof(Character.MovementTypes.FreeLook))]
    [UltimateCharacterController.StateSystem.AddState("Zoom", "538aa537a9f445e40b8a2c2758627962")]
    public class FreeLook : FirstPerson
    {
        [Tooltip("The minimum yaw angle (in degrees).")]
        [SerializeField] protected float m_MinYawLimit = -90;
        [Tooltip("The maximum yaw angle (in degrees).")]
        [SerializeField] protected float m_MaxYawLimit = 90;
        [Tooltip("The speed in which the camera should rotate towards the yaw limit when out of bounds.")]
        [Range(0, 1)] [SerializeField] protected float m_YawLimitLerpSpeed = 0.7f;

        public float MinYawLimit { get { return m_MinYawLimit; } set { m_MinYawLimit = value; } }
        public float MaxYawLimit { get { return m_MaxYawLimit; } set { m_MaxYawLimit = value; } }
        public float YawLimitLerpSpeed { get { return m_YawLimitLerpSpeed; } set { m_YawLimitLerpSpeed = value; } }

        private Transform m_FirstPersonObjectsTransform;
        private Vector3 m_LookDirection;

        /// <summary>
        /// Attaches the camera to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the camera to.</param>
        public override void AttachCharacter(GameObject character)
        {
            m_FirstPersonObjectsTransform = null;

            base.AttachCharacter(character);

            if (m_Character != null) {
                var firstPersonObjects = m_Character.GetComponentInChildren<FirstPersonObjects>(true);
                if (firstPersonObjects == null) {
                    // The component may have already been changed to be a child of the camera.
                    firstPersonObjects = m_GameObject.GetComponentInChildren<FirstPersonObjects>(true);
                }
                // FirstPersonObjects won't exist if the character carries no items.
                if (firstPersonObjects != null) {
                    m_FirstPersonObjectsTransform = firstPersonObjects.transform;
                }
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
            // Update the rotation. The yaw may have a limit.
            if (Mathf.Abs(m_MinYawLimit - m_MaxYawLimit) < 360) {
                // Determine the new rotation with the updated yaw.
                var targetRotation = MathUtility.TransformQuaternion(m_CharacterRotation, Quaternion.Euler(m_Pitch, m_Yaw, 0));
                var diff = MathUtility.InverseTransformQuaternion(Quaternion.LookRotation(Vector3.forward, m_CharacterLocomotion.Up), targetRotation * Quaternion.Inverse(m_CharacterTransform.rotation));
                // The rotation shouldn't extend beyond the min and max yaw limit.
                var targetYaw = MathUtility.ClampAngle(diff.eulerAngles.y, horizontalMovement, m_MinYawLimit, m_MaxYawLimit);
                m_Yaw += Mathf.Lerp(0, Mathf.DeltaAngle(diff.eulerAngles.y, targetYaw), m_YawLimitLerpSpeed);
            } else {
                m_Yaw += horizontalMovement;
            }

            // Return the rotation.
            return base.Rotate(horizontalMovement, verticalMovement, immediatePosition);
        }

        /// <summary>
        /// Returns the direction that the character is looking.
        /// </summary>
        /// <param name="lookPosition">The position that the character is looking from.</param>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <param name="layerMask">The LayerMask value of the objects that the look direction can hit.</param>
        /// <param name="useRecoil">Should recoil be included in the look direction?</param>
        /// <returns>The direction that the character is looking.</returns>
        public override Vector3 LookDirection(Vector3 lookPosition, bool characterLookDirection, int layerMask, bool useRecoil)
        {
            var rotation = m_FirstPersonObjectsTransform != null ? m_FirstPersonObjectsTransform.rotation : m_CameraController.Anchor.rotation;

            // Cast a ray from the camera point in the forward direction. The look direction is then the vector from the look position to the hit point.
            RaycastHit hit;
            Vector3 hitPoint;
            if (Physics.Raycast(m_Transform.position, rotation * Vector3.forward, out hit, m_LookDirectionDistance, layerMask, QueryTriggerInteraction.Ignore)) {
                hitPoint = hit.point;
            } else {
                Vector3 position;
                if (useRecoil) {
                    position = GetAnchorTransformPoint(m_PositionSpring.Value + m_SecondaryPositionSpring.Value);
                } else {
                    position = lookPosition;
                }
                m_LookDirection.Set(0, 0, m_LookDirectionDistance);
                hitPoint = MathUtility.TransformPoint(position, rotation, m_LookDirection);
            }

            return (hitPoint - lookPosition).normalized;
        }
    }
}