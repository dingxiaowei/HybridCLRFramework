/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Effects
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Audio;
    using UnityEngine;

    /// <summary>
    /// Plays an AudioClip when the effect starts.
    /// </summary>
    public class PlayAudioClip : Effect
    {
        [Tooltip("A set of AudioClips that can be played when the effect is started.")]
        [HideInInspector] [SerializeField] protected AudioClipSet m_AudioClipSet = new AudioClipSet();

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

            var audioSource = m_AudioClipSet.PlayAudioClip(m_GameObject);
            if (audioSource != null) {
                Scheduler.ScheduleFixed(audioSource.clip.length, StopEffect);
            }
        }
    }
}