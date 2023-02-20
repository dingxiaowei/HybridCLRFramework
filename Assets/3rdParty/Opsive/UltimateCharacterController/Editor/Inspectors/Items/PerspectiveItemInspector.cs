/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEditor;
using UnityEngine;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem;
using System;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Items
{
    /// <summary>
    /// Shows a custom inspector for the PerspectiveItem component.
    /// </summary>
    [CustomEditor(typeof(PerspectiveItem))]
    public abstract class PerspectiveItemInspector : StateBehaviorInspector
    {
        /// <summary>
        /// Returns the actions to draw before the State list is drawn.
        /// </summary>
        /// <returns>The actions to draw before the State list is drawn.</returns>
        protected override Action GetDrawCallback()
        {
            var baseCallback = base.GetDrawCallback();

            baseCallback += () =>
            {
                if (Foldout("Render")) {
                    EditorGUI.indentLevel++;
                    GUI.enabled = !Application.isPlaying;
                    var objectProperty = PropertyFromName("m_Object");
                    EditorGUILayout.PropertyField(objectProperty);
                    GUI.enabled = true;
                    if (objectProperty.objectReferenceValue == null || EditorUtility.IsPersistent(objectProperty.objectReferenceValue) ||
                        (objectProperty.objectReferenceValue as GameObject).transform.IsChildOf((target as PerspectiveItem).transform)) {
                        DrawSpawnParentProperties();
                    } else if (!Application.isPlaying) {
                        // The object must a GameObject and a child of the current character.
                        if ((objectProperty.objectReferenceValue as GameObject) == null ||
                            (objectProperty.objectReferenceValue as GameObject).GetComponentInParent<ItemHandler>() != (target as PerspectiveItem).GetComponentInParent<ItemHandler>()) {
                            objectProperty.objectReferenceValue = null;
                        }
                    }
                    DrawRenderProperties();
                    EditorGUI.indentLevel--;
                }
            };

            return baseCallback;
        }

        /// <summary>
        /// Draws the options for spawning based on a parent.
        /// </summary>
        protected virtual void DrawSpawnParentProperties() { }

        /// <summary>
        /// Checks to see if the Object reference value exists. If it doesn't then a warning will be displayed.
        /// </summary>
        protected void CheckForObject()
        {
            if (PropertyFromName("m_Object").objectReferenceValue == null) {
                EditorGUILayout.HelpBox("An Object is required if the item is visible.", MessageType.Warning);
            }
        }

        /// <summary>
        /// Draws the options for the render foldout.
        /// </summary>
        protected virtual void DrawRenderProperties() { }
    }
}