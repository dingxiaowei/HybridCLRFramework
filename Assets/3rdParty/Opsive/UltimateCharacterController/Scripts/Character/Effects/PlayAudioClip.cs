/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Audio;
using Opsive.UltimateCharacterController.Game;

namespace Opsive.UltimateCharacterController.Character.Effects
{
    /// <summary>
    /// Plays an AudioClip when the effect starts.
    /// </summary>
    public class PlayAudioClip : Effect
    {
        [Tooltip("A set of AudioClips that can be played when the effect is started.")]
        [HideInInspector] [SerializeField] protected AudioClipSet m_AudioClipSet;

        public AudioClipSet AudioClipSet { get { return m_AudioClipSet; } set { m_AudioClipSet = value; } }

        /// <summary>
        /// Can the effect be started?
        /// </summary>
        /// <returns>True if the effect can be started.</returns>
        public override bool CanStartEffect()
        {
            return m_AudioClipSet.AudioClips.Length > 0;
        }

        /// <summary>
        /// The effect has been started.
        /// </summary>
        protected override void EffectStarted()
        {
            base.EffectStarted();

            var clip = m_AudioClipSet.PlayAudioClip(m_GameObject);
            AudioManager.Play(m_GameObject, clip);

            Scheduler.ScheduleFixed(clip.length, StopEffect);
        }
    }
}