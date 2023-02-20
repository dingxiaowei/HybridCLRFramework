/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Inventory;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Inventory
{
    /// <summary>
    /// Shows a custom inspector for ItemTypeCount.
    /// </summary>
    public static class ItemTypeCountInspector
    {
        private const int c_ValueWidth = 120;

        /// <summary>
        /// Draws the ItemTypeCount ReordableList header.
        /// </summary>
        public static void OnItemTypeCountHeaderDraw(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width - c_ValueWidth, EditorGUIUtility.singleLineHeight), "Item Type");
            EditorGUI.LabelField(new Rect(rect.x + 12 + (rect.width - c_ValueWidth), rect.y, c_ValueWidth, EditorGUIUtility.singleLineHeight), "Count");
        }

        /// <summary>
        /// Draws the ItemTypeCount ReordableList element.
        /// </summary>
        public static void OnItemTypeCountElementDraw(SerializedProperty itemTypeCountProperties, Rect rect, int index, bool isActive, bool isFocused)
        {
            var objRect = rect;
            objRect.x -= 12;
            objRect.width -= c_ValueWidth - 12;
            objRect.y += (objRect.height - 18) / 2;
            objRect.height = 16;
            EditorGUI.PropertyField(objRect, itemTypeCountProperties.GetArrayElementAtIndex(index).FindPropertyRelative("m_ItemType"), new GUIContent());

            var valueRect = rect;
            valueRect.x = objRect.xMax + 10;
            valueRect.width = c_ValueWidth - 22;
            valueRect.y += (valueRect.height - 18) / 2;
            valueRect.height = 16;
            EditorGUI.PropertyField(valueRect, itemTypeCountProperties.GetArrayElementAtIndex(index).FindPropertyRelative("m_Count"), new GUIContent());
        }
    }
}