/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities.Items
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Input;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Items.Actions;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// ItemAbility which will start using the IUsableItem.
    /// </summary>
    [DefaultStartType(AbilityStartType.ButtonDown)]
    [DefaultStopType(AbilityStopType.ButtonUp)]
    [DefaultInputName("Fire1")]
    [DefaultItemStateIndex(2)]
    [DefaultState("Use")]
    [AllowDuplicateTypes]
    public class Use : ItemAbility
    {
        [Tooltip("The slot that should be used. -1 will use all of the slots.")]
        [SerializeField] protected int m_SlotID = -1;
        [Tooltip("The ID of the ItemAction component that can be used.")]
        [SerializeField] protected int m_ActionID;
        [Tooltip("Should the ability rotate the character to face the look source target?")]
        [SerializeField] protected bool m_RotateTowardsLookSourceTarget = true;

        public int SlotID { get { return m_SlotID; }
            set
            {
                if (m_SlotID != value) {
                    UnregisterSlotEvents(m_SlotID);
                    m_SlotID = value;
                    RegisterSlotEvents(m_SlotID);
                }
            }
        }
        public int ActionID { get { return m_ActionID; } set { m_ActionID = value; } }
        public bool RotateTowardsLookSourceTarget { get { return m_RotateTowardsLookSourceTarget; } set { m_RotateTowardsLookSourceTarget = value; } }

        private ILookSource m_LookSource;
        protected IUsableItem[] m_UsableItems;
        private PlayerInput m_PlayerInput;
        private bool[] m_WaitForUseEvent;
        private bool[] m_CanStopAbility;
        private bool[] m_WaitForUseCompleteEvent;
        private bool[] m_UseCompleted;
        private Item m_FaceTargetItem;
        private ScheduledEventBase[] m_UseEvent;
        private ScheduledEventBase[] m_CanStopEvent;
        private bool m_Started;

        public IUsableItem[] UsableItems { get { return m_UsableItems; } }
        public Item FaceTargetItem { get { return m_FaceTargetItem; } }

        public override bool CanReceiveMultipleStarts { get { return true; } }
#if UNITY_EDITOR
        public override string AbilityDescription {
            get {
                var description = string.Empty;
                if (m_SlotID != -1) {
                    description += "Slot " + m_SlotID;
                }
                if (m_ActionID != 0) {
                    if (!string.IsNullOrEmpty(description)) {
                        description += ", ";
                    }
                    description += "Action " + m_ActionID;
                }
                return description;
            } }
#endif

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_PlayerInput = m_GameObject.GetCachedComponent<PlayerInput>();
            var count = m_SlotID == -1 ? m_Inventory.SlotCount : 1;
            m_UsableItems = new IUsableItem[count];
            m_WaitForUseEvent = new bool[count];
            m_CanStopAbility = new bool[count];
            m_WaitForUseCompleteEvent = new bool[count];
            m_UseCompleted = new bool[count];
            m_UseEvent = new ScheduledEventBase[count];
            m_CanStopEvent = new ScheduledEventBase[count];
            for (int i = 0; i < count; ++i) {
                m_WaitForUseEvent[i] = false;
                m_UseCompleted[i] = true;
            }
            // The look source may have already been assigned if the ability was added to the character after the look source was assigned.
            m_LookSource = m_CharacterLocomotion.LookSource;

            EventHandler.RegisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
            EventHandler.RegisterEvent<bool>(m_GameObject, "OnEnableGameplayInput", OnEnableGameplayInput);
            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorItemUse", OnItemUse);
            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorItemUseComplete", OnItemUseComplete);
#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
            EventHandler.RegisterEvent<ItemAbility, bool>(m_GameObject, "OnCharacterItemAbilityActive", OnItemAbilityActive);
#endif
            // Register for the interested slot events.
            RegisterSlotEvents(m_SlotID);
        }

        /// <summary>
        /// Registers for the interested events according to the slot id.
        /// </summary>
        /// <param name="slotID">The slot id to register for.</param>
        private void RegisterSlotEvents(int slotID)
        {
            if (!Application.isPlaying) {
                return;
            }
            if (slotID == 0) {
                EventHandler.RegisterEvent(m_GameObject, "OnAnimatorItemUseFirstSlot", OnItemUseFirstSlot);
                EventHandler.RegisterEvent(m_GameObject, "OnAnimatorItemUseCompleteFirstSlot", OnItemUseCompleteFirstSlot);
            } else if (slotID == 1) {
                EventHandler.RegisterEvent(m_GameObject, "OnAnimatorItemUseSecondSlot", OnItemUseSecondSlot);
                EventHandler.RegisterEvent(m_GameObject, "OnAnimatorItemUseCompleteSecondSlot", OnItemUseCompleteSecondSlot);
            } else if (slotID == 2) {
                EventHandler.RegisterEvent(m_GameObject, "OnAnimatorItemUseThirdSlot", OnItemUseThirdSlot);
                EventHandler.RegisterEvent(m_GameObject, "OnAnimatorItemUseCompleteThirdSlot", OnItemUseCompleteThirdSlot);
            } else if (slotID != -1) {
                Debug.LogError("Error: The Use ability does not listen to slot " + m_SlotID);
            }
        }

        /// <summary>
        /// Unregisters from the interested events according to the slot id.
        /// </summary>
        /// <param name="slotID">The slot id to unregister from.</param>
        private void UnregisterSlotEvents(int slotID)
        {
            if (!Application.isPlaying) {
                return;
            }
            if (slotID == 0) {
                EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorItemUseFirstSlot", OnItemUseFirstSlot);
                EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorItemUseCompleteFirstSlot", OnItemUseCompleteFirstSlot);
            } else if (slotID == 1) {
                EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorItemUseSecondSlot", OnItemUseSecondSlot);
                EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorItemUseCompleteSecondSlot", OnItemUseCompleteSecondSlot);
            } else if (slotID == 2) {
                EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorItemUseThirdSlot", OnItemUseThirdSlot);
                EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorItemUseCompleteThirdSlot", OnItemUseCompleteThirdSlot);
            }
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
        /// Returns the Item State Index which corresponds to the slot ID.
        /// </summary>
        /// <param name="slotID">The ID of the slot that corresponds to the Item State Index.</param>
        /// <returns>The Item State Index which corresponds to the slot ID.</returns>
        public override int GetItemStateIndex(int slotID)
        {
            // Return the ItemStateIndex if the SlotID matches the requested slotID.
            if (m_SlotID == -1) {
                if (m_UsableItems[slotID] != null) {
                    return m_ItemStateIndex;
                }
            } else if (m_SlotID == slotID && m_UsableItems[0] != null) {
                return m_ItemStateIndex;
            }
            return -1;
        }

        /// <summary>
        /// Returns the Item Substate Index which corresponds to the slot ID.
        /// </summary>
        /// <param name="slotID">The ID of the slot that corresponds to the Item Substate Index.</param>
        /// <returns>The Item Substate Index which corresponds to the slot ID.</returns>
        public override int GetItemSubstateIndex(int slotID)
        {
            if (m_SlotID == -1) {
                if (m_UsableItems[slotID] != null) {
                    return m_UsableItems[slotID].GetItemSubstateIndex();
                }
            } else if (m_SlotID == slotID && m_UsableItems[0] != null) {
                return m_UsableItems[0].GetItemSubstateIndex();
            }
            return -1;
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <returns>True if the item can be used.</returns>
        public override bool CanStartAbility()
        {
            // An attribute may prevent the ability from starting.
            if (!base.CanStartAbility()) {
                return false;
            }

            // Don't use the item if the cursor is over any UI.
            if (m_PlayerInput != null && m_PlayerInput.IsPointerOverUI()) {
                return false;
            }

            // A look source must exist.
            if (m_LookSource == null) {
                return false;
            }

            // If the SlotID is -1 then the ability should use every equipped item at the same time. If only one slot has a UsableItem then the 
            // ability can start. If the SlotID is not -1 then the ability should use the item in the specified slot.
            var canUse = false;
            if (m_SlotID == -1) {
                for (int i = 0; i < m_UsableItems.Length; ++i) {
                    var item = m_Inventory.GetActiveItem(i);
                    if (item == null) {
                        continue;
                    }

                    var itemAction = item.GetItemAction(m_ActionID);
                    if (itemAction == null) {
                        continue;
                    }

                    m_UsableItems[i] = itemAction as IUsableItem;

                    // The item can't be used if it isn't a usable item.
                    if (m_UsableItems[i] != null) {
                        if (m_UseCompleted[i] && !m_CanStopAbility[i] && m_UsableItems[i].IsItemInUse() && m_UsableItems[i].CanStopItemUse()) {
                            m_UsableItems[i].StopItemUse();
                        }

                        if (!m_UsableItems[i].CanUseItem(this, UsableItem.UseAbilityState.Start)) {
                            continue;
                        }
                        canUse = true;
                    }
                }
            } else {
                var item = m_Inventory.GetActiveItem(m_SlotID);
                if (item != null) {
                    var itemAction = item.GetItemAction(m_ActionID);
                    if (itemAction != null) {
                        m_UsableItems[0] = itemAction as IUsableItem;

                        // The item can't be used if it isn't a usable item.
                        if (m_UsableItems[0] != null) {
                            // If the item has completed use and is waiting on the CanStop event then it should reset so it can be used again.
                            if (m_UseCompleted[0] && !m_CanStopAbility[0] && m_UsableItems[0].IsItemInUse() && m_UsableItems[0].CanStopItemUse()) {
                                m_UsableItems[0].StopItemUse();
                            }
                            if (m_UsableItems[0].CanUseItem(this, UsableItem.UseAbilityState.Start)) {
                                canUse = true;
                            }
                        }
                    }
                }
            }

            return canUse;
        }

        /// <summary>
        /// Does the ability use the specified ItemAction type?
        /// </summary>
        /// <param name="itemActionType">The ItemAction type to compare against.</param>
        /// <returns>True if the ability uses the specified ItemAction type.</returns>
        public bool UsesItemActionType(System.Type itemActionType)
        {
            // If the SlotID is -1 then the ability should can every equipped item at the same time. If only one slot has an action which is of the specified type
            // then the entire method will return true. If the SlotID is not -1 then the ability will only check against the single ItemAction.
            if (m_SlotID == -1) {
                for (int i = 0; i < m_UsableItems.Length; ++i) {
                    var item = m_Inventory.GetActiveItem(i);
                    if (item == null) {
                        continue;
                    }

                    var itemAction = item.GetItemAction(m_ActionID);
                    if (itemAction == null) {
                        return false;
                    }
                    // It only takes one ItemAction for the ability to use the specified ItemAction.
                    if (itemAction.GetType().IsAssignableFrom(itemActionType)) {
                        return true;
                    }
                }
            } else {
                var item = m_Inventory.GetActiveItem(m_SlotID);
                if (item != null) {
                    var itemAction = item.GetItemAction(m_ActionID);
                    if (itemAction == null) {
                        return false;
                    }
                    return itemAction.GetType().IsAssignableFrom(itemActionType);
                }
            }

            return false;
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

            if (startingAbility is Use && startingAbility != this) {
                // The same item should not be able to be used by multiple use abilities at the same time. Different items can be used at the same time, such as
                // a primary item and a secondary grenade throw or dual pistols.
                var startingUseAbility = startingAbility as Use;
                for (int i = 0; i < m_UsableItems.Length; ++i) {
                    if (m_UsableItems[i] == null) {
                        continue;
                    }

                    for (int j = 0; j < startingUseAbility.UsableItems.Length; ++j) {
                        if (startingUseAbility.UsableItems[j] == null) {
                            continue;
                        }

                        if (m_UsableItems[i].Item == startingUseAbility.UsableItems[j].Item) {
                            return true;
                        }
                    }
                }
            }

#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
            if (startingAbility is Reload) {
                // The Use ability has priority over the Reload ability. Prevent the reload ability from starting if the use ability is active.
                if (startingAbility.InputIndex != -1) {
                    // If the item isn't actively being used then it shouldn't block reload.
                    var shouldBlock = false;
                    for (int i = 0; i < m_UsableItems.Length; ++i) {
                        if (m_UsableItems[i] != null && !m_UseCompleted[i]) {
                            shouldBlock = true;
                            break;
                        }
                    }
                    if (!shouldBlock) {
                        return false;
                    }

                    var reloadAbility = startingAbility as Reload;
                    StopItemReload(reloadAbility);

                    // The ability should only be blocked if there aren't any items left to reload. An item may still be reloaded if it's parented to a different
                    // slot from what is being used.
                    shouldBlock = true;
                    for (int i = 0; i < reloadAbility.ReloadableItems.Length; ++i) {
                        if (reloadAbility.ReloadableItems[i] != null) {
                            shouldBlock = false;
                        }
                    }
                    return shouldBlock;
                }
            }
#endif
            // Active items can block starting abilities.
            for (int i = 0; i < m_UsableItems.Length; ++i) {
                if (m_UsableItems[i] == null) {
                    continue;
                }
                if (!m_UsableItems[i].CanStartAbility(startingAbility)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        /// <returns>True if the ability should be stopped.</returns>
        public override bool ShouldStopActiveAbility(Ability activeAbility)
        {
            // If Use starts while EquipUnequip is active then EquipUnequip should stop.
            if (activeAbility is EquipUnequip) {
                return true;
            }
#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
            if (activeAbility is Reload) {
                // The Use ability has priority over the Reload ability. Stop Reload if it is currently reloading the item.
                StopItemReload(activeAbility as Reload);
            }
#endif
            return base.ShouldStopActiveAbility(activeAbility);
        }

#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
        /// <summary>
        /// Stops any item that is trying to reload while it is being used.
        /// </summary>
        /// <param name="reloadAbility">A reference to the reload ability.</param>
        /// <returns>True if the same item is trying to be used and reloaded.</returns>
        private void StopItemReload(Reload reloadAbility)
        {
            for (int i = 0; i < m_UsableItems.Length; ++i) {
                if (m_UsableItems[i] == null) {
                    continue;
                }

                for (int j = 0; j < reloadAbility.ReloadableItems.Length; ++j) {
                    if (reloadAbility.ReloadableItems[j] == null) {
                        continue;
                    }

                    if (m_UsableItems[i].Item == reloadAbility.ReloadableItems[j].Item) {
                        reloadAbility.StopItemReload(j);
                    }
                }
            }
        }
#endif

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            // Shootable weapons will deduct the attribute on each use.
            var enableAttributeModifier = true;
#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
            for (int i = 0; i < m_UsableItems.Length; ++i) {
                if (enableAttributeModifier && m_UsableItems[i] != null && m_UsableItems[i] is ShootableWeapon) {
                    enableAttributeModifier = false;
                    break;
                }
            }
#endif
            base.AbilityStarted(enableAttributeModifier);

            // The item may require root motion to prevent sliding. It may also require the character to face the target before it can actually be used.
            m_FaceTargetItem = null;
            var itemStartedUse = false;
            for (int i = 0; i < m_UsableItems.Length; ++i) {
                if (m_UsableItems[i] == null) {
                    m_CanStopAbility[i] = true;
                    continue;
                }

                m_UsableItems[i].StartItemUse(this);
                // An Animator Audio State Set may prevent the item from being used.
                if (!m_UsableItems[i].IsItemInUse()) {
                    m_CanStopAbility[i] = true;
                    continue;
                }

                itemStartedUse = true;
                m_WaitForUseEvent[i] = true;
                m_WaitForUseCompleteEvent[i] = false;
                m_UseCompleted[i] = false;
                ResetCanStopEvent(i);

                if (m_UsableItems[i].ForceRootMotionPosition) {
                    m_CharacterLocomotion.ForceRootMotionPosition = true;
                }
                if (m_UsableItems[i].ForceRootMotionRotation) {
                    m_CharacterLocomotion.ForceRootMotionRotation = true;
                }
                if (m_UsableItems[i].FaceTarget && !m_CharacterLocomotion.ActiveMovementType.UseIndependentLook(true)) {
                    m_FaceTargetItem = m_UsableItems[i].Item;
                }
                ScheduleUseEvent(i);
                EventHandler.ExecuteEvent(m_GameObject, "OnItemStartUse", m_UsableItems[i], true);
            }

            // The ability can start multiple times. Ensure the events are only subscribed to once.
            if (itemStartedUse && !m_Started) {
                EventHandler.ExecuteEvent(m_GameObject, "OnUseAbilityStart", true, this);
                m_Started = true;
            } else if (!itemStartedUse) {
                // The ability should be stopped if no items are being used.
                var stopAbility = true;
                for (int i = 0; i < m_UseCompleted.Length; ++i) {
                    if (m_UsableItems[i] == null) {
                        continue;
                    }

                    if (!m_UseCompleted[i] || !m_CanStopAbility[i]) {
                        stopAbility = false;
                        break;
                    }
                }
                if (stopAbility) {
                    StopAbility();
                }
            }
        }

        /// <summary>
        /// Resets the CanStop event back to its default value.
        /// </summary>
        /// <param name="slotID">The id of the slot that should be reset.</param>
        private void ResetCanStopEvent(int slotID)
        {
            // Melee weapons will not have a stop use delay so should not reset the event.
            if (m_UsableItems[slotID] != null && m_UsableItems[slotID].StopUseAbilityDelay == 0) {
                m_CanStopAbility[slotID] = true;
                return;
            }

            m_CanStopAbility[slotID] = m_StopType == AbilityStopType.Manual;
            if (m_CanStopEvent[slotID] != null) {
                Scheduler.Cancel(m_CanStopEvent[slotID]);
                m_CanStopEvent[slotID] = null;
            }
        }

        /// <summary>
        /// Schedules the use event.
        /// </summary>
        /// <param name="slotID">The id of the slot that should be scheduled.</param>
        private void ScheduleUseEvent(int slotID)
        {
            if (m_UseEvent[slotID] != null) {
                Scheduler.Cancel(m_UseEvent[slotID]);
            }

            if (!m_UsableItems[slotID].UseEvent.WaitForAnimationEvent) {
                m_UseEvent[slotID] = Scheduler.ScheduleFixed(m_UsableItems[slotID].UseEvent.Duration, UseItem, slotID);
            }
        }

        /// <summary>
        /// The animation has used all of the items.
        /// </summary>
        private void OnItemUse()
        {
            for (int i = 0; i < m_UsableItems.Length; ++i) {
                if (m_UsableItems[i] != null) {
                    UseItem(i);
                }
            }
        }

        /// <summary>
        /// The animation has used the first item slot.
        /// </summary>
        private void OnItemUseFirstSlot()
        {
            UseItem(0);
        }

        /// <summary>
        /// The animation has used the second item slot.
        /// </summary>
        private void OnItemUseSecondSlot()
        {
            UseItem(1);
        }

        /// <summary>
        /// The animation has used the third item slot.
        /// </summary>
        private void OnItemUseThirdSlot()
        {
            UseItem(2);
        }

        /// <summary>
        /// The ItemUse event has been triggered.
        /// </summary>
        /// <param name="slotID">The id of the slot that was used.</param>
        private void UseItem(int slotID)
        {
            var usableItem = m_UsableItems[slotID];
            if (usableItem == null) {
                return;
            }

            m_WaitForUseEvent[slotID] = false;
        }

        /// <summary>
        /// Updates the ability.
        /// </summary>
        public override void Update()
        {
            // Do not call the base method to prevent an attribute from stopping the use.
        }

        /// <summary>
        /// Updates the ability after the controller has updated. This will ensure the character is in the most up to date position.
        /// </summary>
        public override void LateUpdate()
        {
            // Enable the collision layer so the weapons can apply damage the originating character.
            m_CharacterLocomotion.EnableColliderCollisionLayer(true);

            // Tries to use the item. This is done within Update because the item can be used multiple times when the input button is held down.
            for (int i = 0; i < m_UsableItems.Length; ++i) {
                if (m_UsableItems[i] != null) {
                    // Allow the items currently in use to be updated.
                    m_UsableItems[i].UseItemUpdate();

                    // If the InputIndex isn't -1 and the stop event isn't null then the ability is trying to be stopped. The ability must remain active for as long
                    // as the StopAbilityDelay but during this time the item should not be used.
                    if (InputIndex != -1 && m_CanStopEvent[i] != null && m_UsableItems[i].CanStopItemUse()) {
                        continue;
                    }

                    // Don't use the item if the item is waiting for the ItemUse event or has already been used.
                    if (m_WaitForUseEvent[i]) {
                        continue;
                    }

                    if (m_UsableItems[i].CanUseItem(this, UsableItem.UseAbilityState.Update)) {
                        m_UsableItems[i].UseItem();

#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
                        // Each use should update the attribute.
                        if (m_UsableItems[i] is ShootableWeapon && m_AttributeModifier != null) {
                            m_AttributeModifier.EnableModifier(true);
                        }
#endif

                        // Using the item may have killed the character and stopped the ability.
                        if (!IsActive) {
                            return;
                        }

                        // The ability may have been stopped immediately after use. This will happen if for example a shootable weapon automatically reloads when it
                        // is out of ammo.
                        if (m_UsableItems[i] != null) {
                            // A custom use animation should be played.
                            m_CharacterLocomotion.UpdateItemAbilityAnimatorParameters();

                            // The ability can be stopped after it has been used to allow hip firing.
                            if (m_UsableItems[i].StopUseAbilityDelay > 0) {
                                ResetCanStopEvent(i);
                                m_CanStopEvent[i] = Scheduler.ScheduleFixed(m_UsableItems[i].StopUseAbilityDelay, AbilityCanStop, i);
                            } else {
                                m_CanStopAbility[i] = true;
                            }

                            // The item needs to be used before the complete event can be called.
                            if (!m_UsableItems[i].IsItemUsePending()) {
                                ScheduleCompleteEvent(i, true);
                            }
                        }
                    }
                }
            }

            m_CharacterLocomotion.EnableColliderCollisionLayer(false);
        }

        /// <summary>
        /// Schedules the complete event.
        /// </summary>
        /// <param name="index">The index of the complete event to schedule.</param>
        /// <param name="scheduleEvent">Should the event be scheduled? If false the Use Complete array will only be set.</param>
        protected void ScheduleCompleteEvent(int index, bool scheduleEvent)
        {
            m_WaitForUseCompleteEvent[index] = true;
            if (scheduleEvent) {
                if (m_UseEvent[index] != null) {
                    Scheduler.Cancel(m_UseEvent[index]);
                }
                if (!m_UsableItems[index].UseCompleteEvent.WaitForAnimationEvent) {
                    m_UseEvent[index] = Scheduler.ScheduleFixed(m_UsableItems[index].UseCompleteEvent.Duration, UseCompleteItem, index);
                }
            }
        }

        /// <summary>
        /// The item has been used and the ability can now stop.
        /// </summary>
        /// <param name="slotID">The ID of the slot that can stop.</param>
        private void AbilityCanStop(int slotID)
        {
            m_CanStopAbility[slotID] = true;
            m_CanStopEvent[slotID] = null;

            // The ability should be stopped if all items have finished being used.
            var stopAbility = true;
            for (int i = 0; i < m_UseCompleted.Length; ++i) {
                if (!m_UseCompleted[i]) {
                    stopAbility = false;
                    break;
                }
            }

            if (stopAbility) {
                StopAbility();
            }
        }

        /// <summary>
        /// Update the controller's rotation values.
        /// </summary>
        public override void UpdateRotation()
        {
            // The rotation doesn't need to be updated if the item doesn't need to face the target.
            if (m_FaceTargetItem == null) {
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
        /// The use animation has completed for all of the items.
        /// </summary>
        private void OnItemUseComplete()
        {
            for (int i = 0; i < m_UsableItems.Length; ++i) {
                if (m_UsableItems[i] != null) {
                    UseCompleteItem(i);
                }
            }
        }

        /// <summary>
        /// The use animation has completed for the first item slot.
        /// </summary>
        private void OnItemUseCompleteFirstSlot()
        {
            UseCompleteItem(0);
        }

        /// <summary>
        /// The use animation has completed for the second item slot.
        /// </summary>
        private void OnItemUseCompleteSecondSlot()
        {
            UseCompleteItem(1);
        }

        /// <summary>
        /// The use animation has completed for the third item slot.
        /// </summary>
        private void OnItemUseCompleteThirdSlot()
        {
            UseCompleteItem(2);
        }

        /// <summary>
        /// The animator has finished playing the use animation.
        /// </summary>
        /// <param name="slotID">The id of the slot that was used.</param>
        protected virtual void UseCompleteItem(int slotID)
        {
            var usableItem = m_UsableItems[slotID];
            if (usableItem == null || !m_WaitForUseCompleteEvent[slotID]) {
                return;
            }

            m_WaitForUseCompleteEvent[slotID] = false;
            m_CharacterLocomotion.UpdateItemAbilityAnimatorParameters();
            m_UseCompleted[slotID] = true;
            m_UsableItems[slotID].ItemUseComplete();

            // The ability should stop when all the items have been used.
            var stopAbility = true;
            for (int i = 0; i < m_UseCompleted.Length; ++i) {
                if (m_UsableItems[i] == null) {
                    continue;
                }

                if (!m_UseCompleted[i] || !m_CanStopAbility[i]) {
                    stopAbility = false;
                    break;
                }
            }
            if (stopAbility) {
                StopAbility();
            }
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility()
        {
            for (int i = 0; i < m_UsableItems.Length; ++i) {
                // If the item is currently being used and it cannot be stopped then the ability cannot stop either.
                if (m_UsableItems[i] != null && m_UsableItems[i].IsItemInUse()) {
                    m_UsableItems[i].TryStopItemUse();
                    // The UsableItem may not be able to be stopped (for example, if a throwable item should be used when the button press is released).
                    if (!m_UsableItems[i].CanStopItemUse()) {
                        return false;
                    }
                    if (!m_UseCompleted[i]) {
                        // The complete event may not have been called if the item use was still pending.
                        if (m_UseEvent[i] == null || !m_UseEvent[i].Active) {
                            ScheduleCompleteEvent(i, true);
                        }
                        return false;
                    }
                }
                // Don't stop if CanStopAbility is false. This will allow hip firing to keep the item held up momentarily after being used. The ability should always
                // be able to stop during a reload.
                if (!m_CanStopAbility[i]
#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
                        && !m_CharacterLocomotion.IsAbilityTypeActive<Reload>()
#endif
                        ) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            // The item may require root motion to prevent sliding.
            for (int i = 0; i < m_UsableItems.Length; ++i) {
                if (m_UsableItems[i] != null) {
                    if (m_UsableItems[i].ForceRootMotionPosition) {
                        m_CharacterLocomotion.ForceRootMotionPosition = false;
                    }
                    if (m_UsableItems[i].ForceRootMotionRotation) {
                        m_CharacterLocomotion.ForceRootMotionRotation = false;
                    }
                    m_UsableItems[i].StopItemUse();
                    EventHandler.ExecuteEvent(m_GameObject, "OnItemStartUse", m_UsableItems[i], false);
                    m_UsableItems[i] = null;
                    m_UseCompleted[i] = true;
                    if (m_UseEvent[i] != null) {
                        Scheduler.Cancel(m_UseEvent[i]);
                        m_UseEvent[i] = null;
                    }
                    ResetCanStopEvent(i);
                }
            }

            m_Started = false;
            EventHandler.ExecuteEvent(m_GameObject, "OnUseAbilityStart", false, this);
        }
        
#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
        /// <summary>
        /// The item ability has been started or stopped.
        /// </summary>
        /// <param name="itemAbility">The item ability which was started or stopped.</param>
        /// <param name="active">True if the ability was started, false if it was stopped.</param>
        private void OnItemAbilityActive(ItemAbility itemAbility, bool active)
        {
            if (!(itemAbility is Reload)) {
                return;
            }

            // Use currently is not active, but it may have to start if the Use ability is trying to be started.
            if (!active && InputIndex != -1 && m_PlayerInput != null) {
                // Change the start type so the button up won't affect if the ability can start.
                var startType = m_StartType;
                if (startType == AbilityStartType.ButtonDown) {
                    m_StartType = AbilityStartType.ButtonDownContinuous;
                }
                if (CanInputStartAbility(m_PlayerInput)) {
                    if (IsActive) {
                        // The use state should be reset if the ability is currently active.
                        for (int i = 0; i < m_UsableItems.Length; ++i) {
                            if (m_UsableItems[i] == null) {
                                continue;
                            }
                            m_UsableItems[i].StartItemUse(this);
                            ResetCanStopEvent(i);
                        }
                        InputIndex = -1;
                    } else {
                        // The ability isn't active, but it should be.
                        StartAbility();
                    }
                }
                m_StartType = startType;
                return;
            }

            var reloadAbility = itemAbility as Reload;
            for (int i = 0; i < reloadAbility.ReloadableItems.Length; ++i) {
                if (reloadAbility.ReloadableItems[i] == null) {
                    continue;
                }
                var slotID = reloadAbility.ReloadableItems[i].Item.SlotID;
                if (m_SlotID != -1) {
                    if (m_SlotID != slotID) {
                        continue;
                    }
                    // If a slot ID is specified then there will only be one element.
                    slotID = 0;
                }

                // If the reload ability is active the CanStop event shouldn't fire so the character can continue to fire after reloading.
                if (active) {
                    // If the ability index is not -1 then the item is trying to be stopped. Prevent the item from being used again when reload is complete.
                    if (InputIndex != -1) {
                        StopAbility(true);
                    } else {
                        ResetCanStopEvent(slotID);
                    }
                } else {
                    m_CanStopEvent[slotID] = Scheduler.ScheduleFixed(m_UsableItems[slotID].StopUseAbilityDelay, AbilityCanStop, slotID);
                }
            }
        }
#endif

        /// <summary>
        /// Enables or disables gameplay input. An example of when it will not be enabled is when there is a fullscreen UI over the main camera.
        /// </summary>
        /// <param name="enable">True if the input is enabled.</param>
        private void OnEnableGameplayInput(bool enable)
        {
            // Force stop the ability if the character no longer has input.
            if (!enable && IsActive) {
                StopAbility(true);
            }
        }

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnEnableGameplayInput", OnEnableGameplayInput);
            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorItemUse", OnItemUse);
            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorItemUseComplete", OnItemUseComplete);
#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
            EventHandler.UnregisterEvent<ItemAbility, bool>(m_GameObject, "OnCharacterItemAbilityActive", OnItemAbilityActive);
#endif
            UnregisterSlotEvents(m_SlotID);
        }
    }
}