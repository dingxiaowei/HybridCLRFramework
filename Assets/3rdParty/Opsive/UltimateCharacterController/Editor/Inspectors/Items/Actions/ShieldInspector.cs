/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.Editor.Inspectors.Items.AnimatorAudioState;
using Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using System;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Items.Actions
{
    /// <summary>
    /// Shows a custom inspector for the Shield component.
    /// </summary>
    [CustomEditor(typeof(Shield))]
    public class ShieldInspector : ItemActionInspector
    {
        private const string c_EditorPrefsSelectedImpactAnimatorAudioStateSetIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedImpactAnimatorAudioStateSetIndex";
        private const string c_EditorPrefsSelectedImpactAnimatorAudioStateSetStateIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedImpactAnimatorAudioStateSetStateIndex";
        private string SelectedImpactAnimatorAudioStateSetIndexKey { get { return c_EditorPrefsSelectedImpactAnimatorAudioStateSetStateIndexKey + "." + target.GetType() + "." + target.name; } }

        private Item m_Item;
        private Shield m_Shield;
        private AttributeManager m_AttributeManager;
        private ReorderableList m_ReorderableImpactAnimatorAudioStateSetList;
        private ReorderableList m_ReorderableImpactAnimatorAudioStateSetAudioList;
        private ReorderableList m_ReorderableImpactAnimatorAudioStateSetStateList;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            m_Shield = target as Shield;
            m_Item = m_Shield.GetComponent<Item>();
            m_AttributeManager = m_Shield.GetComponent<AttributeManager>();
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
                EditorGUILayout.PropertyField(PropertyFromName("m_RequireAim"));
                EditorGUILayout.PropertyField(PropertyFromName("m_AbsorptionFactor"));
                EditorGUILayout.PropertyField(PropertyFromName("m_AbsorbExplosions"));
                // The names will be retrieved by the Attribute Manager.
                var attributeName = InspectorUtility.DrawAttribute(target, m_AttributeManager, (target as Shield).DurabilityAttributeName, "Durability Attribute");
                if (attributeName != (target as Shield).DurabilityAttributeName) {
                    PropertyFromName("m_DurabilityAttributeName").stringValue = attributeName;
                    serializedObject.ApplyModifiedProperties();
                }
                if (!string.IsNullOrEmpty(attributeName)) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_DropWhenDurabilityDepleted"));
                    EditorGUI.indentLevel--;
                }

                if (Foldout("Impact")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_ApplyImpact"));
                    InspectorUtility.DrawAnimationEventTrigger(target, "Impact Complete Event", PropertyFromName("m_ImpactCompleteEvent"));
                    if (Foldout("Animator Audio")) {
                        EditorGUI.indentLevel++;
                        AnimatorAudioStateSetInspector.DrawAnimatorAudioStateSet(m_Shield, m_Shield.ImpactAnimatorAudioStateSet, "m_ImpactAnimatorAudioStateSet", true,
                                    ref m_ReorderableImpactAnimatorAudioStateSetList, OnImpactAnimatorAudioStateListDraw, OnImpactAnimatorAudioStateListSelect,
                                    OnImpactAnimatorAudioStateListAdd, OnImpactAnimatorAudioStateListRemove, SelectedImpactAnimatorAudioStateSetIndexKey,
                                    ref m_ReorderableImpactAnimatorAudioStateSetAudioList, OnImpactAudioListElementDraw, OnImpactAudioListAdd, OnImpactAudioListRemove,
                                    ref m_ReorderableImpactAnimatorAudioStateSetStateList, OnImpactAnimatorAudioStateSetStateListDraw, OnImpactAnimatorAudioStateSetStateListAdd,
                                    OnImpactAnimatorAudioStateSetStateListReorder, OnImpactAnimatorAudioStateListRemove,
                                    GetSelectedImpactAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedImpactAnimatorAudioStateSetIndexKey)));
                        EditorGUI.indentLevel--;
                    }
                }

                GUI.enabled = true;
            };

            return baseCallback;
        }

        /// <summary>
        /// Draws all of the added audio clip elements.
        /// </summary>
        private void OnImpactAudioListElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AnimatorAudioStateSetInspector.OnAudioClipDraw(m_ReorderableImpactAnimatorAudioStateSetAudioList, rect, index, m_Shield.ImpactAnimatorAudioStateSet.States, SelectedImpactAnimatorAudioStateSetIndexKey, target);
        }

        /// <summary>
        /// Adds a new audio clip element to the list.
        /// </summary>
        private void OnImpactAudioListAdd(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAudioClipListAdd(list, m_Shield.ImpactAnimatorAudioStateSet.States, SelectedImpactAnimatorAudioStateSetIndexKey, target);
        }

        /// <summary>
        /// Removes the audio clip at the list index.
        /// </summary>
        private void OnImpactAudioListRemove(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAudioClipListRemove(list, m_Shield.ImpactAnimatorAudioStateSet.States, SelectedImpactAnimatorAudioStateSetIndexKey, target);
        }

        /// <summary>
        /// Draws the AudioStateSet element.
        /// </summary>
        private void OnImpactAnimatorAudioStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AnimatorAudioStateSetInspector.OnAnimatorAudioStateElementDraw(m_ReorderableImpactAnimatorAudioStateSetList, m_Shield.ImpactAnimatorAudioStateSet, rect, index, isActive, isFocused, target);
        }

        /// <summary>
        /// A new element has been selected.
        /// </summary>
        private void OnImpactAnimatorAudioStateListSelect(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAnimatorAudioStateSelect(list, SelectedImpactAnimatorAudioStateSetIndexKey);
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnImpactAnimatorAudioStateListAdd(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAnimatorAudioStateListAdd(list, m_Shield.ImpactAnimatorAudioStateSet, SelectedImpactAnimatorAudioStateSetIndexKey);
        }

        /// <summary>
        /// Removes the state at the list index.
        /// </summary>
        private void OnImpactAnimatorAudioStateListRemove(ReorderableList list)
        {
            AnimatorAudioStateSetInspector.OnAnimatorAudioStateListRemove(list, m_Shield.ImpactAnimatorAudioStateSet, SelectedImpactAnimatorAudioStateSetIndexKey);
        }

        /// <summary>
        /// Returns the state index key for the specified AnimatorAudioStateSet type.
        /// </summary>
        private string GetSelectedImpactAnimatorAudioStateSetStateIndexKey(int index)
        {
            return c_EditorPrefsSelectedImpactAnimatorAudioStateSetStateIndexKey + "." + target.GetType() + "." + target.name + "." + index;
        }

        /// <summary>
        /// Draws all of the added states.
        /// </summary>
        private void OnImpactAnimatorAudioStateSetStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            var animatorAudioState = m_Shield.ImpactAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedImpactAnimatorAudioStateSetIndexKey)];

            // The index may be out of range if the component was copied.
            if (index >= animatorAudioState.States.Length) {
                m_ReorderableImpactAnimatorAudioStateSetStateList.index = -1;
                EditorPrefs.SetInt(GetSelectedImpactAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedImpactAnimatorAudioStateSetIndexKey)), m_ReorderableImpactAnimatorAudioStateSetStateList.index);
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
        private void OnImpactAnimatorAudioStateSetStateListAdd(ReorderableList list)
        {
            StateInspector.OnStateListAdd(AddExistingImpactAnimatorAudioStateSetStatePreset, CreateImpactAnimatorAudioStateSetStatePreset);
        }

        /// <summary>
        /// Adds a new element to the state list which uses an existing preset.
        /// </summary>
        private void AddExistingImpactAnimatorAudioStateSetStatePreset()
        {
            var animatorAudioState = m_Shield.ImpactAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedImpactAnimatorAudioStateSetIndexKey)];
            var states = StateInspector.AddExistingPreset(animatorAudioState.GetType(), animatorAudioState.States, m_ReorderableImpactAnimatorAudioStateSetStateList, GetSelectedImpactAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedImpactAnimatorAudioStateSetIndexKey)));
            if (animatorAudioState.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableImpactAnimatorAudioStateSetStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                animatorAudioState.States = states;
            }
        }

        /// <summary>
        /// Creates a new preset and adds it to a new state in the list.
        /// </summary>
        private void CreateImpactAnimatorAudioStateSetStatePreset()
        {
            var animatorAudioState = m_Shield.ImpactAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedImpactAnimatorAudioStateSetIndexKey)];
            var states = StateInspector.CreatePreset(animatorAudioState, animatorAudioState.States, m_ReorderableImpactAnimatorAudioStateSetStateList, GetSelectedImpactAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedImpactAnimatorAudioStateSetIndexKey)));
            if (animatorAudioState.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableImpactAnimatorAudioStateSetStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                animatorAudioState.States = states;
            }
        }

        /// <summary>
        /// The list has been reordered. Ensure the reorder is valid.
        /// </summary>
        private void OnImpactAnimatorAudioStateSetStateListReorder(ReorderableList list)
        {
            var animatorAudioState = m_Shield.ImpactAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedImpactAnimatorAudioStateSetIndexKey)];

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
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableImpactAnimatorAudioStateSetStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                animatorAudioState.States = states;
            }
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        private void OnImpactAnimatorAudioStateSetStateListRemove(ReorderableList list)
        {
            var animatorAudioState = m_Shield.ImpactAnimatorAudioStateSet.States[EditorPrefs.GetInt(SelectedImpactAnimatorAudioStateSetIndexKey)];
            var states = StateInspector.OnStateListRemove(animatorAudioState.States, GetSelectedImpactAnimatorAudioStateSetStateIndexKey(EditorPrefs.GetInt(SelectedImpactAnimatorAudioStateSetIndexKey)), list);
            if (animatorAudioState.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableImpactAnimatorAudioStateSetStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                animatorAudioState.States = states;
            }
        }

        /// <summary>
        /// Deserialize the animator audio state set after an undo/redo.
        /// </summary>
        private void OnUndoRedo()
        {
            m_Shield.ImpactAnimatorAudioStateSet.DeserializeAnimatorAudioStateSelector(m_Item, m_Item.GetComponentInParent<UltimateCharacterController.Character.UltimateCharacterLocomotion>());
            Repaint();
        }
    }
}