/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character.Effects
{
    using Opsive.UltimateCharacterController.Character.Effects;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Audio;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using UnityEditorInternal;
    using UnityEngine;

    /// <summary>
    /// Draws a custom inspector for the PlayAudioClip effect.
    /// </summary>
    [InspectorDrawer(typeof(PlayAudioClip))]
    public class PlayAudioClipInspectorDrawer : EffectInspectorDrawer
    {
        private PlayAudioClip m_PlayAudioClip;
        private ReorderableList m_AudioClipsList;

        /// <summary>
        /// Draws the fields related to the inspector drawer.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        protected override void DrawInspectorDrawerFields(object target, Object parent)
        {
            ObjectInspector.DrawFields(target, true);

            m_PlayAudioClip = target as PlayAudioClip;
            m_AudioClipsList = AudioClipSetInspector.DrawAudioClipSet(m_PlayAudioClip.AudioClipSet, null, m_AudioClipsList, OnAudioClipDraw, OnAudioClipListAdd, OnAudioClipListRemove);
        }

        /// <summary>
        /// Draws the AudioClip element.
        /// </summary>
        private void OnAudioClipDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            AudioClipSetInspector.OnAudioClipDraw(m_AudioClipsList, rect, index, m_PlayAudioClip.AudioClipSet, null);
        }

        /// <summary>
        /// Adds a new AudioClip element to the AudioClipSet.
        /// </summary>
        private void OnAudioClipListAdd(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListAdd(list, m_PlayAudioClip.AudioClipSet, null);
        }

        /// <summary>
        /// Remove the AudioClip element at the list index.
        /// </summary>
        private void OnAudioClipListRemove(ReorderableList list)
        {
            AudioClipSetInspector.OnAudioClipListRemove(list, m_PlayAudioClip.AudioClipSet, null);
            m_PlayAudioClip.AudioClipSet.AudioClips = (AudioClip[])list.list;
        }
    }
}