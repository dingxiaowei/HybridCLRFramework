/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Editor.Inspectors.Audio;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character.Abilities
{
    /// <summary>
    /// Draws a custom inspector for the Jump Ability.
    /// </summary>
    [InspectorDrawer(typeof(Jump))]
    public class JumpInspectorDrawer : AbilityInspectorDrawer
    {
        private Jump m_Jump;
        private ReorderableList m_ReorderableRepeatedJumpAudioClipsList;

        /// <summary>
        /// Draws the fields related to the inspector drawer.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        protected override void DrawInspectorDrawerFields(object target, Object parent)
        {
            m_Jump = target as Jump;

            InspectorUtility.DrawField(target, "m_MinCeilingJumpHeight");
            InspectorUtility.DrawField(target, "m_GroundedGracePeriod");
            InspectorUtility.DrawField(target, "m_Force");
            InspectorUtility.DrawField(target, "m_SidewaysForceMultiplier");
            InspectorUtility.DrawField(target, "m_BackwardsForceMultiplier");
            InspectorUtility.DrawField(target, "m_Frames");
            InspectorUtility.DrawField(target, "m_JumpEvent");
            InspectorUtility.DrawField(target, "m_JumpSurfaceImpact");
            InspectorUtility.DrawField(target, "m_ForceHold");
            InspectorUtility.DrawField(target, "m_ForceDampingHold");
            InspectorUtility.DrawField(target, "m_MaxRepeatedJumpCount");
            if (m_Jump.MaxRepeatedJumpCount > 0) {
                EditorGUI.indentLevel++;
                InspectorUtility.DrawField(target, "m_RepeatedJumpForce");
                InspectorUtility.DrawField(target, "m_RepeatedJumpFrames");
                if (InspectorUtility.Foldout(target, "Repeated Jump Audio")) {
                    EditorGUI.indentLevel++;
                    AudioClipSetInspector.DrawAudioClipSet(m_Jump.RepeatedJumpAudioClipSet, null, ref m_ReorderableRepeatedJumpAudioClipsList, OnRepeatedJumpAudioClipDraw, OnRepeatedJumpAudioClipListAdd, OnRepeatedJumpAudioClipListRemove);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
            InspectorUtility.DrawField(target, "m_VerticalVelocityStopThreshold");
            InspectorUtility.DrawField(target, "m_RecurrenceDelay");
        }

        /// <summary>
        /// Draws the AudioClip element.
        /// </summary>
        private void OnRepeatedJumpAudioClipDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AudioClipSetInspector.OnAudioClipDraw(m_ReorderableRepeatedJumpAudioClipsList, rect, index, m_Jump.RepeatedJumpAudioClipSet, null);
        }

        /// <summary>
        /// Adds a new AudioClip element to the AudioClipSet.
        /// </summary>
        private void OnRepeatedJumpAudioClipListAdd(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListAdd(list, m_Jump.RepeatedJumpAudioClipSet, null);
        }

        /// <summary>
        /// Remove the AudioClip element at the list index.
        /// </summary>
        private void OnRepeatedJumpAudioClipListRemove(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListRemove(list, m_Jump.RepeatedJumpAudioClipSet, null);
            m_Jump.RepeatedJumpAudioClipSet.AudioClips = (AudioClip[])list.list;
        }
    }
}