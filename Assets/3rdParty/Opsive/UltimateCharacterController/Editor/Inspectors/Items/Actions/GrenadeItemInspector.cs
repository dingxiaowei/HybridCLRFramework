/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Items.Actions
{
    /// <summary>
    /// Shows a custom inspector for the GrenadeItem component.
    /// </summary>
    [CustomEditor(typeof(GrenadeItem))]
    public class GrenadeItemInspector : ThrowableItemInspector
    {
        /// <summary>
        /// Callback which allows subclasses to draw any usable properties.
        /// </summary>
        protected override void DrawUsableProperties()
        {
            base.DrawUsableProperties();

            EditorGUILayout.PropertyField(PropertyFromName("m_AnimatePinRemoval"));
            InspectorUtility.DrawAnimationEventTrigger(target, "Removal Pin Event", PropertyFromName("m_RemovePinEvent"));
        }
    }
}