/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Opsive.UltimateCharacterController.Objects;
using Opsive.UltimateCharacterController.Editor.Inspectors.Audio;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Objects
{
    /// <summary>
    /// Custom inspector for the Explosion component.
    /// </summary>
    [CustomEditor(typeof(Explosion))]
    public class ExplosionInspector : InspectorBase
    {
        private Explosion m_Explosion;
        private ReorderableList m_ReorderableExplosionAudioClipsList;

        /// <summary>
        /// The inspector has been enabled.
        /// </summary>
        public void OnEnable()
        {
            m_Explosion = target as Explosion;
        }

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(PropertyFromName("m_ExplodeOnEnable"));
            EditorGUILayout.PropertyField(PropertyFromName("m_Radius"));
            EditorGUILayout.PropertyField(PropertyFromName("m_DamageAmount"));
            EditorGUILayout.PropertyField(PropertyFromName("m_ImpactForce"));
            EditorGUILayout.PropertyField(PropertyFromName("m_ImpactForceFrames"));
            EditorGUILayout.PropertyField(PropertyFromName("m_ImpactLayers"));
            EditorGUILayout.PropertyField(PropertyFromName("m_LineOfSight"));
            EditorGUILayout.PropertyField(PropertyFromName("m_Lifespan"));
            EditorGUILayout.PropertyField(PropertyFromName("m_MaxCollisionCount"));
            if (Foldout("Audio")) {
                EditorGUI.indentLevel++;
                AudioClipSetInspector.DrawAudioClipSet(m_Explosion.ExplosionAudioClipSet, PropertyFromName("m_ExplosionAudioClipSet"), ref m_ReorderableExplosionAudioClipsList, OnExplosionAudioClipDraw, OnExplosionAudioClipListAdd, OnExplosionAudioClipListRemove);
                EditorGUI.indentLevel--;
            }
            InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnImpactEvent"));

            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Value Change");
                serializedObject.ApplyModifiedProperties();
                InspectorUtility.SetDirty(target);
            }
        }

        /// <summary>
        /// Draws the AudioClip element.
        /// </summary>
        private void OnExplosionAudioClipDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AudioClipSetInspector.OnAudioClipDraw(m_ReorderableExplosionAudioClipsList, rect, index, m_Explosion.ExplosionAudioClipSet, null);
        }

        /// <summary>
        /// Adds a new AudioClip element to the AudioClipSet.
        /// </summary>
        private void OnExplosionAudioClipListAdd(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListAdd(list, m_Explosion.ExplosionAudioClipSet, null);
        }

        /// <summary>
        /// Remove the AudioClip element at the list index.
        /// </summary>
        private void OnExplosionAudioClipListRemove(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListRemove(list, m_Explosion.ExplosionAudioClipSet, null);
            m_Explosion.ExplosionAudioClipSet.AudioClips = (AudioClip[])list.list;
        }
    }
}
