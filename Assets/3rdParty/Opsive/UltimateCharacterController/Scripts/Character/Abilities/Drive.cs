/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Objects.CharacterAssist;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Ability that uses the IDriveSource interface to drive a vehicle.
    /// </summary>
    [DefaultInputName("Action")]
    [DefaultState("Drive")]
    [DefaultStartType(AbilityStartType.ButtonDown)]
    [DefaultStopType(AbilityStopType.ButtonToggle)]
    [DefaultAllowPositionalInput(false)]
    [DefaultAllowRotationalInput(false)]
    [DefaultUseRootMotionPosition(AbilityBoolOverride.True)]
    [DefaultUseRootMotionRotation(AbilityBoolOverride.True)]
    [DefaultUseGravity(AbilityBoolOverride.False)]
    [DefaultDetectHorizontalCollisions(AbilityBoolOverride.False)]
    [DefaultDetectVerticalCollisions(AbilityBoolOverride.False)]
    [DefaultAbilityIndex(14)]
    [DefaultEquippedSlots(0)]
    public class Drive : DetectObjectAbilityBase
    {
        [Tooltip("Should the character teleport for the enter and exit animations?")]
        [SerializeField] protected bool m_TeleportEnterExit;
        [Tooltip("Can the Drive ability aim?")]
        [SerializeField] protected bool m_CanAim;
        [Tooltip("The speed at which the character moves towards the seat location.")]
        [SerializeField] protected float m_MoveSpeed = 0.2f;
        [Tooltip("The speed at which the character rotates towards the seat location.")]
        [SerializeField] protected float m_RotationSpeed = 2f;

        public bool TeleportEnterExit { get => m_TeleportEnterExit; set => m_TeleportEnterExit = value; }
        public bool CanAim { get => m_CanAim; set => m_CanAim = value; }
        public float MoveSpeed { get => m_MoveSpeed; set => m_MoveSpeed = value; }
        public float RotationSpeed { get => m_RotationSpeed; set => m_RotationSpeed = value; }

        /// <summary>
        /// Specifies the current status of the character.
        /// </summary>
        private enum DriveState
        {
            Enter,          // The character is entering the vehicle.
            Drive,          // The character is driving the vehicle.
            Exit,           // The character is exiting the vehicle.
            ExitComplete    // The character has exited the vehicle.    
        }

        private IDriveSource m_DriveSource;
        private Transform m_OriginalParent;
        private Collider[] m_VehicleColliders;

        private DriveState m_DriveState;
        private Collider[] m_OverlapColliders;
        private KinematicObjectManager.UpdateLocation m_StartUpdateLocation;
        private float m_Epsilon = 0.99999f;

        public override int AbilityIntData { get { return m_DriveSource.AnimatorID + (int)m_DriveState; } }
        public override float AbilityFloatData { get { return m_CharacterLocomotion.RawInputVector.x; } }

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_OverlapColliders = new Collider[1];

            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorEnteredVehicle", OnEnteredVehicle);
            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorExitedVehicle", OnExitedVehicle);
        }

        /// <summary>
        /// Validates the object to ensure it is valid for the current ability.
        /// </summary>
        /// <param name="obj">The object being validated.</param>
        /// <param name="raycastHit">The raycast hit of the detected object. Will be null for trigger detections.</param>
        /// <returns>True if the object is valid. The object may not be valid if it doesn't have an ability-specific component attached.</returns>
        protected override bool ValidateObject(GameObject obj, RaycastHit? raycastHit)
        {
            if (!base.ValidateObject(obj, raycastHit)) {
                return false;
            }

            m_DriveSource = obj.GetCachedParentComponent<IDriveSource>();
            if (m_DriveSource == null) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            if (!base.CanStartAbility()) {
                return false;
            }

            return GetValidStartLocation(true) != null;
        }

        /// <summary>
        /// Returns a valid start location.
        /// </summary>
        /// <param name="groundCheck">Should the ground be checked at the start location?</param>
        /// <returns>A valid start location (can be null).</returns>
        private MoveTowardsLocation GetValidStartLocation(bool groundCheck)
        {
            // At least one ability start location must be on the ground and not obstructed by any object.
            var startLocations = m_DriveSource.GameObject.GetComponentsInChildren<MoveTowardsLocation>();
            for (int i = 0; i < startLocations.Length; ++i) {
                // The object must be on the ground.
                if (groundCheck && !Physics.Raycast(startLocations[i].transform.TransformPoint(0, 0.1f, 0), -startLocations[i].transform.up, 0.2f,
                                    m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore)) {
                    continue;
                }

                // If the start location has a collider then it should be clear of any other objects.
                var collider = startLocations[i].gameObject.GetCachedComponent<Collider>();
                if (collider == null || !ColliderOverlap(collider)) {
                    return startLocations[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Is the collider overlapping with any other objects?
        /// </summary>
        /// <param name="dismountCollider">The collider to determine if it is overlapping with another object.</param>
        /// <returns>True if the collider is overlapping.</returns>
        private bool ColliderOverlap(Collider collider)
        {
            if (collider == null) {
                return true;
            }

            int hitCount;
            if (collider is CapsuleCollider) {
                Vector3 startEndCap, endEndCap;
                var capsuleCollider = collider as CapsuleCollider;
                MathUtility.CapsuleColliderEndCaps(capsuleCollider, collider.transform.TransformPoint(capsuleCollider.center), collider.transform.rotation, out startEndCap, out endEndCap);
                hitCount = Physics.OverlapCapsuleNonAlloc(startEndCap, endEndCap, capsuleCollider.radius * MathUtility.ColliderRadiusMultiplier(capsuleCollider), m_OverlapColliders,
                                m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore);
            } else if (collider is BoxCollider) {
                var boxCollider = collider as BoxCollider;
                hitCount = Physics.OverlapBoxNonAlloc(collider.transform.TransformPoint(boxCollider.center), Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale) / 2,
                                    m_OverlapColliders, collider.transform.rotation, m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore);
            } else { // SphereCollider.
                var sphereCollider = collider as SphereCollider;
                hitCount = Physics.OverlapSphereNonAlloc(collider.transform.TransformPoint(sphereCollider.center), sphereCollider.radius * MathUtility.ColliderRadiusMultiplier(sphereCollider),
                                        m_OverlapColliders, m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore);
            }

            // Any overlap occurs anytime there is more one collider intersecting the colliders.
            return hitCount > 0;
        }

        /// <summary>
        /// Returns the possible MoveTowardsLocations that the character can move towards.
        /// </summary>
        /// <returns>The possible MoveTowardsLocations that the character can move towards.</returns>
        public override MoveTowardsLocation[] GetMoveTowardsLocations()
        {
            if (m_TeleportEnterExit) {
                return null;
            }
            return m_DriveSource.GameObject.GetComponentsInChildren<MoveTowardsLocation>();
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            m_OriginalParent = m_Transform.parent;
            m_VehicleColliders = m_DriveSource.GameObject.GetComponentsInChildren<Collider>();
            for (int i = 0; i < m_VehicleColliders.Length; ++i) {
                for (int j = 0; j < m_CharacterLocomotion.ColliderCount; ++j) {
                    Physics.IgnoreCollision(m_VehicleColliders[i], m_CharacterLocomotion.Colliders[j], true);
                }
            }
            m_CharacterLocomotion.AddIgnoredColliders(m_VehicleColliders);
            m_CharacterLocomotion.AlignToGravity = true;
            m_StartUpdateLocation = m_CharacterLocomotion.UpdateLocation;
            // Used FixedUpdate so the root motion location is accurate when getting into the vehicle.
            m_CharacterLocomotion.UpdateLocation = KinematicObjectManager.UpdateLocation.FixedUpdate;

            m_CharacterLocomotion.SetPlatform(m_DriveSource.Transform);
            m_Transform.parent = m_DriveSource.Transform;

            m_DriveState = DriveState.Enter;
            m_DriveSource.EnterVehicle(m_GameObject);

            // Teleport the character if there are no enter/exit animations.
            if (m_TeleportEnterExit) {
                OnEnteredVehicle();
                m_CharacterLocomotion.InputVector = Vector2.zero;
                m_CharacterLocomotion.SetPositionAndRotation(m_DriveSource.DriverLocation.position, m_DriveSource.DriverLocation.rotation, true, false);
            }
        }

        /// <summary>
        /// Callback when the character has entered the vehicle.
        /// </summary>
        private void OnEnteredVehicle()
        {
            m_DriveSource.EnteredVehicle(m_GameObject);
            m_DriveState = DriveState.Drive;
            m_CharacterLocomotion.ForceRootMotionRotation = false;
            m_CharacterLocomotion.ForceRootMotionPosition = false;
            m_CharacterLocomotion.AllowRootMotionRotation = false;
            m_CharacterLocomotion.AllowRootMotionPosition = false;
            m_CharacterLocomotion.UpdateLocation = m_StartUpdateLocation;
            m_CharacterLocomotion.UpdateAbilityAnimatorParameters();
        }

        /// <summary>
        /// Called when another ability is attempting to start and the current ability is active.
        /// Returns true or false depending on if the new ability should be blocked from starting.
        /// </summary>
        /// <param name="startingAbility">The ability that is starting.</param>
        /// <returns>True if the ability should be blocked.</returns>
        public override bool ShouldBlockAbilityStart(Ability startingAbility)
        {
            return m_AllowEquippedSlotsMask == 0 && startingAbility is Items.ItemAbility || (!m_CanAim && startingAbility is Items.Aim) || startingAbility is HeightChange;
        }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        /// <returns>True if the ability should be stopped.</returns>
        public override bool ShouldStopActiveAbility(Ability activeAbility)
        {
            return m_AllowEquippedSlotsMask == 0 && activeAbility is Items.ItemAbility || (!m_CanAim && activeAbility is Items.Aim);
        }

        /// <summary>
        /// Updates the ability.
        /// </summary>
        public override void Update()
        {
            // Try to stop the ability after the character has exited. The ability won't be able to be stopped if the character isn't level with the gravity direction.
            if (m_DriveState == DriveState.ExitComplete && !m_TeleportEnterExit) {
                StopAbility();
            }
        }

        /// <summary>
        /// Update the ability's Animator parameters.
        /// </summary>
        public override void UpdateAnimator()
        {
            // The horizontal input value can be used to animate the steering wheel.
            SetAbilityFloatDataParameter(m_CharacterLocomotion.RawInputVector.x, Time.deltaTime);
        }

        /// <summary>
        /// Update the controller's rotation values.
        /// </summary>
        public override void UpdateRotation()
        {
            var deltaRotation = Quaternion.identity;
            var rotation = m_Transform.rotation;
            if (m_DriveState != DriveState.Drive) {
                if (m_TeleportEnterExit) {
                    return;
                }
                var upNormal = m_DriveState == DriveState.Enter ? m_DriveSource.Transform.up : -m_CharacterLocomotion.GravityDirection;
                // When the character is entering the vehicle they should rotate to face the same up direction as the car. This allows the character to enter while on slopes.
                // Similarly, when the character exits they should rotate to the gravity direction.
                var proj = (rotation * Vector3.forward) - Vector3.Dot(rotation * Vector3.forward, upNormal) * upNormal;
                if (proj.sqrMagnitude > 0.0001f) {
                    var speed = m_RotationSpeed * m_CharacterLocomotion.TimeScale * Time.timeScale * Time.deltaTime * (m_DriveState == DriveState.ExitComplete ? 100 : 1);
                    var targetRotation = Quaternion.Slerp(rotation, Quaternion.LookRotation(proj, upNormal), speed);
                    deltaRotation = deltaRotation * (Quaternion.Inverse(rotation) * targetRotation);
                }
            } else if (m_DriveSource.DriverLocation != null) {
                // The character should fully rotate towards the target rotation after they have entered.
                deltaRotation = MathUtility.InverseTransformQuaternion(m_Transform.rotation, m_DriveSource.DriverLocation.rotation);
            }
            m_CharacterLocomotion.DeltaRotation = deltaRotation.eulerAngles;
        }

        /// <summary>
        /// Update the controller's position values.
        /// </summary>
        public override void UpdatePosition()
        {
            if (m_DriveState != DriveState.Drive || m_TeleportEnterExit || m_DriveSource.DriverLocation == null) {
                return;
            }

            m_CharacterLocomotion.MotorThrottle = Vector3.zero;
            var deltaPosition = Vector3.MoveTowards(m_Transform.position, m_DriveSource.DriverLocation.position, m_MoveSpeed) - m_Transform.position;
            m_CharacterLocomotion.AbilityMotor = deltaPosition / (m_CharacterLocomotion.TimeScaleSquared * Time.timeScale * TimeUtility.FramerateDeltaTime);
        }

        /// <summary>
        /// Callback when the ability tries to be stopped. Start the dismount.
        /// </summary>
        public override void WillTryStopAbility()
        {
            if (m_DriveState != DriveState.Drive) {
                return;
            }

            // The ability can't stop if there are no valid exit locations.
            MoveTowardsLocation startLocation;
            if ((startLocation = GetValidStartLocation(false)) == null) {
                return;
            }

            m_DriveSource.ExitVehicle(m_GameObject);
            m_DriveState = DriveState.Exit;
            m_CharacterLocomotion.AbilityMotor = Vector3.zero;
            m_CharacterLocomotion.ForceRootMotionRotation = true;
            m_CharacterLocomotion.ForceRootMotionPosition = true;
            m_CharacterLocomotion.AllowRootMotionRotation = true;
            m_CharacterLocomotion.AllowRootMotionPosition = true;
            m_CharacterLocomotion.UpdateAbilityAnimatorParameters();

            // Teleport the character if there are no enter/exit animations.
            if (m_TeleportEnterExit) {
                OnExitedVehicle();
                var forward = Vector3.ProjectOnPlane(startLocation.transform.forward, -m_CharacterLocomotion.GravityDirection);
                m_CharacterLocomotion.SetPositionAndRotation(startLocation.transform.position, Quaternion.LookRotation(forward, -m_CharacterLocomotion.GravityDirection), true, false);
            } else {

            }
        }

        /// <summary>
        /// Callback when the character has exited the vehicle.
        /// </summary>
        private void OnExitedVehicle()
        {
            m_DriveSource.ExitedVehicle(m_GameObject);
            m_DriveState = DriveState.ExitComplete;
            m_CharacterLocomotion.UpdateAbilityAnimatorParameters();
            m_Transform.parent = m_OriginalParent;
            m_CharacterLocomotion.SetPlatform(null);
            m_CharacterLocomotion.AlignToGravity = false;
            m_CharacterLocomotion.ForceRootMotionRotation = false;
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility()
        {
            // The character has to be exited in order to stop.
            return m_DriveState == DriveState.ExitComplete && 
                                (m_TeleportEnterExit || Vector3.Dot(m_Transform.rotation * Vector3.up, -m_CharacterLocomotion.GravityDirection) >= m_Epsilon);
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            // If the drive state isn't exit complete then the ability was force stopped.
            if (m_DriveState != DriveState.ExitComplete) {
                m_DriveSource.ExitVehicle(m_GameObject);
                m_CharacterLocomotion.AbilityMotor = Vector3.zero;
                m_CharacterLocomotion.UpdateLocation = m_StartUpdateLocation;
                OnExitedVehicle();
            }

            m_CharacterLocomotion.RemoveIgnoredColliders(m_VehicleColliders);
            for (int i = 0; i < m_VehicleColliders.Length; ++i) {
                for (int j = 0; j < m_CharacterLocomotion.ColliderCount; ++j) {
                    Physics.IgnoreCollision(m_VehicleColliders[i], m_CharacterLocomotion.Colliders[j], false);
                }
            }
        }

        /// <summary>
        /// The character has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorEnteredVehicle", OnEnteredVehicle);
            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorExitedVehicle", OnExitedVehicle);
        }
    }
}