/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Items.Actions.Magic
{
    using Opsive.UltimateCharacterController.Editor.Inspectors;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Items.Actions.Magic.ImpactActions;
    using UnityEngine;

    [InspectorDrawer(typeof(ModifyAttribute))]
    public class ModifyAttributeInspectorDrawer : InspectorDrawer
    {
        public override void OnInspectorGUI(object target, Object parent)
        {
            InspectorUtility.DrawAttributeModifier(null, (target as ModifyAttribute).AttributeModifier, "Attribute Modifier");
        }
    }
}