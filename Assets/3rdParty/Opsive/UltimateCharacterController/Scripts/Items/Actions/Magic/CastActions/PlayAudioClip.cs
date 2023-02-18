/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Magic.CastActions
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Audio;
    using UnityEngine;

    /// <summary>
    /// Plays an audio clip when the cast is performed.
    /// </summary>
    [System.Serializable]
    public class PlayAudioClip : CastAction
    {
        [Tooltip("The AudioClip that should be played. A random AudioClip will be selected.")]
        [SerializeField] protected AudioClip[] m_AudioClips;
        [Tooltip("Plays the AudioClip at the origin. If the value is false the character position will be used.")]
        [SerializeField] protected bool m_PlayAtOrigin = true;
        [Tooltip("Should the AudioClip loop?")]
        [SerializeField] protected bool m_Loop;
        [Tooltip("The duration of the AudioSource fade. Set to 0 to disable fading out.")]
        [SerializeField] protected float m_FadeOutDuration = 0.1f;
        [Tooltip("The amount to fade out the AudioSource.")]
        [SerializeField] protected float m_FadeStep = 0.05f;

        public AudioClip[] AudioClips { get { return m_AudioClips; } set { m_AudioClips = value; } }
        public bool PlayAtOrigin { get { return m_PlayAtOrigin; } set { m_PlayAtOrigin = value; } }
        public bool Loop { get { return m_Loop; } set { m_Loop = value; } }
        public float FadeOutDuration { get { return m_FadeOutDuration; } set { m_FadeOutDuration = value; } }
        public float FadeStep { get { return m_FadeStep; } set { m_FadeStep = value; } }

        private AudioSource m_AudioSource;
        private ScheduledEventBase m_FadeEvent;

        /// <summary>
        /// Performs the cast.
        /// </summary>
        /// <param name="origin">The location that the cast should spawn from.</param>
        /// <param name="direction">The direction of the cast.</param>
        /// <param name="targetPosition">The target position of the cast.</param>
        public override void Cast(Transform origin, Vector3 direction, Vector3 targetPosition)
        {
            if (m_AudioSource != null && m_FadeEvent == null) {
                return;
            }

            if (m_AudioClips == null || m_AudioClips.Length == 0) {
                Debug.LogError("Error: An Audio Clip must be specified.", m_MagicItem);
                return;
            }

            var audioClip = m_AudioClips[Random.Range(0, m_AudioClips.Length)];
            if (audioClip == null) {
                Debug.Log("Error: The Audio Clip array has a null value.");
                return;
            }
            m_AudioSource = AudioManager.PlayAtPosition(audioClip, m_PlayAtOrigin ? origin.position : m_GameObject.transform.position);
            if (m_AudioSource != null) {
                m_AudioSource.volume = 1;
                m_AudioSource.loop = m_Loop;
            }
            if (m_FadeEvent != null) {
                Scheduler.Cancel(m_FadeEvent);
                m_FadeEvent = null;
            }
        }

        /// <summary>
        /// Stops the cast.
        /// </summary>
        public override void Stop()
        {
            base.Stop();

            if (m_AudioSource != null) {
                if (m_FadeOutDuration > 0) {
                    FadeAudio(m_FadeOutDuration / (1 / m_FadeStep));
                } else {
                    m_AudioSource.Stop();
                    m_AudioSource = null;
                }
            }
        }

        /// <summary>
        /// Fades the audio volume.
        /// </summary>
        /// <param name="interval">The interval of the fade.</param>
        private void FadeAudio(float interval)
        {
            m_AudioSource.volume -= m_FadeStep;
            if (m_AudioSource.volume > 0) {
                m_FadeEvent = Scheduler.Schedule(interval, FadeAudio, interval);
            } else {
                m_AudioSource.Stop();
                m_AudioSource = null;
                m_FadeEvent = null;
            }
        }
    }
}