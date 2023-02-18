/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects.CharacterAssist
{
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Specifies the location that the character should move to when the Move Towards ability is started.
    /// </summary>
    public class MoveTowardsLocation : MonoBehaviour
    {
        [Tooltip("The offset relative to the transform that the character should move towards.")]
        [SerializeField] protected Vector3 m_Offset = new Vector3(0, 0, 1);
        [Tooltip("The yaw offset relative to the transform that the character should rotate towards.")]
        [SerializeField] protected float m_YawOffset = 180;
        [Tooltip("The size of the area that the character can start the ability at. A zero value indicates that the character must land on the exact offset.")]
        [SerializeField] protected Vector3 m_Size;
        [Tooltip("The ability can start when the distance between the start location and character is less than the specified value.")]
        [Range(0.0001f, 100)] [SerializeField] protected float m_Distance = 0.01f;
        [Tooltip("The ability can start when the angle threshold between the start location and character is less than the specified value.")]
        [Range(0, 360)] [SerializeField] protected float m_Angle = 0.5f;
        [Tooltip("Is the character required to be on the ground?")]
        [SerializeField] protected bool m_RequireGrounded = true;
        [Tooltip("Should the ability wait to start until all transitions are complete?")]
        [SerializeField] protected bool m_PrecisionStart = true;
        [Tooltip("The multiplier to apply to the character's speed when moving to the start location.")]
        [SerializeField] protected float m_MovementMultiplier = 1;

        public Vector3 Offset { get { return m_Offset; } set { m_Offset = value; } }
        public float YawOffset { get { return m_YawOffset; } set { m_YawOffset = value; } }
        public Vector3 Size { get { return m_Size; } set { m_Size = value; } }
        public float Distance { get { return m_Distance; } set { m_Distance = value; } }
        public float Angle { get { return m_Angle; } set { m_Angle = value; } }
        public bool RequireGrounded { get { return m_RequireGrounded; } set { m_RequireGrounded = value; } }
        public bool PrecisionStart { get { return m_PrecisionStart; } set { m_PrecisionStart = value; } }
        public float MovementMultiplier { get { return m_MovementMultiplier; } set { m_MovementMultiplier = value; } }

        private Transform m_Transform;
        private float m_StartYawOffset;
        private Vector3 m_StartOffset;

        public Vector3 TargetPosition { get { return m_Transform.TransformPoint(m_Offset); } }
        public Quaternion TargetRotation { get { return m_Transform.rotation * Quaternion.Euler(0, m_YawOffset, 0); } }
        public float StartYawOffset { get { return m_StartYawOffset; } }
        public Vector3 StartOffset { get { return m_StartOffset; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;
            m_StartYawOffset = m_YawOffset;
            m_StartOffset = m_Offset;
        }

        /// <summary>
        /// Returns the direction that the character should move towards.
        /// </summary>
        /// <param name="position">The position of the character.</param>
        /// <param name="rotation">The rotation of the character.</param>
        /// <returns>The direction that the character should move towards.</returns>
        public Vector3 GetTargetDirection(Vector3 position, Quaternion rotation)
        {
            var direction = m_Transform.TransformPoint(m_Offset) - position;
            if (m_Size.sqrMagnitude == 0) {
                return MathUtility.InverseTransformDirection(direction, rotation);
            }

            var size = m_Transform.TransformDirection(m_Size);
            if (Mathf.Abs(direction.x) < Mathf.Abs(size.x / 2)) { direction.x = 0; }
            if (Mathf.Abs(direction.y) < Mathf.Abs(size.y / 2)) { direction.y = 0; }
            if (Mathf.Abs(direction.z) < Mathf.Abs(size.z / 2)) { direction.z = 0; }
            return MathUtility.InverseTransformDirection(direction, rotation);
        }

        /// <summary>
        /// Is the character in a valid position?
        /// </summary>
        /// <param name="position">The position of the character.</param>
        /// <param name="rotation">The rotation of the character.</param>
        /// <param name="grounded">Is the character grounded?</param>
        /// <returns>True if the position is valid.</returns>
        public bool IsPositionValid(Vector3 position, Quaternion rotation, bool grounded)
        {
            var direction = GetTargetDirection(position, rotation);
            if (Mathf.Abs(direction.x) <= m_Distance && 
                ((m_RequireGrounded && grounded) || (!m_RequireGrounded && (Mathf.Abs(direction.y) <= m_Distance))) &&
                Mathf.Abs(direction.z) <= m_Distance) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Is the character in a valid rotation?
        /// </summary>
        /// <param name="rotation">The rotation of the character.</param>
        /// <returns>True if the rotation is valid.</returns>
        public bool IsRotationValid(Quaternion rotation)
        {
            Vector3 forwardDirection;
            if (m_RequireGrounded) {
                forwardDirection = Vector3.ProjectOnPlane(m_Transform.forward, rotation * Vector3.up);
            } else {
                forwardDirection = m_Transform.forward;
            }
            return Vector3.Angle(Quaternion.Euler(0, m_YawOffset, 0) * forwardDirection, rotation * Vector3.forward) <= m_Angle / 2 + 0.001f;
        }
    }
}