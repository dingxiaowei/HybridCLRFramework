/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.StateSystem;
    using Opsive.UltimateCharacterController.Utility;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    /// <summary>
    /// StateInspector is a helper class which will manage the inspector for the state system.
    /// </summary>
    public static class StateInspector
    {
        private const string c_EditorPrefsSelectedIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedStateIndex";
        private const string c_EditorPrefsLastPresetPathKey = "Opsive.UltimateCharacterController.Editor.Inspectors.LastPresetPath";
        private const int c_MaxPresetWidth = 120;
        private const int c_MinBlockedByWidth = 40;
        private const int c_MaxBlockedByWidth = 76;
        private const int c_MinPersistWidth = 20;
        private const int c_MaxPersistWidth = 48;
        private const int c_MinActivateWidth = 20;
        private const int c_MaxActivateWidth = 50;
        private const int c_WidthBuffer = 3;

        /// <summary>
        /// Draws the states within a ReorderableList.
        /// </summary>
        public static ReorderableList DrawStates(ReorderableList reorderableList, SerializedObject serializedObject, SerializedProperty states, string selectedIndexKey,
                                        ReorderableList.ElementCallbackDelegate drawCallback, ReorderableList.AddCallbackDelegate addCallback,
                                        ReorderableList.ReorderCallbackDelegate reorderCallback, ReorderableList.RemoveCallbackDelegate removeCallback)
        {
            // Initialize the reorder list on first run.
            if (reorderableList == null) {
                reorderableList = new ReorderableList(serializedObject, states, !Application.isPlaying, true, !Application.isPlaying, !Application.isPlaying && states.arraySize > 1);
                reorderableList.drawHeaderCallback = (Rect rect) =>
                {
                    // Setup the field sizings.
                    rect.x += 14;
                    rect.x -= EditorGUI.indentLevel * InspectorUtility.IndentWidth;
                    var fieldWidth = rect.width / 5;
                    var blockedByWidth = Mathf.Max(c_MinBlockedByWidth, Mathf.Min(c_MaxBlockedByWidth, fieldWidth)) + EditorGUI.indentLevel * InspectorUtility.IndentWidth;
                    fieldWidth = rect.width / 7;
                    var persistWidth = Mathf.Max(c_MinPersistWidth, Mathf.Min(c_MaxPersistWidth, fieldWidth));
                    var activateWidth = Mathf.Max(c_MinActivateWidth, Mathf.Min(c_MaxActivateWidth, fieldWidth));
                    fieldWidth = (rect.width - blockedByWidth - persistWidth - activateWidth) / 2 - (c_WidthBuffer * 3);
                    var presetWidth = Mathf.Min(c_MaxPresetWidth, fieldWidth) + EditorGUI.indentLevel * InspectorUtility.IndentWidth * 2;
                    var nameWidth = Mathf.Max(0, rect.width - presetWidth - blockedByWidth - persistWidth - activateWidth - (c_WidthBuffer * 6)) + EditorGUI.indentLevel * InspectorUtility.IndentWidth * 3;
                    var startRectX = rect.x;

                    EditorGUI.LabelField(new Rect(startRectX, rect.y + 1, nameWidth, EditorGUIUtility.singleLineHeight), "Name");
                    startRectX += nameWidth + c_WidthBuffer - EditorGUI.indentLevel * InspectorUtility.IndentWidth;
                    EditorGUI.LabelField(new Rect(startRectX, rect.y + 1, presetWidth, EditorGUIUtility.singleLineHeight), "Preset");
                    startRectX += presetWidth - c_WidthBuffer * 3 - EditorGUI.indentLevel * InspectorUtility.IndentWidth;
                    EditorGUI.LabelField(new Rect(startRectX, rect.y + 1, blockedByWidth + EditorGUI.indentLevel * InspectorUtility.IndentWidth, EditorGUIUtility.singleLineHeight), "Blocked By");
                    startRectX += blockedByWidth + c_WidthBuffer * 5 - EditorGUI.indentLevel * InspectorUtility.IndentWidth;
                    EditorGUI.LabelField(new Rect(startRectX, rect.y + 1, persistWidth + EditorGUI.indentLevel * InspectorUtility.IndentWidth, EditorGUIUtility.singleLineHeight), "Persist");
                    startRectX += persistWidth;
                    EditorGUI.LabelField(new Rect(startRectX, rect.y + 1, activateWidth + EditorGUI.indentLevel * InspectorUtility.IndentWidth, EditorGUIUtility.singleLineHeight), "Activate");
                };
                reorderableList.drawElementCallback = drawCallback;
                reorderableList.onAddCallback = addCallback;
                reorderableList.onReorderCallback = reorderCallback;
                reorderableList.onRemoveCallback = removeCallback;
                reorderableList.onSelectCallback = (ReorderableList list) =>
                {
                    EditorPrefs.SetInt(selectedIndexKey, list.index);
                };
                if (EditorPrefs.GetInt(selectedIndexKey, -1) != -1) {
                    reorderableList.index = EditorPrefs.GetInt(selectedIndexKey, -1);
                }
            }

            // Indent the list so it lines up with the rest of the content.
            var listRect = GUILayoutUtility.GetRect(0, reorderableList.GetHeight());
            listRect.x += InspectorUtility.IndentWidth * (EditorGUI.indentLevel + 1);
            listRect.xMax -= InspectorUtility.IndentWidth * (EditorGUI.indentLevel + 1);
            reorderableList.DoList(listRect);
            return reorderableList;
        }

        /// <summary>
        /// Draws all of the added states.
        /// </summary>
        public static void OnStateListDraw(object obj, UltimateCharacterController.StateSystem.State[] states, Rect rect, int index)
        {
            OnStateListDraw(obj, states, null, rect, index);
        }

        /// <summary>
        /// Draws all of the added states.
        /// </summary>
        public static void OnStateListDraw(object obj, UltimateCharacterController.StateSystem.State[] states, SerializedProperty statesProperty, Rect rect, int index)
        {
            if (rect.width < 0) {
                return;
            }

            // States cannot be edited at runtime, nor can the Default state ever be edited.
            GUI.enabled = !Application.isPlaying && index != states.Length - 1;
            rect.x -= EditorGUI.indentLevel * InspectorUtility.IndentWidth;

            // Ensure the default state doesn't get changed.
            var state = states[index];
            var stateProperty = (statesProperty != null ? statesProperty.GetArrayElementAtIndex(index) : null);
            if (!Application.isPlaying && index == states.Length - 1) {
                if (statesProperty != null && stateProperty == null) {
                    statesProperty.InsertArrayElementAtIndex(index);
                    stateProperty = statesProperty.GetArrayElementAtIndex(index);
                    stateProperty.FindPropertyRelative("m_Name").stringValue = "Default";
                    stateProperty.FindPropertyRelative("m_Default").boolValue = true;
                } else if (statesProperty == null && state == null) {
                    states[index] = state = new UltimateCharacterController.StateSystem.State("Default", true);
                    GUI.changed = true;
                }
                if (state.Name != "Default") {
                    if (stateProperty != null) {
                        stateProperty.FindPropertyRelative("m_Name").stringValue = "Default";
                    } else {
                        state.Name = "Default";
                    }
                    GUI.changed = true;
                }
                if (!state.Default) {
                    if (stateProperty != null) {
                        stateProperty.FindPropertyRelative("m_Default").boolValue = true;
                    } else {
                        state.Default = true;
                    }
                    GUI.changed = true;
                }
            }

            // Setup the field sizings.
            var fieldWidth = rect.width / 5;
            var blockedByWidth = Mathf.Max(c_MinBlockedByWidth, Mathf.Min(c_MaxBlockedByWidth, fieldWidth)) + EditorGUI.indentLevel * InspectorUtility.IndentWidth;
            fieldWidth = rect.width / 7;
            var persistWidth = Mathf.Max(c_MinPersistWidth, Mathf.Min(c_MaxPersistWidth, fieldWidth));
            var activateWidth = Mathf.Max(c_MinActivateWidth, Mathf.Min(c_MaxActivateWidth, fieldWidth));
            fieldWidth = (rect.width - blockedByWidth - persistWidth - activateWidth) / 2 - (c_WidthBuffer * 3);
            var presetWidth = Mathf.Min(c_MaxPresetWidth, fieldWidth) + EditorGUI.indentLevel * 30;
            var nameWidth = Mathf.Max(0, rect.width - presetWidth - blockedByWidth - persistWidth - activateWidth - (c_WidthBuffer * 6)) + EditorGUI.indentLevel * 45;
            var startRectX = rect.x;

            // The state name has to be unique. 
            var active = state.Active && !state.IsBlocked();
            var desiredName = EditorGUI.TextField(new Rect(startRectX, rect.y + 1, nameWidth, EditorGUIUtility.singleLineHeight), state.Name + 
                (active ? " (Active)" : string.Empty),
                (active ? InspectorStyles.BoldTextField : EditorStyles.textField));
            if (!Application.isPlaying && desiredName != state.Name && IsUniqueName(states, desiredName)) {
                // The name of the state that is blocking the current state should be updated.
                for (int i = 0; i < states.Length; ++i) {
                    if (states[i] == state) {
                        continue;
                    }

                    if (states[i].BlockList != null) {
                        for (int j = 0; j < states[i].BlockList.Length; ++j) {
                            if (states[i].BlockList[j] == state.Name) {
                                if (stateProperty != null) {
                                    statesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("m_BlockList").GetArrayElementAtIndex(j).stringValue = desiredName;
                                } else {
                                    states[i].BlockList[j] = desiredName;
                                }
                            }
                        }
                    }
                }

                if (stateProperty != null) {
                    stateProperty.FindPropertyRelative("m_Name").stringValue = desiredName;
                } else {
                    state.Name = desiredName;
                }
            }
            startRectX += nameWidth + c_WidthBuffer - EditorGUI.indentLevel * InspectorUtility.IndentWidth;

            // The preset cannot be null.
            var desiredPreset = EditorGUI.ObjectField(new Rect(startRectX, rect.y + 1, presetWidth,
                                                        EditorGUIUtility.singleLineHeight), string.Empty, state.Preset, typeof(PersistablePreset), false) as PersistablePreset;
            if (desiredPreset != null) {
                if (UnityEngineUtility.GetType(desiredPreset.Data.ObjectType).IsInstanceOfType(obj)) {
                    if (stateProperty != null) {
                        stateProperty.FindPropertyRelative("m_Preset").objectReferenceValue = desiredPreset;
                    } else {
                        state.Preset = desiredPreset;
                    }
                } else {
                    Debug.LogError($"Error: Unable to add preset. {desiredPreset.name} ({desiredPreset.Data.ObjectType}) doesn't use the same object type ({obj.GetType().FullName}).");
                }
            }
            startRectX += presetWidth + c_WidthBuffer - EditorGUI.indentLevel * InspectorUtility.IndentWidth;

            // Create a popup of the states that can block the current state. There are several conditions which would prevent a state from being able to block
            // another state so this popup has to first be filtered.
            var stateName = state.Name;
            var blockList = state.BlockList;
            var allStates = new List<string>();
            var selected = 0;
            for (int i = 0; i < states.Length; ++i) {
                var currentState = states[i];
                if (currentState == null) {
                    states[i] = currentState = new UltimateCharacterController.StateSystem.State();
                }
                // The current state cannot block the default state.
                if (currentState.Default) {
                    continue;
                }
                string name;
                // The current state cannot block itself.
                if ((name = currentState.Name) == stateName) {
                    continue;
                }
                // The selected state cannot block the current state if the current state blocks the selected state.
                var currentStateBlockList = currentState.BlockList;
                var canAdd = true;
                if (currentStateBlockList != null) {
                    for (int j = 0; j < currentStateBlockList.Length; ++j) {
                        if (stateName == currentStateBlockList[j]) {
                            canAdd = false;
                            break;
                        }
                    }
                }

                // canAdd will be false if the current state is blocking the selected state.
                if (!canAdd) {
                    continue;
                }

                // The current state can block the selected state. Add the name to the popup and determine if the state is selected. A mask is used
                // to allow multiple selected states.
                allStates.Add(name);
                if (blockList != null) {
                    for (int j = 0; j < blockList.Length; ++j) {
                        if (allStates[allStates.Count - 1] == blockList[j]) {
                            selected |= 1 << (allStates.Count - 1);
                            break;
                        }
                    }
                }
            }
            // At least one value needs to exist.
            if (allStates.Count == 0) {
                allStates.Add("Nothing");
            }

            // Draw the actual popup.
            var blockMask = EditorGUI.MaskField(new Rect(startRectX, rect.y + 1, blockedByWidth, EditorGUIUtility.singleLineHeight), string.Empty, selected, allStates.ToArray());
            if (blockMask != selected) {
                var stateNames = new List<string>();
                var blockListProperty = (stateProperty != null ? stateProperty.FindPropertyRelative("m_BlockList") : null);
                if (blockListProperty != null) {
                    blockListProperty.ClearArray();
                }
                for (int i = 0; i < allStates.Count; ++i) {
                    // If the state index is within the block mask then that state should be added to the list. A blockMask of -1 indicates Everything.
                    if (((1 << i) & blockMask) != 0 || blockMask == -1) {
                        if (blockListProperty != null) {
                            blockListProperty.InsertArrayElementAtIndex(blockListProperty.arraySize);
                            blockListProperty.GetArrayElementAtIndex(blockListProperty.arraySize - 1).stringValue = allStates[i];
                        } else {
                            stateNames.Add(allStates[i]);
                        }
                    }
                }
                if (blockListProperty == null) {
                    state.BlockList = stateNames.ToArray();
                }
            }
            startRectX += blockedByWidth + c_WidthBuffer;

            GUI.enabled = index < states.Length - 1;

            if (GUI.Button(new Rect(startRectX + persistWidth / 2, rect.y + 1, 18, EditorGUIUtility.singleLineHeight), InspectorStyles.PersistIcon, InspectorStyles.NoPaddingButtonStyle)) {
                // Populate the position map so ObjectInspector.DrawProperties to know which properties already exist.
                var valuePositionMap = new Dictionary<int, int>(desiredPreset.Data.ValueHashes.Length);
                for (int i = 0; i < desiredPreset.Data.ValueHashes.Length; ++i) {
                    valuePositionMap.Add(desiredPreset.Data.ValueHashes[i], i);
                }

                // Loop through all of the properties on the object.
                var properties = Serialization.GetSerializedProperties(obj.GetType(), MemberVisibility.Public);
                var bitwiseHash = new System.Version(desiredPreset.Data.Version).CompareTo(new System.Version("3.1")) >= 0;
                // Remove and add the properties that are being serialized.
                for (int i = 0; i < properties.Length; ++i) {
                    var hash = Serialization.StringHash(properties[i].PropertyType.FullName) + Serialization.StringHash(properties[i].Name);
                    // The property is currently being serialized.
                    if (valuePositionMap.ContainsKey(hash)) {
                        // Add the new property to the serialization.
                        object value = null;
                        var property = properties[i];
                        if (!typeof(Object).IsAssignableFrom(property.PropertyType)) {
                            var unityObjectIndexes = new List<int>();
                            Serialization.GetUnityObjectIndexes(ref unityObjectIndexes, property.PropertyType, property.Name, 0, valuePositionMap, desiredPreset.Data.ValueHashes, desiredPreset.Data.ValuePositions,
                                                                desiredPreset.Data.Values, false, MemberVisibility.Public, bitwiseHash);

                            Serialization.RemoveProperty(i, unityObjectIndexes, desiredPreset.Data, MemberVisibility.Public, bitwiseHash);

                            // Get the current value of the active object.
                            var getMethod = property.GetGetMethod();
                            if (getMethod != null) {
                                value = getMethod.Invoke(obj, null);
                            }
                            // Add the property back with the updated value.
                            Serialization.AddProperty(property, value, unityObjectIndexes, desiredPreset.Data, MemberVisibility.Public);
                        }
                    }
                }
            }
            startRectX += persistWidth + c_WidthBuffer;

            GUI.enabled = Application.isPlaying && index < states.Length - 1;
            if (GUI.Button(new Rect(startRectX + activateWidth / 2, rect.y + 1, 18, EditorGUIUtility.singleLineHeight), InspectorStyles.ActivateIcon, InspectorStyles.NoPaddingButtonStyle)) {
                StateManager.ActivateState(states[index], !states[index].Active, states);
            }

            GUI.enabled = true;
        }

        /// <summary>
        /// Is the state name unique compared to the other states?
        /// </summary>
        private static bool IsUniqueName(UltimateCharacterController.StateSystem.State[] states, string name)
        {
            // A blank string is not unique.
            if (string.IsNullOrEmpty(name)) {
                return false;
            }

            // A name is not unique if it is equal to any other state name.
            for (int i = 0; i < states.Length; ++i) {
                if (states[i].Name == name) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        public static void OnStateListAdd(GenericMenu.MenuFunction addExistingCallback, GenericMenu.MenuFunction createNewCallback)
        {
            var addMenu = new GenericMenu();
            if (!Application.isPlaying) {
                addMenu.AddItem(new GUIContent("Add Existing Preset"), false, addExistingCallback);
            }
            addMenu.AddItem(new GUIContent("Create New Preset"), false, createNewCallback);
            addMenu.ShowAsContext();
        }

        /// <summary>
        /// Adds a new element to the state list which uses an existing preset.
        /// </summary>
        public static UltimateCharacterController.StateSystem.State[] AddExistingPreset(System.Type objType, UltimateCharacterController.StateSystem.State[] states, ReorderableList reorderableList, string selectedIndexKey)
        {
            // A state must have a preset - open the file panel to select it.
            var path = EditorPrefs.GetString(c_EditorPrefsLastPresetPathKey, InspectorUtility.GetSaveFilePath());
            path = EditorUtility.OpenFilePanelWithFilters("Select Preset", path, new string[] { "Preset", "asset" });
            if (path.Length != 0 && Application.dataPath.Length < path.Length) {
                EditorPrefs.SetString(c_EditorPrefsLastPresetPathKey, System.IO.Path.GetDirectoryName(path));
                // The path is relative to the project.
                path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));
                var preset = AssetDatabase.LoadAssetAtPath<PersistablePreset>(path);
                if (preset == null) {
                    Debug.LogError($"Error: Unable to add preset. {System.IO.Path.GetFileName(path)} isn't located within the same project directory.");
                    return states;
                }
                // The preset object type has to belong to the same object type.
                if (preset.Data.ObjectType == objType.FullName) {
                    var startName = objType.Name + "Preset";
                    var name = preset.name;
                    if (!string.IsNullOrEmpty(name.Replace(startName, ""))) {
                        name = name.Replace(startName, "");
                    }
                    states = InsertStateElement(states, reorderableList, selectedIndexKey, name, preset);
                } else {
                    Debug.LogError($"Error: Unable to add preset. {preset.name} ({preset.Data.ObjectType}) doesn't use the same object type ({objType.FullName}).");
                }
            }
            return states;
        }

        /// <summary>
        /// Creates a new preset and adds it to a new state in the list.
        /// </summary>
        public static UltimateCharacterController.StateSystem.State[] CreatePreset(object target, UltimateCharacterController.StateSystem.State[] states, ReorderableList reorderableList, string selectedIndexKey)
        {
            var preset = PersistablePreset.CreatePreset(target);
            if (preset != null) {
                var startName = target.GetType().Name + "Preset.asset";
                var path = EditorPrefs.GetString(c_EditorPrefsLastPresetPathKey, InspectorUtility.GetSaveFilePath());
                path = EditorUtility.SaveFilePanel("Save Preset", path, startName, "asset");
                if (path.Length != 0 && Application.dataPath.Length < path.Length) {
                    EditorPrefs.SetString(c_EditorPrefsLastPresetPathKey, System.IO.Path.GetDirectoryName(path));
                    // The path is relative to the project.
                    path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));
                    // Do not delete/add if an existing preset already exists to prevent the references from being destroyed.
                    var existingPreset = AssetDatabase.LoadAssetAtPath<Preset>(path);
                    if (existingPreset != null) {
                        EditorUtility.DisplayDialog("Unable to Save Preset", "The preset must reference a unique file name.", "Okay");
                        return states;
                    }

                    var name = System.IO.Path.GetFileNameWithoutExtension(path);
                    if (!string.IsNullOrEmpty(name.Replace(target.GetType().Name + "Preset", ""))) {
                        name = name.Replace(target.GetType().Name + "Preset", "");
                    }

                    AssetDatabase.CreateAsset(preset, path);
                    AssetDatabase.ImportAsset(path);
                    EditorGUIUtility.PingObject(preset);
                    if (!Application.isPlaying) {
                        states = InsertStateElement(states, reorderableList, selectedIndexKey, name, preset);
                    }
                }
            }
            return states;
        }

        /// <summary>
        /// Inserts a new state element in the state array.
        /// </summary>
        private static UltimateCharacterController.StateSystem.State[] InsertStateElement(UltimateCharacterController.StateSystem.State[] states, ReorderableList reorderableList, string selectedIndexKey, string name, PersistablePreset preset)
        {
            // The name has to be unique to prevent it from interferring with other state names.
            if (!IsUniqueName(states, name)) {
                var postfixIndex = 1;
                while (!IsUniqueName(states, name + " " + postfixIndex)) {
                    postfixIndex++;
                }
                name += " " + postfixIndex;
            }

            // Create the element.
            var state = new UltimateCharacterController.StateSystem.State(name, false);
            state.Preset = preset;
            var stateList = new List<UltimateCharacterController.StateSystem.State>(states);
            stateList.Insert(0, state);
            reorderableList.displayRemove = stateList.Count > 1;

            // Select the new element.
            reorderableList.index = stateList.Count - 1;
            EditorPrefs.SetInt(selectedIndexKey, reorderableList.index);
            return stateList.ToArray();
        }

        /// <summary>
        /// The list has been reordered. Ensure the reorder is valid.
        /// </summary>
        public static UltimateCharacterController.StateSystem.State[] OnStateListReorder(UltimateCharacterController.StateSystem.State[] states)
        {
            // The Default state must always be last.
            for (int i = 0; i < states.Length; ++i) {
                var state = states[i];
                if (state.Default && i != states.Length - 1) {
                    SwapElements(states, i, states.Length - 1);
                    break;
                }
            }
            return states;
        }

        /// <summary>
        /// Swaps the elements in the specified positions of the array.
        /// </summary>
        public static void SwapElements(UltimateCharacterController.StateSystem.State[] states, int origIndex, int newIndex)
        {
            var origElement = states[origIndex];
            states[origIndex] = states[newIndex];
            states[newIndex] = origElement;
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        public static UltimateCharacterController.StateSystem.State[] OnStateListRemove(UltimateCharacterController.StateSystem.State[] states, string selectedIndexKey, ReorderableList reorderableList)
        {
            return OnStateListRemove(states, null, selectedIndexKey, reorderableList);
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        public static UltimateCharacterController.StateSystem.State[] OnStateListRemove(UltimateCharacterController.StateSystem.State[] states, SerializedProperty statesProperty, string selectedIndexKey, ReorderableList reorderableList)
        {
            // The last state cannot be removed.
            if (reorderableList.index == states.Length - 1) {
                EditorUtility.DisplayDialog("Unable to Remove", "The Default State cannot be removed.", "OK");
                return states;
            }

            // The block lists must be updated to account for the state removal.
            for (int i = 0; i < states.Length; ++i) {
                if (i == reorderableList.index) {
                    continue;
                }

                var state = states[i];
                if (state.BlockList != null && state.BlockList.Length > 0) {
                    var blockList = new List<string>(state.BlockList);
                    var blockListProperty = (statesProperty != null ? statesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("m_BlockList") : null);
                    for (int j = blockList.Count - 1; j > -1; --j) {
                        if (blockList[j] == states[reorderableList.index].Name) {
                            blockList.RemoveAt(j);
                            if (blockListProperty != null) {
                                blockListProperty.DeleteArrayElementAtIndex(j);
                            }
                        }
                    }
                    state.BlockList = blockList.ToArray();
                }
            }

            var stateList = new List<UltimateCharacterController.StateSystem.State>(states);
            stateList.RemoveAt(reorderableList.index);
            if (statesProperty != null) {
                statesProperty.DeleteArrayElementAtIndex(reorderableList.index);
            }
            reorderableList.index = reorderableList.index - 1;
            if (reorderableList.index == -1 && stateList.Count > 0) {
                reorderableList.index = 0;
            }
            reorderableList.displayRemove = stateList.Count > 1;
            return stateList.ToArray();
        }

        /// <summary>
        /// Updates the value of the default state if the game is active.
        /// </summary>
        /// <param name="states">An array of all of the states.</param>
        public static void UpdateDefaultStateValues(UltimateCharacterController.StateSystem.State[] states)
        {
            // If there is a change to the object with only the default state active then the values on the preset should be updated. This will prevent the values from
            // switching back to the default state when there is a state change.
            if (Application.isPlaying) {
                var onlyDefaultActive = true;
                for (int i = 0; i < states.Length - 1; ++i) {
                    if (states[i].Active) {
                        onlyDefaultActive = false;
                        break;
                    }
                }

                if (onlyDefaultActive && states[states.Length - 1].Preset != null) {
                    states[states.Length - 1].Preset.UpdateValue();
                }
            }
        }
    }
}