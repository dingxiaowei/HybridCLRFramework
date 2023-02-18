/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character
{
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.MovementTypes;
    using Opsive.UltimateCharacterController.Character.Effects;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.StateSystem;
    using Opsive.UltimateCharacterController.Utility;
    using Opsive.UltimateCharacterController.Utility.Builders;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    /// <summary>
    /// Shows a custom inspector for the UltimateCharacterLocomotion.
    /// </summary>
    [CustomEditor(typeof(UltimateCharacterLocomotion))]
    public class UltimateCharacterLocomotionInspector : StateBehaviorInspector
    {
        private const string c_EditorPrefsSelectedMovementTypeIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedMovementTypeIndex";
        private const string c_EditorPrefsSelectedMovementTypeStateIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.MovementTypes.SelectedMovementTypeStateIndex";
        private const string c_EditorPrefsSelectedAbilityIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedAbilityIndex";
        private const string c_EditorPrefsSelectedAbilityStateIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.Ability.SelectedAbilityStateIndex";
        private const string c_EditorPrefsSelectedItemAbilityIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedItemAbilityIndex";
        private const string c_EditorPrefsSelectedItemAbilityStateIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.Ability.SelectedItemAbilityStateIndex";
        private const string c_EditorPrefsSelectedEffectIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedEffectIndex";
        private const string c_EditorPrefsSelectedEffectStateIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.Effect.SelectedAbilityStateIndex";
        private const string c_EditorPrefsLastLastAnimatorCodePathKey = "Opsive.UltimateCharacterController.Editor.Inspectors.Ability.LastAnimatorCodePath";
        private string SelectedMovementTypeIndexKey { get { return c_EditorPrefsSelectedMovementTypeIndexKey + "." + target.GetType() + "." + target.name; } }
        private string SelectedAbilityIndexKey { get { return c_EditorPrefsSelectedAbilityIndexKey + "." + target.GetType() + "." + target.name; } }
        private string SelectedItemAbilityIndexKey { get { return c_EditorPrefsSelectedItemAbilityIndexKey + "." + target.GetType() + "." + target.name; } }
        private string SelectedEffectIndexKey { get { return c_EditorPrefsSelectedEffectIndexKey + "." + target.GetType() + "." + target.name; } }

        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private ReorderableList m_ReorderableMovementTypeList;
        private ReorderableList m_ReorderableAbilityList;
        private ReorderableList m_ReorderableItemAbilityList;
        private ReorderableList m_ReorderableEffectList;
        private ReorderableList m_ReorderableMovementTypeStateList;
        private ReorderableList m_ReorderableAbilityStateList;
        private ReorderableList m_ReorderableItemAbilityStateList;
        private ReorderableList m_ReorderableEffectStateList;

        private string[] m_FirstPersonMovementTypeNames;
        private string[] m_ThirdPersonMovementTypeNames;
        private Type[] m_FirstPersonMovementTypes;
        private Type[] m_ThirdPersonMovementTypes;

        /// <summary>
        /// Initialize any starting values.
        /// </summary>
        protected override void OnEnable()
        {
            m_CharacterLocomotion = target as UltimateCharacterLocomotion;

            ReorderableListSerializationHelper.OnEnable();

            // After an undo or redo has been performed the character effects need to be deserialized.
            Undo.undoRedoPerformed += OnUndoRedo;

            // The movement types, abilities, effects may have changed since the last serialization (such as if a class no longer exists) so serialize the objects
            // again if there is a change.
            if (m_CharacterLocomotion.MovementTypes == null && m_CharacterLocomotion.DeserializeMovementTypes()) {
                // Do not serialize the movement types during runtime.
                if (!Application.isPlaying) {
                    SerializeMovementTypes();
                }
            }
            if (m_CharacterLocomotion.Abilities == null && m_CharacterLocomotion.DeserializeAbilities()) {
                // Do not serialize the abilities during runtime.
                if (!Application.isPlaying) {
                    SerializeAbilities();
                }
            }
            if (m_CharacterLocomotion.ItemAbilities == null && m_CharacterLocomotion.DeserializeItemAbilities()) {
                // Do not serialize the item abilities during runtime.
                if (!Application.isPlaying) {
                    SerializeItemAbilities();
                }
            }
            if (m_CharacterLocomotion.Effects == null && m_CharacterLocomotion.DeserializeEffects()) {
                // Do not serialize the abilities during runtime.
                if (!Application.isPlaying) {
                    SerializeEffects();
                }
            }

            UpdateDefaultMovementTypes();
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
                if (Application.isPlaying) {
                    EditorGUILayout.HelpBox("Having this inspector open may decrease your framerate. For best performance the character GameObject " +
                                            "should not be selected to prevent all of the inspectors from drawing.", MessageType.Warning);
                }


                // Only show the first/third person movement type popup if that movement type is available.
                if (m_FirstPersonMovementTypeNames != null && m_FirstPersonMovementTypeNames.Length > 0) {
                    var selectedIndex = 0;
                    for (int i = 0; i < m_FirstPersonMovementTypes.Length; ++i) {
                        if (m_FirstPersonMovementTypes[i].FullName == m_CharacterLocomotion.FirstPersonMovementTypeFullName) {
                            selectedIndex = i;
                            break;
                        }
                    }
                    var index = EditorGUILayout.Popup("First Person Movement Type", selectedIndex, m_FirstPersonMovementTypeNames);
                    if (index != selectedIndex) {
                        m_CharacterLocomotion.FirstPersonMovementTypeFullName = m_FirstPersonMovementTypes[index].FullName;
                        // Update the default movement type if the current movement type is first person. Do not update when playing because the first person property will update the current type.
                        if (Application.isPlaying && m_CharacterLocomotion.ActiveMovementType.GetType().FullName.Contains("FirstPerson")) {
                            m_CharacterLocomotion.MovementTypeFullName = m_CharacterLocomotion.FirstPersonMovementTypeFullName;
                        }
                    }
                }
                if (m_ThirdPersonMovementTypeNames != null && m_ThirdPersonMovementTypeNames.Length > 0) {
                    var selectedIndex = 0;
                    for (int i = 0; i < m_ThirdPersonMovementTypes.Length; ++i) {
                        if (m_ThirdPersonMovementTypes[i].FullName == m_CharacterLocomotion.ThirdPersonMovementTypeFullName) {
                            selectedIndex = i;
                            break;
                        }
                    }
                    var index = EditorGUILayout.Popup("Third Person Movement Type", selectedIndex, m_ThirdPersonMovementTypeNames);
                    if (index != selectedIndex) {
                        m_CharacterLocomotion.ThirdPersonMovementTypeFullName = m_ThirdPersonMovementTypes[index].FullName;
                        // Update the default movement type if the current movement type is third person. Do not update when playing because the third person property will update the current type.
                        if (!Application.isPlaying && (m_CharacterLocomotion.ActiveMovementType == null || m_CharacterLocomotion.ActiveMovementType.GetType().FullName.Contains("ThirdPerson"))) {
                            m_CharacterLocomotion.MovementTypeFullName = m_CharacterLocomotion.ThirdPersonMovementTypeFullName;
                        }
                    }
                }
                EditorGUILayout.BeginVertical("Box");
                ReorderableListSerializationHelper.DrawReorderableList(ref m_ReorderableMovementTypeList, this, m_CharacterLocomotion.MovementTypes, "m_MovementTypeData",
                                                                OnMovementTypeListDrawHeader, OnMovementTypeListDraw, OnMovementTypeListReorder, OnMovementTypeListAdd,
                                                                OnMovementTypeListRemove, OnMovementTypeListSelect,
                                                                DrawSelectedMovementType, SelectedMovementTypeIndexKey, true, false);
                EditorGUILayout.EndVertical();

                EditorGUILayout.PropertyField(PropertyFromName("m_FirstPersonStateName"));
                EditorGUILayout.PropertyField(PropertyFromName("m_ThirdPersonStateName"));

                if (Foldout("Motor")) {
                    EditorGUI.indentLevel++;
                    var updateLocation = PropertyFromName("m_UpdateLocation");
                    EditorGUILayout.PropertyField(updateLocation);
                    if (updateLocation.enumValueIndex == (int)UltimateCharacterController.Game.KinematicObjectManager.UpdateLocation.Update) {
                        EditorGUILayout.HelpBox("It is recommended that the framerate is limited using Application.targetFrameRate.", MessageType.Info);
                    }
                    var useRootMotionPosition = PropertyFromName("m_UseRootMotionPosition");
                    EditorGUILayout.PropertyField(useRootMotionPosition);
                    EditorGUI.indentLevel++;
                    if (useRootMotionPosition.boolValue) {
                        EditorGUILayout.PropertyField(PropertyFromName("m_RootMotionSpeedMultiplier"));
                        EditorGUILayout.PropertyField(PropertyFromName("m_RootMotionAirForceMultiplier"));
                    } else {
                        EditorGUILayout.PropertyField(PropertyFromName("m_MotorAcceleration"));
                        EditorGUILayout.PropertyField(PropertyFromName("m_MotorDamping"));
                        EditorGUILayout.PropertyField(PropertyFromName("m_MotorAirborneAcceleration"));
                        EditorGUILayout.PropertyField(PropertyFromName("m_MotorAirborneDamping"));
                        EditorGUILayout.Slider(PropertyFromName("m_PreviousAccelerationInfluence"), 0, 1);
                    }
                    EditorGUI.indentLevel--;
                    var useRootMotionRotation = PropertyFromName("m_UseRootMotionRotation");
                    EditorGUILayout.PropertyField(useRootMotionRotation);
                    EditorGUI.indentLevel++;
                    if (useRootMotionRotation.boolValue) {
                        EditorGUILayout.PropertyField(PropertyFromName("m_RootMotionRotationMultiplier"));
                    } else {
                        EditorGUILayout.PropertyField(PropertyFromName("m_MotorRotationSpeed"));
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.PropertyField(PropertyFromName("m_MotorBackwardsMultiplier"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_MotorSlopeForceUp"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_MotorSlopeForceDown"));
                    // The Ultimate Character Controller has a callback for the changed timescale so set the controller property instead of using the SerializedProperty.
                    var timeScaleProperty = PropertyFromName("m_TimeScale");
                    if (Application.isPlaying) {
                        var prevTimeScaleValue = timeScaleProperty.floatValue;
                        var timeScaleValue = EditorGUILayout.Slider(new GUIContent(InspectorUtility.SplitCamelCase(timeScaleProperty.name), timeScaleProperty.tooltip), timeScaleProperty.floatValue, 0, 4);
                        if (timeScaleValue != prevTimeScaleValue) {
                            m_CharacterLocomotion.TimeScale = timeScaleValue;
                        }
                    } else {
                        EditorGUILayout.Slider(timeScaleProperty, 0, 4);
                    }
                    EditorGUI.indentLevel--;
                }

                if (Foldout("Physics")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_Mass"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_SkinWidth"));
                    EditorGUILayout.Slider(PropertyFromName("m_SlopeLimit"), 0, 90);
                    var stepHeight = PropertyFromName("m_MaxStepHeight");
                    EditorGUILayout.PropertyField(stepHeight);
                    // The step height should always be less than or equal to the collider radius. This will prevent any jittering when moving up stairs.
                    var maxColliderRadius = GetMaxColliderRadius();
                    if (stepHeight.floatValue > maxColliderRadius) {
                        EditorGUILayout.HelpBox("Warning: The Max Step Height is greater than the max collider radius (" + maxColliderRadius + "). The step height should " +
                                                "be decreased or the collider radius should be increased in order for the step height to work correctly.", MessageType.Warning);
                    }
                    EditorGUILayout.PropertyField(PropertyFromName("m_ExternalForceDamping"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ExternalForceAirDamping"));
                    var useGravity = PropertyFromName("m_UseGravity");
                    EditorGUILayout.PropertyField(useGravity);
                    if (useGravity.boolValue) {
                        EditorGUI.indentLevel++;
                        var gravityDirection = PropertyFromName("m_GravityDirection").vector3Value;
                        EditorGUILayout.PropertyField(PropertyFromName("m_GravityDirection"));
                        // Set the property to ensure the direction is normalized.
                        if (gravityDirection != PropertyFromName("m_GravityDirection").vector3Value) {
                            m_CharacterLocomotion.GravityDirection = PropertyFromName("m_GravityDirection").vector3Value;
                        }
                        EditorGUILayout.PropertyField(PropertyFromName("m_GravityMagnitude"));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.PropertyField(PropertyFromName("m_WallGlideCurve"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_WallBounceModifier"));
                    EditorGUI.indentLevel--;
                }

                if (Foldout("Collisions")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_DetectHorizontalCollisions"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_DetectVerticalCollisions"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ColliderLayerMask"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_MaxCollisionCount"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_MaxSoftForceFrames"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_RotationCollisionCheckCount"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_MaxOverlapIterations"));
                    EditorGUI.indentLevel--;
                }

                if (Foldout("Movement")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_SmoothedBones"), true);
                    EditorGUILayout.PropertyField(PropertyFromName("m_MovingStateName"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_AirborneStateName"));
                    var stickToGround = PropertyFromName("m_StickToGround");
                    EditorGUILayout.PropertyField(stickToGround);
                    if (stickToGround.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(PropertyFromName("m_Stickiness"));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }

                if (Foldout("Moving Platform")) {
                    EditorGUI.indentLevel++;
                    var stickToMovingPlatform = PropertyFromName("m_StickToMovingPlatform");
                    EditorGUILayout.PropertyField(stickToMovingPlatform);
                    if (!stickToMovingPlatform.boolValue) {
                        EditorGUILayout.PropertyField(PropertyFromName("m_MovingPlatformSeperationVelocity"));
                    }
                    EditorGUILayout.PropertyField(PropertyFromName("m_MinHorizontalMovingPlatformStickSpeed"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_MovingPlatformForceDamping"));
                    EditorGUI.indentLevel--;
                }

                if (Foldout("Animator")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_YawMultiplier"));
                    EditorGUI.indentLevel--;
                }

                // Draw the ability and effect reorderable lists last.
                if (Foldout("Abilities")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical("Box");
                    ReorderableListSerializationHelper.DrawReorderableList(ref m_ReorderableAbilityList, this, m_CharacterLocomotion.Abilities, "m_AbilityData",
                                                                    OnAbilityListDrawHeader, OnAbilityListDraw, OnAbilityListReorder, OnAbilityListAdd, OnAbilityListRemove, OnAbilityListSelect,
                                                                    DrawSelectedAbility, SelectedAbilityIndexKey, false, true);
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                }
                if (Foldout("Item Abilities")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical("Box");
                    ReorderableListSerializationHelper.DrawReorderableList(ref m_ReorderableItemAbilityList, this, m_CharacterLocomotion.ItemAbilities, "m_ItemAbilityData",
                                                                    OnItemAbilityListDrawHeader, OnItemAbilityListDraw, OnItemAbilityListReorder, OnItemAbilityListAdd, OnItemAbilityListRemove,
                                                                    OnItemAbilityListSelect, DrawSelectedItemAbility, SelectedItemAbilityIndexKey, false, true);
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                }
                if (Foldout("Effects")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical("Box");
                    ReorderableListSerializationHelper.DrawReorderableList(ref m_ReorderableEffectList, this, m_CharacterLocomotion.Effects, "m_EffectData", OnEffectListDrawHeader,
                                                                    OnEffectListDraw, OnEffectListReorder, OnEffectListAdd, OnEffectListRemove, OnEffectListSelect, DrawSelectedEffect,
                                                                    SelectedEffectIndexKey, false, true);
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                }
                if (Foldout("Events")) {
                    EditorGUI.indentLevel++;
                    InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnMovementTypeActiveEvent"));
                    InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnAbilityActiveEvent"));
                    InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnItemAbilityActiveEvent"));
                    InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnGroundedEvent"));
                    InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnLandEvent"));
                    InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnChangeTimeScaleEvent"));
                    InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnChangeMovingPlatformsEvent"));
                    EditorGUI.indentLevel--;
                }
            };

            return baseCallback;
        }

        /// <summary>
        /// Draws the header for the movement type list.
        /// </summary>
        private void OnMovementTypeListDrawHeader(Rect rect)
        {
            var activeRect = rect;
            activeRect.width -= 33;
            EditorGUI.LabelField(activeRect, "Movement Type");

            activeRect.x += activeRect.width - 12;
            activeRect.width = 49;
            EditorGUI.LabelField(activeRect, "Active");
        }

        /// <summary>
        /// Draws the header for the ability list.
        /// </summary>
        private void OnAbilityListDrawHeader(Rect rect)
        {
            var activeRect = rect;
            activeRect.x += 13;
            activeRect.width -= 33;
            EditorGUI.LabelField(activeRect, "Ability");

            activeRect.x += activeRect.width - 32;
            activeRect.width = 50;
            EditorGUI.LabelField(activeRect, "Enabled");
        }

        /// <summary>
        /// Draws the header for the item ability list.
        /// </summary>
        private void OnItemAbilityListDrawHeader(Rect rect)
        {
            var activeRect = rect;
            activeRect.x += 13;
            activeRect.width -= 33;
            EditorGUI.LabelField(activeRect, "Item Ability");

            activeRect.x += activeRect.width - 32;
            activeRect.width = 50;
            EditorGUI.LabelField(activeRect, "Enabled");
        }

        /// <summary>
        /// Draws the header for the effect list.
        /// </summary>
        private void OnEffectListDrawHeader(Rect rect)
        {
            var activeRect = rect;
            activeRect.x += 13;
            activeRect.width -= 33;
            EditorGUI.LabelField(activeRect, "Effect");

            activeRect.x += activeRect.width - 32;
            activeRect.width = 50;
            EditorGUI.LabelField(activeRect, "Enabled");
        }

        /// <summary>
        /// Draws all of the added movement types.
        /// </summary>
        private void OnMovementTypeListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            // The index may be out of range if the component was copied.
            if (index >= m_CharacterLocomotion.MovementTypes.Length) {
                m_ReorderableMovementTypeList.index = -1;
                EditorPrefs.SetInt(SelectedMovementTypeIndexKey, m_ReorderableMovementTypeList.index);
                return;
            }

            var movementType = m_CharacterLocomotion.MovementTypes[index];
            var label = InspectorUtility.DisplayTypeName(movementType.GetType(), true);

            // Reduce the rect width so the active toggle can be added.
            var activeRect = rect;
            activeRect.width -= 20;
            EditorGUI.LabelField(activeRect, label);

            // Draw the active toggle and serialize if there is a change.
            EditorGUI.BeginChangeCheck();
            activeRect = rect;
            activeRect.x += activeRect.width - 32;
            activeRect.width = 20;
            EditorGUI.Toggle(activeRect, m_CharacterLocomotion.MovementTypeFullName == movementType.GetType().FullName, EditorStyles.radioButton);
            if (EditorGUI.EndChangeCheck()) {
                // Update the camera's view type if the movement type perspective changed.
                if (!Application.isPlaying) {
                    var camera = UnityEngineUtility.FindCamera(m_CharacterLocomotion.gameObject);
                    if (camera != null) {
                        var prevFirstPerson = m_CharacterLocomotion.MovementTypeFullName.Contains("FirstPersonController");
                        var firstPerson = movementType.GetType().FullName.Contains("FirstPersonController");
                        if (prevFirstPerson != firstPerson) {
                            var cameraController = camera.GetComponent<UltimateCharacterController.Camera.CameraController>();
                            var viewType = firstPerson ? cameraController.FirstPersonViewTypeFullName : cameraController.ThirdPersonViewTypeFullName;
                            if (!string.IsNullOrEmpty(viewType)) {
                                cameraController.SetViewType(UnityEngineUtility.GetType(viewType), true);
                                var cameraSerializedObject = new SerializedObject(cameraController);
                                PropertyFromName(cameraSerializedObject, "m_ViewTypeFullName").stringValue = UnityEngineUtility.GetType(viewType).FullName;
                                if (firstPerson) {
                                    PropertyFromName(cameraSerializedObject, "m_FirstPersonViewTypeFullName").stringValue = UnityEngineUtility.GetType(viewType).FullName;
                                } else {
                                    PropertyFromName(cameraSerializedObject, "m_ThirdPersonViewTypeFullName").stringValue = UnityEngineUtility.GetType(viewType).FullName;
                                }
                                cameraSerializedObject.ApplyModifiedProperties();
                            }
                        }
                    }
                }
                m_CharacterLocomotion.MovementTypeFullName = PropertyFromName("m_MovementTypeFullName").stringValue = movementType.GetType().FullName;
                if (movementType.GetType().FullName.Contains("FirstPerson")) {
                    m_CharacterLocomotion.FirstPersonMovementTypeFullName = PropertyFromName("m_FirstPersonMovementTypeFullName").stringValue = m_CharacterLocomotion.MovementTypeFullName;
                } else {
                    m_CharacterLocomotion.ThirdPersonMovementTypeFullName = PropertyFromName("m_ThirdPersonMovementTypeFullName").stringValue = m_CharacterLocomotion.MovementTypeFullName;
                }
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Draws all of the added abilities.
        /// </summary>
        private void OnAbilityListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            // The index may be out of range if the component was copied.
            if (index >= m_CharacterLocomotion.Abilities.Length) {
                m_ReorderableAbilityList.index = -1;
                EditorPrefs.SetInt(SelectedAbilityIndexKey, m_ReorderableAbilityList.index);
                return;
            }

            var ability = m_CharacterLocomotion.Abilities[index];
            if (ability == null) {
                SerializeAbilities();
                return;
            }

            var label = InspectorUtility.DisplayTypeName(ability.GetType(), true);
            var description = ability.AbilityDescription;
            var inspectorDescription = ability.InspectorDescription;
            if (!string.IsNullOrEmpty(inspectorDescription)) {
                if (string.IsNullOrEmpty(description)) {
                    description = inspectorDescription;
                } else {
                    description += ", " + inspectorDescription;
                }
            }
            if (!string.IsNullOrEmpty(description)) {
                label = string.Format("{0} ({1})", label, description);
            }
            if (ability.IsActive) {
                label += " (Active)";
            }

            // Reduce the rect width so the enabled toggle can be added.
            var activeRect = rect;
            activeRect.width -= 20;
            EditorGUI.LabelField(activeRect, label);

            // Draw the enabled toggle and serialize if there is a change.
            EditorGUI.BeginChangeCheck();
            activeRect = rect;
            activeRect.x += activeRect.width - 32;
            activeRect.width = 20;
            ability.Enabled = EditorGUI.Toggle(activeRect, ability.Enabled);
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                SerializeAbilities();
            }
        }

        /// <summary>
        /// Draws all of the added Item abilities.
        /// </summary>
        private void OnItemAbilityListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            // The index may be out of range if the component was copied.
            if (index >= m_CharacterLocomotion.ItemAbilities.Length) {
                m_ReorderableItemAbilityList.index = -1;
                EditorPrefs.SetInt(SelectedItemAbilityIndexKey, m_ReorderableItemAbilityList.index);
                return;
            }

            var itemAbility = m_CharacterLocomotion.ItemAbilities[index];
            if (itemAbility == null) {
                SerializeItemAbilities();
                return;
            }

            var label = InspectorUtility.DisplayTypeName(itemAbility.GetType(), true);
            var description = itemAbility.AbilityDescription;
            var inspectorDescription = itemAbility.InspectorDescription;
            if (!string.IsNullOrEmpty(inspectorDescription)) {
                if (string.IsNullOrEmpty(description)) {
                    description = inspectorDescription;
                } else {
                    description += ", " + inspectorDescription;
                }
            }
            if (!string.IsNullOrEmpty(description)) {
                label = string.Format("{0} ({1})", label, description);
            }
            if (itemAbility.IsActive) {
                label += " (Active)";
            }

            // Reduce the rect width so the enabled toggle can be added.
            var activeRect = rect;
            activeRect.width -= 20;
            EditorGUI.LabelField(activeRect, label);

            // Draw the enabled toggle and serialize if there is a change.
            EditorGUI.BeginChangeCheck();
            activeRect = rect;
            activeRect.x += activeRect.width - 32;
            activeRect.width = 20;
            itemAbility.Enabled = EditorGUI.Toggle(activeRect, itemAbility.Enabled);
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                SerializeItemAbilities();
            }
        }

        /// <summary>
        /// Draws all of the added effects.
        /// </summary>
        private void OnEffectListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            // The index may be out of range if the component was copied.
            if (index >= m_CharacterLocomotion.Effects.Length) {
                m_ReorderableEffectList.index = -1;
                EditorPrefs.SetInt(SelectedEffectIndexKey, m_ReorderableEffectList.index);
                return;
            }

            var effect = m_CharacterLocomotion.Effects[index];
            if (effect == null) {
                SerializeEffects();
                return;
            }

            var label = InspectorUtility.DisplayTypeName(effect.GetType(), true);
            var inspectorDescription = effect.InspectorDescription;
            if (!string.IsNullOrEmpty(inspectorDescription)) {
                label = string.Format("{0} ({1})", label, inspectorDescription);
            }
            if (effect.IsActive) {
                label += " (Active)";
            }

            // Reduce the rect width so the enabled toggle can be added.
            var activeRect = rect;
            activeRect.width -= 20;
            EditorGUI.LabelField(activeRect, label);

            // Draw the enabled toggle and serialize if there is a change.
            EditorGUI.BeginChangeCheck();
            activeRect = rect;
            activeRect.x += activeRect.width - 32;
            activeRect.width = 20;
            effect.Enabled = EditorGUI.Toggle(activeRect, effect.Enabled);
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                SerializeEffects();
            }
        }

        /// <summary>
        /// The movement type list has been reordered.
        /// </summary>
        private void OnMovementTypeListReorder(ReorderableList list)
        {
            // Deserialize the movement types so the MovementTypes array will be correct. The list operates on the AbilityData array.
            m_CharacterLocomotion.DeserializeMovementTypes(true);

            // Update the selected index.
            EditorPrefs.SetInt(SelectedMovementTypeIndexKey, list.index);
        }

        /// <summary>
        /// The ability list has been reordered.
        /// </summary>
        private void OnAbilityListReorder(ReorderableList list)
        {
            // Deserialize the abilities so the Abilities array will be correct. The list operates on the AbilityData array.
            m_CharacterLocomotion.DeserializeAbilities(true);

            // Update the ability index.
            for (int i = 0; i < m_CharacterLocomotion.Abilities.Length; ++i) {
                m_CharacterLocomotion.Abilities[i].Index = i;
            }
            // Serialize the new ability index.
            SerializeAbilities();

            // Update the selected index.
            EditorPrefs.SetInt(SelectedAbilityIndexKey, m_ReorderableAbilityList.index);
        }

        /// <summary>
        /// The item ability list has been reordered.
        /// </summary>
        private void OnItemAbilityListReorder(ReorderableList list)
        {
            // Deserialize the item abilities so the ItemAbilities array will be correct. The list operates on the ItemAbilityData array.
            m_CharacterLocomotion.DeserializeItemAbilities(true);

            // Update the ability index.
            for (int i = 0; i < m_CharacterLocomotion.ItemAbilities.Length; ++i) {
                m_CharacterLocomotion.ItemAbilities[i].Index = i;
            }
            // Serialize the new item ability index.
            SerializeItemAbilities();

            // Update the selected index.
            EditorPrefs.SetInt(SelectedItemAbilityIndexKey, m_ReorderableItemAbilityList.index);
        }

        /// <summary>
        /// The effect list has been reordered.
        /// </summary>
        private void OnEffectListReorder(ReorderableList list)
        {
            // Deserialize the item effects so the Effects array will be correct. The list operates on the EffectData array.
            m_CharacterLocomotion.DeserializeEffects(true);

            // Serialize the new effect list.
            SerializeEffects();

            // Update the selected index.
            EditorPrefs.SetInt(SelectedEffectIndexKey, m_ReorderableEffectList.index);
        }

        /// <summary>
        /// Adds a new movement type element to the list.
        /// </summary>
        private void OnMovementTypeListAdd(ReorderableList list)
        {
            ReorderableListSerializationHelper.AddObjectType(typeof(MovementType), true, m_CharacterLocomotion.MovementTypes, AddMovementType);
        }

        /// <summary>
        /// Adds a new ability element to the list.
        /// </summary>
        private void OnAbilityListAdd(ReorderableList list)
        {
            ReorderableListSerializationHelper.AddObjectType(typeof(Ability), true, m_CharacterLocomotion.Abilities, AddAbility);
        }

        /// <summary>
        /// Adds a new item ability element to the list.
        /// </summary>
        private void OnItemAbilityListAdd(ReorderableList list)
        {
            ReorderableListSerializationHelper.AddObjectType(typeof(ItemAbility), true, m_CharacterLocomotion.ItemAbilities, AddItemAbility);
        }

        /// <summary>
        /// Adds a new effect element to the list.
        /// </summary>
        private void OnEffectListAdd(ReorderableList list)
        {
            ReorderableListSerializationHelper.AddObjectType(typeof(Effect), true, m_CharacterLocomotion.Effects, AddEffect);
        }

        /// <summary>
        /// Adds the movement type with the specified type.
        /// </summary>
        private void AddMovementType(object obj)
        {
            var movementTypes = m_CharacterLocomotion.MovementTypes;
            if (movementTypes == null) {
                movementTypes = new MovementType[1];
            } else {
                Array.Resize(ref movementTypes, movementTypes.Length + 1);
            }
            movementTypes[movementTypes.Length - 1] = Activator.CreateInstance(obj as Type) as MovementType;
            m_CharacterLocomotion.MovementTypes = movementTypes;
            SerializeMovementTypes();

#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
            // If both a first and third person movement type exists then the PerspectiveMonitor should also be added.
            if (m_CharacterLocomotion.GetComponent<UltimateCharacterController.ThirdPersonController.Character.PerspectiveMonitor>() == null) {
                var hasFirstPersonMovementType = false;
                var hasThirdPersonMovementType = false;
                for (int i = 0; i < m_CharacterLocomotion.MovementTypes.Length; ++i) {
                    if (!hasFirstPersonMovementType && m_CharacterLocomotion.MovementTypes[i].GetType().FullName.Contains("FirstPersonController")) {
                        hasFirstPersonMovementType = true;
                    } else if (!hasThirdPersonMovementType && m_CharacterLocomotion.MovementTypes[i].GetType().FullName.Contains("ThirdPersonController")) {
                        hasThirdPersonMovementType = true;
                    }
                    if (hasFirstPersonMovementType && hasThirdPersonMovementType) {
                        break;
                    }
                }

                // If a first and third person movement type exists then the component should be added.
                if (hasFirstPersonMovementType && hasThirdPersonMovementType) {
                    m_CharacterLocomotion.gameObject.AddComponent<UltimateCharacterController.ThirdPersonController.Character.PerspectiveMonitor>();
                }
            }
#endif

            // Don't show the remove button if there is only one movement type left.
            m_ReorderableMovementTypeList.displayRemove = m_CharacterLocomotion.MovementTypes.Length > 1;

            // Select the newly added movementType.
            m_ReorderableMovementTypeList.index = movementTypes.Length - 1;
            EditorPrefs.SetInt(SelectedMovementTypeIndexKey, m_ReorderableMovementTypeList.index);
        }

        /// <summary>
        /// Adds the ability with the specified type.
        /// </summary>
        private void AddAbility(object obj)
        {
            var ability = AbilityBuilder.AddAbility(m_CharacterLocomotion, obj as Type);

            // Select the newly added ability.
            m_ReorderableAbilityList.index = m_CharacterLocomotion.Abilities.Length - 1;
            EditorPrefs.SetInt(SelectedAbilityIndexKey, m_ReorderableAbilityList.index);

            // Allow the ability to perform any initialization.
            var inspectorDrawer = InspectorDrawerUtility.InspectorDrawerForType(ability.GetType()) as AbilityInspectorDrawer;
            if (inspectorDrawer != null) {
                inspectorDrawer.AbilityAdded(ability, target);
            }
        }

        /// <summary>
        /// Adds the item ability with the specified type.
        /// </summary>
        private void AddItemAbility(object obj)
        {
            var ability = AbilityBuilder.AddItemAbility(m_CharacterLocomotion, obj as Type);

            // Select the newly added item ability.
            m_ReorderableItemAbilityList.index = m_CharacterLocomotion.ItemAbilities.Length - 1;
            EditorPrefs.SetInt(SelectedItemAbilityIndexKey, m_ReorderableItemAbilityList.index);

            // Allow the ability to perform any initialization.
            var inspectorDrawer = InspectorDrawerUtility.InspectorDrawerForType(ability.GetType()) as AbilityInspectorDrawer;
            if (inspectorDrawer != null) {
                inspectorDrawer.AbilityAdded(ability, target);
            }
        }

        /// <summary>
        /// Adds the effect with the specified type.
        /// </summary>
        private void AddEffect(object obj)
        {
            var effects = m_CharacterLocomotion.Effects;
            if (effects == null) {
                effects = new Effect[1];
            } else {
                Array.Resize(ref effects, effects.Length + 1);
            }
            effects[effects.Length - 1] = Activator.CreateInstance(obj as Type) as Effect;
            m_CharacterLocomotion.Effects = effects;
            SerializeEffects();

            // Select the newly added effect.
            m_ReorderableEffectList.index = effects.Length - 1;
            EditorPrefs.SetInt(SelectedEffectIndexKey, m_ReorderableEffectList.index);
        }

        /// <summary>
        /// Remove the movement type at the list index.
        /// </summary>
        private void OnMovementTypeListRemove(ReorderableList list)
        {
            var movementTypes = new List<MovementType>(m_CharacterLocomotion.MovementTypes);
            // Select a new movement type if the currently selected movement type is being removed.
            var removedSelected = movementTypes[list.index].GetType().FullName == m_CharacterLocomotion.MovementTypeFullName;

            // Remove the element.
            InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
            movementTypes.RemoveAt(list.index);
            m_CharacterLocomotion.MovementTypes = movementTypes.ToArray();
            SerializeMovementTypes();

            // Don't show the remove button if there is only one movement type left.
            m_ReorderableMovementTypeList.displayRemove = m_CharacterLocomotion.MovementTypes.Length > 1;

            // Update the index to point to no longer point to the now deleted movement type.
            list.index = list.index - 1;
            if (list.index == -1 && movementTypes.Count > 0) {
                list.index = 0;
            }
            if (removedSelected) {
                m_CharacterLocomotion.MovementTypeFullName = movementTypes[list.index].GetType().FullName;
            }
            EditorPrefs.SetInt(SelectedMovementTypeIndexKey, list.index);

#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
            // If both a first and third person movement type no longer exist then the PerspectiveMonitor should also be removed.
            if (m_CharacterLocomotion.GetComponent<UltimateCharacterController.ThirdPersonController.Character.PerspectiveMonitor>() != null) {
                var hasFirstPersonMovementType = false;
                var hasThirdPersonMovementType = false;
                for (int i = 0; i < m_CharacterLocomotion.MovementTypes.Length; ++i) {
                    if (!hasFirstPersonMovementType && m_CharacterLocomotion.MovementTypes[i].GetType().FullName.Contains("FirstPersonController")) {
                        hasFirstPersonMovementType = true;
                    } else if (!hasThirdPersonMovementType && m_CharacterLocomotion.MovementTypes[i].GetType().FullName.Contains("ThirdPersonController")) {
                        hasThirdPersonMovementType = true;
                    }
                    if (hasFirstPersonMovementType && hasThirdPersonMovementType) {
                        break;
                    }
                }

                // If a first and third person movement type no longer exist then the component should be removed.
                if (!hasFirstPersonMovementType || !hasThirdPersonMovementType) {
                    DestroyImmediate(m_CharacterLocomotion.gameObject.GetComponent<UltimateCharacterController.ThirdPersonController.Character.PerspectiveMonitor>(), true);
                }
            }
#endif

            UpdateDefaultMovementTypes();
        }

        /// <summary>
        /// Remove the ability at the list index.
        /// </summary>
        private void OnAbilityListRemove(ReorderableList list)
        {
            InspectorUtility.RecordUndoDirtyObject(target, "Change Value");

            // Allow the ability to perform any destruction.
            var ability = m_CharacterLocomotion.Abilities[list.index];
            var inspectorDrawer = InspectorDrawerUtility.InspectorDrawerForType(ability.GetType()) as AbilityInspectorDrawer;
            if (inspectorDrawer != null) {
                inspectorDrawer.AbilityRemoved(ability, target);
            }

            var abilities = new List<Ability>(m_CharacterLocomotion.Abilities);
            abilities.RemoveAt(list.index);
            m_CharacterLocomotion.Abilities = abilities.ToArray();
            SerializeAbilities();

            // Update the index to point to no longer point to the now deleted ability.
            list.index = list.index - 1;
            if (list.index == -1 && abilities.Count > 0) {
                list.index = 0;
            }
            EditorPrefs.SetInt(SelectedAbilityIndexKey, list.index);
        }

        /// <summary>
        /// Remove the item ability at the list index.
        /// </summary>
        private void OnItemAbilityListRemove(ReorderableList list)
        {
            InspectorUtility.RecordUndoDirtyObject(target, "Change Value");

            // Allow the ability to perform any destruction.
            var ability = m_CharacterLocomotion.ItemAbilities[list.index];
            var inspectorDrawer = InspectorDrawerUtility.InspectorDrawerForType(ability.GetType()) as AbilityInspectorDrawer;
            if (inspectorDrawer != null) {
                inspectorDrawer.AbilityRemoved(ability, target);
            }

            var itemAbilities = new List<ItemAbility>(m_CharacterLocomotion.ItemAbilities);
            itemAbilities.RemoveAt(list.index);
            m_CharacterLocomotion.ItemAbilities = itemAbilities.ToArray();
            SerializeItemAbilities();

            // Update the index to point to no longer point to the now deleted ability.
            list.index = list.index - 1;
            if (list.index == -1 && itemAbilities.Count > 0) {
                list.index = 0;
            }
            EditorPrefs.SetInt(SelectedItemAbilityIndexKey, list.index);
        }

        /// <summary>
        /// Remove the effect at the list index.
        /// </summary>
        private void OnEffectListRemove(ReorderableList list)
        {
            InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
            var effects = new List<Effect>(m_CharacterLocomotion.Effects);
            effects.RemoveAt(list.index);
            m_CharacterLocomotion.Effects = effects.ToArray();
            SerializeEffects();

            // Update the index to point to no longer point to the now deleted effect.
            list.index = list.index - 1;
            if (list.index == -1 && effects.Count > 0) {
                list.index = 0;
            }
            EditorPrefs.SetInt(SelectedEffectIndexKey, list.index);
        }

        /// <summary>
        /// A new element has been selected within the list.
        /// </summary>
        private void OnMovementTypeListSelect(ReorderableList list)
        {
            EditorPrefs.SetInt(SelectedMovementTypeIndexKey, list.index);
            // The movement type's state list should start out fresh so a reference doesn't have to be cached for each movement type.
            m_ReorderableMovementTypeStateList = null;
        }

        /// <summary>
        /// A new element has been selected within the list.
        /// </summary>
        private void OnAbilityListSelect(ReorderableList list)
        {
            EditorPrefs.SetInt(SelectedAbilityIndexKey, list.index);
            // The ability's state list should start out fresh so a reference doesn't have to be cached for each ability.
            m_ReorderableAbilityStateList = null;
        }

        /// <summary>
        /// A new element has been selected within the list.
        /// </summary>
        private void OnItemAbilityListSelect(ReorderableList list)
        {
            EditorPrefs.SetInt(SelectedItemAbilityIndexKey, list.index);
            // The item ability's state list should start out fresh so a reference doesn't have to be cached for each item ability.
            m_ReorderableItemAbilityStateList = null;
        }

        /// <summary>
        /// A new element has been selected within the list.
        /// </summary>
        private void OnEffectListSelect(ReorderableList list)
        {
            EditorPrefs.SetInt(SelectedEffectIndexKey, list.index);
            // The effect's state list should start out fresh so a reference doesn't have to be cached for each effect.
            m_ReorderableEffectStateList = null;
        }

        /// <summary>
        /// Draws the specified movement type.
        /// </summary>
        private void DrawSelectedMovementType(int index)
        {
            EditorGUI.indentLevel++;
            var movementType = m_CharacterLocomotion.MovementTypes[index];
            InspectorUtility.DrawObject(movementType, true, true, target, true, SerializeMovementTypes);

            if (InspectorUtility.Foldout(movementType, new GUIContent("States"), false)) {
                // The MovementType class derives from system.object at the base level and reorderable lists can only operate on Unity objects. To get around this restriction
                // create a dummy array within a Unity object that corresponds to the number of elements within the ability's state list. When the reorderable list is drawn
                // the ability object will be used so it's like the dummy object never existed.
                var selectedMovementType = movementType as MovementType;
                var gameObject = new GameObject();
                var stateIndexHelper = gameObject.AddComponent<StateInspectorHelper>();
                stateIndexHelper.StateIndexData = new int[selectedMovementType.States.Length];
                for (int i = 0; i < stateIndexHelper.StateIndexData.Length; ++i) {
                    stateIndexHelper.StateIndexData[i] = i;
                }
                var stateIndexSerializedObject = new SerializedObject(stateIndexHelper);
                m_ReorderableMovementTypeStateList = StateInspector.DrawStates(m_ReorderableMovementTypeStateList, serializedObject, stateIndexSerializedObject.FindProperty("m_StateIndexData"),
                                                            GetSelectedMovementTypeStateIndexKey(selectedMovementType), OnMovementTypeStateListDraw, OnMovementTypeStateListAdd,
                                                            OnMovementTypeStateListReorder, OnMovementTypeStateListRemove);
                DestroyImmediate(gameObject);
            }
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Returns the state index key for the specified movement type.
        /// </summary>
        private string GetSelectedMovementTypeStateIndexKey(MovementType movementType)
        {
            return c_EditorPrefsSelectedMovementTypeStateIndexKey + "." + target.GetType() + "." + target.name + "." + movementType.GetType();
        }

        /// <summary>
        /// Draws all of the added states.
        /// </summary>
        private void OnMovementTypeStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (m_CharacterLocomotion.MovementTypes == null || EditorPrefs.GetInt(SelectedMovementTypeIndexKey) >= m_CharacterLocomotion.MovementTypes.Length) {
                m_ReorderableMovementTypeStateList.index = -1;
                return;
            }

            EditorGUI.BeginChangeCheck();
            var movementType = m_CharacterLocomotion.MovementTypes[EditorPrefs.GetInt(SelectedMovementTypeIndexKey)];

            // The index may be out of range if the component was copied.
            if (index >= movementType.States.Length) {
                m_ReorderableMovementTypeStateList.index = -1;
                EditorPrefs.SetInt(GetSelectedMovementTypeStateIndexKey(movementType), m_ReorderableMovementTypeStateList.index);
                return;
            }

            StateInspector.OnStateListDraw(movementType, movementType.States, rect, index);
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                SerializeMovementTypes();

                StateInspector.UpdateDefaultStateValues(movementType.States);
            }
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnMovementTypeStateListAdd(ReorderableList list)
        {
            StateInspector.OnStateListAdd(AddExistingMovementTypePreset, CreateMovementTypePreset);
        }

        /// <summary>
        /// Adds a new element to the state list which uses an existing preset.
        /// </summary>
        private void AddExistingMovementTypePreset()
        {
            var movementType = m_CharacterLocomotion.MovementTypes[EditorPrefs.GetInt(SelectedMovementTypeIndexKey)];
            var states = StateInspector.AddExistingPreset(movementType.GetType(), movementType.States, m_ReorderableMovementTypeStateList, GetSelectedMovementTypeStateIndexKey(movementType));
            if (movementType.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableMovementTypeStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                movementType.States = states;
                SerializeMovementTypes();
            }
        }

        /// <summary>
        /// Creates a new preset and adds it to a new state in the list.
        /// </summary>
        private void CreateMovementTypePreset()
        {
            var movementType = m_CharacterLocomotion.MovementTypes[EditorPrefs.GetInt(SelectedMovementTypeIndexKey)];
            var states = StateInspector.CreatePreset(movementType, movementType.States, m_ReorderableMovementTypeStateList, GetSelectedMovementTypeStateIndexKey(movementType));
            if (movementType.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableMovementTypeStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                movementType.States = states;
                SerializeMovementTypes();
            }
        }

        /// <summary>
        /// The list has been reordered. Ensure the reorder is valid.
        /// </summary>
        private void OnMovementTypeStateListReorder(ReorderableList list)
        {
            var movementType = m_CharacterLocomotion.MovementTypes[EditorPrefs.GetInt(SelectedMovementTypeIndexKey)];

            // Use the dummy array in order to determine what element the selected index was swapped with.
            var copiedStates = new UltimateCharacterController.StateSystem.State[movementType.States.Length];
            Array.Copy(movementType.States, copiedStates, movementType.States.Length);
            for (int i = 0; i < movementType.States.Length; ++i) {
                var element = list.serializedProperty.GetArrayElementAtIndex(i);
                if (element.intValue != i) {
                    movementType.States[i] = copiedStates[element.intValue];
                    element.intValue = i;
                }
            }

            var states = StateInspector.OnStateListReorder(movementType.States);
            if (movementType.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableMovementTypeStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                movementType.States = states;
                SerializeMovementTypes();
            }
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        private void OnMovementTypeStateListRemove(ReorderableList list)
        {
            var movementType = m_CharacterLocomotion.MovementTypes[EditorPrefs.GetInt(SelectedMovementTypeIndexKey)];
            var states = StateInspector.OnStateListRemove(movementType.States, GetSelectedMovementTypeStateIndexKey(movementType), list);
            if (movementType.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableMovementTypeStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                movementType.States = states;
                SerializeMovementTypes();
            }
        }

        /// <summary>
        /// Draws the specified ability.
        /// </summary>
        private void DrawSelectedAbility(int index)
        {
            var ability = m_CharacterLocomotion.Abilities[index];
            InspectorUtility.DrawObject(ability, true, false, target, true, SerializeAbilities);

            var selectedAbility = ability as Ability;
            if (InspectorUtility.Foldout(ability, new GUIContent("States"), false)) {
                // The Ability class derives from system.object at the base level and reorderable lists can only operate on Unity objects. To get around this restriction
                // create a dummy array within a Unity object that corresponds to the number of elements within the ability's state list. When the reorderable list is drawn
                // the ability object will be used so it's like the dummy object never existed.
                var gameObject = new GameObject();
                var stateIndexHelper = gameObject.AddComponent<StateInspectorHelper>();
                stateIndexHelper.StateIndexData = new int[selectedAbility.States.Length];
                for (int i = 0; i < stateIndexHelper.StateIndexData.Length; ++i) {
                    stateIndexHelper.StateIndexData[i] = i;
                }
                var stateIndexSerializedObject = new SerializedObject(stateIndexHelper);
                m_ReorderableAbilityStateList = StateInspector.DrawStates(m_ReorderableAbilityStateList, serializedObject, stateIndexSerializedObject.FindProperty("m_StateIndexData"), 
                                                            GetSelectedAbilityStateIndexKey(selectedAbility), OnAbilityStateListDraw, OnAbilityStateListAdd, OnAbilityStateListReorder, 
                                                            OnAbilityStateListRemove);
                DestroyImmediate(gameObject);
            }

            EditorGUI.BeginChangeCheck();
            DrawEditorControls(selectedAbility);
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                SerializeAbilities();
            }
        }

        /// <summary>
        /// Draws any editor controls that the ability can draw.
        /// </summary>
        /// <param name="ability">The ability which should draw the editor controls.</param>
        private void DrawEditorControls(Ability ability)
        {
            // The abilities can have an editor callback. If the callback exists then show the Editor foldout after the States foldout.
            var inspectorDrawer = InspectorDrawerUtility.InspectorDrawerForType(ability.GetType()) as AbilityInspectorDrawer;
            Action editorCallback = null;
            if (inspectorDrawer != null) {
                editorCallback = inspectorDrawer.GetEditorCallback(ability, target);
                if ((editorCallback != null || inspectorDrawer.CanBuildAnimator || ability.AbilityIndexParameter != -1) && InspectorUtility.Foldout(ability, new GUIContent("Editor"), false)) {
                    EditorGUI.indentLevel++;
                    if (editorCallback != null) {
                        editorCallback();
                    }
                    var animator = (target as UltimateCharacterLocomotion).GetComponent<Animator>();
                    UnityEditor.Animations.AnimatorController animatorController = null;
                    if (animator != null) {
                        animatorController = (UnityEditor.Animations.AnimatorController)animator.runtimeAnimatorController;
                    }

                    UnityEditor.Animations.AnimatorController firstPersonAnimatorController = null;
#if FIRST_PERSON_CONTROLLER
                    var firstPersonBaseObjects = (target as UltimateCharacterLocomotion).GetComponentsInChildren<UltimateCharacterController.FirstPersonController.Character.Identifiers.FirstPersonBaseObject>();
                    if (firstPersonBaseObjects != null && firstPersonBaseObjects.Length > 0) {
                        var firstPersonBaseObject = firstPersonBaseObjects[0];
                        // Choose the base object with the lowest ID.
                        for (int i = 1; i < firstPersonBaseObjects.Length; ++i) {
                            if (firstPersonBaseObjects[i].ID < firstPersonBaseObject.ID) {
                                firstPersonBaseObject = firstPersonBaseObjects[i];
                            }
                        }
                        animator = firstPersonBaseObject.GetComponent<Animator>();
                        if (animator != null) {
                            firstPersonAnimatorController = (UnityEditor.Animations.AnimatorController)animator.runtimeAnimatorController;
                        }
                    }
#endif

                    GUILayout.BeginHorizontal();
                    GUI.enabled = animatorController != null;
                    GUILayout.Space(InspectorUtility.IndentWidth * 2);
                    if (GUILayout.Button("Generate Animator Code")) {
                        var baseDirectory = EditorPrefs.GetString(c_EditorPrefsLastLastAnimatorCodePathKey, System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this))));
                        var path = inspectorDrawer.GenerateAnimatorCode(ability, animatorController, firstPersonAnimatorController, baseDirectory);
                        if (!string.IsNullOrEmpty(path)) {
                            EditorPrefs.SetString(c_EditorPrefsLastLastAnimatorCodePathKey, System.IO.Path.GetFullPath(path.Replace(Application.dataPath, "Assets")));
                        }
                    }
                    GUI.enabled = GUI.enabled && inspectorDrawer.CanBuildAnimator;
                    if (GUILayout.Button("Build Animator")) { 
                        inspectorDrawer.BuildAnimator(animatorController, firstPersonAnimatorController);
                    }
                    GUI.enabled = true;
                    GUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                }
            }
        }

        /// <summary>
        /// Returns the state index key for the specified ability.
        /// </summary>
        private string GetSelectedAbilityStateIndexKey(Ability ability)
        {
            return c_EditorPrefsSelectedAbilityStateIndexKey + "." + target.GetType() + "." + target.name + "." + ability.GetType() + "." + ability.Index;
        }

        /// <summary>
        /// Draws all of the added states.
        /// </summary>
        private void OnAbilityStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (m_ReorderableAbilityStateList == null || m_CharacterLocomotion.Abilities == null || m_CharacterLocomotion.Abilities.Length <= EditorPrefs.GetInt(SelectedAbilityIndexKey)) {
                if (m_ReorderableAbilityStateList != null) {
                    EditorPrefs.SetInt(SelectedAbilityIndexKey, -1);
                }
                return;
            }

            EditorGUI.BeginChangeCheck();
            var ability = m_CharacterLocomotion.Abilities[EditorPrefs.GetInt(SelectedAbilityIndexKey)];

            // The index may be out of range if the component was copied.
            if (index >= m_CharacterLocomotion.Abilities[EditorPrefs.GetInt(SelectedAbilityIndexKey)].States.Length) {
                m_ReorderableAbilityStateList.index = -1;
                EditorPrefs.SetInt(GetSelectedAbilityStateIndexKey(ability), m_ReorderableAbilityStateList.index);
                return;
            }

            StateInspector.OnStateListDraw(ability, ability.States, rect, index);
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                SerializeAbilities();

                StateInspector.UpdateDefaultStateValues(ability.States);
            }
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnAbilityStateListAdd(ReorderableList list)
        {
            StateInspector.OnStateListAdd(AddExistingAbilityPreset, CreateAbilityPreset);
        }

        /// <summary>
        /// Adds a new element to the state list which uses an existing preset.
        /// </summary>
        private void AddExistingAbilityPreset()
        {
            var ability = m_CharacterLocomotion.Abilities[EditorPrefs.GetInt(SelectedAbilityIndexKey)];
            var states = StateInspector.AddExistingPreset(ability.GetType(), ability.States, m_ReorderableAbilityStateList, GetSelectedAbilityStateIndexKey(ability));
            if (ability.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableAbilityStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                ability.States = states;
                SerializeAbilities();
            }
        }

        /// <summary>
        /// Creates a new preset and adds it to a new state in the list.
        /// </summary>
        private void CreateAbilityPreset()
        {
            var ability = m_CharacterLocomotion.Abilities[EditorPrefs.GetInt(SelectedAbilityIndexKey)];
            var states = StateInspector.CreatePreset(ability, ability.States, m_ReorderableAbilityStateList, GetSelectedAbilityStateIndexKey(ability));
            if (ability.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableAbilityStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                ability.States = states;
                SerializeAbilities();
            }
        }

        /// <summary>
        /// The list has been reordered. Ensure the reorder is valid.
        /// </summary>
        private void OnAbilityStateListReorder(ReorderableList list)
        {
            var ability = m_CharacterLocomotion.Abilities[EditorPrefs.GetInt(SelectedAbilityIndexKey)];

            // Use the dummy array in order to determine what element the selected index was swapped with.
            var copiedStates = new UltimateCharacterController.StateSystem.State[ability.States.Length];
            Array.Copy(ability.States, copiedStates, ability.States.Length);
            for (int i = 0; i < ability.States.Length; ++i) {
                var element = list.serializedProperty.GetArrayElementAtIndex(i);
                if (element.intValue != i) {
                    ability.States[i] = copiedStates[element.intValue];
                    element.intValue = i;
                }
            }

            var states = StateInspector.OnStateListReorder(ability.States);
            if (ability.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableAbilityStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                ability.States = states;
                SerializeAbilities();
            }
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        private void OnAbilityStateListRemove(ReorderableList list)
        {
            var ability = m_CharacterLocomotion.Abilities[EditorPrefs.GetInt(SelectedAbilityIndexKey)];
            var states = StateInspector.OnStateListRemove(ability.States, GetSelectedAbilityStateIndexKey(ability), list);
            if (ability.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableAbilityStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                ability.States = states;
                SerializeAbilities();
            }
        }

        /// <summary>
        /// Draws the specified item ability.
        /// </summary>
        private void DrawSelectedItemAbility(int index)
        {
            var itemAbility = m_CharacterLocomotion.ItemAbilities[index];
            InspectorUtility.DrawObject(itemAbility, true, false, target, true, SerializeItemAbilities);

            var selectedItemAbility = itemAbility as ItemAbility;
            if (InspectorUtility.Foldout(itemAbility, new GUIContent("States"), false)) {
                // The ItemAbility class derives from system.object at the base level and reorderable lists can only operate on Unity objects. To get around this restriction
                // create a dummy array within a Unity object that corresponds to the number of elements within the item ability's state list. When the reorderable list is drawn
                // the item ability object will be used so it's like the dummy object never existed.
                var gameObject = new GameObject();
                var stateIndexHelper = gameObject.AddComponent<StateInspectorHelper>();
                stateIndexHelper.StateIndexData = new int[selectedItemAbility.States.Length];
                for (int i = 0; i < stateIndexHelper.StateIndexData.Length; ++i) {
                    stateIndexHelper.StateIndexData[i] = i;
                }
                var stateIndexSerializedObject = new SerializedObject(stateIndexHelper);
                m_ReorderableItemAbilityStateList = StateInspector.DrawStates(m_ReorderableItemAbilityStateList, serializedObject, stateIndexSerializedObject.FindProperty("m_StateIndexData"),
                                                            GetSelectedItemAbilityStateIndexKey(selectedItemAbility), OnItemAbilityStateListDraw, OnItemAbilityStateListAdd, 
                                                            OnItemAbilityStateListReorder, OnItemAbilityStateListRemove);
                DestroyImmediate(gameObject);
            }

            DrawEditorControls(selectedItemAbility);
        }

        /// <summary>
        /// Returns the state index key for the specified item ability.
        /// </summary>
        private string GetSelectedItemAbilityStateIndexKey(ItemAbility itemAbility)
        {
            return c_EditorPrefsSelectedItemAbilityStateIndexKey + "." + target.GetType() + "." + target.name + "." + itemAbility.GetType() + "." + itemAbility.Index;
        }

        /// <summary>
        /// Draws all of the added states.
        /// </summary>
        private void OnItemAbilityStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            var itemAbility = m_CharacterLocomotion.ItemAbilities[EditorPrefs.GetInt(SelectedItemAbilityIndexKey)];

            // The index may be out of range if the component was copied.
            if (index >= m_CharacterLocomotion.ItemAbilities[EditorPrefs.GetInt(SelectedItemAbilityIndexKey)].States.Length) {
                if (m_ReorderableItemAbilityStateList != null) {
                    m_ReorderableItemAbilityStateList.index = -1;
                    EditorPrefs.SetInt(GetSelectedAbilityStateIndexKey(itemAbility), m_ReorderableItemAbilityStateList.index);
                }
                return;
            }

            StateInspector.OnStateListDraw(itemAbility, itemAbility.States, rect, index);
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                SerializeItemAbilities();

                StateInspector.UpdateDefaultStateValues(itemAbility.States);
            }
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnItemAbilityStateListAdd(ReorderableList list)
        {
            StateInspector.OnStateListAdd(AddExistingItemAbilityPreset, CreateItemAbilityPreset);
        }

        /// <summary>
        /// Adds a new element to the state list which uses an existing preset.
        /// </summary>
        private void AddExistingItemAbilityPreset()
        {
            var itemAbility = m_CharacterLocomotion.ItemAbilities[EditorPrefs.GetInt(SelectedItemAbilityIndexKey)];
            var states = StateInspector.AddExistingPreset(itemAbility.GetType(), itemAbility.States, m_ReorderableItemAbilityStateList, GetSelectedItemAbilityStateIndexKey(itemAbility));
            if (itemAbility.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableItemAbilityStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                itemAbility.States = states;
                SerializeItemAbilities();
            }
        }

        /// <summary>
        /// Creates a new preset and adds it to a new state in the list.
        /// </summary>
        private void CreateItemAbilityPreset()
        {
            var itemAbility = m_CharacterLocomotion.ItemAbilities[EditorPrefs.GetInt(SelectedItemAbilityIndexKey)];
            var states = StateInspector.CreatePreset(itemAbility, itemAbility.States, m_ReorderableItemAbilityStateList, GetSelectedItemAbilityStateIndexKey(itemAbility));
            if (itemAbility.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableItemAbilityStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                itemAbility.States = states;
                SerializeItemAbilities();
            }
        }

        /// <summary>
        /// The list has been reordered. Ensure the reorder is valid.
        /// </summary>
        private void OnItemAbilityStateListReorder(ReorderableList list)
        {
            var itemAbility = m_CharacterLocomotion.ItemAbilities[EditorPrefs.GetInt(SelectedItemAbilityIndexKey)];

            // Use the dummy array in order to determine what element the selected index was swapped with.
            var copiedStates = new UltimateCharacterController.StateSystem.State[itemAbility.States.Length];
            Array.Copy(itemAbility.States, copiedStates, itemAbility.States.Length);
            for (int i = 0; i < itemAbility.States.Length; ++i) {
                var element = list.serializedProperty.GetArrayElementAtIndex(i);
                if (element.intValue != i) {
                    itemAbility.States[i] = copiedStates[element.intValue];
                    element.intValue = i;
                }
            }

            var states = StateInspector.OnStateListReorder(itemAbility.States);
            if (itemAbility.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableItemAbilityStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                itemAbility.States = states;
                SerializeItemAbilities();
            }
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        private void OnItemAbilityStateListRemove(ReorderableList list)
        {
            var itemAbility = m_CharacterLocomotion.ItemAbilities[EditorPrefs.GetInt(SelectedItemAbilityIndexKey)];
            var states = StateInspector.OnStateListRemove(itemAbility.States, GetSelectedItemAbilityStateIndexKey(itemAbility), list);
            if (itemAbility.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableItemAbilityStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                itemAbility.States = states;
                SerializeItemAbilities();
            }
        }

        /// <summary>
        /// Draws the specified effect.
        /// </summary>
        private void DrawSelectedEffect(int index)
        {
            var effect = m_CharacterLocomotion.Effects[index];
            InspectorUtility.DrawObject(effect, true, false, target, true, SerializeEffects);

            if (InspectorUtility.Foldout(effect, new GUIContent("States"), false)) {
                // The Effect class derives from system.object at the base level and reorderable lists can only operate on Unity objects. To get around this restriction
                // create a dummy array within a Unity object that corresponds to the number of elements within the ability's state list. When the reorderable list is drawn
                // the ability object will be used so it's like the dummy object never existed.
                var selectedEffect = effect as Effect;
                var gameObject = new GameObject();
                var stateIndexHelper = gameObject.AddComponent<StateInspectorHelper>();
                stateIndexHelper.StateIndexData = new int[selectedEffect.States.Length];
                for (int i = 0; i < stateIndexHelper.StateIndexData.Length; ++i) {
                    stateIndexHelper.StateIndexData[i] = i;
                }
                var stateIndexSerializedObject = new SerializedObject(stateIndexHelper);
                m_ReorderableEffectStateList = StateInspector.DrawStates(m_ReorderableEffectStateList, serializedObject, stateIndexSerializedObject.FindProperty("m_StateIndexData"),
                                                            GetSelectedEffectStateIndexKey(selectedEffect), OnEffectStateListDraw, OnEffectStateListAdd, OnEffectStateListReorder, 
                                                            OnEffectStateListRemove);
                DestroyImmediate(gameObject);
            }
        }

        /// <summary>
        /// Returns the state index key for the specified effect.
        /// </summary>
        private string GetSelectedEffectStateIndexKey(Effect effect)
        {
            return c_EditorPrefsSelectedEffectStateIndexKey + "." + target.GetType() + "." + target.name + "." + effect.GetType();
        }

        /// <summary>
        /// Draws all of the added states.
        /// </summary>
        private void OnEffectStateListDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();
            var effect = m_CharacterLocomotion.Effects[EditorPrefs.GetInt(SelectedEffectIndexKey)];

            // The index may be out of range if the component was copied.
            if (index >= m_CharacterLocomotion.Effects[EditorPrefs.GetInt(SelectedEffectIndexKey)].States.Length) {
                m_ReorderableEffectStateList.index = -1;
                EditorPrefs.SetInt(GetSelectedEffectStateIndexKey(effect), m_ReorderableEffectStateList.index);
                return;
            }

            StateInspector.OnStateListDraw(effect, effect.States, rect, index);
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                SerializeEffects();

                StateInspector.UpdateDefaultStateValues(effect.States);
            }
        }

        /// <summary>
        /// Adds a new state element to the list.
        /// </summary>
        private void OnEffectStateListAdd(ReorderableList list)
        {
            StateInspector.OnStateListAdd(AddExistingEffectPreset, CreateEffectPreset);
        }

        /// <summary>
        /// Adds a new element to the state list which uses an existing preset.
        /// </summary>
        private void AddExistingEffectPreset()
        {
            var effect = m_CharacterLocomotion.Effects[EditorPrefs.GetInt(SelectedEffectIndexKey)];
            var states = StateInspector.AddExistingPreset(effect.GetType(), effect.States, m_ReorderableEffectStateList, GetSelectedEffectStateIndexKey(effect));
            if (effect.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableEffectStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                effect.States = states;
                SerializeEffects();
            }
        }

        /// <summary>
        /// Creates a new preset and adds it to a new state in the list.
        /// </summary>
        private void CreateEffectPreset()
        {
            var effect = m_CharacterLocomotion.Effects[EditorPrefs.GetInt(SelectedEffectIndexKey)];
            var states = StateInspector.CreatePreset(effect, effect.States, m_ReorderableEffectStateList, GetSelectedEffectStateIndexKey(effect));
            if (effect.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableEffectStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                effect.States = states;
                SerializeEffects();
            }
        }

        /// <summary>
        /// The list has been reordered. Ensure the reorder is valid.
        /// </summary>
        private void OnEffectStateListReorder(ReorderableList list)
        {
            var effect = m_CharacterLocomotion.Effects[EditorPrefs.GetInt(SelectedEffectIndexKey)];

            // Use the dummy array in order to determine what element the selected index was swapped with.
            var copiedStates = new UltimateCharacterController.StateSystem.State[effect.States.Length];
            Array.Copy(effect.States, copiedStates, effect.States.Length);
            for (int i = 0; i < effect.States.Length; ++i) {
                var element = list.serializedProperty.GetArrayElementAtIndex(i);
                if (element.intValue != i) {
                    effect.States[i] = copiedStates[element.intValue];
                    element.intValue = i;
                }
            }

            var states = StateInspector.OnStateListReorder(effect.States);
            if (effect.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableEffectStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                effect.States = states;
                SerializeEffects();
            }
        }

        /// <summary>
        /// The ReordableList remove button has been pressed. Remove the selected state.
        /// </summary>
        private void OnEffectStateListRemove(ReorderableList list)
        {
            var effect = m_CharacterLocomotion.Effects[EditorPrefs.GetInt(SelectedEffectIndexKey)];
            var states = StateInspector.OnStateListRemove(effect.States, GetSelectedEffectStateIndexKey(effect), list);
            if (effect.States.Length != states.Length) {
                InspectorUtility.SynchronizePropertyCount(states, m_ReorderableEffectStateList.serializedProperty);
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                effect.States = states;
                SerializeEffects();
            }
        }
        
        /// <summary>
        /// Serialize all of the movement types to the MovementType array.
        /// </summary>
        private void SerializeMovementTypes()
        {
            var movementTypes = new List<MovementType>(m_CharacterLocomotion.MovementTypes);
            m_CharacterLocomotion.MovementTypeData = Shared.Utility.Serialization.Serialize<MovementType>(movementTypes);
            m_CharacterLocomotion.MovementTypes = movementTypes.ToArray();
            UpdateDefaultMovementTypes();
            InspectorUtility.SetDirty(target);
        }

        /// <summary>
        /// Updates the default first/third movement type based on the movement types availabe on the character controller.
        /// </summary>
        private void UpdateDefaultMovementTypes()
        {
            // The movement type may not exist anymore.
            if (UnityEngineUtility.GetType(m_CharacterLocomotion.FirstPersonMovementTypeFullName) == null) {
                m_CharacterLocomotion.FirstPersonMovementTypeFullName = string.Empty;
                InspectorUtility.SetDirty(target);
            }
            if (UnityEngineUtility.GetType(m_CharacterLocomotion.ThirdPersonMovementTypeFullName) == null) {
                m_CharacterLocomotion.ThirdPersonMovementTypeFullName = string.Empty;
                InspectorUtility.SetDirty(target);
            }

            var hasSelectedMovementType = false;
            var firstPersonMovementTypes = new List<Type>();
            var thirdPersonMovementTypes = new List<Type>();
            var firstPersonMovementTypeNames = new List<string>();
            var thirdPersonMovementTypeNames = new List<string>();
            var movementTypes = m_CharacterLocomotion.MovementTypes;
            if (movementTypes != null) {
                for (int i = 0; i < movementTypes.Length; ++i) {
                    if (movementTypes[i] == null) {
                        continue;
                    }
                    if (movementTypes[i].GetType().FullName.Contains("FirstPerson")) {
                        // Use the movement type if the type is currently empty.
                        if (string.IsNullOrEmpty(m_CharacterLocomotion.FirstPersonMovementTypeFullName)) {
                            m_CharacterLocomotion.FirstPersonMovementTypeFullName = movementTypes[i].GetType().FullName;
                        }
                        firstPersonMovementTypes.Add(movementTypes[i].GetType());
                        firstPersonMovementTypeNames.Add(InspectorUtility.DisplayTypeName(movementTypes[i].GetType(), false));
                    } else { // Third Person.
                        // Use the movement type if the type is currently empty.
                        if (string.IsNullOrEmpty(m_CharacterLocomotion.ThirdPersonMovementTypeFullName)) {
                            m_CharacterLocomotion.ThirdPersonMovementTypeFullName = movementTypes[i].GetType().FullName;
                        }
                        thirdPersonMovementTypes.Add(movementTypes[i].GetType());
                        thirdPersonMovementTypeNames.Add(InspectorUtility.DisplayTypeName(movementTypes[i].GetType(), false));
                    }

                    if (m_CharacterLocomotion.MovementTypeFullName == movementTypes[i].GetType().FullName) {
                        hasSelectedMovementType = true;
                    }
                }
            }
            m_FirstPersonMovementTypes = firstPersonMovementTypes.ToArray();
            m_ThirdPersonMovementTypes = thirdPersonMovementTypes.ToArray();
            m_FirstPersonMovementTypeNames = firstPersonMovementTypeNames.ToArray();
            m_ThirdPersonMovementTypeNames = thirdPersonMovementTypeNames.ToArray();

            // If the selected MovementType no longer exists in the list then select the first movement type.
            if (!hasSelectedMovementType) {
                m_CharacterLocomotion.MovementTypeFullName = string.Empty;
                if (movementTypes != null && movementTypes.Length > 0) {
                    m_CharacterLocomotion.MovementTypeFullName = movementTypes[0].GetType().FullName;
                }
            }
        }

        /// <summary>
        /// Serialize all of the abilities to the AbilityData array.
        /// </summary>
        private void SerializeAbilities()
        {
            AbilityBuilder.SerializeAbilities(m_CharacterLocomotion);
            InspectorUtility.SetDirty(target);
        }

        /// <summary>
        /// Serialize all of the item abilities to the ItemAbilityData array.
        /// </summary>
        private void SerializeItemAbilities()
        {
            AbilityBuilder.SerializeItemAbilities(m_CharacterLocomotion);
            InspectorUtility.SetDirty(target); 
        }

        /// <summary>
        /// Serialize all of the effects to the EffectData array.
        /// </summary>
        private void SerializeEffects()
        {
            var effects = new List<Effect>(m_CharacterLocomotion.Effects);
            m_CharacterLocomotion.EffectData = Shared.Utility.Serialization.Serialize<Effect>(effects);
            m_CharacterLocomotion.Effects = effects.ToArray();
            InspectorUtility.SetDirty(target);
        }

        /// <summary>
        /// Returns the maximum collider radius of the colliders within the character.
        /// </summary>
        /// <returns>The maximum collider radius of the colliders within the character.</returns>
        private float GetMaxColliderRadius()
        {
            float maxColliderRadius = 0;
            var layer = (target as UltimateCharacterLocomotion).gameObject.layer;
            var colliders = (target as UltimateCharacterLocomotion).GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; ++i) {
                if (colliders[i].isTrigger || colliders[i].gameObject.layer != layer) {
                    continue;
                }
                var radius = 0f;
                if (colliders[i] is CapsuleCollider) {
                    radius = (colliders[i] as CapsuleCollider).radius;
                } else if (colliders[i] is SphereCollider) {
                    radius = (colliders[i] as SphereCollider).radius;
                }
                if (radius > maxColliderRadius) {
                    maxColliderRadius = radius;
                }
            }
            return maxColliderRadius;
        }

        /// <summary>
        /// Deserialize the abilities and effects after an undo/redo.
        /// </summary>
        private void OnUndoRedo()
        {
            m_CharacterLocomotion.DeserializeMovementTypes(true);
            m_CharacterLocomotion.DeserializeAbilities(true);
            m_CharacterLocomotion.DeserializeEffects(true);
            if (m_ReorderableMovementTypeList != null) {
                m_ReorderableMovementTypeList.displayRemove = m_CharacterLocomotion.MovementTypes.Length > 1;
            }
            Repaint();
        }
    }
}