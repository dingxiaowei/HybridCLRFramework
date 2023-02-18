/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character.Abilities.Items
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Inventory;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws a custom inspector for the ItemSetAbilityBase ItemAbility.
    /// </summary>
    [InspectorDrawer(typeof(ItemSetAbilityBase))]
    public class ItemSetAbilityBaseInspectorDrawer : AbilityInspectorDrawer
    {
        /// <summary>
        /// Draws the fields related to the inspector drawer.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        protected override void DrawInspectorDrawerFields(object target, Object parent)
        {
            // ItemCollection must exist for the categories to be populated.
            var itemSetManager = (parent as UltimateCharacterLocomotion).GetComponent<ItemSetManagerBase>();
            if (itemSetManager == null) {
                EditorGUILayout.HelpBox("The character must have the ItemSetManager component.", MessageType.Error);
                return;
            }
            itemSetManager.Initialize(false);
            if (itemSetManager.CategoryItemSets == null || itemSetManager.CategoryItemSets.Length == 0) {
                return;
            }

            // Draw a popup with all of the ItemSet categories.
            var categoryID = InspectorUtility.GetFieldValue<uint>(target, "m_ItemSetCategoryID");
            var selected = -1;
            var categoryNames = new string[itemSetManager.CategoryItemSets.Length];
            for (int i = 0; i < categoryNames.Length; ++i) {
                categoryNames[i] = itemSetManager.CategoryItemSets[i].CategoryName;
                if (categoryID == itemSetManager.CategoryItemSets[i].CategoryID) {
                    selected = i;
                }
            }
            var newSelected = EditorGUILayout.Popup("ItemSet Category", (selected != -1 ? selected : 0), categoryNames);
            if (selected != newSelected || RandomID.IsIDEmpty(categoryID)) {
                InspectorUtility.SetFieldValue(target, "m_ItemSetCategoryID", itemSetManager.CategoryItemSets[newSelected].CategoryID);
                GUI.changed = true;
            }

            base.DrawInspectorDrawerFields(target, parent);
        }
    }
}