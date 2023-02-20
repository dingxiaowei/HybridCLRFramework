/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEditor;
using Opsive.UltimateCharacterController.ThirdPersonController.Camera;
using Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem;
using System;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.ThirdPersonController.Camera
{
    /// <summary>
    /// Shows a custom inspector for the ObjectFader.
    /// </summary>
    [CustomEditor(typeof(ObjectFader))]
    public class ObjectFaderInspector : StateBehaviorInspector
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
                EditorGUILayout.PropertyField(PropertyFromName("m_CharacterFade"));
                if (PropertyFromName("m_CharacterFade").boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_CacheCharacterMaterials"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_StartFadeDistance"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_EndFadeDistance"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_CharacterFadeStateChangeCooldown"));
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(PropertyFromName("m_ObstructingObjectsFade"));
                if (PropertyFromName("m_ObstructingObjectsFade").boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_CollisionRadius"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_FadeSpeed"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_FadeColor"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_AutoSetMode"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_DisableCollider"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_MaxObstructingColliderCount"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_MaxObstructingMaterialCount"));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(PropertyFromName("m_TransformOffset"));
            };
            return baseCallback;
        }
    }
}