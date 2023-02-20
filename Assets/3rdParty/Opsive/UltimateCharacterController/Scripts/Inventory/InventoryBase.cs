/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Items.Actions;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
using Opsive.UltimateCharacterController.Networking;
using Opsive.UltimateCharacterController.Networking.Character;
using Opsive.UltimateCharacterController.Utility;
#endif
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Inventory
{
    /// <summary>
    /// Provides a common base class for any character Inventory.
    /// </summary>
    public abstract class InventoryBase : MonoBehaviour
    {
        [Tooltip("Should all of the ItemTypes be removed when the character dies?")]
        [SerializeField] protected bool m_RemoveAllOnDeath = true;
        [Tooltip("Should the default loadout be loaded when the character respawns?")]
        [SerializeField] protected bool m_LoadDefaultLoadoutOnRespawn = true;
        [Tooltip("Items to load when the Inventory is initially created or on a character respawn.")]
        [SerializeField] protected ItemTypeCount[] m_DefaultLoadout;
        [Tooltip("Unity event that is invoked when an item is initially added to the inventory.")]
        [SerializeField] protected UnityItemEvent m_OnAddItemEvent;
        [Tooltip("Unity event that is invoked when an ItemType is picked up.")]
        [SerializeField] protected UnityItemTypeFloatBoolBoolEvent m_OnPickupItemTypeEvent;
        [Tooltip("Unity event that is invoked when an item is picked up.")]
        [SerializeField] protected UnityItemFloatBoolBoolEvent m_OnPickupItemEvent;
        [Tooltip("Unity event that is invoked when an item is equipped.")]
        [SerializeField] protected UnityItemIntEvent m_OnEquipItemEvent;
        [Tooltip("Unity event that is invoked when an ItemType is used.")]
        [SerializeField] protected UnityItemTypeFloatEvent m_OnUseItemTypeEvent;
        [Tooltip("Unity event that is invoked when an item is unequipped.")]
        [SerializeField] protected UnityItemIntEvent m_OnUnequipItemEvent;
        [Tooltip("Unity event that is invoked when an item is removed.")]
        [SerializeField] protected UnityItemIntEvent m_OnRemoveItemEvent;

        public bool RemoveAllOnDeath { get { return m_RemoveAllOnDeath; } set { m_RemoveAllOnDeath = value; } }
        public bool LoadDefaultLoadoutOnRespawn { get { return m_LoadDefaultLoadoutOnRespawn; } set { m_LoadDefaultLoadoutOnRespawn = value; } }
        public ItemTypeCount[] DefaultLoadout { get { return m_DefaultLoadout; } set { m_DefaultLoadout = value; } }
        public UnityItemEvent OnAddItemEvent { get { return m_OnAddItemEvent; } set { m_OnAddItemEvent = value; } }
        public UnityItemTypeFloatBoolBoolEvent OnPickupItemTypeEvent { get { return m_OnPickupItemTypeEvent; } set { m_OnPickupItemTypeEvent = value; } }
        public UnityItemFloatBoolBoolEvent OnPickupItemEvent { get { return m_OnPickupItemEvent; } set { m_OnPickupItemEvent = value; } }
        public UnityItemIntEvent OnEquipItemEvent { get { return m_OnEquipItemEvent; } set { m_OnEquipItemEvent = value; } }
        public UnityItemTypeFloatEvent OnUseItemTypeEvent { get { return m_OnUseItemTypeEvent; } set { m_OnUseItemTypeEvent = value; } }
        public UnityItemIntEvent OnUnequipItemEvent { get { return m_OnUnequipItemEvent; } set { m_OnUnequipItemEvent = value; } }
        public UnityItemIntEvent OnRemoveItemEvent { get { return m_OnRemoveItemEvent; } set { m_OnRemoveItemEvent = value; } }

        private GameObject m_GameObject;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        private INetworkInfo m_NetworkInfo;
        private INetworkCharacter m_NetworkCharacter;
#endif

        protected int m_SlotCount = 1;
        private List<Item> m_AllItems = new List<Item>();
#if UNITY_EDITOR
        private List<ItemType> m_AllItemTypes = new List<ItemType>();
#endif

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
        private void Start()
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
        /// Pick up each ItemType within the DefaultLoadout.
        /// </summary>
        public void LoadDefaultLoadout()
        {
            if (m_DefaultLoadout != null) {
                for (int i = 0; i < m_DefaultLoadout.Length; ++i) {
                    PickupItemType(m_DefaultLoadout[i].ItemType, m_DefaultLoadout[i].Count, -1, true, false);
                }
            }
        }

        /// <summary>
        /// Determines if the character has the specified item.
        /// </summary>
        /// <param name="item">The item to check against.</param>
        /// <returns>True if the character has the item.</returns>
        public bool HasItem(Item item) { return item != null && GetItem(item.SlotID, item.ItemType) != null; }

        /// <summary>
        /// Adds the item to the inventory. This does not add the actual ItemType - PickupItem does that.
        /// </summary>
        /// <param name="item">The Item to add.</param>
        /// <param name="immediateEquip">Can the item be equipped immediately?</param>
        public void AddItem(Item item, bool immediateEquip)
        {
            if (AddItemInternal(item)) {
                m_AllItems.Add(item);

                // Notify those interested that an item has been added.
                EventHandler.ExecuteEvent(m_GameObject, "OnInventoryAddItem", item);
                if (m_OnAddItemEvent != null) {
                    m_OnAddItemEvent.Invoke(item);
                }

                // The PickupItemType event should also be called in cases where the count is greater than 0.
                // This allows the ItemType to be picked up before the item has been added.
                float count;
                if ((count = GetItemTypeCount(item.ItemType)) > 0) {
                    item.Pickup();

                    EventHandler.ExecuteEvent(m_GameObject, "OnInventoryPickupItem", item, count, immediateEquip, false);
                    if (m_OnPickupItemEvent != null) {
                        m_OnPickupItemEvent.Invoke(item, count, immediateEquip, false);
                    }
                }
            }
        }

        /// <summary>
        /// Internal method which adds the item to the Inventory. This does not add the actual ItemType - PickupItem does that.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>True if the item was added to the inventory.</returns>
        protected abstract bool AddItemInternal(Item item);

        /// <summary>
        /// Adds the specified count of the ItemType to the inventory.
        /// </summary>
        /// <param name="itemType">The ItemType to add.</param>
        /// <param name="count">The amount of ItemType to add.</param>
        /// <param name="slotID">The slot ID that picked up the item. A -1 value will indicate no specified slot.</param>
        /// <param name="immediatePickup">Should the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        /// <returns>True if the ItemType was picked up.</returns>
        public bool PickupItemType(ItemType itemType, float count, int slotID, bool immediatePickup, bool forceEquip)
        {
            return PickupItemType(itemType, count, slotID, immediatePickup, forceEquip, true);
        }

        /// <summary>
        /// Adds the specified count of the ItemType to the inventory.
        /// </summary>
        /// <param name="itemType">The ItemType to add.</param>
        /// <param name="count">The amount of ItemType to add.</param>
        /// <param name="slotID">The slot ID that picked up the item. A -1 value will indicate no specified slot.</param>
        /// <param name="immediatePickup">Should the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        /// <param name="notifyOnPickup">Should other objects be notified that the ItemType was picked up?</param>
        /// <returns>True if the ItemType was picked up.</returns>
        public bool PickupItemType(ItemType itemType, float count, int slotID, bool immediatePickup, bool forceEquip, bool notifyOnPickup)
        {
            // Prevent pickup when the inventory isn't enabled.
            if (itemType == null || !enabled || count == 0) {
                return false;
            }

            var pickedUp = PickupItemTypeInternal(itemType, count);

            // Notify those interested that an item has been picked up.
            if (notifyOnPickup) {
                if (slotID == -1) {
                    // Find the slot that the item belongs to (if any).
                    for (int i = 0; i < m_SlotCount; ++i) {
                        if (GetItem(i, itemType) != null) {
                            ItemTypePickup(itemType, count, i, immediatePickup, forceEquip);
                            slotID = i;
                        }
                    }
                    if (slotID == -1) {
                        // The ItemType doesn't correspond to an item so execute the event once.
                        ItemTypePickup(itemType, count, -1, immediatePickup, forceEquip);
                    }
                } else {
                    ItemTypePickup(itemType, count, slotID, immediatePickup, forceEquip);
                }

                // If the slot ID isn't -1 then AddItem has already run. Add the item if it hasn't already been added. This will occur if the item is removed
                // and then later added again.
                if (slotID != -1) {
                    var item = GetItem(slotID, itemType);
                    if (item != null && !m_AllItems.Contains(item)) {
                        m_AllItems.Add(GetItem(slotID, itemType));
                    }
                }
            }
#if UNITY_EDITOR
            if (!m_AllItemTypes.Contains(itemType)) {
                m_AllItemTypes.Add(itemType);
            }
#endif
            return pickedUp;
        }

        /// <summary>
        /// The ItemType has been picked up. Notify interested objects.
        /// </summary>
        /// <param name="itemType">The ItemType that was picked up.</param>
        /// <param name="count">The number of ItemType picked up.</param>
        /// <param name="slotID">The ID of the slot which the item belongs to.</param>
        /// <param name="immediatePickup">Was the item be picked up immediately?</param>
        /// <param name="forcePickup">Should the item be force equipped?</param>
        private void ItemTypePickup(ItemType itemType, float count, int slotID, bool immediatePickup, bool forceEquip)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.IsLocalPlayer()) {
                m_NetworkCharacter.ItemTypePickup(itemType.ID, count, slotID, immediatePickup, forceEquip);
            }
#endif

            EventHandler.ExecuteEvent(m_GameObject, "OnInventoryPickupItemType", itemType, count, immediatePickup, forceEquip);
            if (m_OnPickupItemTypeEvent != null) {
                m_OnPickupItemTypeEvent.Invoke(itemType, count, immediatePickup, forceEquip);
            }

            if (slotID != -1) {
                var item = GetItem(slotID, itemType);
                if (item != null) {
                    item.Pickup();

                    EventHandler.ExecuteEvent(m_GameObject, "OnInventoryPickupItem", item, count, immediatePickup, forceEquip);
                    if (m_OnPickupItemEvent != null) {
                        m_OnPickupItemEvent.Invoke(item, count, immediatePickup, forceEquip);
                    }
                }
            }
        }

        /// <summary>
        /// Internal method which adds the specified count of the ItemType to the inventory.
        /// </summary>
        /// <param name="itemType">The ItemType to add.</param>
        /// <param name="count">The amount of ItemType to add.</param>
        /// <returns>True if the ItemType was picked up successfully.</returns>
        protected abstract bool PickupItemTypeInternal(ItemType itemType, float count);

        /// <summary>
        /// Returns the item in the specified slot.
        /// </summary>
        /// <param name="slotID">The ID of the slot.</param>
        /// <returns>The item which occupies the specified slot. Can be null.</returns>
        public Item GetItem(int slotID) { return GetItemInternal(slotID); }

        /// <summary>
        /// Internal method which returns the item in the specified slot.
        /// </summary>
        /// <param name="slotID">The ID of the slot which the item belongs to.</param>
        /// <returns>The Item which occupies the specified slot. Can be null.</returns>
        protected abstract Item GetItemInternal(int slotID);

        /// <summary>
        /// Returns the item that corresponds to the specified ItemType.
        /// </summary>
        /// <param name="slotID">The ID of the slot which the item belongs to.</param>
        /// <param name="itemType">The ItemType of the item.</param>
        /// <returns>The item which occupies the specified slot. Can be null.</returns>
        public Item GetItem(int slotID, ItemType itemType) { return GetItemInternal(slotID, itemType); }

        /// <summary>
        /// Internal method which returns the item that corresponds to the specified ItemType.
        /// </summary>
        /// <param name="slotID">The ID of the slot which the item belongs to.</param>
        /// <param name="itemType">The ItemType of the item.</param>
        /// <returns>The item which occupies the specified slot. Can be null.</returns>
        protected abstract Item GetItemInternal(int slotID, ItemType itemType);

        /// <summary>
        /// Returns a list of all of the items in the inventory.
        /// </summary>
        /// <returns>A list of all of the items in the inventory.</returns>
        public List<Item> GetAllItems() { return m_AllItems; }

#if UNITY_EDITOR
        /// <summary>
        /// Returns a list of all of the ItemTypes in the inventory. Only used by the editor for the inventory inspector.
        /// </summary>
        /// <returns>A list of all of the ItemTypes in the inventory.</returns>
        public List<ItemType> GetAllItemTypes() { return m_AllItemTypes; }
#endif

        /// <summary>
        /// Equips the ItemType in the specified slot.
        /// </summary>
        /// <param name="itemType">The ItemType to equip.</param>
        /// <param name="slotID">The ID of the slot.</param>
        /// <param name="immediateEquip">Is the item being equipped immediately? Immediate equips will occur from the default loadout or quickly switching to the item.</param>
        public void EquipItem(ItemType itemType, int slotID, bool immediateEquip)
        {
            if (itemType == null) {
                return;
            }

            var currentItem = GetItem(slotID);
            if (currentItem != null) {
                // Don't equip if the ItemType is already equipped.
                if (currentItem.ItemType == itemType) {
                    return;
                }
                UnequipItem(slotID);
            }

            var item = EquipItemInternal(itemType, slotID);
            if (item != null) {
                item.Equip(immediateEquip);

                // Notify those interested that an item has been equipped.
                EventHandler.ExecuteEvent(m_GameObject, "OnInventoryEquipItem", item, slotID);
                if (m_OnEquipItemEvent != null) {
                    m_OnEquipItemEvent.Invoke(item, slotID);
                }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                if (m_NetworkInfo != null && m_NetworkInfo.IsLocalPlayer()) {
                    m_NetworkCharacter.EquipUnequipItem(itemType.ID, slotID, true);
                }
#endif
            }
        }

        /// <summary>
        /// Internal method which equips the ItemType in the specified slot.
        /// </summary>
        /// <param name="itemType">The ItemType to equip.</param>
        /// <param name="slotID">The ID of the slot.</param>
        /// <returns>The item which corresponds to the ItemType. Can be null.</returns>
        protected abstract Item EquipItemInternal(ItemType itemType, int slotID);

        /// <summary>
        /// Unequips the specified ItemType in the specified slot.
        /// </summary>
        /// <param name="itemType">The ItemType to unequip. If the ItemType isn't currently equipped then no changes will be made.</param>
        /// <param name="slotID">The ID of the slot.</param>
        public void UnequipItem(ItemType itemType, int slotID)
        {
            // No need to unequip if the item is already unequipped or the ItemTypes don't match.
            var currentItem = GetItem(slotID);
            if (currentItem == null || currentItem.ItemType != itemType) {
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
            var currentItem = GetItem(slotID);
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
                    m_NetworkCharacter.EquipUnequipItem(item.ItemType.ID, slotID, false);
                }
#endif
            }
        }

        /// <summary>
        /// Internal method which unequips the item in the specified slot.
        /// </summary>
        /// <param name="slotID">The ID of the slot.</param>
        /// <returns>The item that was unequipped.</returns>
        protected abstract Item UnequipItemInternal(int slotID);

        /// <summary>
        /// Returns the count of the specified ItemType.
        /// </summary>
        /// <param name="itemType">The ItemType to get the count of.</param>
        /// <returns>The count of the specified ItemType.</returns>
        public float GetItemTypeCount(ItemType itemType) { if (itemType == null) { return 0; } return GetItemTypeCountInternal(itemType); }

        /// <summary>
        /// Internal method which returns the count of the specified ItemType.
        /// </summary>
        /// <param name="itemType">The ItemType to get the count of.</param>
        /// <returns>The count of the specified ItemType.</returns>
        protected abstract float GetItemTypeCountInternal(ItemType itemType);

        /// <summary>
        /// Uses the specified count of the ItemType.
        /// </summary>
        /// <param name="itemType">The ItemType to use.</param>
        /// <param name="count">The amount of ItemType to use.</param>
        public void UseItem(ItemType itemType, float count)
        {
            if (itemType == null) {
                return;
            }

            UseItemInternal(itemType, count);

            // Notify those interested that an item has been used.
            var remaining = GetItemTypeCount(itemType);
            EventHandler.ExecuteEvent(m_GameObject, "OnInventoryUseItemType", itemType, remaining);
            if (m_OnUseItemTypeEvent != null) {
                m_OnUseItemTypeEvent.Invoke(itemType, remaining);
            }
        }

        /// <summary>
        /// Internal method which uses the specified count of the ItemType.
        /// </summary>
        /// <param name="itemType">The ItemType to use.</param>
        /// <param name="count">The amount of ItemType to use.</param>
        protected abstract void UseItemInternal(ItemType itemType, float count);

        /// <summary>
        /// Removes the ItemType from the inventory.
        /// </summary>
        /// <param name="itemType">The ItemType to remove.</param>
        /// <param name="slotID">The ID of the slot.</param>
        /// <param name="drop">Should the item be dropped when removed?</param>
        public void RemoveItem(ItemType itemType, int slotID, bool drop)
        {
            var item = GetItem(slotID, itemType);
            if (item != null) {

                // The item should be dropped before unequipped so the drop position will be correct.
                if (drop) {
                    item.Drop(false);
                }

                // An equipped item needs to be unequipped.
                UnequipItem(itemType, slotID);

                // If the item isn't dropped then it is removed immediately.
                if (!drop) {
                    item.Remove();
                }

                var itemActions = item.ItemActions;
                if (itemActions != null) {
                    IUsableItem usableItem;
                    ItemType consumableItemType;
                    for (int i = 0; i < itemActions.Length; ++i) {
                        if (((usableItem = itemActions[i] as IUsableItem) != null) && (consumableItemType = usableItem.GetConsumableItemType()) != null) {
                            // Any consumable ItemTypes should also be removed if there are no more of the same items remaining.
                            if (GetItemTypeCount(itemType) == 1) {
                                RemoveItemTypeInternal(consumableItemType, slotID);
#if UNITY_EDITOR
                                m_AllItemTypes.Remove(consumableItemType);
#endif
                            }

                            usableItem.RemoveConsumableItemTypeCount();

                            // Notify those interested of the removed count.
                            var consumableCount = GetItemTypeCount(consumableItemType);
                            EventHandler.ExecuteEvent(m_GameObject, "OnInventoryUseItemType", consumableItemType, consumableCount);
                            if (m_OnUseItemTypeEvent != null) {
                                m_OnUseItemTypeEvent.Invoke(consumableItemType, consumableCount);
                            }
#if UNITY_EDITOR
                            if (GetItemTypeCount(itemType) == 0) {
                                m_AllItemTypes.Remove(consumableItemType);
                            }
#endif
                        }
                    }
                }
                m_AllItems.Remove(item);
            }

            // The ItemType should be removed from the inventory.
            RemoveItemTypeInternal(itemType, slotID);
#if UNITY_EDITOR
            if (GetItemTypeCount(itemType) == 0) {
                m_AllItemTypes.Remove(itemType);
            }
#endif

            // Notify those interested that the item will be removed.
            EventHandler.ExecuteEvent(m_GameObject, "OnInventoryRemoveItem", item, slotID);
            if (m_OnRemoveItemEvent != null) {
                m_OnRemoveItemEvent.Invoke(item, slotID);
            }
        }

        /// <summary>
        /// Internal method which removes the ItemType from the inventory.
        /// </summary>
        /// <param name="itemType">The ItemType to remove.</param>
        /// <param name="slotID">The ID of the slot.</param>
        /// <returns>The item that was removed.</returns>
        protected abstract Item RemoveItemTypeInternal(ItemType itemType, int slotID);

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
                var itemType = allItems[i].ItemType;
                var slotID = allItems[i].SlotID;
                while (GetItemTypeCount(itemType) > 0) {
                    RemoveItem(itemType, slotID, true);
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
