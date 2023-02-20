/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Camera;
using Opsive.UltimateCharacterController.Camera.ViewTypes;
using Opsive.UltimateCharacterController.Motion;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes
{
    /// <summary>
    /// Looks at the specified target. If no target is specified the camera will look in the character's direction.
    /// </summary>
    public class LookAt : ViewType
    {
        [Tooltip("The object to look at. If null then the character's transform will be used.")]
        [SerializeField] protected Transform m_Target;
        [Tooltip("The offset relative to the target.")]
        [SerializeField] protected Vector3 m_Offset = new Vector3(0, 3, -2);
        [Tooltip("The minimum distance away from the target that the camera should move towards.")]
        [SerializeField] protected float m_MinLookDistance = 1;
        [Tooltip("The maximum distance away from the target that the camera should move towards.")]
        [SerializeField] protected float m_MaxLookDistance = 5;
        [Tooltip("The speed at which the camera should move.")]
        [SerializeField] protected float m_MoveSpeed = 10;
        [Tooltip("The radius of the camera's collision sphere to prevent it from clipping with other objects.")]
        [SerializeField] protected float m_CollisionRadius = 0.05f;
        [Tooltip("The speed at which the view type should rotate towards the target rotation.")]
        [Range(0, 1)] [SerializeField] protected float m_RotationalLerpSpeed = 0.9f;
        [Tooltip("The spring used for applying a rotation to the camera.")]
        [SerializeField] protected Spring m_RotationSpring;

        public Transform Target { get { return m_Target; } set { m_Target = value; } }
        public Vector3 Offset { get { return m_Offset; } set { m_Offset = value; } }
        public float MinLookDistance { get { return m_MinLookDistance; } set { m_MinLookDistance = value; } }
        public float MaxLookDistance { get { return m_MaxLookDistance; } set { m_MaxLookDistance = value; } }
        public float MoveSpeed { get { return m_MoveSpeed; } set { m_MoveSpeed = value; } }
        public float CollisionRadius { get { return m_CollisionRadius; } set { m_CollisionRadius = value; } }
        public float RotationalLerpSpeed { get { return m_RotationalLerpSpeed; } set { m_RotationalLerpSpeed = value; } }
        public Spring RotationSpring
        {
            get { return m_RotationSpring; }
            set
            {
                m_RotationSpring = value;
                if (m_RotationSpring != null) { m_RotationSpring.Initialize(true, true); }
            }
        }

        public override Quaternion CharacterRotation { get { return m_CharacterTransform.rotation; } }
        public override bool FirstPersonPerspective { get { return m_CharacterLocomotion != null ? m_CharacterLocomotion.FirstPersonPerspective : false; } }
        public override float LookDirectionDistance { get { return m_Offset.magnitude; } }
        public override float Pitch { get { return 0; } }
        public override float Yaw { get { return 0; } }

        private Vector3 m_PrevRotationSpringValue;
        private Vector3 m_PrevRotationSpringVelocity;
        private RaycastHit m_RaycastHit;

        /// <summary>
        /// Initializes the view type to the specified camera controller.
        /// </summary>
        /// <param name="cameraController">The camera controller to initialize the view type to.</param>
        public override void Initialize(CameraController cameraController)
        {
            base.Initialize(cameraController);

            m_RotationSpring.Initialize(true, true);
        }

        /// <summary>
        /// Attaches the view type to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the camera to.</param>
        public override void AttachCharacter(GameObject character)
        {
            base.AttachCharacter(character);

            if (m_Target == null) {
                m_Target = m_CharacterTransform;
            }
        }

        /// <summary>
        /// Rotates the camera to look at the target.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        /// <returns>The updated rotation.</returns>
        public override Quaternion Rotate(float horizontalMovement, float verticalMovement, bool immediateUpdate)
        {
            var rotation = Quaternion.LookRotation((m_Target.position - m_Transform.position).normalized, -m_CharacterLocomotion.GravityDirection);
            if (!immediateUpdate) {
                rotation = Quaternion.Slerp(m_Transform.rotation, rotation, m_RotationalLerpSpeed);
            }

            // Add the rotational spring value.
            var localEulerAngles = MathUtility.InverseTransformQuaternion(m_CameraController.Anchor.rotation, rotation).eulerAngles;
            localEulerAngles += m_RotationSpring.Value;
            rotation = MathUtility.TransformQuaternion(m_CameraController.Anchor.rotation, Quaternion.Euler(localEulerAngles));

            return rotation;
        }

        /// <summary>
        /// Moves the camera to look at the target.
        /// </summary>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        /// <returns>The updated position.</returns>
        public override Vector3 Move(bool immediateUpdate)
        {
            var cameraPosition = MathUtility.TransformPoint(m_Target.position, m_CharacterTransform.rotation, m_Offset);
            // Move towards the target if the target is too far away.
            var direction = (m_Target.position - m_Transform.position);
            var distance = direction.magnitude;
            Vector3 targetPosition;
            if (distance > m_MaxLookDistance) {
                targetPosition = Vector3.MoveTowards(m_Transform.position, cameraPosition, immediateUpdate ? (distance - m_MaxLookDistance * 0.8f) : Time.fixedDeltaTime * m_MoveSpeed);
            } else if (distance < m_MinLookDistance) {
                targetPosition = Vector3.MoveTowards(m_Transform.position, cameraPosition - (m_Target.position - m_Transform.position) * (m_MaxLookDistance - distance),
                                                        immediateUpdate ? m_MaxLookDistance - distance : Time.fixedDeltaTime * m_MoveSpeed);
            } else {
                targetPosition = m_Transform.position;
            }

            var collisionLayerEnabled = m_CharacterLocomotion.CollisionLayerEnabled;
            m_CharacterLocomotion.EnableColliderCollisionLayer(false);
            // Fire a sphere to prevent the camera from colliding with other objects.
            direction = targetPosition - m_Target.position;
            if (Physics.SphereCast(m_Target.position, m_CollisionRadius, direction.normalized, out m_RaycastHit, Mathf.Max(direction.magnitude - m_MinLookDistance, 0.01f), m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore)) {
                // Move the camera in if the character isn't in view.
                targetPosition = m_RaycastHit.point + m_RaycastHit.normal * m_CollisionRadius;
            }
            m_CharacterLocomotion.EnableColliderCollisionLayer(collisionLayerEnabled);

            return targetPosition;
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

        /// <summary>
        /// Adds a rotational force to the ViewType.
        /// </summary>
        /// <param name="force">The force to add.</param>
        public override void AddRotationalForce(Vector3 force)
        {
            m_RotationSpring.AddForce(force);
        }

        /// <summary>
        /// Callback when the StateManager will change the active state on the current object.
        /// </summary>
        public override void StateWillChange()
        {
            // Remember the interal spring values so they can be restored if a new spring is applied during the state change.
            m_PrevRotationSpringValue = m_RotationSpring.Value;
            m_PrevRotationSpringVelocity = m_RotationSpring.Velocity;
        }

        /// <summary>
        /// Callback when the StateManager has changed the active state on the current object.
        /// </summary>
        public override void StateChange()
        {
            m_RotationSpring.Value = m_PrevRotationSpringValue;
            m_RotationSpring.Velocity = m_PrevRotationSpringVelocity;
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            m_RotationSpring.Destroy();
        }
    }
}