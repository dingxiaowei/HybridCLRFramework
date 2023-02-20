/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Objects.CharacterAssist;
using Opsive.UltimateCharacterController.StateSystem;
using Opsive.UltimateCharacterController.Editor.Inspectors.Inventory;
using Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Objects.CharacterAssist
{
    /// <summary>
    /// Custom inspector for the ItemPickup component.
    /// </summary>
    [CustomEditor(typeof(ItemPickup), true)]
    public class ItemPickupInspector : ObjectPickupInspector
    {
        private const int c_SlotRowHeight = 22;
        private const int c_SlotRowPadding = 4;
        private const string c_EditorPrefsSelectedPickupSetIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.ItemPickup.SelectedPickupSetIndex";
        private const string c_EditorPrefsSelectedPickupSetStateIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.ItemPickup.SelectedPickupSetStateIndex";
        private string SelectedPickupSetIndexKey { get { return c_EditorPrefsSelectedPickupSetIndexKey + "." + target.GetType() + "." + target.name; } }

        private ItemPickup m_ItemPickup;
        private ReorderableList m_ReorderablePickupSet;
        private ReorderableList m_ReorderablePickupSetStateList;
        private ReorderableList m_ReordableItemCount;
        private int m_SlotCount = 2;

        /// <summary>
        /// Determine the slot count.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            m_ItemPickup = target as ItemPickup;
            if (m_ItemPickup == null) {
                return;
            }

            if (m_ItemPickup.ItemPickupSet == null) {
                m_ItemPickup.ItemPickupSet = new ItemPickup.PickupSet[0];
            } else if (m_ItemPickup.ItemPickupSet.Length > 0) {
                m_SlotCount = m_ItemPickup.ItemPickupSet[0].ItemSet.Slots.Length;
            }
        }

        /// <summary>
        /// Draws the object pickup fields.
        /// </summary>
        protected override void DrawObjectPickupFields()
        {
            EditorGUILayout.PropertyField(PropertyFromName("m_AlwaysPickup"));

            if (Foldout("Item Pickup Set")) {
                EditorGUI.indentLevel++;
                // Only the character can determine the slot count. Because the ItemPickup isn't attached to a character the slot count
                // needs to be manually specified. OnEnable will determine the slot count of any existing ItemSets so this only needs to be done
                // when there is a change or no PickupSets specified.
                var slotCount = EditorGUILayout.IntField("Slot Count", m_SlotCount);
                if (m_SlotCount != slotCount) {
                    m_SlotCount = slotCount;
                    var pickupSet = (target as ItemPickup).ItemPickupSet;
                    for (int i = 0; i < pickupSet.Length; ++i) {
                        UpdateSlotCount(pickupSet, i);
                    }
                }

                if (m_ReordableItemCount == null) {
                    var itemListProperty = PropertyFromName("m_ItemPickupSet");
                    m_ReorderablePickupSet = new ReorderableList(serializedObject, itemListProperty, true, false, true, true);
                    m_ReorderablePickupSet.drawHeaderCallback = (Rect rect) =>
                    {
                        GUI.Label(rect, "Pickup Sets (select row to edit)");
                    };
                    m_ReorderablePickupSet.drawElementCallback = OnPickupSetElementDraw;
                    m_ReorderablePickupSet.onAddCallback += OnPickupSetListAdd;
                    m_ReorderablePickupSet.onSelectCallback = (ReorderableList list) =>
                    {
                        EditorPrefs.SetInt(SelectedPickupSetIndexKey, list.index);
                        // The pickup set's state list should start out fresh so a reference doesn't have to be cached for each pickup set.
                        m_ReorderablePickupSetStateList = null;
                    };
                    m_ReorderablePickupSet.onRemoveCallback = OnPickupSetListRemove;
                }
                if ((target as ItemPickup).ItemPickupSet.Length > 0) {
                    m_ReorderablePickupSet.elementHeight = (c_SlotRowHeight + c_SlotRowPadding) * (m_SlotCount + 1) + 21;
                } else {
                    m_ReorderablePickupSet.elementHeight = c_SlotRowHeight;
                }
                var listRect = GUILayoutUtility.GetRect(0, m_ReorderablePickupSet.GetHeight());
                listRect.x += EditorGUI.indentLevel * InspectorUtility.IndentWidth;
                listRect.xMax -= EditorGUI.indentLevel * InspectorUtility.IndentWidth;
                if (EditorPrefs.GetInt(SelectedPickupSetIndexKey, -1) != -1) {
                    m_ReorderablePickupSet.index = EditorPrefs.GetInt(SelectedPickupSetIndexKey, -1);
                }
                m_ReorderablePickupSet.DoList(listRect);

                if (m_ReorderablePickupSet.index != -1 && m_ReorderablePickupSet.index < m_ItemPickup.ItemPickupSet.Length) {
                    GUI.enabled = !Application.isPlaying;
                    DrawSelectedPickupSet(m_ItemPickup.ItemPickupSet[m_ReorderablePickupSet.index], m_ReorderablePickupSet.index);
                    GUI.enabled = true;
                }

                EditorGUI.indentLevel--;
            }

            if (Foldout("Item Type Counts")) {
                EditorGUI.indentLevel++;
                if (m_ReordableItemCount == null) {
                    var itemListProperty = PropertyFromName("m_ItemTypeCounts");
                    m_ReordableItemCount = new ReorderableList(serializedObject, itemListProperty, true, true, true, true);
                    m_ReordableItemCount.drawHeaderCallback = OnItemTypeCountHeaderDraw;
                    m_ReordableItemCount.drawElementCallback = OnItemTypeCountElementDraw;
                    m_ReordableItemCount.elementHeight = c_SlotRowHeight;
                }
                var listRect = GUILayoutUtility.GetRect(0, m_ReordableItemCount.GetHeight());
                listRect.x += EditorGUI.indentLevel * InspectorUtility.IndentWidth;
                listRect.xMax -= EditorGUI.indentLevel * InspectorUtility.IndentWidth;
                m_ReordableItemCount.DoList(listRect);
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// Draws the PickupSet ReordableList element.
        /// </summary>
        private void OnPickupSetElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            var pickupSet = (target as ItemPickup).ItemPickupSet;

            // The number of slots may have changed since the ItemType was added.
            UpdateSlotCount(pickupSet, index);

            // Draw a row background.
            var rowRect = rect;
            rowRect.y += c_SlotRowPadding / 2;
            rowRect.height -= c_SlotRowPadding;
            GUI.Label(rowRect, "", InspectorStyles.FieldStyle);

            // Draw the ItemSet title.
            var elementTitleRect = rowRect;
            elementTitleRect.y += 2;
            elementTitleRect.height = 19;
            GUI.Label(elementTitleRect, "PickupSet " + index, InspectorStyles.CenterBoldLabel);

            EditorGUI.BeginChangeCheck();

            // Each slot should have its own lighter background.
            var slotRect = rowRect;
            slotRect.x += 4;
            slotRect.width -= 8;
            slotRect.height = c_SlotRowHeight;
            for (int i = 0; i < m_SlotCount + 1; ++i) {
                slotRect.y = elementTitleRect.yMax + i * c_SlotRowHeight + 2;
                var color = GUI.color;
                GUI.color -= new Color(0.15f, 0.15f, 0.15f, 0);
                GUI.Label(slotRect, "", InspectorStyles.ItemStyle);
                GUI.color = color;

                var labelRect = slotRect;
                labelRect.x += 2;
                labelRect.y += 4;

                var objRect = slotRect;
                objRect.x += 36;
                objRect.width -= 42;
                objRect.y += (objRect.height - 16) / 2 + 1;
                objRect.height = 16;
                if (i == 0) { // Draw the item field. 
                    GUI.Label(labelRect, "Item");
                    pickupSet[index].Item = EditorGUI.ObjectField(objRect, pickupSet[index].Item, typeof(GameObject), false) as GameObject;
                } else { // Draw the slot field.
                    GUI.Label(labelRect, "Slot " + (i - 1));
                    pickupSet[index].ItemSet.Slots[i - 1] = EditorGUI.ObjectField(objRect, pickupSet[index].ItemSet.Slots[i - 1], typeof(ItemType), false) as ItemType;
                }
            }

            (target as ItemPickup).ItemPickupSet = pickupSet;

            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.SetDirty(target);
            }
        }

        /// <summary>
        /// Updates the PickupSet element at the specified index to the current SlotCount.
        /// </summary>
        private void UpdateSlotCount(ItemPickup.PickupSet[] pickupSet, int index)
        {
            if (pickupSet[index].ItemSet.Slots == null || pickupSet[index].ItemSet.Slots.Length != m_SlotCount) {
                var slots = pickupSet[index].ItemSet.Slots;
                System.Array.Resize<ItemType>(ref slots, m_SlotCount);
                pickupSet[index].ItemSet.Slots = slots;
            }
        }

        /// <summary>
        /// Draws the ItemTypeCount ReordableList header.
        /// </summary>
        private void OnItemTypeCountHeaderDraw(Rect rect)
        {
            ItemTypeCountInspector.OnItemTypeCountHeaderDraw(rect);
        }

        /// <summary>
        /// Adds a new PickupSet element.
        /// </summary>
        private void OnPickupSetListAdd(ReorderableList reorderableList)
        {
            var index = reorderableList.count;
            var list = new List<ItemPickup.PickupSet>(m_ItemPickup.ItemPickupSet);
            list.Add(new ItemPickup.PickupSet());
            m_ItemPickup.ItemPickupSet = list.ToArray();
            reorderableList.index = index;
            m_ItemPickup.ItemPickupSet[index].ItemSet.Slots = new ItemType[m_SlotCount];
            EditorPrefs.SetInt(SelectedPickupSetIndexKey, reorderableList.index);

            // The last element should start enabled if it is the first element.
            if (index == 0) {
                m_ItemPickup.ItemPickupSet[index].ItemSet.Enabled = true;
            }
            InspectorUtility.SetDirty(target);
            // The pickup set's state list should start out fresh so a reference doesn't have to be cached for each pickup set.
            m_ReorderablePickupSetStateList = null;
        }

        /// <summary> 
        /// The ReordableList remove button has been pressed. Remove the selected PickupSet.
        /// </summary>
        private void OnPickupSetListRemove(ReorderableList reorderableList)
        {
            var itemPickupSetProperty = PropertyFromName("m_ItemPickupSet");
            itemPickupSetProperty.DeleteArrayElementAtIndex(reorderableList.index);

            // Update the index to point to no longer point to the now deleted pickup set.
            reorderableList.index = reorderableList.index - 1;
            if (reorderableList.index == -1 && m_ItemPickup.ItemPickupSet.Length > 0) {
                reorderableList.index = 0;
            }
            EditorPrefs.SetInt(SelectedPickupSetIndexKey, reorderableList.index);
            serializedObject.ApplyModifiedProperties();
            // The pickup set's state list should start out fresh so a reference doesn't have to be cached for each pickup set.
            m_ReorderablePickupSetStateList = null;
        }

        /// <summary>
        /// Draws the ItemTypeCount ReordableList element.
        /// </summary>
        private void OnItemTypeCountElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            ItemTypeCountInspector.OnItemTypeCountElementDraw(PropertyFromName("m_ItemTypeCounts"), rect, index, isActive, isFocused);
        }

        /// <summary>
        /// Draws the specified item set.
        /// </summary>
        private void DrawSelectedPickupSet(ItemPickup.PickupSet pickupSet, int index)
        {
            GUILayout.Label("Pickup Set " + index, InspectorStyles.CenterBoldLabel);

            pickupSet.Item = (GameObject)EditorGUILayout.ObjectField("Item", pickupSet.Item, typeof(GameObject), false);
            if (pickupSet.Item != null) {
                // Automatically fill in the ItemType for the specified item.
                var item = pickupSet.Item.GetComponent<Item>();
                if (item != null && item.SlotID < pickupSet.ItemSet.Slots.Length) {
                    pickupSet.ItemSet.Slots[item.SlotID] = item.ItemType;
                }
            }

            pickupSet.ItemSet.State = EditorGUILayout.TextField(new GUIContent("State", "Optionally specify a state that the character should switch to when the Item Set is active."), pickupSet.ItemSet.State);
            // Draws all of the slots ItemTypes.
            for (int i = 0; i < m_SlotCount; ++i) {
                pickupSet.ItemSet.Slots[i] = (ItemType)EditorGUILayout.ObjectField("Slot " + i, pickupSet.ItemSet.Slots[i], typeof(ItemType), false);
            }
            pickupSet.Default = EditorGUILayout.Toggle(new GUIContent("Default", "True if the ItemSet is the default Item Set."), pickupSet.Default);
            pickupSet.ItemSet.Enabled = EditorGUILayout.Toggle(new GUIContent("Enabled", "True if the ItemSet can be equipped."), pickupSet.ItemSet.Enabled);
            pickupSet.ItemSet.CanSwitchTo = EditorGUILayout.Toggle(new GUIContent("Can Switch To", "True if the ItemSet can be switched to by the EquipNext/EquipPrevious abilities."), pickupSet.ItemSet.CanSwitchTo);
            pickupSet.ItemSet.DisabledIndex = EditorGUILayout.IntField(new GUIContent("Disabled Index", "The ItemSet that should be activated if the current ItemSet is disabled."), pickupSet.ItemSet.DisabledIndex);

            if (InspectorUtility.Foldout(pickupSet, new GUIContent("States"), false)) {
                // The MovementType class derives from system.object at the base level and reorderable lists can only operate on Unity objects. To get around this restriction
                // create a dummy array within a Unity object that corresponds to the number of elements within the ability's state list. When the reorderable list is drawn
                // the ability object will be used so it's like the dummy object never existed.
                var gameObject = new GameObject();
                var stateIndexHelper = gameObject.AddComponent<StateInspectorHelper>();
                stateIndexHelper.StateIndexData = new int[pickupSet.ItemSet.States.Length];
                for (int i = 0; i < stateIndexHelper.StateIndexData.Length; ++i) {
                    stateIndexHelper.StateIndexData[i] = i;
                }
                var stateIndexSerializedObject = new SerializedObject(stateIndexHelper);
                m_ReorderablePickupSetStateList = StateInspector.DrawStates(m_ReorderablePickupSetStateList, serializedObject,
                                                            stateIndexSerializedObject.FindProperty("m_StateIndexData"),
                                                            GetSelectedPickupSetStateIndexKey(index), OnPickupSetStateListDraw, OnPickupSetStateListAdd,
                                                            OnPickupSetStateListReorder, OnPickupSetStateListRemove);
                DestroyImmediate(gameObject);
            }
        }

        /// <summary>
        /// Returns the state index key for the specified pickup set type.
        /// </summary>
        private string GetSelectedPickupSetStateIndexKey(int itemSetIndex)
        {
            return c_EditorPrefsSelectedPickupSetStateIndexKey + "." + target.GetType() + "." + target.name + "." + " " + itemSetIndex;
        }

        /// <summary>
        /// Draws all of the added states.
        /// </summary>
        private void OnPickupSetStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            var pickupSet = m_ItemPickup.ItemPickupSet[EditorPrefs.GetInt(SelectedPickupSetIndexKey, -1)];

            // The index may be out of range if the component was copied.
            if (index >= pickupSet.ItemSet.States.Length) {
                m_ReorderablePickupSetStateList.index = -1;
                return;
            }

            StateInspector.OnStateListDraw(pickupSet.ItemSet, pickupSet.ItemSet.States, rect, index);
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                StateInspector.UpdateDefaultStateValues(pickupSet.ItemSet.States);
            }
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnPickupSetStateListAdd(ReorderableList list)
        {
            StateInspector.OnStateListAdd(AddExistingPickupSetPreset, CreatePickupSetPreset);
        }
        /// <summary>
        /// Adds a new element to the state list which uses an existing preset.
        /// </summary>
        private void AddExistingPickupSetPreset()
        {
            var pickupSet = m_ItemPickup.ItemPickupSet[EditorPrefs.GetInt(SelectedPickupSetIndexKey, -1)];
            var states = StateInspector.AddExistingPreset(pickupSet.ItemSet.GetType(), pickupSet.ItemSet.States, m_ReorderablePickupSetStateList,
                GetSelectedPickupSetStateIndexKey(EditorPrefs.GetInt(SelectedPickupSetIndexKey, -1)));
            if (pickupSet.ItemSet.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderablePickupSetStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                pickupSet.ItemSet.States = states;
            }
        }

        /// <summary>
        /// Creates a new preset and adds it to a new state in the list.
        /// </summary>
        private void CreatePickupSetPreset()
        {
            var pickupSet = m_ItemPickup.ItemPickupSet[EditorPrefs.GetInt(SelectedPickupSetIndexKey, -1)];
            var states = StateInspector.CreatePreset(pickupSet.ItemSet, pickupSet.ItemSet.States, m_ReorderablePickupSetStateList,
                GetSelectedPickupSetStateIndexKey(EditorPrefs.GetInt(SelectedPickupSetIndexKey, -1)));
            if (pickupSet.ItemSet.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderablePickupSetStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                pickupSet.ItemSet.States = states;
            }
        }

        /// <summary>
        /// The list has been reordered. Ensure the reorder is valid.
        /// </summary>
        private void OnPickupSetStateListReorder(ReorderableList list)
        {
            var pickupSet = m_ItemPickup.ItemPickupSet[EditorPrefs.GetInt(SelectedPickupSetIndexKey, -1)];

            // Use the dummy array in order to determine what element the selected index was swapped with.
            var copiedStates = new UltimateCharacterController.StateSystem.State[pickupSet.ItemSet.States.Length];
            System.Array.Copy(pickupSet.ItemSet.States, copiedStates, pickupSet.ItemSet.States.Length);
            for (int i = 0; i < pickupSet.ItemSet.States.Length; ++i) {
                var element = list.serializedProperty.GetArrayElementAtIndex(i);
                if (element.intValue != i) {
                    pickupSet.ItemSet.States[i] = copiedStates[element.intValue];
                    element.intValue = i;
                }
            }

            var states = StateInspector.OnStateListReorder(pickupSet.ItemSet.States);
            if (pickupSet.ItemSet.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderablePickupSetStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                pickupSet.ItemSet.States = states;
            }
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        private void OnPickupSetStateListRemove(ReorderableList list)
        {
            var pickupSet = m_ItemPickup.ItemPickupSet[EditorPrefs.GetInt(SelectedPickupSetIndexKey, -1)];
            var states = StateInspector.OnStateListRemove(pickupSet.ItemSet.States, GetSelectedPickupSetStateIndexKey(EditorPrefs.GetInt(SelectedPickupSetIndexKey, -1)), list);
            if (pickupSet.ItemSet.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderablePickupSetStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                pickupSet.ItemSet.States = states;
            }
        }
    }
}