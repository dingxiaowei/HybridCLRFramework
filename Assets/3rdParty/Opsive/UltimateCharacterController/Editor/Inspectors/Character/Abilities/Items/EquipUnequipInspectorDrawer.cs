/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Character.Abilities.Items;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character.Abilities.Items
{
    /// <summary>
    /// Draws a custom inspector for the EquipUnequip ItemAbility.
    /// </summary>
    [InspectorDrawer(typeof(EquipUnequip))]
    public class EquipUnequipInspectorDrawer : ItemSetAbilityBaseInspectorDrawer
    {
        /// <summary>
        /// Draws the fields related to the inspector drawer.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        protected override void DrawInspectorDrawerFields(object target, Object parent)
        {
            // Draw AutoEquip manually so it'll use the MaskField.
            var autoEquip = (int)InspectorUtility.GetFieldValue<EquipUnequip.AutoEquipType>(target, "m_AutoEquip");
            var equipString = System.Enum.GetNames(typeof(EquipUnequip.AutoEquipType));
            var value = EditorGUILayout.MaskField(new GUIContent("Auto Equip", InspectorUtility.GetFieldTooltip(target, "m_AutoEquip")), autoEquip, equipString);
            if (value != autoEquip) {
                InspectorUtility.SetFieldValue(target, "m_AutoEquip", value);
            }

            base.DrawInspectorDrawerFields(target, parent);
        }
    }
}