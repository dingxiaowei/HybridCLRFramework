/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Character.Abilities.Starters;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character.Abilities.Starters
{
    /// <summary>
    /// Draws a custom inspector for the ComboTimeout AbilityStarter.
    /// </summary>
    [InspectorDrawer(typeof(ComboTimeout))]
    public class ComboTimeoutInspectorDrawer : InspectorDrawer
    {
        /// <summary>
        /// Called when the object should be drawn to the inspector.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        public override void OnInspectorGUI(object target, Object parent)
        {
            var comboTimeout = target as ComboTimeout;

            EditorGUI.BeginChangeCheck();
            // Draw a custom array inspector for the input names.
            var elements = comboTimeout.ComboInputElements;
            if (elements == null || elements.Length == 0) {
                elements = new ComboTimeout.ComboInputElement[1];
                GUI.changed = true;
            }

            // Draw the table header.
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("");
            var prevLabelWidth = EditorGUIUtility.labelWidth;
            GUILayout.Space(-110);
            EditorGUIUtility.labelWidth = 60;
            EditorGUILayout.LabelField("Input Name");
            GUILayout.Space(-30);
            EditorGUILayout.LabelField("Start Type", GUILayout.MaxWidth(180));
            GUILayout.Space(-100);
            EditorGUILayout.LabelField("Timeout", GUILayout.Width(140));
            EditorGUIUtility.labelWidth = prevLabelWidth;
            EditorGUILayout.EndHorizontal();

            // Draw each combo element.
            for (int i = 0; i < elements.Length; ++i) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Combo " + (i + 1));
                GUILayout.Space(-110);
                elements[i].InputName = EditorGUILayout.TextField(elements[i].InputName);
                GUILayout.Space(-30);
                elements[i].AxisInput = EditorGUILayout.Popup(elements[i].AxisInput ? 1 : 0, new string[] { "Button Down", "Axis" }, GUILayout.MaxWidth(180)) == 1;
                GUILayout.Space(-30);
                // The first element does not use the timeout value.
                GUI.enabled = i > 0;
                elements[i].Timeout = EditorGUILayout.FloatField(elements[i].Timeout, GUILayout.Width(140 - (i == elements.Length - 1 ? 44 : 0)));
                GUI.enabled = true;
                // Only the last row can add/remove elements.
                if (i == elements.Length - 1) {
                    if (i > 0 && GUILayout.Button(InspectorStyles.RemoveIcon, InspectorStyles.NoPaddingButtonStyle, GUILayout.Width(18))) {
                        System.Array.Resize(ref elements, elements.Length - 1);
                    }
                    if (GUILayout.Button(InspectorStyles.AddIcon, InspectorStyles.NoPaddingButtonStyle, GUILayout.Width(18))) {
                        System.Array.Resize(ref elements, elements.Length + 1);
                        elements[elements.Length - 1] = elements[elements.Length - 2];
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            if (EditorGUI.EndChangeCheck()) {
                comboTimeout.ComboInputElements = elements;
                GUI.changed = true;
            }
        }
    }
}