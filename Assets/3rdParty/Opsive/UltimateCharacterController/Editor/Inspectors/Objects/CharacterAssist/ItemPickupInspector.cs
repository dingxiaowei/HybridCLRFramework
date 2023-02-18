/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Objects.CharacterAssist
{
    using Opsive.UltimateCharacterController.Editor.Inspectors.Inventory;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Editor.Managers;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Objects.CharacterAssist;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    /// <summary>
    /// Custom inspector for the ItemPickup component.
    /// </summary>
    [CustomEditor(typeof(ItemPickup), true)]
    public class ItemPickupInspector : ItemPickupBaseInspector
    {
        private ItemCollection m_ItemCollection;
        private ReorderableList m_ReordableItemAmount;

        /// <summary>
        /// Finds the Item Collection.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            m_ItemCollection = ManagerUtility.FindItemCollection(this);
        }

        /// <summary>
        /// Draws the header fields for the ItemPickupSet.
        /// </summary>
        protected override void DrawItemPickupSetHeaderFields()
        {
            m_ItemCollection = EditorGUILayout.ObjectField("Item Collection", m_ItemCollection, typeof(ItemCollection), false) as ItemCollection;
        }

        /// <summary>
        /// Draws the inspector for the ItemIdentifier list.
        /// </summary>
        protected override void DrawItemIdentifierInspector()
        {
            if (Foldout("Item Definition Amounts")) {
                EditorGUI.indentLevel++;
                if (m_ReordableItemAmount == null) {
                    var itemListProperty = PropertyFromName("m_ItemDefinitionAmounts");
                    m_ReordableItemAmount = new ReorderableList(serializedObject, itemListProperty, true, true, true, true);
                    m_ReordableItemAmount.drawHeaderCallback = OnItemIdentifierAmountHeaderDraw;
                    m_ReordableItemAmount.drawElementCallback = OnItemIdentifierAmountElementDraw;
                    m_ReordableItemAmount.elementHeight = c_SlotRowHeight;
                }
                var listRect = GUILayoutUtility.GetRect(0, m_ReordableItemAmount.GetHeight());
                listRect.x += EditorGUI.indentLevel * InspectorUtility.IndentWidth;
                listRect.xMax -= EditorGUI.indentLevel * InspectorUtility.IndentWidth;
                m_ReordableItemAmount.DoList(listRect);
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// Draws the field that displays the available categories.
        /// </summary>
        /// <param name="pickupSet">The PickupSet that should be drawn.</param>
        /// <param name="objRect">The location the categories should draw.</param>
        protected override void DrawAvailableCategories(ItemPickupBase.PickupSet pickupSet, Rect objRect)
        {
            var categoryNames = new string[((m_ItemCollection != null && m_ItemCollection.Categories != null) ? m_ItemCollection.Categories.Length : 0) + 1];
            categoryNames[0] = "(Not Specified)";
            var selected = 0;
            if (categoryNames.Length > 1 && GUI.enabled) {
                for (int i = 0; i < m_ItemCollection.Categories.Length; ++i) {
                    categoryNames[i + 1] = m_ItemCollection.Categories[i].name;
                    if (pickupSet.CategoryID == m_ItemCollection.Categories[i].ID) {
                        selected = i;
                    }
                }
            }
            int newSelected;
            if (objRect.width == 0) {
                newSelected = EditorGUILayout.Popup("Category", selected != -1 ? selected : 0, categoryNames);
            } else {
                newSelected = EditorGUI.Popup(objRect, selected != -1 ? selected : 0, categoryNames);
            }
            if (selected != newSelected) {
                if (newSelected == 0) {
                    pickupSet.CategoryID = 0;
                } else {
                    pickupSet.CategoryID = m_ItemCollection.Categories[newSelected - 1].ID;
                }
                GUI.changed = true;
            }
        }

        /// <summary>
        /// Draws the ItemIdentifierAmount ReordableList header.
        /// </summary>
        private void OnItemIdentifierAmountHeaderDraw(Rect rect)
        {
            ItemDefinitionAmountInspector.OnItemDefinitionAmountHeaderDraw(rect);
        }

        /// <summary>
        /// Draws the ItemIdentifierAmount ReordableList element.
        /// </summary>
        private void OnItemIdentifierAmountElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            ItemDefinitionAmountInspector.OnItemDefinitionAmountElementDraw(PropertyFromName("m_ItemDefinitionAmounts"), rect, index, isActive, isFocused);
        }
    }
}