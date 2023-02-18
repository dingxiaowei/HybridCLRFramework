/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Objects;
    using Opsive.UltimateCharacterController.Utility;
    using Opsive.UltimateCharacterController.StateSystem;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The CharacterLocomotion class serves as the base character controller class. It handles the base locomotion with the following features:
    /// - Movement
    /// - Collision Detection
    /// - Root Motion
    /// - Wall Bouncing
    /// - Wall Gliding
    /// - Slopes
    /// - Stairs
    /// - Push Rigidbodies
    /// - Gravity Direction
    /// - Variable Time Scale
    /// - CapsuleCollider and SphereCollider support (for generic characters)
    /// - Moving Platforms
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public abstract class CharacterLocomotion : StateBehavior, IForceObject
    {
        // Padding value used to prevent the character's collider from overlapping the environment collider. Overlapped colliders don't work well with ray casts.
        private const float c_ColliderSpacing = 0.01f;
        private const float c_ColliderSpacingCubed = c_ColliderSpacing * c_ColliderSpacing * c_ColliderSpacing * c_ColliderSpacing;
        // Padding value used to add a bit of spacing for the slope limit.
        private const float c_SlopeLimitSpacing = 0.3f;

        [Tooltip("Should root motion be used to move the character?")]
        [SerializeField] protected bool m_UseRootMotionPosition;
        [Tooltip("If using root motion, applies a multiplier to the root motion delta position while on the ground.")]
        [SerializeField] protected float m_RootMotionSpeedMultiplier = 1;
        [Tooltip("If using root motion, applies a multiplier to the root motion delta position while in the air.")]
        [SerializeField] protected float m_RootMotionAirForceMultiplier = 1;
        [Tooltip("Should root motion be used to rotate the character?")]
        [SerializeField] protected bool m_UseRootMotionRotation;
        [Tooltip("If using root motion, applies a multiplier to the root motion delta rotation.")]
        [SerializeField] protected float m_RootMotionRotationMultiplier = 1;
        [Tooltip("The rate at which the character can rotate. Only used by non-root motion characters.")]
        [SerializeField] protected float m_MotorRotationSpeed = 10;
        [Tooltip("The mass of the character.")]
        [SerializeField] protected float m_Mass = 100;
        [Tooltip("Specifies the width of the characters skin, use for ground detection.")]
        [SerializeField] protected float m_SkinWidth = 0.08f;
        [Tooltip("The maximum object slope angle that the character can traverse (in degrees).")]
        [SerializeField] protected float m_SlopeLimit = 40f;
        [Tooltip("The maximum height that the character can step on top of.")]
        [SerializeField] protected float m_MaxStepHeight = 0.35f;
        [Tooltip("The rate at which the character's motor force accelerates while on the ground. Only used by non-root motion characters.")]
        [SerializeField] protected Vector3 m_MotorAcceleration = new Vector3(0.18f, 0, 0.18f);
        [Tooltip("The rate at which the character's motor force decelerates while on the ground. Only used by non-root motion characters.")]
        [SerializeField] protected float m_MotorDamping = 0.27f;
        [Tooltip("The rate at which the character's motor force accelerates while in the air. Only used by non-root motion characters.")]
        [SerializeField] protected Vector3 m_MotorAirborneAcceleration = new Vector3(0.18f, 0, 0.18f);
        [Tooltip("The rate at which the character's motor force decelerates while in the air. Only used by non-root motion characters.")]
        [SerializeField] protected float m_MotorAirborneDamping = 0.01f;
        [Tooltip("A multiplier which is applied to the motor while moving backwards.")]
        [SerializeField] protected float m_MotorBackwardsMultiplier = 0.7f;
        [Tooltip("A (0-1) value specifying the amount of influence the previous acceleration direction has on the current velocity.")]
        [SerializeField] protected float m_PreviousAccelerationInfluence = 1;
        [Tooltip("Should the motor force be adjusted while on a slope?")]
        [SerializeField] protected bool m_AdjustMotorForceOnSlope = true;
        [Tooltip("If adjusting the motor force on a slope, the force multiplier when on an upward slope.")]
        [SerializeField] protected float m_MotorSlopeForceUp = 1f;
        [Tooltip("If adjusting the motor force on a slope, the force multiplier when on a downward slope.")]
        [SerializeField] protected float m_MotorSlopeForceDown = 1.25f;
        [Tooltip("The rate at which the character's external force decelerates.")]
        [SerializeField] protected float m_ExternalForceDamping = 0.1f;
        [Tooltip("The rate at which the character's external force decelerates while in the air.")]
        [SerializeField] protected float m_ExternalForceAirDamping = 0.05f;
        [Tooltip("Should the character stick to the ground?")]
        [SerializeField] protected bool m_StickToGround = true;
        [Tooltip("If the character is sticking to the ground, specifies how sticky the ground is. A higher value means the ground is more sticky.")]
        [SerializeField] protected float m_Stickiness = 0.2f;
        [Tooltip("The local time scale of the character.")]
        [SerializeField] protected float m_TimeScale = 1;
        [Tooltip("Should gravity be applied?")]
        [SerializeField] protected bool m_UseGravity = true;
        [Tooltip("The normalized direction of the gravity force.")]
        [SerializeField] protected Vector3 m_GravityDirection = new Vector3(0, -1, 0);
        [Tooltip("A amount of gravity force to apply.")]
        [SerializeField] protected float m_GravityMagnitude = 3.92f;
        [Tooltip("Can the character detect horizontal collisions?")]
        [SerializeField] protected bool m_DetectHorizontalCollisions = true;
        [Tooltip("Can the character detect vertical collisions?")]
        [SerializeField] protected bool m_DetectVerticalCollisions = true;
        [Tooltip("The layers that can act as colliders for the character.")]
        [SerializeField] protected LayerMask m_ColliderLayerMask = ~(1 << LayerManager.IgnoreRaycast | 1 << LayerManager.SubCharacter | 
                                                                     1 << LayerManager.Overlay | 1 << LayerManager.VisualEffect);
        [Tooltip("The maximum number of colliders that the character can detect.")]
        [SerializeField] protected int m_MaxCollisionCount = 100;
        [Tooltip("The maximum number of frames that the soft force can be distributed by.")]
        [SerializeField] protected int m_MaxSoftForceFrames = 100;
        [Tooltip("The maximum number of collision checks that should be performed when rotating.")]
        [SerializeField] protected int m_RotationCollisionCheckCount = 10;
        [Tooltip("The maximum number of iterations to detect collision overlaps.")]
        [SerializeField] protected int m_MaxOverlapIterations = 6;
        [Tooltip("A curve specifying the amount to move when gliding along a wall. The x variable represents the dot product between the character look direction and wall normal. " +
                 "An x value of 0 means the character is looking directly at the wall. An x value of 1 indicates the character is looking parallel to the wall.")]
        [SerializeField] protected AnimationCurve m_WallGlideCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(0.1f, 1.5f, 0, 0), new Keyframe(1, 1.5f) });
        [Tooltip("A multiplier to apply when hitting a wall. Allows for the character to bounce off of a wall.")]
        [SerializeField] protected float m_WallBounceModifier = 1;

        [Tooltip("Should the character stick to the moving platform? If false the character will inherit the moving platform's momentum when the platform stops quickly.")]
        [SerializeField] protected bool m_StickToMovingPlatform = true;
        [Tooltip("The velocity magnitude required for the character to separate from the moving platform due to a sudden moving platform stop.")]
        [SerializeField] protected float m_MovingPlatformSeperationVelocity = 5;
        [Tooltip("The maximum speed of the platform that the character should stick when the platform collides with the character from a horizontal position.")]
        [SerializeField] protected float m_MinHorizontalMovingPlatformStickSpeed = 2;
        [Tooltip("The rate at which the character's moving platform force decelerates when the character is no longer on the platform.")]
        [SerializeField] protected float m_MovingPlatformForceDamping;

        [Tooltip("An array of bones that should be smoothed by the Kinematic Object Manager.")]
        [SerializeField] protected Transform[] m_SmoothedBones;

        public float ColliderSpacing { get { return c_ColliderSpacing; } }
        public float ColliderSpacingCubed { get { return c_ColliderSpacingCubed; } }
        public float SlopeLimitSpacing { get { return c_SlopeLimitSpacing; } }
        public bool UseRootMotionPosition { get { return m_UseRootMotionPosition; } set { m_UseRootMotionPosition = value; } }
        public float RootMotionSpeedMultiplier { get { return m_RootMotionSpeedMultiplier; } set { m_RootMotionSpeedMultiplier = value; } }
        public float RootMotionAirForceMultiplier { get { return m_RootMotionAirForceMultiplier; } set { m_RootMotionAirForceMultiplier = value; } }
        public bool UseRootMotionRotation { get { return m_UseRootMotionRotation; } set { m_UseRootMotionRotation = value; } }
        public float RootMotionRotationMultiplier { get { return m_RootMotionRotationMultiplier; } set { m_RootMotionRotationMultiplier = value; } }
        public float MotorRotationSpeed { get { return m_MotorRotationSpeed; } set { m_MotorRotationSpeed = value; } }
        public float Mass { get { return m_Mass; } set { m_Mass = value; } }
        public float SkinWidth { get { return m_SkinWidth; } set { m_SkinWidth = value; } }
        public float SlopeLimit { get { return m_SlopeLimit; } set { m_SlopeLimit = value; } }
        public float MaxStepHeight { get { return m_MaxStepHeight; } set { m_MaxStepHeight = value; } }
        public Vector3 MotorAcceleration { get { return m_MotorAcceleration; } set { m_MotorAcceleration = value; } }
        public float MotorDamping { get { return m_MotorDamping; } set { m_MotorDamping = value; } }
        public Vector3 MotorAirborneAcceleration { get { return m_MotorAirborneAcceleration; } set { m_MotorAirborneAcceleration = value; } }
        public float MotorAirborneDamping { get { return m_MotorAirborneDamping; } set { m_MotorAirborneDamping = value; } }
        public float MotorBackwardsMultiplier { get { return m_MotorBackwardsMultiplier; } set { m_MotorBackwardsMultiplier = value; } }
        public float PreviousAccelerationInfluence { get { return m_PreviousAccelerationInfluence; } set { m_PreviousAccelerationInfluence = value; } }
        public bool AdjustMotorForceOnSlope { get { return m_AdjustMotorForceOnSlope; } set { m_AdjustMotorForceOnSlope = value; } }
        public float MotorSlopeForceUp { get { return m_MotorSlopeForceUp; } set { m_MotorSlopeForceUp = value; } }
        public float MotorSlopeForceDown { get { return m_MotorSlopeForceDown; } set { m_MotorSlopeForceDown = value; } }
        public float ExternalForceDamping { get { return m_ExternalForceDamping; } set { m_ExternalForceDamping = value; } }
        public float ExternalForceAirDamping { get { return m_ExternalForceAirDamping; } set { m_ExternalForceAirDamping = value; } }
        public bool StickToGround { get { return m_StickToGround; } set { m_StickToGround = value; } }
        public float Stickiness { get { return m_Stickiness; } set { m_Stickiness = value; } }
        public virtual float TimeScale { get { return m_TimeScale; } set { m_TimeScale = value; } }
        public bool UseGravity { get { return m_UseGravity; } set { m_UseGravity = value; } }
        public Vector3 GravityDirection { get { return m_GravityDirection; } set { m_GravityDirection = value; m_GravityDirection.Normalize(); } }
        public float GravityMagnitude { get { return m_GravityMagnitude; } set { m_GravityMagnitude = value; } }
        public bool DetectHorizontalCollisions { get { return m_DetectHorizontalCollisions; } set { m_DetectHorizontalCollisions = value; } }
        public bool DetectVerticalCollisions { get { return m_DetectVerticalCollisions; } set { m_DetectVerticalCollisions = value; } }
        public LayerMask ColliderLayerMask { get { return m_ColliderLayerMask; } set { m_ColliderLayerMask = value; } }
        public int MaxCollisionCount { get { return m_MaxCollisionCount; } }
        public int MaxSoftForceFrames { get { return m_MaxSoftForceFrames; } }
        public int RotationCollisionCheckCount { get { return m_RotationCollisionCheckCount; } set { m_RotationCollisionCheckCount = value; } }
        public int MaxOverlapIterations { get { return m_MaxOverlapIterations; } set { m_MaxOverlapIterations = value; } }
        public AnimationCurve WallGlideCurve { get { return m_WallGlideCurve; } set { m_WallGlideCurve = value; } }
        public float WallBounceModifier { get { return m_WallBounceModifier; } set { m_WallBounceModifier = value; } }
        public bool StickToMovingPlatform { get { return m_StickToMovingPlatform; } set { m_StickToMovingPlatform = value; } }
        public float MovingPlatformSeperationVelocity { get { return m_MovingPlatformSeperationVelocity; } set { m_MovingPlatformSeperationVelocity = value; } }
        public float MinHorizontalMovingPlatformStickSpeed { get { return m_MinHorizontalMovingPlatformStickSpeed; } set { m_MinHorizontalMovingPlatformStickSpeed = value; } }
        public float MovingPlatformForceDamping { get { return m_MovingPlatformForceDamping; } set { m_MovingPlatformForceDamping = value; } }
        public Transform[] SmoothedBones { get { return m_SmoothedBones; } }

        protected Transform m_Transform;
        private Rigidbody m_Rigidbody;
        protected AnimatorMonitor m_AnimatorMonitor;
        protected Collider[] m_Colliders;
        private Collider[] m_IgnoredColliders;
        protected int m_ColliderCount;
        private int m_IgnoredColliderCount;
        private GameObject[] m_ColliderGameObjects;
        private GameObject[] m_IgnoredColliderGameObjects;
        protected CharacterLayerManager m_CharacterLayerManager;
        private RaycastHit m_GroundRaycastHit;
        private Vector3 m_GroundRaycastOrigin;
        private Transform m_GroundHitTransform;
        private RaycastHit m_HorizontalRaycastHit;
        protected RaycastHit[] m_RaycastHits;
        private RaycastHit[] m_CombinedRaycastHits;
        private Collider[] m_OverlapColliderHit;
        private RaycastHit m_BlankRaycastHit = new RaycastHit();
        private Ray m_SlopeRay = new Ray();
        private RaycastHit m_RaycastHit;

        private Vector3 m_Up;
        private float m_Height;
        private float m_Radius = float.MaxValue;
        private Vector3 m_MotorThrottle;
        private Quaternion m_MotorRotation;
        private Quaternion m_PrevMotorRotation;
        private Vector3 m_MoveDirection;
        protected Quaternion m_Torque = Quaternion.identity;
        private Vector3 m_PrevPosition;
        private Vector3 m_Velocity;
        private bool m_CheckRotationCollision;
        private bool m_AllowUseGravity = true;
        private bool m_ForceUseGravity;
        private bool m_AllowRootMotionPosition = true;
        private bool m_AllowRootMotionRotation = true;
        private bool m_ForceRootMotionPosition;
        private bool m_ForceRootMotionRotation;
        private bool m_AllowHorizontalCollisionDetection = true;
        private bool m_ForceHorizontalCollisionDetection;
        private bool m_AllowVerticalCollisionDetection = true;
        private bool m_ForceVerticalCollisionDetection;
        protected Vector3 m_AnimatorDeltaPosition;
        protected Quaternion m_AnimatorDeltaRotation;
        private Vector3 m_LocalRootMotionForce;
        private Quaternion m_LocalRootMotionRotation = Quaternion.identity;
        private float m_SlopeFactor = 1;

        private Vector3 m_ExternalForce;
        private float m_GravityAmount;
        private Vector3[] m_SoftForceFrames;
        protected UnityEngineUtility.RaycastHitComparer m_RaycastHitComparer = new UnityEngineUtility.RaycastHitComparer();
        private Dictionary<RaycastHit, int> m_ColliderIndexMap;
        private int[] m_ColliderLayers;
        private int[] m_IgnoredColliderLayers;

        protected Vector2 m_InputVector;
        protected Vector3 m_DeltaRotation;
        protected bool m_Grounded = true;
        private bool m_CollisionLayerEnabled = true;
        private bool m_AlignToGravity;
        private bool m_SmoothGravityYawDelta = true;
        private bool m_ManualMove;

        protected Transform m_Platform;
        private Vector3 m_PlatformRelativePosition;
        private Quaternion m_PlatformRotationOffset;
        protected Quaternion m_PlatformTorque = Quaternion.identity;
        private Vector3 m_PlatformMovement;
        private Vector3 m_PlatformVelocity;
        private bool m_GroundedMovingPlatform;
        private bool m_PlatformOverride;

        public Collider[] Colliders { get { return m_Colliders; } }
        public Collider[] IgnoredColliders { get { return m_IgnoredColliders; } }
        public int ColliderCount { get { return m_ColliderCount; } }
        public bool AllowUseGravity { set { m_AllowUseGravity = value; } }
        public bool ForceUseGravity { set { m_ForceUseGravity = value; } }
        public bool UsingGravity { get { return (m_UseGravity || m_ForceUseGravity) && m_AllowUseGravity; } }
        public bool AllowRootMotionPosition { set { m_AllowRootMotionPosition = value; } }
        public bool ForceRootMotionPosition { set { m_ForceRootMotionPosition = value; } }
        public bool UsingRootMotionPosition { get { return (m_UseRootMotionPosition || m_ForceRootMotionPosition) && m_AllowRootMotionPosition; } }
        public bool AllowRootMotionRotation { set { m_AllowRootMotionRotation = value; } }
        public bool ForceRootMotionRotation { set { m_ForceRootMotionRotation = value; } }
        public bool UsingRootMotionRotation { get { return (m_UseRootMotionRotation || m_ForceRootMotionRotation) && m_AllowRootMotionRotation; } }
        public Vector3 AnimatorDeltaPosition { get { return m_AnimatorDeltaPosition; } set { m_AnimatorDeltaPosition = value; } }
        public Quaternion AnimatorDeltaRotation { get { return m_AnimatorDeltaRotation; } set { m_AnimatorDeltaRotation = value; } }
        public bool AllowHorizontalCollisionDetection { set { m_AllowHorizontalCollisionDetection = value; } }
        public bool ForceHorizontalCollisionDetection { set { m_ForceHorizontalCollisionDetection = value; } }
        public bool UsingHorizontalCollisionDetection { get { return (m_DetectHorizontalCollisions || m_ForceHorizontalCollisionDetection) && m_AllowHorizontalCollisionDetection; } }
        public bool AllowVerticalCollisionDetection { set { m_AllowVerticalCollisionDetection = value; } }
        public bool ForceVerticalCollisionDetection { set { m_ForceVerticalCollisionDetection = value; } }
        public bool UsingVerticalCollisionDetection { get { return (m_DetectVerticalCollisions || m_ForceVerticalCollisionDetection) && m_AllowVerticalCollisionDetection; } }
        [Shared.Utility.NonSerialized] public Vector3 MoveDirection { get { return m_MoveDirection; } set { m_MoveDirection = value; } }
        [Shared.Utility.NonSerialized] public Quaternion Torque { get { return m_Torque; } set { m_Torque = value; } }
        public Vector3 LocomotionVelocity { get { return m_Velocity - m_PlatformVelocity; } protected set { m_Velocity = value; } }
        public Vector3 LocalLocomotionVelocity { get { return m_Transform.InverseTransformDirection(m_Velocity - m_PlatformVelocity); } }
        [Snapshot] public Vector3 Up { get { return m_Up; } protected set { m_Up = value; } }
        public float Height { get { return m_Height; } }
        public float Radius { get { return m_Radius; } }
        public Vector3 Center { get { return m_Transform.InverseTransformPoint(m_Transform.position + (m_Up * m_Height / 2)); } }
        [Snapshot] public bool Grounded { get { return m_Grounded; } protected set { m_Grounded = value; } }
        public Vector3 LocalExternalForce { get { return m_Transform.InverseTransformDirection(m_ExternalForce); } }
        public RaycastHit GroundRaycastHit { get { return m_GroundRaycastHit; } }
        public RaycastHit HorizontalRaycastHit { get { return m_HorizontalRaycastHit; } }
        public float TimeScaleSquared { get { return m_TimeScale * m_TimeScale; } }
        public Transform Platform { get { return m_Platform; } }
        public Vector3 PlatformMovement { get { return m_PlatformMovement; } }
        public Quaternion PlatformTorque { get { return m_PlatformTorque; } }
        public bool CollisionLayerEnabled { get { return m_CollisionLayerEnabled; } }
        [Opsive.Shared.Utility.NonSerialized] public bool AlignToGravity { get { return m_AlignToGravity; } set { m_AlignToGravity = value; } }
        [Opsive.Shared.Utility.NonSerialized] public bool SmoothGravityYawDelta { get { return m_SmoothGravityYawDelta; } set { m_SmoothGravityYawDelta = value; } }
        [Opsive.Shared.Utility.NonSerialized] public bool ManualMove { get { return m_ManualMove; } set { m_ManualMove = value; } }
        [Snapshot] public Vector3 PlatformVelocity { get { return m_PlatformVelocity; } set { m_PlatformVelocity = value; } }
        [Snapshot] protected float SlopeFactor { get { return m_SlopeFactor; } set { m_SlopeFactor = value; } }

        // The Velocity property will return the combined character locomotion and platform velocity.
        // For only the character's locmotion velocity use the LocomotionVelocity property.
        [Opsive.Shared.Utility.NonSerialized] public Vector3 Velocity { get { return m_Velocity; } set { m_Velocity = value; } }
        public Vector3 LocalVelocity { get { return m_Transform.InverseTransformDirection(m_Velocity); } }

        [Snapshot] public Vector3 MotorThrottle { get { return m_MotorThrottle; } set { m_MotorThrottle = value; } }
        [Snapshot] public Quaternion MotorRotation { get { return m_MotorRotation; } set { m_MotorRotation = value; } }
        [Snapshot] public Quaternion PrevMotorRotation { get { return m_PrevMotorRotation; } set { m_PrevMotorRotation = value; } }
        [Snapshot] public Vector3 ExternalForce { get { return m_ExternalForce; } protected set { m_ExternalForce = value; } }
        [Snapshot] public float GravityAmount { get { return m_GravityAmount; } set { m_GravityAmount = value; } }
        [Snapshot] protected Vector3 PrevPosition { get { return m_PrevPosition; } set { m_PrevPosition = value; } }
        [Snapshot] protected Vector3 PlatformRelativePosition { get { return m_PlatformRelativePosition; } set { m_PlatformRelativePosition = value; } }

        /// <summary>
        /// Cache the component references and initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            m_Transform = transform;
            m_CharacterLayerManager = gameObject.GetCachedComponent<CharacterLayerManager>();
            m_AnimatorMonitor = gameObject.GetCachedComponent<AnimatorMonitor>();

            base.Awake();

            m_Up = m_Transform.up;
            m_PrevPosition = m_Transform.position;
            m_MotorRotation = m_PrevMotorRotation = m_Transform.rotation;
            m_GravityDirection = -m_Up;

            // The Rigidbody is only used to notify Unity that the character isn't static. The Rigidbody doesn't control any movement.
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Rigidbody.mass = m_Mass;
            m_Rigidbody.isKinematic = true;
            m_Rigidbody.constraints = RigidbodyConstraints.FreezeAll;

            var colliders = GetComponentsInChildren<Collider>();
            m_Colliders = new Collider[colliders.Length];
            m_IgnoredColliders = new Collider[colliders.Length];
            m_CheckRotationCollision = GetComponentInChildren<CapsuleColliderPositioner>(true) != null;
            for (int i = 0; i < colliders.Length; ++i) {
                // There are a variety of colliders which should be ignored. Only CapsuleCollider and SphereColliders are supported.
                if (!colliders[i].enabled || colliders[i].isTrigger) {
                    continue;
                }
                // Sub colliders are parented to the character but they are not used for collision detection.
                if (!MathUtility.InLayerMask(colliders[i].gameObject.layer, m_ColliderLayerMask) || !(colliders[i] is CapsuleCollider || colliders[i] is SphereCollider)) {
                    m_IgnoredColliders[m_IgnoredColliderCount] = colliders[i];
                    m_IgnoredColliderCount++;
                    continue;
                }
                m_Colliders[m_ColliderCount] = colliders[i];
                m_ColliderCount++;

                // Determine the max height of the character based on the collider.
                var height = MathUtility.LocalColliderHeight(m_Transform, colliders[i]);
                if (height > m_Height) {
                    m_Height = height;
                }

                // Determine the mim radius of the character.
                var radius = float.MaxValue;
                if (colliders[i] is CapsuleCollider) {
                    radius = (colliders[i] as CapsuleCollider).radius;
                } else { // SphereCollider.
                    radius = (colliders[i] as SphereCollider).radius;
                }
                if (radius < m_Radius) {
                    m_Radius = radius;
                }

                // The rotation collider check only needs to be checked if the collider rotates on an axis other than the relative-y axis.
                if (!m_CheckRotationCollision) {
                    m_CheckRotationCollision = CanCauseRotationCollision(colliders[i]);
                }
            }

            // Resize the array depending on the number of valid colliders.
            if (m_Colliders.Length != m_ColliderCount) {
                Array.Resize(ref m_Colliders, m_ColliderCount);
            }
            if (m_Colliders.Length == 0) {
                Debug.LogWarning($"Warning: The character {name} doesn't contain any colliders. Only capsule and sphere colliders are supported.", this);
            }
            if (m_IgnoredColliders.Length != m_IgnoredColliderCount) {
                Array.Resize(ref m_IgnoredColliders, m_IgnoredColliderCount);
            }
            // Cache the collider GameObjects for best performance.
            m_ColliderGameObjects = new GameObject[m_Colliders.Length];
            for (int i = 0; i < m_ColliderGameObjects.Length; ++i) {
                m_ColliderGameObjects[i] = m_Colliders[i].gameObject;
            }
            m_IgnoredColliderGameObjects = new GameObject[m_IgnoredColliders.Length];
            for (int i = 0; i < m_IgnoredColliderGameObjects.Length; ++i) {
                m_IgnoredColliderGameObjects[i] = m_IgnoredColliders[i].gameObject;
            }

            m_RaycastHits = new RaycastHit[m_MaxCollisionCount];
            // If there are multiple colliders then save a mapping between the raycast hit and the collider index.
            if (m_Colliders.Length > 1) {
                m_ColliderIndexMap = new Dictionary<RaycastHit, int>(new UnityEngineUtility.RaycastHitEqualityComparer());
                m_CombinedRaycastHits = new RaycastHit[m_RaycastHits.Length * m_Colliders.Length];
            }
            m_ColliderLayers = new int[m_Colliders.Length];
            m_IgnoredColliderLayers = new int[m_IgnoredColliders.Length];
            m_OverlapColliderHit = new Collider[m_MaxCollisionCount];
            m_SoftForceFrames = new Vector3[m_MaxSoftForceFrames];
        }

        /// <summary>
        /// The character has been enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            ResetRotationPosition();
        }

        /// <summary>
        /// Can the specified collider cause a collision when the character is rotating? The collider can cause a rotation collision when it would rotate on
        /// an axis other than the relative-y axis.
        /// </summary>
        /// <param name="collider">The collider to check against.</param>
        /// <returns>True if the collider could cause a rotation collision.</returns>
        private bool CanCauseRotationCollision(Collider collider)
        {
            Vector3 direction;
            if (collider is CapsuleCollider) {
                Vector3 startEndCap, endEndCap;
                var capsuleCollider = collider as CapsuleCollider;
                // The CapsuleCollider's end caps and the center position must be on the same relative-y axis.
                direction = m_Transform.InverseTransformDirection(collider.transform.TransformPoint((collider as CapsuleCollider).center) - m_Transform.position);
                if (Mathf.Abs(direction.x) > 0.0001f || Mathf.Abs(direction.z) > 0.0001f) {
                    return true;
                }
                MathUtility.CapsuleColliderEndCaps(capsuleCollider, capsuleCollider.transform.position, capsuleCollider.transform.rotation, out startEndCap, out endEndCap);
                direction = m_Transform.InverseTransformDirection(startEndCap - endEndCap);
            } else { // SphereCollider.
                direction = m_Transform.InverseTransformDirection(collider.transform.TransformPoint((collider as SphereCollider).center) - m_Transform.position);
            }
            if (Mathf.Abs(direction.x) > 0.0001f || Mathf.Abs(direction.z) > 0.0001f) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determine if the character is on the ground when the game begins.
        /// </summary>
        public virtual void Start()
        {
            // Disable the colliders to prevent the grounded check from hitting the character.
            EnableColliderCollisionLayer(false);

            // Update the grounded state so it is accurate on the first run.
            UpdateGroundState(CheckGround(), false);
            // CheckGround updates the MoveDirection. This value should be reset because it's not applied until Move.
            m_MoveDirection = Vector3.zero;

            // Reenable the disabled colliders.
            EnableColliderCollisionLayer(true);
        }

        /// <summary>
        /// Moves the character according to the input. This method exists to allow AI to easily move the character instead of having to go through
        /// the ControllerHandler.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="forwardMovement">-1 to 1 value specifying the amount of forward movement.</param>
        /// <param name="targetRotation">Value specifying the amount of yaw rotation change.</param>
        public virtual void Move(float horizontalMovement, float forwardMovement, float deltaYawRotation)
        {
            if (m_TimeScale == 0 || Time.deltaTime == 0) {
                return;
            }

            // Assign the inputs.
            m_InputVector.x = horizontalMovement;
            m_InputVector.y = forwardMovement;
            m_DeltaRotation.Set(0, deltaYawRotation, 0);
            //m_InputVector.x = 1;
            //m_InputVector.y = 1;
            //m_DeltaYawRotation = 5;

            // Disable the colliders to prevent any subsequent raycasts from hitting the character.
            EnableColliderCollisionLayer(false);

            // Provide a callback for when the UltimateCharacterLocomotion subclass should perform its updates (such as updating the abilities).
            UpdateUltimateLocomotion();

            // Update the animator so the correct animations will play.
            UpdateAnimator();

            // Update the position and rotation after the animator has updated.
            UpdatePositionAndRotation(false);

            // Update the external forces after the movement has been applied.
            // This should be done after the movement is applied so the full force value has been applied within UpdateMovemnet.
            UpdateExternalForces();

            // Reenable the disabled colliders.
            EnableColliderCollisionLayer(true);
        }

        /// <summary>
        /// Provides a virtual method for the UltimateCharacterLocomotion to perform its updates (such as updating the abilities).
        /// </summary>
        protected virtual void UpdateUltimateLocomotion() { }

        /// <summary>
        /// Provides a virtual method to update the animator parameters.
        /// </summary>
        protected virtual void UpdateAnimator() { }

        /// <summary>
        /// Updates the position and rotation. This should be done after the animator has updated so the root motion is accurate for the current frame.
        /// </summary>
        /// <param name="fromAnimatorMove">Is the position and rotation being updated from the animator move method?</param>
        /// <returns>True if the position and rotation were updated.</returns>
        private void UpdatePositionAndRotation(bool fromAnimatorMove)
        {
            if (m_AnimatorMonitor != null && m_AnimatorMonitor.AnimatorEnabled) {
                // When the character is being moved manually it should always be updated within the main Move loop.
                if (fromAnimatorMove == m_ManualMove) {
                    return;
                }
            }

            UpdatePositionAndRotation();
        }

        /// <summary>
        /// Updates the position and rotation. This should be done after the animator has updated so the root motion is accurate for the current frame.
        /// </summary>
        protected virtual void UpdatePositionAndRotation()
        {
            // Depending on when OnAnimatorMove is called the collision layer may not already be disabled.
            var collisionLayerEnabled = m_CollisionLayerEnabled;
            EnableColliderCollisionLayer(false);

            // Before any other movements are done the character should first stay aligned to any moving platforms.
            UpdatePlatformMovement();

            // Update the rotation before the position so the forces will be applied in the correct direction.
            UpdateRotation();

            // Apply the resulting rotation changes.
            ApplyRotation();

            // Update all forces and check for collisions.
            UpdatePosition();

            // Apply the resulting position changes.
            ApplyPosition();

            // Rever the collision layer to the previous value.
            EnableColliderCollisionLayer(collisionLayerEnabled);
        }

        /// <summary>
        /// Updates the position and rotation changes while on a moving platform.
        /// </summary>
        private void UpdatePlatformMovement()
        {
            if (m_Platform == null) {
                if (m_Grounded) {
                    return;
                }
                // The character may have previously been on a platform and should inherit the platform movement.
                var damping = m_MovingPlatformForceDamping * (m_TimeScale * TimeUtility.FramerateDeltaTime);
                m_PlatformMovement /= (1 + damping);
                m_MoveDirection += m_PlatformMovement;

                m_PlatformTorque = Quaternion.RotateTowards(m_PlatformTorque, Quaternion.identity, damping);
                m_Torque *= m_PlatformTorque;
                return;
            }

            // The character may not need to be attached to the horizontal platform anymore.
            if (!m_GroundedMovingPlatform && !m_PlatformOverride) {
                var moveDirection = m_Transform.position - m_Platform.TransformPoint(m_PlatformRelativePosition);
                // The move direction has to be in the same direction as the character's current position.
                var hitCount = NonAllocCast(moveDirection, Vector3.zero);
                var platformCollision = false;
                for (int i = 0; i < hitCount; ++i) {
                    var closestRaycastHit = QuickSelect.SmallestK(m_CombinedRaycastHits, hitCount, i, m_RaycastHitComparer);
                    if (closestRaycastHit.transform == m_Platform) {
                        platformCollision = true;
                        break;
                    }
                }
                if (!platformCollision) {
                    UpdateMovingPlatformTransform(null, false);
                    return;
                }
            }

            // Update the position changes.
            m_PlatformMovement = m_Platform.TransformPoint(m_PlatformRelativePosition) - m_Transform.position;
            m_MoveDirection += m_PlatformMovement;

            // If the character doesn't stick to the moving platform and the platform slows down more than the separation velocity then the
            // moving platform momentum should be transferred to the character.
            var platformVelocity = m_PlatformMovement / (m_TimeScale * Time.deltaTime);
            if (!m_StickToMovingPlatform) {
                if (platformVelocity.sqrMagnitude < m_PlatformVelocity.sqrMagnitude && (platformVelocity - m_PlatformVelocity).magnitude > m_MovingPlatformSeperationVelocity) {
                    AddForce(m_PlatformMovement, 1, false, true);
                }
            }
            m_PlatformVelocity = platformVelocity;

            // Update the rotation changes.
            m_PlatformTorque = MathUtility.InverseTransformQuaternion(m_Platform.rotation, m_PlatformRotationOffset) *
                                Quaternion.Inverse(MathUtility.InverseTransformQuaternion(m_Platform.rotation, m_Transform.rotation * Quaternion.Inverse(m_Platform.rotation)));
            if (!m_AlignToGravity) {
                // Only the local y rotation should affect the character's rotation.
                var localPlatformTorque = MathUtility.InverseTransformQuaternion(m_Transform.rotation, m_PlatformTorque).eulerAngles;
                localPlatformTorque.x = localPlatformTorque.z = 0;
                m_PlatformTorque = MathUtility.TransformQuaternion(m_Transform.rotation, Quaternion.Euler(localPlatformTorque));
            }
            m_Torque *= m_PlatformTorque;
        }

        /// <summary>
        /// Update the rotation forces.
        /// </summary>
        protected virtual void UpdateRotation()
        {
            // Rotate according to the root motion rotation or target rotation.
            Quaternion rotationDelta, targetRotation;
            var rotation = m_Transform.rotation * m_Torque;
            if (UsingRootMotionRotation) {
                targetRotation = rotation * m_LocalRootMotionRotation;
                if (m_AlignToGravity) {
                    targetRotation *= Quaternion.Euler(m_DeltaRotation);
                }
            } else {
                if (m_AlignToGravity) {
                    // When aligning to gravity the character should rotate immediately to the up direction.
                    if (m_SmoothGravityYawDelta) {
                        m_DeltaRotation.y = Mathf.LerpAngle(0, MathUtility.ClampInnerAngle(m_DeltaRotation.y), m_MotorRotationSpeed * m_TimeScale * TimeUtility.DeltaTimeScaled);
                    }
                    targetRotation = rotation * Quaternion.Euler(m_DeltaRotation);
                } else {
                    targetRotation = Quaternion.Slerp(rotation, rotation * Quaternion.Euler(m_DeltaRotation), m_MotorRotationSpeed * m_TimeScale * TimeUtility.DeltaTimeScaled);
                }
            }
            rotationDelta = Quaternion.Inverse(rotation) * targetRotation;
            m_LocalRootMotionRotation = Quaternion.identity;
            rotationDelta = CheckRotation(rotationDelta, false);

            // Apply the delta rotation.
            m_Torque *= rotationDelta;
        }

        /// <summary>
        /// Checks the rotation to ensure the character's colliders won't collide with any other objects.
        /// </summary>
        /// <param name="rotationDelta">The delta to apply to the rotation.</param>
        /// <param name="forceCheck">Should the rotation be force checked? This is used when the character is aligning to the ground.</param>
        /// <returns>A valid rotation delta.</returns>
        public Quaternion CheckRotation(Quaternion rotationDelta, bool forceCheck)
        {
            // A rotation change can cause horizontal collisions. If horizontal collisions are not checked then the rotation does not need to be checked.
            if (!UsingHorizontalCollisionDetection) {
                return rotationDelta;
            }

            // The rotation only needs to be checked if a collider could cause a collision when rotating. For example, a vertical CapsuleCollider centered 
            // in the origin doesn't need to be checked because it can't collide with anything else when rotating.
            if (m_CheckRotationCollision && rotationDelta != Quaternion.identity) {
                if (m_ColliderCount > 1) {
                    // Clear the index map to start it off fresh.
                    m_ColliderIndexMap.Clear();
                }

                // There is no "rotation sphere/capsule cast" so the collisions must be detected manually. Loop through all of the colliders checking for a collision using
                // the Physics Overlap method. If there is a collision then the penetration must be determined to detect slopes. If the collision is not on a slope then
                // the rotation would overlap another collider so a smaller rotation must be used. Do this for the maximum number of collision checks, with the last one
                // being no rotation at all.
                for (int i = 0; i < m_ColliderCount; ++i) {
                    // The collider doesn't need to be checked if the rotation doesn't cause a change on the relative x or z axis. It will always be checked if the character
                    // is being realigned to the ground because the colliders will always change on the relative x or z axis.
                    if (!forceCheck && !CanCauseRotationCollision(m_Colliders[i])) {
                        continue;
                    }

                    // Prevent the character from intersecting with another object while rotating.
                    var targetRotationDelta = rotationDelta;
                    var hitCount = 0;
                    for (int k = 0; k < m_RotationCollisionCheckCount; ++k) {
                        // Slerp towards Quaternion.identity which will not add any rotation at all.
                        rotationDelta = Quaternion.Slerp(targetRotationDelta, Quaternion.identity, k / (float)(m_RotationCollisionCheckCount - 1));
                        // Calculate what the matrix for the child collider will be based on the rotation delta. This is done instead of setting the rotation
                        // direction on the Transform to reduce the calls to the Unity API.
                        var matrix = MathUtility.ApplyRotationToChildMatrices(m_Colliders[i].transform, m_Transform, rotationDelta);
                        // Store the position and rotation from the matrix for future use.
                        var targetPosition = MathUtility.PositionFromMatrix(matrix);
                        var targetRotation = MathUtility.QuaternionFromMatrix(matrix);

                        if (m_Colliders[i] is CapsuleCollider) {
                            Vector3 startEndCap, endEndCap;
                            var capsuleCollider = m_Colliders[i] as CapsuleCollider;
                            capsuleCollider.radius += c_ColliderSpacing;
                            MathUtility.CapsuleColliderEndCaps(capsuleCollider, targetPosition, targetRotation, out startEndCap, out endEndCap);
                            hitCount = Physics.OverlapCapsuleNonAlloc(startEndCap, endEndCap, capsuleCollider.radius * MathUtility.ColliderRadiusMultiplier(capsuleCollider),
                                                    m_OverlapColliderHit, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
                        } else { // SphereCollider.
                            var sphereCollider = m_Colliders[i] as SphereCollider;
                            sphereCollider.radius += c_ColliderSpacing;
                            hitCount = Physics.OverlapSphereNonAlloc(targetPosition, sphereCollider.radius * MathUtility.ColliderRadiusMultiplier(sphereCollider),
                                                    m_OverlapColliderHit, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
                        }

                        var overlap = false;
                        if (hitCount > 0) {
                            Vector3 direction;
                            float distance;
                            // If there is a collision ensure that collision is with a non-sloped object.
                            for (int j = 0; j < hitCount; ++j) {
                                if (Physics.ComputePenetration(m_Colliders[i], targetPosition, targetRotation,
                                    m_OverlapColliderHit[j], m_OverlapColliderHit[j].transform.position, m_OverlapColliderHit[j].transform.rotation, out direction, out distance)) {

                                    // If the hit object is less then the slope limit then the character should rotate with the slope rather then stopping immediately.
                                    var slope = Vector3.Angle((m_Transform.rotation * rotationDelta) * Vector3.up, direction);
                                    if (slope <= m_SlopeLimit + c_SlopeLimitSpacing) {
                                        continue;
                                    }

                                    overlap = true;
                                    break;
                                }
                            }
                        }

                        // The size of the collider has been increased ever so slightly so the collider bounds won't overlap other colliders. If this was not done
                        // the character would be able to rotate in a position that would allow them to move through other colliders.
                        if (m_Colliders[i] is CapsuleCollider) {
                            (m_Colliders[i] as CapsuleCollider).radius -= c_ColliderSpacing;
                        } else { // SphereCollider.
                            (m_Colliders[i] as SphereCollider).radius -= c_ColliderSpacing;
                        }

                        // If there is no overlap then the rotation is valid and can be used.
                        if (!overlap) {
                            break;
                        }
                    }
                }
            }
            return rotationDelta;
        }

        /// <summary>
        /// Applies the final rotation to the transform.
        /// </summary>
        protected virtual void ApplyRotation()
        {
            // Apply the rotation.
            m_Transform.rotation = MathUtility.Round(m_Transform.rotation * m_Torque, 1000000);
            m_Up = m_Transform.up;
            m_MotorRotation = m_Transform.rotation;
            m_Torque = Quaternion.identity;

            if (m_Platform != null) {
                m_PlatformRotationOffset = m_Transform.rotation * Quaternion.Inverse(m_Platform.rotation);
            }
        }

        /// <summary>
        /// Move according to the forces.
        /// </summary>
        protected virtual void UpdatePosition()
        {
            // Update any base movement forces.
            UpdateMotorThrottle();

            var deltaTime = TimeScaleSquared * Time.timeScale * TimeUtility.FramerateDeltaTime;
            m_MoveDirection += m_ExternalForce * deltaTime + (m_MotorThrottle * (UsingRootMotionPosition ? 1 : deltaTime)) - m_GravityDirection * m_GravityAmount * deltaTime;

            // After the character has moved update the collisions. This will prevent the character from moving through solid objects.
            DeflectHorizontalCollisions();
            DeflectVerticalCollisions();
        }

        /// <summary>
        /// Updates the motor forces.
        /// </summary>
        protected virtual void UpdateMotorThrottle()
        {
            if (UsingRootMotionPosition || m_LocalRootMotionForce.sqrMagnitude > 0) {
                m_MotorThrottle = MathUtility.TransformDirection(m_LocalRootMotionForce, m_MotorRotation) * (m_Grounded ? 1 : m_RootMotionAirForceMultiplier) * m_SlopeFactor;
            } else {
                // Apply a multiplier if the character is moving backwards.
                var backwardsMultiplier = 1f;
                if (m_InputVector.y < 0) {
                    backwardsMultiplier *= Mathf.Lerp(1, m_MotorBackwardsMultiplier, Mathf.Abs(m_InputVector.y));
                }
                // As the character changes rotation the same local motor throttle force should be applied. This is most apparent when the character is being aligned to the ground
                // and the local y direction changes.
                var prevLocalMotorThrottle = MathUtility.InverseTransformDirection(m_MotorThrottle, m_PrevMotorRotation) * m_PreviousAccelerationInfluence;
                var rotation = Quaternion.Slerp(m_PrevMotorRotation, m_MotorRotation, m_PreviousAccelerationInfluence);
                var acceleration = (m_Grounded ? m_MotorAcceleration : m_MotorAirborneAcceleration) * m_SlopeFactor * backwardsMultiplier * 0.1f;
                // Convert input into motor forces. Normalize the input vector to prevent the diagonal from moving faster.
                var normalizedInputVector = m_InputVector.normalized * Mathf.Max(Mathf.Abs(m_InputVector.x), Mathf.Abs(m_InputVector.y));
                m_MotorThrottle = MathUtility.TransformDirection(new Vector3(prevLocalMotorThrottle.x + normalizedInputVector.x * acceleration.x,
                                            prevLocalMotorThrottle.y, prevLocalMotorThrottle.z + normalizedInputVector.y * acceleration.z), rotation);
                // Dampen motor forces.
                m_MotorThrottle /= (1 + ((m_Grounded ? m_MotorDamping : m_MotorAirborneDamping) * m_TimeScale * Time.timeScale));
            }
            m_PrevMotorRotation = m_MotorRotation;
            m_LocalRootMotionForce = Vector3.zero;
        }

        /// <summary>
        /// Deflects any collisions in the horizontal direction.
        /// </summary>
        private void DeflectHorizontalCollisions()
        {
            m_HorizontalRaycastHit = m_BlankRaycastHit;
            // No horizontal collision checks are necessary if no collisions are being detected.
            if (!UsingHorizontalCollisionDetection) {
                return;
            }

            // While on a platform the move direction will contain the movement caused by the platform. When performing collision detection this value should be ignored
            // because the character is going to move with the platform.
            var platformIndependentMoveDirection = (m_MoveDirection - m_PlatformMovement);
            var horizontalDirection = Vector3.ProjectOnPlane(platformIndependentMoveDirection, m_Up);
            // No casts are necessary if there is no movement.
            if (horizontalDirection.sqrMagnitude < c_ColliderSpacingCubed && Vector3.ProjectOnPlane(m_PlatformMovement, m_Up).sqrMagnitude < c_ColliderSpacingCubed) {
                var localDirection = m_Transform.InverseTransformDirection(platformIndependentMoveDirection);
                localDirection.x = localDirection.z = 0;
                m_MoveDirection = m_Transform.TransformDirection(localDirection) + m_PlatformMovement;
                return;
            }

            var hitCount = NonAllocCast(horizontalDirection, m_PlatformMovement);
            if (hitCount == 0) {
                return;
            }

            var localPlatformIndependentMoveDirection = m_Transform.InverseTransformDirection(platformIndependentMoveDirection);
            var moveDistance = 0f;
            var hitStrength = 0f;
            var hitMoveDirection = Vector3.zero;
            for (int i = 0; i < hitCount; ++i) {
                var closestRaycastHit = QuickSelect.SmallestK(m_CombinedRaycastHits, hitCount, i, m_RaycastHitComparer);

                // Determine which collider caused the intersection.
                var activeCollider = m_ColliderCount > 1 ? m_Colliders[m_ColliderIndexMap[closestRaycastHit]] : m_Colliders[0];

                // If the distance is 0 then the colliders are overlapping. Use ComputePenetaration to seperate the objects.
                if (closestRaycastHit.distance == 0) {
                    var offset = Vector3.zero;
                    ComputePenetration(activeCollider, closestRaycastHit.collider, horizontalDirection, false, out offset);
                    if (offset.sqrMagnitude >= c_ColliderSpacingCubed) {
                        moveDistance = Mathf.Max(0, horizontalDirection.magnitude - offset.magnitude - c_ColliderSpacing);
                        platformIndependentMoveDirection = Vector3.ProjectOnPlane(horizontalDirection.normalized * moveDistance, m_Up) +
                                                m_Up * localPlatformIndependentMoveDirection.y;
                    } else {
                        platformIndependentMoveDirection = Vector3.zero;
                    }
                    break;
                }

                // Push any hit Rigidbodies that can be pushed.
                var hitGameObject = closestRaycastHit.transform.gameObject;
                var hitRigidbody = hitGameObject.GetCachedParentComponent<Rigidbody>();
                var canStep = true;
                if (hitRigidbody != null) {
                    var radius = (activeCollider is CapsuleCollider ?
                                    ((activeCollider as CapsuleCollider).radius * MathUtility.ColliderRadiusMultiplier(activeCollider as CapsuleCollider)) :
                                    ((activeCollider as SphereCollider).radius * MathUtility.ColliderRadiusMultiplier(activeCollider)));
                    canStep = !PushRigidbody(hitRigidbody, horizontalDirection, closestRaycastHit.point, radius);
                }

                // Determine if the character should step over the object. If the object is a rigidbody that was pushed then the character shouldn't attempt to step over
                // the object since the object will be moving.
                if (m_Grounded && canStep) {
                    // Only check for slope/steps if the hit point is lower than the max step height.
                    var groundPoint = m_Transform.InverseTransformPoint(closestRaycastHit.point);
                    if (groundPoint.y <= m_MaxStepHeight + c_ColliderSpacing) {
                        // A CapsuleCast/SphereCast normal isn't always the true normal: http://answers.unity3d.com/questions/50825/raycasthitnormal-what-does-it-really-return.html.
                        // Cast a regular raycast in order to determine the true normal.
                        m_SlopeRay.direction = horizontalDirection.normalized;
                        m_SlopeRay.origin = closestRaycastHit.point - m_SlopeRay.direction * (c_ColliderSpacing + 0.1f);
                        if (!Physics.Raycast(m_SlopeRay, out m_RaycastHit, (c_ColliderSpacing + 0.11f), 1 << hitGameObject.layer, QueryTriggerInteraction.Ignore)) {
                            m_RaycastHit = closestRaycastHit;
                        }
                        var slope = Vector3.Angle(m_Up, m_RaycastHit.normal);
                        if (slope <= m_SlopeLimit + c_SlopeLimitSpacing) {
                            continue;
                        }

                        // Cast a ray directly in front of the character. If it doesn't hit an object then the object is shorter than the step height and should be stepped on.
                        // Continue out of the loop to prevent the character from stopping in front of the object.
                        if (SingleCast(activeCollider, horizontalDirection, m_PlatformMovement + m_Up * (m_MaxStepHeight - c_ColliderSpacing))) {
                            if ((m_RaycastHit.distance - c_ColliderSpacing) < horizontalDirection.magnitude) {
                                // An object was hit. The character can step over the object if the slope is less than the limit.
                                m_SlopeRay.direction = horizontalDirection.normalized;
                                m_SlopeRay.origin = closestRaycastHit.point - m_SlopeRay.direction * (c_ColliderSpacing + 0.1f);
                                var normal = m_RaycastHit.normal;
                                if (Physics.Raycast(m_SlopeRay, out m_RaycastHit, (c_ColliderSpacing + 0.11f), 1 << hitGameObject.layer, QueryTriggerInteraction.Ignore)) {
                                    normal = m_RaycastHit.normal;
                                }
                                slope = Vector3.Angle(m_Up, normal);
                                if (slope <= m_SlopeLimit + c_SlopeLimitSpacing) {
                                    continue;
                                }
                            }
                        } else {
                            groundPoint.y = 0;
                            groundPoint = m_Transform.TransformPoint(groundPoint);
                            var direction = groundPoint - m_Transform.position;
                            if (OverlapCount(activeCollider, (direction.normalized * (direction.magnitude + m_Radius * 0.5f)) + m_PlatformMovement + m_Up * (m_MaxStepHeight - c_ColliderSpacing)) == 0) {
                                // Step over the object if there are no objects in the way.
                                continue;
                            }
                        }
                    }
                }

#if UNITY_EDITOR
                Debug.DrawRay(closestRaycastHit.point, closestRaycastHit.normal, Color.red);
#endif

                // Determine the direction that the character should move based off of the angle of the hit object.
                var hitNormal = Vector3.ProjectOnPlane(closestRaycastHit.normal, m_Up).normalized;
                var targetDirection = Vector3.Cross(hitNormal, m_Up).normalized;
                var closestPoint = MathUtility.ClosestPointOnCollider(m_Transform, activeCollider, closestRaycastHit.point, platformIndependentMoveDirection, true, false);
                if ((Vector3.Dot(Vector3.Cross(Vector3.ProjectOnPlane(m_Transform.position - closestPoint, m_Up).normalized, horizontalDirection).normalized, m_Up)) > 0) {
                    targetDirection = -targetDirection;
                }

                // The hit distance may be more than the horizontal direction magnitude if the hit object is within the radius distance. 
                // This could happen if there is a small object directly in front of the character but under the collider's concave curve.
                // Subtract the collider spacing constant to prevent the character's collider from overlapping the hit collider.
                moveDistance = Mathf.Min(closestRaycastHit.distance - c_ColliderSpacing, horizontalDirection.magnitude);
                if (moveDistance < 0.001f || Vector3.Angle(m_Up, m_GroundRaycastHit.normal) > m_SlopeLimit + c_SlopeLimitSpacing) {
                    moveDistance = 0;
                }

                // Glide across the wall if it's at an angle relative to the character. Evaluate how much to glide based off a curve. Don't glide faster
                // then the horizontal direction magnitude unless the curve specifies to.
                var dynamicFrictionValue = Mathf.Clamp01(1 - MathUtility.FrictionValue(activeCollider.material, closestRaycastHit.collider.material, true));
                hitStrength = 1 - Vector3.Dot(horizontalDirection.normalized, -hitNormal);
                hitMoveDirection = targetDirection * (horizontalDirection.magnitude - moveDistance) * m_WallGlideCurve.Evaluate(hitStrength) * dynamicFrictionValue;
                if (hitMoveDirection.magnitude <= c_ColliderSpacing) {
                    hitMoveDirection = Vector3.zero;
                    hitStrength = 0;
                    m_HorizontalRaycastHit = closestRaycastHit;
                }
                platformIndependentMoveDirection = (horizontalDirection.normalized * moveDistance) + hitMoveDirection + m_Up * localPlatformIndependentMoveDirection.y;

                // The character may bounce away from the object. This bounce is applied to the external force so it'll be checked next frame.
                var bouncinessValue = MathUtility.BouncinessValue(activeCollider.material, closestRaycastHit.collider.material);
                if (bouncinessValue > 0.0f) {
                    var magnitude = Mathf.Max(m_ExternalForce.magnitude, bouncinessValue * m_WallBounceModifier);
                    AddForce(Vector3.Reflect(horizontalDirection, hitNormal).normalized * magnitude, 1, false, true);
                }
                break;
            }
            ResetCombinedRaycastHits();

            // Do another cast in the desired direction to ensure the position is valid.
            if (hitStrength > 0.0001f) {
                hitCount = NonAllocCast(hitMoveDirection, m_PlatformMovement);
                for (int i = 0; i < hitCount; ++i) {
                    var closestRaycastHit = QuickSelect.SmallestK(m_CombinedRaycastHits, hitCount, i, m_RaycastHitComparer);
                    // Determine if the character should step over the object. The character must be on the ground with a slope less than the limit in order to step over.
                    var activeCollider = m_ColliderCount > 1 ? m_Colliders[m_ColliderIndexMap[closestRaycastHit]] : m_Colliders[0];
                    if (m_Grounded) {
                        var groundPoint = m_Transform.InverseTransformPoint(closestRaycastHit.point);
                        if (groundPoint.y > c_ColliderSpacing && groundPoint.y <= m_MaxStepHeight + c_ColliderSpacing) {
                            var hitGameObject = closestRaycastHit.transform.gameObject;
                            // A CapsuleCast/SphereCast normal isn't always the true normal: http://answers.unity3d.com/questions/50825/raycasthitnormal-what-does-it-really-return.html.
                            // Cast a regular raycast in order to determine the true normal.
                            m_SlopeRay.direction = hitMoveDirection.normalized;
                            m_SlopeRay.origin = closestRaycastHit.point - m_SlopeRay.direction * (c_ColliderSpacing + 0.1f);
                            if (!Physics.Raycast(m_SlopeRay, out m_RaycastHit, (c_ColliderSpacing + 0.11f), 1 << hitGameObject.layer, QueryTriggerInteraction.Ignore)) {
                                m_RaycastHit = closestRaycastHit;
                            }
                            var slope = Vector3.Angle(m_Up, m_RaycastHit.normal);
                            if (slope <= m_SlopeLimit + c_SlopeLimitSpacing) {
                                continue;
                            }

                            // Cast a ray directly in front of the character. If it doesn't hit an object then the object is shorter than the step height and should be stepped on.
                            // Continue out of the loop to prevent the character from stopping in front of the object.
                            if (SingleCast(activeCollider, hitMoveDirection, m_PlatformMovement + m_Up * (m_MaxStepHeight - c_ColliderSpacing))) {
                                if ((m_RaycastHit.distance - c_ColliderSpacing) < hitMoveDirection.magnitude) {
                                    // An object was hit. The character can step over the object if the slope is less than the limit.
                                    m_SlopeRay.direction = hitMoveDirection.normalized;
                                    m_SlopeRay.origin = closestRaycastHit.point - m_SlopeRay.direction * (c_ColliderSpacing + 0.1f);
                                    var normal = m_RaycastHit.normal;
                                    if (Physics.Raycast(m_SlopeRay, out m_RaycastHit, (c_ColliderSpacing + 0.11f), 1 << hitGameObject.layer, QueryTriggerInteraction.Ignore)) {
                                        normal = m_RaycastHit.normal;
                                    }
                                    slope = Vector3.Angle(m_Up, normal);
                                    if (slope <= m_SlopeLimit + c_SlopeLimitSpacing) {
                                        continue;
                                    }
                                }
                            } else {
                                groundPoint.y = 0;
                                groundPoint = m_Transform.TransformPoint(groundPoint);
                                var direction = groundPoint - m_Transform.position;
                                if (OverlapCount(activeCollider, (direction.normalized * (direction.magnitude + m_Radius * 0.5f)) + m_PlatformMovement + m_Up * (m_MaxStepHeight - c_ColliderSpacing)) == 0) {
                                    // Step over the object if there are no objects in the way.
                                    continue;
                                }
                            }
                        }
                    }
#if UNITY_EDITOR
                    Debug.DrawRay(closestRaycastHit.point, closestRaycastHit.normal, Color.yellow);
#endif

                    // Another collider is in the way of the hit move direction. Do not apply the full magnitude of the move direction.
                    // Subtract the collider spacing constant to prevent the character's collider from overlapping the hit collider.
                    // In addition to subtracting the collider spacing, also subtract the distance that the move distance is contributing to the hit move direction.
                    // This will prevent the character from moving too far because the hit point direction is similar to the horizontal direction.
                    // This last condition is especially apprant with -extremely- fast moving objects.
                    var moveDistanceContribution = moveDistance * Mathf.Cos(Vector3.Angle(horizontalDirection.normalized, hitMoveDirection.normalized) * Mathf.Deg2Rad);
                    var hitMoveDistance = Mathf.Min(closestRaycastHit.distance - moveDistanceContribution - hitMoveDirection.magnitude - c_ColliderSpacing, hitMoveDirection.magnitude);
                    if (hitMoveDistance < 0.001f || Vector3.Angle(m_Up, m_GroundRaycastHit.normal) > m_SlopeLimit + c_SlopeLimitSpacing) {
                        hitMoveDistance = 0;
                        m_HorizontalRaycastHit = closestRaycastHit;
                    }

                    platformIndependentMoveDirection = (horizontalDirection.normalized * moveDistance) + hitMoveDirection.normalized * hitMoveDistance + m_Up * localPlatformIndependentMoveDirection.y;
                    break;
                }
                ResetCombinedRaycastHits();
            }

            m_MoveDirection = platformIndependentMoveDirection + m_PlatformMovement;
        }

        /// <summary>
        /// Deflects any collisions in the vertical direction.
        /// </summary>
        private void DeflectVerticalCollisions()
        {
            // Ensure the character doesn't collide with any objects above them.
            var localMoveDirection = m_Transform.InverseTransformDirection(m_MoveDirection);
            if (UsingVerticalCollisionDetection && localMoveDirection.y > 0) {
                var horizontalDirection = Vector3.ProjectOnPlane(m_MoveDirection - m_PlatformMovement, m_Up);
                var hitCount = NonAllocCast(m_Up * (localMoveDirection.y + c_ColliderSpacing), horizontalDirection);
                if (hitCount > 0) {
                    var closestRaycastHit = QuickSelect.SmallestK(m_CombinedRaycastHits, hitCount, 0, m_RaycastHitComparer);
                    if (closestRaycastHit.distance == 0) {
                        // Use ComputePenetration to determine a direction that doesn't overlap with other objects.
                        var activeCollider = m_ColliderCount > 1 ? m_Colliders[m_ColliderIndexMap[closestRaycastHit]] : m_Colliders[0];
                        var offset = Vector3.zero;
                        var overlap = ComputePenetration(activeCollider, closestRaycastHit.collider, horizontalDirection, true, out offset);
                        if (overlap) {
                            overlap = ComputePenetration(activeCollider, closestRaycastHit.collider, horizontalDirection, false, out offset);
                        }
                        if (!overlap) {
                            // The offset is valid.
                            localMoveDirection.y = m_Transform.InverseTransformDirection(offset).y;
                        } else {
                            // The offset still causes an overlap. Set the vertical move direction to 0.
                            localMoveDirection.y = 0;
                        }
                    } else {
                        // The character would hit an object above it. Use the hit object to determine the max distance that the character can move.
                        localMoveDirection.y = Mathf.Max(0, closestRaycastHit.distance - c_ColliderSpacing);
                    }

                    // The character shouldn't move up anymore.
                    if (localMoveDirection.y == 0) {
                        var localExternalForce = LocalExternalForce;
                        localExternalForce.y = 0;
                        m_ExternalForce = m_Transform.TransformDirection(localExternalForce);
                    }

                    // Convert back to the global direction for CheckGround.
                    m_MoveDirection = m_Transform.TransformDirection(localMoveDirection);
                }
            }

            // Is the character on the ground?
            var grounded = CheckGround();
            localMoveDirection = m_Transform.InverseTransformDirection(m_MoveDirection);

            var accumulateGravity = UsingGravity;
            var verticalOffset = 0f;
            if (UsingVerticalCollisionDetection && m_GroundRaycastHit.distance != 0) {
                verticalOffset = m_Transform.InverseTransformDirection(m_GroundRaycastHit.point - m_GroundRaycastOrigin).y + c_ColliderSpacing;
                if (Mathf.Abs(verticalOffset) < 0.0001f) {
                    verticalOffset = 0;
                }

                // Staying in local space, determine if the vertical offset or the gravity force should be used. The lesser value should be used to prevent the character
                // from moving through the floor. Add the collider spacing constant to prevent the character's collider from overlapping the ground.
                // Use the verticalOffset if the local move direction is negative (ignoring the platform) and less than the vertical offset (plus an offset if sticking to the ground).
                var localPlatformVerticalDirection = m_Platform != null ? m_Transform.InverseTransformDirection(m_PlatformMovement).y : 0;
                if ((m_Grounded || grounded) && (localMoveDirection.y - localPlatformVerticalDirection) < m_SkinWidth && verticalOffset > -c_ColliderSpacing -
                                                                                    ((m_Grounded && m_StickToGround) ? m_Stickiness : 0)) {
                    localMoveDirection.y += verticalOffset;
                    // Don't allow the character to go through the ground.
                    if (localMoveDirection.y < -m_GroundRaycastHit.distance + localPlatformVerticalDirection) {
                        localMoveDirection.y = -m_GroundRaycastHit.distance + localPlatformVerticalDirection + c_ColliderSpacing;
                    }
                    accumulateGravity = false;
                    // If the character wasn't previously grounded they are now. This allows the character to stick to the ground without the grounded state updating.
                    grounded = true;
                }
            }

            // Convert the local move direction back to world position.
            m_MoveDirection = m_Transform.TransformDirection(localMoveDirection);

            if (accumulateGravity) { // The character is in the air.
                // Accumulate gravity.
                m_GravityAmount += (m_GravityMagnitude * -0.001f) / Time.timeScale;
            } else if (grounded) { // The character is on the ground.
                // If the character is standing on a rigidbody then a downward force should be applied.
                if (m_GroundRaycastHit.rigidbody != null) {
                    m_GroundRaycastHit.rigidbody.AddForceAtPosition(-m_Up * ((m_Mass / m_GroundRaycastHit.rigidbody.mass) + m_GravityAmount) / Time.deltaTime,
                                                            m_Transform.position + m_MoveDirection, ForceMode.Force);
                }

                // No gravity is applied when the character is grounded.
                m_GravityAmount = 0;
            }

            // Update the grounded status. The vertical collisions may change the grounded state so this value is not updated immediately (such as if the character is sticking to the ground).
            UpdateGroundState(grounded, true);
        }

        /// <summary>
        /// Determines if the character is grounded.
        /// </summary>
        /// <returns>True if the character is grounded.</returns>
        private bool CheckGround()
        {
            // Reset the ground raycast hit for the next run.
            m_GroundRaycastHit = m_BlankRaycastHit;

            var verticalMoveDirection = m_Transform.InverseTransformDirection(m_MoveDirection).y;
            var horizontalDirection = Vector3.ProjectOnPlane(m_MoveDirection, -m_GravityDirection);
            var localPlatformVerticalDirection = m_Platform != null ? m_Transform.InverseTransformDirection(m_PlatformMovement).y : 0;
            var horizontalCastOffset = horizontalDirection + (-m_GravityDirection * localPlatformVerticalDirection);
            var hitCount = NonAllocCast(m_GravityDirection * (Mathf.Abs(verticalMoveDirection) + m_MaxStepHeight + (m_Grounded && m_StickToGround ? m_Stickiness : 0) + c_ColliderSpacing),
                                            horizontalCastOffset);
            var grounded = false;
            // The character hit the ground if any hit points are below the collider.
            for (int i = 0; i < hitCount; ++i) {
                var closestRaycastHit = QuickSelect.SmallestK(m_CombinedRaycastHits, hitCount, i, m_RaycastHitComparer);
                var activeCollider = m_ColliderCount > 1 ? m_Colliders[m_ColliderIndexMap[closestRaycastHit]] : m_Colliders[0];

                // When DeflectHorizontalCollisions runs it will not do any vertical placement. If the character is on a slope or a step there will be a collision
                // because the move direction hasn't changed vertically. The RaycastHit distance will then be 0 indicating there are objects overlapping it.
                if (closestRaycastHit.distance == 0) {
                    // If vertical collisions are not being detected then don't try to readjust the collider position. The hit collider should not count as being grounded
                    // because it overlaps the character collider.
                    if (!UsingVerticalCollisionDetection) {
                        continue;
                    }

                    // Do not run ComputeGroundPenetration if the character should step over the object. DeflectVerticalForces will account for the vertical step difference.
                    var horizontalStep = false;
                    if (UsingHorizontalCollisionDetection && horizontalDirection.sqrMagnitude >= 0.000001f) {
                        var horizontalPlatformMovement = Vector3.ProjectOnPlane(m_PlatformMovement, m_Up);
                        if (SingleCast(activeCollider, horizontalDirection, horizontalPlatformMovement)) {
                            var groundPoint = m_Transform.InverseTransformPoint(m_RaycastHit.point);
                            if (groundPoint.y <= m_MaxStepHeight + c_ColliderSpacing) {
                                // If there are no objects in front of the character then any objects that is in front is shorter than the step height and should be stepped on.
                                if (OverlapCount(activeCollider, horizontalDirection + horizontalPlatformMovement + m_Up * (m_MaxStepHeight - c_ColliderSpacing)) == 0) {
                                    horizontalStep = true;
                                }
                            }
                        }
                    }

                    // ComputeGroundPenetration has to potential of running twice. The first time it will try to keep the velocity at a constant speed while also preventing
                    // any overlap. If it can't do that then ComputeGroundPenetration will be run a second time to which does not try to keep a constant speed.
                    var offset = Vector3.zero;
                    var overlap = !horizontalStep && UsingHorizontalCollisionDetection && ComputePenetration(activeCollider, closestRaycastHit.collider, horizontalCastOffset, true, out offset);
                    if (overlap) {
                        overlap = ComputePenetration(activeCollider, closestRaycastHit.collider, horizontalCastOffset, false, out offset);
                    }

                    // If there is no more overlap then perform a final cast to determine the location of the ground.
                    if (!overlap) {
                        var localOffset = m_Transform.InverseTransformDirection(offset);
                        // Prevent ComputeGroundPenetration from trying to reposition the character because of a collision.
                        if (!UsingHorizontalCollisionDetection && Mathf.Abs(localOffset.y) <= 0.001f) {
                            if (!SingleCast(activeCollider, m_GravityDirection * (m_MaxStepHeight + c_ColliderSpacing), Vector3.ProjectOnPlane(m_MoveDirection, -m_GravityDirection) + m_Transform.up * (m_MaxStepHeight + c_ColliderSpacing))) {
                                continue;
                            }
                        }
                        // Remove any external force on the y direction once the character has hit the collider above them.
                        if (localOffset.y < 0) {
                            var localExternalForce = m_Transform.InverseTransformDirection(m_ExternalForce);
                            localExternalForce.y = 0;
                            m_ExternalForce = m_Transform.TransformDirection(localExternalForce);
                        }

                        // Reset any upwards offset to 0. It will later be added back in after the gravity forces have been detected.
                        if (localOffset.y > 0) { localOffset.y = 0; }
                        // The move direction should update based on the ComputePenetration value.
                        m_MoveDirection += m_Transform.TransformDirection(localOffset);

                        // One more cast should be performed to determine the ground object now that the character is in a non-overlapping position.
                        if (SingleCast(activeCollider, m_GravityDirection * (m_MaxStepHeight + c_ColliderSpacing), Vector3.ProjectOnPlane(m_MoveDirection, -m_GravityDirection) + m_Transform.up * (m_MaxStepHeight + c_ColliderSpacing))) {
                            closestRaycastHit = m_RaycastHit;
                        } else {
                            // The character is not grounded after resolving the collision. Continue the grounded check next update.
                            break;
                        }
                    } else {
                        m_MoveDirection = Vector3.zero;
                        break;
                    }
                }

                // The raycast position is determined by the location that the raycast hit. This is required because the capsule collider is long and if the hit point is within the 
                // center length of the capsule collider then the character should not account for that hit distance.
                m_GroundRaycastHit = closestRaycastHit;
                m_GroundRaycastOrigin = MathUtility.ClosestPointOnCollider(m_Transform, activeCollider, m_GroundRaycastHit.point, m_MoveDirection, false, true);

                // The character is grounded when the ground contact point is within the skin width. The ground raycast hit and origin still need to be set to detect vertical collisions.
                var localDirection = m_Transform.InverseTransformDirection(m_GroundRaycastHit.point - m_GroundRaycastOrigin);
                var deltaTime = m_TimeScale * Time.timeScale * TimeUtility.FramerateDeltaTime;
                grounded = (localDirection.y - (LocalExternalForce.y * deltaTime)) >= -m_SkinWidth;
                break;
            }
            ResetCombinedRaycastHits();

            // Update the moving platform if the character lands on a new platform or moves off of the platform.
            if (grounded != m_Grounded || m_GroundRaycastHit.transform != m_GroundHitTransform) {
                var target = grounded ? m_GroundRaycastHit.transform : null;
                if (target != m_GroundHitTransform) {
                    m_GroundHitTransform = target;
                    if (!m_PlatformOverride) {
                        UpdateMovingPlatformTransform(m_GroundHitTransform, true);
                    }
                }
            }

            UpdateSlopeFactor();

            return grounded;
        }

        /// <summary>
        /// Use ComputePenetration to prevent the collider from overlapping with another collider.
        /// </summary>
        /// <param name="activeCollider">The collider which may is being checked to ensure it doesn't cause a collision.</param>
        /// <param name="hitCollider">The original collider that caused a collision with the active collider.</param>
        /// <param name="horizontalDirection">The horizontal direction that the character is moving.</param>
        /// <param name="constantVelocity">Should a constant velocity be used?</param>
        /// <param name="offset">The offset returned by Physics.ComputePenetration.</param>
        /// <returns>True if an overlap still occurs even after using the offset.</returns>
        private bool ComputePenetration(Collider activeCollider, Collider hitCollider, Vector3 horizontalDirection, bool constantVelocity, out Vector3 offset)
        {
            var iterations = m_MaxOverlapIterations;
            float distance;
            Vector3 direction;
            offset = Vector3.zero;
            var overlap = true;
            m_OverlapColliderHit[0] = hitCollider;

            while (iterations > 0) {
                if (Physics.ComputePenetration(activeCollider, activeCollider.transform.position + horizontalDirection + offset,
                                                    activeCollider.transform.rotation, m_OverlapColliderHit[0], m_OverlapColliderHit[0].transform.position, m_OverlapColliderHit[0].transform.rotation, out direction, out distance)) {
                    offset += direction.normalized * (distance + c_ColliderSpacing);
                } else {
                    // End early - no need to keep trying.
                    offset = Vector3.zero;
                    overlap = false;
                    break;
                }
                if (constantVelocity) {
                    // Keep the same velocity magnitude as before the overlap. This will prevent slopes from causing the character to change velocities.
                    offset = (horizontalDirection + offset).normalized * horizontalDirection.magnitude - horizontalDirection;
                }

                // Determine if the offset resolved the overlap.
                if (OverlapCount(activeCollider, horizontalDirection + offset) == 0) {
                    overlap = false;
                    break;
                }

                iterations--;
            }
            return overlap;
        }

        /// <summary>
        /// When the character changes grounded state the moving platform should also be updated. This
        /// allows the character to always reference the correct moving platform (if one exists at all).
        /// </summary>
        /// <param name="hitTransform">The name of the possible moving platform transform.</param>
        /// <param name="groundUpdate">Is the moving platform update being called from a grounded check?</param>
        /// <returns>True if the platform changed.</returns>
        private bool UpdateMovingPlatformTransform(Transform hitTransform, bool groundUpdate)
        {
            // If the platform is not null then the platform should only update if the method is being called from the grounded check.
            // This will prevent the moving platform from being updated more than once during a single move event.
            if (m_Platform != null && m_GroundedMovingPlatform != groundUpdate) {
                return false;
            }

            // Update the moving platform if on the ground and the ground transform is a moving platform.
            if (hitTransform != null) {
                // The character may not be on the ground if the character is teleported to a location that overlaps the moving platform.
                if (hitTransform.gameObject.layer == LayerManager.MovingPlatform) {
                    if (hitTransform != m_Platform) {
                        SetPlatform(hitTransform, false);
                    }
                    m_GroundedMovingPlatform = groundUpdate;
                    return true;
                } else if (m_Platform != null) {
                    TransferPlatformMovement();
                    SetPlatform(null, false);
                    return true;
                }
            } else if (m_Platform != null && hitTransform == null) { // The character is no longer on a moving platform.
                if (m_ExternalForce.sqrMagnitude > 0.01f) {
                    m_PlatformMovement = Vector3.zero;
                    m_PlatformTorque = Quaternion.identity;
                }
                SetPlatform(null, true);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the moving platform to the specified transform.
        /// </summary>
        /// <param name="platform">The platform transform that should be set. Can be null.</param>
        public void SetPlatform(Transform platform)
        {
            SetPlatform(platform, true);
        }

        /// <summary>
        /// Sets the moving platform to the specified transform.
        /// </summary>
        /// <param name="platform">The platform transform that should be set. Can be null.</param>
        /// <param name="platformOverride">Is the default moving platform logic being overridden?</param>
        /// <returns>True if the platform was changed.</returns>
        protected virtual bool SetPlatform(Transform platform, bool platformOverride)
        {
            if (m_Platform == platform) {
                return false;
            }
            m_Platform = platform;
            m_PlatformOverride = m_Platform != null && platformOverride;
            if (m_Platform != null) {
                var localDirection = (m_Grounded ? 
                    m_Transform.InverseTransformDirection(m_GroundRaycastHit.point - m_GroundRaycastOrigin + m_Up * c_ColliderSpacing) : 
                    Vector3.zero);
                m_PlatformRelativePosition = m_Platform.InverseTransformPoint(m_Transform.position) + localDirection;
                m_PlatformRotationOffset = m_Transform.rotation * Quaternion.Inverse(m_Platform.rotation);
            } else if (platformOverride) {
                TransferPlatformMovement();
            }
            return true;
        }

        /// <summary>
        /// Transfers the platform movement from the platform onto the character.
        /// </summary>
        private void TransferPlatformMovement()
        {
            m_MotorThrottle += m_PlatformMovement;
            m_Torque *= m_PlatformTorque;

            m_PlatformVelocity = m_PlatformMovement = Vector3.zero;
            m_PlatformTorque = Quaternion.identity;
        }

        /// <summary>
        /// Updates the slope factor. This gives the option of slowing the character down while moving up a slope or increasing the speed while moving down.
        /// </summary>
        protected virtual void UpdateSlopeFactor()
        {
            // The character isn't on a slope while in the air.
            if (!m_Grounded || !UsingVerticalCollisionDetection) {
                m_SlopeFactor = 1;
                return;
            }

            // Determine if the slope is uphill or downhill.
            m_SlopeFactor = 1 + (1 - (Vector3.Angle(m_GroundRaycastHit.normal, m_MotorThrottle) / 90));

            if (Mathf.Abs(1 - m_SlopeFactor) < 0.01f) { // Standing still, moving on flat ground, or moving perpendicular to a slope.
                m_SlopeFactor = 1;
            } else if (m_SlopeFactor > 1) { // Downhill.
                m_SlopeFactor = m_MotorSlopeForceDown / m_SlopeFactor;
            } else { // Uphill.
                m_SlopeFactor *= m_MotorSlopeForceUp;
            }
        }

        /// <summary>
        /// Updates the grounded state.
        /// </summary>
        /// <param name="grounded">Is the character grounded?</param>
        /// <param name="eventUpdate">Should the events be sent if the grounded status changes?</param>
        /// <returns>True if the grounded state changed.</returns>
        protected virtual bool UpdateGroundState(bool grounded, bool sendEvents)
        {
            // Update the grounded state. Allows for cleanup when the character hits the ground or moves into the air.
            if (m_Grounded != grounded) {
                m_Grounded = grounded;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Applies the final move direction to the transform.
        /// </summary>
        protected virtual void ApplyPosition()
        {
            // Apply the position.
            m_Transform.position = m_Transform.position + m_MoveDirection;

            m_Velocity = (m_Transform.position - m_PrevPosition) / (m_TimeScale * Time.deltaTime);
            m_PrevPosition = m_Transform.position;
            m_MoveDirection = Vector3.zero;
            // Update the platform variables.
            if (m_Platform != null) {
                m_PlatformRelativePosition = m_Platform.InverseTransformPoint(m_Transform.position);
            }
        }

        /// <summary>
        /// Updates any external forces.
        /// </summary>
        private void UpdateExternalForces()
        {
            // Apply a soft force (forces applied over several frames).
            if (m_SoftForceFrames[0] != Vector3.zero) {
                AddExternalForce(m_SoftForceFrames[0], false);
                for (int v = 0; v < m_MaxSoftForceFrames; v++) {
                    m_SoftForceFrames[v] = (v < m_MaxSoftForceFrames - 1) ? m_SoftForceFrames[v + 1] : Vector3.zero;
                    if (m_SoftForceFrames[v] == Vector3.zero) {
                        break;
                    }
                }
            }

            // Dampen external forces.
            var deltaTime = m_TimeScale * TimeUtility.FramerateDeltaTime;
            m_ExternalForce /= (1 + (m_Grounded ? m_ExternalForceDamping : m_ExternalForceAirDamping) * deltaTime);
        }

        /// <summary>
        /// Callback from the animator when root motion has updated.
        /// </summary>
        protected virtual void OnAnimatorMove()
        {
            // The delta position will be NaN after the first respawn frame. The TimeScale must also be positive.
            if (float.IsNaN(m_AnimatorDeltaPosition.x) || m_TimeScale == 0 || Time.timeScale == 0 || Time.deltaTime == 0) {
                return;
            }

            if (m_AnimatorDeltaPosition.sqrMagnitude > 0) {
                m_LocalRootMotionForce += m_Transform.InverseTransformDirection(m_AnimatorDeltaPosition) * m_RootMotionSpeedMultiplier;
                m_AnimatorDeltaPosition = Vector3.zero;
            }
            if (m_AnimatorDeltaRotation != Quaternion.identity) {
                float angle; Vector3 axis;
                m_AnimatorDeltaRotation.ToAngleAxis(out angle, out axis);
                angle *= m_RootMotionRotationMultiplier;
                m_LocalRootMotionRotation *= Quaternion.AngleAxis(angle, axis);
                m_AnimatorDeltaRotation = Quaternion.identity;
            }

            // Root motion has been retrieved from the animator. The rotation and position should now be applied based on the root motion data. This should be done within
            // OnAnimatorMove so the rotation and position will be applied before the animator does its IK pass.
            UpdatePositionAndRotation(true);
        }

        /// <summary>
        /// Casts a ray in the specified direction. If the character has multiple colliders added then a ray will be cast from each collider.
        /// A CapsuleCast or SphereCast is used depending on the type of collider that has been added. The result is stored in the m_CombinedRaycastHits array.
        /// </summary>
        /// <param name="direction">The direction to perform the cast.</param>
        /// <param name="offset">Any offset to apply to the cast.</param>
        /// <returns>The number of objects hit from the cast.</returns>
        private int NonAllocCast(Vector3 direction, Vector3 offset)
        {
            if (m_ColliderCount > 1) {
                // Clear the index map to start it off fresh.
                m_ColliderIndexMap.Clear();
            }

            var hitCount = 0;
            for (int i = 0; i < m_ColliderCount; ++i) {
                int localHitCount;
                // Determine if the collider would intersect any objects.
                if (m_Colliders[i] is CapsuleCollider) {
                    Vector3 startEndCap, endEndCap;
                    var capsuleCollider = m_Colliders[i] as CapsuleCollider;
                    MathUtility.CapsuleColliderEndCaps(capsuleCollider, capsuleCollider.transform.position + offset, capsuleCollider.transform.rotation, out startEndCap, out endEndCap);
                    var radius = capsuleCollider.radius * MathUtility.ColliderRadiusMultiplier(capsuleCollider) - c_ColliderSpacing;
                    localHitCount = Physics.CapsuleCastNonAlloc(startEndCap, endEndCap, radius, direction.normalized, m_RaycastHits, direction.magnitude + c_ColliderSpacing, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
                } else { // SphereCollider.
                    var sphereCollider = m_Colliders[i] as SphereCollider;
                    var radius = sphereCollider.radius * MathUtility.ColliderRadiusMultiplier(sphereCollider) - c_ColliderSpacing;
                    localHitCount = Physics.SphereCastNonAlloc(sphereCollider.transform.TransformPoint(sphereCollider.center) + offset, radius, direction.normalized,
                                                                    m_RaycastHits, direction.magnitude + c_ColliderSpacing, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
                }

                if (localHitCount > 0) {
                    // The mapping needs to be saved if there are multiple colliders.
                    if (m_ColliderCount > 1) {
                        int validHitCount = 0;
                        for (int j = 0; j < localHitCount; ++j) {
                            if (m_ColliderIndexMap.ContainsKey(m_RaycastHits[j])) {
                                continue;
                            }
                            // Ensure the array is large enough.
                            if (hitCount + j >= m_CombinedRaycastHits.Length) {
                                Debug.LogWarning("Warning: The maximum number of collisions has been reached. Consider increasing the CharacterLocomotion MaxCollisionCount value.");
                                continue;
                            }

                            m_ColliderIndexMap.Add(m_RaycastHits[j], i);
                            m_CombinedRaycastHits[hitCount + j] = m_RaycastHits[j];
                            validHitCount += 1;
                        }
                        hitCount += validHitCount;
                    } else {
                        m_CombinedRaycastHits = m_RaycastHits;
                        hitCount += localHitCount;
                    }
                }
            }

            return hitCount;
        }

        /// <summary>
        /// Casts a ray using in the specified direction.  A CapsuleCast or SphereCast is used depending on the type of collider that has been added.
        /// The result is stored in the m_RaycastHit object.
        /// </summary>
        /// <param name="collider">The collider which is performing the cast.</param>
        /// <param name="direction">The direction to perform the cast.</param>
        /// <param name="offset">Any offset to apply to the cast.</param>
        /// <returns>Did the cast hit an object?</returns>
        private bool SingleCast(Collider collider, Vector3 direction, Vector3 offset)
        {
            // Determine if the collider would intersect any objects.
            if (collider is CapsuleCollider) {
                Vector3 startEndCap, endEndCap;
                var capsuleCollider = collider as CapsuleCollider;
                MathUtility.CapsuleColliderEndCaps(capsuleCollider, capsuleCollider.transform.position + offset, capsuleCollider.transform.rotation, out startEndCap, out endEndCap);
                var radius = capsuleCollider.radius * MathUtility.ColliderRadiusMultiplier(capsuleCollider) - c_ColliderSpacing;
                return Physics.CapsuleCast(startEndCap, endEndCap, radius, direction.normalized, out m_RaycastHit, direction.magnitude + c_ColliderSpacing, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
            } else { // SphereCollider.
                var sphereCollider = collider as SphereCollider;
                var radius = sphereCollider.radius * MathUtility.ColliderRadiusMultiplier(sphereCollider) - c_ColliderSpacing;
                return Physics.SphereCast(sphereCollider.transform.TransformPoint(sphereCollider.center) + offset, radius, direction.normalized,
                                                                out m_RaycastHit, direction.magnitude + c_ColliderSpacing, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
            }
        }

        /// <summary>
        /// Returns the number of colliders that are overlapping the character's collider.
        /// </summary>
        /// <param name="collider">The collider to check against.</param>
        /// <param name="offset">The offset to apply to the character's collider position.</param>
        /// <returns>The number of objects which overlap the collider. These objects will be populated within m_OverlapColliderHit.</returns>
        protected int OverlapCount(Collider collider, Vector3 offset)
        {
            if (collider is CapsuleCollider) {
                Vector3 startEndCap, endEndCap;
                var capsuleCollider = collider as CapsuleCollider;
                MathUtility.CapsuleColliderEndCaps(capsuleCollider, collider.transform.position + offset, collider.transform.rotation, out startEndCap, out endEndCap);
                return Physics.OverlapCapsuleNonAlloc(startEndCap, endEndCap, capsuleCollider.radius * MathUtility.ColliderRadiusMultiplier(capsuleCollider) - c_ColliderSpacing,
                                                        m_OverlapColliderHit, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
            } else { // SphereCollider.
                var sphereCollider = collider as SphereCollider;
                return Physics.OverlapSphereNonAlloc(sphereCollider.transform.TransformPoint(sphereCollider.center) + offset,
                                                        sphereCollider.radius * MathUtility.ColliderRadiusMultiplier(sphereCollider) - c_ColliderSpacing,
                                                        m_OverlapColliderHit, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
            }
        }

        /// <summary>
        /// Resets the m_CombinedRaycastHits array to blank RaycastHit objects. This will prevent old hit objects from being used during the current frame.
        /// </summary>
        private void ResetCombinedRaycastHits()
        {
            if (m_CombinedRaycastHits == null) {
                return;
            }

            for (int i = 0; i < m_CombinedRaycastHits.Length; ++i) {
                if (m_CombinedRaycastHits[i].collider == null) {
                    break;
                }

                m_CombinedRaycastHits[i] = m_BlankRaycastHit;
            }
        }

        /// <summary>
        /// If the collision layer is disabled then all of the character's colliders will be set to an IgnoreRaycast layer. This
        /// prevents any CapsuleCast or SphereCasts from returning a collider added to the character itself.
        /// </summary>
        /// <param name="enable"></param>
        public void EnableColliderCollisionLayer(bool enable)
        {
            // Protect against duplicate enabled values changing the collider layer.
            if (m_CollisionLayerEnabled == enable) {
                return;
            }
            m_CollisionLayerEnabled = enable;

	        if (enable) {
		        for (int i = 0; i < m_ColliderCount; ++i) {
			        m_ColliderGameObjects[i].layer = m_ColliderLayers[i];
		        }
		        for (int i = 0; i < m_IgnoredColliderCount; ++i) {
			        m_IgnoredColliderGameObjects[i].layer = m_IgnoredColliderLayers[i];
		        }
	        } else {
		        for (int i = 0; i < m_ColliderCount; ++i) {
			        m_ColliderLayers[i] = m_ColliderGameObjects[i].layer;
			        m_ColliderGameObjects[i].layer = LayerManager.IgnoreRaycast;
		        }
				for (int i = 0; i < m_IgnoredColliderCount; ++i) {
					m_IgnoredColliderLayers[i] = m_IgnoredColliderGameObjects[i].layer;
					m_IgnoredColliderGameObjects[i].layer = LayerManager.IgnoreRaycast;
				}
			}
        }

        /// <summary>
        /// Adds a collider to the existing collider array.
        /// </summary>
        /// <param name="collider">The collider that should be added to the array.</param>
        public void AddCollider(Collider collider)
        {
            m_ColliderCount = AddCollider(collider, ref m_Colliders, ref m_ColliderLayers, ref m_ColliderGameObjects, m_ColliderCount);
            if (m_ColliderCount > 0 && m_ColliderIndexMap == null && m_Colliders.Length > 1) {
                m_ColliderIndexMap = new Dictionary<RaycastHit, int>(new UnityEngineUtility.RaycastHitEqualityComparer());
            }
        }

        /// <summary>
        /// Adds a collider to the existing collider array.
        /// </summary>
        /// <param name="collider">The collider that should be added to the array.</param>
        /// <param name="existingColliders">An array of colliders that the colliders array should be added to.</param>
        /// <param name="existingColliderLayers">An array of existing collider layers that may need to be resized.</param>
        /// <param name="existingColliderGameObjects">An array of existing collider GameObjects that should be updated.</param>
        /// <param name="existingColliderCount">The count of the existing colliders array.</param>
        /// <returns>The new count of the existing colliders array.</returns>
        private int AddCollider(Collider collider, ref Collider[] existingColliders, ref int[] existingColliderLayers, ref GameObject[] existingColliderGameObjects, int existingColliderCount)
        {
            // Don't add an already added collider.
            for (int i = 0; i < existingColliderCount; ++i) {
                if (existingColliders[i] == collider) {
                    return existingColliderCount;
                }
            }

            // The collider should be added.
            if (existingColliderCount == existingColliders.Length) {
                Array.Resize(ref existingColliders, existingColliders.Length + 1);
                Array.Resize(ref existingColliderLayers, existingColliderLayers.Length + 1);
                Array.Resize(ref existingColliderGameObjects, existingColliderGameObjects.Length + 1);
            }
            existingColliders[existingColliderCount] = collider;
            existingColliderGameObjects[existingColliderCount] = collider.gameObject;
            if (!m_CollisionLayerEnabled) {
                existingColliderLayers[existingColliderCount] = existingColliderGameObjects[existingColliderCount].layer;
                existingColliderGameObjects[existingColliderCount].layer = LayerManager.IgnoreRaycast;
            }
            return existingColliderCount + 1;
        }

        /// <summary>
        /// Removes the specified collider from the collider array.
        /// </summary>
        /// <param name="collider">The collider which should be removed from the array.</param>
        public void RemoveCollider(Collider collider)
        {
            m_ColliderCount = RemoveCollider(collider, ref m_Colliders, ref m_ColliderLayers, ref m_ColliderGameObjects, m_ColliderCount);
        }

        /// <summary>
        /// Removes the specified collider from the collider array.
        /// </summary>
        /// <param name="collider">The collider which should be removed from the array.</param>
        /// <param name="existingColliders">An array of colliders that the colliders array should be removed from.</param>
        /// <param name="existingColliderLayers">An array of existing collider layers that may need to be resized.</param>
        /// <param name="existingColliderGameObjects">An array of existing collider GameObjects that should be updated.</param>
        /// <param name="existingColliderCount">The count of the existing colliders array.</param>
        /// <returns>The number of colliders within the existing colliders array.</returns>
        public int RemoveCollider(Collider collider, ref Collider[] existingColliders, ref int[] existingColliderLayers, ref GameObject[] existingColliderGameObjects, int existingColliderCount)
        {
            for (int i = existingColliders.Length - 1; i > -1; --i) {
                if (existingColliders[i] != collider) {
                    continue;
                }
                // The collider may be removed when collisions are disabled. The layer should be reverted back to its original.
                if (!m_CollisionLayerEnabled) {
                    existingColliderGameObjects[i].layer = existingColliderLayers[i];
                }

                // Do not resize the array for performance reasons. Move all of the next colliders back a slot instead.
                for (int j = i; j < existingColliderCount - 1; ++j) {
                    existingColliders[j] = existingColliders[j + 1];
                    existingColliderLayers[j] = existingColliderLayers[j + 1];
                    existingColliderGameObjects[j] = existingColliderGameObjects[j + 1];
                }
                existingColliderCount--;
                existingColliders[i] = null;
                existingColliderGameObjects[i] = null;
            }
            return existingColliderCount;
        }

        /// <summary>
        /// Adds an array to the collider array.
        /// </summary>
        /// <param name="colliders">The colliders which should be added to the array.</param>
        public void AddColliders(Collider[] colliders)
        {
            m_ColliderCount = AddColliders(colliders, ref m_Colliders, ref m_ColliderLayers, ref m_ColliderGameObjects, m_ColliderCount);
            if (m_ColliderIndexMap == null && m_Colliders.Length > 1) {
                m_ColliderIndexMap = new Dictionary<RaycastHit, int>(new UnityEngineUtility.RaycastHitEqualityComparer());
            }
        }

        /// <summary>
        /// Adds the colliders array to the existing colliders array. The existing colliders array length will be resized if the new
        /// set of colliders won't fit.
        /// </summary>
        /// <param name="colliders">The colliders that should be added to the existing colliders.</param>
        /// <param name="existingColliders">An array of colliders that the colliders array should be added to.</param>
        /// <param name="existingColliderLayers">An array of existing collider layers that may need to be resized.</param>
        /// <param name="existingColliderGameObjects">An array of existing collider GameObjects that should be updated.</param>
        /// <param name="existingColliderCount">The count of the existing colliders array.</param>
        /// <returns>The new count of the existing colliders array.</returns>
        private int AddColliders(Collider[] colliders, ref Collider[] existingColliders, ref int[] existingColliderLayers, ref GameObject[] existingColliderGameObjects, int existingColliderCount)
        {
            // The array may need to be increased with the new colliders.
            if (existingColliders.Length < existingColliderCount + colliders.Length) {
                var diff = (existingColliderCount + colliders.Length) - existingColliders.Length;
                Array.Resize(ref existingColliders, existingColliders.Length + diff);
                Array.Resize(ref existingColliderLayers, existingColliderLayers.Length + diff);
                Array.Resize(ref existingColliderGameObjects, existingColliderGameObjects.Length + diff);
            }
            var startCount = existingColliderCount;
            for (int i = 0; i < colliders.Length; ++i) {
                if (colliders[i] == null) {
                    continue;
                }

                // Don't add an already added collider.
                var addCollider = true;
                for (int j = 0; j < startCount; ++j) {
                    if (colliders[i] == existingColliders[j]) {
                        addCollider = false;
                        break;
                    }
                }

                // The collider is new - add it to the array.
                if (addCollider) {
                    existingColliders[existingColliderCount] = colliders[i];
                    existingColliderGameObjects[existingColliderCount] = colliders[i].gameObject;
                    if (!m_CollisionLayerEnabled) {
                        existingColliderLayers[existingColliderCount] = existingColliderGameObjects[existingColliderCount].layer;
                        existingColliderGameObjects[existingColliderCount].layer = LayerManager.IgnoreRaycast;
                    }
                    existingColliderCount++;
                }
            }

            // Return the new collider count.
            return existingColliderCount;
        }

        /// <summary>
        /// Removes the specified colliders from the collider array.
        /// </summary>
        /// <param name="colliders">The colliders that should be removed.</param>
        public void RemoveColliders(Collider[] colliders)
        {
            m_ColliderCount = RemoveColliders(colliders, ref m_Colliders, ref m_ColliderLayers, ref m_ColliderGameObjects, m_ColliderCount);
        }

        /// <summary>
        /// Removes the colliders from the collider array.
        /// </summary>
        /// <param name="colliders">The colliders that should be removed.</param>
        /// <param name="existingColliders">An array of colliders that the colliders array should be removed from.</param>
        /// <param name="existingColliderLayers">An array of existing collider layers that may need to be resized.</param>
        /// <param name="existingColliderGameObjects">An array of existing collider GameObjects that should be updated.</param>
        /// <param name="existingColliderCount">The count of the existing colliders array.</param>
        /// <returns>The number of colliders within the existing colliders array.</returns>
        private int RemoveColliders(Collider[] colliders, ref Collider[] existingColliders, ref int[] existingColliderLayers, ref GameObject[] existingColliderGameObjects, int existingColliderCount)
        {
            for (int i = existingColliderCount - 1; i > -1; --i) {
                for (int j = colliders.Length - 1; j > -1; --j) {
                    if (existingColliders[i] != colliders[j]) {
                        continue;
                    }
                    // The collider may be removed when collisions are disabled. The layer should be reverted back to its original.
                    if (!m_CollisionLayerEnabled) {
                        existingColliderGameObjects[i].layer = existingColliderLayers[i];
                    }

                    // Do not resize the array for performance reasons. Move all of the next colliders back a slot instead.
                    for (int k = i; k < existingColliderCount - 1; ++k) {
                        existingColliders[k] = existingColliders[k + 1];
                        existingColliderLayers[k] = existingColliderLayers[k + 1];
                        existingColliderGameObjects[k] = existingColliderGameObjects[k + 1];
                    }
                    existingColliderCount--;
                    existingColliders[existingColliderCount] = null;
                    existingColliderGameObjects[existingColliderCount] = null;
                    break;
                }
            }
            return existingColliderCount;
        }

        /// <summary>
        /// Adds an element to the ignored collider array.
        /// </summary>
        /// <param name="collider">The collider which should be added to the array.</param>
        public void AddIgnoredCollider(Collider collider)
        {
            m_IgnoredColliderCount = AddCollider(collider, ref m_IgnoredColliders, ref m_IgnoredColliderLayers, ref m_IgnoredColliderGameObjects, m_IgnoredColliderCount);
        }

        /// <summary>
        /// Removes the specified collider from the ignored collider array.
        /// </summary>
        /// <param name="collider">The collider which should be removed from the array.</param>
        public void RemoveIgnoredCollider(Collider collider)
        {
            m_IgnoredColliderCount = RemoveCollider(collider, ref m_IgnoredColliders, ref m_IgnoredColliderLayers, ref m_IgnoredColliderGameObjects, m_IgnoredColliderCount);
        }

        /// <summary>
        /// Adds an array to the ignored collider array.
        /// </summary>
        /// <param name="colliders">The colliders which should be added to the array.</param>
        public void AddIgnoredColliders(Collider[] colliders)
        {
            m_IgnoredColliderCount = AddColliders(colliders, ref m_IgnoredColliders, ref m_IgnoredColliderLayers, ref m_IgnoredColliderGameObjects, m_IgnoredColliderCount);
        }

        /// <summary>
        /// Removes the specified colliders from the ignored collider array.
        /// </summary>
        /// <param name="colliders">The colliders that should be removed.</param>
        public void RemoveIgnoredColliders(Collider[] colliders)
        {
            m_IgnoredColliderCount = RemoveColliders(colliders, ref m_IgnoredColliders, ref m_IgnoredColliderLayers, ref m_IgnoredColliderGameObjects, m_IgnoredColliderCount);
        }

        /// <summary>
        /// Adds a force to the character. The force will either be an external or soft force.
        /// </summary>
        /// <param name="force">The force to add.</param>
        public void AddForce(Vector3 force)
        {
            AddForce(force, 1, true, true);
        }

        /// <summary>
        /// Adds a force to the character in the specified number of frames. The force will either be an external or soft force.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        public void AddForce(Vector3 force, int frames)
        {
            AddForce(force, frames, true, true);
        }

        /// <summary>
        /// Adds a force to the character in the specified number of frames. The force will either be an external or soft force.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        /// <param name="scaleByTime">Should the force be scaled by the timescale?</param>
        /// <param name="scaleByMass">Should the force be scaled by the character's mass?</param>
        public void AddForce(Vector3 force, int frames, bool scaleByMass, bool scaleByTime)
        {
            if (scaleByMass) {
                force /= m_Mass;
            }
            if (frames > 1) {
                AddSoftForce(force, frames, scaleByTime);
            } else {
                AddExternalForce(force, scaleByTime);
            }
        }

        /// <summary>
        /// Adds an external force to add.
        /// </summary>
        /// <param name="force">The force to add.</param>
        private void AddExternalForce(Vector3 force)
        {
            AddExternalForce(force, true);
        }

        /// <summary>
        /// Adds an external force to add.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="scaleByTime">Should the force be scaled by the timescale?</param>
        private void AddExternalForce(Vector3 force, bool scaleByTime)
        {
            // The force may already account for a variable time scale in which case the force should not be scaled by time. For example, the jump damping force.
            if (scaleByTime) {
                var timeScale = m_TimeScale * Time.timeScale;
                if (timeScale < 1) {
                    force /= timeScale;
                }
            }
            m_ExternalForce += force;
        }

        /// <summary>
        /// Adds a soft force to the character. A soft force is spread out through up to c_MaxSoftForceFrames frames.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        private void AddSoftForce(Vector3 force, float frames)
        {
            AddSoftForce(force, frames, true);
        }

        /// <summary>
        /// Adds a soft force to the character. A soft force is spread out through up to c_MaxSoftForceFrames frames.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        /// <param name="scaleByTime">Should the force be scaled by the timescale?</param>
        private void AddSoftForce(Vector3 force, float frames, bool scaleByTime)
        {
            frames = Mathf.Clamp(frames, 1, m_MaxSoftForceFrames);
            AddExternalForce(force / frames, scaleByTime);
            var timeScale = (scaleByTime ? m_TimeScale * Time.timeScale : 1f);
            for (int v = 0; v < (Mathf.RoundToInt(frames) - 1); v++) {
                m_SoftForceFrames[v] += (force / (frames * timeScale));
            }
        }

        /// <summary>
        /// Adds a force relative to the character. The force will either be an external or soft force.
        /// </summary>
        /// <param name="force">The force to add.</param>
        public void AddRelativeForce(Vector3 force)
        {
            AddRelativeForce(force, 1);
        }

        /// <summary>
        /// Adds a force relative to the character in the specified number of frames. The force will either be an external or soft force.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        public void AddRelativeForce(Vector3 force, int frames)
        {
            AddRelativeForce(force, frames, true, true);
        }

        /// <summary>
        /// Adds a force relative to the character in the specified number of frames. The force will either be an external or soft force.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        /// <param name="scaleByMass">Should the force be scaled by the character's mass?</param>
        /// <param name="scaleByTime">Should the force be scaled by the timescale?</param>
        public void AddRelativeForce(Vector3 force, int frames, bool scaleByMass, bool scaleByTime)
        {
            // Convert the force into a relative force.
            force = m_Transform.InverseTransformVector(force);

            AddForce(force, frames, scaleByMass, scaleByTime);
        }

        /// <summary>
        /// Pushes the target Rigidbody in the specified direction.
        /// </summary>
        /// <param name="targetRigidbody">The Rigidbody to push.</param>
        /// <param name="moveDirection">The direction that the character is moving.</param>
        /// <param name="point">The point at which to apply the push force.</param>
        /// <param name="radius">The radius of the pushing collider.</param>
        /// <returns>Was the rigidbody pushed?</returns>
        protected virtual bool PushRigidbody(Rigidbody targetRigidbody, Vector3 moveDirection, Vector3 point, float radius)
        {
            if (targetRigidbody.isKinematic) {
                return false;
            }

            targetRigidbody.AddForceAtPosition((moveDirection / Time.deltaTime) * (m_Mass / targetRigidbody.mass) * 0.01f, point, ForceMode.VelocityChange);
            return targetRigidbody.velocity.sqrMagnitude > 0.1f;
        }

        /// <summary>
        /// Returns the collider which contains the point within its bounding box.
        /// </summary>
        /// <param name="point">The point to determine if it is within the bounding box of the character.</param>
        /// <returns>The collider which contains the point within its bounding box. Can be null.</returns>
        public Collider BoundsCountains(Vector3 point)
        {
            for (int i = 0; i < m_ColliderCount; ++i) {
                if (m_Colliders[i].bounds.Contains(point)) {
                    return m_Colliders[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Sets the rotation of the character.
        /// </summary>
        /// <param name="rotation">The rotation to set.</param>
        public virtual void SetRotation(Quaternion rotation)
        {
            m_Transform.rotation = m_MotorRotation = m_PrevMotorRotation = rotation;
            m_LocalRootMotionRotation = m_Torque = Quaternion.identity;
            m_Up = m_Transform.up;
            if (m_AlignToGravity) {
                m_GravityDirection = -m_Up;
            }
        }

        /// <summary>
        /// Sets the position of the character.
        /// </summary>
        /// <param name="position">The position to set.</param>
        public virtual void SetPosition(Vector3 position)
        {
            m_Transform.position = m_PrevPosition = position;
            m_MotorThrottle = m_LocalRootMotionForce = m_MoveDirection = m_Velocity = m_ExternalForce = Vector3.zero;
            m_GravityAmount = 0;
            m_GroundHitTransform = null;

            if (m_Platform != null) {
                if (m_PlatformOverride) {
                    m_PlatformRelativePosition = m_Platform.InverseTransformPoint(m_Transform.position);
                } else {
                    // Remove the moving platform if the character changes position. It will be set again on the next update if the character is on a moving platform.
                    UpdateMovingPlatformTransform(null, true);
                }
            }
        }

        /// <summary>
        /// Resets the rotation and position to their default values.
        /// </summary>
        public virtual void ResetRotationPosition()
        {
            m_MotorRotation = m_PrevMotorRotation = m_Transform.rotation;
            m_Up = m_Transform.up;
            m_LocalRootMotionRotation = m_Torque = Quaternion.identity;
            if (m_AlignToGravity) {
                m_GravityDirection = -m_Up;
            }

            m_PrevPosition = m_Transform.position;
            m_MotorThrottle = m_LocalRootMotionForce = m_MoveDirection = m_Velocity = m_ExternalForce = Vector3.zero;
        }

        /// <summary>
        /// Resets the variables to the default values.
        /// </summary>
        private void Reset()
        {
            AddDefaultSmoothedBones();
        }

        /// <summary>
        /// Adds the default humanoid smoothed bones.
        /// </summary>
        public void AddDefaultSmoothedBones()
        {
            var animator = gameObject.GetComponent<Animator>();
            if (animator == null || !animator.isHuman) {
                return;
            }

            // The smoothed bone variable should be populated with all of the humanoid bones.
            var bones = new List<Transform>();
            AddBone(bones, animator, HumanBodyBones.Spine);
            AddBone(bones, animator, HumanBodyBones.Chest);
            AddBone(bones, animator, HumanBodyBones.Neck);
            AddBone(bones, animator, HumanBodyBones.Head);
            AddBone(bones, animator, HumanBodyBones.LeftShoulder);
            AddBone(bones, animator, HumanBodyBones.LeftUpperArm);
            AddBone(bones, animator, HumanBodyBones.LeftLowerArm);
            AddBone(bones, animator, HumanBodyBones.LeftHand);
            AddBone(bones, animator, HumanBodyBones.RightShoulder);
            AddBone(bones, animator, HumanBodyBones.RightUpperArm);
            AddBone(bones, animator, HumanBodyBones.RightLowerArm);
            AddBone(bones, animator, HumanBodyBones.RightHand);

            if (bones.Count > 0) {
                m_SmoothedBones = bones.ToArray();
            }
        }

        /// <summary>
        /// Adds the bone to the list if the bone exists.
        /// </summary>
        /// <param name="bones">The list of current bones.</param>
        /// <param name="animator">A reference to the character's animator.</param>
        /// <param name="bone">The humanoid bone that should be added if it exists.</param>
        private void AddBone(List<Transform> bones, Animator animator, HumanBodyBones bone)
        {
            var boneTransform = animator.GetBoneTransform(bone);
            if (boneTransform != null) {
                bones.Add(boneTransform);
            }
        }
    }
}