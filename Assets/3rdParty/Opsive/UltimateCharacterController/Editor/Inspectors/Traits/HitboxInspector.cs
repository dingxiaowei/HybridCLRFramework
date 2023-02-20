/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Traits
{
    /// <summary>
    /// Draws a custom inspector for the hitbox.
    /// </summary>
    public class HitboxInspector
    {
        /// <summary>
        /// Draws the hitbox.
        /// </summary>
        public static void DrawHitbox(ref ReorderableList reorderableList, SerializedObject serializedObject, SerializedProperty hitboxProperty, ReorderableList.ElementCallbackDelegate elementCallback)
        {
            if (reorderableList == null) {
                reorderableList = new ReorderableList(serializedObject, hitboxProperty, true, true, true, true);
                reorderableList.drawHeaderCallback = OnHitboxHeaderDraw;
                reorderableList.onAddCallback = OnHitboxListAdd;
                reorderableList.drawElementCallback = elementCallback;
            }
            // Indent the list so it lines up with the rest of the content.
            var rect = GUILayoutUtility.GetRect(0, reorderableList.GetHeight());
            rect.x += EditorGUI.indentLevel * InspectorUtility.IndentWidth;
            rect.xMax -= EditorGUI.indentLevel * InspectorUtility.IndentWidth;
            reorderableList.DoList(rect);
        }

        /// <summary>
        /// Draws the Hitbox ReordableList header.
        /// </summary>
        public static void OnHitboxHeaderDraw(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width - 130, EditorGUIUtility.singleLineHeight), "Collider");
            EditorGUI.LabelField(new Rect(rect.x + (rect.width - 130), rect.y, 130, EditorGUIUtility.singleLineHeight), " Damage Multiplier");
        }

        /// <summary>
        /// A new hitbox element has been added.
        /// </summary>
        private static void OnHitboxListAdd(ReorderableList reorderableList)
        {
            reorderableList.serializedProperty.InsertArrayElementAtIndex(reorderableList.serializedProperty.arraySize);
            var hitbox = reorderableList.serializedProperty.GetArrayElementAtIndex(reorderableList.serializedProperty.arraySize - 1);
            hitbox.FindPropertyRelative("m_DamageMultiplier").floatValue = 1;

            var serializedObject = reorderableList.serializedProperty.serializedObject;
            InspectorUtility.RecordUndoDirtyObject(serializedObject.targetObject, "Change Value");
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the Hitbox ReordableList element.
        /// </summary>
        public static void HitboxElementDraw(ReorderableList reorderableList, Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();

            var hitbox = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            var colliderProperty = hitbox.FindPropertyRelative("m_Collider");
            var multiplierProperty = hitbox.FindPropertyRelative("m_DamageMultiplier");
            EditorGUI.ObjectField(new Rect(rect.x - 12, rect.y + 1, (rect.width - 110), EditorGUIUtility.singleLineHeight), colliderProperty, new GUIContent());
            multiplierProperty.floatValue = EditorGUI.FloatField(new Rect(rect.x + (rect.width - 126), rect.y + 1, 126, EditorGUIUtility.singleLineHeight), multiplierProperty.floatValue);

            if (EditorGUI.EndChangeCheck()) {
                var serializedObject = reorderableList.serializedProperty.serializedObject;
                InspectorUtility.RecordUndoDirtyObject(serializedObject.targetObject, "Change Value");
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Draws a visual representation of the hitbox.
        /// </summary>
        public static void DrawHitboxGizmo(Hitbox[] hitboxes, GizmoType gizmoType)
        {
            if (hitboxes == null) {
                return;
            }

            for (int i = 0; i < hitboxes.Length; ++i) {
                var collider = hitboxes[i].Collider;
                if (collider == null) {
                    continue;
                }

                // The color depends on the damage multiplier.
                var multiplier = hitboxes[i].DamageMultiplier;
                var color = multiplier > 1 ? Color.Lerp(Color.yellow, Color.red, Mathf.Clamp01(multiplier - 1)) : Color.Lerp(Color.green, Color.yellow, Mathf.Clamp01(multiplier));
                color.a = 0.5f;
                Gizmos.color = color;

                // The gizmo should be drawn in the same location as the collider.
                var transform = collider.transform;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

                // Box and sphere colliders can be drawn.
                if (collider is BoxCollider) {
                    Gizmos.DrawCube((collider as BoxCollider).center, (collider as BoxCollider).size);

                    Gizmos.color = InspectorUtility.GetContrastColor(color);
                    Gizmos.DrawWireCube((collider as BoxCollider).center, (collider as BoxCollider).size);
                } else if (collider is SphereCollider) {
                    Gizmos.DrawSphere((collider as SphereCollider).center, (collider as SphereCollider).radius);

                    Gizmos.color = InspectorUtility.GetContrastColor(color);
                    Gizmos.DrawWireSphere((collider as SphereCollider).center, (collider as SphereCollider).radius);
                }
            }
        }
    }
}