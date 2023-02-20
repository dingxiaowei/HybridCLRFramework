/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Inventory
{
    /// <summary>
    /// The ItemSetManager manages the ItemSets belonging to the character.
    /// </summary>
    public class ItemSetManager : MonoBehaviour
    {
        [Tooltip("A reference to the ItemCollection that the inventory is using.")]
        [SerializeField] protected ItemCollection m_ItemCollection;
        [Tooltip("Sepcifies the order that the items can be equipped.")]
        [SerializeField] protected CategoryItemSet[] m_CategoryItemSets = new CategoryItemSet[0];

        public ItemCollection ItemCollection { get { return m_ItemCollection; } set { InitializeItemCollection(value); } }
        public CategoryItemSet[] CategoryItemSets { get { return m_CategoryItemSets; } set { m_CategoryItemSets = value; } }

        private GameObject m_GameObject;
        private InventoryBase m_Inventory;
        private int[] m_ActiveItemSetIndex;
        private int[] m_NextItemSetIndex;

        public int[] ActiveItemSetIndex { get { return m_ActiveItemSetIndex; } }

        /// <summary>
        /// Initialize the ItemCollection and ItemSet.
        /// </summary>
        private void Awake()
        {
            InitializeItemCollection(m_ItemCollection);

            for (int i = 0; i < m_CategoryItemSets.Length; ++i) {
                for (int j = 0; j < m_CategoryItemSets[i].ItemSetList.Count; ++j) {
                    // The ItemSet must be initialized.
                    m_CategoryItemSets[i].ItemSetList[j].Initialize(m_GameObject, this, m_CategoryItemSets[i].CategoryID, i, j);
                }
            }
        }

        /// <summary>
        /// Initializes the ItemCollection reference. This is useful if the ItemCollection is assigned after Awake is called.
        /// </summary>
        /// <param name="itemCollection">The ItemCollection to initialize.</param>
        private void InitializeItemCollection(ItemCollection itemCollection)
        {
            // The ItemCollection has already been initialized.
            if (m_Inventory != null) {
                return;
            }

            if (itemCollection == null) {
                return;
            }

            m_ItemCollection = itemCollection;

            m_GameObject = gameObject;
            m_Inventory = m_GameObject.GetCachedComponent<InventoryBase>();
            if (m_CategoryItemSets.Length == 0 && m_ItemCollection.Categories.Length > 0) {
                m_CategoryItemSets = new CategoryItemSet[m_ItemCollection.Categories.Length];
            }

            m_ActiveItemSetIndex = new int[m_CategoryItemSets.Length];
            m_NextItemSetIndex = new int[m_ActiveItemSetIndex.Length];
            for (int i = 0; i < m_CategoryItemSets.Length; ++i) {
                m_ActiveItemSetIndex[i] = -1;
                m_NextItemSetIndex[i] = -1;
                if (m_CategoryItemSets[i] == null) {
                    m_CategoryItemSets[i] = new CategoryItemSet(m_ItemCollection.Categories[i].ID);
                }
            }

            // Store the category index value within the ItemType to prevent the index from having to be retrieved every time the ID is used.
            var itemTypes = m_ItemCollection.ItemTypes;
            for (int i = 0; i < itemTypes.Length; ++i) {
                var indices = new int[itemTypes[i].CategoryIDs.Length];
                for (int j = 0; j < itemTypes[i].CategoryIDs.Length; ++j) {
                    var categoryID = itemTypes[i].CategoryIDs[j];
                    for (int k = 0; k < m_CategoryItemSets.Length; ++k) {
                        if (m_CategoryItemSets[k].CategoryID == categoryID) {
                            indices[j] = k;
                            break;
                        }
                    }
                }
                itemTypes[i].CategoryIndices = indices;
            }

            if (Application.isPlaying) {
                EventHandler.RegisterEvent<Item>(gameObject, "OnInventoryAddItem", OnAddItem);
            }
        }

        /// <summary>
        /// Returns the corresponding category index which maps to the ID.
        /// </summary>
        /// <param name="categoryID">The ID of the category to get.</param>
        /// <returns>The corresponding category index which maps to the ID.</returns>
        public int CategoryIDToIndex(int categoryID)
        {
            for (int i = 0; i < m_CategoryItemSets.Length; ++i) {
                if (m_CategoryItemSets[i].CategoryID == categoryID) {
                    return i;
                }
            }
            Debug.LogError("Error: Category " + categoryID + " cannot be found.");
            return -1;
        }

        /// <summary>
        /// The inventory has added the specified item.
        /// </summary>
        /// <param name="item">The item that was added.</param>
        private void OnAddItem(Item item)
        {
            for (int i = 0; i < item.ItemType.CategoryIndices.Length; ++i) {
                AddItemSet(item, item.ItemType.CategoryIndices[i]);
            }
        }

        /// <summary>
        /// Adds a new ItemSet for the specified item if it doesn't already exist.
        /// </summary>
        /// <param name="item">The item to add the ItemSet for.</param>
        /// <param name="categoryIndex">The ItemSet category index to add the ItemType to.</param>
        private void AddItemSet(Item item, int categoryIndex)
        {
            var addItemSet = true;
            var itemSetList = m_CategoryItemSets[categoryIndex].ItemSetList;
            for (int i = 0; i < itemSetList.Count; ++i) {
                if (itemSetList[i].Slots[item.SlotID] == item.ItemType) {
                    addItemSet = false;
                    break;
                }
            }
            // If no ItemSet exists with the added Item then add a new ItemSet.
            if (addItemSet) {
                var slots = new ItemType[m_Inventory.SlotCount];
                slots[item.SlotID] = item.ItemType;
                itemSetList.Add(new ItemSet(slots, string.Empty));
            }
        }

        /// <summary>
        /// Adds the ItemSet for the specified item if it doesn't already exist.
        /// </summary>
        /// <param name="item">The item to add the ItemSet for.</param>
        /// <param name="itemSet">The ItemSet to add.</param>
        /// <param name="defaultItemSet">Is the ItemSet the default ItemSet within the category?</param>
        public void AddItemSet(Item item, ItemSet itemSet, bool defaultItemSet)
        {
            for (int i = 0; i < item.ItemType.CategoryIndices.Length; ++i) {
                var categoryIndex = item.ItemType.CategoryIndices[i];
                var addItemSet = true;
                var itemSetList = m_CategoryItemSets[categoryIndex].ItemSetList;
                for (int j = 0; j < itemSetList.Count; ++j) {
                    var slots = itemSetList[j].Slots;
                    var slotMatch = true;
                    for (int k = 0; k < slots.Length; ++k) {
                        if (slots[k] != itemSet.Slots[k]) {
                            slotMatch = false;
                            break;
                        }
                    }
                    if (slotMatch) {
                        addItemSet = false;
                        break;
                    }
                }
                // If the ItemSet doesn't exist then add it to the list.
                if (addItemSet) {
                    itemSetList.Add(itemSet);
                    // The ItemSet must be initialized.
                    itemSet.Initialize(m_GameObject, this, m_CategoryItemSets[categoryIndex].CategoryID, categoryIndex, itemSetList.Count - 1);

                    // The ItemSet can be default if no existing ItemSets are the default ItemSet.
                    if (defaultItemSet && m_CategoryItemSets[categoryIndex].DefaultItemSetIndex == -1) {
                        m_CategoryItemSets[categoryIndex].DefaultItemSetIndex = itemSetList.Count - 1;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the ItemSet that the item belongs to.
        /// </summary>
        /// <param name="item">The item to get the ItemSet of.</param>
        /// <param name="categoryIndex">The index of the ItemSet category.</param>
        /// <param name="checkIfValid">Should the ItemSet be checked to see if it is valid?.</param>
        /// <param name="allowMultipleItemTypes">Can multiple ItemTypes be returned within the ItemSet?</param>
        /// <returns>The ItemSet that the item belongs to.</returns>
        public int GetItemSetIndex(Item item, int categoryIndex, bool checkIfValid, bool allowMultipleItemTypes)
        {
            if (categoryIndex == -1) {
                return -1;
            }

            // The ItemSet may be in the process of being changed. Test the next item set first to determine if this item set should be returned.
            System.Collections.Generic.List<ItemSet> itemSetList;
            if (m_NextItemSetIndex[categoryIndex] != -1) {
                itemSetList = m_CategoryItemSets[categoryIndex].ItemSetList;
                var itemSet = itemSetList[m_NextItemSetIndex[categoryIndex]];
                if (itemSet.Slots[item.SlotID] == item.ItemType && (!checkIfValid || IsItemSetValid(categoryIndex, m_NextItemSetIndex[categoryIndex], false))) {
                    return m_NextItemSetIndex[categoryIndex];
                }
            }

            // Search through all of the ItemSets for one that contains the specified item.
            itemSetList = m_CategoryItemSets[categoryIndex].ItemSetList;
            var validItemSet = -1;
            for (int i = 0; i < itemSetList.Count; ++i) {
                // The ItemSet must contain the item at the specified slot in addition to being a valid ItemSet.
                if (itemSetList[i].Slots[item.SlotID] == item.ItemType && (!checkIfValid || IsItemSetValid(categoryIndex, i, false))) {
                    if (allowMultipleItemTypes) {
                        return i;
                    }
                    // The ItemSet is valid, but do not return it immediately if the ItemSet uses more than one ItemType. This will prevent a dual wield ItemSet from equipping
                    // when a single ItemType was picked up.
                    var independentItemSet = true;
                    for (int j = 0; j < itemSetList[i].Slots.Length; ++j) {
                        if (j == item.SlotID) {
                            continue;
                        }
                        if (itemSetList[i].Slots[j] != null) {
                            independentItemSet = false;
                            break;
                        }
                    }

                    if (independentItemSet) {
                        return i;
                    } else if (validItemSet == -1) {
                        validItemSet = i;
                    }
                }
            }
            return validItemSet;
        }

        /// <summary>
        /// Returns the default ItemSet index for the specified category index.
        /// </summary>
        /// <param name="categoryIndex">The index of the cateogry to get the default ItemSet index of.</param>
        /// <returns>The default ItemSet index for the specified category index.</returns>
        public int GetDefaultItemSetIndex(int categoryIndex)
        {
            if (categoryIndex == -1) {
                return -1;
            }
            return m_CategoryItemSets[categoryIndex].DefaultItemSetIndex;
        }

        /// <summary>
        /// Returns true if the specified ItemSet is valid. A valid ItemSet means the character has all of the items specified in the inventory.
        /// </summary>
        /// <param name="categoryIndex">The index of the ItemSet category.</param>
        /// <param name="itemSetIndex">The ItemSet within the category.</param>
        /// <param name="checkIfCanSwitchTo">Should the ItemSet be checked if it can be switched to?</param>
        /// <returns>True if the specified ItemSet is valid.</returns>
        public bool IsItemSetValid(int categoryIndex, int itemSetIndex, bool checkIfCanSwitchTo)
        {
            if (itemSetIndex == -1 || itemSetIndex >= m_CategoryItemSets[categoryIndex].ItemSetList.Count) {
                return false;
            }

            var itemSet = m_CategoryItemSets[categoryIndex].ItemSetList[itemSetIndex];

            // The ItemSet isn't valid if it isn't enabled.
            if (!itemSet.Enabled) {
                return false;
            }

            // The ItemSet may not be able to be switched to.
            if (checkIfCanSwitchTo && !itemSet.CanSwitchTo) {
                return false;
            }

            for (int i = 0; i < itemSet.Slots.Length; ++i) {
                if (itemSet.Slots[i] == null) {
                    continue;
                }

                // It only takes one item for the ItemSet not to be valid.
                var item = m_Inventory.GetItem(i, itemSet.Slots[i]);
                if (item == null) {
                    return false;
                }

                // Ensure the inventory has the number of items required for the current ItemSet.
                var requiredCount = 0;
                for (int j = 0; j < itemSet.Slots.Length; ++j) {
                    if (itemSet.Slots[j] == item.ItemType) {
                        requiredCount++;
                    }
                }
                if (m_Inventory.GetItemTypeCount(item.ItemType) < requiredCount) {
                    return false;
                }

                // Usable items may not be able to be equipped if they don't have any consumable ItemTypes left.
                for (int j = 0; j < item.ItemActions.Length; ++j) {
                    var usableItem = item.ItemActions[j] as IUsableItem;
                    if (usableItem != null) {
                        if (!usableItem.CanEquipEmptyItem && usableItem.GetConsumableItemType() != null && m_Inventory.GetItemTypeCount(usableItem.GetConsumableItemType()) == 0) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the index of the ItemSet that is next or previous in the list.
        /// </summary>
        /// <param name="categoryIndex">The category of ItemSets to get.</param>
        /// <param name="currentItemSetIndex">The current ItemSet index.</param>
        /// <param name="next">Should the next ItemSet be retrieved? If false the previous ItemSet will be retrieved.</param>
        /// <returns>The index of the ItemSet that is next or previous in the list.</returns>
        public int NextActiveItemSetIndex(int categoryIndex, int currentItemSetIndex, bool next)
        {
            if (currentItemSetIndex == -1) {
                return -1;
            }
            var itemSetListCount = m_CategoryItemSets[categoryIndex].ItemSetList.Count;
            // The ItemSet can't be switched if there are zero or only one ItemSets.
            if (itemSetListCount <= 1) {
                return -1;
            }
            var itemSetIndex = currentItemSetIndex;
            do {
                itemSetIndex = (itemSetIndex + (next ? 1 : -1)) % itemSetListCount;
                if (itemSetIndex < 0) {
                    itemSetIndex = itemSetListCount - 1;
                }
            } while (itemSetIndex != currentItemSetIndex && !IsItemSetValid(categoryIndex, itemSetIndex, true));

            return itemSetIndex;
        }

        /// <summary>
        /// Updates the next ItemSet to the specified value.
        /// </summary>
        /// <param name="categoryIndex">The category to update the ItemSet within.</param>
        /// <param name="itemSetIndex">The ItemSet to set.</param>
        public void UpdateNextItemSet(int categoryIndex, int itemSetIndex)
        {
            // No updates are necessary if the indicies are the same.
            var activeItemSetIndex = m_NextItemSetIndex[categoryIndex];
            if (activeItemSetIndex == itemSetIndex) {
                return;
            }

            m_NextItemSetIndex[categoryIndex] = itemSetIndex;
        }

        /// <summary>
        /// Updates the active ItemSet to the specified value.
        /// </summary>
        /// <param name="categoryIndex">The category to update the ItemSet within.</param>
        /// <param name="itemSetIndex">The ItemSet to set.</param>
        public void UpdateActiveItemSet(int categoryIndex, int itemSetIndex)
        {
            // No updates are necessary if the indicies are the same.
            var activeItemSetIndex = m_ActiveItemSetIndex[categoryIndex];
            if (activeItemSetIndex == itemSetIndex) {
                return;
            }

            if (m_ActiveItemSetIndex[categoryIndex] != -1) {
                m_CategoryItemSets[categoryIndex].ItemSetList[m_ActiveItemSetIndex[categoryIndex]].Active = false;
            }
            if (itemSetIndex >= m_CategoryItemSets[categoryIndex].ItemSetList.Count) {
                itemSetIndex = -1;
            }
            m_ActiveItemSetIndex[categoryIndex] = itemSetIndex;
            m_NextItemSetIndex[categoryIndex] = -1;
            if (itemSetIndex != -1) {
                m_CategoryItemSets[categoryIndex].ItemSetList[itemSetIndex].Active = true;
            }
            string state, newState = null;
            if (itemSetIndex != -1) {
                if (!string.IsNullOrEmpty((state = m_CategoryItemSets[categoryIndex].ItemSetList[itemSetIndex].State))) {
                    StateSystem.StateManager.SetState(m_GameObject, state, true);
                    // Store the new state name for testing against.
                    newState = state;
                }
            }
            if (activeItemSetIndex != -1 && !string.IsNullOrEmpty((state = m_CategoryItemSets[categoryIndex].ItemSetList[activeItemSetIndex].State))) {
                // If the new state is null or different, then deactivate the state of the old item set.
                if (string.IsNullOrEmpty(newState) || state != newState) {
                    StateSystem.StateManager.SetState(m_GameObject, state, false);
                }
            }
            EventHandler.ExecuteEvent(m_GameObject, "OnItemSetManagerUpdateItemSet", categoryIndex, itemSetIndex);
        }

        /// <summary>
        /// Sets the default ItemSet for the specified category.
        /// </summary>
        /// <param name="categoryIndex">The category to set the default itemset of.</param>
        public void SetDefaultItemSet(int categoryIndex)
        {
            var itemSetIndex = GetDefaultItemSetIndex(categoryIndex);
            if (IsItemSetValid(categoryIndex, itemSetIndex, false)) {
                UpdateActiveItemSet(categoryIndex, itemSetIndex);
            }
        }

        /// <summary>
        /// Returns the ItemType which should be equipped for the specified slot.
        /// </summary>
        /// <param name="slot">The slot to get the ItemType of.</param>
        /// <returns>The ItemType which should be equipped for the specified slot. Can be null.</returns>
        public ItemType GetEquipItemType(int slot)
        {
            if (slot == -1 || slot >= m_Inventory.SlotCount) {
                return null;
            }
            for (int i = 0; i < m_CategoryItemSets.Length; ++i) {
                if (m_ActiveItemSetIndex[i] != -1) {
                    var itemSet = m_CategoryItemSets[i].ItemSetList[m_ActiveItemSetIndex[i]];
                    if (itemSet.Enabled && itemSet.Slots[slot] != null) {
                        return itemSet.Slots[slot];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the ItemType which should be equipped for the specified categoryIndex and slot.
        /// </summary>
        /// <param name="categoryIndex">The category to get the ItemType of.</param>
        /// <param name="slot">The slot to get the ItemType of.</param>
        /// <returns>The ItemType which should be equipped for the specified slot. Can be null.</returns>
        public ItemType GetEquipItemType(int categoryIndex, int slot)
        {
            if (categoryIndex == -1 || categoryIndex >= m_CategoryItemSets.Length || slot == -1 || slot >= m_Inventory.SlotCount) {
                return null;
            }
            if (m_ActiveItemSetIndex[categoryIndex] != -1) {
                var itemSet = m_CategoryItemSets[categoryIndex].ItemSetList[m_ActiveItemSetIndex[categoryIndex]];
                return itemSet.Slots[slot];
            }
            return null;
        }

        /// <summary>
        /// Returns the ItemType which should be equipped for the specified categoryIndex, ItemSet, and slot.
        /// </summary>
        /// <param name="categoryIndex">The category to get the ItemType of.</param>
        /// <param name="targetItemSetIndex">The ItemSet to get the ItemType of.</param>
        /// <param name="slot">The slot to get the ItemType of.</param>
        /// <returns>The ItemType which should be equipped for the specified categoryIndex, ItemSet, and slot. Can be null.</returns>
        public ItemType GetEquipItemType(int categoryIndex, int targetItemSetIndex, int slot)
        {
            if (categoryIndex == -1 || categoryIndex >= m_CategoryItemSets.Length || 
                targetItemSetIndex == -1 || targetItemSetIndex >= m_CategoryItemSets[categoryIndex].ItemSetList.Count || 
                slot == -1 || slot >= m_Inventory.SlotCount) {
                return null;
            }

            return m_CategoryItemSets[categoryIndex].ItemSetList[targetItemSetIndex].Slots[slot];
        }

        /// <summary>
        /// The character has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<Item>(gameObject, "OnInventoryAddItem", OnAddItem);
        }
    }
}