/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character
{
    using Opsive.Shared.Utility;
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.StateSystem;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Rotates and sets the CapsuleCollider height so it always matches the same relative location/size of the character.
    /// </summary>
    public class CapsuleColliderPositioner : StateBehavior
    {
        [Tooltip("Should the positioner rotate the collider to match the targets?")]
        [SerializeField] protected bool m_RotateCollider;
        [Tooltip("If Rotate Collider is enabled, should the collider be rotated based on the end caps? If false the collider will be rotated based on the specified bone.")]
        [SerializeField] protected bool m_EndCapRotation = true;
        [Tooltip("Specifies the bone that the collider should rotate with if not using End Cap Rotation.")]
        [SerializeField] protected Transform m_RotationBone;
        [Tooltip("An offset to apply to the rotation bone.")]
        [SerializeField] protected Vector3 m_RotationBoneOffset;
        [Tooltip("Should the positioner adjust the height of the collider?")]
        [SerializeField] protected bool m_AdjustHeight = true;
        [Tooltip("A reference to the target that is near the first end cap.")]
        [SerializeField] protected Transform m_FirstEndCapTarget;
        [Tooltip("A reference to the target that is near the second end cap.")]
        [SerializeField] protected Transform m_SecondEndCapTarget;
        [Tooltip("The padding on top of the second end cap target.")]
        [SerializeField] protected float m_SecondEndCapPadding;
        [Tooltip("Specifies the bone that the collider should positioned to if not using Adjust Height.")]
        [SerializeField] protected Transform m_PositionBone;
        [Tooltip("The overridden height if the height is not being adjusted. Set to -1 to disable.")]
        [SerializeField] protected float m_HeightOverride = -1;
        [Tooltip("The offset to apply to the collider's center position.")]
        [SerializeField] protected Vector3 m_CenterOffset;

        public bool RotateCollider { get { return m_RotateCollider; }
            set {
                if (m_RotateCollider != value) {
                    // Remember the original rotation so the value can be restored if the collider is no longer being rotated.
                    if (value) {
                        m_PrevLocalRotation = m_Transform.localRotation;
                        UpdateRotationHeight();
                    } else {
                        m_Transform.localRotation = m_PrevLocalRotation;
                    }
                    m_RotateCollider = value;
                }
            }
        }
        public bool EndCapRotation { get { return m_EndCapRotation; } set { m_EndCapRotation = value; } }
        [NonSerialized] public Transform RotationBone { get { return m_RotationBone; } set { m_RotationBone = value; } }
        public Vector3 RotationBoneOffset { get { return m_RotationBoneOffset; } set { m_RotationBoneOffset = value; } }
        public bool AdjustHeight { get { return m_AdjustHeight; } 
            set {
                if (m_AdjustHeight != value) {
                    m_AdjustHeight = value;
                    if (m_AdjustHeight) {
                        m_Transform.localPosition = Vector3.zero;
                        UpdateRotationHeight();
                    }
                }
            }
        }
        [NonSerialized] public Transform FirstEndCapTarget { get { return m_FirstEndCapTarget; } set { m_FirstEndCapTarget = value; } }
        [NonSerialized] public Transform SecondEndCapTarget { get { return m_SecondEndCapTarget; } set { m_SecondEndCapTarget = value; Initialize(); } }
        [NonSerialized] public float SecondEndCapPadding { get { return m_SecondEndCapPadding; } set { m_SecondEndCapPadding = value; } }
        [NonSerialized] public Transform PositionBone { get { return m_PositionBone; } set { m_PositionBone = value; } }
        public float HeightOverride { get { return m_HeightOverride; } set { m_HeightOverride = value; } }
        public Vector3 CenterOffset { get { return m_CenterOffset; }
            set {
                if (m_ColliderOffsetEvent != null) {
                    Scheduler.Cancel(m_ColliderOffsetEvent);
                    m_ColliderOffsetEvent = null;
                }
                AdjustCenterOffset(value);
            }
        }

        private Transform m_Transform;
        private CapsuleCollider m_CapsuleCollider;
        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private GameObject m_CharacterGameObject;
        private Transform m_CharacterTransform;
        private CharacterLayerManager m_CharacterLayerManager;

        private Collider[] m_OverlapColliders;
        private Vector3 m_FirstEndCapOffset;
        private Vector3 m_SecondEndCapOffset;
        private Vector3 m_FirstEndCapLocalPosition;
        private Quaternion m_PrevLocalRotation;
        private ScheduledEventBase m_ColliderOffsetEvent;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_Transform = transform;
            m_CapsuleCollider = GetComponent<CapsuleCollider>();
            if (m_CapsuleCollider.direction != 1) {
                Debug.LogError("Error: The CapsuleColliderPositioner only works with capsule colliders that are in the Y-axis direction.");
                enabled = false;
                return;
            }
            m_CharacterLocomotion = gameObject.GetCachedParentComponent<UltimateCharacterLocomotion>();
            m_CharacterGameObject = m_CharacterLocomotion.gameObject;
            m_CharacterTransform = m_CharacterLocomotion.transform;
            m_CharacterLayerManager = gameObject.GetCachedParentComponent<CharacterLayerManager>();

            m_OverlapColliders = new Collider[1];

            EventHandler.RegisterEvent(m_CharacterGameObject, "OnAnimatorSnapped", AnimatorSnapped);
            EventHandler.RegisterEvent<bool>(m_CharacterGameObject, "OnCharacterImmediateTransformChange", OnImmediateTransformChange);
            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_CharacterGameObject, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_CharacterGameObject, "OnRespawn", OnRespawn);

            Initialize();

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            // The positioner cannot be used with server autoritative implementations.
            var networkInfo = m_CharacterGameObject.GetCachedComponent<Networking.INetworkInfo>();
            if (networkInfo != null && networkInfo.IsServerAuthoritative()) {
                enabled = false;
                Debug.LogWarning("Warning: The CapsuleColliderPositioner has been disabled. Unity bug 985643 needs to be fixed for it to work over a server authoritative network.");
            }
#endif
        }

        /// <summary>
        /// The animator has snapped into position. Reinitialized.
        /// </summary>
        private void AnimatorSnapped()
        {
            // The positioner only needs to be initialized once.
            EventHandler.UnregisterEvent(m_CharacterGameObject, "OnAnimatorSnapped", AnimatorSnapped);

            Initialize();
        }

        /// <summary>
        /// Sets the initial end cap offsets.
        /// </summary>
        private void Initialize()
        {
            // If the first and second end cap targets are null then the component is likely added at runtime.
            if (!Application.isPlaying || (m_FirstEndCapTarget == null && m_SecondEndCapTarget == null)) {
                return;
            }

            // If the CapsuleCollider doesn't have a second end cap then the character doesn't have an animator. The end cap should be
            // a child GameObject so abilities (such as HeightChange) can adjust the collider height.
            if (m_SecondEndCapTarget == null) {
                m_SecondEndCapTarget = new GameObject("EndCap").transform;
                m_SecondEndCapTarget.SetParentOrigin(m_Transform);

                var localPosition = m_SecondEndCapTarget.localPosition;
                localPosition.y = m_CapsuleCollider.height;
                m_SecondEndCapTarget.localPosition = localPosition;

                EventHandler.RegisterEvent<float>(m_CharacterGameObject, "OnHeightChangeAdjustHeight", AdjustCapsuleColliderHeight);
            }

            Vector3 firstEndCap, secondEndCap;
            MathUtility.CapsuleColliderEndCaps(m_CapsuleCollider, m_Transform.position, m_Transform.rotation, out firstEndCap, out secondEndCap);

            if ((m_FirstEndCapTarget.position - firstEndCap).sqrMagnitude > (m_SecondEndCapTarget.position - firstEndCap).sqrMagnitude) {
                // The second target may be closer to the first end cap than the first target is. Switch the targets.
                var target = m_FirstEndCapTarget;
                m_FirstEndCapTarget = m_SecondEndCapTarget;
                m_SecondEndCapTarget = target;
            }
            m_Transform.position = m_FirstEndCapTarget.position;
            m_FirstEndCapLocalPosition = m_FirstEndCapTarget.localPosition;
            m_FirstEndCapOffset = m_Transform.InverseTransformDirection(m_FirstEndCapTarget.position - firstEndCap);
            m_SecondEndCapOffset = m_Transform.InverseTransformDirection(m_SecondEndCapTarget.position - secondEndCap) - m_CharacterTransform.up * m_SecondEndCapPadding;
        }

        /// <summary>
        /// Perform the rotation and height changes.
        /// </summary>
        private void FixedUpdate()
        {
            UpdateRotationHeight();
        }

        /// <summary>
        /// Does the actual rotation and height changes. This method is separate from FixedUpdate so the rotation/height can be determined 
        /// during server reconciliation on the network.
        /// </summary>
        public void UpdateRotationHeight()
        {
            m_CharacterLocomotion.EnableColliderCollisionLayer(false);
            // The first end cap may change positions.
            if (m_FirstEndCapTarget.localPosition != m_FirstEndCapLocalPosition) {
                m_Transform.position = m_FirstEndCapTarget.position;
                m_FirstEndCapLocalPosition = m_FirstEndCapTarget.localPosition;
            }

            Vector3 localDirection;
            if (m_RotateCollider) {
                if (m_EndCapRotation) {
                    // Update the rotation of the CapsuleCollider so it is rotated in the same direction as the end cap targets.
                    var direction = m_SecondEndCapTarget.position - m_FirstEndCapTarget.position;
                    m_Transform.rotation = Quaternion.LookRotation(Vector3.Cross(m_CharacterTransform.forward, direction.normalized), direction.normalized);
                } else {
                    m_Transform.rotation = m_RotationBone.rotation * Quaternion.Euler(m_RotationBoneOffset);
                }
            }

            if (m_AdjustHeight) {
                // After the CapsuleCollider has rotated determine the new height of the CapsuleCollider. This can be done by determining the current
                // end cap locations and then getting the offset from the start end cap offsets.
                Vector3 firstEndCap, secondEndCap;
                MathUtility.CapsuleColliderEndCaps(m_CapsuleCollider, m_Transform.position, m_Transform.rotation, out firstEndCap, out secondEndCap);
                var firstEndCapOffset = m_Transform.InverseTransformDirection(m_FirstEndCapTarget.position - firstEndCap);
                var secondEndCapOffset = m_Transform.InverseTransformDirection(m_SecondEndCapTarget.position - secondEndCap);
                var offset = m_SecondEndCapOffset - m_FirstEndCapOffset;
                localDirection = ((secondEndCapOffset - firstEndCapOffset) - offset);

                // Determine if the new height would cause any collisions. If it does not then apply the height changes. A negative height change will never cause any
                // collisions so the OverlapCapsule does not need to be checked. A valid capsule collider height is always greater than 2 times the radius of the collider.
                var heightMultiplier = MathUtility.CapsuleColliderHeightMultiplier(m_CapsuleCollider);
                var targetHeight = m_CapsuleCollider.height + localDirection.y / heightMultiplier;
                if (targetHeight >= m_CapsuleCollider.radius * 2 && (localDirection.y < 0 || !m_CharacterLocomotion.UsingVerticalCollisionDetection ||
                            Physics.OverlapCapsuleNonAlloc(firstEndCap, secondEndCap + m_CharacterLocomotion.Up * localDirection.y,
                                        m_CapsuleCollider.radius * MathUtility.ColliderRadiusMultiplier(m_CapsuleCollider), m_OverlapColliders, m_CharacterLayerManager.SolidObjectLayers,
                                        QueryTriggerInteraction.Ignore) == 0)) {
                    // Adjust the CapsuleCollider height and center to account for the new offset.
                    m_CapsuleCollider.height = targetHeight;
                    var center = m_CapsuleCollider.center;
                    center.y += localDirection.y / (heightMultiplier * 2);
                    m_CapsuleCollider.center = center;
                }
            } else {
                if (m_PositionBone != null) {
                    m_Transform.position = MathUtility.TransformPoint(m_PositionBone.position, m_Transform.rotation, -m_CapsuleCollider.center);
                }
                if (m_HeightOverride != -1) {
                    m_CapsuleCollider.height = m_HeightOverride;
                    var center = m_CapsuleCollider.center;
                    center.y = m_CapsuleCollider.height / 2 + m_CenterOffset.y;
                    m_CapsuleCollider.center = center;
                }
            }
            m_CharacterLocomotion.EnableColliderCollisionLayer(true);
        }

        /// <summary>
        /// Adjusts the collider height by the given amount.
        /// </summary>
        /// <param name="amount">The amount to adjust the collider height by.</param>
        private void AdjustCapsuleColliderHeight(float amount)
        {
            var localPosition = m_SecondEndCapTarget.localPosition;
            localPosition.y = m_CapsuleCollider.height + amount;
            m_SecondEndCapTarget.localPosition = localPosition;
        }

        /// <summary>
        /// Adjusts the collider's center position to the specified value.
        /// </summary>
        /// <param name="targetOffset">The desired offset value.</param>
        private void AdjustCenterOffset(Vector3 targetOffset)
        {
            var delta = targetOffset - m_CenterOffset;
            m_CapsuleCollider.center += delta;
            m_CapsuleCollider.height += delta.y / 2;

            if (!m_CharacterLocomotion.UsingHorizontalCollisionDetection) {
                m_CenterOffset = targetOffset;
                return;
            }

            // Apply the offset if there are no collisions.
            var collisionEnabled = m_CharacterLocomotion.CollisionLayerEnabled;
            m_CharacterLocomotion.EnableColliderCollisionLayer(false);
            Vector3 firstEndCap, secondEndCap;
            MathUtility.CapsuleColliderEndCaps(m_CapsuleCollider, m_Transform.position, m_Transform.rotation, out firstEndCap, out secondEndCap);
            if (Physics.OverlapCapsuleNonAlloc(firstEndCap, secondEndCap, m_CapsuleCollider.radius * MathUtility.ColliderRadiusMultiplier(m_CapsuleCollider), 
                                    m_OverlapColliders, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore) > 0) {
                m_CapsuleCollider.center -= delta;
                m_CapsuleCollider.height -= delta.y / 2;
                m_ColliderOffsetEvent = Scheduler.Schedule(Time.fixedDeltaTime, AdjustCenterOffset, targetOffset);
            } else {
                m_CenterOffset = targetOffset;
            }
            m_CharacterLocomotion.EnableColliderCollisionLayer(collisionEnabled);
        }

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            enabled = false;
        }

        /// <summary>
        /// The character has respawned.
        /// </summary>
        private void OnRespawn()
        {
            enabled = true;
        }

        /// <summary>
        /// The character's position or rotation has been teleported.
        /// </summary>
        /// <param name="snapAnimator">Should the animator be snapped?</param>
        private void OnImmediateTransformChange(bool snapAnimator)
        {
            if (!enabled) {
                return;
            }
            UpdateRotationHeight();
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Scheduler.Cancel(m_ColliderOffsetEvent);

            EventHandler.UnregisterEvent(m_CharacterGameObject, "OnAnimatorSnapped", Initialize);
            EventHandler.UnregisterEvent<float>(m_CharacterGameObject, "OnHeightChangeAdjustHeight", AdjustCapsuleColliderHeight);
            EventHandler.UnregisterEvent<bool>(m_CharacterGameObject, "OnCharacterImmediateTransformChange", OnImmediateTransformChange);
            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_CharacterGameObject, "OnDeath", OnDeath);
            EventHandler.UnregisterEvent(m_CharacterGameObject, "OnRespawn", OnRespawn);
        }
    }
}