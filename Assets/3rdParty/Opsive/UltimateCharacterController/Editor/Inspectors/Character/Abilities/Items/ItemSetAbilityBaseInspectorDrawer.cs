/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.Abilities.Items;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character.Abilities.Items
{
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
            var itemSetManager = (parent as UltimateCharacterLocomotion).GetComponent<ItemSetManager>();
            if (itemSetManager.ItemCollection == null) {
                EditorGUILayout.HelpBox("An ItemCollection reference is required on the ItemSetManager.", MessageType.Error);
                return;
            }

            // Draw a popup with all of the ItemSet categories.
            var categoryID = InspectorUtility.GetFieldValue<int>(target, "m_ItemSetCategoryID");
            var selected = -1;
            var categoryNames = new string[itemSetManager.ItemCollection.Categories.Length];
            for (int i = 0; i < categoryNames.Length; ++i) {
                categoryNames[i] = itemSetManager.ItemCollection.Categories[i].Name;
                if (categoryID == itemSetManager.ItemCollection.Categories[i].ID) {
                    selected = i;
                }
            }
            var newSelected = EditorGUILayout.Popup("ItemSet Category", (selected != -1 ? selected : 0), categoryNames);
            if (selected != newSelected || categoryID == 0) {
                InspectorUtility.SetFieldValue(target, "m_ItemSetCategoryID", itemSetManager.ItemCollection.Categories[newSelected].ID);
                GUI.changed = true;
            }

            base.DrawInspectorDrawerFields(target, parent);
        }
    }
}