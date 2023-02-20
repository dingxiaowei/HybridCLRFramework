/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.ThirdPersonController.Camera.ViewTypes
{
    /// <summary>
    /// Draws a custom inspector for the RPG View Type.
    /// </summary>
    [InspectorDrawer(typeof(RPG))]
    public class RPGInspectorDrawer : ThirdPersonInspectorDrawer
    {
        /// <summary>
        /// Called when the object should be drawn to the inspector.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        public override void OnInspectorGUI(object target, Object parent)
        {
            base.OnInspectorGUI(target, parent);

            if (InspectorUtility.Foldout(target, "RPG")) {
                EditorGUI.indentLevel++;
                InspectorUtility.DrawField(target, "m_YawSnapDamping");
                InspectorUtility.DrawField(target, "m_AllowFreeMovement");
                InspectorUtility.DrawField(target, "m_CameraFreeMovementInputName");
                EditorGUI.indentLevel--;
            }
        }
    }
}