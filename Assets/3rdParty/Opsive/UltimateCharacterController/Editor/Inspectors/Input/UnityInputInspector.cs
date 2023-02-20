/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEditor;
using Opsive.UltimateCharacterController.Input;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Input
{
    /// <summary>
    /// Shows a custom inspector for the UnityInput.
    /// </summary>
    [CustomEditor(typeof(UnityInput))]
    public class UnityInputInspector : PlayerInputInspector
    {
        /// <summary>
        /// Draws all of the fields related to input.
        /// </summary>
        protected override void DrawInputFields()
        {
            EditorGUILayout.PropertyField(PropertyFromName("m_ForceInput"));
            EditorGUILayout.PropertyField(PropertyFromName("m_DisableCursor"));
            var disableCursorProperty = PropertyFromName("m_EnableCursorWithEscape");
            EditorGUILayout.PropertyField(disableCursorProperty);
            if (disableCursorProperty.boolValue) {
                EditorGUILayout.PropertyField(PropertyFromName("m_PreventLookVectorChanges"));
            }
            EditorGUILayout.PropertyField(PropertyFromName("m_JoystickUpThreshold"));
        }
    }
}