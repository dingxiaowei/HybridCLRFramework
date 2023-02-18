/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Audio;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.SurfaceSystem;
    using Opsive.UltimateCharacterController.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A trajectory object follows a kinematic parabolic curve and can be simuated using the SimulateTrajectory method.
    /// </summary>
    public class TrajectoryObject : MonoBehaviour, IForceObject
    {
        // Padding value used to prevent the collider from overlapping the environment collider. Overlapped colliders don't work well with ray casts.
        private const float c_ColliderSpacing = 0.01f;

        /// <summary>
        /// Specifies how the object should behave after hitting another collider.
        /// </summary>
        public enum CollisionMode
        {
            Collide,        // Collides with the object. Does not bounce.
            Reflect,        // Reflect according to the velocity.
            RandomReflect,  // Reflect in a random direction. This mode will make the object nonkinematic but for visual only objects such as shells this is preferred.
            Ignore          // Passes through the object. A collision is reported.
        }

        [Tooltip("Should the component initialize when enabled?")]
        [SerializeField] protected bool m_InitializeOnEnable = false;
        [Tooltip("The mass of the object.")]
        [SerializeField] protected float m_Mass = 1;
        [Tooltip("Multiplies the starting velocity by the specified value.")]
        [SerializeField] protected float m_StartVelocityMultiplier = 1;
        [Tooltip("The amount of gravity to apply to the object.")]
        [Range(0, 40)] [SerializeField] protected float m_GravityMagnitude = 9.8f;
        [Tooltip("The movement speed.")]
        [SerializeField] protected float m_Speed = 1;
        [Tooltip("The rotation speed.")]
        [SerializeField] protected float m_RotationSpeed = 5;
        [Tooltip("The amount of damping to apply to the movement.")]
        [Range(0, 1)] [SerializeField] protected float m_Damping = 0.1f;
        [Tooltip("Amount of damping to apply to the torque.")]
        [Range(0, 1)] [SerializeField] protected float m_RotationDamping = 0.1f;
        [Tooltip("Should the object rotate in the direction that it is moving?")]
        [SerializeField] protected bool m_RotateInMoveDirection;
        [Tooltip("When the velocity and torque have a square magnitude value less than the specified value the object will be considered settled.")]
        [SerializeField] protected float m_SettleThreshold;
        [Tooltip("Specifies if the collider should settle on its side or upright. The higher the value the more likely the collider will settle on its side. " +
                 "This is only used for CapsuleColliders and BoxColliders.")]
        [Range(0, 1)] [SerializeField] protected float m_SidewaysSettleThreshold = 0.75f;
        [Tooltip("Starts to rotate to the settle rotation when the velocity magnitude is less than the specified values.")]
        [SerializeField] protected float m_StartSidewaysVelocityMagnitude = 3f;
        [Tooltip("The layers that the object can collide with.")]
        [SerializeField] protected LayerMask m_ImpactLayers = ~(1 << LayerManager.IgnoreRaycast | 1 << LayerManager.Water | 1 << LayerManager.SubCharacter | 1 << LayerManager.Overlay |
                                                                1 << LayerManager.VisualEffect);
        [Tooltip("The identifier that is used when the object collides with another object.")]
        [SerializeField] protected SurfaceImpact m_SurfaceImpact;
        [Tooltip("When a force is applied the multiplier will modify the magnitude of the force.")]
        [SerializeField] protected float m_ForceMultiplier = 40;
        [Tooltip("Specifies how the object should behave after hitting another collider.")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_BounceMode")] // 2.2.
        [SerializeField] protected CollisionMode m_CollisionMode = CollisionMode.Reflect;
        [Tooltip("If the object can reflect, specifies the multiplier to apply to the reflect velocity.")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_BounceMultiplier")]
        [Range(0, 4)] [SerializeField] protected float m_ReflectMultiplier = 1;
        [Tooltip("The maximum number of objects the projectile can collide with at a time.")]
        [SerializeField] protected int m_MaxCollisionCount = 5;
        [Tooltip("The maximum number of positions any single curve amplitude can contain.")]
        [SerializeField] protected int m_MaxPositionCount = 150;
        [Tooltip("The audio that should be looped while the object is active.")]
        [SerializeField] protected AudioClipSet m_ActiveAudioClipSet = new AudioClipSet();

        public float Mass { get { return m_Mass; } set { m_Mass = value; } }
        public float StartVelocityMultiplier { get { return m_StartVelocityMultiplier; } set { m_StartVelocityMultiplier = value; } }
        public float GravityMagnitude { get { return m_GravityMagnitude; } set { m_GravityMagnitude = value; } }
        public float Speed { get { return m_Speed; } set { m_Speed = value; } }
        public float RotationSpeed { get { return m_RotationSpeed; } set { m_RotationSpeed = value; } }
        public float Damping { get { return m_Damping; } set { m_Damping = value; } }
        public float RotationDamping { get { return m_RotationDamping; } set { m_RotationDamping = value; } }
        public bool RotateInMoveDirection { get { return m_RotateInMoveDirection; } set { m_RotateInMoveDirection = value; } }
        public float SettleThreshold { get { return m_SettleThreshold; } set { m_SettleThreshold = value; } }
        public float SidewaysSettleThreshold { get { return m_SidewaysSettleThreshold; } set { m_SidewaysSettleThreshold = value; } }
        public float StartSidewaysVelocityMagnitude { get { return m_StartSidewaysVelocityMagnitude; } set { m_StartSidewaysVelocityMagnitude = value; } }
        public LayerMask ImpactLayers { get { return m_ImpactLayers; } set { m_ImpactLayers = value; } }
        public SurfaceImpact SurfaceImpact { get { return m_SurfaceImpact; } set { m_SurfaceImpact = value; } }
        public float ForceMultiplier { get { return m_ForceMultiplier; } set { m_ForceMultiplier = value; } }
        public CollisionMode Collision { get { return m_CollisionMode; } set { m_CollisionMode = value; } }
        public float ReflectMultiplier { get { return m_ReflectMultiplier; } set { m_ReflectMultiplier = value; } }
        public AudioClipSet ActiveAudioClipSet { get { return m_ActiveAudioClipSet; } set { m_ActiveAudioClipSet = value; } }

        protected GameObject m_GameObject;
        protected Transform m_Transform;
        protected Collider m_Collider;
        protected GameObject m_Originator;
        protected Transform m_OriginatorTransform;
        protected UltimateCharacterLocomotion m_OriginatorCharacterLocomotion;
        private LineRenderer m_LineRenderer;

        protected RaycastHit m_RaycastHit;
        protected Collider[] m_ColliderHit;
        private List<Vector3> m_Positions;

        private Vector3 m_Gravity;
        protected Vector3 m_NormalizedGravity;
        protected Vector3 m_Velocity;
        protected Vector3 m_Torque;
        private bool m_DeterminedRotation;
        private bool m_SettleSideways;
        private bool m_OriginatorCollisionCheck;

        private float m_TimeScale;
        private bool m_AutoDisable;
        private bool m_MovementSettled;
        private bool m_RotationSettled;
        private bool m_InCollision;
        private bool m_Collided;

        private Transform m_Platform;
        private Vector3 m_PlatformRelativePosition;
        private Quaternion m_PrevPlatformRotation;

        public GameObject Originator { get { return m_Originator; } }
        public Vector3 Velocity { get { return m_Velocity; } }
        public Vector3 Torque { get { return m_Torque; } }
        public LineRenderer LineRenderer { get { return m_LineRenderer; } }
        protected bool AutoDisable { set { m_AutoDisable = value; } }

        /// <summary>
        /// Initialize the defualt values.
        /// </summary>
        protected virtual void Awake()
        {
            // The movement will be controlled by the TrajectoryObject.
            var rigidbody = GetComponent<Rigidbody>();
            if (rigidbody != null) {
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;
            }

            // The object may want to play audio.
            var hasActiveAudioClipSet = false;
            if ((hasActiveAudioClipSet = (m_ActiveAudioClipSet.AudioClips != null && m_ActiveAudioClipSet.AudioClips.Length > 0 && m_ActiveAudioClipSet.AudioClips[0] != null)) || 
                        m_SurfaceImpact != null) {
                AudioManager.Register(gameObject);
                // The looping audio should have a reserved index of 0.
                if (hasActiveAudioClipSet) {
                    AudioManager.SetReserveCount(gameObject, 1);
                }
            }

            enabled = m_InitializeOnEnable;
        }

        /// <summary>
        /// The component has been enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (m_InitializeOnEnable) {
                InitializeComponentReferences();
                Initialize(Vector3.zero, Vector3.zero, null, false, -m_Transform.up);
            }
        }

        /// <summary>
        /// Runs a simulation of the parabolic trajectory with the given start and end position. The trajectory will then be displayed with the attached LineRenderer.
        /// </summary>
        /// <param name="originator">The object that instantiated the trajectory object.</param>
        /// <param name="startPosition">The starting position.</param>
        /// <param name="endPosition">The ending position.</param>
        public void SimulateTrajectory(GameObject originator, Vector3 startPosition, Vector3 endPosition)
        {
            var velocity = CalculateVelocity(startPosition, endPosition);
            Initialize(velocity, Vector3.zero, originator);

            if (m_LineRenderer == null) {
                Debug.LogError($"Error: A LineRenderer must be added to the Trajectory Object {name}.", this);
                return;
            }
            if (m_Positions == null) {
                m_Positions = new List<Vector3>();
            } else {
                m_Positions.Clear();
            }
            m_Positions.Add(startPosition);
            SimulateTrajectory(startPosition, m_Transform.rotation, m_Positions, 0);
            // Insert the end position into the list to ensure the complete curve is shown.
            m_Positions[m_Positions.Count - 1] = endPosition;

            // Show the curve.
            m_LineRenderer.positionCount = m_Positions.Count;
            m_LineRenderer.SetPositions(m_Positions.ToArray());

            m_MovementSettled = m_RotationSettled = false;
            m_Collided = false;
            if (m_Collider != null) {
                m_Collider.enabled = false;
            }
            enabled = false;
        }

        /// <summary>
        /// Runs a simulation of the parabolic trajectory.  The trajectory will then be displayed with the attached LineRenderer.
        /// </summary>
        /// <param name="originator">The object that instantiated the trajectory object.</param>
        /// <param name="position">The starting position.</param>
        /// <param name="rotation">The starting rotation.</param>
        /// <param name="velocity">The starting velocity.</param>
        /// <param name="torque">The starting torque.</param>
        public void SimulateTrajectory(GameObject originator, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 torque)
        {
            Initialize(velocity, torque, originator, false);

            if (m_LineRenderer == null) {
                Debug.LogError($"Error: A LineRenderer must be added to the Trajectory Object {name}.", this);
                return;
            }
            if (m_Positions == null) {
                m_Positions = new List<Vector3>();
            } else {
                m_Positions.Clear();
            }
            m_Positions.Add(position);
            SimulateTrajectory(position, rotation, m_Positions, 0);

            // Show the curve.
            m_LineRenderer.positionCount = m_Positions.Count;
            m_LineRenderer.SetPositions(m_Positions.ToArray());

            m_MovementSettled = m_RotationSettled = false;
            m_InCollision = false;
            m_Collided = false;
            if (m_Collider != null) {
                m_Collider.enabled = false;
            }
            enabled = false;
        }

        /// <summary>
        /// Clears the trajectory from the LineRenderer.
        /// </summary>
        public void ClearTrajectory()
        {
            if (m_LineRenderer == null) {
                return;
            }

            m_LineRenderer.positionCount = 0;
            if (m_Originator != null) {
                EventHandler.UnregisterEvent<float>(m_Originator, "OnCharacterChangeTimeScale", OnChangeTimeScale);
                m_Originator = null;
                m_OriginatorCharacterLocomotion = null;
            }
        }

        /// <summary>
        /// Calculates the velocity to move from startPosition to endPosition.
        /// </summary>
        /// <param name="startPosition">The starting position.</param>
        /// <param name="endPosition">The ending position.</param>
        /// <returns>The velocity required to move from startPosition to endPosition.</returns>
        private Vector3 CalculateVelocity(Vector3 startPosition, Vector3 endPosition)
        {
            var direction = endPosition - startPosition;
            return direction - m_Gravity * 0.5f + direction * 0.02f;
        }

        /// <summary>
        /// Runs a simulation of the parabolic trajectory. Will save off the positions in the positions list.
        /// </summary>
        /// <param name="originator">The object that instantiated the trajectory object.</param>
        /// <param name="velocity">The starting velocity.</param>
        /// <param name="torque">The starting torque.</param>
        /// <param name="positions">The list of positions that the object will move through.</param>
        /// <param name="positionsSkip">Reduce the number of saved positions by skipping a specified number of positions.</param>
        public void SimulateTrajectory(GameObject originator, Vector3 velocity, Vector3 torque, List<Vector3> positions, int positionsSkip)
        {
            Initialize(velocity, torque, originator, false);

            SimulateTrajectory(m_Transform.position, m_Transform.rotation, positions, positionsSkip);
            if (m_Collider != null) {
                m_Collider.enabled = false;
            }
            enabled = false;
        }

        /// <summary>
        /// Runs a simulation of the parabolic trajectory. Will save off the positions in the positions list.
        /// </summary>
        /// <param name="position">The current position of the object.</param>
        /// <param name="rotation">The current rotation of the object.</param>
        /// <param name="positions">The list of positions that the object will move through.</param>
        /// <param name="positionsSkip">Reduce the number of saved positions by skipping a specified number of positions.</param>
        public void SimulateTrajectory(Vector3 position, Quaternion rotation, List<Vector3> positions, int positionsSkip)
        {
            for (int i = 0; i < m_MaxPositionCount; i++) {
                // Saving every position may be too high of a resolution than what is necessary - allow every x number of positions be skipped.
                for (int j = 0; j < positionsSkip + 1; ++j) {
                    if (!Move(ref position, rotation)) {
                        // If the object hit a collider then SimulateTrajectory should be recused and run again with the updated position and rotation value.
                        SimulateTrajectory(position, rotation, positions, positionsSkip);
                        return;
                    }

                    Rotate(position, ref rotation);
                }
                positions.Add(position);

                // The loop can stop when both the position and rotation have settled.
                if (m_MovementSettled && m_RotationSettled) {
                    break;
                }
            }
        }

        /// <summary>
        /// Initializes the object with the specified velocity and torque.
        /// </summary>
        /// <param name="velocity">The starting velocity.</param>
        /// <param name="torque">The starting torque.</param>
        /// <param name="originator">The object that instantiated the trajectory object.</param>
        public virtual void Initialize(Vector3 velocity, Vector3 torque, GameObject originator)
        {
            Initialize(velocity, torque, originator, true);
        }

        /// <summary>
        /// Initializes the object with the specified velocity and torque.
        /// </summary>
        /// <param name="velocity">The starting velocity.</param>
        /// <param name="torque">The starting torque.</param>
        /// <param name="originator">The object that instantiated the trajectory object.</param>
        /// <param name="originatorCollisionCheck">Should a collision check against the originator be performed?</param>
        public virtual void Initialize(Vector3 velocity, Vector3 torque, GameObject originator, bool originatorCollisionCheck)
        {
            Initialize(velocity, torque, originator, originatorCollisionCheck, Vector3.down);
        }

        /// <summary>
        /// Initializes the object with the specified velocity and torque.
        /// </summary>
        /// <param name="velocity">The starting velocity.</param>
        /// <param name="torque">The starting torque.</param>
        /// <param name="originator">The object that instantiated the trajectory object.</param>
        /// <param name="originatorCollisionCheck">Should a collision check against the originator be performed?</param>
        /// <param name="defaultNormalizedGravity">The normalized gravity direction if a character isn't specified for the originator.</param>
        public virtual void Initialize(Vector3 velocity, Vector3 torque, GameObject originator, bool originatorCollisionCheck, Vector3 defaultNormalizedGravity)
        {
            InitializeComponentReferences();

            m_Velocity = velocity / m_Mass * m_StartVelocityMultiplier;
            m_Torque = torque;
            SetOriginator(originator, defaultNormalizedGravity);
            m_Gravity = m_NormalizedGravity * m_GravityMagnitude;
            m_OriginatorCollisionCheck = m_Originator != null;

            m_Platform = null;
            m_MovementSettled = m_RotationSettled = false;
            m_InCollision = false;
            m_Collided = false;
            if (m_Collider != null) {
                m_Collider.enabled = true;
            }
            m_ActiveAudioClipSet.PlayAudioClip(m_GameObject, 0, true);
            enabled = true;

            // Set the layer to prevent the current object from getting in the way of the casts.
            var previousLayer = m_GameObject.layer;
            m_GameObject.layer = LayerManager.IgnoreRaycast;

            // The object could start in a collision state.
            if (originatorCollisionCheck && OverlapCast(m_Transform.position, m_Transform.rotation)) {
                OnCollision(null);
                if (m_CollisionMode == CollisionMode.Collide) {
                    m_MovementSettled = m_RotationSettled = true;
                } else if (m_CollisionMode != CollisionMode.Ignore) { // Reflect and Random Reflection.
                    // Update the velocity to the reflection direction. Use the originator's forward direction as the normal because the actual collision point is not determined.
                    m_Velocity = Vector3.Reflect(m_Velocity, -m_OriginatorTransform.forward) * m_ReflectMultiplier;
                }
            }

            m_GameObject.layer = previousLayer;
        }

        /// <summary>
        /// Sets the originator of the TrajectoryObject.
        /// </summary>
        /// <param name="originator">The originator that should be set.</param>
        /// <param name="defaultNormalizedGravity">The default gravity direction.</param>
        protected void SetOriginator(GameObject originator, Vector3 defaultNormalizedGravity)
        {
            if (m_Originator == originator) {
                return;
            }

            if (originator != null) {
                m_Originator = originator;
                m_OriginatorTransform = m_Originator.transform;
                m_OriginatorCharacterLocomotion = m_Originator.GetCachedComponent<UltimateCharacterLocomotion>();
                if (m_OriginatorCharacterLocomotion != null) {
                    m_NormalizedGravity = m_OriginatorCharacterLocomotion.GravityDirection;
                    m_TimeScale = m_OriginatorCharacterLocomotion.TimeScale;
                    EventHandler.RegisterEvent<float>(m_Originator, "OnCharacterChangeTimeScale", OnChangeTimeScale);
                } else {
                    m_NormalizedGravity = defaultNormalizedGravity;
                    m_TimeScale = 1;
                }
            } else {
                m_NormalizedGravity = defaultNormalizedGravity;
                m_TimeScale = 1;
                m_OriginatorTransform = null;
            }
        }

        /// <summary>
        /// Retruns true if any objects are overlapping with the Trajectory Object.
        /// </summary>
        /// <param name="position">The position of the cast.</param>
        /// <param name="rotation">The rotation of the cast.</param>
        /// <returns>True if any objects are overlapping with the Trajectory Object.</returns>
        private bool OverlapCast(Vector3 position, Quaternion rotation)
        {
            // No need to do a cast if the originator is null.
            if (m_OriginatorTransform == null) {
                return false;
            }

            int hit = 0;
            if (m_Collider is SphereCollider) {
                var sphereCollider = m_Collider as SphereCollider;
                hit = Physics.OverlapSphereNonAlloc(MathUtility.TransformPoint(position, m_Transform.rotation, sphereCollider.center), 
                                sphereCollider.radius * MathUtility.ColliderRadiusMultiplier(sphereCollider), m_ColliderHit, m_ImpactLayers, QueryTriggerInteraction.Ignore);
            } else if (m_Collider is CapsuleCollider) {
                var capsuleCollider = m_Collider as CapsuleCollider;
                Vector3 startEndCap, endEndCap;
                MathUtility.CapsuleColliderEndCaps(capsuleCollider, position, rotation, out startEndCap, out endEndCap);
                hit = Physics.OverlapCapsuleNonAlloc(startEndCap, endEndCap, capsuleCollider.radius * MathUtility.ColliderRadiusMultiplier(capsuleCollider), m_ColliderHit, m_ImpactLayers, QueryTriggerInteraction.Ignore);
            } else if (m_Collider is BoxCollider) {
                var boxCollider = m_Collider as BoxCollider;
                hit = Physics.OverlapBoxNonAlloc(MathUtility.TransformPoint(position, m_Transform.rotation, boxCollider.center), Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale) / 2, m_ColliderHit, m_Transform.rotation, m_ImpactLayers, QueryTriggerInteraction.Ignore);
            }

            if (hit > 0) {
                // The TrajectoryObject is only in an overlap state if the object is overlapping a non-character or camera collider.
                for (int i = 0; i < hit; ++i) {
                    if (!m_ColliderHit[i].transform.IsChildOf(m_OriginatorTransform)
#if FIRST_PERSON_CONTROLLER
                    // The object should not hit any colliders who are a child of the camera.
                    && m_ColliderHit[i].transform.gameObject.GetCachedParentComponent<FirstPersonController.Character.FirstPersonObjects>() == null
#endif
                    ) {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Initializes the local component references.
        /// </summary>
        protected void InitializeComponentReferences()
        {
            if (m_GameObject != null) {
                return;
            }
            m_GameObject = gameObject;
            m_Transform = transform;
            var colliders = GetComponents<Collider>();
            for (int i = 0; i < colliders.Length; ++i) {
                // The collider cannot be a triger.
                if (colliders[i].isTrigger) {
                    continue;
                }

                // The collider has to be of the correct type.
                if (!(colliders[i] is SphereCollider) && !(colliders[i] is CapsuleCollider) && !(colliders[i] is BoxCollider)) {
                    continue;
                }

                m_Collider = colliders[i];
                break;
            }
            m_LineRenderer = GetComponent<LineRenderer>();
            m_ColliderHit = new Collider[m_MaxCollisionCount];
        }

        /// <summary>
        /// Move and rotate the object according to a parabolic trajectory.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            // Update the position.
            var position = m_Transform.position;
            var rotation = m_Transform.rotation;

            // Set the layer to prevent the current object from getting in the way of the casts.
            var previousLayer = m_GameObject.layer;
            m_GameObject.layer = LayerManager.IgnoreRaycast;

            if (!Move(ref position, rotation)) {
                // If the object collided with another object then Move should be called one more time so the reflected velocity is used.
                // If the second Move method is not called then the object would wait a tick before it is moved.
                Move(ref position, rotation);
            }

            // The object may have been disabed within OnCollision. Do not do any more updates for a disabled object.
            if (enabled) {
                if (m_Platform != null) {
                    position += (m_Platform.TransformPoint(m_PlatformRelativePosition) - position);
                    m_PlatformRelativePosition = m_Platform.InverseTransformPoint(position);
                }
                m_Transform.position = position;

                // Update the rotation.
                Rotate(position, ref rotation);
                if (m_Platform != null) {
                    rotation *= (m_Platform.rotation * Quaternion.Inverse(m_PrevPlatformRotation));
                    m_PrevPlatformRotation = m_Platform.rotation;
                }
                m_Transform.rotation = rotation;
            }

            m_GameObject.layer = previousLayer;

            // If both the position and rotation are done making changes then the component can be disabled.
            if (m_AutoDisable && m_MovementSettled && m_RotationSettled) {
                enabled = false;
            }
        }

        /// <summary>
        /// Move the object based on the current velocity.
        /// </summary>
        /// <param name="position">The current position of the object. Passed by reference so the updated position can be set.</param>
        /// <param name="rotation">The current rotation of the object.</param>
        /// <returns>True if the position was updated or the movement has settled.</returns>
        private bool Move(ref Vector3 position, Quaternion rotation)
        {
            // The object can't move if the movement and rotation has settled.
            if (m_MovementSettled && m_RotationSettled && m_SettleThreshold > 0) {
                return true;
            }

            // Stop moving if the velocity is less than a minimum threshold and the object is on the ground.
            if (m_Velocity.sqrMagnitude < m_SettleThreshold && m_RotationSettled) {
                // The object should be on the ground before the object has settled.
                if (SingleCast(position, rotation, m_NormalizedGravity * c_ColliderSpacing)) {
                    m_MovementSettled = true;
                    return true;
                }
            }

            var deltaTime = m_TimeScale * Time.fixedDeltaTime * Time.timeScale;

            // The object hasn't settled yet - move based on the velocity.
            m_Velocity += m_Gravity * deltaTime;
            m_Velocity *= Mathf.Clamp01(1 - m_Damping * deltaTime);

            // If the object hits an object then it should either reflect off of that object or stop moving.
            var targetPosition = position + m_Velocity * m_Speed * deltaTime;
            var direction = targetPosition - position;
            if (SingleCast(position, rotation, direction)) {
                if (m_RaycastHit.transform.gameObject.layer == LayerManager.MovingPlatform) {
                    if (m_RaycastHit.transform != m_Platform) {
                        m_Platform = m_RaycastHit.transform;
                        m_PlatformRelativePosition = m_Platform.InverseTransformPoint(position);
                        m_PrevPlatformRotation = m_Platform.rotation;
                    }
                } else {
                    m_Platform = null;
                }

                // If the object has settled but not disabled a collision will occur every frame. Prevent the effects from playing because of this.
                if (!m_InCollision) {
                    m_InCollision = true;
                    OnCollision(m_RaycastHit);
                }

                if (m_CollisionMode == CollisionMode.Collide) {
                    m_Velocity = Vector3.zero;
                    m_Torque = Vector3.zero;
                    m_MovementSettled = true;
                    enabled = false;
                    return true;
                } else if (m_CollisionMode != CollisionMode.Ignore) { // Reflect and Random Reflect.
                    Vector3 velocity;
                    if (m_CollisionMode == CollisionMode.RandomReflect) {
                        // Add ramdomness to the bounce.
                        // This mode should not be used over the network unless it doesn't matter if the object is synchronized (such as a shell).
                        velocity = Quaternion.AngleAxis(Random.Range(-70, 70), m_RaycastHit.normal) * m_Velocity;
                    } else { // Reflect.
                        velocity = m_Velocity;
                    }

                    // The bounce strenght is dependent on the physic material.
                    var dynamicFrictionValue = m_Collider != null ? Mathf.Clamp01(1 - MathUtility.FrictionValue(m_Collider.material, m_RaycastHit.collider.material, true)) : 0;
                    // Update the velocity to the reflection direction.
                    m_Velocity = Vector3.Reflect(velocity, m_RaycastHit.normal) * dynamicFrictionValue * m_ReflectMultiplier;
                    if (m_Velocity.magnitude < m_StartSidewaysVelocityMagnitude) {
                        m_MovementSettled = true;
                    }
                    m_Collided = true;
                    return false;
                }
            } else {
                m_Platform = null;
                m_InCollision = false;
            }
            position = targetPosition;
            return true;
        }

        /// <summary>
        /// The object has collided with another object.
        /// </summary>
        /// <param name="hit">The RaycastHit of the object. Can be null.</param>
        protected virtual void OnCollision(RaycastHit? hit)
        {
            if (hit != null && hit.HasValue) {
                m_ActiveAudioClipSet.Stop(m_GameObject, 0);

                // A Rigidbody should be affected by the impact.
                if (hit.Value.rigidbody != null) {
                    hit.Value.rigidbody.AddForceAtPosition(m_Velocity, hit.Value.point);
                }

                // An impact has occurred.
                if (m_SurfaceImpact != null) {
                    SurfaceManager.SpawnEffect(hit.Value, m_SurfaceImpact, m_NormalizedGravity, m_TimeScale, m_GameObject);
                }
            }
        }

        /// <summary>
        /// Does a cast in in the specified direction.
        /// </summary>
        /// <param name="position">The position of the cast.</param>
        /// <param name="rotation">The rotation of the cast.</param>
        /// <param name="direction">The direction of the cast.</param>
        /// <returns>The number of hit results.</returns>
        protected virtual bool SingleCast(Vector3 position, Quaternion rotation, Vector3 direction)
        {
            var hit = false;
            if (m_Collider is SphereCollider) {
                var sphereCollider = m_Collider as SphereCollider;
                hit = Physics.SphereCast(position, sphereCollider.radius * MathUtility.ColliderRadiusMultiplier(sphereCollider), direction.normalized, out m_RaycastHit, direction.magnitude + c_ColliderSpacing, m_ImpactLayers, QueryTriggerInteraction.Ignore);
            } else if (m_Collider is CapsuleCollider) {
                var capsuleCollider = m_Collider as CapsuleCollider;
                Vector3 startEndCap, endEndCap;
                MathUtility.CapsuleColliderEndCaps(capsuleCollider, position, rotation, out startEndCap, out endEndCap);
                var radius = capsuleCollider.radius * MathUtility.ColliderRadiusMultiplier(capsuleCollider);
                hit = Physics.CapsuleCast(startEndCap, endEndCap, radius, direction.normalized, out m_RaycastHit, direction.magnitude + c_ColliderSpacing, m_ImpactLayers, QueryTriggerInteraction.Ignore);
            } else if (m_Collider is BoxCollider) {
                var boxCollider = m_Collider as BoxCollider;
                hit = Physics.BoxCast(m_Transform.TransformPoint(boxCollider.center), boxCollider.size / 4, direction.normalized, out m_RaycastHit, m_Transform.rotation, direction.magnitude + c_ColliderSpacing, m_ImpactLayers, QueryTriggerInteraction.Ignore);
            } else { // No collider attached.
                hit = Physics.Raycast(position, direction.normalized, out m_RaycastHit, direction.magnitude + c_ColliderSpacing, m_ImpactLayers, QueryTriggerInteraction.Ignore);
            }

            // The object should not collide with the originator to prevent the character from hitting themself.
            if (m_OriginatorCollisionCheck && m_OriginatorTransform != null) {
                if (hit && (m_RaycastHit.transform.IsChildOf(m_OriginatorTransform)
#if FIRST_PERSON_CONTROLLER
                    // The object should not hit any colliders who are a child of the camera.
                    || m_RaycastHit.transform.gameObject.GetCachedParentComponent<FirstPersonController.Character.FirstPersonObjects>() != null
#endif
                    )) {
                    hit = false;
                } else {
                    m_OriginatorCollisionCheck = false;
                }
            }

            return hit;
        }

        /// <summary>
        /// Rotate the object based on the current torque.
        /// </summary>
        /// <param name="position">The current position of the object.</param>
        /// <param name="rotation">The current rotation of the object. Passed by reference so the updated rotation can be set.</param>
        private void Rotate(Vector3 position, ref Quaternion rotation)
        {
            // The object should rotate to the desired direction after it has bounced and the rotation has settled.
            if ((m_CollisionMode == CollisionMode.Collide || m_Collided) && (m_Torque.sqrMagnitude < m_SettleThreshold || m_MovementSettled)) {
                if (m_Collider is CapsuleCollider || m_Collider is BoxCollider) {
                    if (!m_RotationSettled) {
                        var up = -m_NormalizedGravity;
                        var normal = up;
                        if (SingleCast(position, rotation, m_NormalizedGravity * c_ColliderSpacing)) {
                            normal = m_RaycastHit.normal;
                        }
                        var dot = Mathf.Abs(Vector3.Dot(normal, rotation * Vector3.up));
                        if (dot > 0.0001 && dot < 0.9999) {
                            // Allow the object to be force rotated to a rotation based on the sideways settle threshold. This works well with bullet
                            // shells to allow them to settle upright instead of always settling on their side.
                            var localRotation = MathUtility.InverseTransformQuaternion(Quaternion.LookRotation(Vector3.forward, up), rotation).eulerAngles;
                            if (!m_DeterminedRotation) {
                                m_SettleSideways = dot < m_SidewaysSettleThreshold;
                                m_DeterminedRotation = true;
                            }
                            localRotation.x = 0;
                            if (m_SettleSideways) { // The collider should settle on its side.
                                localRotation.z = Mathf.Abs(MathUtility.ClampInnerAngle(localRotation.z)) < 90 ? 0 : 180;
                            } else { // The collider should settle upright.
                                localRotation.z = MathUtility.ClampInnerAngle(localRotation.z) < 0 ? 270 : 90;
                            }
                            var target = MathUtility.TransformQuaternion(Quaternion.LookRotation(Vector3.forward, up), Quaternion.Euler(localRotation));
                            var deltaTime = m_TimeScale * Time.fixedDeltaTime * Time.timeScale;
                            rotation = Quaternion.Slerp(rotation, target, m_RotationSpeed * deltaTime);
                        } else {
                            // The object has finished rotating.
                            m_Torque = Vector3.zero;
                            m_RotationSettled = true;
                        }
                    }
                } else {
                    m_Torque = Vector3.zero;
                    m_RotationSettled = true;
                }
            }

            // Determine the new rotation.
            if (m_RotateInMoveDirection && m_Velocity.sqrMagnitude > 0) {
                rotation = Quaternion.LookRotation(m_Velocity.normalized, -m_Gravity);
            }
            m_Torque *= Mathf.Clamp01(1 - m_RotationDamping);
            var targetRotation = rotation * Quaternion.Euler(m_Torque);

            // Do not rotate if the collider would intersect with another object. A SphereCollider does not need this check.
            var hitCount = 0;
            if (m_Collider is CapsuleCollider) {
                Vector3 startEndCap, endEndCap;
                var capsuleCollider = m_Collider as CapsuleCollider;
                MathUtility.CapsuleColliderEndCaps(capsuleCollider, position, targetRotation, out startEndCap, out endEndCap);
                hitCount = Physics.OverlapCapsuleNonAlloc(startEndCap, endEndCap, capsuleCollider.radius * MathUtility.CapsuleColliderHeightMultiplier(capsuleCollider), m_ColliderHit, m_ImpactLayers, QueryTriggerInteraction.Ignore);
            } else if (m_Collider is BoxCollider) {
                var boxCollider = m_Collider as BoxCollider;
                hitCount = Physics.OverlapBoxNonAlloc(MathUtility.TransformPoint(position, m_Transform.rotation, boxCollider.center), Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale) / 2, m_ColliderHit, m_Transform.rotation, m_ImpactLayers, QueryTriggerInteraction.Ignore);
            }

            // Apply the rotation if the rotation doesnt intersect any object.
            if (hitCount == 0) {
                rotation = targetRotation;
            }
        }

        /// <summary>
        /// Stops the projectile from moving.
        /// </summary>
        protected void Stop()
        {
            m_Velocity = m_Torque = Vector3.zero;
            m_MovementSettled = m_RotationSettled = true;
        }

        /// <summary>
        /// Add the torque value to the object.
        /// </summary>
        /// <param name="torque">The amount of torque to add.</param>
        protected void AddTorque(Vector3 torque)
        {
            m_Torque += torque;
        }

        /// <summary>
        /// Adds a force to the object.
        /// </summary>
        /// <param name="force">The force to add to the object.</param>
        /// <param name="frames">The number of frames to add the force to. This is not used by the TrajectoryObject.</param>
        public void AddForce(Vector3 force, int frames)
        {
            AddForce(force);
        }

        /// <summary>
        /// Adds a force to the object.
        /// </summary>
        /// <param name="force">The force to add to the object.</param>
        public void AddForce(Vector3 force)
        {
            m_Velocity += (force / m_Mass) * m_ForceMultiplier;
            if (m_MovementSettled) {
                m_MovementSettled = false;
            }
        }

        /// <summary>
        /// The character's local timescale has changed.
        /// </summary>
        /// <param name="timeScale">The new timescale.</param>
        private void OnChangeTimeScale(float timeScale)
        {
            m_TimeScale = timeScale;
        }

        /// <summary>
        /// The component has been disabled.
        /// </summary>
        protected virtual void OnDisable()
        {
            if (m_Originator != null) {
                EventHandler.UnregisterEvent<float>(m_Originator, "OnCharacterChangeTimeScale", OnChangeTimeScale);
                m_Originator = null;
            }
        }
    }
}