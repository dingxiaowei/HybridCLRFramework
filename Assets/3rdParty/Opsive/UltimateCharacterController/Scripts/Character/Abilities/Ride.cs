/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Objects.CharacterAssist;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
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
    [AllowMultipleAbilityTypes]
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
        [Tooltip("Specifies if the ability should wait for the OnAnimatorDismount animation event or wait for the specified duration before dismounting from the rideable object.")]
        [SerializeField] protected AnimationEventTrigger m_DismountEvent;
        [Tooltip("After the character mounts should the ability reequip the item that the character had before mounting?")]
        [SerializeField] protected bool m_ReequipItemAfterMount = true;

        public AnimationEventTrigger MountEvent { get { return m_MountEvent; } set { m_MountEvent = value; } }
        public AnimationEventTrigger DismountEvent { get { return m_DismountEvent; } set { m_DismountEvent = value; } }
        public bool ReequipItemAfterMount { get { return m_ReequipItemAfterMount; } set { m_ReequipItemAfterMount = value; } }

        private Rideable m_Rideable;
        
        private bool m_LeftMount;
        private ScheduledEventBase m_MountDismountEvent;
        private RideState m_RideState = RideState.DismountComplete;

        private Vector3 m_PositionOffset;

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
        /// <param name="fromTrigger">Is the object being validated within a trigger?</param>
        /// <returns>True if the object is valid. The object may not be valid if it doesn't have an ability-specific component attached.</returns>
        protected override bool ValidateObject(GameObject obj, bool withinTrigger)
        {
            if (!base.ValidateObject(obj, withinTrigger)) {
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
        /// Returns the possible AbilityStartLocations that the character can move towards.
        /// </summary>
        /// <returns>The possible AbilityStartLocations that the character can move towards.</returns>
        public override AbilityStartLocation[] GetStartLocations()
        {
            return m_Rideable.GameObject.GetComponentsInChildren<AbilityStartLocation>();
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            m_LeftMount = m_Rideable.Transform.InverseTransformPoint(m_Transform.position).x < 0;
            m_Rideable.Mount(this);

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
            return base.ShouldBlockAbilityStart(startingAbility);
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
            m_CharacterLocomotion.UpdateAbilityAnimatorParameters();
            m_Rideable.OnCharacterMount();

            m_PositionOffset = m_Rideable.Transform.InverseTransformPoint(m_Transform.position);

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
            }
        }

        /// <summary>
        /// Verify the rotation values.
        /// </summary>
        public override void ApplyRotation()
        {
            if (m_RideState == RideState.Ride || m_RideState == RideState.WaitForItemUnequip) {
                // Ensure the character stays in the correct rotation.
                m_CharacterLocomotion.Torque = m_Rideable.Torque;
            }
        }

        /// <summary>
        /// Verify the position values.
        /// </summary>
        public override void ApplyPosition()
        {
            if (m_RideState == RideState.Ride || m_RideState == RideState.WaitForItemUnequip) {
                // Ensure the character stays in the correct position.
                var targetPosition = m_Rideable.Transform.TransformPoint(m_PositionOffset);
                m_CharacterLocomotion.MoveDirection = targetPosition - m_Transform.position;
            }
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
            return m_RideState == RideState.DismountComplete;
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