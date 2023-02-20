/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Opsive.UltimateCharacterController.Objects.CharacterAssist;
using Opsive.UltimateCharacterController.Editor.Inspectors.Audio;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Objects.CharacterAssist
{
    /// <summary>
    /// Custom inspector for the ObjectPickup component.
    /// </summary>
    [CustomEditor(typeof(ObjectPickup), true)]
    public class ObjectPickupInspector : InspectorBase
    {
        private ObjectPickup m_ObjectPickup;
        private ReorderableList m_ReorderablePickupAudioClipsList;

        /// <summary>
        /// The inspector has been enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            m_ObjectPickup = target as ObjectPickup;
        }

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            DrawObjectPickupFields();
            EditorGUILayout.PropertyField(PropertyFromName("m_TriggerEnableDelay"));
            EditorGUILayout.PropertyField(PropertyFromName("m_PickupOnTriggerEnter"));
            EditorGUILayout.PropertyField(PropertyFromName("m_RotationSpeed"));
            if (Foldout("UI")) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName("m_PickupMessageText"));
                EditorGUILayout.PropertyField(PropertyFromName("m_PickupMessageIcon"));
                EditorGUI.indentLevel--;
            }
            if (Foldout("Audio")) {
                EditorGUI.indentLevel++;
                AudioClipSetInspector.DrawAudioClipSet(m_ObjectPickup.PickupAudioClipSet, PropertyFromName("m_PickupAudioClipSet"), ref m_ReorderablePickupAudioClipsList, OnPickupAudioClipDraw, OnPickupAudioClipListAdd, OnPickupAudioClipListRemove);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Value Change");
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Draws the object pickup fields.
        /// </summary>
        protected virtual void DrawObjectPickupFields() { }

        /// <summary>
        /// Draws the AudioClip element.
        /// </summary>
        private void OnPickupAudioClipDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AudioClipSetInspector.OnAudioClipDraw(m_ReorderablePickupAudioClipsList, rect, index, m_ObjectPickup.PickupAudioClipSet, null);
        }

        /// <summary>
        /// Adds a new AudioClip element to the AudioClipSet.
        /// </summary>
        private void OnPickupAudioClipListAdd(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListAdd(list, m_ObjectPickup.PickupAudioClipSet, null);
        }

        /// <summary>
        /// Remove the AudioClip element at the list index.
        /// </summary>
        private void OnPickupAudioClipListRemove(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListRemove(list, m_ObjectPickup.PickupAudioClipSet, null);
            m_ObjectPickup.PickupAudioClipSet.AudioClips = (AudioClip[])list.list;
        }
    }
}