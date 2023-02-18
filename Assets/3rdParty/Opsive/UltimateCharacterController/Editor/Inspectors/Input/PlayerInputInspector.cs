/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Input
{
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem;
    using Opsive.UltimateCharacterController.Input;
    using System;
    using UnityEditor;

    /// <summary>
    /// Shows a custom inspector for the PlayerInput.
    /// </summary>
    [CustomEditor(typeof(PlayerInput))]
    public class PlayerInputInspector : StateBehaviorInspector
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
                EditorGUILayout.PropertyField(PropertyFromName("m_HorizontalLookInputName"));
                EditorGUILayout.PropertyField(PropertyFromName("m_VerticalLookInputName"));
                var lookVector = PropertyFromName("m_LookVectorMode");
                EditorGUILayout.PropertyField(lookVector);
                if (lookVector.enumValueIndex == (int)PlayerInput.LookVectorMode.Smoothed) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_LookSensitivity"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_LookSensitivityMultiplier"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_SmoothLookSteps"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_SmoothLookWeight"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_SmoothExponent"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_LookAccelerationThreshold"));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(PropertyFromName("m_ControllerConnectedCheckRate"));
                EditorGUILayout.PropertyField(PropertyFromName("m_ConnectedControllerState"));

                DrawInputFields();

                // Event fields should be last.
                if (Foldout("Events")) {
                    EditorGUI.indentLevel++;
                    InspectorUtility.UnityEventPropertyField(PropertyFromName("m_EnableGamplayInputEvent"));
                    EditorGUI.indentLevel--;
                }
            };

            return baseCallback;
        }

        /// <summary>
        /// Draws all of the fields related to input.
        /// </summary>
        protected virtual void DrawInputFields() { }
    }
}