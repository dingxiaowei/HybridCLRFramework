/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.FirstPersonController.Items
{
    using Opsive.UltimateCharacterController.Editor.Inspectors.Items;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.FirstPersonController.Items;
    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Shows a custom inspector for the FirstPersonPerspectiveItem.
    /// </summary>
    [CustomEditor(typeof(FirstPersonPerspectiveItem))]
    public class FirstPersonPerspectiveItemInspector : PerspectiveItemInspector
    {
        /// <summary>
        /// Returns the actions to draw before the State list is drawn.
        /// </summary>
        /// <returns>The actions to draw before the State list is drawn.</returns>
        protected override Action GetDrawCallback()
        {
            var baseCallback = base.GetDrawCallback();

            baseCallback += () =>
            {
                DrawMotionProperties();
            };

            return baseCallback;
        }

        /// <summary>
        /// Draws the options for spawning based on a parent.
        /// </summary>
        protected override void DrawSpawnParentProperties()
        {
            EditorGUILayout.PropertyField(PropertyFromName("m_FirstPersonBaseObjectID"));
        }

        /// <summary>
        /// Draws the options for the render foldout.
        /// </summary>
        protected override void DrawRenderProperties()
        {
            EditorGUILayout.PropertyField(PropertyFromName("m_VisibleItem"));
            var firstPersonObject = PropertyFromName("m_Object").objectReferenceValue as GameObject;
            if (firstPersonObject == null) {
                EditorGUILayout.PropertyField(PropertyFromName("m_LocalSpawnPosition"));
                EditorGUILayout.PropertyField(PropertyFromName("m_LocalSpawnRotation"));
            } else {
                // The first person object must have a FirstPersonObjectBase component.
                if (firstPersonObject.GetComponent<UltimateCharacterController.FirstPersonController.Character.Identifiers.FirstPersonBaseObject>() == null &&
                    firstPersonObject.GetComponentInParent<UltimateCharacterController.Items.ItemSlot>() == null) {
                    EditorGUILayout.HelpBox("The incorrect object is assigned. The Object must have a FirstPersonBaseObject component attached or be a child of an ItemSlot.", MessageType.Error);
                }
            }
#if ULTIMATE_CHARACTER_CONTROLLER_VR
            EditorGUILayout.PropertyField(PropertyFromName("m_VRHandParent"));
#endif
            if (firstPersonObject == null || firstPersonObject.transform.IsChildOf((target as FirstPersonPerspectiveItem).transform)) {
                EditorGUILayout.PropertyField(PropertyFromName("m_AdditionalControlObjectBaseIDs"), true);
            } else {
                EditorGUILayout.PropertyField(PropertyFromName("m_AdditionalControlObjects"), true);
            }
        }

        /// <summary>
        /// Draws the VisibleItem motion properties.
        /// </summary>
        protected virtual void DrawMotionProperties()
        {
            if (Foldout("Position Spring")) {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(PropertyFromName("m_PositionOffset"));
                GUI.enabled = (target as FirstPersonPerspectiveItem).Object != null;
                if (GUILayout.Button(InspectorStyles.UpdateIcon, InspectorStyles.NoPaddingButtonStyle, GUILayout.Width(18))) {
                    PropertyFromName("m_PositionOffset").vector3Value = (target as FirstPersonPerspectiveItem).Object.transform.localPosition;
                    GUI.changed = true;
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                if (Application.isPlaying && GUI.changed) {
                    (target as FirstPersonPerspectiveItem).PositionOffset = PropertyFromName("m_PositionOffset").vector3Value;
                }
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(PropertyFromName("m_PositionExitOffset"));
                GUI.enabled = (target as FirstPersonPerspectiveItem).Object != null;
                if (GUILayout.Button(InspectorStyles.UpdateIcon, InspectorStyles.NoPaddingButtonStyle, GUILayout.Width(18))) {
                    PropertyFromName("m_PositionExitOffset").vector3Value = (target as FirstPersonPerspectiveItem).Object.transform.localPosition;
                    GUI.changed = true;
                }
                GUI.enabled = true;
                if (Application.isPlaying && GUI.changed) {
                    (target as FirstPersonPerspectiveItem).PositionExitOffset = PropertyFromName("m_PositionExitOffset").vector3Value;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Slider(PropertyFromName("m_PositionFallImpact"), 0, 1);
                EditorGUILayout.IntSlider(PropertyFromName("m_PositionFallImpactSoftness"), 0, 30);
                EditorGUILayout.Slider(PropertyFromName("m_PositionFallRetract"), 0, 10);
                EditorGUILayout.PropertyField(PropertyFromName("m_PositionMoveSlide"));
                EditorGUILayout.PropertyField(PropertyFromName("m_PositionPlatformSlide"));
                EditorGUILayout.Slider(PropertyFromName("m_PositionInputVelocityScale"), 0, 10);
                EditorGUILayout.PropertyField(PropertyFromName("m_PositionMaxInputVelocity"));
                InspectorUtility.DrawSpring(target, "Spring", "m_PositionSpring");
                EditorGUI.indentLevel--;
            }
            if (Foldout("Rotation Spring")) {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(PropertyFromName("m_RotationOffset"));
                GUI.enabled = (target as FirstPersonPerspectiveItem).Object != null;
                if (GUILayout.Button(InspectorStyles.UpdateIcon, InspectorStyles.NoPaddingButtonStyle, GUILayout.Width(18))) {
                    PropertyFromName("m_RotationOffset").vector3Value = (target as FirstPersonPerspectiveItem).Object.transform.localEulerAngles;
                    GUI.changed = true;
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                if (Application.isPlaying && GUI.changed) {
                    (target as FirstPersonPerspectiveItem).RotationOffset = PropertyFromName("m_RotationOffset").vector3Value;
                }
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(PropertyFromName("m_RotationExitOffset"));
                GUI.enabled = (target as FirstPersonPerspectiveItem).Object != null;
                if (GUILayout.Button(InspectorStyles.UpdateIcon, InspectorStyles.NoPaddingButtonStyle, GUILayout.Width(18))) {
                    PropertyFromName("m_RotationExitOffset").vector3Value = (target as FirstPersonPerspectiveItem).Object.transform.localEulerAngles;
                    GUI.changed = true;
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                if (Application.isPlaying && GUI.changed) {
                    (target as FirstPersonPerspectiveItem).RotationExitOffset = PropertyFromName("m_RotationExitOffset").vector3Value;
                }
                EditorGUILayout.Slider(PropertyFromName("m_RotationFallImpact"), 0, 100);
                EditorGUILayout.IntSlider(PropertyFromName("m_RotationFallImpactSoftness"), 0, 30);
                EditorGUILayout.PropertyField(PropertyFromName("m_RotationLookSway"));
                EditorGUILayout.PropertyField(PropertyFromName("m_RotationStrafeSway"));
                EditorGUILayout.PropertyField(PropertyFromName("m_RotationVerticalSway"));
                EditorGUILayout.PropertyField(PropertyFromName("m_RotationPlatformSway"));
                EditorGUILayout.Slider(PropertyFromName("m_RotationGroundSwayMultiplier"), 0, 1);
                EditorGUILayout.Slider(PropertyFromName("m_RotationInputVelocityScale"), 0, 10);
                EditorGUILayout.PropertyField(PropertyFromName("m_RotationMaxInputVelocity"));
                InspectorUtility.DrawSpring(target, "Spring", "m_RotationSpring");
                EditorGUI.indentLevel--;
            }
            if (Foldout("Pivot Position Spring")) {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(PropertyFromName("m_PivotPositionOffset"));
                GUI.enabled = (target as FirstPersonPerspectiveItem).PivotTransform != null;
                if (GUILayout.Button(InspectorStyles.UpdateIcon, InspectorStyles.NoPaddingButtonStyle, GUILayout.Width(18))) {
                    PropertyFromName("m_PivotPositionOffset").vector3Value = (target as FirstPersonPerspectiveItem).PivotTransform.localPosition;
                    GUI.changed = true;
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                if (Application.isPlaying && GUI.changed) {
                    (target as FirstPersonPerspectiveItem).PivotPositionOffset = PropertyFromName("m_PivotPositionOffset").vector3Value;
                }
                InspectorUtility.DrawSpring(target, "Spring", "m_PivotPositionSpring");
                EditorGUI.indentLevel--;
            }
            if (Foldout("Pivot Rotation Spring")) {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(PropertyFromName("m_PivotRotationOffset"));
                GUI.enabled = (target as FirstPersonPerspectiveItem).PivotTransform != null;
                if (GUILayout.Button(InspectorStyles.UpdateIcon, InspectorStyles.NoPaddingButtonStyle, GUILayout.Width(18))) {
                    PropertyFromName("m_PivotRotationOffset").vector3Value = (target as FirstPersonPerspectiveItem).PivotTransform.localEulerAngles;
                    GUI.changed = true;
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                if (Application.isPlaying && GUI.changed) {
                    (target as FirstPersonPerspectiveItem).PivotRotationOffset = PropertyFromName("m_PivotRotationOffset").vector3Value;
                }
                InspectorUtility.DrawSpring(target, "Spring", "m_PivotRotationSpring");
                EditorGUI.indentLevel--;
            }
            if (Foldout("Shake")) {
                EditorGUI.indentLevel++;
                EditorGUILayout.Slider(PropertyFromName("m_ShakeSpeed"), 0, 10);
                EditorGUILayout.PropertyField(PropertyFromName("m_ShakeAmplitude"));
                EditorGUI.indentLevel--;
            }
            if (Foldout("Bob")) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName("m_BobPositionalRate"));
                EditorGUILayout.PropertyField(PropertyFromName("m_BobPositionalAmplitude"));
                EditorGUILayout.PropertyField(PropertyFromName("m_BobRotationalRate"));
                EditorGUILayout.PropertyField(PropertyFromName("m_BobRotationalAmplitude"));
                EditorGUILayout.Slider(PropertyFromName("m_BobInputVelocityScale"), 0, 10);
                EditorGUILayout.PropertyField(PropertyFromName("m_BobMaxInputVelocity"));
                EditorGUILayout.PropertyField(PropertyFromName("m_BobRequireGroundContact"));
                EditorGUI.indentLevel--;
            }
            if (Foldout("Step")) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName("m_StepMinVelocity"));
                EditorGUILayout.IntSlider(PropertyFromName("m_StepSoftness"), 0, 30);
                EditorGUILayout.PropertyField(PropertyFromName("m_StepPositionForce"));
                EditorGUILayout.PropertyField(PropertyFromName("m_StepRotationForce"));
                EditorGUILayout.Slider(PropertyFromName("m_StepForceScale"), 0, 1);
                EditorGUILayout.Slider(PropertyFromName("m_StepPositionBalance"), -1, 1);
                EditorGUILayout.Slider(PropertyFromName("m_StepRotationBalance"), -1, 1);
                EditorGUI.indentLevel--;
            }
        }
    }
}