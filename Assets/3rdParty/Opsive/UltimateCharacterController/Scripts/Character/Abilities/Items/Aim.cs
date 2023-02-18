/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities.Items
{
    using Opsive.Shared.Events;
    using Opsive.UltimateCharacterController.Input;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// ItemAbility which will aim the item.
    /// </summary>
    [DefaultStartType(AbilityStartType.ButtonDownContinuous)]
    [DefaultStopType(AbilityStopType.ButtonUp)]
    [DefaultInputName("Fire2")]
    [DefaultItemStateIndex(1)]
    [DefaultState("Aim")]
    public class Aim : ItemAbility
    {
        [Tooltip("When the Aim ability is activated should it stop the speed change ability?")]
        [SerializeField] protected bool m_StopSpeedChange = true;
        [Tooltip("Should the ability activate when the first person perspective is enabled?")]
        [SerializeField] protected bool m_ActivateInFirstPerson = true;
        [Tooltip("Should the ability rotate the character to face the look source target?")]
        [SerializeField] protected bool m_RotateTowardsLookSourceTarget = true;

        public bool StopSpeedChange { get { return m_StopSpeedChange; } set { m_StopSpeedChange = value; } }
        public bool ActivateInFirstPerson { get { return m_ActivateInFirstPerson; } set { m_ActivateInFirstPerson = value; } }
        public bool RotateTowardsLookSourceTarget { get { return m_RotateTowardsLookSourceTarget; } set { m_RotateTowardsLookSourceTarget = value; } }

        private ILookSource m_LookSource;
        private bool m_FirstPersonPerspective;
        private bool m_InputStart;
        private bool m_PerspectiveSwitch;
#if THIRD_PERSON_CONTROLLER
        private ThirdPersonController.Character.Abilities.Items.ItemPullback m_ItemPullback;
#endif

        public override bool CanReceiveMultipleStarts { get { return true; } }
        public bool InputStart { get { return m_InputStart; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

#if THIRD_PERSON_CONTROLLER
            m_ItemPullback = m_CharacterLocomotion.GetAbility<ThirdPersonController.Character.Abilities.Items.ItemPullback>();
#endif
            // The look source may have already been assigned if the ability was added to the character after the look source was assigned.
            m_LookSource = m_CharacterLocomotion.LookSource;

            EventHandler.RegisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCameraWillChangePerspectives", OnChangePerspectives);
            EventHandler.RegisterEvent<Ability, bool>(m_GameObject, "OnCharacterAbilityActive", OnAbilityActive);
            EventHandler.RegisterEvent<ItemAbility, bool>(m_GameObject, "OnCharacterItemAbilityActive", OnItemAbilityActive);
            EventHandler.RegisterEvent(m_GameObject, "OnRespawn", OnRespawn);
        }

        /// <summary>
        /// A new ILookSource object has been attached to the character.
        /// </summary>
        /// <param name="lookSource">The ILookSource object attached to the character.</param>
        private void OnAttachLookSource(ILookSource lookSource)
        {
            m_LookSource = lookSource;
        }

        /// <summary>
        /// The character perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the character in a first person perspective?</param>
        private void OnChangePerspectives(bool firstPersonPerspective)
        {
            // While in first person mode the character is always aiming so the ability should be started so the character shadow is correct.
            m_FirstPersonPerspective = firstPersonPerspective;
            if (m_ActivateInFirstPerson) {
                if (!IsActive && firstPersonPerspective) {
                    StartAbility();
                } else if ( IsActive && !firstPersonPerspective && !m_InputStart) {
                    m_PerspectiveSwitch = true;
                    StopAbility(true);
                    m_PerspectiveSwitch = false;
                }
            }
        }

        /// <summary>
        /// Returns the Item State Index which corresponds to the slot ID.
        /// </summary>
        /// <param name="slotID">The ID of the slot that corresponds to the Item State Index.</param>
        /// <returns>The Item State Index which corresponds to the slot ID.</returns>
        public override int GetItemStateIndex(int slotID)
        {
#if THIRD_PERSON_CONTROLLER
            // If the ItemPullback ability is active then an object would be obstructing the item location. Wait to change the state index
            // until no objects are obstructing the item location.
            if (m_ItemPullback != null && m_ItemPullback.IsActive) {
                return -1;
            }
#endif
            return base.GetItemStateIndex(slotID);
        }

        /// <summary>
        /// Returns the Item Substate Index which corresponds to the slot ID.
        /// </summary>
        /// <param name="slotID">The ID of the slot that corresponds to the Item Substate Index.</param>
        /// <returns>The Item Substate Index which corresponds to the slot ID.</returns>
        public override int GetItemSubstateIndex(int slotID)
        {
#if THIRD_PERSON_CONTROLLER
            // If the ItemPullback ability is active then an object would be obstructing the item location. Wait to change the state index
            // until no objects are obstructing the item location.
            if (m_ItemPullback != null && m_ItemPullback.IsActive) {
                return -1;
            }
#endif
            // Return the UsableItem's substate index if it isn't 0. This will allow the animator to know if the item is out of ammo.
            var item = m_Inventory.GetActiveItem(slotID);
            if (item != null && item.DominantItem) {
                var itemActions = item.ItemActions;
                for (int i = 0; i < itemActions.Length; ++i) {
                    var usableItem = itemActions[i] as UltimateCharacterController.Items.Actions.IUsableItem;
                    if (usableItem != null) {
                        var substateIndex = usableItem.GetItemSubstateIndex();
                        if (substateIndex != -1) {
                            return substateIndex;
                        }
                    }
                }
            }

            return m_InputStart ? 1 : 0;
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            if (m_InputStart) {
                return false;
            }
#if THIRD_PERSON_CONTROLLER
            if (m_ItemPullback != null && m_ItemPullback.IsActive) {
                return false;
            }
#endif
            return base.CanStartAbility();
        }

        /// <summary>
        /// Called when another ability is attempting to start and the current ability is active.
        /// Returns true or false depending on if the new ability should be blocked from starting.
        /// </summary>
        /// <param name="startingAbility">The ability that is starting.</param>
        /// <returns>True if the ability should be blocked.</returns>
        public override bool ShouldBlockAbilityStart(Ability startingAbility)
        {
            if (base.ShouldBlockAbilityStart(startingAbility)) {
                return true;
            }
            return PreventAbility(startingAbility);
        }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        /// <returns>True if the ability should be stopped.</returns>
        public override bool ShouldStopActiveAbility(Ability activeAbility)
        {
            if (base.ShouldStopActiveAbility(activeAbility)) {
                return true;
            }
            return PreventAbility(activeAbility);
        }

        /// <summary>
        /// Can the specified ability be active while the Aim ability is active?
        /// </summary>
        /// <param name="activeAbility">The ability to check if it can be active.</param>
        /// <returns>True if the specified ability can be active while the Aim ability is active.</returns>
        private bool PreventAbility(Ability activeAbility)
        {
            // InputStart isn't set until AbilityStarted so the InputIndex should be used as well.
            if (m_StopSpeedChange && (InputIndex != -1 || m_InputStart) && activeAbility is SpeedChange) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            // If the ability started because of the first person perspective change then the state shouldn't be changed. If the state was changed
            // to first person and the state was changed then the camera would zoom in which should only occur with a button press.
            if (InputIndex != -1 || m_StartType == AbilityStartType.Automatic) {
                base.AbilityStarted();
                m_InputStart = true;
            }
            EventHandler.ExecuteEvent(m_GameObject, "OnAimAbilityStart", true, m_InputStart);
            EventHandler.ExecuteEvent(m_GameObject, "OnAimAbilityAim", true);
        }

        /// <summary>
        /// Update the controller's rotation values.
        /// </summary>
        public override void UpdateRotation()
        {
            // If the character can look independently then the character does not need to rotate to face the look direction.
            if (m_CharacterLocomotion.ActiveMovementType.UseIndependentLook(true)) {
                return;
            }

            // The look source may be null if a remote player is still being initialized.
            if (m_LookSource == null || !m_RotateTowardsLookSourceTarget) {
                return;
            }

            // Determine the direction that the character should be facing.
            var lookDirection = m_LookSource.LookDirection(m_LookSource.LookPosition(), true, m_CharacterLayerManager.IgnoreInvisibleCharacterLayers, false);
            var rotation = m_Transform.rotation * Quaternion.Euler(m_CharacterLocomotion.DeltaRotation);
            var localLookDirection = MathUtility.InverseTransformDirection(lookDirection, rotation);
            localLookDirection.y = 0;
            lookDirection = MathUtility.TransformDirection(localLookDirection, rotation);
            var targetRotation = Quaternion.LookRotation(lookDirection, rotation * Vector3.up);
            m_CharacterLocomotion.DeltaRotation = (Quaternion.Inverse(m_Transform.rotation) * targetRotation).eulerAngles;
        }

        /// <summary>
        /// Can the input stop the ability?
        /// </summary>
        /// <param name="playerInput">A reference to the input component.</param>
        /// <returns>True if the input can stop the ability.</returns>
        public override bool CanInputStopAbility(PlayerInput playerInput)
        {
            if (m_ActivateInFirstPerson && !m_InputStart) {
                return false;
            }
            return base.CanInputStopAbility(playerInput);
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility()
        {
            if (m_ActivateInFirstPerson && m_FirstPersonPerspective) {
                // The ability can't stop for as long as the character is in first person mode. If the ability was started in with a button press then
                // the ability shouldn't stop but it should change the state so the camera will no longer zoom.
                if (m_InputStart) {
                    base.AbilityStopped(false);
                    EventHandler.ExecuteEvent(m_GameObject, "OnAimAbilityStart", false, m_InputStart);
                    m_InputStart = false;
                    m_CharacterLocomotion.UpdateItemAbilityAnimatorParameters();
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            // The base AbilityStopped may have already been called within CanStopAbility - don't call it again to prevent duplicate calls.
            EventHandler.ExecuteEvent(m_GameObject, "OnAimAbilityAim", false);
            if (m_PerspectiveSwitch && !m_InputStart) {
                return;
            }

            EventHandler.ExecuteEvent(m_GameObject, "OnAimAbilityStart", false, m_InputStart);
            m_InputStart = false;

            base.AbilityStopped(force);
        }

        /// <summary>
        /// Should the input be checked to ensure button up is using the correct value?
        /// </summary>
        /// <returns>True if the input should be checked.</returns>
        protected override bool ShouldCheckInput() { return false; }

        /// <summary>
        /// The ability has been started or stopped.
        /// </summary>
        /// <param name="ability">The ability which was started or stopped.</param>
        /// <param name="active">True if the ability was started, false if it was stopped.</param>
        private void OnAbilityActive(Ability ability, bool active)
        {
            // Another ability may have stopped aiming while in first person. Activate aiming again if necessary.
            if (!IsActive && !active && m_ActivateInFirstPerson && m_CharacterLocomotion.FirstPersonPerspective) {
                StartAbility();
            }
        }

        /// <summary>
        /// The item ability has been started or stopped.
        /// </summary>
        /// <param name="itemAbility">The item ability which was started or stopped.</param>
        /// <param name="active">True if the ability was started, false if it was stopped.</param>
        private void OnItemAbilityActive(ItemAbility itemAbility, bool active)
        {
#if UNITY_EDITOR
            if (IsActive && itemAbility.Index > Index && (itemAbility is Use
#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
                || itemAbility is Reload
#endif
                )) {
                Debug.Log($"Warning: The ability {itemAbility.GetType().Name} started but it has a lower priority than the aim ability." +
                          "This will prevent that ability from updating the Animator.");
            }
#endif
#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
            // The Aim ability should start again if the Reload ability stopped and is in a first person view. The Reload ability would have
            // stopped the Aim ability.
            if (itemAbility is Reload && !active && m_CharacterLocomotion.FirstPersonPerspective) {
                OnChangePerspectives(true);
            }
#endif
#if THIRD_PERSON_CONTROLLER
            // If the ItemPullback ability is activated then the aim state should no longer be set. This will prevent the aim animator parameters from updating.
            if (IsActive && itemAbility == m_ItemPullback) {
                EventHandler.ExecuteEvent(m_GameObject, "OnAimAbilityAim", !active);
            }
#endif
        }

        /// <summary>
        /// The character has respawned. Determine if the ability should start.
        /// </summary>
        private void OnRespawn()
        {
            if (m_ActivateInFirstPerson && m_CharacterLocomotion.FirstPersonPerspective) {
                StartAbility();
            }
        }

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCameraWillChangePerspectives", OnChangePerspectives);
            EventHandler.UnregisterEvent<Ability, bool>(m_GameObject, "OnCharacterAbilityActive", OnAbilityActive);
            EventHandler.UnregisterEvent<ItemAbility, bool>(m_GameObject, "OnCharacterItemAbilityActive", OnItemAbilityActive);
            EventHandler.UnregisterEvent(m_GameObject, "OnRespawn", OnRespawn);
        }
    }
}