/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes;
using Opsive.UltimateCharacterController.Editor.Inspectors.Camera;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.ThirdPersonController.Camera.ViewTypes
{
    /// <summary>
    /// Draws a custom inspector for the Third Person View Type.
    /// </summary>
    [InspectorDrawer(typeof(ThirdPerson))]
    public class ThirdPersonInspectorDrawer : ViewTypeInspectorDrawer
    {
        /// <summary>
        /// Called when the object should be drawn to the inspector.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        public override void OnInspectorGUI(object target, Object parent)
        {
            InspectorUtility.DrawField(target, "m_LookDirectionDistance");
            InspectorUtility.DrawField(target, "m_ForwardAxis");
            InspectorUtility.DrawField(target, "m_LookOffset");
            InspectorUtility.DrawField(target, "m_LookOffsetSmoothing");
            InspectorUtility.DrawField(target, "m_PositionSmoothing");
            InspectorUtility.DrawField(target, "m_ObstructionPositionSmoothing");
            InspectorUtility.DrawFieldSlider(target, "m_FieldOfView", 1, 179);
            InspectorUtility.DrawFieldSlider(target, "m_FieldOfViewDamping", 0, 5);
            InspectorUtility.DrawField(target, "m_CollisionRadius");
            InspectorUtility.DrawField(target, "m_CollisionAnchorOffset");
            if (InspectorUtility.Foldout(target, "Primary Spring")) {
                EditorGUI.indentLevel++;
                InspectorUtility.DrawSpring(target, "Position Spring", "m_PositionSpring");
                InspectorUtility.DrawSpring(target, "Rotation Spring", "m_RotationSpring");
                EditorGUI.indentLevel--;
            }

            if (InspectorUtility.Foldout(target, "Secondary Spring")) {
                EditorGUI.indentLevel++;
                InspectorUtility.DrawSpring(target, "Position Spring", "m_SecondaryPositionSpring");
                InspectorUtility.DrawSpring(target, "Rotation Spring", "m_SecondaryRotationSpring");
                EditorGUI.indentLevel--;
            }

            if (InspectorUtility.Foldout(target, "Step Zoom")) {
                EditorGUI.indentLevel++;
                InspectorUtility.DrawField(target, "m_StepZoomInputName");
                InspectorUtility.DrawField(target, "m_StepZoomSensitivity");
                InspectorUtility.DrawField(target, "m_MinStepZoom");
                InspectorUtility.DrawField(target, "m_MaxStepZoom");
                EditorGUI.indentLevel--;
            }

            if (InspectorUtility.Foldout(target, "Limits")) {
                EditorGUI.indentLevel++;
                OnDrawLimits(target);
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// Callback which draws the limits for the view type.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        protected virtual void OnDrawLimits(object target)
        {
            var minPitchLimit = InspectorUtility.GetFieldValue<float>(target, "m_MinPitchLimit");
            var maxPitchLimit = InspectorUtility.GetFieldValue<float>(target, "m_MaxPitchLimit");
            var minValue = Mathf.Round(minPitchLimit * 100f) / 100f;
            var maxValue = Mathf.Round(maxPitchLimit * 100f) / 100f;
            InspectorUtility.MinMaxSlider(ref minValue, ref maxValue, -90, 90, new GUIContent("Pitch Limit", "The min and max limit of the pitch angle (in degrees)."));
            if (minValue != minPitchLimit) {
                InspectorUtility.SetFieldValue(target, "m_MinPitchLimit", minValue);
            }
            if (minValue != maxPitchLimit) {
                InspectorUtility.SetFieldValue(target, "m_MaxPitchLimit", maxValue);
            }
        }
    }
}