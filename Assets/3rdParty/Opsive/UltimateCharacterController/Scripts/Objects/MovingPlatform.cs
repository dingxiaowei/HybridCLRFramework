/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.StateSystem;
    using Opsive.UltimateCharacterController.Traits;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// The MovingPlatform component will move an object from one point to another. GameObjects with the Moving Platform component should be on the MovingPlatform layer.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class MovingPlatform : StateBehavior, IKinematicObject, IInteractableTarget
    {
        /// <summary>
        /// Represets a point that the platform can traverse.
        /// </summary>
        [System.Serializable]
        public struct Waypoint
        {
            [Tooltip("The transform the waypoint that the platform should traverse.")]
            [SerializeField] private Transform m_Transform;
            [Tooltip("The amount of time that the platform should stay at the current waypoint before moving to the next waypoint")]
            [SerializeField] private float m_Delay;
            [Tooltip("The state that should be triggered when the platform is moving towards it.")]
            [SerializeField] private string m_State;

            private int m_StateHash;

            public Transform Transform { get { return m_Transform; } set { m_Transform = value; } }
            public float Delay { get { return m_Delay; } set { m_Delay = value; } }
            public string State { get { return m_State; } set { m_State = value; } }
            public int StateHash { get { return m_StateHash; } }

            /// <summary>
            /// Initializes the state hash.
            /// </summary>
            public void Initialize()
            {
                m_StateHash = Serialization.StringHash(m_State);
            }
        }

        /// <summary>
        /// Specifies the direction that the platform should traverse.
        /// </summary>
        public enum PathDirection
        {
            Forward,    // Move waypoints from least to greatest.
            Backwards   // Move waypoints from greatest to least.
        }

        /// <summary>
        /// Specifies how the platform traverses through waypoints.
        /// </summary>
        public enum PathMovementType
        {
            PingPong,   // Moves to the last waypoint and then back the way it came from.
            Loop,       // Moves to the last waypoint and then directly to the first waypoint.
            Target      // Moves to the specified waypoint index.
        }

        /// <summary>
        /// Specifies how the platform should interpolate the movement speed.
        /// </summary>
        public enum MovementInterpolationMode
        {
            EaseInOut,  // Gently moves into full movement and then gently moves out of it at each waypoint.
            EaseIn,     // Gently moves into full movement.
            EaseOut,    // Moves into full movement immediately and gently moves out of full movement at each waypoint.
            EaseOut2,   // Moves into full movement immediately and moves out of full movement according to the movement speed.
            Slerp,      // Uses Vector3.Slerp to move in and out of movement according to the movement speed.
            Lerp        // Uses Vector3.Lerp to move in and out of movement according to the movement speed.
        }

        /// <summary>
        /// Specifies how the platform should interpolate the rotation speed.
        /// </summary>
        public enum RotateInterpolationMode
        {
            SyncToMovement, // Rotates according to the movement speed.
            EaseOut,        // Uses Quaternion.Lerp to lerp the rotation based on a linear curve.
            CustomEaseOut,  // Uses Quaternion.Lerp to lerp the rotation based on the RotationEaseAmount. 
            CustomRotate    // Rotates according to the rotation speed.
        }

        [Tooltip("Specifies the location that the object should be updated.")]
        [SerializeField] protected KinematicObjectManager.UpdateLocation m_UpdateLocation = KinematicObjectManager.UpdateLocation.FixedUpdate;
        [Tooltip("The waypoints to traverse.")]
        [SerializeField] protected Waypoint[] m_Waypoints;
        [Tooltip("Specifies the direction that the platform should traverse.")]
        [SerializeField] protected PathDirection m_Direction;
        [Tooltip("Specifies how the platform traverses through waypoints.")]
        [SerializeField] protected PathMovementType m_MovementType;
        [Tooltip("If using the Target PathMovementType, specifies the waypoint index to move towards.")]
        [SerializeField] protected int m_TargetWaypoint;
        [Tooltip("The speed at which the platform should move.")]
        [SerializeField] protected float m_MovementSpeed = 0.1f;
        [Tooltip("Specifies how the platform should interpolate the movement speed.")]
        [SerializeField] protected MovementInterpolationMode m_MovementInterpolation = MovementInterpolationMode.EaseInOut;
        [Tooltip("Specifies how the platform should interpolate the rotation speed.")]
        [SerializeField] protected RotateInterpolationMode m_RotationInterpolation;
        [Tooltip("If using the CustomEaseOut RotationInterpolationMode, specifies the amount to ease into the target rotation.")]
        [SerializeField] protected float m_RotationEaseAmount = 0.1f;
        [Tooltip("If using the CustomRotate RotationInterpolationMode, specifies the rotation speed.")]
        [SerializeField] protected Vector3 m_CustomRotationSpeed;
        [Tooltip("The maximum angle that the platform can rotation. Set to -1 to have no max angle.")]
        [SerializeField] protected float m_MaxRotationDeltaAngle = -1;
        [Tooltip("The state name that should activate when the character enters the platform trigger.")]
        [SerializeField] protected string m_CharacterTriggerState;
        [Tooltip("Should the platform be enabled when interacted with?")]
        [SerializeField] protected bool m_EnableOnInteract;
        [Tooltip("Should the directions be changed if the character interacts with the platform while it is moving?")]
        [SerializeField] protected bool m_ChangeDirectionsOnInteract = false;
#if UNITY_EDITOR
        [Tooltip("The color to draw the editor gizmo in (editor only).")]
        [SerializeField] protected Color m_GizmoColor = new Color(0, 0, 1, 0.3f);
        [Tooltip("Should the delay and distance labels be drawh t0 tye scene view (editor only)?")]
        [SerializeField] protected bool m_DrawDebugLabels;
#endif

        public KinematicObjectManager.UpdateLocation UpdateLocation { get { return m_UpdateLocation; } }
        [NonSerialized] public Waypoint[] Waypoints { get { return m_Waypoints; } set { m_Waypoints = value; } }
        [NonSerialized] public PathDirection Direction { get { return m_Direction; } set { m_Direction = value; } }
        public PathMovementType MovementType { get { return m_MovementType; } set { m_MovementType = value; } } 
        public int TargetWaypoint { get { return m_TargetWaypoint; } set { m_TargetWaypoint = value; } }
        public float MovementSpeed { get { return m_MovementSpeed; } set { m_MovementSpeed = value; } }
        public MovementInterpolationMode MovementInterpolation { get { return m_MovementInterpolation; } set { m_MovementInterpolation = value; } }
        public RotateInterpolationMode RotationInterpolation { get { return m_RotationInterpolation; } set { m_RotationInterpolation = value; } }
        public float RotationEaseAmount { get { return m_RotationEaseAmount; } set { m_RotationEaseAmount = value; } }
        public Vector3 CustomRotationSpeed { get { return m_CustomRotationSpeed; } set { m_CustomRotationSpeed = value; } }
        public string CharacterTriggerState { get { return m_CharacterTriggerState; } set { m_CharacterTriggerState = value; } }
        public bool EnableOnInteract { get { return m_EnableOnInteract; } set { m_EnableOnInteract = value; } }
        public bool ChangeDirectionsOnInteract { get { return m_ChangeDirectionsOnInteract; } set { m_ChangeDirectionsOnInteract = value; } }
#if UNITY_EDITOR
        [NonSerialized] public Color GizmoColor { get { return m_GizmoColor; } set { m_GizmoColor = value; } }
        [NonSerialized] public bool DrawDebugLabels { get { return m_DrawDebugLabels; } set { m_DrawDebugLabels = value; } }
#endif

        protected GameObject m_GameObject;
        protected Transform m_Transform;
        private Rigidbody m_Rigidbody;

        private int m_KinematicObjectIndex = -1;
        protected int m_NextWaypoint;
        protected int m_PreviousWaypoint;
        protected float m_NextWaypointDistance;
        protected Quaternion m_OriginalRotation;
        protected float m_MoveTime;
        protected Vector3 m_TargetPosition;
        protected Quaternion m_TargetRotation;
        private Vector3 m_MovePosition;
        private Quaternion m_MoveRotation;
        private AnimationCurve m_EaseInOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        private AnimationCurve m_LinearCurve = AnimationCurve.Linear(0, 0, 1, 1);
        protected ScheduledEventBase m_NextWaypointEvent;
        protected int m_ActiveCharacterCount;

        public int NextWaypoint { get { return m_NextWaypoint; } }
        public int KinematicObjectIndex { get { return m_KinematicObjectIndex; } set { m_KinematicObjectIndex = value; } }

        /// <summary>
        /// Cache the component references and initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

#if UNITY_EDITOR
            // Sanity check in the editor:
            for (int i = 0; i < m_Waypoints.Length; ++i) {
                if (m_Waypoints[i].Transform == null) {
                    Debug.LogError("Error: Moving Platform " + gameObject.name + " has a null waypoint. This platform will be disabled.");
                    enabled = false;
                    return;
                }
            }
#endif

            m_GameObject = gameObject;
            m_Transform = transform;
            m_MovePosition = m_Transform.position;
            m_MoveRotation = m_Transform.rotation;

            // The Rigidbody is only used to notify Unity that the character isn't static and for collision events. The Rigidbody doesn't control any movement.
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            m_Rigidbody.isKinematic = true;
            m_Rigidbody.constraints = RigidbodyConstraints.FreezeAll;

            // The GameObject must be on the MovingPlatform layer.
            if (m_GameObject.layer != LayerManager.MovingPlatform) {
                Debug.LogWarning("Warning: " + m_GameObject.name + " is a moving platform not using the MovingPlatform layer. Please change this layer.");
                m_GameObject.layer = LayerManager.MovingPlatform;
            }

            // The platform can rotate without any waypoints.
            if (m_Waypoints.Length > 0) {
                for (int i = 0; i < m_Waypoints.Length; ++i) {
                    m_Waypoints[i].Initialize();
                }
                m_TargetRotation = m_OriginalRotation = m_Waypoints[m_NextWaypoint].Transform.rotation;
                m_TargetPosition = m_Waypoints[m_NextWaypoint].Transform.position;

                if (!string.IsNullOrEmpty(m_Waypoints[m_NextWaypoint].State)) {
                    StateManager.SetState(m_GameObject, m_Waypoints[m_NextWaypoint].State, true);
                }
                m_PreviousWaypoint = m_NextWaypoint;
            }

            // Start disabled until the platform is interacted with.
            if (m_EnableOnInteract) {
                enabled = false;
            }
        }

        /// <summary>
        /// Registers the object with the KinematicObjectManager.
        /// </summary>
        protected virtual void OnEnable()
        {
            m_KinematicObjectIndex = KinematicObjectManager.RegisterKinematicObject(this);
        }

        /// <summary>
        /// Update the platform movement and rotation.
        /// </summary>
        public void Move()
        {
            // Updates the path to the next waypoint if necessary.
            UpdatePath();

            // Rotate along the path.
            UpdateRotation();

            // Applies the rotation.
            ApplyRotation();

            // No more updates are necessary if the platform is waiting at the current waypoint.
            if (m_NextWaypointEvent != null || m_Waypoints.Length == 0) {
                return;
            }

            // Progress towards the waypoint.
            UpdatePosition();

            // Applies the position.
            ApplyPosition();
        }

        /// <summary>
        /// If the platform has arrived at the current waypoint then the next waypoint should be determined.
        /// </summary>
        private void UpdatePath()
        {
            if (GetRemainingDistance() < 0.01f && m_NextWaypointEvent == null && (m_MovementType != PathMovementType.Target || m_NextWaypoint != m_TargetWaypoint)) {
                m_NextWaypointEvent = Scheduler.ScheduleFixed(m_Waypoints[m_NextWaypoint].Delay, UpdateWaypoint);
            }
        }

        /// <summary>
        /// Updates the moving platform to move to the next waypoint.
        /// </summary>
        protected void UpdateWaypoint()
        {
            // The state should always reflect the state of the next waypoint. If moving in reverse then the state has to be updated before the index changes.
            if (m_Direction == PathDirection.Backwards) {
                UpdateState();
            }
            m_PreviousWaypoint = m_NextWaypoint;

            switch (m_MovementType) {
                case PathMovementType.Target:
                    if (m_NextWaypoint != m_TargetWaypoint) {
                        GoToNextWaypoint();
                    }
                    break;
                case PathMovementType.Loop:
                    GoToNextWaypoint();
                    break;
                case PathMovementType.PingPong:
                    if (m_Direction == PathDirection.Backwards) {
                        if (m_NextWaypoint == 0) {
                            m_Direction = PathDirection.Forward;
                        }
                    } else {
                        if (m_NextWaypoint == (m_Waypoints.Length - 1)) {
                            m_Direction = PathDirection.Backwards;
                        }
                    }
                    GoToNextWaypoint();
                    break;
            }
            // The state should always reflect the state of the next waypoint. If moving in reverse then the state has to be updated before the index changes.
            if (m_Direction == PathDirection.Forward) {
                UpdateState();
            }
            m_NextWaypointEvent = null;
        }

        /// <summary>
        /// Disables the state at the old index and enables the state at the new index.
        /// </summary>
        private void UpdateState()
        {
            if (m_Waypoints[m_PreviousWaypoint].StateHash != m_Waypoints[m_NextWaypoint].StateHash) {
                // The previous state should be disabled.
                if (m_Waypoints[m_PreviousWaypoint].StateHash != 0) {
                    StateManager.SetState(m_GameObject, m_Waypoints[m_PreviousWaypoint].State, false);
                }
                if (m_Waypoints[m_NextWaypoint].StateHash != 0) {
                    StateManager.SetState(m_GameObject, m_Waypoints[m_NextWaypoint].State, true);
                }
            }
        }

        /// <summary>
        /// Returns the distance to the next waypoint.
        /// </summary>
        /// <returns>The distance to the next waypoint.</returns>
        protected float GetRemainingDistance()
        {
            if (m_Waypoints.Length == 0) {
                return float.MaxValue;
            }
            return Vector3.Distance(m_Transform.position, m_Waypoints[m_NextWaypoint].Transform.position);
        }

        /// <summary>
        /// Determines the next waypoint.
        /// </summary>
        private void GoToNextWaypoint()
        {
            // The next waypoint is based on the path direction.
            switch (m_Direction) {
                case PathDirection.Forward:
                    m_NextWaypoint = GetNextWaypoint(true);
                    break;
                case PathDirection.Backwards:
                    m_NextWaypoint = GetNextWaypoint(false);
                    break;
            }

            // Update the path related variables.
            m_MoveTime = 0;
            m_OriginalRotation = m_TargetRotation;
            m_TargetPosition = m_Waypoints[m_NextWaypoint].Transform.position;
            m_TargetRotation = m_Waypoints[m_NextWaypoint].Transform.rotation;
            m_NextWaypointDistance = GetRemainingDistance();
        }

        /// <summary>
        /// Returns the next waypoint index.
        /// </summary>
        /// <param name="increase">Should the waypoint index be inceased? If false it'll be decreased.</param>
        /// <returns>The next waypoint index.</returns>
        private int GetNextWaypoint(bool increase)
        {
            m_NextWaypoint = (m_NextWaypoint + (increase ? 1 : -1)) % m_Waypoints.Length;
            if (m_NextWaypoint < 0) {
                m_NextWaypoint = 0;
            }
            return m_NextWaypoint;
        }

        /// <summary>
        /// Updates platform angle according to the current rotation interpolation mode.
        /// </summary>
        private void UpdateRotation()
        {
            switch (m_RotationInterpolation) {
                case RotateInterpolationMode.SyncToMovement:
                    if (m_NextWaypointEvent == null) {
                        m_MoveRotation = Quaternion.Lerp(m_OriginalRotation, m_TargetRotation, 1.0f - (GetRemainingDistance() / m_NextWaypointDistance));
                    }
                    break;
                case RotateInterpolationMode.EaseOut:
                    m_MoveRotation = Quaternion.Lerp(m_Transform.rotation, m_TargetRotation, m_LinearCurve.Evaluate(m_MoveTime));
                    break;
                case RotateInterpolationMode.CustomEaseOut:
                    m_MoveRotation = Quaternion.Lerp(m_Transform.rotation, m_TargetRotation, m_RotationEaseAmount);
                    break;
                case RotateInterpolationMode.CustomRotate:
                    m_MoveRotation = m_Transform.rotation * Quaternion.Euler(m_CustomRotationSpeed);
                    break;
            }
            if (m_MaxRotationDeltaAngle != -1) {
                m_MoveRotation = Quaternion.RotateTowards(m_Transform.rotation, m_MoveRotation, m_MaxRotationDeltaAngle);
            }
        }

        /// <summary>
        /// Applies the rotational movement to the Transform.
        /// </summary>
        private void ApplyRotation()
        {
            m_Transform.rotation = m_MoveRotation;
        }

        /// <summary>
        /// Updates platform position according to the current movement interpolation mode.
        /// </summary>
        private void UpdatePosition()
        {
            switch (m_MovementInterpolation)
            {
                case MovementInterpolationMode.EaseInOut:
                    m_MovePosition = Vector3.Lerp(m_Transform.position, m_TargetPosition, m_EaseInOutCurve.Evaluate(m_MoveTime));
                    break;
                case MovementInterpolationMode.EaseIn:
                    m_MovePosition = Vector3.MoveTowards(m_Transform.position, m_TargetPosition, m_MoveTime);
                    break;
                case MovementInterpolationMode.EaseOut:
                    m_MovePosition = Vector3.Lerp(m_Transform.position, m_TargetPosition, m_LinearCurve.Evaluate(m_MoveTime));
                    break;
                case MovementInterpolationMode.EaseOut2:
                    m_MovePosition = Vector3.Lerp(m_Transform.position, m_TargetPosition, m_MovementSpeed * 0.25f);
                    break;
                case MovementInterpolationMode.Slerp:
                    m_MovePosition = Vector3.Slerp(m_Transform.position, m_TargetPosition, m_LinearCurve.Evaluate(m_MoveTime));
                    break;
                case MovementInterpolationMode.Lerp:
                    m_MovePosition = Vector3.MoveTowards(m_Transform.position, m_TargetPosition, m_MovementSpeed);
                    break;
            }
        }

        /// <summary>
        /// Applies the positional movement to the Transform.
        /// </summary>
        private void ApplyPosition()
        {
            m_Transform.position = m_MovePosition;

            // Progress the move time and also store the updated metrics.
            m_MoveTime += m_MovementSpeed * 0.01f * Time.deltaTime;
        }

        /// <summary>
        /// Can the target be interacted with?
        /// </summary>
        /// <param name="character">The character that wants to interactact with the target.</param>
        /// <returns>True if the target can be interacted with.</returns>
        public bool CanInteract(GameObject character)
        {
            return true;
        }

        /// <summary>
        /// Interact with the target.
        /// </summary>
        /// <param name="character">The character that wants to interactact with the target.</param>
        public void Interact(GameObject character)
        {
            if (m_EnableOnInteract) {
                enabled = true;
            } else if (m_ChangeDirectionsOnInteract) {
                // If the platform is already moving and is interacted with then it should change directions.
                m_Direction = m_Direction == PathDirection.Forward ? PathDirection.Backwards : PathDirection.Forward;
            }
        }

        /// <summary>
        /// An object has entered the trigger.
        /// </summary>
        /// <param name="other">The object that entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            // Characters will have a CharacterLayerManager.
            var layerManager = other.gameObject.GetCachedParentComponent<CharacterLayerManager>();
            if (layerManager == null) {
                return;
            }

            if (!MathUtility.InLayerMask(other.gameObject.layer, layerManager.CharacterLayer)) {
                return;
            }

            m_ActiveCharacterCount++;

            // The platform can activate a state based on the character trigger state. Only the first character should activate the state
            // if multiple characters land on the platform.
            if (m_ActiveCharacterCount == 1 && !string.IsNullOrEmpty(m_CharacterTriggerState)) {
                StateManager.SetState(m_GameObject, m_CharacterTriggerState, true);
            }
        }

        /// <summary>
        /// An object has exited the trigger.
        /// </summary>
        /// <param name="other">The collider that exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            // No further checks need to be done if a character isn't on the platform.
            if (m_ActiveCharacterCount == 0) {
                return;
            }

            // Characters will have a CharacterLayerManager.
            var layerManager = other.gameObject.GetCachedParentComponent<CharacterLayerManager>();
            if (layerManager == null) {
                return;
            }

            if (!MathUtility.InLayerMask(other.gameObject.layer, layerManager.CharacterLayer)) {
                return;
            }

            m_ActiveCharacterCount--;

            if (m_ActiveCharacterCount == 0 && !string.IsNullOrEmpty(m_CharacterTriggerState)) {
                StateManager.SetState(m_GameObject, m_CharacterTriggerState, false);
            }
        }

        /// <summary>
        /// Unregisters the object with the KinematicObjectManager.
        /// </summary>
        protected virtual void OnDisable()
        {
            KinematicObjectManager.UnregisterKinematicObject(m_KinematicObjectIndex);
        }
    }
}