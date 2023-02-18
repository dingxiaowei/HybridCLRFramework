/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Camera
{
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Camera.ViewTypes;
    using Opsive.UltimateCharacterController.Utility;
    using Opsive.UltimateCharacterController.Utility.Builders;
    using Opsive.UltimateCharacterController.StateSystem;
    using Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditorInternal;

    /// <summary>
    /// Shows a custom inspector for the CameraController.
    /// </summary>
    [CustomEditor(typeof(CameraController))]
    public class CameraControllerInspector : StateBehaviorInspector
    {
        private const string c_EditorPrefsSelectedViewTypeIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.Camera.SelectedViewTypeIndex";
        private const string c_EditorPrefsSelectedViewTypeStateIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.Camera.SelectedViewTypeStateIndex";
        private string SelectedViewTypeIndexKey { get { return c_EditorPrefsSelectedViewTypeIndexKey + "." + target.GetType() + "." + target.name; } }

        private string[] m_FirstPersonViewTypeNames;
        private string[] m_ThirdPersonViewTypeNames;
        private Type[] m_FirstPersonViewTypes;
        private Type[] m_ThirdPersonViewTypes;

        private CameraController m_CameraController;
        private ReorderableList m_ReorderableViewTypeList;
        private ReorderableList m_ReorderableViewTypeStateList;

        /// <summary>
        /// Search for the available view types types.
        /// </summary>
        protected override void OnEnable()
        {
            m_CameraController = target as CameraController;

            // After an undo or redo has been performed the view types need to be deserialized.
            Undo.undoRedoPerformed += OnUndoRedo;

            try {
                // The view types may have changed since the last serialization (such as if a class no longer exists) so serialize the objects
                // again if there is a change.
                if (m_CameraController.ViewTypes == null && m_CameraController.DeserializeViewTypes()) {
                    // Do not serialize the view type during runtime.
                    if (!Application.isPlaying) {
                        SerializeViewTypes();
                    }
                }
            } catch (Exception) { }

            UpdateDefaultViewTypes();
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
                if (Foldout("Character")) {
                    EditorGUI.indentLevel++;
                    var initOnAwake = PropertyFromName("m_InitCharacterOnAwake");
                    EditorGUILayout.PropertyField(initOnAwake);
                    if (initOnAwake.boolValue || Application.isPlaying) {
                        var characterProperty = PropertyFromName("m_Character");
                        EditorGUILayout.PropertyField(characterProperty);
                        if (!Application.isPlaying && characterProperty.objectReferenceValue != null) {
                            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(characterProperty.objectReferenceValue))) {
                                EditorGUILayout.HelpBox("The Camera Controller Character property cannot point to a prefab.", MessageType.Error);
                            }
                        }
                    }

                    var autoAnchorProperty = PropertyFromName("m_AutoAnchor");
                    EditorGUILayout.PropertyField(autoAnchorProperty);
                    if (autoAnchorProperty.boolValue) {
                        EditorGUILayout.PropertyField(PropertyFromName("m_AutoAnchorBone"));
                    } else {
                        var anchorProperty = PropertyFromName("m_Anchor");
                        anchorProperty.objectReferenceValue = EditorGUILayout.ObjectField("Anchor", anchorProperty.objectReferenceValue, typeof(Transform), true, GUILayout.MinWidth(80)) as Transform;
                        if (anchorProperty.objectReferenceValue == null) {
                            EditorGUILayout.HelpBox("The anchor specifies the Transform that the camera should follow. If null it will use the Character's Transform.", MessageType.Info);
                        }
                    }
                    EditorGUILayout.PropertyField(PropertyFromName("m_AnchorOffset"));

                    EditorGUI.indentLevel--;
                }

                if (Foldout("View Types")) {
                    EditorGUILayout.BeginVertical("Box");
                    EditorGUI.indentLevel++;
                    // Only show the first/third person view type popup if that view type is available.
                    if (!string.IsNullOrEmpty(m_CameraController.FirstPersonViewTypeFullName) && !string.IsNullOrEmpty(m_CameraController.ThirdPersonViewTypeFullName)) {
                        var selectedIndex = 0;
                        for (int i = 0; i < m_FirstPersonViewTypes.Length; ++i) {
                            if (m_FirstPersonViewTypes[i].FullName == m_CameraController.FirstPersonViewTypeFullName) {
                                selectedIndex = i;
                                break;
                            }
                        }
                        var index = EditorGUILayout.Popup("First Person View Type", selectedIndex, m_FirstPersonViewTypeNames);
                        if (index != selectedIndex) {
                            m_CameraController.FirstPersonViewTypeFullName = PropertyFromName("m_FirstPersonViewTypeFullName").stringValue = m_FirstPersonViewTypes[index].FullName;
                            // Update the default view type if the current view type is first person. Do not update when playing because the first person property will update the current type.
                            if (Application.isPlaying && m_CameraController.ActiveViewType.FirstPersonPerspective) {
                                m_CameraController.ViewTypeFullName = m_CameraController.FirstPersonViewTypeFullName;
                            }
                            serializedObject.ApplyModifiedProperties();
                        }
                        for (int i = 0; i < m_ThirdPersonViewTypes.Length; ++i) {
                            if (m_ThirdPersonViewTypes[i].FullName == m_CameraController.ThirdPersonViewTypeFullName) {
                                selectedIndex = i;
                                break;
                            }
                        }
                        index = EditorGUILayout.Popup("Third Person View Type", selectedIndex, m_ThirdPersonViewTypeNames);
                        if (index != selectedIndex) {
                            m_CameraController.ThirdPersonViewTypeFullName = PropertyFromName("m_ThirdPersonViewTypeFullName").stringValue = m_ThirdPersonViewTypes[index].FullName;
                            // Update the default view type if the current view type is third person. Do not update when playing because the third person property will update the current type.
                            if (!Application.isPlaying && !m_CameraController.ActiveViewType.FirstPersonPerspective) {
                                m_CameraController.ViewTypeFullName = m_CameraController.ThirdPersonViewTypeFullName;
                            }
                            serializedObject.ApplyModifiedProperties();
                        }
                        EditorGUILayout.PropertyField(PropertyFromName("m_CanChangePerspectives"));
                    }
                    ReorderableListSerializationHelper.DrawReorderableList(ref m_ReorderableViewTypeList, this, m_CameraController.ViewTypes, "m_ViewTypeData", 
                                                                    OnViewTypeListDrawHeader, OnViewTypeListDraw, OnViewTypeListReorder, OnViewTypeListAdd, 
                                                                    OnViewTypeListRemove, OnViewTypeListSelect, DrawSelectedViewType, SelectedViewTypeIndexKey, true, true);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                }
                if (Foldout("Zoom")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_CanZoom"));
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(PropertyFromName("m_ZoomState"));
                    GUILayout.Space(-5);
                    GUI.enabled = !string.IsNullOrEmpty(PropertyFromName("m_ZoomState").stringValue);
                    var appendItemIdentifierNameProperty = PropertyFromName("m_StateAppendItemIdentifierName");
                    appendItemIdentifierNameProperty.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Append Item", "Should the ItemIdentifier name be appened to the state name?"),
                                                            appendItemIdentifierNameProperty.boolValue, GUILayout.Width(110));
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel--;
                }
                if (Foldout("Events")) {
                    EditorGUI.indentLevel++;
                    InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnChangeViewTypesEvent"));
                    InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnChangePerspectivesEvent"));
                    InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnZoomEvent"));
                    EditorGUI.indentLevel--;
                }
            };

            return baseCallback;
        }

        /// <summary>
        /// Draws the header for the view type list.
        /// </summary>
        private void OnViewTypeListDrawHeader(Rect rect)
        {
            var activeRect = rect;
            activeRect.width -= 33;
            EditorGUI.LabelField(activeRect, "View Type");

            activeRect.x += activeRect.width - 12;
            activeRect.width = 49;
            EditorGUI.LabelField(activeRect, "Active");
        }

        /// <summary>
        /// Draws all of the added view types.
        /// </summary>
        private void OnViewTypeListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            // The index may be out of range if the component was copied.
            if (index >= m_CameraController.ViewTypes.Length) {
                m_ReorderableViewTypeList.index = -1;
                EditorPrefs.SetInt(SelectedViewTypeIndexKey, m_ReorderableViewTypeList.index);
                return;
            }

            var viewType = m_CameraController.ViewTypes[index];
            if (viewType == null) {
                var viewTypes = new List<ViewType>(m_CameraController.ViewTypes);
                viewTypes.RemoveAt(index);
                m_CameraController.ViewTypes = viewTypes.ToArray();
                SerializeViewTypes();
                return;
            }
            var label = InspectorUtility.DisplayTypeName(viewType.GetType(), true);

            // Reduce the rect width so the active toggle can be added.
            var activeRect = rect;
            activeRect.width -= 20;
            EditorGUI.LabelField(activeRect, label);

            // Draw the active toggle and serialize if there is a change.
            if (!(m_CameraController.ViewTypes[index] is UltimateCharacterController.Camera.ViewTypes.Transition)) {
                EditorGUI.BeginChangeCheck();
                activeRect = rect;
                activeRect.x += activeRect.width - 32;
                activeRect.width = 20;
                EditorGUI.Toggle(activeRect, m_CameraController.ViewTypeFullName == viewType.GetType().FullName, EditorStyles.radioButton);
                if (EditorGUI.EndChangeCheck()) {
                    m_CameraController.ViewTypeFullName = PropertyFromName("m_ViewTypeFullName").stringValue = viewType.GetType().FullName;
                    if (m_CameraController.ViewTypes[index].FirstPersonPerspective) {
                        m_CameraController.FirstPersonViewTypeFullName = PropertyFromName("m_FirstPersonViewTypeFullName").stringValue = m_CameraController.ViewTypeFullName;
                    } else {
                        m_CameraController.ThirdPersonViewTypeFullName = PropertyFromName("m_ThirdPersonViewTypeFullName").stringValue = m_CameraController.ViewTypeFullName;
                    }
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        /// <summary>
        /// The view type list has been reordered.
        /// </summary>
        private void OnViewTypeListReorder(ReorderableList list)
        {
            // Deserialize the view types so the ViewType array will be correct. The list operates on the ViewTypeData array.
            m_CameraController.DeserializeViewTypes(true);

            // Update the selected index.
            EditorPrefs.SetInt(SelectedViewTypeIndexKey, list.index);
        }

        /// <summary>
        /// Adds a new view type element to the list.
        /// </summary>
        private void OnViewTypeListAdd(ReorderableList list)
        {
            ReorderableListSerializationHelper.AddObjectType(typeof(ViewType), true, m_CameraController.ViewTypes, AddViewType);
        }

        /// <summary>
        /// Adds the view type with the specified type.
        /// </summary>
        private void AddViewType(object obj)
        {
            var viewType = ViewTypeBuilder.AddViewType(m_CameraController, obj as Type);
            m_ReorderableViewTypeList.displayRemove = m_CameraController.ViewTypes.Length > 1;

            // Select the newly added view type.
            m_ReorderableViewTypeList.index = m_CameraController.ViewTypes.Length - 1;
            EditorPrefs.SetInt(SelectedViewTypeIndexKey, m_ReorderableViewTypeList.index);

            // The view type's state list should start out fresh to prevent the old view type states from being shown.
            m_ReorderableViewTypeStateList = null;

            // Allow the view type to perform any initialization.
            var inspectorDrawer = InspectorDrawerUtility.InspectorDrawerForType(viewType.GetType()) as ViewTypeInspectorDrawer;
            if (inspectorDrawer != null) {
                inspectorDrawer.ViewTypeAdded(viewType, target);
            }
        }

        /// <summary>
        /// Remove the view type at the list index.
        /// </summary>
        private void OnViewTypeListRemove(ReorderableList list)
        {
            var viewTypes = new List<ViewType>(m_CameraController.ViewTypes);
            // Select a new view type if the currently selected view type is being removed.
            var removedSelected = viewTypes[list.index].GetType().FullName == m_CameraController.ViewTypeFullName;
            var viewTypeFullName = viewTypes[list.index].GetType().FullName;

            // Remove the element.
            InspectorUtility.RecordUndoDirtyObject(target, "Change Value");

            // Allow the ability to perform any destruction.
            var viewType = viewTypes[list.index];
            var inspectorDrawer = InspectorDrawerUtility.InspectorDrawerForType(viewType.GetType()) as ViewTypeInspectorDrawer;
            if (inspectorDrawer != null) {
                inspectorDrawer.ViewTypeRemoved(viewType, target);
            }

            viewTypes.RemoveAt(list.index);
            m_CameraController.ViewTypes = viewTypes.ToArray();
            // Update the default first/third view type.
            if (m_CameraController.FirstPersonViewTypeFullName == viewTypeFullName) {
                m_CameraController.FirstPersonViewTypeFullName = string.Empty;
                UpdateDefaultViewTypes();
            } else if (m_CameraController.ThirdPersonViewTypeFullName == viewTypeFullName) {
                m_CameraController.ThirdPersonViewTypeFullName = string.Empty;
                UpdateDefaultViewTypes();
            }
            SerializeViewTypes();

            // Don't show the remove button if there is only one view type left.
            list.displayRemove = m_CameraController.ViewTypes.Length > 1;

            // Update the index to point to no longer point to the now deleted view type.
            list.index = list.index - 1;
            if (list.index == -1 && viewTypes.Count > 0) {
                list.index = 0;
            }
            if (removedSelected) {
                m_CameraController.ViewTypeFullName = viewTypes[list.index].GetType().FullName;
            }
            EditorPrefs.SetInt(SelectedViewTypeIndexKey, list.index);

            // The view type's state list should start out fresh to prevent the old view type states from being shown.
            m_ReorderableViewTypeStateList = null;
        }

        /// <summary>
        /// A new element has been selected within the list.
        /// </summary>
        private void OnViewTypeListSelect(ReorderableList list)
        {
            EditorPrefs.SetInt(SelectedViewTypeIndexKey, list.index);
            // The view type's state list should start out fresh so a reference doesn't have to be cached for each view type.
            m_ReorderableViewTypeStateList = null;
        }

        /// <summary>
        /// Draws the specified view type.
        /// </summary>
        private void DrawSelectedViewType(int index)
        {
            var viewType = m_CameraController.ViewTypes[index];
            InspectorUtility.DrawObject(viewType, true, true, target, true, SerializeViewTypes);

            if (InspectorUtility.Foldout(viewType, new GUIContent("States"), false)) {
                // The View Type class derives from system.object at the base level and reorderable lists can only operate on Unity objects. To get around this restriction
                // create a dummy array within a Unity object that corresponds to the number of elements within the view type's state list. When the reorderable list is drawn
                // the view type object will be used so it's like the dummy object never existed.
                var selectedViewType = viewType as ViewType;
                var gameObject = new GameObject();
                var stateIndexHelper = gameObject.AddComponent<StateInspectorHelper>();
                stateIndexHelper.StateIndexData = new int[selectedViewType.States.Length];
                for (int i = 0; i < stateIndexHelper.StateIndexData.Length; ++i) {
                    stateIndexHelper.StateIndexData[i] = i;
                }
                var stateIndexSerializedObject = new SerializedObject(stateIndexHelper);
                m_ReorderableViewTypeStateList = StateInspector.DrawStates(m_ReorderableViewTypeStateList, serializedObject, stateIndexSerializedObject.FindProperty("m_StateIndexData"), 
                                                            GetSelectedViewTypeStateIndexKey(selectedViewType), OnViewTypeStateListDraw, OnViewTypeStateListAdd, OnViewTypeStateListReorder, 
                                                            OnViewTypeStateListRemove);
                DestroyImmediate(gameObject);
            }
        }

        /// <summary>
        /// Returns the state index key for the specified view type.
        /// </summary>
        private string GetSelectedViewTypeStateIndexKey(ViewType viewType)
        {
            return c_EditorPrefsSelectedViewTypeStateIndexKey + "." + target.GetType() + "." + target.name + "." + viewType.GetType();
        }

        /// <summary>
        /// Draws all of the added states.
        /// </summary>
        private void OnViewTypeStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (m_ReorderableViewTypeStateList == null) {
                return;
            }

            var viewType = m_CameraController.ViewTypes[EditorPrefs.GetInt(SelectedViewTypeIndexKey)];

            // The index may be out of range if the component was copied.
            if (index >= m_CameraController.ViewTypes[EditorPrefs.GetInt(SelectedViewTypeIndexKey)].States.Length) {
                m_ReorderableViewTypeStateList.index = -1;
                EditorPrefs.SetInt(GetSelectedViewTypeStateIndexKey(viewType), m_ReorderableViewTypeStateList.index);
                return;
            }

            EditorGUI.BeginChangeCheck();
            StateInspector.OnStateListDraw(viewType, viewType.States, rect, index);
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                SerializeViewTypes();

                StateInspector.UpdateDefaultStateValues(viewType.States);
            }
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnViewTypeStateListAdd(ReorderableList list)
        {
            StateInspector.OnStateListAdd(AddExistingViewTypePreset, CreateViewTypePreset);
        }

        /// <summary>
        /// Adds a new element to the state list which uses an existing preset.
        /// </summary>
        private void AddExistingViewTypePreset()
        {
            var viewType = m_CameraController.ViewTypes[EditorPrefs.GetInt(SelectedViewTypeIndexKey)];
            var states = StateInspector.AddExistingPreset(viewType.GetType(), viewType.States, m_ReorderableViewTypeStateList, GetSelectedViewTypeStateIndexKey(viewType));
            if (viewType.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableViewTypeStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                viewType.States = states;
                SerializeViewTypes();
            }
        }

        /// <summary>
        /// Creates a new preset and adds it to a new state in the list.
        /// </summary>
        private void CreateViewTypePreset()
        {
            var viewType = m_CameraController.ViewTypes[EditorPrefs.GetInt(SelectedViewTypeIndexKey)];
            var states = StateInspector.CreatePreset(viewType, viewType.States, m_ReorderableViewTypeStateList, GetSelectedViewTypeStateIndexKey(viewType));
            if (viewType.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableViewTypeStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                viewType.States = states;
                SerializeViewTypes();
            }
        }

        /// <summary>
        /// The list has been reordered. Ensure the reorder is valid.
        /// </summary>
        private void OnViewTypeStateListReorder(ReorderableList list)
        {
            var viewType = m_CameraController.ViewTypes[EditorPrefs.GetInt(SelectedViewTypeIndexKey)];

            // Use the dummy array in order to determine what element the selected index was swapped with.
            var copiedStates = new UltimateCharacterController.StateSystem.State[viewType.States.Length];
            Array.Copy(viewType.States, copiedStates, viewType.States.Length);
            for (int i = 0; i < viewType.States.Length; ++i) {
                var element = list.serializedProperty.GetArrayElementAtIndex(i);
                if (element.intValue != i) {
                    viewType.States[i] = copiedStates[element.intValue];
                    element.intValue = i;
                }
            }

            var states = StateInspector.OnStateListReorder(viewType.States);
            if (viewType.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableViewTypeStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                viewType.States = states;
                SerializeViewTypes();
            }
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        private void OnViewTypeStateListRemove(ReorderableList list)
        {
            var viewType = m_CameraController.ViewTypes[EditorPrefs.GetInt(SelectedViewTypeIndexKey)];
            var states = StateInspector.OnStateListRemove(viewType.States, GetSelectedViewTypeStateIndexKey(viewType), list);
            if (viewType.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableViewTypeStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                viewType.States = states;
                SerializeViewTypes();
            }
        }

        /// <summary>
        /// Serialize all of the view tyoes to the ViewTypeData array.
        /// </summary>
        private void SerializeViewTypes()
        {
            ViewTypeBuilder.SerializeViewTypes(m_CameraController);

            // Update the default first and third person view types based off of the new view type list.
            UpdateDefaultViewTypes();
            InspectorUtility.SetDirty(target);
        }

        /// <summary>
        /// Updates the default first/third view type based on the view types availabe on the camera controller.
        /// </summary>
        private void UpdateDefaultViewTypes()
        {
            // The view type may not exist anymore.
            if (UnityEngineUtility.GetType(m_CameraController.FirstPersonViewTypeFullName) == null) {
                m_CameraController.FirstPersonViewTypeFullName = string.Empty;
                InspectorUtility.SetDirty(target);
            }
            if (UnityEngineUtility.GetType(m_CameraController.ThirdPersonViewTypeFullName) == null) {
                m_CameraController.ThirdPersonViewTypeFullName = string.Empty;
                InspectorUtility.SetDirty(target);
            }

            var hasSelectedViewType = false;
            var firstPersonViewTypes = new List<Type>();
            var thirdPersonViewTypes = new List<Type>();
            var firstPersonViewTypeNames = new List<string>();
            var thirdPersonViewTypeNames = new List<string>();
            var viewTypes = m_CameraController.ViewTypes;
            if (viewTypes != null) {
                for (int i = 0; i < viewTypes.Length; ++i) {
                    if (viewTypes[i] == null) {
                        continue;
                    }
                    // Transition view types are not limited to one perspective.
                    if (viewTypes[i] is UltimateCharacterController.Camera.ViewTypes.Transition) {
                        continue;
                    }
                    if (viewTypes[i].FirstPersonPerspective) {
                        // Use the view type if the type is currently empty.
                        if (string.IsNullOrEmpty(m_CameraController.FirstPersonViewTypeFullName)) {
                            m_CameraController.FirstPersonViewTypeFullName = viewTypes[i].GetType().FullName;
                        }
                        firstPersonViewTypes.Add(viewTypes[i].GetType());
                        firstPersonViewTypeNames.Add(InspectorUtility.DisplayTypeName(viewTypes[i].GetType(), false));
                    } else { // Third Person.
                        // Use the view type if the type is currently empty.
                        if (string.IsNullOrEmpty(m_CameraController.ThirdPersonViewTypeFullName)) {
                            m_CameraController.ThirdPersonViewTypeFullName = viewTypes[i].GetType().FullName;
                        }
                        thirdPersonViewTypes.Add(viewTypes[i].GetType());
                        thirdPersonViewTypeNames.Add(InspectorUtility.DisplayTypeName(viewTypes[i].GetType(), false));
                    }

                    if (m_CameraController.ViewTypeFullName == viewTypes[i].GetType().FullName) {
                        hasSelectedViewType = true;
                    }
                }
            }
            m_FirstPersonViewTypes = firstPersonViewTypes.ToArray();
            m_ThirdPersonViewTypes = thirdPersonViewTypes.ToArray();
            m_FirstPersonViewTypeNames = firstPersonViewTypeNames.ToArray();
            m_ThirdPersonViewTypeNames = thirdPersonViewTypeNames.ToArray();

            // If the selected ViewType no longer exists in the list then select the next view type.
            if (!hasSelectedViewType) {
                m_CameraController.ViewTypeFullName = string.Empty;
                if (viewTypes != null && viewTypes.Length > 0) {
                    for (int i = 0; i < viewTypes.Length; ++i) {
                        // Transition ViewTypes cannot be selected.
                        if (viewTypes[i] is UltimateCharacterController.Camera.ViewTypes.Transition) {
                            continue;
                        }

                        m_CameraController.ViewTypeFullName = viewTypes[i].GetType().FullName;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Deserialize the view tpes after an undo/redo.
        /// </summary>
        private void OnUndoRedo()
        {
            m_CameraController.DeserializeViewTypes(true);
            Repaint();
        }
    }
}