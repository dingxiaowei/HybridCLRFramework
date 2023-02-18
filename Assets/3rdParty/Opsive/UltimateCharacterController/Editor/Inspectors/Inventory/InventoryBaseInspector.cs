/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Inventory
{
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Inventory;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Custom inspector for the InventoryBase component.
    /// </summary>
    [CustomEditor(typeof(InventoryBase))]
    public class InventoryBaseInspector : InspectorBase
    {
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
                var itemIdentifiers = inventory.GetAllItemIdentifiers();
                if (itemIdentifiers.Count > 0) {
                    GUI.enabled = false;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Item Identifier");
                    EditorGUILayout.LabelField("Count");
                    GUILayout.Space(-150);
                    EditorGUILayout.EndHorizontal();
                    for (int i = 0; i < itemIdentifiers.Count; ++i) {
                        EditorGUILayout.BeginHorizontal();
                        var style = EditorStyles.label;
                        var label = itemIdentifiers[i].ToString();
                        var activeCount = 0;
                        for (int j = 0; j < inventory.SlotCount; ++j) {
                            var item = inventory.GetActiveItem(j);
                            if (item != null && item.ItemIdentifier == itemIdentifiers[i]) {
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
                        EditorGUILayout.LabelField(inventory.GetItemIdentifierAmount(itemIdentifiers[i]).ToString());
                        GUILayout.Space(-150);
                        EditorGUILayout.EndHorizontal();
                    }
                    GUI.enabled = true;
                } else {
                    EditorGUILayout.LabelField("(Nothing in inventory)");
                }
                EditorGUI.indentLevel--;
            }

            DrawInventoryProperties();

            EditorGUILayout.PropertyField(PropertyFromName("m_RemoveAllOnDeath"));
            EditorGUILayout.PropertyField(PropertyFromName("m_LoadDefaultLoadoutOnRespawn"));
            EditorGUILayout.PropertyField(PropertyFromName("m_UnequippedStateName"));

            if (Foldout("Events")) {
                EditorGUI.indentLevel++;
                InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnAddItemEvent"));
                InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnPickupItemIdentifierEvent"));
                InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnPickupItemEvent"));
                InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnEquipItemEvent"));
                InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnAdjustItemIdentifierAmountEvent"));
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
        /// Draws the properties for the inventory subclass.
        /// </summary>
        protected virtual void DrawInventoryProperties() { }
    }
}