/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.StateSystem;

namespace Opsive.UltimateCharacterController.ThirdPersonController.Character.Abilities.Items
{
    /// <summary>
    /// Allows the ItemPullback collider to be resized/positioned based upon the state.
    /// </summary>
    public class ItemPullbackCollider : StateBehavior
    {
        [Tooltip("The offset to apply to the transform relative to the starting local position.")]
        [SerializeField] protected Vector3 m_LocalPositionOffset;
        [Tooltip("The offset to apply to the radius to the starting radius.")]
        [SerializeField] protected float m_RadiusOffset;

        public Vector3 LocalPositionOffset { get { return m_LocalPositionOffset; } set { m_LocalPositionOffset = value; } }
        public float RadiusOffset { get { return m_RadiusOffset; } set { m_RadiusOffset = value; } }

        private Transform m_Transform;
        private Collider m_Collider;

        private Vector3 m_LocalPosition;
        private float m_Radius;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_Collider = GetComponent<Collider>();
            if (!(m_Collider is CapsuleCollider) && !(m_Collider is SphereCollider)) {
                Debug.LogWarning("Warning: The ItemPullbackCollider only supports capsule and sphere colliders.");
                enabled = false;
                return;
            }

            m_Transform = transform;
            m_LocalPosition = m_Transform.localPosition;
            if (m_Collider is CapsuleCollider) {
                m_Radius = (m_Collider as CapsuleCollider).radius;
            } else { // SphereCollider
                m_Radius = (m_Collider as SphereCollider).radius;
            }
        }

        /// <summary>
        /// Callback when the StateManager has changed the active state on the current object.
        /// </summary>
        public override void StateChange()
        {
            m_Transform.localPosition = m_LocalPosition + m_LocalPositionOffset;

            if (m_Collider is CapsuleCollider) {
                (m_Collider as CapsuleCollider).radius = m_Radius + m_RadiusOffset;
            } else { // SphereCollider
                (m_Collider as SphereCollider).radius = m_Radius + m_RadiusOffset;
            }
        }
    }
}