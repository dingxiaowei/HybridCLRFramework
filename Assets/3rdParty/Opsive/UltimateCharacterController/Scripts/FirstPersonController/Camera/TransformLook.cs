/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Camera;
using Opsive.UltimateCharacterController.Camera.ViewTypes;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.FirstPersonController.Camera.ViewTypes
{
    /// <summary>
    /// The TransformLook ViewType is a first person view type that will have the camera look in the forward direction relative to the target transform.
    /// </summary>
    public class TransformLook : ViewType
    {
        [Tooltip("The object that determines the position of the camera.")]
        [SerializeField] protected Transform m_MoveTarget;
        [Tooltip("The object that determines the rotation of the camera.")]
        [SerializeField] protected Transform m_RotationTarget;
        [Tooltip("The offset relative to the move target.")]
        [SerializeField] protected Vector3 m_Offset = new Vector3(0, 0.2f, 0.2f);
        [Tooltip("The radius of the camera's collision sphere to prevent it from clipping with other objects.")]
        [SerializeField] protected float m_CollisionRadius = 0.05f;
        [Tooltip("The speed at which the camera should move.")]
        [SerializeField] protected float m_MoveSpeed = 10;
        [Tooltip("The speed at which the view type should rotate towards the target rotation.")]
        [Range(0, 1)] [SerializeField] protected float m_RotationalLerpSpeed = 0.9f;

        public Transform MoveTarget { get { return m_MoveTarget; } set { m_MoveTarget = value; } }
        public Transform RotationTarget { get { return m_RotationTarget; } set { m_RotationTarget = value; } }
        public Vector3 Offset { get { return m_Offset; } set { m_Offset = value; } }
        public float CollisionRadius { get { return m_CollisionRadius; } set { m_CollisionRadius = value; } }
        public float MoveSpeed { get { return m_MoveSpeed; } set { m_MoveSpeed = value; } }
        public float RotationalLerpSpeed { get { return m_RotationalLerpSpeed; } set { m_RotationalLerpSpeed = value; } }

        public override Quaternion CharacterRotation { get { return m_CharacterTransform.rotation; } }
        public override bool FirstPersonPerspective { get { return true; } }
        public override float LookDirectionDistance { get { return m_Offset.magnitude; } }
        public override float Pitch { get { return 0; } }
        public override float Yaw { get { return 0; } }

        private UnityEngine.Camera m_Camera;
        private RaycastHit m_RaycastHit;

        /// <summary>
        /// Initializes the view type to the specified camera controller.
        /// </summary>
        /// <param name="cameraController">The camera controller to initialize the view type to.</param>
        public override void Initialize(CameraController cameraController)
        {
            base.Initialize(cameraController);

            m_Camera = cameraController.gameObject.GetCachedComponent<UnityEngine.Camera>();
        }

        /// <summary>
        /// Attaches the view type to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the camera to.</param>
        public override void AttachCharacter(GameObject character)
        {
            base.AttachCharacter(character);

            if (m_MoveTarget == null || m_RotationTarget == null) {
                Transform moveTarget = m_CharacterTransform, rotationTarget = m_CharacterTransform;
                var characterAnimator = m_Character.GetCachedComponent<Animator>();
                if (characterAnimator != null) {
                    moveTarget = characterAnimator.GetBoneTransform(HumanBodyBones.Head);
                    rotationTarget = characterAnimator.GetBoneTransform(HumanBodyBones.Hips);
                }

                m_MoveTarget = moveTarget;
                m_RotationTarget = rotationTarget;
            }
        }

        /// <summary>
        /// Rotates the camera to face in the same direction as the target.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        /// <returns>The updated rotation.</returns>
        public override Quaternion Rotate(float horizontalMovement, float verticalMovement, bool immediateUpdate)
        {
            var rotation = m_RotationTarget.rotation;
            if (!immediateUpdate) {
                rotation = Quaternion.Slerp(m_Transform.rotation, rotation, m_RotationalLerpSpeed);
            }
            return rotation;
        }

        /// <summary>
        /// Moves the camera to be in the target position.
        /// </summary>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        /// <returns>The updated position.</returns>
        public override Vector3 Move(bool immediateUpdate)
        {
            // Ensure there aren't any objects obstructing the distance between the anchor offset and the target position.
            var collisionLayerEnabled = m_CharacterLocomotion.CollisionLayerEnabled;
            m_CharacterLocomotion.EnableColliderCollisionLayer(false);
            var targetPosition = m_MoveTarget.TransformPoint(m_Offset);
            var direction = targetPosition - m_MoveTarget.position;
            if (Physics.SphereCast(m_MoveTarget.position, m_CollisionRadius, direction.normalized, out m_RaycastHit, direction.magnitude + m_Camera.nearClipPlane,
                            m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore)) {
                // Move the camera in if an object obstructed the view.
                targetPosition = m_RaycastHit.point + m_RaycastHit.normal * (m_Camera.nearClipPlane + m_CharacterLocomotion.ColliderSpacing);
            }
            m_CharacterLocomotion.EnableColliderCollisionLayer(collisionLayerEnabled);

            return Vector3.MoveTowards(m_Transform.position, targetPosition, immediateUpdate ? float.MaxValue : Time.fixedDeltaTime * m_MoveSpeed);
        }

        /// <summary>
        /// Returns the direction that the character is looking.
        /// </summary>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <returns>The direction that the character is looking.</returns>
        public override Vector3 LookDirection(bool characterLookDirection)
        {
            return m_CharacterTransform.forward;
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
            return m_CharacterTransform.forward;
        }
    }
}