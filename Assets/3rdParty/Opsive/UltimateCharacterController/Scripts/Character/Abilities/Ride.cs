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
    /// The Ride ability allows the character to ride another Ultimate Character Locomotion character (such as a horse).
    /// </summary>
    [DefaultStartType(AbilityStartType.ButtonDown)]
    [DefaultStopType(AbilityStopType.ButtonToggle)]
    [DefaultInputName("Action")]
    [DefaultAbilityIndex(12)]
    [DefaultAllowRotationalInput(false)]
    [DefaultUseGravity(AbilityBoolOverride.False)]
    [DefaultDetectHorizontalCollisions(AbilityBoolOverride.False)]
    [DefaultDetectVerticalCollisions(AbilityBoolOverride.False)]
    [DefaultUseRootMotionPosition(AbilityBoolOverride.True)]
    [DefaultUseRootMotionRotation(AbilityBoolOverride.True)]
    [DefaultEquippedSlots(0)]
    [AllowDuplicateTypes]
    public class Ride : DetectObjectAbilityBase, Items.IItemToggledReceiver
    {
        /// <summary>
        /// Specifies the current status of the character.
        /// </summary>
        private enum RideState
        {
            Mount,              // The character is mounting the object.
            Ride,               // The character is riding the object.
            WaitForItemUnequip, // The character is waiting for the item to be unequipped so it can then start to dismount.
            Dismount,           // The character is dismounting from the object.
            DismountComplete    // The character is no longer on the rideable object.
        }

        [Tooltip("Specifies if the ability should wait for the OnAnimatorMount animation event or wait for the specified duration before mounting to the rideable object.")]
        [SerializeField] protected AnimationEventTrigger m_MountEvent;
        [Tooltip("The speed at which the character moves towards the ride location.")]
        [SerializeField] protected float m_MoveSpeed = 0.2f;
        [Tooltip("The speed at which the character rotates towards ride location.")]
        [SerializeField] protected float m_RotationSpeed = 2f;
        [Tooltip("Specifies if the ability should wait for the OnAnimatorDismount animation event or wait for the specified duration before dismounting from the rideable object.")]
        [SerializeField] protected AnimationEventTrigger m_DismountEvent;
        [Tooltip("After the character mounts should the ability reequip the item that the character had before mounting?")]
        [SerializeField] protected bool m_ReequipItemAfterMount = true;

        public AnimationEventTrigger MountEvent { get { return m_MountEvent; } set { m_MountEvent = value; } }
        public float MoveSpeed { get { return m_MoveSpeed; } set { m_MoveSpeed = value; } }
        public float RotationSpeed { get { return m_RotationSpeed; } set { m_RotationSpeed = value; } }
        public AnimationEventTrigger DismountEvent { get { return m_DismountEvent; } set { m_DismountEvent = value; } }
        public bool ReequipItemAfterMount { get { return m_ReequipItemAfterMount; } set { m_ReequipItemAfterMount = value; } }

        private Rideable m_Rideable;
        
        private bool m_LeftMount;
        private KinematicObjectManager.UpdateLocation m_StartUpdateLocation;
        private ScheduledEventBase m_MountDismountEvent;
        private RideState m_RideState = RideState.DismountComplete;
        private float m_Epsilon = 0.99999f;

        public override int AbilityIntData
        {
            get
            {
                if (m_RideState == RideState.Mount) {
                    return m_LeftMount ? 1 : 2;
                } else if (m_RideState == RideState.Ride) {
                    return 3;
                } else if (m_RideState == RideState.Dismount) {
                    return m_LeftMount ? 4 : 5;
                }

                return base.AbilityIntData;
            }
        }
        public UltimateCharacterLocomotion CharacterLocomotion { get { return m_CharacterLocomotion; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorRideMount", OnMount);
            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorRideDismount", OnDismount);
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

            var characterLocomotion = obj.GetCachedParentComponent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return false;
            }

            // A rideable object must be added to the character.
            var rideable = characterLocomotion.GetAbility<Rideable>();
            if (rideable == null) {
                return false;
            }

            m_Rideable = rideable;
            return true;
        }

        /// <summary>
        /// Returns the possible MoveTowardsLocations that the character can move towards.
        /// </summary>
        /// <returns>The possible MoveTowardsLocations that the character can move towards.</returns>
        public override MoveTowardsLocation[] GetMoveTowardsLocations()
        {
            return m_Rideable.GameObject.GetComponentsInChildren<MoveTowardsLocation>();
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            m_StartUpdateLocation = m_CharacterLocomotion.UpdateLocation;
            // Used FixedUpdate so the root motion location is accurate when getting on the Rideable object.
            m_CharacterLocomotion.UpdateLocation = KinematicObjectManager.UpdateLocation.FixedUpdate;

            m_LeftMount = m_Rideable.Transform.InverseTransformPoint(m_Transform.position).x < 0;
            m_Rideable.Mount(this);
            m_CharacterLocomotion.SetPlatform(m_Rideable.Transform);

            // The character will look independently of the rotation.
            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterForceIndependentLook", true);
            m_RideState = RideState.Mount;
            m_CharacterLocomotion.UpdateAbilityAnimatorParameters();

            // Update the rideable object's parameters as well so it can stay synchronized to the ride obejct.
            m_Rideable.CharacterLocomotion.UpdateAbilityAnimatorParameters();
            if (!m_MountEvent.WaitForAnimationEvent) {
                m_MountDismountEvent = Scheduler.Schedule(m_MountEvent.Duration, OnMount);
            }
        }

        /// <summary>
        /// Called when another ability is attempting to start and the current ability is active.
        /// Returns true or false depending on if the new ability should be blocked from starting.
        /// </summary>
        /// <param name="startingAbility">The ability that is starting.</param>
        /// <returns>True if the ability should be blocked.</returns>
        public override bool ShouldBlockAbilityStart(Ability startingAbility)
        {
#if THIRD_PERSON_CONTROLLER
            if (startingAbility is ThirdPersonController.Character.Abilities.Items.ItemPullback) {
                return true;
            }
#endif
#if ULTIMATE_CHARACTER_CONTROLLER_MELEE
            if (startingAbility is Items.InAirMeleeUse) {
                return true;
            }
#endif
            // The character cannot interact with any items while mounting/dismounting.
            if (m_RideState != RideState.Ride && startingAbility is Items.ItemAbility) {
                return true;
            }
            return startingAbility is HeightChange || base.ShouldBlockAbilityStart(startingAbility);
        }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        /// <returns>True if the ability should be stopped.</returns>
        public override bool ShouldStopActiveAbility(Ability activeAbility)
        {
#if THIRD_PERSON_CONTROLLER
            if (activeAbility is ThirdPersonController.Character.Abilities.Items.ItemPullback) {
                return true;
            }
#endif
            return base.ShouldStopActiveAbility(activeAbility);
        }

        /// <summary>
        /// Mounts the character on the object.
        /// </summary>
        private void OnMount()
        {
            m_RideState = RideState.Ride;
            m_CharacterLocomotion.ForceRootMotionPosition = false;
            m_CharacterLocomotion.AllowRootMotionPosition = false;
            m_CharacterLocomotion.UpdateLocation = m_StartUpdateLocation;
            m_CharacterLocomotion.UpdateAbilityAnimatorParameters();
            m_Rideable.OnCharacterMount();

            // The item was unequipped when mounting - it may need to be reequiped again.
            if (m_CharacterLocomotion.ItemEquipVerifierAbility != null) {
                if (m_ReequipItemAfterMount) {
                    m_CharacterLocomotion.ItemEquipVerifierAbility.TryToggleItem(this, false);
                } else {
                    m_CharacterLocomotion.ItemEquipVerifierAbility.Reset();
                }
            }
        }

        /// <summary>
        /// Updates the input vector.
        /// </summary>
        public override void Update()
        {
            if (m_RideState == RideState.Ride || m_RideState == RideState.WaitForItemUnequip) {
                // The input parameters should match the rideable object's input parameters.
                m_CharacterLocomotion.InputVector = m_Rideable.CharacterLocomotion.InputVector;
                m_CharacterLocomotion.DeltaRotation = m_Rideable.CharacterLocomotion.DeltaRotation;
            } else if (m_RideState == RideState.DismountComplete) {
                StopAbility();
            }
        }

        /// <summary>
        /// Update the controller's rotation values.
        /// </summary>
        public override void UpdateRotation()
        {
            var deltaRotation = Quaternion.identity;
            var rotation = m_Transform.rotation;
            if (m_RideState != RideState.Ride && m_RideState != RideState.WaitForItemUnequip) {
                var upNormal = m_RideState == RideState.Mount ? m_Rideable.Transform.up : -m_CharacterLocomotion.GravityDirection;
                // When the character is starting to mount they should rotate to face the same up direction as the rideable object. This allows the character to enter while on slopes.
                // Similarly, when the character exits they should rotate to the gravity direction.
                var proj = (rotation * Vector3.forward) - Vector3.Dot(rotation * Vector3.forward, upNormal) * upNormal;
                if (proj.sqrMagnitude > 0.0001f) {
                    var speed = m_RotationSpeed * m_CharacterLocomotion.TimeScale * Time.timeScale * Time.deltaTime * (m_RideState == RideState.DismountComplete ? 100 : 1);
                    var targetRotation = Quaternion.Slerp(rotation, Quaternion.LookRotation(proj, upNormal), speed);
                    deltaRotation = deltaRotation * (Quaternion.Inverse(rotation) * targetRotation);
                }
            } else if (m_Rideable.RideLocation != null) {
                // The character should fully rotate towards the target rotation after they have mounted.
                deltaRotation = MathUtility.InverseTransformQuaternion(m_Transform.rotation, m_Rideable.RideLocation.rotation);
            }
            m_CharacterLocomotion.DeltaRotation = deltaRotation.eulerAngles;
        }

        /// <summary>
        /// Update the controller's position values.
        /// </summary>
        public override void UpdatePosition()
        {
            if (m_RideState != RideState.Ride && m_RideState != RideState.WaitForItemUnequip) {
                return;
            }

            m_CharacterLocomotion.MotorThrottle = Vector3.zero;
            var deltaPosition = Vector3.MoveTowards(m_Transform.position, m_Rideable.RideLocation.position, m_MoveSpeed) - m_Transform.position;
            m_CharacterLocomotion.AbilityMotor = deltaPosition / (m_CharacterLocomotion.TimeScaleSquared * Time.timeScale * TimeUtility.FramerateDeltaTime);
        }

        /// <summary>
        /// Callback when the ability tries to be stopped. Start the dismount.
        /// </summary>
        public override void WillTryStopAbility()
        {
            if (m_RideState != RideState.Ride) {
                return;
            }

            // The character may not have space to dismount.
            if (!m_Rideable.CanDismount(ref m_LeftMount)) {
                return;
            }

            if (m_CharacterLocomotion.ItemEquipVerifierAbility != null) {
                // Don't allow a dismount if the character is equipping an item just after mounting.
                if (m_CharacterLocomotion.ItemEquipVerifierAbility.IsActive) {
                    return;
                }

                // If an item is equipped then it should first be unequipped before dismounting.
                if (m_CharacterLocomotion.ItemEquipVerifierAbility.TryToggleItem(this, true)) {
                    m_RideState = RideState.WaitForItemUnequip;
                    return;
                }
            }

            StartDismount();
        }

        /// <summary>
        /// Starts to dismount from the RideableObject.
        /// </summary>
        private void StartDismount()
        {
            m_RideState = RideState.Dismount;
            m_CharacterLocomotion.AbilityMotor = Vector3.zero;
            m_CharacterLocomotion.ForceRootMotionPosition = true;
            m_CharacterLocomotion.AllowRootMotionPosition = true;
            m_CharacterLocomotion.UpdateAbilityAnimatorParameters();
            // Update the rideable object's parameters as well so it can stay synchronized to the ride obejct.
            m_Rideable.CharacterLocomotion.UpdateAbilityAnimatorParameters();
            m_Rideable.StartDismount();

            // If the ability is active then it should also be stopped.
            var aimAbility = m_CharacterLocomotion.GetAbility<Items.Aim>();
            if (aimAbility != null) {
                aimAbility.StopAbility();
            }

            if (!m_DismountEvent.WaitForAnimationEvent) {
                m_MountDismountEvent = Scheduler.Schedule(m_DismountEvent.Duration, OnDismount);
            }
        }

        /// <summary>
        /// The character has dismounted - stop the ability.
        /// </summary>
        private void OnDismount()
        {
            m_RideState = RideState.DismountComplete;
            m_Rideable.Dismounted();
            m_Rideable = null;
            m_CharacterLocomotion.SetPlatform(null);
            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterForceIndependentLook", false);

            StopAbility();
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility()
        {
            // The character has to be dismounted in order to stop.
            return m_RideState == RideState.DismountComplete && Vector3.Dot(m_Transform.rotation * Vector3.up, -m_CharacterLocomotion.GravityDirection) >= m_Epsilon;
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            if (m_MountDismountEvent != null) {
                Scheduler.Cancel(m_MountDismountEvent);
                m_MountDismountEvent = null;
            }

            // If the state isn't complete then the ability was force stopped.
            if (m_RideState != RideState.DismountComplete) {
                m_Rideable.StartDismount();
                m_Rideable.Dismounted();
                m_Rideable = null;
                m_CharacterLocomotion.SetPlatform(null);
                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterForceIndependentLook", false);
            }
        }

        /// <summary>
        /// The ItemEquipVerifier ability has toggled an item slot.
        /// </summary>
        public void ItemToggled()
        {
            if (m_RideState != RideState.WaitForItemUnequip) {
                return;
            }

            // The character can dismount as soon as the item is no longer equipped.
            StartDismount();
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorRideMount", OnMount);
            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorRideDismount", OnDismount);
        }
    }
}