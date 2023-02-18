/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Inventory
{
    using Opsive.UltimateCharacterController.Inventory;
    using UnityEditor;

    /// <summary>
    /// Custom inspector for the ItemSetManager component.
    /// </summary>
    [CustomEditor(typeof(ItemSetManager))]
    public class ItemSetManagerInspector : ItemSetManagerBaseInspector
    {
        private ItemCollection m_ItemCollection;

        /// <summary>
        /// Initializes the ItemSet categories.
        /// </summary>
        /// <returns>True if the categories were initialized.</returns>
        protected override bool InitializeCategories()
        {
            var itemSetManager = m_ItemSetManager as ItemSetManager;
            var itemCollection = EditorGUILayout.ObjectField("Item Collection", itemSetManager.ItemCollection, typeof(ItemCollection), false) as ItemCollection;
            if (itemCollection == null) {
                EditorGUILayout.HelpBox("An ItemCollection reference is required.", MessageType.Error);
                return false;
            } else if (itemSetManager.ItemCollection != itemCollection) {
                itemSetManager.ItemCollection = itemCollection;
                m_ItemSetReorderableList = null;
            }
            m_ItemCollection = itemCollection;
            CheckCategories(m_ItemCollection.Categories);
            CheckItemSetAbilities(m_ItemCollection.Categories);
            return true;
        }
    }
}