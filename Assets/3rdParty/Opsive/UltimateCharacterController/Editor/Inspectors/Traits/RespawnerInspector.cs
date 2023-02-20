/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.Editor.Inspectors.Audio;
using Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using System;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Traits
{
    /// <summary>
    /// Shows a custom inspector for the Respawner component.
    /// </summary>
    [CustomEditor(typeof(Respawner), true)]
    public class RespawnerInspector : StateBehaviorInspector
    {
        private ReorderableList m_ReorderableRespawnAudioClipsList;

        private Respawner m_Respawner;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            m_Respawner = target as Respawner;
        }

        /// <summary>
        /// Returns the actions to draw before the State list is drawn.
        /// </summary>
        /// <returns>The actions to draw before the State list is drawn.</returns>
        protected override Action GetDrawCallback()
        {
            var baseCallback = base.GetDrawCallback();

            baseCallback += () =>
            {
                var positioningMode = PropertyFromName("m_PositioningMode");
                EditorGUILayout.PropertyField(positioningMode);
                if ((Respawner.SpawnPositioningMode)positioningMode.enumValueIndex == Respawner.SpawnPositioningMode.SpawnPoint) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_Grouping"));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(PropertyFromName("m_MinRespawnTime"));
                EditorGUILayout.PropertyField(PropertyFromName("m_MaxRespawnTime"));
                EditorGUILayout.PropertyField(PropertyFromName("m_ScheduleRespawnOnDeath"));
                EditorGUILayout.PropertyField(PropertyFromName("m_ScheduleRespawnOnDisable"));

                if (Foldout("Respawn Audio")) {
                    EditorGUI.indentLevel++;
                    AudioClipSetInspector.DrawAudioClipSet(m_Respawner.RespawnAudioClipSet, PropertyFromName("m_RespawnAudioClipSet"), ref m_ReorderableRespawnAudioClipsList, OnRespawnAudioClipDraw, OnRespawnAudioClipListAdd, OnRespawnAudioClipListRemove);
                    EditorGUI.indentLevel--;
                }

                if (Foldout("Events")) {
                    EditorGUI.indentLevel++;
                    InspectorUtility.UnityEventPropertyField(PropertyFromName("m_OnRespawnEvent"));
                    EditorGUI.indentLevel--;
                }
            };

            return baseCallback;
        }

        /// <summary>
        /// Draws the AudioClip element.
        /// </summary>
        private void OnRespawnAudioClipDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AudioClipSetInspector.OnAudioClipDraw(m_ReorderableRespawnAudioClipsList, rect, index, m_Respawner.RespawnAudioClipSet, null);
        }

        /// <summary>
        /// Adds a new AudioClip element to the AudioClipSet.
        /// </summary>
        private void OnRespawnAudioClipListAdd(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListAdd(list, m_Respawner.RespawnAudioClipSet, null);
        }

        /// <summary>
        /// Remove the AudioClip element at the list index.
        /// </summary>
        private void OnRespawnAudioClipListRemove(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListRemove(list, m_Respawner.RespawnAudioClipSet, null);
            m_Respawner.RespawnAudioClipSet.AudioClips = (AudioClip[])list.list;
        }
    }
}