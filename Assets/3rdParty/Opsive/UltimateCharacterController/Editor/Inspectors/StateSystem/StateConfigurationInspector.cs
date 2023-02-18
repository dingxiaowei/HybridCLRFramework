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
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    /// <summary>
    /// Custom inspector for the StateConfiguration component.
    /// </summary>
    [CustomEditor(typeof(StateConfiguration))]
    public class StateConfigurationInspector : InspectorBase
    {
        /// <summary>
        /// Specifies what states the inspector should copy.
        /// </summary>
        private enum CopyType { All,       // Copies both the default and non-default states.
                                Default,   // Copies only the default states.
                                NonDefault // Copies only the non-default states.
                              }

        private const string c_EditorPrefsSelectedProfileKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedProfile";
        private string SelectedProfileKey { get { return c_EditorPrefsSelectedProfileKey + "." + target.GetType() + "." + target.name; } }
        private const int c_MaxPresetWidth = 120;
        private const int c_MinBlockedByWidth = 40;
        private const int c_MaxBlockedByWidth = 76;
        private const int c_WidthBuffer = 3;

        private ReorderableList m_StateReorderableList;
        private StateConfiguration m_StateConfiguration;
        private GameObject m_CopyFromObject;
        private CopyType m_CopyType;

        /// <summary>
        /// Creates a new StateConfiguration.
        /// </summary>
        [MenuItem("Assets/Create/Ultimate Character Controller/State Configuration")]
        public static void CreateStateConfiguration()
        {
            var path = EditorUtility.SaveFilePanel("Save State Configuration", InspectorUtility.GetSaveFilePath(), "StateConfiguration.asset", "asset");
            if (path.Length != 0 && Application.dataPath.Length < path.Length) {
                var stateConfiguration = ScriptableObject.CreateInstance<StateConfiguration>();

                // Save the configuration file.
                path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(stateConfiguration, path);
                AssetDatabase.ImportAsset(path);
            }
        }
        
        /// <summary>
        /// Assign the state configuration reference.
        /// </summary>
        public void OnEnable()
        {
            m_StateConfiguration = target as StateConfiguration;
            m_StateConfiguration.hideFlags = HideFlags.None;
            EditorUtility.SetDirty(m_StateConfiguration);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            m_CopyFromObject = EditorGUILayout.ObjectField("Copy From", m_CopyFromObject, typeof(GameObject), true) as GameObject;
            GUI.enabled = m_CopyFromObject != null;
            m_CopyType = (CopyType)EditorGUILayout.EnumPopup(m_CopyType, GUILayout.MaxWidth(70));
            if (GUILayout.Button("Copy", GUILayout.MaxWidth(70))) {
                CopyFromGameObject(m_CopyFromObject);
                m_StateReorderableList = null;
                EditorPrefs.SetInt(SelectedProfileKey, m_StateConfiguration.Profiles.Length - 1);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(15);
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginHorizontal();
            // Convert the state index into a string.
            var stateNames = new List<string>();
            if (m_StateConfiguration.Profiles == null || m_StateConfiguration.Profiles.Length == 0) {
                stateNames.Add("(None)");
            }
            if (m_StateConfiguration.Profiles != null) {
                for (int i = 0; i < m_StateConfiguration.Profiles.Length; ++i) {
                    stateNames.Add(m_StateConfiguration.Profiles[i].Name);
                }
            }
            var prevSelectedProfile = EditorPrefs.GetInt(SelectedProfileKey, 0);
            var selectedProfileIndex = EditorGUILayout.Popup("Selected Profile", prevSelectedProfile, stateNames.ToArray());
            // If the profile index changed then the reorderable list should be set to null so the reorderable list can refresh.
            if (selectedProfileIndex != prevSelectedProfile) {
                EditorPrefs.SetInt(SelectedProfileKey, selectedProfileIndex);
                m_StateReorderableList = null;
            }
            if (GUILayout.Button(InspectorStyles.AddIcon, InspectorStyles.NoPaddingButtonStyle, GUILayout.Width(18))) {
                // Add a new profile.
                var profiles = m_StateConfiguration.Profiles;
                Array.Resize(ref profiles, profiles != null ? profiles.Length + 1 : 1);
                var profile = new StateConfiguration.Profile();

                var count = profiles.Length;
                var profileName = string.Empty;
                do {
                    profileName = "Profile " + count;
                    count++;
                } while (!IsUniqueProfileName(profileName));

                profile.Name = profileName;
                profiles[profiles.Length - 1] = profile;
                m_StateConfiguration.Profiles = profiles;
                selectedProfileIndex = m_StateConfiguration.Profiles.Length - 1;
                EditorPrefs.SetInt(SelectedProfileKey, selectedProfileIndex);
                // Set the reorderableList to null and return so the correct array will be used on the next update.
                m_StateReorderableList = null;
            }
            GUI.enabled = m_StateConfiguration.Profiles != null && m_StateConfiguration.Profiles.Length > 0;
            if (GUILayout.Button(InspectorStyles.RemoveIcon, InspectorStyles.NoPaddingButtonStyle, GUILayout.Width(18))) {
                // Removes the current profile.
                var profileList = new List<StateConfiguration.Profile>(m_StateConfiguration.Profiles);

                var removeProfile = m_StateConfiguration.Profiles[selectedProfileIndex];
                for (int j = 0; j < removeProfile.StateElements.Length; ++j) {
                    if (removeProfile.StateElements[j].Default) {
                        DestroyImmediate(removeProfile.StateElements[j].Preset, true);
                    }
                }

                profileList.RemoveAt(selectedProfileIndex);
                m_StateConfiguration.Profiles = profileList.ToArray();
                selectedProfileIndex = Mathf.Max(0, selectedProfileIndex - 1);
                EditorPrefs.SetInt(SelectedProfileKey, selectedProfileIndex);
                // Set the reorderableList to null and return so the correct array will be used on the next update.
                m_StateReorderableList = null;
                AssetDatabase.SaveAssets();
            }
            if (GUILayout.Button(InspectorStyles.DuplicateIcon, InspectorStyles.NoPaddingButtonStyle, GUILayout.Width(20))) {
                // Duplicates the current profile.
                var profiles = m_StateConfiguration.Profiles;
                Array.Resize(ref profiles, profiles != null ? profiles.Length + 1 : 1);

                var duplicateProfile = profiles[selectedProfileIndex];
                var count = 1;
                var profileName = string.Empty;
                do {
                    profileName = duplicateProfile.Name + " " + count;
                    count++;
                } while (!IsUniqueProfileName(profileName));

                var profile = new StateConfiguration.Profile();

                profile.Name = profileName;
                profile.Type = duplicateProfile.Type;
                profile.StateElements = new StateConfiguration.Profile.StateElement[duplicateProfile.StateElements.Length];
                for (int i = 0; i < profile.StateElements.Length; ++i) {
                    var preset = ScriptableObject.Instantiate(duplicateProfile.StateElements[i].Preset);
                    preset.name = duplicateProfile.StateElements[i].Preset.name;
                    AssetDatabase.AddObjectToAsset(preset, m_StateConfiguration);
                    profile.StateElements[i] = new StateConfiguration.Profile.StateElement(duplicateProfile.StateElements[i].Name, preset,
                                                duplicateProfile.StateElements[i].BlockList, duplicateProfile.StateElements[i].Default);
                }

                profiles[profiles.Length - 1] = profile;
                m_StateConfiguration.Profiles = profiles;
                selectedProfileIndex = m_StateConfiguration.Profiles.Length - 1;
                EditorPrefs.SetInt(SelectedProfileKey, selectedProfileIndex);
                // Set the reorderableList to null and return so the correct array will be used on the next update.
                m_StateReorderableList = null;
                AssetDatabase.SaveAssets();
            }
            var selectedProfile = (m_StateConfiguration.Profiles != null && selectedProfileIndex < m_StateConfiguration.Profiles.Length) ? m_StateConfiguration.Profiles[selectedProfileIndex] : null;
            GUI.enabled = selectedProfile != null;
            GUILayout.EndHorizontal();
            var name = EditorGUILayout.TextField("Name", selectedProfile != null ? selectedProfile.Name : string.Empty);
            if (selectedProfile != null) {
                if (IsUniqueProfileName(name)) {
                    selectedProfile.Name = name;
                }
            }
            var profileType = (StateConfiguration.Profile.ProfileType)EditorGUILayout.EnumPopup("Profile Type", selectedProfile != null ? selectedProfile.Type : StateConfiguration.Profile.ProfileType.Character);
            if (selectedProfile != null) {
                selectedProfile.Type = profileType;
            }
            if (m_StateReorderableList == null) {
                StateConfiguration.Profile.StateElement[] states = selectedProfile != null ? selectedProfile.StateElements : null;
                if (states == null) {
                    states = new StateConfiguration.Profile.StateElement[0];
                }
                m_StateReorderableList = new ReorderableList(states, typeof(StateConfiguration.Profile.StateElement), true, true,
                                                                    m_StateConfiguration.Profiles != null && m_StateConfiguration.Profiles.Length > 0, states.Length > 0);
                m_StateReorderableList.drawHeaderCallback = OnStateListHeaderDraw;
                m_StateReorderableList.drawElementCallback = OnStateListDraw;
                m_StateReorderableList.onAddCallback = OnStateListAdd;
                m_StateReorderableList.onRemoveCallback = OnStateListRemove;
            }
            m_StateReorderableList.DoLayoutList();
            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Copies a set of profiles from the GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to copy the profiles from.</param>
        private void CopyFromGameObject(GameObject gameObject)
        {
            var stateList = new List<UltimateCharacterController.StateSystem.State>();
            var stateBehaviors = gameObject.GetComponents<StateBehavior>();
            for (int i = 0; i < stateBehaviors.Length; ++i) {
                AddStatesFromObject(stateBehaviors[i], stateList);

                // Add any states from the movement types, abilities, item abilities, and effects.
                if (stateBehaviors[i] is UltimateCharacterController.Character.UltimateCharacterLocomotion) {
                    var characterLocomotion = stateBehaviors[i] as UltimateCharacterController.Character.UltimateCharacterLocomotion;

                    characterLocomotion.DeserializeMovementTypes();
                    if (characterLocomotion.MovementTypes != null) {
                        for (int j = 0; j < characterLocomotion.MovementTypes.Length; ++j) {
                            AddStatesFromObject(characterLocomotion.MovementTypes[j], stateList);
                        }
                    }

                    characterLocomotion.DeserializeAbilities();
                    if (characterLocomotion.Abilities != null) {
                        for (int j = 0; j < characterLocomotion.Abilities.Length; ++j) {
                            AddStatesFromObject(characterLocomotion.Abilities[j], stateList);
                        }
                    }

                    characterLocomotion.DeserializeItemAbilities();
                    if (characterLocomotion.ItemAbilities != null) {
                        for (int j = 0; j < characterLocomotion.ItemAbilities.Length; ++j) {
                            AddStatesFromObject(characterLocomotion.ItemAbilities[j], stateList);
                        }
                    }

                    characterLocomotion.DeserializeEffects();
                    if (characterLocomotion.Effects != null) {
                        for (int j = 0; j < characterLocomotion.Effects.Length; ++j) {
                            AddStatesFromObject(characterLocomotion.Effects[j], stateList);
                        }
                    }
                }

                // Add any states from the view types.
                if (stateBehaviors[i] is UltimateCharacterController.Camera.CameraController) {
                    var cameraController = stateBehaviors[i] as UltimateCharacterController.Camera.CameraController;

                    cameraController.DeserializeViewTypes();
                    if (cameraController.ViewTypes != null) {
                        for (int j = 0; j < cameraController.ViewTypes.Length; ++j) {
                            AddStatesFromObject(cameraController.ViewTypes[j], stateList);
                        }
                    }
                }
            }

            AddStatesToProfile(gameObject, stateList);

            // Find any child items.
            var items = gameObject.GetComponentsInChildren<UltimateCharacterController.Items.Item>();
            for (int i = 0; i < items.Length; ++i) {
                // Ignore itself - the states have already been added.
                if (items[i].gameObject == gameObject) {
                    continue;
                }

                stateBehaviors = items[i].GetComponents<StateBehavior>();
                stateList.Clear();
                for (int j = 0; j < stateBehaviors.Length; ++j) {
                    AddStatesFromObject(stateBehaviors[j], stateList);
                }

                AddStatesToProfile(items[i].gameObject, stateList);
            }

            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Returns the states added to the specified object.
        /// </summary>
        /// <param name="obj">The object to get the states from/</param>
        /// <returns>The states added to the specified object.</returns>
        private void AddStatesFromObject(object obj, List<UltimateCharacterController.StateSystem.State> stateList)
        {
            UltimateCharacterController.StateSystem.State[] states = null;
            if (obj is StateBehavior) {
                states = (obj as StateBehavior).States;
            } else if (obj is StateObject) {
                states = (obj as StateObject).States;
            }
            if (states == null) {
                return;
            }
            if (m_CopyType == CopyType.All || m_CopyType == CopyType.NonDefault) {
                for (int i = 0; i < states.Length - 1; ++i) {
                    // Ignore any states that don't have a preset.
                    if (states[i].Preset == null) {
                        continue;
                    }
                    stateList.Add(states[i]);
                }
            }

            if (m_CopyType == CopyType.All || m_CopyType == CopyType.Default) {
                // The default state has to be persisted.
                var preset = PersistablePreset.CreatePreset(obj, MemberVisibility.AllPublic);
                preset.name = obj.GetType().Name;
                AssetDatabase.AddObjectToAsset(preset, m_StateConfiguration);
                stateList.Add(new UltimateCharacterController.StateSystem.State("Default" + preset.name, preset, null));
            }
        }

        /// <summary>
        /// Adds the list of states to the current profile.
        /// </summary>
        /// <param name="gameObject">The GameObject that the states were added from.</param>
        /// <param name="stateList">A list of states to add.</param>
        private void AddStatesToProfile(GameObject gameObject, List<UltimateCharacterController.StateSystem.State> stateList)
        {
            if (stateList.Count == 0) {
                return;
            }

            var profile = new StateConfiguration.Profile();
            // If the current name isn't unique then this profile needs to be updated.
            var addProfile = true;
            if (m_StateConfiguration.Profiles != null) {
                for (int i = 0; i < m_StateConfiguration.Profiles.Length; ++i) {
                    if (m_StateConfiguration.Profiles[i].Name == gameObject.name) {
                        profile = m_StateConfiguration.Profiles[i];
                        addProfile = false;
                        // The default presets should be removed because they will be regenerated.
                        for (int j = 0; j < profile.StateElements.Length; ++j) {
                            if (profile.StateElements[j].Default) {
                                DestroyImmediate(profile.StateElements[j].Preset, true);
                            }
                        }

                        break;
                    }
                }
            }
            profile.Name = gameObject.name;

            if (gameObject.GetComponent<UltimateCharacterController.Character.UltimateCharacterLocomotion>() != null) {
                profile.Type = StateConfiguration.Profile.ProfileType.Character;
            } else if (gameObject.GetComponent<UltimateCharacterController.Camera.CameraController>() != null) {
                profile.Type = StateConfiguration.Profile.ProfileType.Camera;
            } else if (gameObject.GetComponent<UltimateCharacterController.Items.Item>()) {
                profile.Type = StateConfiguration.Profile.ProfileType.Item;
            } else {
                Debug.LogWarning("Warning: The object " + gameObject.name + " is not a supported type.");
                return;
            }

            var stateElements = new StateConfiguration.Profile.StateElement[stateList.Count];
            var configPath = AssetDatabase.GetAssetPath(m_StateConfiguration);
            for (int i = 0; i < stateList.Count; ++i) {
                // The default states will have the same path as the base ScriptableObject.
                var withinConfiguration = configPath == AssetDatabase.GetAssetPath(stateList[i].Preset);
                stateElements[i] = new StateConfiguration.Profile.StateElement(stateList[i].Name, stateList[i].Preset as PersistablePreset, stateList[i].BlockList, withinConfiguration);
            }
            profile.StateElements = stateElements;
            if (addProfile) {
                var profiles = m_StateConfiguration.Profiles;
                Array.Resize(ref profiles, profiles != null ? profiles.Length + 1 : 1);
                profiles[profiles.Length - 1] = profile;
                m_StateConfiguration.Profiles = profiles;
            }
        }

        /// <summary>
        /// Is the profile name unique compared to the other profiles?
        /// </summary>
        private bool IsUniqueProfileName(string name)
        {
            // A blank string is not unique.
            if (string.IsNullOrEmpty(name)) {
                return false;
            }

            // A name is not unique if it is equal to any other state name.
            for (int i = 0; i < m_StateConfiguration.Profiles.Length; ++i) {
                if (m_StateConfiguration.Profiles[i].Name == name) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Draws the header for the State list.
        /// </summary>
        private void OnStateListHeaderDraw(Rect rect)
        {
            var fieldWidth = rect.width / 3;
            var blockedByWidth = Mathf.Max(c_MinBlockedByWidth, Mathf.Min(c_MaxBlockedByWidth, fieldWidth)) + EditorGUI.indentLevel * InspectorUtility.IndentWidth;
            fieldWidth = (rect.width - blockedByWidth) / 2 - (c_WidthBuffer * 3);
            var presetWidth = Mathf.Min(c_MaxPresetWidth, fieldWidth) + EditorGUI.indentLevel * InspectorUtility.IndentWidth * 2;
            var nameWidth = Mathf.Max(0, rect.width - presetWidth - blockedByWidth - (c_WidthBuffer * 4)) + EditorGUI.indentLevel * InspectorUtility.IndentWidth * 3;
            var startRectX = rect.x + 12;

            EditorGUI.LabelField(new Rect(startRectX, rect.y + 1, nameWidth, EditorGUIUtility.singleLineHeight), "Name");
            startRectX += nameWidth + c_WidthBuffer - EditorGUI.indentLevel * InspectorUtility.IndentWidth;
            EditorGUI.LabelField(new Rect(startRectX, rect.y + 1, presetWidth, EditorGUIUtility.singleLineHeight), "Preset");
            startRectX += presetWidth - c_WidthBuffer * 3 - EditorGUI.indentLevel * InspectorUtility.IndentWidth;
            EditorGUI.LabelField(new Rect(startRectX, rect.y + 1, blockedByWidth + EditorGUI.indentLevel * InspectorUtility.IndentWidth, EditorGUIUtility.singleLineHeight), "Blocked By");
        }

        /// <summary>
        /// Draws all of the states.
        /// </summary>
        public void OnStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (rect.width < 0) {
                return;
            }

            var profileIndex = EditorPrefs.GetInt(SelectedProfileKey, 0);
            var profile = m_StateConfiguration.Profiles[profileIndex];
            if (index >= profile.StateElements.Length) {
                return;
            }

            var stateElement = profile.StateElements[index];

            // Setup the field sizings.
            var fieldWidth = rect.width / 3;
            var blockedByWidth = Mathf.Max(c_MinBlockedByWidth, Mathf.Min(c_MaxBlockedByWidth, fieldWidth)) + EditorGUI.indentLevel * InspectorUtility.IndentWidth;
            fieldWidth = (rect.width - blockedByWidth) / 2 - (c_WidthBuffer * 3);
            var presetWidth = Mathf.Min(c_MaxPresetWidth, fieldWidth) + EditorGUI.indentLevel * 30;
            var nameWidth = Mathf.Max(0, rect.width - presetWidth - blockedByWidth - (c_WidthBuffer * 4)) + EditorGUI.indentLevel * InspectorUtility.IndentWidth * 3;
            var startRectX = rect.x;

            GUI.enabled = !stateElement.Default;
            var desiredName = EditorGUI.TextField(new Rect(startRectX, rect.y + 1, nameWidth, EditorGUIUtility.singleLineHeight), stateElement.Name);
            if (desiredName != stateElement.Name && IsUniqueStateName(profile.StateElements, stateElement, desiredName)) {
                stateElement.Name = desiredName;
            }
            startRectX += nameWidth + c_WidthBuffer - EditorGUI.indentLevel * InspectorUtility.IndentWidth;

            var preset = EditorGUI.ObjectField(new Rect(startRectX, rect.y + 1, presetWidth,
                                                        EditorGUIUtility.singleLineHeight), string.Empty, stateElement.Preset, typeof(PersistablePreset), false) as PersistablePreset;
            if (preset != stateElement.Preset) {
                // If the preset is just set then ensure the state name doesn't conflict with any other state name.
                stateElement.Preset = preset;
                var name = stateElement.Name;
                var count = 1;
                while (!IsUniqueStateName(profile.StateElements, stateElement, name)) {
                    name = stateElement.Name + " " + count;
                    count++;
                }
                stateElement.Name = name;
                m_StateConfiguration.ResetInitialization();
            }

            startRectX += presetWidth + c_WidthBuffer - EditorGUI.indentLevel * InspectorUtility.IndentWidth;

            // Create a popup of the states that can block the current state. There are several conditions which would prevent a state from being able to block
            // another state so this popup has to first be filtered.
            var stateName = stateElement.Name;
            var blockList = stateElement.BlockList;
            var allStates = new List<string>();
            var selected = 0;
            if (stateElement.Preset != null) {
                var objType = stateElement.Preset.Data.ObjectType;
                for (int i = 0; i < profile.StateElements.Length; ++i) {
                    var currentState = profile.StateElements[i];

                    // The current state cannot block another object type.
                    if (currentState.Preset == null || objType != currentState.Preset.Data.ObjectType) {
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
            } else {
                GUI.enabled = false;
            }
            // At least one value needs to exist.
            if (allStates.Count == 0) {
                allStates.Add("Nothing");
            }

            // Draw the actual popup.
            var blockMask = EditorGUI.MaskField(new Rect(startRectX, rect.y + 1, blockedByWidth, EditorGUIUtility.singleLineHeight), string.Empty, selected, allStates.ToArray());
            if (blockMask != selected) {
                var stateNames = new List<string>();
                for (int i = 0; i < allStates.Count; ++i) {
                    // If the state index is within the block mask then that state should be added to the list. A blockMask of -1 indicates Everything.
                    if (((1 << i) & blockMask) != 0 || blockMask == -1) {
                        stateNames.Add(allStates[i]);
                    }
                }
                stateElement.BlockList = stateNames.ToArray();
            }
            GUI.enabled = true;
        }

        /// <summary>
        /// Is the state name unique compared to the other states?
        /// </summary>
        private bool IsUniqueStateName(StateConfiguration.Profile.StateElement[] states, StateConfiguration.Profile.StateElement state, string name)
        {
            // A preset is required in order to match the object type.
            if (state.Preset == null) {
                return true;
            }

            // A blank string is not unique.
            if (string.IsNullOrEmpty(name)) {
                return false;
            }

            var objType = state.Preset.Data.ObjectType;
            // A name is not unique if it is equal to any other state name.
            for (int i = 0; i < states.Length; ++i) {
                // The object type must match in order to be compared.
                if (states[i].Preset == null || objType != states[i].Preset.Data.ObjectType) {
                    continue;
                }
                if (states[i] == state) {
                    continue;
                }
                if (states[i].Name == name) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        public void OnStateListAdd(ReorderableList list)
        {
            var profileIndex = EditorPrefs.GetInt(SelectedProfileKey, 0);
            var states = m_StateConfiguration.Profiles[profileIndex].StateElements;
            if (states == null) {
                states = new StateConfiguration.Profile.StateElement[1];
            } else {
                Array.Resize(ref states, states.Length + 1);
            }
            list.list = m_StateConfiguration.Profiles[profileIndex].StateElements = states;
            m_StateReorderableList.displayAdd = true;
            m_StateReorderableList.displayRemove = true;
            m_StateConfiguration.ResetInitialization();
            InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
            EditorUtility.SetDirty(target);
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        public void OnStateListRemove(ReorderableList list)
        {
            var profileIndex = EditorPrefs.GetInt(SelectedProfileKey, 0);
            var states = m_StateConfiguration.Profiles[profileIndex].StateElements;

            // Convert to a list and remove the state. A new list needs to be assigned because a new allocation occurred.
            var stateList = new List<StateConfiguration.Profile.StateElement>(states);
            // The preset is no longer used if the state is default.
            if (stateList[list.index].Default) {
                DestroyImmediate(stateList[list.index].Preset, true);
            }
            stateList.RemoveAt(list.index);
            list.list = m_StateConfiguration.Profiles[profileIndex].StateElements = stateList.ToArray();
            list.index = list.index - 1;
            m_StateReorderableList.displayRemove = stateList.Count > 0;
            m_StateConfiguration.ResetInitialization();
            InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
    }
}
