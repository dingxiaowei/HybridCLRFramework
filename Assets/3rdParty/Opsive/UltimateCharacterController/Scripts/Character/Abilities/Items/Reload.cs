/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities.Items
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Items.Actions;
    using Opsive.UltimateCharacterController.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// ItemAbility which will reload the item. There are two parts to a reload:
    /// - The first part will take the reload amount from the inventory and add it to the item.
    /// - The second part can wait for a small amount of time after the first part to ensure the reload animation is complete before ending the ability.
    /// </summary>
    [DefaultStartType(AbilityStartType.ButtonDown)]
    [DefaultInputName("Reload")]
    [DefaultItemStateIndex(3)]
    [AllowDuplicateTypes]
    public class Reload : ItemAbility
    {
        /// <summary>
        /// Specifies when the item should automatically be reloaded.
        /// </summary>
        public enum AutoReloadType
        {
            Pickup = 1, // The item should be reloaded upon pickup for the first time.
            Empty = 2   // Automatically reload when the item is empty.
        }

        [Tooltip("The slot that should be reloaded. -1 will use all of the slots.")]
        [SerializeField] protected int m_SlotID = -1;
        [Tooltip("The ID of the ItemAction component that can be reloaded.")]
        [SerializeField] protected int m_ActionID;

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

        private IReloadableItem[] m_ReloadableItems;
        private ScheduledEventBase[] m_ReloadEvents;
        private HashSet<Item> m_InventoryItems = new HashSet<Item>();
        private HashSet<Item> m_EquippedItems = new HashSet<Item>();
        private IReloadableItem[] m_CanReloadItems;
        private bool[] m_Reloaded;

        public IReloadableItem[] ReloadableItems { get { return m_ReloadableItems; } }

#if UNITY_EDITOR
        public override string AbilityDescription { get { if (m_SlotID != -1) { return "Slot " + m_SlotID; } return string.Empty; } }
#endif

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_ReloadableItems = new IReloadableItem[m_SlotID == -1 ? m_Inventory.SlotCount : 1];
            m_ReloadEvents = new ScheduledEventBase[m_ReloadableItems.Length];
            m_Reloaded = new bool[m_ReloadableItems.Length];

            EventHandler.RegisterEvent(m_GameObject, "OnItemPickupStartPickup", OnStartPickup);
            EventHandler.RegisterEvent<IItemIdentifier, int, bool, bool>(m_GameObject, "OnInventoryPickupItemIdentifier", OnPickupItemIdentifier);
            EventHandler.RegisterEvent<int, IItemIdentifier, bool, bool>(m_GameObject, "OnItemTryReload", OnTryReload);
            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorItemReload", OnItemReload);
            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorItemReloadComplete", OnItemReloadComplete);
            // Register for the interested slot events.
            RegisterSlotEvents(m_SlotID);
        }

        /// <summary>
        /// Registers for the interested events according to the slot id.
        /// </summary>
        /// <param name="slotID">The slot id to register for.</param>
        private void RegisterSlotEvents(int slotID)
        {
            if (slotID == 0) {
                EventHandler.RegisterEvent(m_GameObject, "OnAnimatorItemReloadFirstSlot", OnItemReloadFirstSlot);
                EventHandler.RegisterEvent(m_GameObject, "OnAnimatorItemReloadCompleteFirstSlot", OnItemReloadCompleteFirstSlot);
            } else if (slotID == 1) {
                EventHandler.RegisterEvent(m_GameObject, "OnAnimatorItemReloadSecondSlot", OnItemReloadSecondSlot);
                EventHandler.RegisterEvent(m_GameObject, "OnAnimatorItemReloadCompleteSecondSlot", OnItemReloadCompleteSecondSlot);
            } else if (slotID == 2) {
                EventHandler.RegisterEvent(m_GameObject, "OnAnimatorItemReloadThirdSlot", OnItemReloadThirdSlot);
                EventHandler.RegisterEvent(m_GameObject, "OnAnimatorItemReloadCompleteThirdSlot", OnItemReloadCompleteThirdSlot);
            } else if (slotID != -1) {
                Debug.LogError("Error: The Reload ability does not listen to slot " + m_SlotID);
            }
        }

        /// <summary>
        /// Unregisters from the interested events according to the slot id.
        /// </summary>
        /// <param name="slotID">The slot id to unregister from.</param>
        private void UnregisterSlotEvents(int slotID)
        {
            if (slotID == 0) {
                EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorItemReloadFirstSlot", OnItemReloadFirstSlot);
                EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorItemReloadCompleteFirstSlot", OnItemReloadCompleteFirstSlot);
            } else if (slotID == 1) {
                EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorItemReloadSecondSlot", OnItemReloadSecondSlot);
                EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorItemReloadCompleteSecondSlot", OnItemReloadCompleteSecondSlot);
            } else if (slotID == 2) {
                EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorItemReloadThirdSlot", OnItemReloadThirdSlot);
                EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorItemReloadCompleteThirdSlot", OnItemReloadCompleteThirdSlot);
            }
        }

        /// <summary>
        /// Can the item be reloaded?
        /// </summary>
        /// <returns>True if the item can be reloaded.</returns>
        public override bool CanStartAbility()
        {
            // An attribute may prevent the ability from starting.
            if (!base.CanStartAbility()) {
                return false;
            }

            var canReload = false;
            // If the SlotID is -1 then the ability should reload every equipped item at the same time. If only one slot has a ReloadableItem then the 
            // ability can start. If the SlotID is not -1 then the ability should reload the item in the specified slot.
            if (m_SlotID == -1) {
                for (int i = 0; i < m_ReloadableItems.Length; ++i) {
                    var item = m_Inventory.GetActiveItem(i);
                    if (item == null) {
                        continue;
                    }

                    var itemAction = item.GetItemAction(m_ActionID);
                    if (itemAction == null) {
                        Debug.LogWarning("Warning: The item " + item.name + " must have an ItemAction component attached to it in order to be reloaded.");
                        continue;
                    }

                    m_ReloadableItems[i] = itemAction as IReloadableItem;
                    // The item can't be reloaded if it isn't a reloadable item.
                    if (m_ReloadableItems[i] != null && m_ReloadableItems[i].CanReloadItem(true)) {
                        canReload = true;
                    } else {
                        // The ability should not attempt to reload the item if IReloadableItem says that it cannot reload.
                        m_ReloadableItems[i] = null;
                    }
                }
            } else {
                var item = m_Inventory.GetActiveItem(m_SlotID);
                if (item != null) {
                    var itemAction = item.GetItemAction(m_ActionID);
                    if (itemAction == null) {
                        Debug.LogWarning("Warning: The item " + item.name + " must have an ItemAction component attached to it in order to be used.");
                    } else {
                        m_ReloadableItems[0] = itemAction as IReloadableItem;
                        canReload = m_ReloadableItems[0] != null && m_ReloadableItems[0].CanReloadItem(true);
                    }
                }
            }

            return canReload;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted(false);

            for (int i = 0; i < m_ReloadableItems.Length; ++i) {
                if (m_ReloadableItems[i] != null) {
                    m_Reloaded[i] = false;
                    m_ReloadableItems[i].StartItemReload();
                    if (!m_ReloadableItems[i].ReloadEvent.WaitForAnimationEvent) {
                        m_ReloadEvents[i] = Scheduler.ScheduleFixed(m_ReloadableItems[i].ReloadEvent.Duration, ReloadItem, i);
                    }
                }
            }
        }

        /// <summary>
        /// Stops reloading the item in the specified slot.
        /// </summary>
        /// <param name="slotID">The ID of the slot to stop reloading the item at.</param>
        public void StopItemReload(int slotID)
        {
            if (m_ReloadableItems[slotID] == null) {
                return;
            }

            m_ReloadableItems[slotID].ItemReloadComplete(false, false);
            m_ReloadableItems[slotID] = null;
            m_Reloaded[slotID] = false;
            Scheduler.Cancel(m_ReloadEvents[slotID]);
            m_ReloadEvents[slotID] = null;
            m_CharacterLocomotion.UpdateItemAbilityAnimatorParameters();

            // The ability won't be active if CanStartAbility filled in the ReloadableItem but the ability hasn't started yet.
            if (!IsActive) {
                return;
            }
            // The ability should stop if no more items can be reloaded.
            var canStop = true;
            for (int i = 0; i < m_ReloadableItems.Length; ++i) {
                if (m_ReloadableItems[i] != null) {
                    canStop = false;
                }
            }
            if (canStop) {
                StopAbility(true);
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
            if (base.ShouldBlockAbilityStart(startingAbility)) {
                return true;
            }
            if (startingAbility is Use) {
                // The ability should be able to be used unless the dominant item state doesn't match. This will prevent a secondary grenade throw 
                // from being started when the primary item is being used. It will not prevent two independent items from being used at the same time.
                var dominantItem = true;
                for (int i = 0; i < m_ReloadableItems.Length; ++i) {
                    if (m_ReloadableItems[i] == null) {
                        continue;
                    }

                    if (!m_ReloadableItems[i].Item.DominantItem) {
                        dominantItem = false;
                        break;
                    }
                }

                var useAbility = startingAbility as Use;
                for (int i = 0; i < useAbility.UsableItems.Length; ++i) {
                    if (useAbility.UsableItems[i] == null) {
                        continue;
                    }

                    if (dominantItem != useAbility.UsableItems[i].Item.DominantItem) {
                        return true;
                    }
                }
            }
            // Equip/Unequip cannot be active at the same time as Reload.
            return startingAbility is EquipUnequip;
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
                if (m_ReloadableItems[slotID] != null) {
                    return m_ItemStateIndex;
                }
            } else if (m_SlotID == slotID && m_ReloadableItems[0] != null) {
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
                if (m_ReloadableItems[slotID] != null) {
                    if (m_Reloaded[slotID]) {
                        return 0;
                    }
                    return m_ReloadableItems[slotID].ReloadAnimatorAudioStateSet.GetItemSubstateIndex();
                }
            } else if (m_SlotID == slotID && m_ReloadableItems[0] != null) {
                if (m_Reloaded[0]) {
                    return 0;
                }
                return m_ReloadableItems[0].ReloadAnimatorAudioStateSet.GetItemSubstateIndex();
            }

            return -1;
        }

        /// <summary>
        /// The animation has reloaded all of the items.
        /// </summary>
        private void OnItemReload()
        {
            for (int i = 0; i < m_ReloadableItems.Length; ++i) {
                if (m_ReloadableItems[i] != null && !m_Reloaded[i]) {
                    ReloadItem(i);
                }
            }
        }

        /// <summary>
        /// The animation has reloaded the first item slot.
        /// </summary>
        private void OnItemReloadFirstSlot()
        {
            ReloadItem(0);
        }

        /// <summary>
        /// The animation has reloaded the second item slot.
        /// </summary>
        private void OnItemReloadSecondSlot()
        {
            ReloadItem(1);
        }

        /// <summary>
        /// The animation has reloaded the third item slot.
        /// </summary>
        private void OnItemReloadThirdSlot()
        {
            ReloadItem(2);
        }

        /// <summary>
        /// The animation has reloaded the item.
        /// </summary>
        /// <param name="slotID">The slot that is reloading the item.</param>
        private void ReloadItem(int slotID)
        {
            var reloadableItem = m_ReloadableItems[slotID];
            if (reloadableItem == null) {
                return;
            }

            reloadableItem.ReloadItem(false);
            
            // Each reload should update the attribute.
            if (m_AttributeModifier != null) {
                m_AttributeModifier.EnableModifier(true);
            }

            var canReload = true;
            // An attribute may prevent the reload from being able to continue.
            if (m_AttributeModifier != null && !m_AttributeModifier.IsValid()) {
                canReload = false;
            }


            // The item may need to be reloaded again if the reload type is single and the inventory still has ammo.
            if (canReload && reloadableItem.CanReloadItem(true)) {
                m_CharacterLocomotion.UpdateItemAbilityAnimatorParameters();
                if (!reloadableItem.ReloadEvent.WaitForAnimationEvent) {
                    m_ReloadEvents[slotID] = Scheduler.ScheduleFixed(reloadableItem.ReloadEvent.Duration, ReloadItem, slotID);
                }
            } else {
                m_Reloaded[slotID] = true;
                m_CharacterLocomotion.UpdateItemAbilityAnimatorParameters();
                // The reload ability isn't done until the ReloadItemComplete method is called.
                if (!reloadableItem.ReloadCompleteEvent.WaitForAnimationEvent) {
                    m_ReloadEvents[slotID] = Scheduler.ScheduleFixed(reloadableItem.ReloadCompleteEvent.Duration, ReloadItemComplete, slotID);
                }
            }
        }

        /// <summary>
        /// The reload animation has completed for all of the items.
        /// </summary>
        private void OnItemReloadComplete()
        {
            for (int i = 0; i < m_ReloadableItems.Length; ++i) {
                if (m_ReloadableItems[i] != null) {
                    ReloadItemComplete(i);
                }
            }
        }

        /// <summary>
        /// The reload animation has completed for the first item slot.
        /// </summary>
        private void OnItemReloadCompleteFirstSlot()
        {
            ReloadItemComplete(0);
        }

        /// <summary>
        /// The reload animation has completed for the second item slot.
        /// </summary>
        private void OnItemReloadCompleteSecondSlot()
        {
            ReloadItemComplete(1);
        }

        /// <summary>
        /// The reload animation has completed for the third item slot.
        /// </summary>
        private void OnItemReloadCompleteThirdSlot()
        {
            ReloadItemComplete(2);
        }

        /// <summary>
        /// The animator has finished playing the reload animation.
        /// </summary>
        /// <param name="slotID">The slot that is reloading the item.</param>
        private void ReloadItemComplete(int slotID)
        {
            var reloadableItem = m_ReloadableItems[slotID];
            if (reloadableItem == null) {
                return;
            }

            m_ReloadableItems[slotID].ItemReloadComplete(true, false);
            m_ReloadableItems[slotID] = null;
            if (m_ReloadEvents[slotID] != null) {
                Scheduler.Cancel(m_ReloadEvents[slotID]);
                m_ReloadEvents[slotID] = null;
            }

            // Don't stop the ability unless all slots have been reloaded.
            var stopAbility = true;
            for (int i = 0; i < m_ReloadableItems.Length; ++i) {
                if (m_ReloadableItems[i] != null) {
                    stopAbility = false;
                    break;
                }
            }
            if (stopAbility) {
                StopAbility();
            }
        }

        /// <summary>
        /// The ItemPickup component is starting to pick up ItemIdentifier.
        /// </summary>
        private void OnStartPickup()
        {
            // Remember the initial item inventory list to be able to determine if an item has been added.
            m_InventoryItems.Clear();
            var allItems = m_Inventory.GetAllItems();
            for (int i = 0; i < allItems.Count; ++i) {
                if (m_Inventory.GetItemIdentifierAmount(allItems[i].ItemIdentifier) == 0) {
                    continue;
                }
                m_InventoryItems.Add(allItems[i]);
            }
            m_EquippedItems.Clear();
            Item item;
            for (int i = 0; i < m_Inventory.SlotCount; ++i) {
                if ((item = m_Inventory.GetActiveItem(i)) != null) {
                    m_EquippedItems.Add(item);
                }
            }
        }

        /// <summary>
        /// An ItemIdentifier has been picked up within the inventory.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier that has been equipped.</param>
        /// <param name="amount">The amount of ItemIdentifier picked up.</param>
        /// <param name="immediatePickup">Was the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        private void OnPickupItemIdentifier(IItemIdentifier itemIdentifier, int amount, bool immediatePickup, bool forceEquip)
        {
            // Determine if the equipped item should be reloaded.
            OnTryReload(-1, itemIdentifier, immediatePickup, true);
        }

        /// <summary>
        /// Tries the reload the item with the specified ItemIdentifier.
        /// </summary>
        /// <param name="slotID">The SlotID of the item trying to reload.</param>
        /// <param name="itemIdentifier">The ItemIdentifier which should be reloaded.</param>
        /// <param name="immediateReload">Should the item be reloaded immediately?</param>
        /// <param name="equipCheck">Should the equipped items be checked.</param>
        private void OnTryReload(int slotID, IItemIdentifier itemIdentifier, bool immediateReload, bool equipCheck)
        {
            if (m_SlotID != -1 && slotID != -1 && m_SlotID != slotID) {
                return;
            }

            var allItems = m_Inventory.GetAllItems();
            var canReloadCount = 0;
            for (int i = 0; i < allItems.Count; ++i) {
                var item = allItems[i];
                if (slotID != -1 && item.SlotID != slotID) {
                    continue;
                }

                IReloadableItem reloadableItem;
                if ((reloadableItem = ShouldReload(item, itemIdentifier, slotID == -1)) != null) { // -1 indicates that the item is being picked up.
                    if (m_CanReloadItems == null || m_CanReloadItems.Length == canReloadCount) {
                        System.Array.Resize(ref m_CanReloadItems, canReloadCount + 1);
                    }
                    m_CanReloadItems[canReloadCount] = reloadableItem;
                    canReloadCount++;
                }
            }

            if (canReloadCount > 0) {
                var startAbility = false;
                for (int i = 0; i < canReloadCount; ++i) {
                    var reloadableItem = m_CanReloadItems[i];
                    // The item should automatically be reloaded if:
                    // - The item is being reloaded automatically.
                    // - The item isn't currently equipped. Non-equipped items don't need to play an animation.
                    if (immediateReload || (equipCheck && !m_EquippedItems.Contains(reloadableItem.Item))) {
                        reloadableItem.ReloadItem(true);
                        reloadableItem.ItemReloadComplete(true, immediateReload);
                    } else {
                        startAbility = true;
                        if (m_SlotID == -1) {
                            m_ReloadableItems[reloadableItem.Item.SlotID] = reloadableItem;
                        } else {
                            m_ReloadableItems[0] = reloadableItem;
                        }
                    }
                }
                if (startAbility) {
                    StartAbility();
                }
            }
        }

        /// <summary>
        /// Should the item be reloaded? An IReloadableItem reference will be returned if the item can be reloaded.
        /// </summary>
        /// <param name="item">The item which may need to be reloaded.</param>
        /// <param name="itemIdentifier">The ItemIdentifier that is being reloaded.</param>
        /// <param name="fromPickup">Is the item being reloaded from a pickup?</param>
        /// <returns>A reference to the IReloadableItem if the item can be reloaded. Null if the item cannot be reloaded.</returns>
        private IReloadableItem ShouldReload(Item item, IItemIdentifier itemIdentifier, bool fromPickup)
        {
            var itemAction = item.GetItemAction(m_ActionID);

            // Don't reload if the item isn't a IReloadableItem.
            var reloadableItem = itemAction as IReloadableItem;
            if (reloadableItem == null) {
                return null;
            }

            // Don't reload if the ItemIdentifier doesn't match.
            if (reloadableItem.GetReloadableItemIdentifier() != itemIdentifier) {
                return null;
            }

            var autoReload = false;
            if ((reloadableItem.AutoReload & AutoReloadType.Empty) != 0 && (reloadableItem is IUsableItem && (reloadableItem as IUsableItem).GetConsumableItemIdentifierAmount() == 0)) {
                // The item is empty.
                autoReload = true;
            } else if ((reloadableItem.AutoReload & AutoReloadType.Pickup) != 0 && fromPickup) {
                // The item was just picked up for the first time.
                autoReload = true;
            }

            // Don't automatically reload if the item says that it shouldn't.
            if (!autoReload) {
                return null;
            }

            // Don't reload if the reloadable item can't reload.
            if (!reloadableItem.CanReloadItem(true)) {
                return null;
            }

            // Reload.
            return reloadableItem;
        }

        /// <summary>
        /// Can the camera zoom while the ability is active?
        /// </summary>
        /// <returns>True if the camera can zoom while the ability is active.</returns>
        public override bool CanCameraZoom()
        {
            for (int i = 0; i < m_ReloadableItems.Length; ++i) {
                if (m_ReloadableItems[i] != null && !m_ReloadableItems[i].CanCameraZoom) {
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

            // Ensure the arrays are set to null for the next run.
            for (int i = 0; i < m_ReloadableItems.Length; ++i) {
                if (m_ReloadableItems[i] != null) {
                    m_ReloadableItems[i].ItemReloadComplete(!force, force);
                    m_ReloadableItems[i] = null;
                    Scheduler.Cancel(m_ReloadEvents[i]);
                    m_ReloadEvents[i] = null;
                }
            }
        }

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent(m_GameObject, "OnItemPickupStartPickup", OnStartPickup);
            EventHandler.UnregisterEvent<IItemIdentifier, int, bool, bool>(m_GameObject, "OnInventoryPickupItemIdentifier", OnPickupItemIdentifier);
            EventHandler.UnregisterEvent<int, IItemIdentifier, bool, bool>(m_GameObject, "OnItemTryReload", OnTryReload);
            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorItemReload", OnItemReload);
            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorItemReloadComplete", OnItemReloadComplete);
            UnregisterSlotEvents(m_SlotID);
        }
    }
}