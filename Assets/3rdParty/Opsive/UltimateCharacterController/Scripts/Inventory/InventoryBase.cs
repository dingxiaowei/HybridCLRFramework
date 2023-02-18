/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using Opsive.Shared.Events;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.Shared.Game;
#endif
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Events;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Items.Actions;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.UltimateCharacterController.Networking;
    using Opsive.UltimateCharacterController.Networking.Character;
#endif
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Provides a common base class for any character Inventory.
    /// </summary>
    public abstract class InventoryBase : MonoBehaviour
    {
        [Tooltip("Should all of the IItemIdentifier be removed when the character dies?")]
        [SerializeField] protected bool m_RemoveAllOnDeath = true;
        [Tooltip("Should the default loadout be loaded when the character respawns?")]
        [SerializeField] protected bool m_LoadDefaultLoadoutOnRespawn = true;
        [Tooltip("The name of the state when the inventory is unequipped.")]
        [SerializeField] protected string m_UnequippedStateName = "Unequipped";
        [Tooltip("Unity event that is invoked when an item is initially added to the inventory.")]
        [SerializeField] protected UnityItemEvent m_OnAddItemEvent;
        [Tooltip("Unity event that is invoked when an IItemIdentifier is picked up.")]
        [SerializeField] protected UnityItemIdentifierFloatBoolBoolEvent m_OnPickupItemIdentifierEvent;
        [Tooltip("Unity event that is invoked when an item is picked up.")]
        [SerializeField] protected UnityItemFloatBoolBoolEvent m_OnPickupItemEvent;
        [Tooltip("Unity event that is invoked when an item is equipped.")]
        [SerializeField] protected UnityItemIntEvent m_OnEquipItemEvent;
        [Tooltip("Unity event that is invoked when an IItemIdentifier is adjusted.")]
        [SerializeField] protected UnityItemIdentifierFloatEvent m_OnAdjustItemIdentifierAmountEvent;
        [Tooltip("Unity event that is invoked when an item is unequipped.")]
        [SerializeField] protected UnityItemIntEvent m_OnUnequipItemEvent;
        [Tooltip("Unity event that is invoked when an item is removed.")]
        [SerializeField] protected UnityItemIntEvent m_OnRemoveItemEvent;

        public bool RemoveAllOnDeath { get { return m_RemoveAllOnDeath; } set { m_RemoveAllOnDeath = value; } }
        public bool LoadDefaultLoadoutOnRespawn { get { return m_LoadDefaultLoadoutOnRespawn; } set { m_LoadDefaultLoadoutOnRespawn = value; } }
        public string UnequippedStateName { get { return m_UnequippedStateName; } set { m_UnequippedStateName = value; } }
        public UnityItemEvent OnAddItemEvent { get { return m_OnAddItemEvent; } set { m_OnAddItemEvent = value; } }
        public UnityItemIdentifierFloatBoolBoolEvent OnPickupItemIdentifierEvent { get { return m_OnPickupItemIdentifierEvent; } set { m_OnPickupItemIdentifierEvent = value; } }
        public UnityItemFloatBoolBoolEvent OnPickupItemEvent { get { return m_OnPickupItemEvent; } set { m_OnPickupItemEvent = value; } }
        public UnityItemIntEvent OnEquipItemEvent { get { return m_OnEquipItemEvent; } set { m_OnEquipItemEvent = value; } }
        public UnityItemIdentifierFloatEvent OnAdjustItemIdentifierAmountEvent { get { return m_OnAdjustItemIdentifierAmountEvent; } set { m_OnAdjustItemIdentifierAmountEvent = value; } }
        public UnityItemIntEvent OnUnequipItemEvent { get { return m_OnUnequipItemEvent; } set { m_OnUnequipItemEvent = value; } }
        public UnityItemIntEvent OnRemoveItemEvent { get { return m_OnRemoveItemEvent; } set { m_OnRemoveItemEvent = value; } }

        protected GameObject m_GameObject;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        private INetworkInfo m_NetworkInfo;
        private INetworkCharacter m_NetworkCharacter;
#endif

        protected int m_SlotCount = 1;
        private List<Item> m_AllItems = new List<Item>();
        private List<IItemIdentifier> m_AllItemIdentifiers = new List<IItemIdentifier>();

        public int SlotCount { get {
#if UNITY_EDITOR
                if (!Application.isPlaying) { DetermineSlotCount(); }
#endif
                return m_SlotCount;
            } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected virtual void Awake()
        {
            m_GameObject = gameObject;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            m_NetworkInfo = m_GameObject.GetCachedComponent<INetworkInfo>();
            m_NetworkCharacter = m_GameObject.GetCachedComponent<INetworkCharacter>();
#endif

            DetermineSlotCount();

            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_GameObject, "OnRespawn", OnRespawn);
        }

        /// <summary>
        /// Determines the number of slots on the character.
        /// </summary>
        public void DetermineSlotCount()
        {
            // The number of slots depends on the maximum number of ItemSlot IDs.
            var itemSlots = GetComponentsInChildren<ItemSlot>(true);
            for (int i = 0; i < itemSlots.Length; ++i) {
                if (m_SlotCount <= itemSlots[i].ID) {
                    m_SlotCount = itemSlots[i].ID + 1;
                }
            }
        }

        /// <summary>
        /// Loads the default loadout.
        /// </summary>
        protected virtual void Start()
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo == null || m_NetworkInfo.IsLocalPlayer()) {
                if (m_NetworkInfo != null) {
                    // Load the default loadout on the network first to ensure it is received before any equip events.
                    m_NetworkCharacter.LoadDefaultLoadout();
                }
#endif
                LoadDefaultLoadout();
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            }
#endif

            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterSnapAnimator");
        }

        /// <summary>
        /// Pick up each ItemIdentifier within the DefaultLoadout.
        /// </summary>
        public abstract void LoadDefaultLoadout();

        /// <summary>
        /// Determines if the character has the specified item.
        /// </summary>
        /// <param name="item">The item to check against.</param>
        /// <returns>True if the character has the item.</returns>
        public bool HasItem(Item item) { return HasItemInternal(item); }

        /// <summary>
        /// Internal method which determines if the character has the specified item.
        /// </summary>
        /// <param name="item">The item to check against.</param>
        /// <returns>True if the character has the item.</returns>
        protected abstract bool HasItemInternal(Item item);

        /// <summary>
        /// Adds the item to the inventory. This does not add the actual ItemIdentifier - PickupItem does that.
        /// </summary>
        /// <param name="item">The Item to add.</param>
        /// <param name="immediateEquip">Can the item be equipped immediately?</param>
        /// <param name="forcePickup">Should the item be force equipped?</param>
        public void AddItem(Item item, bool immediateEquip, bool forceEquip)
        {
            if (AddItemInternal(item)) {
                m_AllItems.Add(item);

                // Notify those interested that an item has been added.
                EventHandler.ExecuteEvent(m_GameObject, "OnInventoryAddItem", item);
                if (m_OnAddItemEvent != null) {
                    m_OnAddItemEvent.Invoke(item);
                }

                // The ItemIdentifier event should also be called in cases where the amount is greater than 0.
                // This allows the ItemIdentifier to be picked up before the item has been added.
                if (GetItemIdentifierAmount(item.ItemIdentifier) > 0) {
                    ItemIdentifierPickedUp(item.ItemIdentifier, 1, item.SlotID, immediateEquip, forceEquip);
                }
            }
        }

        /// <summary>
        /// Internal method which adds the item to the Inventory. This does not add the actual IItemIdentifier - PickupItem does that.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>True if the item was added to the inventory.</returns>
        protected abstract bool AddItemInternal(Item item);

        /// <summary>
        /// Adds the specified amount of the ItemIdentifier to the inventory.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to add.</param>
        /// <param name="amount">The amount of ItemIdentifier to add.</param>
        /// <param name="slotID">The slot ID that picked up the item. A -1 value will indicate no specified slot.</param>
        /// <param name="immediatePickup">Should the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        /// <returns>True if the ItemIdentifier was picked up.</returns>
        public bool Pickup(IItemIdentifier itemIdentifier, int amount, int slotID, bool immediatePickup, bool forceEquip)
        {
            return Pickup(itemIdentifier, amount, slotID, immediatePickup, forceEquip, true);
        }

        /// <summary>
        /// Adds the specified amount of the ItemIdentifier to the inventory.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to add.</param>
        /// <param name="amount">The amount of ItemIdentifier to add.</param>
        /// <param name="slotID">The slot ID that picked up the item. A -1 value will indicate no specified slot.</param>
        /// <param name="immediatePickup">Should the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        /// <param name="notifyOnPickup">Should other objects be notified that the ItemIdentifier was picked up?</param>
        /// <returns>True if the ItemIdentifier was picked up.</returns>
        public bool Pickup(IItemIdentifier itemIdentifier, int amount, int slotID, bool immediatePickup, bool forceEquip, bool notifyOnPickup)
        {
            // Prevent pickup when the inventory isn't enabled.
            if (itemIdentifier == null || !enabled || amount == 0) {
                return false;
            }

            var pickedUp = PickupInternal(itemIdentifier, amount);

            // Notify those interested that an item has been picked up.
            if (pickedUp && notifyOnPickup) {
                if (slotID == -1) {
                    // Find the slot that the item belongs to (if any).
                    for (int i = 0; i < m_SlotCount; ++i) {
                        if (GetItem(itemIdentifier, i) != null) {
                            ItemIdentifierPickedUp(itemIdentifier, amount, i, immediatePickup, forceEquip);
                            slotID = i;
                        }
                    }
                    if (slotID == -1) {
                        // The ItemIdentifier doesn't correspond to an item so execute the event once.
                        ItemIdentifierPickedUp(itemIdentifier, amount, -1, immediatePickup, forceEquip);
                    }
                } else {
                    ItemIdentifierPickedUp(itemIdentifier, amount, slotID, immediatePickup, forceEquip);
                }

                // If the slot ID isn't -1 then AddItem has already run. Add the item if it hasn't already been added. This will occur if the item is removed
                // and then later added again.
                if (slotID != -1) {
                    var item = GetItem(itemIdentifier, slotID);
                    if (item != null && !m_AllItems.Contains(item)) {
                        m_AllItems.Add(item);
                    }
                }
            }
            return pickedUp;
        }

        /// <summary>
        /// Internal method which adds the specified amount of the ItemIdentifier to the inventory.
        /// </summary>
        /// <param name="itemIdentifier">The IItemIdentifier to add.</param>
        /// <param name="amount">The amount of ItemIdentifier to add.</param>
        /// <returns>True if the ItemIdentifier was picked up successfully.</returns>
        protected abstract bool PickupInternal(IItemIdentifier itemIdentifier, int amount);

        /// <summary>
        /// The ItemIdentifier has been picked up. Notify interested objects.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier that was picked up.</param>
        /// <param name="amount">The number of ItemIdentifier picked up.</param>
        /// <param name="slotID">The ID of the slot which the item belongs to.</param>
        /// <param name="immediatePickup">Was the item be picked up immediately?</param>
        /// <param name="forcePickup">Should the item be force equipped?</param>
        protected void ItemIdentifierPickedUp(IItemIdentifier itemIdentifier, int amount, int slotID, bool immediatePickup, bool forceEquip)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.IsLocalPlayer()) {
                m_NetworkCharacter.ItemIdentifierPickup(itemIdentifier.ID, amount, slotID, immediatePickup, forceEquip);
            }
#endif

            EventHandler.ExecuteEvent(m_GameObject, "OnInventoryPickupItemIdentifier", itemIdentifier, amount, immediatePickup, forceEquip);
            if (m_OnPickupItemIdentifierEvent != null) {
                m_OnPickupItemIdentifierEvent.Invoke(itemIdentifier, amount, immediatePickup, forceEquip);
            }

            if (slotID != -1) {
                var item = GetItem(itemIdentifier, slotID);
                if (item != null) {
                    item.Pickup();

                    EventHandler.ExecuteEvent(m_GameObject, "OnInventoryPickupItem", item, amount, immediatePickup, forceEquip);
                    if (m_OnPickupItemEvent != null) {
                        m_OnPickupItemEvent.Invoke(item, amount, immediatePickup, forceEquip);
                    }
                }
            }
            if (!m_AllItemIdentifiers.Contains(itemIdentifier)) {
                m_AllItemIdentifiers.Add(itemIdentifier);
            }
        }

        /// <summary>
        /// Returns the active item in the specified slot.
        /// </summary>
        /// <param name="slotID">The ID of the slot.</param>
        /// <returns>The active item which occupies the specified slot. Can be null.</returns>
        public Item GetActiveItem(int slotID) { return GetActiveItemInternal(slotID); }

        /// <summary>
        /// Internal method which returns the active item in the specified slot.
        /// </summary>
        /// <param name="slotID">The ID of the slot which the item belongs to.</param>
        /// <returns>The active item which occupies the specified slot. Can be null.</returns>
        protected abstract Item GetActiveItemInternal(int slotID);

        /// <summary>
        /// Returns the item that corresponds to the specified ItemIdentifier.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier of the item.</param>
        /// <param name="slotID">The ID of the slot which the item belongs to.</param>
        /// <returns>The item which occupies the specified slot. Can be null.</returns>
        public Item GetItem(IItemIdentifier itemIdentifier, int slotID) { return GetItemInternal(itemIdentifier, slotID); }

        /// <summary>
        /// Internal method which returns the item that corresponds to the specified IItemIdentifier.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier of the item.</param>
        /// <param name="slotID">The ID of the slot which the item belongs to.</param>
        /// <returns>The item which occupies the specified slot. Can be null.</returns>
        protected abstract Item GetItemInternal(IItemIdentifier itemIdentifier, int slotID);

        /// <summary>
        /// Returns a list of all of the items in the inventory.
        /// </summary>
        /// <returns>A list of all of the items in the inventory.</returns>
        public List<Item> GetAllItems() { return m_AllItems; }

        /// <summary>
        /// Returns a list of all of the ItemIdentifier in the inventory. Only used by the editor for the inventory inspector.
        /// </summary>
        /// <returns>A list of all of the ItemIdentifier in the inventory.</returns>
        public List<IItemIdentifier> GetAllItemIdentifiers() { return m_AllItemIdentifiers; }

        /// <summary>
        /// Equips the ItemIdentifier in the specified slot.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to equip.</param>
        /// <param name="slotID">The ID of the slot.</param>
        /// <param name="immediateEquip">Is the item being equipped immediately? Immediate equips will occur from the default loadout or quickly switching to the item.</param>
        public void EquipItem(IItemIdentifier itemIdentifier, int slotID, bool immediateEquip)
        {
            if (itemIdentifier == null) {
                return;
            }

            var currentItem = GetActiveItem(slotID);
            if (currentItem != null) {
                // Don't equip if the IItemIdentifier is already equipped.
                if (currentItem.ItemIdentifier == itemIdentifier) {
                    return;
                }
                UnequipItem(slotID);
            }

            var item = EquipItemInternal(itemIdentifier, slotID);
            if (item != null) {
                item.Equip(immediateEquip);

                // Notify those interested that an item has been equipped.
                EventHandler.ExecuteEvent(m_GameObject, "OnInventoryEquipItem", item, slotID);
                if (m_OnEquipItemEvent != null) {
                    m_OnEquipItemEvent.Invoke(item, slotID);
                }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                if (m_NetworkInfo != null && m_NetworkInfo.IsLocalPlayer()) {
                    m_NetworkCharacter.EquipUnequipItem(itemIdentifier.ID, slotID, true);
                }
#endif

                if (!string.IsNullOrEmpty(m_UnequippedStateName)) {
                    StateSystem.StateManager.SetState(m_GameObject, m_UnequippedStateName, false);
                }
            }
        }

        /// <summary>
        /// Internal method which equips the ItemIdentifier in the specified slot.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to equip.</param>
        /// <param name="slotID">The ID of the slot.</param>
        /// <returns>The item which corresponds to the ItemIdentifier. Can be null.</returns>
        protected abstract Item EquipItemInternal(IItemIdentifier itemIdentifier, int slotID);

        /// <summary>
        /// Unequips the specified ItemIdentifier in the specified slot.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to unequip. If the ItemIdentifier isn't currently equipped then no changes will be made.</param>
        /// <param name="slotID">The ID of the slot.</param>
        public void UnequipItem(IItemIdentifier itemIdentifier, int slotID)
        {
            // No need to unequip if the item is already unequipped or the ItemIdentifier don't match.
            var currentItem = GetActiveItem(slotID);
            if (currentItem == null || currentItem.ItemIdentifier != itemIdentifier) {
                return;
            }

            UnequipItem(slotID);
        }

        /// <summary>
        /// Unequips the item in the specified slot.
        /// </summary>
        /// <param name="slotID">The ID of the slot.</param>
        public void UnequipItem(int slotID)
        {
            // No need to unequip if the item is already unequipped.
            var currentItem = GetActiveItem(slotID);
            if (currentItem == null) {
                return;
            }

            var item = UnequipItemInternal(slotID);
            if (item != null) {
                item.Unequip();

                // Notify those interested that an item has been unequipped.
                EventHandler.ExecuteEvent(m_GameObject, "OnInventoryUnequipItem", item, slotID);
                if (m_OnUnequipItemEvent != null) {
                    m_OnUnequipItemEvent.Invoke(item, slotID);
                }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                if (m_NetworkInfo != null && m_NetworkInfo.IsLocalPlayer()) {
                    m_NetworkCharacter.EquipUnequipItem(item.ItemIdentifier.ID, slotID, false);
                }
#endif

                // Optionally enable a state when the inventory is unequipped.
                if (!string.IsNullOrEmpty(m_UnequippedStateName)) {
                    var unequipped = true;
                    for (int i = 0; i < m_SlotCount; ++i) {
                        if (i == slotID) {
                            continue;
                        }

                        if (GetActiveItem(i) != null) {
                            unequipped = false;
                        } 
                    }
                    if (unequipped) {
                        StateSystem.StateManager.SetState(m_GameObject, m_UnequippedStateName, true);
                    }
                }
            }
        }

        /// <summary>
        /// Internal method which unequips the item in the specified slot.
        /// </summary>
        /// <param name="slotID">The ID of the slot.</param>
        /// <returns>The item that was unequipped.</returns>
        protected abstract Item UnequipItemInternal(int slotID);

        /// <summary>
        /// Returns the amount of the specified ItemIdentifier.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to get the amount of.</param>
        /// <returns>The amount of the specified ItemIdentifier.</returns>
        public int GetItemIdentifierAmount(IItemIdentifier itemIdentifier) { if (itemIdentifier == null) { return 0; } return GetItemIdentifierAmountInternal(itemIdentifier); }

        /// <summary>
        /// Internal method which returns the amount of the specified ItemIdentifier.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to get the amount of.</param>
        /// <returns>The amount of the specified ItemIdentifier.</returns>
        protected abstract int GetItemIdentifierAmountInternal(IItemIdentifier itemIdentifier);

        /// <summary>
        /// Adjusts the amount of the specified ItemIdentifier.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to adjust.</param>
        /// <param name="amount">The amount of ItemIdentifier to adjust.</param>
        public void AdjustItemIdentifierAmount(IItemIdentifier itemIdentifier, int amount)
        {
            if (itemIdentifier == null || amount == 0) {
                return;
            }

            AdjustItemIdentifierAmountInternal(itemIdentifier, amount);

            // Notify those interested that an item has been adjusted.
            var remaining = GetItemIdentifierAmount(itemIdentifier);
            EventHandler.ExecuteEvent(m_GameObject, "OnInventoryAdjustItemIdentifierAmount", itemIdentifier, remaining);
            if (m_OnAdjustItemIdentifierAmountEvent != null) {
                m_OnAdjustItemIdentifierAmountEvent.Invoke(itemIdentifier, remaining);
            }
        }

        /// <summary>
        /// Internal method which adjusts the amount of the specified ItemIdentifier.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to adjust.</param>
        /// <param name="amount">The amount of ItemIdentifier to adjust.</param>
        protected abstract void AdjustItemIdentifierAmountInternal(IItemIdentifier itemIdentifier, int amount);

        /// <summary>
        /// Removes the ItemIdentifier from the inventory.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to remove.</param>
        /// <param name="slotID">The ID of the slot.</param>
        /// <param name="amount">The amount of the ItemIdentnfier that should be removed.</param>
        /// <param name="drop">Should the item be dropped when removed?</param>
        public void RemoveItem(IItemIdentifier itemIdentifier, int slotID, int amount, bool drop)
        {
            var item = GetItem(itemIdentifier, slotID);

            if (item != null) {
                // The item should be dropped before unequipped so the drop position will be correct.
                if (drop) {
                    item.Drop(amount, false);
                }

                // An equipped item needs to be unequipped.
                UnequipItem(itemIdentifier, slotID);

                // If the item isn't dropped then it is removed immediately.
                if (!drop) {
                    item.Remove();
                }

                if (item.DropConsumableItems) {
                    var itemActions = item.ItemActions;
                    if (itemActions != null) {
                        IUsableItem usableItem;
                        IItemIdentifier consumableItemIdentifier;
                        for (int i = 0; i < itemActions.Length; ++i) {
                            if (((usableItem = itemActions[i] as IUsableItem) != null) && (consumableItemIdentifier = usableItem.GetConsumableItemIdentifier()) != null) {
                                usableItem.RemoveConsumableItemIdentifierAmount();

                                // Any consumable ItemIdentifier should also be removed if there are no more of the same items remaining.
                                if (GetItemIdentifierAmount(itemIdentifier) == 1) {
                                    RemoveItemIdentifierInternal(consumableItemIdentifier, slotID, amount);
                                    m_AllItemIdentifiers.Remove(consumableItemIdentifier);
                                }

                                // Notify those interested of the removed amount.
                                SendItemIdentifierAdjustmentEvents(consumableItemIdentifier);
                            }
                        }
                    }
                }
                m_AllItems.Remove(item);
            }

            // The ItemIdentifier should be removed from the inventory.
            RemoveItemIdentifierInternal(itemIdentifier, slotID, amount);
            if (GetItemIdentifierAmount(itemIdentifier) == 0) {
                m_AllItemIdentifiers.Remove(itemIdentifier);
            }

            // Notify those interested that the item will be removed.
            if (item != null) {
                EventHandler.ExecuteEvent(m_GameObject, "OnInventoryRemoveItem", item, slotID);
                if (m_OnRemoveItemEvent != null) {
                    m_OnRemoveItemEvent.Invoke(item, slotID);
                }
            } else {
                SendItemIdentifierAdjustmentEvents(itemIdentifier);
            }
        }

        /// <summary>
        /// Sends the ItemIdentifier adjustment events.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to remove.</param>
        private void SendItemIdentifierAdjustmentEvents(IItemIdentifier itemIdentifier)
        {
            // Notify those interested of the removed amount.
            var amount = GetItemIdentifierAmount(itemIdentifier);
            EventHandler.ExecuteEvent(m_GameObject, "OnInventoryAdjustItemIdentifierAmount", itemIdentifier, amount);
            if (m_OnAdjustItemIdentifierAmountEvent != null) {
                m_OnAdjustItemIdentifierAmountEvent.Invoke(itemIdentifier, amount);
            }
            if (amount == 0) {
                m_AllItemIdentifiers.Remove(itemIdentifier);
            }
        }

        /// <summary>
        /// Internal method which removes the ItemIdentifier from the inventory.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to remove.</param>
        /// <param name="slotID">The ID of the slot.</param>
        /// <param name="amount">The amount of the ItemIdentifier that should be removed.</param>
        protected abstract void RemoveItemIdentifierInternal(IItemIdentifier itemIdentifier, int slotID, int amount);

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            enabled = false;
            // The item's drop method will call RemoveItem within the inventory.
            if (m_RemoveAllOnDeath) {
                RemoveAllItems();

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                if (m_NetworkInfo != null && m_NetworkInfo.IsLocalPlayer()) {
                    m_NetworkCharacter.RemoveAllItems();
                }
#endif
            }
        }

        /// <summary>
        /// Removes all of the items from the inventory.
        /// </summary>
        public void RemoveAllItems()
        {
            var allItems = GetAllItems();
            for (int i = allItems.Count - 1; i >= 0; --i) {
                // Multiple items may be dropped at the same time.
                if (allItems.Count <= i) {
                    continue;
                }
                var itemIdentifier = allItems[i].ItemIdentifier;
                var slotID = allItems[i].SlotID;
                while (GetItemIdentifierAmount(itemIdentifier) > 0) {
                    RemoveItem(itemIdentifier, slotID, 1, true);
                }
            }
        }

        /// <summary>
        /// The character has respawned.
        /// </summary>
        private void OnRespawn()
        {
            enabled = true;
            if (m_LoadDefaultLoadoutOnRespawn) {
                LoadDefaultLoadout();
            }

            // Notify others that the inventory has respawned - allows EquipUnequip to equip any previously equipped items.
            EventHandler.ExecuteEvent(m_GameObject, "OnInventoryRespawned");
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.UnregisterEvent(m_GameObject, "OnRespawn", OnRespawn);
        }
    }
}
