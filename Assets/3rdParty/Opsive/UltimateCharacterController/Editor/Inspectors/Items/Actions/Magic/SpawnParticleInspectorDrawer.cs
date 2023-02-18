/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Items.Actions.Magic
{
    using Opsive.UltimateCharacterController.Editor.Inspectors;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Items.Actions.Magic.CastActions;
    using UnityEngine;
    using UnityEditor;

    /// <summary>
    /// Draws an inspector for the SpawnParticle CastAction.
    /// </summary>
    [InspectorDrawer(typeof(SpawnParticle))]
    public class SpawnParticleInspectorDrawer : InspectorDrawer
    {
        /// <summary>
        /// Called when the object should be drawn to the inspector.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        public override void OnInspectorGUI(object target, Object parent)
        {
            InspectorUtility.DrawField(target, "m_ParticlePrefab");
            InspectorUtility.DrawField(target, "m_PositionOffset");
            InspectorUtility.DrawField(target, "m_RotationOffset");
            InspectorUtility.DrawField(target, "m_ParentToOrigin");
            InspectorUtility.DrawField(target, "m_ProjectDirectionOnPlane");
            InspectorUtility.DrawField(target, "m_ClearParentOnStop");
            InspectorUtility.DrawField(target, "m_SetRendererLengthScale");
            var spawnParticle = target as SpawnParticle;
            if (spawnParticle.SetRendererLengthScale) {
                EditorGUI.indentLevel++;
                InspectorUtility.DrawField(target, "m_AdditionalLength");
                EditorGUI.indentLevel--;
            }
            spawnParticle.ParticleLayer = EditorGUILayout.LayerField(new GUIContent("Particle Layer", "The layer that the particle should occupy."), spawnParticle.ParticleLayer);
            InspectorUtility.DrawField(target, "m_FadeInDuration");
            InspectorUtility.DrawField(target, "m_FadeOutDuration");
            if (spawnParticle.FadeInDuration > 0 || spawnParticle.FadeOutDuration > 0) {
                EditorGUI.indentLevel++;
                InspectorUtility.DrawField(target, "m_MaterialColorName");
                InspectorUtility.DrawField(target, "m_FadeStep");
                EditorGUI.indentLevel--;
            }
        }
    }
}