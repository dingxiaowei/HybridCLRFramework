/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character.Abilities
{
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Audio;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    /// <summary>
    /// Draws a custom inspector for the Jump Ability.
    /// </summary>
    [InspectorDrawer(typeof(Jump))]
    public class JumpInspectorDrawer : AbilityInspectorDrawer
    {
        private Jump m_Jump;
        private ReorderableList m_ReorderableAirborneJumpAudioClipsList;

        /// <summary>
        /// The ability has been added to the Ultimate Character Locomotion. Perform any initialization.
        /// </summary>
        /// <param name="ability">The ability that has been added.</param>
        /// <param name="parent">The parent of the added ability.</param>
        public override void AbilityAdded(Ability ability, Object parent)
        {
            base.AbilityAdded(ability, parent);

            // The character should jump immediately if there is no animator.
            var characterLocomotion = parent as UltimateCharacterController.Character.UltimateCharacterLocomotion;
            var animator = characterLocomotion.GetComponent<Animator>();
            if (animator == null) {
                (ability as Jump).JumpEvent = new UltimateCharacterController.Utility.AnimationEventTrigger(false, 0);
            }
        }

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
            InspectorUtility.DrawField(target, "m_MaxAirborneJumpCount");
            if (m_Jump.MaxAirborneJumpCount > 0) {
                EditorGUI.indentLevel++;
                InspectorUtility.DrawField(target, "m_AirborneJumpForce");
                InspectorUtility.DrawField(target, "m_AirborneJumpFrames");
                if (InspectorUtility.Foldout(target, "Airborne Jump Audio")) {
                    EditorGUI.indentLevel++;
                    m_ReorderableAirborneJumpAudioClipsList = AudioClipSetInspector.DrawAudioClipSet(m_Jump.AirborneJumpAudioClipSet, null, m_ReorderableAirborneJumpAudioClipsList, OnAirborneJumpAudioClipDraw, OnAirborneJumpAudioClipListAdd, OnAirborneJumpAudioClipListRemove);
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
        private void OnAirborneJumpAudioClipDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AudioClipSetInspector.OnAudioClipDraw(m_ReorderableAirborneJumpAudioClipsList, rect, index, m_Jump.AirborneJumpAudioClipSet, null);
        }

        /// <summary>
        /// Adds a new AudioClip element to the AudioClipSet.
        /// </summary>
        private void OnAirborneJumpAudioClipListAdd(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListAdd(list, m_Jump.AirborneJumpAudioClipSet, null);
        }

        /// <summary>
        /// Remove the AudioClip element at the list index.
        /// </summary>
        private void OnAirborneJumpAudioClipListRemove(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListRemove(list, m_Jump.AirborneJumpAudioClipSet, null);
            m_Jump.AirborneJumpAudioClipSet.AudioClips = (AudioClip[])list.list;
        }
    }
}