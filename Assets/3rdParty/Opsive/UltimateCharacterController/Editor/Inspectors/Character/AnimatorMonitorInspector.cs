/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors
{
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using UnityEditor;

    /// <summary>
    /// Shows a custom inspector for the AnimatorMonitor component.
    /// </summary>
    [CustomEditor(typeof(AnimatorMonitor), true)]
    public class AnimatorMonitorInspector : InspectorBase
    {
        /// <summary>
        /// Draws the inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            if (Foldout("Time")) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName("m_HorizontalMovementDampingTime"));
                EditorGUILayout.PropertyField(PropertyFromName("m_ForwardMovementDampingTime"));
                EditorGUILayout.PropertyField(PropertyFromName("m_PitchDampingTime"));
                EditorGUILayout.PropertyField(PropertyFromName("m_YawDampingTime"));
                EditorGUI.indentLevel--;
            }
            if (Foldout("Editor")) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName("m_LogAbilityParameterChanges"));
                EditorGUILayout.PropertyField(PropertyFromName("m_LogItemParameterChanges"));
                EditorGUILayout.PropertyField(PropertyFromName("m_LogEvents"));
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Value Change");
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}