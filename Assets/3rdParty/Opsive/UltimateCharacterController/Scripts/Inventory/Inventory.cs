/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Items;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Inventory
{
    /// <summary>
    /// Implements InventoryBase - adds a basic inventory to the character controller.
    /// </summary>
    public class Inventory : InventoryBase
    {
        private Dictionary<ItemType, Item>[] m_ItemTypeItemMap;
        private Dictionary<ItemType, float> m_ItemTypeCount = new Dictionary<ItemType, float>();
        private Item[] m_ActiveItem;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_ItemTypeItemMap = new Dictionary<ItemType, Item>[m_SlotCount];
            for (int i = 0; i < m_SlotCount; ++i) {
                m_ItemTypeItemMap[i] = new Dictionary<ItemType, Item>();
            }
            m_ActiveItem = new Item[m_SlotCount];
        }

        /// <summary>
        /// Adds the item to the inventory. This does not add the actual ItemType - PickupItem does that.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>True if the item was added to the inventory.</returns>
        protected override bool AddItemInternal(Item item)
        {
            if (item.ItemType == null) {
                Debug.LogError("Error: Item " + item.gameObject.name + " has no ItemType.");
                return false;
            }

            if (m_ItemTypeItemMap == null) {
                Debug.LogError("Error: Unable to add " + item.gameObject.name + " because the inventory component doesn't exist.");
                return false;
            }

            if (item.SlotID >= m_ItemTypeItemMap.Length) {
                Debug.LogError("Error: Unable to add " + item.gameObject.name + " because the slot id is greater than the number of slots that exist in the inventory.");
                return false;
            }

            // The item may already exist in the inventory.
            if (m_ItemTypeItemMap[item.SlotID].ContainsKey(item.ItemType)) {
                return false;
            }

            // The item doesn't exist - add it.
            m_ItemTypeItemMap[item.SlotID].Add(item.ItemType, item);

            // The item can be added without being picked up yet - add to the ItemTypeCount so the item can safely be removed.
            if (!m_ItemTypeCount.ContainsKey(item.ItemType)) {
                m_ItemTypeCount.Add(item.ItemType, 0);
            }
            return true;
        }

        /// <summary>
        /// Adds the specified count of the ItemType to the inventory.
        /// </summary>
        /// <param name="itemType">The ItemType to add.</param>
        /// <param name="count">The amount of ItemType to add.</param>
        /// <returns>True if the ItemType was picked up.</returns>
        protected override bool PickupItemTypeInternal(ItemType itemType, float count)
        {
            var existingAmount = 0f;
            if (!m_ItemTypeCount.TryGetValue(itemType, out existingAmount)) {
                m_ItemTypeCount.Add(itemType, Mathf.Min(count, itemType.Capacity));
            } else {
                // The ItemType was not picked up if it is already at capacity.
                if (existingAmount == itemType.Capacity) {
                    return false;
                }
                m_ItemTypeCount[itemType] = Mathf.Clamp(existingAmount + count, 0, itemType.Capacity);
            }
            return true;
        }

        /// <summary>
        /// Internal method which returns the item in the specified slot.
        /// </summary>
        /// <param name="slotID">The ID of the slot which the item belongs to.</param>
        /// <returns>The item which occupies the specified slot. Can be null.</returns>
        protected override Item GetItemInternal(int slotID)
        {
            return m_ActiveItem[slotID];
        }

        /// <summary>
        /// Internal method which returns the item that corresponds to the specified ItemType.
        /// </summary>
        /// <param name="slotID">The ID of the slot which the item belongs to.</param>
        /// <param name="itemType">The ItemType of the item.</param>
        /// <returns>The item which occupies the specified slot. Can be null.</returns>
        protected override Item GetItemInternal(int slotID, ItemType itemType)
        {
            Item item;
            if (m_ItemTypeItemMap[slotID].TryGetValue(itemType, out item)) {
                return item;
            }
            return null;
        }

        /// <summary>
        /// Internal method which equips the ItemType in the specified slot.
        /// </summary>
        /// <param name="itemType">The ItemType to equip.</param>
        /// <param name="slotID">The ID of the slot.</param>
        /// <returns>The item which corresponds to the ItemType. Can be null.</returns>
        protected override Item EquipItemInternal(ItemType itemType, int slotID)
        {
            Item item;
            // The ItemType has to exist in the inventory.
            if (!m_ItemTypeItemMap[slotID].TryGetValue(itemType, out item)) {
                Debug.LogError("Error: Unable to equip item with ItemType " + itemType + " - the ItemType hasn't been added to the inventory.");
                return null;
            }

            m_ActiveItem[slotID] = item;
            return item;
        }

        /// <summary>
        /// Internal method which unequips the item in the specified slot.
        /// </summary>
        /// <param name="slotID">The ID of the slot.</param>
        /// <returns>The item that was unequipped.</returns>
        protected override Item UnequipItemInternal(int slotID)
        {
            var prevItem = m_ActiveItem[slotID];
            m_ActiveItem[slotID] = null;
            return prevItem;
        }

        /// <summary>
        /// Internal method which returns the count of the specified ItemType.
        /// </summary>
        /// <param name="itemType">The ItemType to get the count of.</param>
        /// <returns>The count of the specified ItemType.</returns>
        protected override float GetItemTypeCountInternal(ItemType itemType)
        {
            var count = 0f;
            m_ItemTypeCount.TryGetValue(itemType, out count);
            return count;
        }

        /// <summary>
        /// Internal method which uses the specified count of the ItemType.
        /// </summary>
        /// <param name="itemType">The ItemType to use.</param>
        /// <param name="count">The amount of ItemType to use.</param>
        protected override void UseItemInternal(ItemType itemType, float count)
        {
            var existingAmount = 0f;
            if (!m_ItemTypeCount.TryGetValue(itemType, out existingAmount)) {
                Debug.LogError("Error: Trying to use item " + itemType.name + " when the ItemType doesn't exist.");
                return;
            }
            m_ItemTypeCount[itemType] = Mathf.Clamp(existingAmount - count, 0, itemType.Capacity);
        }

        /// <summary>
        /// Internal method which removes the ItemType from the inventory.
        /// </summary>
        /// <param name="itemType">The ItemType to remove.</param>
        /// <param name="slotID">The ID of the slot.</param>
        /// <returns>The item that was removed (can be null).</returns>
        protected override Item RemoveItemTypeInternal(ItemType itemType, int slotID)
        {
            var existingAmount = 0f;
            if (!m_ItemTypeCount.TryGetValue(itemType, out existingAmount)) {
                return null;
            }

            Item item;
            if (!m_ItemTypeItemMap[slotID].TryGetValue(itemType, out item)) {
                // Remove the ItemType. This ItemType does not correspond to an item so it should be completely removed.
                m_ItemTypeCount[itemType] = 0;
                return null;
            }
            // Remove a single Item. The character may be carrying multiple of the same ItemType in the case of dual wielding.
            m_ItemTypeCount[itemType] = Mathf.Clamp(existingAmount - 1, 0, itemType.Capacity);

            if (slotID == -1) {
                return null;
            }

            // The item should no longer be equipped.
            if (m_ActiveItem[slotID] != null && m_ActiveItem[slotID].ItemType == itemType) {
                m_ActiveItem[slotID] = null;
            }

            return item;
        }
    }
}