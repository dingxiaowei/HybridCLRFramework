/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes;
using Opsive.UltimateCharacterController.Editor.Inspectors.Camera;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.ThirdPersonController.Camera.ViewTypes
{
    /// <summary>
    /// Draws a custom inspector for the Top Down View Type.
    /// </summary>
    [InspectorDrawer(typeof(TopDown))]
    public class TopDownInspectorDrawer : ViewTypeInspectorDrawer
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
            InspectorUtility.DrawField(target, "m_UpAxis");
            InspectorUtility.DrawField(target, "m_RotationSpeed");
            InspectorUtility.DrawField(target, "m_CollisionRadius");
            InspectorUtility.DrawField(target, "m_ViewDistance");
            InspectorUtility.DrawField(target, "m_ViewStep");
            InspectorUtility.DrawField(target, "m_VerticalLookDirection");

            var minPitchLimit = InspectorUtility.GetFieldValue<float>(target, "m_MinPitchLimit");
            var maxPitchLimit = InspectorUtility.GetFieldValue<float>(target, "m_MaxPitchLimit");
            var minValue = Mathf.Round(minPitchLimit * 100f) / 100f;
            var maxValue = Mathf.Round(maxPitchLimit * 100f) / 100f;
            InspectorUtility.MinMaxSlider(ref minValue, ref maxValue, 0, 89.99f, new GUIContent("Pitch Limit", "The min and max limit of the pitch angle (in degrees)."));
            if (minValue != minPitchLimit) {
                InspectorUtility.SetFieldValue(target, "m_MinPitchLimit", minValue);
            }
            if (minValue != maxPitchLimit) {
                InspectorUtility.SetFieldValue(target, "m_MaxPitchLimit", maxValue);
            }
        }
    }
}