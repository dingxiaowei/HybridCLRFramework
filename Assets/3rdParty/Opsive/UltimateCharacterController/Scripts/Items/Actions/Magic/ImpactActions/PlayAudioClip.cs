/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Magic.ImpactActions
{
    using Opsive.UltimateCharacterController.Audio;
    using UnityEngine;

    /// <summary>
    /// Plays an AudioClip on the impacted object.
    /// </summary>
    public class PlayAudioClip : ImpactAction
    {
        [Tooltip("The AudioClip that should be played when the impact occurs. A random AudioClip will be selected.")]
        [SerializeField] protected AudioClip[] m_AudioClips;

        public AudioClip[] AudioClips { get { return m_AudioClips; } set { m_AudioClips = value; } }

        /// <summary>
        /// Perform the impact action.
        /// </summary>
        /// <param name="castID">The ID of the cast.</param>
        /// <param name="source">The object that caused the cast.</param>
        /// <param name="target">The object that was hit by the cast.</param>
        /// <param name="hit">The raycast that caused the impact.</param>
        protected override void ImpactInternal(uint castID, GameObject source, GameObject target, RaycastHit hit)
        {
            if (m_AudioClips == null || m_AudioClips.Length == 0) {
                Debug.LogError("Error: An Audio Clip must be specified", m_MagicItem);
                return;
            }

            var audioClip = m_AudioClips[Random.Range(0, m_AudioClips.Length)];
            if (audioClip == null) {
                Debug.Log("Error: The Audio Clip array has a null value.");
                return;
            }
            AudioManager.PlayAtPosition(audioClip, hit.point);
        }
    }
}