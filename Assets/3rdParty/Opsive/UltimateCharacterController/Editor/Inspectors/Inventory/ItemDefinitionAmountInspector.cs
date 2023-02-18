/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Inventory
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Shows a custom inspector for ItemDefinitionAmount.
    /// </summary>
    public static class ItemDefinitionAmountInspector
    {
        private const int c_ValueWidth = 120;

        /// <summary>
        /// Draws the ItemDefinitionAmount ReordableList header.
        /// </summary>
        public static void OnItemDefinitionAmountHeaderDraw(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width - c_ValueWidth, EditorGUIUtility.singleLineHeight), "Item Definition");
            EditorGUI.LabelField(new Rect(rect.x + 12 + (rect.width - c_ValueWidth), rect.y, c_ValueWidth, EditorGUIUtility.singleLineHeight), "Amount");
        }

        /// <summary>
        /// Draws the ItemDefinitionAmount ReordableList element.
        /// </summary>
        public static void OnItemDefinitionAmountElementDraw(SerializedProperty itemIdentifierAmountProperties, Rect rect, int index, bool isActive, bool isFocused)
        {
            var objRect = rect;
            objRect.x -= 12;
            objRect.width -= c_ValueWidth - 12;
            objRect.y += (objRect.height - 18) / 2;
            objRect.height = 16;
            EditorGUI.PropertyField(objRect, itemIdentifierAmountProperties.GetArrayElementAtIndex(index).FindPropertyRelative("ItemDefinition"), new GUIContent());

            var valueRect = rect;
            valueRect.x = objRect.xMax + 10;
            valueRect.width = c_ValueWidth - 22;
            valueRect.y += (valueRect.height - 18) / 2;
            valueRect.height = 16;
            EditorGUI.PropertyField(valueRect, itemIdentifierAmountProperties.GetArrayElementAtIndex(index).FindPropertyRelative("Amount"), new GUIContent());
        }
    }
}