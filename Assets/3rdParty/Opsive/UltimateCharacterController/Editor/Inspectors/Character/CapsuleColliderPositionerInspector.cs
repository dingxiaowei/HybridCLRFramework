/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character
{
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem;
    using System;
    using UnityEditor;

    [CustomEditor(typeof(CapsuleColliderPositioner))]
    public class CapsuleColliderPositionerInspector : StateBehaviorInspector
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
                EditorGUILayout.PropertyField(PropertyFromName("m_FirstEndCapTarget"));
                EditorGUILayout.PropertyField(PropertyFromName("m_SecondEndCapTarget"));
                EditorGUILayout.PropertyField(PropertyFromName("m_SecondEndCapPadding"));
                EditorGUILayout.PropertyField(PropertyFromName("m_RotateCollider"));
                if (PropertyFromName("m_RotateCollider").boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_EndCapRotation"));
                    if (!PropertyFromName("m_EndCapRotation").boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(PropertyFromName("m_RotationBone"));
                        EditorGUILayout.PropertyField(PropertyFromName("m_RotationBoneOffset"));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(PropertyFromName("m_AdjustHeight"));
                if (!PropertyFromName("m_AdjustHeight").boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_PositionBone"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_HeightOverride"));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(PropertyFromName("m_CenterOffset"));
            };
            return baseCallback;
        }
    }
}