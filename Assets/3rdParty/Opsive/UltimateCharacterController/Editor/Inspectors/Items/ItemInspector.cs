/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Items
{
    using Opsive.UltimateCharacterController.Editor.Inspectors.Items.AnimatorAudioState;
    using Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Inventory;
    using System;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    /// <summary>
    /// Shows a custom inspector for the Item component.
    /// </summary>
    [CustomEditor(typeof(Item), true)]
    public class ItemInspector : StateBehaviorInspector
    {
        private const string c_EditorPrefsSelectedEquipAnimatorAudioStateSetIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedEquipAnimatorAudioStateSetIndex";
        private const string c_EditorPrefsSelectedEquipAnimatorAudioStateSetStateIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedEquipAnimatorAudioStateSetStateIndex";
        private const string c_EditorPrefsSelectedUnequipAnimatorAudioStateSetIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedUnequipAnimatorAudioStateSetIndex";
        private const string c_EditorPrefsSelectedUnequipAnimatorAudioStateSetStateIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedUnequipAnimatorAudioStateSetStateIndex";
        private string SelectedEquipAnimatorAudioStateSetIndexKey { get { return c_EditorPrefsSelectedEquipAnimatorAudioStateSetIndexKey + "." + target.GetType() + "." + target.name; } }
        private string SelectedUnequipAnimatorAudioStateSetIndexKey { get { return c_EditorPrefsSelectedUnequipAnimatorAudioStateSetIndexKey + "." + target.GetType() + "." + target.name; } }

        private Item m_Item;
        private ReorderableList m_ReorderableEquipAnimatorAudioStateSetList;
        private ReorderableList m_ReorderableEquipAnimatorAudioStateSetAudioList;
        private ReorderableList m_ReorderableEquipAnimatorAudioStateSetStateList;
        private ReorderableList m_ReorderableUnequipAnimatorAudioStateSetList;
        private ReorderableList m_ReorderableUnequipAnimatorAudioStateSetAudioList;
        private ReorderableList m_ReorderableUnequipAnimatorAudioStateSetStateList;

        /// <summary>
        /// Initialize any starting values.
        /// </summary>
        protected override void OnEnable()
        {
            m_Item = target as Item;

            // After an undo or redo has been performed the animator parameter states need to be deserialized.
            Undo.undoRedoPerformed += OnUndoRedo;

            m_Item.EquipAnimatorAudioStateSet.DeserializeAnimatorAudioStateSelector(m_Item, m_Item.GetComponentInParent<UltimateCharacterController.Character.UltimateCharacterLocomotion>());
            m_Item.UnequipAnimatorAudioStateSet.DeserializeAnimatorAudioStateSelector(m_Item, m_Item.GetComponentInParent<UltimateCharacterController.Character.UltimateCharacterLocomotion>());
        }

        /// <summary>
        /// Perform any cleanup when the inspector has been disabled.
        /// </summary>
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        /// <summary>
        /// Returns the actions to draw before the State list is drawn.
        /// </summary>
        /// <returns>The actions to draw before the State list is drawn.</returns>
        protected override Action GetDrawCallback()
        {
            var baseCallback = base.GetDrawCallback();

            baseCallback += () =>
            {
                var itemDefinition = PropertyFromName("m_ItemDefinition");
                EditorGUILayout.PropertyField(itemDefinition);
                if (itemDefinition == null) {
                    EditorGUILayout.HelpBox("An Item Definition is required.", MessageType.Error);
                } else {
                    // Ensure the Item Definition exists within the collection set by the Item Set Manager.
                    var itemSetManager = (target as Item).GetComponentInParent<ItemSetManager>();
                    if (itemSetManager != null && itemSetManager.ItemCollection != null) {
                        if (AssetDatabase.GetAssetPath(itemDefinition.objectReferenceValue) != AssetDatabase.GetAssetPath(itemSetManager.ItemCollection)) {
                            EditorGUILayout.HelpBox("The Item Definition must exist within the Item Collection specified on the character's Item Set Manager.", MessageType.Error);
                        }
                    }
                }
                if (Application.isPlaying) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("Item Identifier", m_Item.ItemIdentifier == null ? "(none)" : m_Item.ItemIdentifier.ToString());
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(PropertyFromName("m_SlotID"));
                EditorGUILayout.PropertyField(PropertyFromName("m_AnimatorItemID"));
                EditorGUILayout.PropertyField(PropertyFromName("m_AnimatorMovementSetID"));
                EditorGUILayout.PropertyField(PropertyFromName("m_DominantItem"));
                EditorGUILayout.PropertyField(PropertyFromName("m_UniqueItemSet"));
                EditorGUILayout.PropertyField(PropertyFromName("m_AllowCameraZoom"));
                EditorGUILayout.PropertyField(PropertyFromName("m_DropPrefab"));
                if (PropertyFromName("m_DropPrefab").objectReferenceValue != null) {
                    EditorGUI.indentLevel++;
#if ULTIMATE_CHARACTER_CONTROLLER_VR
                    EditorGUILayout.PropertyField(PropertyFromName("m_DropVelocityMultiplier"));
#endif
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(PropertyFromName("m_FullInventoryDrop"));
                EditorGUILayout.PropertyField(PropertyFromName("m_DropConsumableItems"));
                if (Foldout("Equip")) {
                    EditorGUI.indentLevel++;
                    InspectorUtility.DrawAnimationEventTrigger(target, "Equip Event", PropertyFromName("m_EquipEvent"));
                    InspectorUtility.DrawAnimationEventTrigger(target, "Equip Complete Event", PropertyFromName("m_EquipCompleteEvent"));
                    if (Foldout("Animator Audio")) {
                        EditorGUI.indentLevel++;
                        AnimatorAudioStateSetInspector.DrawAnimatorAudioStateSet(m_Item, m_Item.EquipAnimatorAudioStateSet, "m_EquipAnimatorAudioStateSet", true,
                                    ref m_ReorderableEquipAnimatorAudioStateSetList, OnEquipAnimatorAudioStateListDraw, OnEquipAnimatorAudioStateListSelect,
                                    OnEquipAnimatorAudioStateListAdd, OnEquipAnimatorAudioStateListRemove, SelectedEquipAnimatorAudioStateSetIndexKey,
                                    ref m_ReorderableEquipAnimatorAudioStateSetAudioList, 
                                    OnEquipAudioListElementDraw, OnEquipAudioListAdd, OnEquipAudioListRemove, ref m_ReorderableEquipAnimatorAudioStateSetStateList,
                                    OnEquipAnimatorAudioStateSetStateListDraw, OnEquipAnimatorAudioStateSetStateListAdd, OnEquipAnimatorAudioStateSetStateListReorder, OnEquipAnimatorAudioStateSetStateListRemove,
                                    GetSelectedEquipAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedEquipAnimatorAudioStateSetIndexKey)));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
                if (Foldout("Unequip")) {
                    EditorGUI.indentLevel++;
                    InspectorUtility.DrawAnimationEventTrigger(target, "Unequip Event", PropertyFromName("m_UnequipEvent"));
                    InspectorUtility.DrawAnimationEventTrigger(target, "Unequip Complete Event", PropertyFromName("m_UnequipCompleteEvent"));
                    if (Foldout("Animator Audio")) {
                        EditorGUI.indentLevel++;
                        AnimatorAudioStateSetInspector.DrawAnimatorAudioStateSet(m_Item, m_Item.UnequipAnimatorAudioStateSet, "m_UnequipAnimatorAudioStateSet", true,
                                    ref m_ReorderableUnequipAnimatorAudioStateSetList, OnUnequipAnimatorAudioStateListDraw, OnUnequipAnimatorAudioStateListSelect,
                                    OnUnequipAnimatorAudioStateListAdd, OnUnequipAnimatorAudioStateListRemove, SelectedUnequipAnimatorAudioStateSetIndexKey,
                                    ref m_ReorderableUnequipAnimatorAudioStateSetAudioList,
                                    OnUnequipAudioListElementDraw, OnUnequipAudioListAdd, OnUnequipAudioListRemove, ref m_ReorderableUnequipAnimatorAudioStateSetStateList,
                                    OnUnequipAnimatorAudioStateSetStateListDraw, OnUnequipAnimatorAudioStateSetStateListAdd, OnUnequipAnimatorAudioStateSetStateListReorder, OnUnequipAnimatorAudioStateSetStateListRemove,
                                    GetSelectedUnequipAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedUnequipAnimatorAudioStateSetIndexKey)));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
                if (Foldout("UI")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_UIMonitorID"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_Icon"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ShowCrosshairsOnAim"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_CenterCrosshairs"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_QuadrantOffset"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_MaxQuadrantSpread"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_QuadrantSpreadDamping"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_LeftCrosshairs"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_TopCrosshairs"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_RightCrosshairs"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_BottomCrosshairs"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ShowFullScreenUI"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_FullScreenUIID"));
                    EditorGUI.indentLevel--;
                }
                if (Foldout("Events")) {
                    EditorGUI.indentLevel++;
                    InspectorUtility.UnityEventPropertyField(PropertyFromName("m_PickupItemEvent"));
                    InspectorUtility.UnityEventPropertyField(PropertyFromName("m_EquipItemEvent"));
                    InspectorUtility.UnityEventPropertyField(PropertyFromName("m_UnequipItemEvent"));
                    InspectorUtility.UnityEventPropertyField(PropertyFromName("m_DropItemEvent"));
                    EditorGUI.indentLevel--;
                }
            };

            return baseCallback;
        }

        /// <summary>
        /// Draws all of the added audio clip elements.
        /// </summary>
        private void OnEquipAudioListElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AnimatorAudioStateSetInspector.OnAudioClipDraw(m_ReorderableEquipAnimatorAudioStateSetAudioList, rect, index, m_Item.EquipAnimatorAudioStateSet.States, SelectedEquipAnimatorAudioStateSetIndexKey, target);
        }

        /// <summary>
        /// Adds a new audio clip element to the list.
        /// </summary>
        private void OnEquipAudioListAdd(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAudioClipListAdd(list, m_Item.EquipAnimatorAudioStateSet.States, SelectedEquipAnimatorAudioStateSetIndexKey, target);
        }

        /// <summary>
        /// Removes the audio clip at the list index.
        /// </summary>
        private void OnEquipAudioListRemove(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAudioClipListRemove(list, m_Item.EquipAnimatorAudioStateSet.States, SelectedEquipAnimatorAudioStateSetIndexKey, target);
        }

        /// <summary>
        /// Draws all of the added audio clip elements.
        /// </summary>
        private void OnUnequipAudioListElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AnimatorAudioStateSetInspector.OnAudioClipDraw(m_ReorderableUnequipAnimatorAudioStateSetAudioList, rect, index, m_Item.UnequipAnimatorAudioStateSet.States, SelectedUnequipAnimatorAudioStateSetIndexKey, target);
        }

        /// <summary>
        /// Adds a new audio clip element to the list.
        /// </summary>
        private void OnUnequipAudioListAdd(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAudioClipListAdd(list, m_Item.UnequipAnimatorAudioStateSet.States, SelectedUnequipAnimatorAudioStateSetIndexKey, target);
        }

        /// <summary>
        /// Removes the audio clip at the list index.
        /// </summary>
        private void OnUnequipAudioListRemove(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAudioClipListRemove(list, m_Item.UnequipAnimatorAudioStateSet.States, SelectedUnequipAnimatorAudioStateSetIndexKey, target);
        }

        /// <summary>
        /// Draws the AudioStateSet element.
        /// </summary>
        private void OnEquipAnimatorAudioStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AnimatorAudioStateSetInspector.OnAnimatorAudioStateElementDraw(m_ReorderableEquipAnimatorAudioStateSetList, m_Item.EquipAnimatorAudioStateSet, rect, index, isActive, isFocused, target);
        }

        /// <summary>
        /// Draws the AudioStateSet element.
        /// </summary>
        private void OnUnequipAnimatorAudioStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AnimatorAudioStateSetInspector.OnAnimatorAudioStateElementDraw(m_ReorderableUnequipAnimatorAudioStateSetList, m_Item.UnequipAnimatorAudioStateSet, rect, index, isActive, isFocused, target);
        }

        /// <summary>
        /// A new element has been selected.
        /// </summary>
        private void OnEquipAnimatorAudioStateListSelect(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAnimatorAudioStateSelect(list, SelectedEquipAnimatorAudioStateSetIndexKey);
        }

        /// <summary>
        /// A new element has been selected.
        /// </summary>
        private void OnUnequipAnimatorAudioStateListSelect(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAnimatorAudioStateSelect(list, SelectedUnequipAnimatorAudioStateSetIndexKey);
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnEquipAnimatorAudioStateListAdd(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAnimatorAudioStateListAdd(list, m_Item.EquipAnimatorAudioStateSet, SelectedEquipAnimatorAudioStateSetIndexKey);
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnUnequipAnimatorAudioStateListAdd(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAnimatorAudioStateListAdd(list, m_Item.UnequipAnimatorAudioStateSet, SelectedUnequipAnimatorAudioStateSetIndexKey);
        }

        /// <summary>
        /// Removes the state at the list index.
        /// </summary>
        private void OnEquipAnimatorAudioStateListRemove(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAnimatorAudioStateListRemove(list, m_Item.EquipAnimatorAudioStateSet, SelectedEquipAnimatorAudioStateSetIndexKey);
        }

        /// <summary>
        /// Removes the state at the list index.
        /// </summary>
        private void OnUnequipAnimatorAudioStateListRemove(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAnimatorAudioStateListRemove(list, m_Item.UnequipAnimatorAudioStateSet, SelectedUnequipAnimatorAudioStateSetIndexKey);
        }

        /// <summary>
        /// Returns the state index key for the specified AnimatorAudioStateSet type.
        /// </summary>
        private string GetSelectedEquipAnimatorAudioStateSetStateIndexKey(int index)
        {
            return c_EditorPrefsSelectedEquipAnimatorAudioStateSetStateIndexKey + "." + target.GetType() + "." + target.name + "." + index;
        }

        /// <summary>
        /// Returns the state index key for the specified AnimatorAudioStateSet type.
        /// </summary>
        private string GetSelectedUnequipAnimatorAudioStateSetStateIndexKey(int index)
        {
            return c_EditorPrefsSelectedUnequipAnimatorAudioStateSetStateIndexKey + "." + target.GetType() + "." + target.name + "." + index;
        }

        /// <summary>
        /// Draws all of the added states.
        /// </summary>
        private void OnEquipAnimatorAudioStateSetStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            var animatorAudioState = m_Item.EquipAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedEquipAnimatorAudioStateSetIndexKey)];

            // The index may be out of range if the component was copied.
            if (index >= animatorAudioState.States.Length) {
                m_ReorderableEquipAnimatorAudioStateSetStateList.index = -1;
                EditorPrefs.SetInt(GetSelectedEquipAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedEquipAnimatorAudioStateSetIndexKey)), m_ReorderableEquipAnimatorAudioStateSetStateList.index);
                return;
            }

            StateInspector.OnStateListDraw(animatorAudioState, animatorAudioState.States, rect, index);
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                StateInspector.UpdateDefaultStateValues(animatorAudioState.States);
            }
        }

        /// <summary>
        /// Draws all of the added states.
        /// </summary>
        private void OnUnequipAnimatorAudioStateSetStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            var animatorAudioState = m_Item.UnequipAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedUnequipAnimatorAudioStateSetIndexKey)];

            // The index may be out of range if the component was copied.
            if (index >= animatorAudioState.States.Length) {
                m_ReorderableUnequipAnimatorAudioStateSetStateList.index = -1;
                EditorPrefs.SetInt(GetSelectedUnequipAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedUnequipAnimatorAudioStateSetIndexKey)), m_ReorderableUnequipAnimatorAudioStateSetStateList.index);
                return;
            }

            StateInspector.OnStateListDraw(animatorAudioState, animatorAudioState.States, rect, index);
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                StateInspector.UpdateDefaultStateValues(animatorAudioState.States);
            }
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnEquipAnimatorAudioStateSetStateListAdd(ReorderableList list)
        {
            StateInspector.OnStateListAdd(AddExistingEquipAnimatorAudioStateSetStatePreset, CreateEquipAnimatorAudioStateSetStatePreset);
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnUnequipAnimatorAudioStateSetStateListAdd(ReorderableList list)
        {
            StateInspector.OnStateListAdd(AddExistingUnequipAnimatorAudioStateSetStatePreset, CreateUnequipAnimatorAudioStateSetStatePreset);
        }

        /// <summary>
        /// Adds a new element to the state list which uses an existing preset.
        /// </summary>
        private void AddExistingEquipAnimatorAudioStateSetStatePreset()
        {
            var animatorAudioState = m_Item.EquipAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedEquipAnimatorAudioStateSetIndexKey)];
            var states = StateInspector.AddExistingPreset(animatorAudioState.GetType(), animatorAudioState.States, m_ReorderableEquipAnimatorAudioStateSetStateList, GetSelectedEquipAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedEquipAnimatorAudioStateSetIndexKey)));
            if (animatorAudioState.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableEquipAnimatorAudioStateSetStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                animatorAudioState.States = states;
            }
        }

        /// <summary>
        /// Adds a new element to the state list which uses an existing preset.
        /// </summary>
        private void AddExistingUnequipAnimatorAudioStateSetStatePreset()
        {
            var animatorAudioState = m_Item.UnequipAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedUnequipAnimatorAudioStateSetIndexKey)];
            var states = StateInspector.AddExistingPreset(animatorAudioState.GetType(), animatorAudioState.States, m_ReorderableUnequipAnimatorAudioStateSetStateList, GetSelectedUnequipAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedUnequipAnimatorAudioStateSetIndexKey)));
            if (animatorAudioState.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableUnequipAnimatorAudioStateSetStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                animatorAudioState.States = states;
            }
        }

        /// <summary>
        /// Creates a new preset and adds it to a new state in the list.
        /// </summary>
        private void CreateEquipAnimatorAudioStateSetStatePreset()
        {
            var animatorAudioState = m_Item.EquipAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedEquipAnimatorAudioStateSetIndexKey)];
            var states = StateInspector.CreatePreset(animatorAudioState, animatorAudioState.States, m_ReorderableEquipAnimatorAudioStateSetStateList, GetSelectedEquipAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedEquipAnimatorAudioStateSetIndexKey)));
            if (animatorAudioState.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableEquipAnimatorAudioStateSetStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                animatorAudioState.States = states;
            }
        }

        /// <summary>
        /// Creates a new preset and adds it to a new state in the list.
        /// </summary>
        private void CreateUnequipAnimatorAudioStateSetStatePreset()
        {
            var animatorAudioState = m_Item.UnequipAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedUnequipAnimatorAudioStateSetIndexKey)];
            var states = StateInspector.CreatePreset(animatorAudioState, animatorAudioState.States, m_ReorderableUnequipAnimatorAudioStateSetStateList, GetSelectedUnequipAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedUnequipAnimatorAudioStateSetIndexKey)));
            if (animatorAudioState.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableUnequipAnimatorAudioStateSetStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                animatorAudioState.States = states;
            }
        }

        /// <summary>
        /// The list has been reordered. Ensure the reorder is valid.
        /// </summary>
        private void OnEquipAnimatorAudioStateSetStateListReorder(ReorderableList list)
        {
            var animatorAudioState = m_Item.EquipAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedEquipAnimatorAudioStateSetIndexKey)];

            // Use the dummy array in order to determine what element the selected index was swapped with.
            var copiedStates = new UltimateCharacterController.StateSystem.State[animatorAudioState.States.Length];
            Array.Copy(animatorAudioState.States, copiedStates, animatorAudioState.States.Length);
            for (int i = 0; i < animatorAudioState.States.Length; ++i) {
                var element = list.serializedProperty.GetArrayElementAtIndex(i);
                if (element.intValue != i) {
                    animatorAudioState.States[i] = copiedStates[element.intValue];
                    element.intValue = i;
                }
            }

            var states = StateInspector.OnStateListReorder(animatorAudioState.States);
            if (animatorAudioState.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableEquipAnimatorAudioStateSetStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                animatorAudioState.States = states;
            }
        }

        /// <summary>
        /// The list has been reordered. Ensure the reorder is valid.
        /// </summary>
        private void OnUnequipAnimatorAudioStateSetStateListReorder(ReorderableList list)
        {
            var animatorAudioState = m_Item.UnequipAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedUnequipAnimatorAudioStateSetIndexKey)];

            // Use the dummy array in order to determine what element the selected index was swapped with.
            var copiedStates = new UltimateCharacterController.StateSystem.State[animatorAudioState.States.Length];
            Array.Copy(animatorAudioState.States, copiedStates, animatorAudioState.States.Length);
            for (int i = 0; i < animatorAudioState.States.Length; ++i) {
                var element = list.serializedProperty.GetArrayElementAtIndex(i);
                if (element.intValue != i) {
                    animatorAudioState.States[i] = copiedStates[element.intValue];
                    element.intValue = i;
                }
            }

            var states = StateInspector.OnStateListReorder(animatorAudioState.States);
            if (animatorAudioState.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableUnequipAnimatorAudioStateSetStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                animatorAudioState.States = states;
            }
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        private void OnEquipAnimatorAudioStateSetStateListRemove(ReorderableList list)
        {
            var animatorAudioState = m_Item.EquipAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedEquipAnimatorAudioStateSetIndexKey)];
            var states = StateInspector.OnStateListRemove(animatorAudioState.States, GetSelectedEquipAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedEquipAnimatorAudioStateSetIndexKey)), list);
            if (animatorAudioState.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableEquipAnimatorAudioStateSetStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                animatorAudioState.States = states;
            }
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        private void OnUnequipAnimatorAudioStateSetStateListRemove(ReorderableList list)
        {
            var animatorAudioState = m_Item.UnequipAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedUnequipAnimatorAudioStateSetIndexKey)];
            var states = StateInspector.OnStateListRemove(animatorAudioState.States, GetSelectedUnequipAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedUnequipAnimatorAudioStateSetIndexKey)), list);
            if (animatorAudioState.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableUnequipAnimatorAudioStateSetStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                animatorAudioState.States = states;
            }
        }

        /// <summary>
        /// Deserialize the animator audio state states after an undo/redo.
        /// </summary>
        protected virtual void OnUndoRedo()
        {
            m_Item.EquipAnimatorAudioStateSet.DeserializeAnimatorAudioStateSelector(m_Item, m_Item.GetComponentInParent<UltimateCharacterController.Character.UltimateCharacterLocomotion>());
            m_Item.UnequipAnimatorAudioStateSet.DeserializeAnimatorAudioStateSelector(m_Item, m_Item.GetComponentInParent<UltimateCharacterController.Character.UltimateCharacterLocomotion>());
            Repaint();
        }
    }
}