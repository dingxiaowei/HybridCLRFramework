/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Inventory
{
    /// <summary>
    /// Custom inspector for the InventoryBase component.
    /// </summary>
    [CustomEditor(typeof(InventoryBase), true)]
    public class InventoryBaseInspector : InspectorBase
    {
        private ReorderableList m_DefaultLoadoutReordableList;

        /// <summary>
        /// Draws the inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();

            // Show the items currently within the inspector.
            if (Foldout("Current Inventory")) {
                EditorGUI.indentLevel++;
                var inventory = target as InventoryBase;
                var itemTypes = inventory.GetAllItemTypes();
                if (itemTypes.Count > 0) {
                    GUI.enabled = false;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("ItemType");
                    EditorGUILayout.LabelField("Count");
                    GUILayout.Space(-150);
                    EditorGUILayout.EndHorizontal();
                    for (int i = 0; i < itemTypes.Count; ++i) {
                        EditorGUILayout.BeginHorizontal();
                        var style = EditorStyles.label;
                        var label = itemTypes[i].name;
                        var activeCount = 0;
                        for (int j = 0; j < inventory.SlotCount; ++j) {
                            if (inventory.GetItem(j) != null && inventory.GetItem(j).ItemType == itemTypes[i]) {
                                if (activeCount == 0) {
                                    label += " (Slot " + j;
                                } else {
                                    label += ", " + j;
                                }
                                style = EditorStyles.boldLabel;
                                activeCount++;
                            }
                        }
                        if (activeCount > 0) {
                            label += ")";
                        }
                        EditorGUILayout.LabelField(label, style);
                        EditorGUILayout.LabelField(inventory.GetItemTypeCount(itemTypes[i]).ToString());
                        GUILayout.Space(-150);
                        EditorGUILayout.EndHorizontal();
                    }
                    GUI.enabled = true;
                } else {
                    EditorGUILayout.LabelField("(Nothing in inventory)");
                }
                EditorGUI.indentLevel--;
            }

            if (Foldout("Default Loadout")) {
                EditorGUI.indentLevel++;
                if (m_DefaultLoadoutReordableList == null) {
                    var itemListProperty = PropertyFromName("m_DefaultLoadout");
                    m_DefaultLoadoutReordableList = new ReorderableList(serializedObject, itemListProperty, true, true, true, true);
                    m_DefaultLoadoutReordableList.drawHeaderCallback = OnDefaultLoadoutHeaderDraw;
                    m_DefaultLoadoutReordableList.drawElementCallback = OnDefaultLoadoutElementDraw;
                }
                var listRect = GUILayoutUtility.GetRect(0, m_DefaultLoadoutReordableList.GetHeight());
                listRect.x += EditorGUI.indentLevel * InspectorUtility.IndentWidth;
                listRect.xMax -= EditorGUI.indentLevel * InspectorUtility.IndentWidth;
                m_DefaultLoadoutReordableList.DoList(listRect);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(PropertyFromName("m_RemoveAllOnDeath"));
            EditorGUILayout.PropertyField(PropertyFromName("m_LoadDefaultLoadoutOnRespawn"));

            if (Foldout("Events")) {
                EditorGUI.indentLevel++;
                InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnAddItemEvent"));
                InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnPickupItemTypeEvent"));
                InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnPickupItemEvent"));
                InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnEquipItemEvent"));
                InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnUseItemTypeEvent"));
                InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnUnequipItemEvent"));
                InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnRemoveItemEvent"));
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Draws the DefaultLoadout ReordableList header.
        /// </summary>
        private void OnDefaultLoadoutHeaderDraw(Rect rect)
        {
            ItemTypeCountInspector.OnItemTypeCountHeaderDraw(rect);
        }

        /// <summary>
        /// Draws the DefaultLoadout ReordableList element.
        /// </summary>
        private void OnDefaultLoadoutElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            ItemTypeCountInspector.OnItemTypeCountElementDraw(PropertyFromName("m_DefaultLoadout"), rect, index, isActive, isFocused);
        }
    }
}