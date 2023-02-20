/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Opsive.UltimateCharacterController.StateSystem;
using Opsive.UltimateCharacterController.Editor.Inspectors.Audio;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem.CharacterAssist
{
    /// <summary>
    /// Custom inspector for the StateTrigger component.
    /// </summary>
    [CustomEditor(typeof(StateTrigger), true)]
    public class StateTriggerInspector : InspectorBase
    {
        private StateTrigger m_StateTrigger;
        private ReorderableList m_ReorderableActivateAudioClipsList;

        /// <summary>
        /// The inspector has been enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            m_StateTrigger = target as StateTrigger;
        }

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(PropertyFromName("m_StateName"));
            EditorGUILayout.PropertyField(PropertyFromName("m_Delay"));
            EditorGUILayout.PropertyField(PropertyFromName("m_Duration"));
            EditorGUILayout.PropertyField(PropertyFromName("m_LayerMask"));
            EditorGUILayout.PropertyField(PropertyFromName("m_RequireCharacter"));
            if (Foldout("Audio")) {
                EditorGUI.indentLevel++;
                AudioClipSetInspector.DrawAudioClipSet(m_StateTrigger.ActivateAudioClipSet, PropertyFromName("m_ActivateAudioClipSet"), ref m_ReorderableActivateAudioClipsList, OnActivateAudioClipDraw, OnActivateAudioClipListAdd, OnActivateAudioClipListRemove);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Value Change");
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Draws the AudioClip element.
        /// </summary>
        private void OnActivateAudioClipDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AudioClipSetInspector.OnAudioClipDraw(m_ReorderableActivateAudioClipsList, rect, index, m_StateTrigger.ActivateAudioClipSet, null);
        }

        /// <summary>
        /// Adds a new AudioClip element to the AudioClipSet.
        /// </summary>
        private void OnActivateAudioClipListAdd(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListAdd(list, m_StateTrigger.ActivateAudioClipSet, null);
        }

        /// <summary>
        /// Remove the AudioClip element at the list index.
        /// </summary>
        private void OnActivateAudioClipListRemove(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListRemove(list, m_StateTrigger.ActivateAudioClipSet, null);
            m_StateTrigger.ActivateAudioClipSet.AudioClips = (AudioClip[])list.list;
        }
    }
}