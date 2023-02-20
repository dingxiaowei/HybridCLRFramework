/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character.Abilities
{
    /// <summary>
    /// Draws a custom inspector for the RestrictPosition Ability.
    /// </summary>
    [InspectorDrawer(typeof(RestrictPosition))]
    public class RestrictPositionInspectorDrawer : AbilityInspectorDrawer
    {
        /// <summary>
        /// Draws the fields related to the inspector drawer.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        protected override void DrawInspectorDrawerFields(object target, Object parent)
        {
            var restriction = (RestrictPosition.RestrictionType)EditorGUILayout.EnumPopup(new GUIContent("Restriction", InspectorUtility.GetFieldTooltip(target, "m_Restriction")), 
                                                                        InspectorUtility.GetFieldValue<RestrictPosition.RestrictionType>(target, "m_Restriction"));
            InspectorUtility.SetFieldValue(target, "m_Restriction", restriction);

            // Draw the x restriction.
            if (restriction != RestrictPosition.RestrictionType.RestrictZ) {
                EditorGUI.indentLevel++;
                var minValue = InspectorUtility.GetFieldValue<float>(target, "m_MinXPosition");
                var maxValue = InspectorUtility.GetFieldValue<float>(target, "m_MaxXPosition");
                minValue = EditorGUILayout.FloatField(new GUIContent("Min X Position", InspectorUtility.GetFieldTooltip(target, "m_MinXPosition")), minValue);
                if (minValue > maxValue) {
                    maxValue = minValue;
                }
                maxValue = EditorGUILayout.FloatField(new GUIContent("Max X Position", InspectorUtility.GetFieldTooltip(target, "m_MaxXPosition")), maxValue);
                if (maxValue < minValue) {
                    minValue = maxValue;
                }
                InspectorUtility.SetFieldValue(target, "m_MinXPosition", minValue);
                InspectorUtility.SetFieldValue(target, "m_MaxXPosition", maxValue);
                EditorGUI.indentLevel--;
            }

            // Draw the z restriction.
            if (restriction != RestrictPosition.RestrictionType.RestrictX) {
                EditorGUI.indentLevel++;
                var minValue = InspectorUtility.GetFieldValue<float>(target, "m_MinZPosition");
                var maxValue = InspectorUtility.GetFieldValue<float>(target, "m_MaxZPosition");
                minValue = EditorGUILayout.FloatField(new GUIContent("Min Z Position", InspectorUtility.GetFieldTooltip(target, "m_MinZPosition")), minValue);
                if (minValue > maxValue) {
                    maxValue = minValue;
                }
                maxValue = EditorGUILayout.FloatField(new GUIContent("Max Z Position", InspectorUtility.GetFieldTooltip(target, "m_MaxZPosition")), maxValue);
                if (maxValue < minValue) {
                    minValue = maxValue;
                }
                InspectorUtility.SetFieldValue(target, "m_MinZPosition", minValue);
                InspectorUtility.SetFieldValue(target, "m_MaxZPosition", maxValue);
                EditorGUI.indentLevel--;
            }

            var stopAnimation = EditorGUILayout.Toggle(new GUIContent("Stop Animation", InspectorUtility.GetFieldTooltip(target, "m_StopAnimation")),
                                                        InspectorUtility.GetFieldValue<bool>(target, "m_StopAnimation"));
            InspectorUtility.SetFieldValue(target, "m_StopAnimation", stopAnimation);
            if (stopAnimation) {
                EditorGUI.indentLevel++;
                InspectorUtility.DrawField(target, "m_StopAnimationBuffer");
                EditorGUI.indentLevel--;
            }

            base.DrawInspectorDrawerFields(target, parent);
        }
    }
}