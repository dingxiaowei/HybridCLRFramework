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
    /// Draws a custom inspector for the DetectObjectAbilityBase ability.
    /// </summary>
    [InspectorDrawer(typeof(DetectObjectAbilityBase))]
    public class DetectObjectAbilityBaseInspectorDrawer : AbilityInspectorDrawer
    {
        /// <summary>
        /// Draws the fields related to the inspector drawer.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        protected override void DrawInspectorDrawerFields(object target, Object parent)
        {
            // Draw ObjectDetectionMode manually so it'll use the MaskField.
            var objectDetection = (int)InspectorUtility.GetFieldValue<DetectObjectAbilityBase.ObjectDetectionMode>(target, "m_ObjectDetection");
            var objectDetectionString = System.Enum.GetNames(typeof(DetectObjectAbilityBase.ObjectDetectionMode));
            var value = EditorGUILayout.MaskField(new GUIContent("Object Detection", InspectorUtility.GetFieldTooltip(target, "m_ObjectDetection")), objectDetection, objectDetectionString);
            if (value != objectDetection) {
                InspectorUtility.SetFieldValue(target, "m_ObjectDetection", value);
            }
            // The ability may not use any detection.
            if (value != 0) {
                EditorGUI.indentLevel++;
                InspectorUtility.DrawField(target, "m_DetectLayers");
                InspectorUtility.DrawField(target, "m_UseLookPosition");
                InspectorUtility.DrawField(target, "m_UseLookDirection");
                InspectorUtility.DrawField(target, "m_AngleThreshold");
                InspectorUtility.DrawField(target, "m_ObjectID");

                var objectDetectionEnumValue = (DetectObjectAbilityBase.ObjectDetectionMode)value;
                if (objectDetectionEnumValue != DetectObjectAbilityBase.ObjectDetectionMode.Trigger) {
                    InspectorUtility.DrawField(target, "m_CastDistance");
                    InspectorUtility.DrawField(target, "m_CastFrameInterval");
                    InspectorUtility.DrawField(target, "m_CastOffset");
                    if ((objectDetectionEnumValue & DetectObjectAbilityBase.ObjectDetectionMode.Spherecast) != 0) {
                        InspectorUtility.DrawField(target, "m_SpherecastRadius");
                    }
                } else {
                    InspectorUtility.DrawField(target, "m_MaxTriggerObjectCount");
                }

                EditorGUI.indentLevel--;
            }

            base.DrawInspectorDrawerFields(target, parent);
        }
    }
}