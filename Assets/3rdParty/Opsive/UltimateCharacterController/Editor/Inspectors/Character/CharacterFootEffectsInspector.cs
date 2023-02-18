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

    [CustomEditor(typeof(CharacterFootEffects), true)]
    public class CharacterFootEffectsInspector : StateBehaviorInspector
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
                if (Foldout("Footprint")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_SurfaceImpact"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_FootstepMode"));
                    var mode = (CharacterFootEffects.FootstepPlacementMode)PropertyFromName("m_FootstepMode").enumValueIndex;
                    EditorGUILayout.PropertyField(PropertyFromName("m_FootOffset"));
                    if (mode == CharacterFootEffects.FootstepPlacementMode.BodyStep) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(PropertyFromName("m_Feet"), true);
                        EditorGUILayout.PropertyField(PropertyFromName("m_MoveDirectionFrameCount"));
                        EditorGUI.indentLevel--;
                    } else if (mode == CharacterFootEffects.FootstepPlacementMode.FixedInterval) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(PropertyFromName("m_Interval"));
                        EditorGUI.indentLevel--;
                    } else if (mode == CharacterFootEffects.FootstepPlacementMode.CameraBob) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(PropertyFromName("m_MinBobInterval"));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
            };
            return baseCallback;
        }
    }
}