/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Items.Actions;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// The ItemSetManager manages the ItemSets belonging to the character.
    /// </summary>
    public abstract class ItemSetManagerBase : MonoBehaviour
    {
        [Tooltip("Sepcifies the order that the items can be equipped.")]
        [SerializeField] protected CategoryItemSet[] m_CategoryItemSets;
        public CategoryItemSet[] CategoryItemSets { get { return m_CategoryItemSets; } set { m_CategoryItemSets = value; } }

        [System.NonSerialized] protected bool m_Initialized;
        protected GameObject m_GameObject;
        protected InventoryBase m_Inventory;
        protected int[] m_ActiveItemSetIndex;
        protected int[] m_NextItemSetIndex;
        private HashSet<IItemIdentifier> m_CheckedItemIdentifiers = new HashSet<IItemIdentifier>();
        protected Dictionary<IItemCategoryIdentifier, int> m_CategoryIndexMap;
        public int[] ActiveItemSetIndex { get { return m_ActiveItemSetIndex; } }
        public int[] NextItemSetIndex { get { return m_NextItemSetIndex; } }

        /// <summary>
        /// Initialize the ItemCollection and ItemSet.
        /// </summary>
        private void Awake()
        {
            m_GameObject = gameObject;
            m_Inventory = m_GameObject.GetCachedComponent<InventoryBase>();

            Initialize(true);

            EventHandler.RegisterEvent<Item>(m_GameObject, "OnInventoryAddItem", OnAddItem);
        }

        /// <summary>
        /// Initializes the ItemSetManager.
        /// </summary>
        /// <param name="force">Should the ItemSet be force initialized?</param>
        public abstract void Initialize(bool force);

        /// <summary>
        /// Returns the corresponding category index which maps to the category.
        /// </summary>
        /// <param name="category">The interested category.</param>
        /// <returns>The corresponding category index which maps to the category.</returns>
        public int CategoryToIndex(IItemCategoryIdentifier category)
        {
            if (category == null) {
                return -1;
            }

            if (m_CategoryIndexMap.TryGetValue(category, out var index)) {
                return index;
            }

            return -1;
        }

        /// <summary>
        /// Returns the corresponding category index which maps to the ID.
        /// </summary>
        /// <param name="categoryID">The ID of the category to get.</param>
        /// <returns>The corresponding category index which maps to the ID.</returns>
        public int CategoryIDToIndex(uint categoryID)
        {
            for (int i = 0; i < m_CategoryItemSets.Length; ++i) {
                if (m_CategoryItemSets[i].CategoryID == categoryID) {
                    return i;
                }
            }
            Debug.LogError($"Error: Category with ID {categoryID} cannot be found.");
            return -1;
        }

        /// <summary>
        /// Returns true if the ItemDefinition belongs to the category with the specified index.
        /// </summary>
        /// <param name="itemDefinition">The ItemDefinition to determine if it belongs to the category.</param>
        /// <param name="categoryIndex">The index of the category which the ItemIdentifier may belong to.</param>
        /// <returns>True if the ItemDefinition belongs to the category with the specified index.</returns>
        public bool IsCategoryMember(ItemDefinitionBase itemDefinition, int categoryIndex)
        {
            // If an ItemDefinition doesn't have a category it is a member of every category.
            if (itemDefinition.GetItemCategory() == null) {
                return true;
            }

            return IsCategoryMember(itemDefinition.GetItemCategory(), categoryIndex);
        }

        /// <summary>
        /// Returns true if the CategoryIdentifier belongs to the category with the specified index.
        /// </summary>
        /// <param name="itemCategory">The CategoryIdentifier to determine if it belongs to the category.</param>
        /// <param name="categoryIndex">The index of the category which the ItemIdentifier may belong to.</param>
        /// <returns>True if the ItemIdentifier belongs to the category with the specified index.</returns>
        private bool IsCategoryMember(IItemCategoryIdentifier itemCategory, int categoryIndex)
        {
            if (categoryIndex >= m_CategoryItemSets.Length) {
                return false;
            }

            if (itemCategory == m_CategoryItemSets[categoryIndex].ItemCategory) {
                return true;
            }

            // Recursively search the parents.
            var categoryParents = itemCategory.GetDirectParents();
            if (categoryParents == null) {
                return false;
            }
            for (int i = 0; i < categoryParents.Count; ++i) {
                if (IsCategoryMember(categoryParents[i], categoryIndex)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the ItemDefinitionrepresents the default ItemCategory.
        /// </summary>
        /// <param name="itemIdentifier">The ItemDefinition to determine if it is the default ItemCategory.</param>
        /// <returns>True if the ItemDefinition represents the default ItemCategory.</returns>
        public bool IsDefaultItemCategory(ItemDefinitionBase itemDefinition)
        {
            if (itemDefinition == null) {
                return false;
            }
            var category = itemDefinition.GetItemCategory();
            if (category == null) {
                return false;
            }

            return IsDefaultItemCategory(itemDefinition, category);
        }

        /// <summary>
        /// Returns true if the ItemCategory represents the default ItemCategory.
        /// </summary>
        /// <param name="itemDefinition">The ItemDefinition to determine if it is the default ItemCategory.</param>
        /// <param name="itemCategory">The ItemCategory to determine if it is the default ItemCategory.</param>
        /// <returns>True if the ItemCategory represents the default ItemCategory.</returns>
        private bool IsDefaultItemCategory(ItemDefinitionBase itemDefinition, IItemCategoryIdentifier itemCategory)
        {
            var categoryParents = itemCategory.GetDirectParents();
            if (categoryParents != null) {
                for (int i = 0; i < categoryParents.Count; ++i) {
                    if (IsDefaultItemCategory(itemDefinition, categoryParents[i])) {
                        return true;
                    }
                }
            }

            var index = CategoryToIndex(itemCategory);
            if (index == -1) {
                return false;
            }

            // The default category does not match the active category. Return false.
            if (m_CategoryItemSets[index].DefaultItemSetIndex != m_ActiveItemSetIndex[index]) {
                return false;
            }

            // The default category is active. Ensure the ItemDefinition is in that ItemSet.
            var hasItemDefinition = false;
            var itemSetList = m_CategoryItemSets[index].ItemSetList[m_ActiveItemSetIndex[index]];
            for (int i = 0; i < itemSetList.Slots.Length; ++i) {
                if (IsChildOf(itemDefinition, itemSetList.Slots[i])) {
                    hasItemDefinition = true;
                    break;
                }
            }
            if (!hasItemDefinition) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// The inventory has added the specified item.
        /// </summary>
        /// <param name="item">The item that was added.</param>
        private void OnAddItem(Item item)
        {
            AddItem(item, item.ItemDefinition.GetItemCategory());
        }

        /// <summary>
        /// Adds the item with the specified ItemCategoryIdentifier.
        /// </summary>
        /// <param name="item">The item that should be added.</param>
        /// <param name="category">The category that the item should be aded to.</param>
        private void AddItem(Item item, IItemCategoryIdentifier category)
        {
            if (category == null) {
                Debug.LogError("Error: No category has been specified. Ensure the ItemIdentifier has been added to a category.");
                return;
            }

            AddItemSet(item, category);
            // The category can have multiple parents. Keep trying to add the item to all of the possible ItemSets.
            var categoryParents = category.GetDirectParents();
            if (categoryParents != null) {
                for (int i = 0; i < categoryParents.Count; ++i) {
                    AddItem(item, categoryParents[i]);
                }
            }
        }

        /// <summary>
        /// Adds a new ItemSet for the specified item if it doesn't already exist.
        /// </summary>
        /// <param name="item">The item to add the ItemSet for.</param>
        /// <param name="category">The category that the item should be aded to.</param>
        private void AddItemSet(Item item, IItemCategoryIdentifier category)
        {
            // The category may not have been added to the ItemSetManager.
            if (category == null || !m_CategoryIndexMap.TryGetValue(category, out var categoryIndex)) {
                return;
            }

            var addItemSet = item.UniqueItemSet;
            List<ItemSet> itemSetList;
            for (int i = 0; i < m_CategoryItemSets.Length; ++i) {
                itemSetList = m_CategoryItemSets[i].ItemSetList;
                for (int j = 0; j < itemSetList.Count; ++j) {
                    // If the item instance has already been added then no new item set needs to be created.
                    if (itemSetList[j].ItemIdentifiers[item.SlotID] == item.ItemIdentifier) {
                        addItemSet = false;
                        break;
                    }
                    // The ItemDefinition exists but the ItemIdentifier does not. Populate the ItemIdentifier with the current item and then stop searching.
                    if (itemSetList[j].ItemIdentifiers[item.SlotID] == null && itemSetList[j].Slots[item.SlotID] == item.ItemDefinition) {
                        itemSetList[j].ItemIdentifiers[item.SlotID] = item.ItemIdentifier;

                        // The item definition matches. Newly added items need to have an ItemSet to themselves.
                        var dedicatedItemDefinition = true;
                        for (int k = 0; k < itemSetList[j].Slots.Length; ++k) {
                            if (k == item.SlotID) {
                                continue;
                            }

                            if (itemSetList[j].Slots[k] != null) {
                                dedicatedItemDefinition = false;
                                break;
                            }
                        }
                        if (dedicatedItemDefinition) {
                            addItemSet = false; // Do not break within the loop because multiple definitions may exist for the same identifier.
                        }
                    }
                }
            }

            // If no ItemSet exists with the added Item then add a new ItemSet.
            itemSetList = m_CategoryItemSets[categoryIndex].ItemSetList;
            if (addItemSet) {
                var itemSet = new ItemSet(m_Inventory.SlotCount, item.SlotID, item.ItemDefinition, item.ItemIdentifier, string.Empty);
                // If the parent ItemIdentifier is not null then the new ItemSet should be added after the ItemSets with the same parent.
                if (item.ItemDefinition.GetParent() != null) {
                    var insertIndex = -1;
                    for (int i = 0; i < itemSetList.Count; ++i) {
                        if (IsChildOf(item.ItemDefinition, itemSetList[i].Slots[item.SlotID])) {
                            // The other slot elements must be empty.
                            var canInsert = true;
                            for (int j = 0; j < itemSetList[i].Slots.Length; ++j) {
                                if (j == item.SlotID) {
                                    continue;
                                }

                                if (itemSetList[i].Slots[j] != null) {
                                    canInsert = false;
                                    break;
                                }
                            }
                            if (canInsert) {
                                insertIndex = i + 1;
                            }
                        } else if (insertIndex != -1) {
                            // The ItemSet should be inserted after the last ItemSet with the same parent.
                            break;
                        }
                    }
                    // Insert the ItemSet with the child ItemDefinition.
                    insertIndex = insertIndex != -1 ? insertIndex : itemSetList.Count;
                    itemSetList.Insert(insertIndex, itemSet);

                    // If the ItemSet was inserted before the active index then the active index needs to update to stay accurate.
                    if (insertIndex <= m_ActiveItemSetIndex[categoryIndex]) {
                        m_ActiveItemSetIndex[categoryIndex] += 1;
                        EventHandler.ExecuteEvent(m_GameObject, "OnItemSetIndexChange", categoryIndex, m_ActiveItemSetIndex[categoryIndex]);
                    }

                    // The ItemSet must be duplicated for any ItemDefinition that has a similar parent.
                    for (int i = itemSetList.Count - 1; i >= 0; --i) {
                        if (IsChildOf(item.ItemDefinition, itemSetList[i].Slots[item.SlotID])) {
                            // Another ItemDefinition must exist in order for the ItemSet to be duplicated.
                            for (int j = 0; j < itemSetList[i].Slots.Length; ++j) {
                                if (j == item.SlotID) {
                                    continue;
                                }
                                // The ItemSet is unique. Duplicate it.
                                if (itemSetList[i].Slots[j] != null) {
                                    DuplicateItemSet(item, categoryIndex, i);
                                    break;
                                }
                            }
                        }
                    }
                } else {
                    // The ItemDefinition doesn't have a parent. Add the ItemSet to the end of the list.
                    itemSet.Initialize(m_GameObject, this, m_CategoryItemSets[categoryIndex].CategoryID, categoryIndex, itemSetList.Count);
                    itemSetList.Add(itemSet);
                }
            } else if (!item.UniqueItemSet) {
                // The individual ItemSet doesn't need to be added, but it may need to duplicate an existing ItemSet.
                for (int i = 0; i < itemSetList.Count; ++i) {
                    if (itemSetList[i].Slots[item.SlotID] != item.ItemDefinition || itemSetList[i].ItemIdentifiers[item.SlotID] == item.ItemIdentifier) {
                        continue;
                    }

                    for (int j = 0; j < itemSetList[i].Slots.Length; ++j) {
                        if (j == item.SlotID) {
                            continue;
                        }
                        if (itemSetList[i].Slots[j] != null) {
                            DuplicateItemSet(item, categoryIndex, i);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the possible child ItemDefinition is a child of the possible parent ItemDefinition.
        /// </summary>
        /// <param name="possibleChild">The ItemDefinition that may be a child of the specified parent.</param>
        /// <param name="categoryIndex">The ItemDefinition that may be a parent of the specified child.</param>
        /// <returns>True if the child ItemDefinition is a child of the parent ItemDefinition.</returns>
        private bool IsChildOf(ItemDefinitionBase possibleChild, ItemDefinitionBase possibleParent)
        {
            if (possibleChild == null || possibleParent == null) {
                return false;
            }

            var itemDefinition = possibleChild;
            while (itemDefinition != null) {
                if (itemDefinition == possibleParent) {
                    return true;
                }
                itemDefinition = itemDefinition.GetParent();
            }
            return false;
        }

        /// <summary>
        /// Duplicates the ItemSet at the specified index.
        /// </summary>
        /// <param name="item">The item that triggered the duplication.</param>
        /// <param name="categoryIndex">The index of the ItemSet category.</param>
        /// <param name="itemSetIndex">The index of the ItemSet that should be duplicated.</param>
        private void DuplicateItemSet(Item item, int categoryIndex, int itemSetIndex)
        {
            var itemSetList = m_CategoryItemSets[categoryIndex].ItemSetList;
            var duplicatedItemSet = new ItemSet(itemSetList[itemSetIndex]);
            duplicatedItemSet.State = itemSetList[itemSetIndex].State;
            duplicatedItemSet.Slots[item.SlotID] = item.ItemDefinition;
            duplicatedItemSet.ItemIdentifiers[item.SlotID] = item.ItemIdentifier;
            duplicatedItemSet.Initialize(m_GameObject, this, m_CategoryItemSets[categoryIndex].CategoryID, categoryIndex, itemSetIndex + 1);
            itemSetList.Insert(itemSetIndex + 1, duplicatedItemSet);

            // All of the subsequent ItemSets need to update their index.
            for (int i = itemSetIndex + 2; i < itemSetList.Count; ++i) {
                itemSetList[i].Index = i;
            }

            // If the ItemSet was inserted before the active index then the active index needs to update to stay accurate.
            if (itemSetIndex + 1 <= m_ActiveItemSetIndex[categoryIndex]) {
                m_ActiveItemSetIndex[categoryIndex] += 1;
                EventHandler.ExecuteEvent(m_GameObject, "OnItemSetIndexChange", categoryIndex, m_ActiveItemSetIndex[categoryIndex]);
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
            AddItemSet(item, itemSet, defaultItemSet, item.ItemDefinition.GetItemCategory());
        }

        /// <summary>
        /// Adds the ItemSet for the specified item if it doesn't already exist.
        /// </summary>
        /// <param name="item">The item to add the ItemSet for.</param>
        /// <param name="itemSet">The ItemSet to add.</param>
        /// <param name="defaultItemSet">Is the ItemSet the default ItemSet within the category?</param>
        /// <param name="category">The category that the ItemSet is trying to be added to.</param>
        private void AddItemSet(Item item, ItemSet itemSet, bool defaultItemSet, IItemCategoryIdentifier category)
        {
            // The category can have multiple parents. Keep trying to add the ItemSet to all of the possible categories.
            var categoryParents = category.GetDirectParents();
            if (categoryParents != null) {
                for (int i = 0; i < categoryParents.Count; ++i) {
                    AddItemSet(item, itemSet, defaultItemSet, categoryParents[i]);
                }
            }

            // The category may not have been added to the ItemSetManager.
            if (!m_CategoryIndexMap.TryGetValue(category, out var categoryIndex)) {
                return;
            }

            var addItemSet = true;
            var itemSetList = m_CategoryItemSets[categoryIndex].ItemSetList;
            for (int i = 0; i < itemSetList.Count; ++i) {
                var slots = itemSetList[i].Slots;
                var slotMatch = true;
                for (int j = 0; j < slots.Length; ++j) {
                    if (slots[j] != itemSet.Slots[j]) {
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
                AddItemSet(itemSet, defaultItemSet, categoryIndex);
            }
        }

        /// <summary>
        /// Adds the ItemSet if it doesn't already exist.
        /// </summary>
        /// <param name="itemSet">The ItemSet to add.</param>
        /// <param name="defaultItemSet">Is the ItemSet the default ItemSet within the category?</param>
        /// <param name="category">The category that the ItemSet is trying to be added to.</param>
        /// <param name="addParents">Should ItemSets be added to the category parents?</param>
        public void AddItemSet(ItemSet itemSet, bool defaultItemSet, IItemCategoryIdentifier category, bool addParents)
        {
            // The ItemSet must have at least one slot filled.
            var validItemSet = false;
            for (int i = 0; i < itemSet.Slots.Length; ++i) {
                if (itemSet.Slots[i] != null) {
                    validItemSet = true;
                    break;
                }
            }
            if (!validItemSet) {
                return;
            }

            // The category can have multiple parents. Keep trying to add the ItemSet to all of the possible categories.
            if (addParents) {
                var categoryParents = category.GetDirectParents();
                if (categoryParents != null) {
                    for (int i = 0; i < categoryParents.Count; ++i) {
                        AddItemSet(itemSet, defaultItemSet, categoryParents[i], addParents);
                    }
                }
            }

            // The category may not have been added to the ItemSetManager.
            if (!m_CategoryIndexMap.TryGetValue(category, out var categoryIndex)) {
                return;
            }

            // Ensure the ItemSet is unique.
            var itemSetList = m_CategoryItemSets[categoryIndex].ItemSetList;
            for (int i = 0; i < itemSetList.Count; ++i) {
                var itemDefinitionMatch = true;
                for (int j = 0; j < itemSetList[i].Slots.Length; ++j) {
                    if (itemSet.Slots[j] != itemSetList[i].Slots[j]) {
                        itemDefinitionMatch = false;
                        break;
                    }
                }
                if (itemDefinitionMatch) {
                    return;
                }
            }

            // The ItemSet is unique. Add it to the list.
            AddItemSet(itemSet, defaultItemSet, categoryIndex);
        }

        /// <summary>
        /// Adds the ItemSet.
        /// </summary>
        /// <param name="itemSet">The ItemSet to add.</param>
        /// <param name="defaultItemSet">Is the ItemSet the default ItemSet within the category?</param>
        /// <param name="categoryIndex">The index of the category that the ItemSet is being added to.</param>
        private void AddItemSet(ItemSet itemSet, bool defaultItemSet, int categoryIndex)
        {
            var itemSetList = m_CategoryItemSets[categoryIndex].ItemSetList;
            itemSet = new ItemSet(itemSet);
            itemSetList.Add(itemSet);
            // The ItemSet must be initialized.
            itemSet.Initialize(m_GameObject, this, m_CategoryItemSets[categoryIndex].CategoryID, categoryIndex, itemSetList.Count - 1);

            // The ItemSet can be default if no existing ItemSets are the default ItemSet.
            if (defaultItemSet && m_CategoryItemSets[categoryIndex].DefaultItemSetIndex == -1) {
                m_CategoryItemSets[categoryIndex].DefaultItemSetIndex = itemSetList.Count - 1;
            }
        }

        /// <summary>
        /// Returns the ItemSet that the item belongs to.
        /// </summary>
        /// <param name="item">The item to get the ItemSet of.</param>
        /// <param name="categoryIndex">The index of the ItemSet category.</param>
        /// <param name="checkIfValid">Should the ItemSet be checked to see if it is valid?.</param>
        /// <returns>The ItemSet that the item belongs to.</returns>
        public int GetItemSetIndex(Item item, int categoryIndex, bool checkIfValid)
        {
            if (categoryIndex == -1) {
                return -1;
            }

            // The ItemSet may be in the process of being changed. Test the next item set first to determine if this item set should be returned.
            List<ItemSet> itemSetList;
            if (m_NextItemSetIndex[categoryIndex] != -1) {
                itemSetList = m_CategoryItemSets[categoryIndex].ItemSetList;
                var itemSet = itemSetList[m_NextItemSetIndex[categoryIndex]];
                if (itemSet.ItemIdentifiers[item.SlotID] == item.ItemIdentifier && (!checkIfValid || IsItemSetValid(categoryIndex, m_NextItemSetIndex[categoryIndex], false))) {
                    return m_NextItemSetIndex[categoryIndex];
                }
            }

            var itemCount = m_Inventory.GetItemIdentifierAmount(item.ItemIdentifier);
            // Search through all of the ItemSets for one that contains the specified item.
            itemSetList = m_CategoryItemSets[categoryIndex].ItemSetList;
            var validItemSet = -1;
            for (int i = 0; i < itemSetList.Count; ++i) {
                // The ItemSet must contain the item at the specified slot in addition to being a valid ItemSet.
                if (itemSetList[i].ItemIdentifiers[item.SlotID] == item.ItemIdentifier && (!checkIfValid || IsItemSetValid(categoryIndex, i, false))) {
                    // The ItemSet is valid, but do not return it immediately if the ItemSet uses more than one ItemDefinitions. This will prevent a dual wield ItemSet from equipping
                    // when a single item was picked up.
                    var validSlotCount = 1;
                    for (int j = 0; j < itemSetList[i].ItemIdentifiers.Length; ++j) {
                        if (j == item.SlotID) {
                            continue;
                        }
                        if (IsChildOf(item.ItemDefinition, itemSetList[i].Slots[j])) {
                            validSlotCount++;
                        }
                    }

                    if (itemCount == validSlotCount) {
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
        /// Returns the target ItemSet index for the specified category index based on the allowed slots bitwise mask.
        /// </summary>
        /// <param name="categoryIndex">The index of the cateogry to get the target ItemSet index of.</param>
        /// <param name="allowedSlotsMask">The bitwise mask indicating which slots are allowed.</param>
        /// <returns>The target ItemSet index for the specified category index.</returns>
        public int GetTargetItemSetIndex(int categoryIndex, int allowedMask)
        {
            if (categoryIndex == -1) {
                return -1;
            }
            var itemSetIndex = m_ActiveItemSetIndex[categoryIndex];

            if (IsItemSetValid(categoryIndex, itemSetIndex, false, allowedMask)) {
                return itemSetIndex;
            }

            // Check the special cases before looping through the entire item set list.
            // Determine if the previous item set is similar to the current item set.
            var itemSetListCount = m_CategoryItemSets[categoryIndex].ItemSetList.Count;
            var prevItemSetIndex = itemSetIndex - 1;
            if (prevItemSetIndex < 0) {
                prevItemSetIndex = itemSetListCount - 1;
            }
            if (itemSetIndex != -1 && prevItemSetIndex != itemSetIndex && IsItemSetValid(categoryIndex, prevItemSetIndex, false, allowedMask)) {
                for (int i = 0; i < m_CategoryItemSets[categoryIndex].ItemSetList[prevItemSetIndex].Slots.Length; ++i) {
                    var prevSlots = m_CategoryItemSets[categoryIndex].ItemSetList[prevItemSetIndex].Slots;
                    if (m_CategoryItemSets[categoryIndex].ItemSetList[itemSetIndex].Slots[i] == prevSlots[i] && prevSlots[i] != null) {
                        // At least one definition matches. Switch to that ItemSet.
                        return prevItemSetIndex;
                    }
                }
            }
            // Check the default item set.
            if (IsItemSetValid(categoryIndex, m_CategoryItemSets[categoryIndex].DefaultItemSetIndex, false, allowedMask)) {
                return m_CategoryItemSets[categoryIndex].DefaultItemSetIndex;
            }

            // Keep checking the ItemSets until a valid item set exists.
            var iterCount = 0;
            do {
                if (iterCount == itemSetListCount) {
                    // No valid ItemSet was found.
                    return -1;
                }
                iterCount++;
                itemSetIndex = (itemSetIndex + 1) % itemSetListCount;
            } while (itemSetIndex == prevItemSetIndex || itemSetIndex == m_CategoryItemSets[categoryIndex].DefaultItemSetIndex || !IsItemSetValid(categoryIndex, itemSetIndex, false, allowedMask));

            return itemSetIndex;
        }

        /// <summary>
        /// Returns true if the specified ItemSet is valid. A valid ItemSet means the character has all of the items specified in the inventory.
        /// </summary>
        /// <param name="categoryIndex">The index of the ItemSet category.</param>
        /// <param name="itemSetIndex">The ItemSet within the category.</param>
        /// <param name="checkIfCanSwitchTo">Should the ItemSet be checked if it can be switched to?</param>
        /// <param name="allowedSlotsMask">The bitwise mask indicating which slots are allowed.</param>
        /// <returns>True if the specified ItemSet is valid.</returns>
        public bool IsItemSetValid(int categoryIndex, int itemSetIndex, bool checkIfCanSwitchTo, int allowedSlotsMask = -1)
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

            var requiredCount = 0;
            var availableCount = 0;
            m_CheckedItemIdentifiers.Clear();
            for (int i = 0; i < itemSet.Slots.Length; ++i) {
                if (itemSet.Slots[i] == null) {
                    continue;
                }

                // If the ItemIdentifier is null then the item hasn't been added yet.
                if (itemSet.ItemIdentifiers[i] == null) {
                    return false;
                }

                // The item may not be in the allowed layer mask.
                if (allowedSlotsMask != -1 && m_CategoryItemSets[categoryIndex].DefaultItemSetIndex != itemSetIndex && !MathUtility.InLayerMask(i, allowedSlotsMask)) {
                    return false;
                }

                // It only takes one item for the ItemSet not to be valid.
                var item = m_Inventory.GetItem(itemSet.ItemIdentifiers[i], i);
                if (item == null) {
                    return false;
                }

                // Usable items may not be able to be equipped if they don't have any consumable ItemIdentifiers left.
                for (int j = 0; j < item.ItemActions.Length; ++j) {
                    var usableItem = item.ItemActions[j] as IUsableItem;
                    if (usableItem != null) {
                        if (!usableItem.CanEquipEmptyItem && usableItem.GetConsumableItemIdentifier() != null && m_Inventory.GetItemIdentifierAmount(usableItem.GetConsumableItemIdentifier()) == 0) {
                            return false;
                        }
                    }
                }

                // Remember the count to ensure the correct number of items exist within the inventory.
                requiredCount++;
                if (!m_CheckedItemIdentifiers.Contains(item.ItemIdentifier)) {
                    availableCount += m_Inventory.GetItemIdentifierAmount(item.ItemIdentifier);
                    m_CheckedItemIdentifiers.Add(item.ItemIdentifier);
                }
            }

            // Ensure the inventory has the number of items required for the current ItemSet.
            if (availableCount < requiredCount) {
                return false;
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
            var nextItemSetIndex = m_NextItemSetIndex[categoryIndex];
            if (nextItemSetIndex == itemSetIndex) {
                return;
            }

            var prevItemSetIndex = nextItemSetIndex != -1 ? nextItemSetIndex : m_ActiveItemSetIndex[categoryIndex];
            m_NextItemSetIndex[categoryIndex] = itemSetIndex;

            EventHandler.ExecuteEvent(m_GameObject, "OnItemSetManagerUpdateNextItemSet", categoryIndex, prevItemSetIndex, itemSetIndex);
        }

        /// <summary>
        /// Updates the active ItemSet to the specified value.
        /// </summary>
        /// <param name="categoryIndex">The category to update the ItemSet within.</param>
        /// <param name="itemSetIndex">The ItemSet to set.</param>
        /// <param name="itemIdentifiers">The active ItemIdentifiers.</param>
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
        /// Returns the ItemIdentifier which should be equipped for the specified slot.
        /// </summary>
        /// <param name="slot">The slot to get the ItemIdentifier of.</param>
        /// <returns>The ItemIdentifier which should be equipped for the specified slot. Can be null.</returns>
        public IItemIdentifier GetEquipItemIdentifier(int slot)
        {
            if (slot == -1 || slot >= m_Inventory.SlotCount) {
                return null;
            }
            for (int i = 0; i < m_CategoryItemSets.Length; ++i) {
                if (m_ActiveItemSetIndex[i] != -1) {
                    var itemSet = m_CategoryItemSets[i].ItemSetList[m_ActiveItemSetIndex[i]];
                    if (itemSet.Enabled && itemSet.ItemIdentifiers[slot] != null) {
                        return itemSet.ItemIdentifiers[slot];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the ItemIdentifier which should be equipped for the specified categoryIndex and slot.
        /// </summary>
        /// <param name="categoryIndex">The category to get the ItemIdentifier of.</param>
        /// <param name="slot">The slot to get the ItemIdentifier of.</param>
        /// <returns>The ItemIdentifier which should be equipped for the specified slot. Can be null.</returns>
        public IItemIdentifier GetEquipItemIdentifier(int categoryIndex, int slot)
        {
            if (categoryIndex == -1 || categoryIndex >= m_CategoryItemSets.Length || slot == -1 || slot >= m_Inventory.SlotCount) {
                return null;
            }
            if (m_ActiveItemSetIndex[categoryIndex] != -1) {
                var itemSet = m_CategoryItemSets[categoryIndex].ItemSetList[m_ActiveItemSetIndex[categoryIndex]];
                return itemSet.ItemIdentifiers[slot];
            }
            return null;
        }

        /// <summary>
        /// Returns the ItemIdentifier which should be equipped for the specified categoryIndex, ItemSet, and slot.
        /// </summary>
        /// <param name="categoryIndex">The category to get the ItemIdentifier of.</param>
        /// <param name="targetItemSetIndex">The ItemSet to get the ItemIdentifier of.</param>
        /// <param name="slot">The slot to get the ItemIdentifier of.</param>
        /// <returns>The ItemIdentifier which should be equipped for the specified categoryIndex, ItemIdentifier, and slot. Can be null.</returns>
        public IItemIdentifier GetEquipItemIdentifier(int categoryIndex, int targetItemSetIndex, int slot)
        {
            if (categoryIndex == -1 || categoryIndex >= m_CategoryItemSets.Length || 
                targetItemSetIndex == -1 || targetItemSetIndex >= m_CategoryItemSets[categoryIndex].ItemSetList.Count || 
                slot == -1 || slot >= m_Inventory.SlotCount) {
                return null;
            }

            return m_CategoryItemSets[categoryIndex].ItemSetList[targetItemSetIndex].ItemIdentifiers[slot];
        }

        /// <summary>
        /// Returns the ItemIdentifier which is going to be equipped next.
        /// </summary>
        /// <param name="slot">The slot to get the ItemIdentifier of.</param>
        /// <param name="categoryIndex">The category index of the found ItemIdentifier.</param>
        /// <returns>The ItemIdentifier which is going to be equipped next. Can be null.</returns>
        public IItemIdentifier GetNextItemIdentifier(int slot, out int categoryIndex)
        {
            categoryIndex = -1;
            if (slot == -1 || slot >= m_Inventory.SlotCount) {
                return null;
            }

            for (int i = 0; i < m_CategoryItemSets.Length; ++i) {
                var index = m_NextItemSetIndex[i] != -1 ? m_NextItemSetIndex[i] : m_ActiveItemSetIndex[i];
                if (index == -1) {
                    continue;
                }
                categoryIndex = i;
                return m_CategoryItemSets[i].ItemSetList[index].ItemIdentifiers[slot];
            }
            return null;
        }

        /// <summary>
        /// The character has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            for (int i = 0; i < m_CategoryItemSets.Length; ++i) {
                for (int j = 0; j < m_CategoryItemSets[i].ItemSetList.Count; ++j) {
                    m_CategoryItemSets[i].ItemSetList[j].OnDestroy();
                }
            }

            EventHandler.UnregisterEvent<Item>(gameObject, "OnInventoryAddItem", OnAddItem);
        }
    }
}