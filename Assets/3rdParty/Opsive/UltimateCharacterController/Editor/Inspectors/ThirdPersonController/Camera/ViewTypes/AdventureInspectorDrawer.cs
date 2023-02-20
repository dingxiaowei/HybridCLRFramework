/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.ThirdPersonController.Camera.ViewTypes
{
    /// <summary>
    /// Draws a custom inspector for the Adventure View Type.
    /// </summary>
    [InspectorDrawer(typeof(Adventure))]
    public class AdventureInspectorDrawer : ThirdPersonInspectorDrawer
    {
        /// <summary>
        /// Callback which draws the limits for the view type.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        protected override void OnDrawLimits(object target)
        {
            base.OnDrawLimits(target);

            var minYawLimit = InspectorUtility.GetFieldValue<float>(target, "m_MinYawLimit");
            var maxYawLimit = InspectorUtility.GetFieldValue<float>(target, "m_MaxYawLimit");
            var minValue = Mathf.Round(minYawLimit * 100f) / 100f;
            var maxValue = Mathf.Round(maxYawLimit * 100f) / 100f;
            InspectorUtility.MinMaxSlider(ref minValue, ref maxValue, -180, 180, new GUIContent("Yaw Limit", "The min and max limit of the yaw angle (in degrees)."));
            if (minValue != minYawLimit) {
                InspectorUtility.SetFieldValue(target, "m_MinYawLimit", minValue);
            }
            if (minValue != maxYawLimit) {
                InspectorUtility.SetFieldValue(target, "m_MaxYawLimit", maxValue);
            }
            InspectorUtility.DrawField(target, "m_YawLimitLerpSpeed");
        }
    }
}