/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Character.Effects;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character.Effects
{
    /// <summary>
    /// Draws a custom inspector for the Shake effect.
    /// </summary>
    [InspectorDrawer(typeof(Shake))]
    public class ShakenspectorDrawer : EffectInspectorDrawer
    {
        /// <summary>
        /// Draws the fields related to the inspector drawer.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        protected override void DrawInspectorDrawerFields(object target, Object parent)
        {
            ObjectInspector.DrawFields(target, true);
            var shakeTarget = (Shake.ShakeTarget)EditorGUILayout.EnumFlagsField(new GUIContent("Shake Target", InspectorUtility.GetFieldTooltip(target, "m_Target")),
                                                                        InspectorUtility.GetFieldValue<Shake.ShakeTarget>(target, "m_Target"));
            InspectorUtility.SetFieldValue(target, "m_Target", shakeTarget);
        }
    }
}