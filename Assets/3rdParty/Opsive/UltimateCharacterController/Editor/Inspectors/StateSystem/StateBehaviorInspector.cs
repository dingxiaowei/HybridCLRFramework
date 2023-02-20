/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Reflection;
using System.Collections.Generic;
using Opsive.UltimateCharacterController.StateSystem;
using Opsive.UltimateCharacterController.Utility;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem
{
    /// <summary>
    /// Contains the base inspector logic for every component which is derived from BaseComponent.
    /// </summary>
    [CustomEditor(typeof(StateBehavior), true)]
    public class StateBehaviorInspector : InspectorBase
    {
        private const string c_EditorPrefsSelectedIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedStateIndex";
        private string SelectedIndexKey { get { return c_EditorPrefsSelectedIndexKey + "." + target.GetType() + "." + target.name; } }

        private ReorderableList m_ReorderableStateList;
        private List<List<FieldInfo>> m_FoldoutFields = new List<List<FieldInfo>>();
        private List<string> m_FoldoutNames = new List<string>();

        /// <summary>
        /// Initializes the inspector.
        /// </summary>
        protected virtual void OnEnable()
        {
            // If the inspector type isn't the current type then a custom inspector has been created for the target.
            if (GetType() != typeof(StateBehaviorInspector)) {
                return;
            }

            // Group the fields into foldouts.
            m_FoldoutFields.Clear();
            m_FoldoutNames.Clear();
            // Start the fields out with no foldout.
            m_FoldoutFields.Add(new List<FieldInfo>());
            m_FoldoutNames.Add(string.Empty);

            // Populate the foldouts.
            var allFields = new List<FieldInfo>(Serialization.GetSerializedFields(target.GetType(), MemberVisibility.Public));
            var foldoutMap = new Dictionary<string, int>();
            var foldoutIndex = 0;
            for (int i = 0; i < allFields.Count; ++i) {
                // Do not show HideInInspector fields.
                if (UnityEngineUtility.HasAttribute(allFields[i], typeof(HideInInspector))) {
                    continue;
                }
                
                // Create a new foldout if there is an InspectorFoldout Attribute.
                var foldoutAttribute = allFields[i].GetCustomAttributes(typeof(InspectorFoldout), false) as InspectorFoldout[];
                if (foldoutAttribute.Length > 0) {
                    if (!string.IsNullOrEmpty(foldoutAttribute[0].Title)) {
                        if (!foldoutMap.TryGetValue(foldoutAttribute[0].Title, out foldoutIndex)) {
                            foldoutIndex = foldoutMap.Count + 1;
                            foldoutMap.Add(foldoutAttribute[0].Title, foldoutIndex);
                            m_FoldoutFields.Add(new List<FieldInfo>());
                            m_FoldoutNames.Add(foldoutAttribute[0].Title);
                        }
                    } else {
                        // Reset the index if the foldout title is blank.
                        foldoutIndex = 0;
                    }
                }

                // Add the field to the last foldout.
                m_FoldoutFields[foldoutIndex].Add(allFields[i]);
            }
        }

        /// <summary>
        /// Draws the component inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();

            var callback = GetDrawCallback();
            if (callback != null) {
                // Wait to draw the inherited component until the Script field has been drawn. This will allow the state foldout to be drawn after the inherited component.
                callback();
            } else {
                // If a callback doesn't exist use the InspectorFoldout attribute to draw all of the fields without having to create custom inspectors for each class.
                for (int i = 0; i < m_FoldoutFields.Count; ++i) {
                    if (m_FoldoutFields[i].Count > 0) {
                        if (!string.IsNullOrEmpty(m_FoldoutNames[i])) {
                            // Draw the fields indented under a foldout.
                            if (Foldout(m_FoldoutNames[i])) {
                                EditorGUI.indentLevel++;
                                for (int j = 0; j < m_FoldoutFields[i].Count; ++j) {
                                    EditorGUILayout.PropertyField(PropertyFromName(m_FoldoutFields[i][j].Name), true);
                                }
                                EditorGUI.indentLevel--;
                            }
                        } else {
                            // There is no foldout so don't indent the EditorGUI.
                            for (int j = 0; j < m_FoldoutFields[i].Count; ++j) {
                                EditorGUILayout.PropertyField(PropertyFromName(m_FoldoutFields[i][j].Name), true);
                            }
                        }
                    }
                }
            }

            if (Foldout("States")) {
                m_ReorderableStateList = StateInspector.DrawStates(m_ReorderableStateList, serializedObject, PropertyFromName("m_States"), 
                                            SelectedIndexKey, OnStateListDraw, OnStateListAdd, OnStateListReorder, OnStateListRemove);
                // EditorGUILayout.PropertyField(PropertyFromName("m_States"), true);
            }

            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                serializedObject.ApplyModifiedProperties();
                StateInspector.UpdateDefaultStateValues((target as StateBehavior).States);
            }
        }

        /// <summary>
        /// Returns the actions to draw before the State list is drawn.
        /// </summary>
        /// <returns>The actions to draw before the State list is drawn.</returns>
        protected virtual Action GetDrawCallback()
        {
            return null;
        }

        /// <summary>
        /// Draws all of the added states.
        /// </summary>
        private void OnStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();

            StateInspector.OnStateListDraw(target, (target as StateBehavior).States, PropertyFromName("m_States"), rect, index);

            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnStateListAdd(ReorderableList list)
        {
            StateInspector.OnStateListAdd(AddExistingPreset, CreatePreset);
        }

        /// <summary>
        /// Adds a new element to the state list which uses an existing preset.
        /// </summary>
        private void AddExistingPreset()
        {
            EditorGUI.BeginChangeCheck();

            var stateComponent = target as StateBehavior;
            stateComponent.States = StateInspector.AddExistingPreset(stateComponent.GetType(), stateComponent.States, m_ReorderableStateList, SelectedIndexKey);

            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Creates a new preset and adds it to a new state in the list.
        /// </summary>
        private void CreatePreset()
        {
            EditorGUI.BeginChangeCheck();

            var stateComponent = target as StateBehavior;
            stateComponent.States = StateInspector.CreatePreset(target, stateComponent.States, m_ReorderableStateList, SelectedIndexKey);
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// The list has been reordered. Ensure the reorder is valid.
        /// </summary>
        private void OnStateListReorder(ReorderableList list)
        {
            EditorGUI.BeginChangeCheck();
            
            var stateComponent = target as StateBehavior;
            stateComponent.States = StateInspector.OnStateListReorder(stateComponent.States);

            if (EditorGUI.EndChangeCheck()) {
                EditorPrefs.SetInt(SelectedIndexKey, list.index);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        private void OnStateListRemove(ReorderableList list)
        {
            EditorGUI.BeginChangeCheck();

            var stateComponent = target as StateBehavior;
            stateComponent.States = StateInspector.OnStateListRemove(stateComponent.States, PropertyFromName("m_States"), SelectedIndexKey, list);

            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}