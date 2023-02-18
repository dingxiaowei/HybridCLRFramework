/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Items;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Implements InventoryBase - adds a basic inventory to the character controller.
    /// </summary>
    public class Inventory : InventoryBase
    {
        [Tooltip("Items to load when the Inventory is initially created or on a character respawn.")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_ItemTypeCount")]
        [SerializeField] protected ItemDefinitionAmount[] m_DefaultLoadout;

        private Dictionary<IItemIdentifier, Item>[] m_ItemIdentifierMap;
        private Dictionary<IItemIdentifier, int> m_ItemIdentifierAmount = new Dictionary<IItemIdentifier, int>();
        private Item[] m_ActiveItem;
        public ItemDefinitionAmount[] DefaultLoadout { get { return m_DefaultLoadout; } set { m_DefaultLoadout = value; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_ItemIdentifierMap = new Dictionary<IItemIdentifier, Item>[m_SlotCount];
            for (int i = 0; i < m_SlotCount; ++i) {
                m_ItemIdentifierMap[i] = new Dictionary<IItemIdentifier, Item>();
            }
            m_ActiveItem = new Item[m_SlotCount];
        }

        /// <summary>
        /// Pick up each ItemIdentifier within the DefaultLoadout.
        /// </summary>
        public override void LoadDefaultLoadout()
        {
            if (m_DefaultLoadout != null) {
                for (int i = 0; i < m_DefaultLoadout.Length; ++i) {
                    Pickup(m_DefaultLoadout[i].ItemIdentifier, m_DefaultLoadout[i].Amount, -1, true, false);
                }
            }
        }

        /// <summary>
        /// Internal method which determines if the character has the specified item.
        /// </summary>
        /// <param name="item">The item to check against.</param>
        /// <returns>True if the character has the item.</returns>
        protected override bool HasItemInternal(Item item)
        {
            if (item == null) {
                return false;
            }
            return GetItemInternal(item.ItemIdentifier, item.SlotID) != null;
        }

        /// <summary>
        /// Adds the item to the inventory. This does not add the actual ItemIdentifier - PickupItem does that.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>True if the item was added to the inventory.</returns>
        protected override bool AddItemInternal(Item item)
        {
            if (item.ItemDefinition == null) {
                Debug.LogError($"Error: Item {item.gameObject.name} has no ItemDefinition.");
                return false;
            }

            if (m_ItemIdentifierMap == null) {
                Debug.LogError($"Error: Unable to add {item.gameObject.name} because the inventory component doesn't exist.");
                return false;
            }

            if (item.SlotID >= m_ItemIdentifierMap.Length) {
                Debug.LogError($"Error: Unable to add {item.gameObject.name} because the slot id is greater than the number of slots that exist in the inventory.");
                return false;
            }

            var itemIdentifier = item.ItemIdentifier;
            if (itemIdentifier == null) {
                itemIdentifier = item.ItemDefinition.CreateItemIdentifier();
            }

            // The item may already exist in the inventory.
            if (m_ItemIdentifierMap[item.SlotID].ContainsKey(itemIdentifier)) {
                return false;
            }

            // The item doesn't exist - add it.
            m_ItemIdentifierMap[item.SlotID].Add(itemIdentifier, item);

            // The item can be added without being picked up yet - add to the ItemIdentifierAmount so the item can safely be removed.
            if (!m_ItemIdentifierAmount.ContainsKey(itemIdentifier)) {
                m_ItemIdentifierAmount.Add(itemIdentifier, 0);
            }
            return true;
        }

        /// <summary>
        /// Adds the specified amount of the ItemIdentifier to the inventory.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to add.</param>
        /// <param name="amount">The amount of ItemIdentifier to add.</param>
        /// <returns>True if the ItemIdentifier was picked up.</returns>
        protected override bool PickupInternal(IItemIdentifier itemIdentifier, int amount)
        {
            if (!m_ItemIdentifierAmount.TryGetValue(itemIdentifier, out var existingAmount)) {
                if (itemIdentifier is ItemType) {
                    var itemType = itemIdentifier as ItemType;
                    m_ItemIdentifierAmount.Add(itemIdentifier, Mathf.Min(amount, itemType.Capacity));
                } else {
                    m_ItemIdentifierAmount.Add(itemIdentifier, amount);
                }
            } else {
                if (itemIdentifier is ItemType) {
                    var itemType = itemIdentifier as ItemType;
                    // The ItemType was not picked up if it is already at capacity.
                    if (existingAmount == itemType.Capacity) {
                        return false;
                    }
                    m_ItemIdentifierAmount[itemIdentifier] = Mathf.Clamp(existingAmount + amount, 0, itemType.Capacity);
                } else {
                    m_ItemIdentifierAmount[itemIdentifier] = existingAmount + amount;
                }
            }
            return true;
        }

        /// <summary>
        /// Internal method which returns the active item in the specified slot.
        /// </summary>
        /// <param name="slotID">The ID of the slot which the item belongs to.</param>
        /// <returns>The active item which occupies the specified slot. Can be null.</returns>
        protected override Item GetActiveItemInternal(int slotID)
        {
            if (slotID < -1 || slotID >= m_ItemIdentifierMap.Length) {
                return null;
            }

            return m_ActiveItem[slotID];
        }

        /// <summary>
        /// Internal method which returns the item that corresponds to the specified ItemIdentifier.
        /// </summary>
        /// <param name="slotID">The ID of the slot which the item belongs to.</param>
        /// <param name="itemIdentifier">The ItemIdentifier of the item.</param>
        /// <returns>The item which occupies the specified slot. Can be null.</returns>
        protected override Item GetItemInternal(IItemIdentifier itemIdentifier, int slotID)
        {
            if (itemIdentifier == null || slotID < -1 || slotID >= m_ItemIdentifierMap.Length) {
                return null;
            }

            if (m_ItemIdentifierMap[slotID].TryGetValue(itemIdentifier, out var item)) {
                return item;
            }
            return null;
        }

        /// <summary>
        /// Internal method which equips the ItemIdentifier in the specified slot.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to equip.</param>
        /// <param name="slotID">The ID of the slot.</param>
        /// <returns>The item which corresponds to the ItemIdentifier. Can be null.</returns>
        protected override Item EquipItemInternal(IItemIdentifier itemIdentifier, int slotID)
        {
            if (itemIdentifier == null || slotID < -1 || slotID >= m_ItemIdentifierMap.Length) {
                return null;
            }

            // The ItemIdentifier has to exist in the inventory.
            if (!m_ItemIdentifierMap[slotID].TryGetValue(itemIdentifier, out var item)) {
                Debug.LogError($"Error: Unable to equip item with ItemIdentifier {itemIdentifier}: the itemIdentifier hasn't been added to the inventory.");
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
            if (slotID < -1 || slotID >= m_ItemIdentifierMap.Length) {
                return null;
            }

            var prevItem = m_ActiveItem[slotID];
            m_ActiveItem[slotID] = null;
            return prevItem;
        }

        /// <summary>
        /// Internal method which returns the amount of the specified ItemIdentifier.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to get the amount of.</param>
        /// <returns>The amount of the specified ItemIdentifier.</returns>
        protected override int GetItemIdentifierAmountInternal(IItemIdentifier itemIdentifier)
        {
            m_ItemIdentifierAmount.TryGetValue(itemIdentifier, out var amount);
            return amount;
        }

        /// <summary>
        /// Internal method which adjusts the amount of the specified ItemIdentifier.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to adjust.</param>
        /// <param name="amount">The amount of ItemIdentifier to adjust.</param>
        protected override void AdjustItemIdentifierAmountInternal(IItemIdentifier itemIdentifier, int amount)
        {
            if (!m_ItemIdentifierAmount.TryGetValue(itemIdentifier, out var existingAmount)) {
                Debug.LogError($"Error: Trying to use item {itemIdentifier} when the ItemIdentifier doesn't exist.");
                return;
            }

            if (itemIdentifier is ItemType) {
                var itemType = itemIdentifier as ItemType;
                m_ItemIdentifierAmount[itemIdentifier] = Mathf.Clamp(existingAmount + amount, 0, itemType.Capacity);
            } else {
                m_ItemIdentifierAmount[itemIdentifier] = existingAmount + amount;
            }
        }

        /// <summary>
        /// Internal method which removes the ItemIdentifier from the inventory.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to remove.</param>
        /// <param name="slotID">The ID of the slot.</param>
        /// <param name="amount">The amount of ItemIdentifier that should be removed.</param>
        /// <returns>The item that was removed (can be null).</returns>
        protected override void RemoveItemIdentifierInternal(IItemIdentifier itemIdentifier, int slotID, int amount)
        {
            if (itemIdentifier == null || !(itemIdentifier is ItemType) || slotID < -1 || 
                    slotID >= m_ItemIdentifierMap.Length || !m_ItemIdentifierAmount.TryGetValue(itemIdentifier, out var existingAmount)) {
                return;
            }

            if (!m_ItemIdentifierMap[slotID].TryGetValue(itemIdentifier, out var item)) {
                // Remove the ItemIdentifier. This ItemIdentifier does not correspond to an item so it should be completely removed.
                m_ItemIdentifierAmount[itemIdentifier] = 0;
                return;
            }
            // Remove a single Item. The character may be carrying multiple of the same ItemIdentifier in the case of dual wielding.
            var itemType = itemIdentifier as ItemType;
            m_ItemIdentifierAmount[itemType] = Mathf.Clamp(item.FullInventoryDrop ? 0 : (existingAmount - 1), 0, itemType.Capacity);

            if (slotID == -1) {
                return;
            }

            // The item should no longer be equipped.
            if (m_ActiveItem[slotID] != null && m_ActiveItem[slotID].ItemIdentifier == itemIdentifier) {
                m_ActiveItem[slotID] = null;
            }
        }
    }
}