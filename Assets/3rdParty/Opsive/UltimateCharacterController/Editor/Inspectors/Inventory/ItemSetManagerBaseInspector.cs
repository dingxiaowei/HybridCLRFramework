/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Inventory
{
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.StateSystem;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    /// <summary>
    /// Custom inspector for the ItemSetManagerBase component.
    /// </summary>
    [CustomEditor(typeof(ItemSetManagerBase))]
    public abstract class ItemSetManagerBaseInspector : InspectorBase
    {
        private const int c_SlotRowHeight = 22;
        private const int c_SlotRowPadding = 4;
        private const string c_EditorPrefsSelectedItemSetIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.ItemSets.SelectedItemSetIndex";
        private const string c_EditorPrefsSelectedItemSetStateIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.ItemSets.SelectedItemSetStateIndex";
        private string SelectedItemSetIndexKey { get { return c_EditorPrefsSelectedItemSetIndexKey + "." + target.GetType() + "." + target.name + "." + m_ItemSetListIndex; } }

        protected ReorderableList[] m_ItemSetReorderableList;
        private ReorderableList[] m_ReorderableItemSetStateList;
        private Dictionary<ReorderableList, int> m_ReorderableListCategoryMap = new Dictionary<ReorderableList, int>();

        protected ItemSetManagerBase m_ItemSetManager;
        private InventoryBase m_InventoryBase;
        private int m_ItemSetListIndex;
        private int m_ItemSetListIndexAdd;

        /// <summary>
        /// Initialize the ItemSetManager.
        /// </summary>
        public void OnEnable()
        {
            m_ItemSetManager = target as ItemSetManagerBase;
            m_InventoryBase = m_ItemSetManager.GetComponent<InventoryBase>();

            m_ItemSetManager.Initialize(false);
        }

        /// <summary>
        /// Draws the inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();

            if (InitializeCategories()) {
                if (m_ItemSetReorderableList == null) {
                    m_ItemSetReorderableList = new ReorderableList[m_ItemSetManager.CategoryItemSets.Length];
                    m_ReorderableItemSetStateList = new ReorderableList[m_ItemSetManager.CategoryItemSets.Length];
                    GUI.changed = true;
                }
                for (int i = 0; i < m_ItemSetReorderableList.Length; ++i) {
                    // All of the callbacks are the same for the list of sets. Keep an index so the method can determine which ItemSet is affected.
                    m_ItemSetListIndex = i;
                    if (m_ItemSetReorderableList[i] == null) {
                        m_ItemSetReorderableList[i] = new ReorderableList(m_ItemSetManager.CategoryItemSets[i].ItemSetList, typeof(ItemSet), true, true, true, true);
                        m_ItemSetReorderableList[i].drawHeaderCallback = (Rect rect) =>
                        {
                            GUI.Label(rect, m_ItemSetManager.CategoryItemSets[m_ItemSetListIndex].CategoryName + " (select row to edit)");
                        };
                        m_ItemSetReorderableList[i].drawElementCallback = OnItemSetElementDraw;
                        m_ItemSetReorderableList[i].onSelectCallback = (ReorderableList list) =>
                        {
                            EditorPrefs.SetInt(SelectedItemSetIndexKey, list.index);
                            // The item set's state list should start out fresh so a reference doesn't have to be cached for each item set.
                            m_ReorderableItemSetStateList[m_ItemSetListIndex] = null;
                        };
                        m_ItemSetReorderableList[i].onReorderCallback += OnItemSetElementReorder;
                        m_ItemSetReorderableList[i].onAddCallback = OnItemSetListAdd;
                        m_ItemSetReorderableList[i].onRemoveCallback = OnItemSetListRemove;
                        GUI.changed = true;
                    }
                    m_ItemSetReorderableList[i].draggable = !Application.isPlaying;
                    if (m_ItemSetManager.CategoryItemSets[i].ItemSetList.Count > 0) {
                        m_ItemSetReorderableList[i].elementHeight = (c_SlotRowHeight + c_SlotRowPadding) * m_InventoryBase.SlotCount + 25;
                    } else {
                        m_ItemSetReorderableList[i].elementHeight = c_SlotRowHeight;
                    }
                    if (EditorPrefs.GetInt(SelectedItemSetIndexKey, -1) != -1) {
                        m_ItemSetReorderableList[i].index = EditorPrefs.GetInt(SelectedItemSetIndexKey, -1);
                    }
                    m_ItemSetReorderableList[i].DoLayoutList();

                    if (m_ItemSetReorderableList[i].index != -1 && m_ItemSetReorderableList[i].index < m_ItemSetManager.CategoryItemSets[i].ItemSetList.Count) {
                        GUI.enabled = !Application.isPlaying;
                        DrawSelectedItemSet(m_ItemSetManager.CategoryItemSets[i].ItemSetList[m_ItemSetReorderableList[i].index], m_ItemSetReorderableList[i].index);
                        GUI.enabled = true;
                    }
                }
            }

            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Initializes the ItemSet categories.
        /// </summary>
        /// <returns>True if the categories were initialized.</returns>
        protected abstract bool InitializeCategories();

        /// <summary>
        /// Ensures all of the ItemSet categories are valid.
        /// </summary>
        /// <param name="categories">The categories that the ItemSet contains.</param>
        protected void CheckCategories(IItemCategoryIdentifier[] categories)
        {
            var categoryItemSets = m_ItemSetManager.CategoryItemSets;
            if (categoryItemSets == null) {
                return;
            }

            for (int i = categoryItemSets.Length - 1; i >= 0; --i) {
                var hasCategory = false;
                for (int j = 0; j < categories.Length; ++j) {
                    if (categoryItemSets[i].CategoryID == categories[j].ID && IsItemSetCategory(categories[j])) {
                        if (categoryItemSets[i].CategoryName != categories[j].ToString()) {
                            categoryItemSets[i].CategoryName = categories[j].ToString();
                            GUI.changed = true;
                        }
                        if (categoryItemSets[i].ItemCategory != categories[j]) {
                            categoryItemSets[i].ItemCategory = categories[j];
                            GUI.changed = true;
                        }
                        hasCategory = true;
                        break;
                    }
                }
                if (!hasCategory) {
                    var categoryList = new List<CategoryItemSet>(categoryItemSets);
                    categoryList.RemoveAt(i);
                    m_ItemSetManager.CategoryItemSets = categoryItemSets = categoryList.ToArray();
                    m_ItemSetReorderableList = null;
                    GUI.changed = true;
                }
            }

            var nonItemSetCategoryCount = 0;
            for (int i = 0; i < categories.Length; ++i) {
                if (!IsItemSetCategory(categories[i])) {
                    nonItemSetCategoryCount++;
                }
            }

            // The number of categories must match.
            var diff = categories.Length - nonItemSetCategoryCount - categoryItemSets.Length;
            if (diff > 0) {
                System.Array.Resize(ref categoryItemSets, categoryItemSets.Length + diff);
                for (int i = categoryItemSets.Length - diff; i < categoryItemSets.Length; ++i) {
                    // A category has been added. Populate the new category.
                    for (int j = 0; j < categories.Length; ++j) {
                        if (!IsItemSetCategory(categories[j])) {
                            continue;
                        }
                        var hasCategory = false;
                        for (int k = 0; k < categoryItemSets.Length; ++k) {
                            if (categoryItemSets[k] != null && categoryItemSets[k].CategoryID == categories[j].ID) {
                                hasCategory = true;
                                break;
                            }
                        }
                        // Don't add an existing category.
                        if (hasCategory) {
                            continue;
                        }
                        categoryItemSets[i] = new CategoryItemSet(categories[j].ID, categories[j].ToString(), categories[j]);
                        break;
                    }
                }
                m_ItemSetManager.CategoryItemSets = categoryItemSets;
                m_ItemSetReorderableList = null;
                GUI.changed = true;
            } else if (diff < 0) {
                var updatedCategoryItemSets = new CategoryItemSet[categoryItemSets.Length + diff];
                var insertCount = 0;
                // Remove the ItemSet that no longer has a category.
                for (int i = 0; i < categoryItemSets.Length; ++i) {
                    var categoryID = categoryItemSets[i].CategoryID;
                    var hasCategory = false;
                    for (int j = 0; j < categories.Length; ++j) {
                        if (categoryID == categories[j].ID && IsItemSetCategory(categories[j])) {
                            hasCategory = true;
                            break;
                        }
                    }
                    // Add the CategoryItemSet to the list if it exists. If the category ID no longer exists then don't transfer it to the new array.
                    if (hasCategory) {
                        updatedCategoryItemSets[insertCount] = categoryItemSets[i];
                        insertCount++;
                    }
                }
                m_ItemSetManager.CategoryItemSets = updatedCategoryItemSets;
                m_ItemSetReorderableList = null;
                GUI.changed = true;
            }
        }

        /// <summary>
        /// Is the category an ItemSet category?
        /// </summary>
        /// <param name="category">The category that may be an ItemSet category.</param>
        /// <returns>True if the category is an ItemSet category.</returns>
        protected virtual bool IsItemSetCategory(IItemCategoryIdentifier category) { return true; }

        /// <summary>
        /// Ensures all of the ItemSetAbilityBase abilities point to the correct ItemCollection.
        /// </summary>
        /// <param name="categories">The categories that the ItemSet contains.</param>
        protected void CheckItemSetAbilities(IItemCategoryIdentifier[] categories)
        {
            var characterLocomotion = m_ItemSetManager.gameObject.GetComponent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            characterLocomotion.DeserializeItemAbilities();
            var itemAbilities = characterLocomotion.ItemAbilities;
            if (itemAbilities == null) {
                return;
            }

            var changed = false;
            for (int i = 0; i < itemAbilities.Length; ++i) {
                // Only check the ItemSetAbilityBase abilities.
                if (!(itemAbilities[i] is ItemSetAbilityBase)) {
                    continue;
                }

                var itemSetAbilityBase = itemAbilities[i] as ItemSetAbilityBase;
                var hasCategory = false;
                for (int j = 0; j < categories.Length; ++j) {
                    if (categories[j].ID == itemSetAbilityBase.ItemSetCategoryID && IsItemSetCategory(categories[j])) {
                        hasCategory = true;
                        break;
                    }
                }

                // If the category doesn't exist then it should be reset.
                if (!hasCategory) {
                    for (int j = 0; j < categories.Length; ++j) {
                        if (IsItemSetCategory(categories[j])) {
                            InspectorUtility.SetFieldValue(itemSetAbilityBase, "m_ItemSetCategoryID", categories[j].ID);
                            changed = true;
                            break;
                        }
                    }
                }
            }

            if (changed) {
                GUI.changed = true;
                UltimateCharacterController.Utility.Builders.AbilityBuilder.SerializeItemAbilities(characterLocomotion);
                InspectorUtility.SetDirty(characterLocomotion);
            }
        }

        /// <summary>
        /// Draws the ItemSet elemeent.
        /// </summary>
        private void OnItemSetElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            // The number of slots may have changed since the ItemSet was added.
            var itemSet = m_ItemSetManager.CategoryItemSets[m_ItemSetListIndex].ItemSetList;
            if (itemSet[index].Slots == null || itemSet[index].Slots.Length != m_InventoryBase.SlotCount) {
                var slots = itemSet[index].Slots;
                System.Array.Resize<ItemDefinitionBase>(ref slots, m_InventoryBase.SlotCount);
                itemSet[index].Slots = slots;
            }

            // Draw a row background.
            var rowRect = rect;
            rowRect.y += c_SlotRowPadding / 2;
            rowRect.height -= c_SlotRowPadding;
            GUI.Label(rowRect, "", InspectorStyles.FieldStyle);

            // Draw the ItemSet title.
            var elementTitleRect = rowRect;
            elementTitleRect.y += 2;
            elementTitleRect.height = 19;
            var itemSetActive = Application.isPlaying && !EditorUtility.IsPersistent(m_ItemSetManager) && m_ItemSetManager.gameObject.activeSelf &&
                                        m_ItemSetManager.ActiveItemSetIndex != null &&
                                        m_ItemSetListIndex < m_ItemSetManager.ActiveItemSetIndex.Length && m_ItemSetManager.ActiveItemSetIndex[m_ItemSetListIndex] == index;
            GUI.Label(elementTitleRect, "Item Set " + index + (itemSetActive ? " (Active)" : ""), InspectorStyles.CenterBoldLabel);

            // Each slot should have its own lighter background.
            var slotRect = rowRect;
            slotRect.x += 4;
            slotRect.width -= 8;
            slotRect.height = c_SlotRowHeight;

            for (int i = 0; i < m_InventoryBase.SlotCount; ++i) {
                slotRect.y = elementTitleRect.yMax + i * c_SlotRowHeight + 2;
                var color = GUI.color;
                GUI.color -= new Color(0.15f, 0.15f, 0.15f, 0);
                GUI.Label(slotRect, "", InspectorStyles.ItemStyle);
                GUI.color = color;

                var labelRect = slotRect;
                labelRect.x += 2;
                labelRect.y += 4;

                var objRect = slotRect;
                objRect.x += 56;
                objRect.width -= 62;
                objRect.y += (objRect.height - 16) / 2 + 1;
                objRect.height = 16;

                GUI.Label(labelRect, "Slot " + i);
                itemSet[index].Slots[i] = EditorGUI.ObjectField(objRect, itemSet[index].Slots[i], typeof(ItemDefinitionBase), false) as ItemDefinitionBase;
            }
        }

        /// <summary>
        /// The ItemSet list has been reordered.
        /// </summary>
        private void OnItemSetElementReorder(ReorderableList reorderableList)
        {
            var prevIndex = EditorPrefs.GetInt(SelectedItemSetIndexKey, -1);
            // The default index should swap with the ItemSet that was swapped.
            if (prevIndex != -1) {
                var defaultIndex = m_ItemSetManager.CategoryItemSets[m_ItemSetListIndex].DefaultItemSetIndex;
                if (defaultIndex == prevIndex) {
                    m_ItemSetManager.CategoryItemSets[m_ItemSetListIndex].DefaultItemSetIndex = reorderableList.index;
                } else if (defaultIndex == reorderableList.index) {
                    m_ItemSetManager.CategoryItemSets[m_ItemSetListIndex].DefaultItemSetIndex = prevIndex;
                } else if (defaultIndex < prevIndex && defaultIndex > reorderableList.index) {
                    m_ItemSetManager.CategoryItemSets[m_ItemSetListIndex].DefaultItemSetIndex++;
                } else if (defaultIndex > prevIndex && defaultIndex < reorderableList.index) {
                    m_ItemSetManager.CategoryItemSets[m_ItemSetListIndex].DefaultItemSetIndex--;
                }
            }

            serializedObject.ApplyModifiedProperties();
            InspectorUtility.SetDirty(target);
            EditorPrefs.SetInt(SelectedItemSetIndexKey, reorderableList.index);
        }

        /// <summary>
        /// Adds a new ItemSet element.
        /// </summary>
        private void OnItemSetListAdd(ReorderableList reorderableList)
        {
            reorderableList.list.Add(new ItemSet(m_InventoryBase.SlotCount, 0, null, null, string.Empty));

            // Select the newly added effect.
            reorderableList.index = reorderableList.list.Count - 1;
            EditorPrefs.SetInt(SelectedItemSetIndexKey, reorderableList.index);
            // The item set's state list should start out fresh so a reference doesn't have to be cached for each item set.
            m_ReorderableItemSetStateList[m_ItemSetListIndex] = null;
        }

        /// <summary> 
        /// The ReordableList remove button has been pressed. Remove the selected ItemSet.
        /// </summary>
        private void OnItemSetListRemove(ReorderableList reorderableList)
        {
            if (m_ItemSetManager.CategoryItemSets[m_ItemSetListIndex].DefaultItemSetIndex == reorderableList.index) {
                m_ItemSetManager.CategoryItemSets[m_ItemSetListIndex].DefaultItemSetIndex = -1;
            }

            reorderableList.list.RemoveAt(reorderableList.index);

            // Update the index to point to no longer point to the now deleted item set.
            reorderableList.index = reorderableList.index - 1;
            if (reorderableList.index == -1 && m_ItemSetManager.CategoryItemSets[m_ItemSetListIndex].ItemSetList.Count > 0) {
                reorderableList.index = 0;
            }
            EditorPrefs.SetInt(SelectedItemSetIndexKey, reorderableList.index);
            // The item set's state list should start out fresh so a reference doesn't have to be cached for each item set.
            m_ReorderableItemSetStateList[m_ItemSetListIndex] = null;
        }

        /// <summary>
        /// Draws the specified item set.
        /// </summary>
        private void DrawSelectedItemSet(ItemSet itemSet, int index)
        {
            GUILayout.Label("Item Set " + index, InspectorStyles.CenterBoldLabel);

            itemSet.State = EditorGUILayout.TextField(new GUIContent("State", "Optionally specify a state that the character should switch to when the Item Set is active."), itemSet.State);

            // Draws all of the slots ItemDefinitions.
            for (int i = 0; i < m_InventoryBase.SlotCount; ++i) {
                var itemDefinition = (ItemDefinitionBase)EditorGUILayout.ObjectField("Slot " + i, itemSet.Slots[i], typeof(ItemDefinitionBase), false);
                // The ItemIdentifier must belong to the parent category.
                if (itemDefinition != null && m_ItemSetManager.IsCategoryMember(itemDefinition, m_ItemSetListIndex)) {
                    itemSet.Slots[i] = itemDefinition;
                    if (Application.isPlaying) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField("Item Identifier", itemSet.ItemIdentifiers[i] == null ? "(none)" : itemSet.ItemIdentifiers[i].ToString());
                        EditorGUI.indentLevel--;
                    }
                } else {
                    itemSet.Slots[i] = null;
                    if (itemDefinition != null) {
                        Debug.LogError($"Error: Unable to add ItemDefinition {itemDefinition.name}. The ItemDefinition category doesn't match the parent category.");
                    }
                }
            }

            var isDefaultIndex = m_ItemSetManager.CategoryItemSets[m_ItemSetListIndex].DefaultItemSetIndex == index;
            var isDefaultIndexToggle = EditorGUILayout.Toggle(new GUIContent("Default", "True if the Item Set is the default Item Set."), isDefaultIndex);
            if (isDefaultIndex != isDefaultIndexToggle) {
                m_ItemSetManager.CategoryItemSets[m_ItemSetListIndex].DefaultItemSetIndex = isDefaultIndexToggle ? index : -1;
            }

            itemSet.Enabled = EditorGUILayout.Toggle(new GUIContent("Enabled", "True if the Item Set can be equipped."), itemSet.Enabled);
            itemSet.CanSwitchTo = EditorGUILayout.Toggle(new GUIContent("Can Switch To", "True if the ItemSet can be switched to by the EquipNext/EquipPrevious abilities."), itemSet.CanSwitchTo);
            itemSet.DisabledIndex = EditorGUILayout.IntField(new GUIContent("Disabled Index", "The ItemSet that should be activated if the current ItemSet is disabled."), itemSet.DisabledIndex);

            if (InspectorUtility.Foldout(itemSet, new GUIContent("States"), false)) {
                // The MovementType class derives from system.object at the base level and reorderable lists can only operate on Unity objects. To get around this restriction
                // create a dummy array within a Unity object that corresponds to the number of elements within the ability's state list. When the reorderable list is drawn
                // the ability object will be used so it's like the dummy object never existed.
                var gameObject = new GameObject();
                var stateIndexHelper = gameObject.AddComponent<StateInspectorHelper>();
                stateIndexHelper.StateIndexData = new int[itemSet.States.Length];
                for (int i = 0; i < stateIndexHelper.StateIndexData.Length; ++i) {
                    stateIndexHelper.StateIndexData[i] = i;
                }
                var stateIndexSerializedObject = new SerializedObject(stateIndexHelper);
                m_ReorderableItemSetStateList[m_ItemSetListIndex] = StateInspector.DrawStates(m_ReorderableItemSetStateList[m_ItemSetListIndex], serializedObject, 
                                                            stateIndexSerializedObject.FindProperty("m_StateIndexData"),
                                                            GetSelectedItemSetStateIndexKey(index), OnItemSetStateListDraw, OnItemSetStateListAdd,
                                                            OnItemSetStateListReorder, OnItemSetStateListRemove);
                if (!m_ReorderableListCategoryMap.ContainsKey(m_ReorderableItemSetStateList[m_ItemSetListIndex])) {
                    m_ReorderableListCategoryMap.Add(m_ReorderableItemSetStateList[m_ItemSetListIndex], m_ItemSetListIndex);
                }
                DestroyImmediate(gameObject);
            }
        }

        /// <summary>
        /// Returns the state index key for the specified item set type.
        /// </summary>
        private string GetSelectedItemSetStateIndexKey(int itemSetIndex)
        {
            return c_EditorPrefsSelectedItemSetStateIndexKey + "." + target.GetType() + "." + target.name + "." + m_ItemSetListIndex + " " + itemSetIndex;
        }

        /// <summary>
        /// Draws all of the added states.
        /// </summary>
        private void OnItemSetStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            if (m_ItemSetListIndex >= m_ItemSetManager.CategoryItemSets.Length ||
                EditorPrefs.GetInt(SelectedItemSetIndexKey, -1) == -1 ||
                EditorPrefs.GetInt(SelectedItemSetIndexKey, -1) >= m_ItemSetManager.CategoryItemSets[m_ItemSetListIndex].ItemSetList.Count) {
                m_ReorderableItemSetStateList[m_ItemSetListIndex].index = -1;
                return;
            }

            var itemSet = m_ItemSetManager.CategoryItemSets[m_ItemSetListIndex].ItemSetList[EditorPrefs.GetInt(SelectedItemSetIndexKey, -1)];

            // The index may be out of range if the component was copied.
            if (index >= itemSet.States.Length) {
                m_ReorderableItemSetStateList[m_ItemSetListIndex].index = -1;
                return;
            }

            StateInspector.OnStateListDraw(itemSet, itemSet.States, rect, index);
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                StateInspector.UpdateDefaultStateValues(itemSet.States);
            }
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnItemSetStateListAdd(ReorderableList list)
        {
            m_ItemSetListIndexAdd = m_ReorderableListCategoryMap[list];
            StateInspector.OnStateListAdd(AddExistingItemSetPreset, CreateItemSetPreset);
        }

        /// <summary>
        /// Adds a new element to the state list which uses an existing preset.
        /// </summary>
        private void AddExistingItemSetPreset()
        {
            m_ItemSetListIndex = m_ItemSetListIndexAdd;
            var itemSet = m_ItemSetManager.CategoryItemSets[m_ItemSetListIndex].ItemSetList[EditorPrefs.GetInt(SelectedItemSetIndexKey, -1)];
            var states = StateInspector.AddExistingPreset(itemSet.GetType(), itemSet.States, m_ReorderableItemSetStateList[m_ItemSetListIndex], 
                GetSelectedItemSetStateIndexKey(EditorPrefs.GetInt(SelectedItemSetIndexKey, -1)));
            if (itemSet.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableItemSetStateList[m_ItemSetListIndex].serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                itemSet.States = states;
            }
        }

        /// <summary>
        /// Creates a new preset and adds it to a new state in the list.
        /// </summary>
        private void CreateItemSetPreset()
        {
            m_ItemSetListIndex = m_ItemSetListIndexAdd;
            var itemSet = m_ItemSetManager.CategoryItemSets[m_ItemSetListIndex].ItemSetList[EditorPrefs.GetInt(SelectedItemSetIndexKey, -1)];
            var states = StateInspector.CreatePreset(itemSet, itemSet.States, m_ReorderableItemSetStateList[m_ItemSetListIndex], 
                GetSelectedItemSetStateIndexKey(EditorPrefs.GetInt(SelectedItemSetIndexKey, -1)));
            if (itemSet.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableItemSetStateList[m_ItemSetListIndex].serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                itemSet.States = states;
            }
        }

        /// <summary>
        /// The list has been reordered. Ensure the reorder is valid.
        /// </summary>
        private void OnItemSetStateListReorder(ReorderableList list)
        {
            m_ItemSetListIndex = m_ReorderableListCategoryMap[list];
            var itemSet = m_ItemSetManager.CategoryItemSets[m_ItemSetListIndex].ItemSetList[EditorPrefs.GetInt(SelectedItemSetIndexKey, -1)];

            // Use the dummy array in order to determine what element the selected index was swapped with.
            var copiedStates = new UltimateCharacterController.StateSystem.State[itemSet.States.Length];
            System.Array.Copy(itemSet.States, copiedStates, itemSet.States.Length);
            for (int i = 0; i < itemSet.States.Length; ++i) {
                var element = list.serializedProperty.GetArrayElementAtIndex(i);
                if (element.intValue != i) {
                    itemSet.States[i] = copiedStates[element.intValue];
                    element.intValue = i;
                }
            }

            var states = StateInspector.OnStateListReorder(itemSet.States);
            if (itemSet.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableItemSetStateList[m_ItemSetListIndex].serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                itemSet.States = states;
            }
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        private void OnItemSetStateListRemove(ReorderableList list)
        {
            m_ItemSetListIndex = m_ReorderableListCategoryMap[list];
            var itemSet = m_ItemSetManager.CategoryItemSets[m_ItemSetListIndex].ItemSetList[EditorPrefs.GetInt(SelectedItemSetIndexKey, -1)];
            var states = StateInspector.OnStateListRemove(itemSet.States, GetSelectedItemSetStateIndexKey(EditorPrefs.GetInt(SelectedItemSetIndexKey, -1)), list);
            if (itemSet.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableItemSetStateList[m_ItemSetListIndex].serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                itemSet.States = states;
            }
        }
    }
}