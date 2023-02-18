/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Audio
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// The AudioManager manages the audio to ensure to ensure no two clips are playing on the same AudioSource at the same time.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager s_Instance;
        private static AudioManager Instance
        {
            get
            {
                if (!s_Initialized) {
                    s_Instance = new GameObject("Audio Manager").AddComponent<AudioManager>();
                    s_Initialized = true;
                }
                return s_Instance;
            }
        }
        private static bool s_Initialized;

        /// <summary>
        /// The AudioSourcesIndex class allows for an AudioSource to be selected based upon its play state. If all AudioSources are being 
        /// played then a new AudioSource will be added.
        /// </summary>
        private class AudioSourcesIndex
        {
            private GameObject m_GameObject;
            private AudioSource[] m_AudioSources;
            private int m_ReserveCount = 0;
            private int m_Index;
            private bool m_AudioManagerGameObject;

            public AudioSource[] AudioSources { get { return m_AudioSources; } }
            public int ReserveCount { set { m_ReserveCount = value; } }

            /// <summary>
            /// AudioSourcesIndex constructor.
            /// </summary>
            /// <param name="gameObject">The GameObject that the AudioSources are attached to.</param>
            /// <param name="audioManagerGameObject">Is this object attached to the AudioManager?</param>
            /// <param name="volume">The volume of the new audio source.</param>
            public AudioSourcesIndex(GameObject gameObject, bool audioManagerGameObject, float volume)
            {
                m_GameObject = gameObject;
                m_AudioManagerGameObject = audioManagerGameObject;
                m_AudioSources = m_GameObject.GetComponents<AudioSource>();

                // At least one AudioSource must exist.
                if (m_AudioSources.Length == 0) {
                    AddAudioSource(volume);
                } else {
                    for (int i = 0; i < m_AudioSources.Length; ++i) {
                        m_AudioSources[i].playOnAwake = false;
                    }
                }
            }

            /// <summary>
            /// Returns an AudioSource which is not currently playing.
            /// </summary>
            /// <param name="reservedIndex">The index of the component that should be played. -1 indicates any component.</param>
            /// <returns>An AudioSource which is not currently playing.</returns>
            public AudioSource GetAvailableAudioSource(int reservedIndex)
            {
                return GetAvailableAudioSource(reservedIndex, m_AudioSources[m_Index], false);
            }

            /// <summary>
            /// Returns an AudioSource which is not currently playing.
            /// </summary>
            /// <param name="reservedIndex">The index of the component that should be played. -1 indicates any component.</param>
            /// <param name="copyFromAudioSource">Specifies which AudioSource to copy from if the properties need to be copied.</param>
            /// <param name="forceCopyProperties">Should the AudioSource properties be copied even if a new AudioSource isn't created?</param>
            /// <returns>An AudioSource which is not currently playing.</returns>
            public AudioSource GetAvailableAudioSource(int reservedIndex, AudioSource copyFromAudioSource, bool forceCopyProperties)
            {
                // If the same AudioSource is requested it allows the audio to be interrupted (such as for equip/unequip).
                AudioSource audioSource = null;
                if (reservedIndex != -1) {
                    while (reservedIndex >= m_AudioSources.Length) {
                        AddAudioSource(1);
                    }
                    if (forceCopyProperties) {
                        CopyAudioProperties(copyFromAudioSource, m_AudioSources[reservedIndex]);
                    }
                    return m_AudioSources[reservedIndex];
                } else if (m_Index < m_ReserveCount) {
                    // Ensure the index doesn't occupy an element that is reserved.
                    m_Index = m_ReserveCount % m_AudioSources.Length;
                    if (m_Index < m_ReserveCount) {
                        // If the index is still less then the reserve count then a new AudioSource needs to be created.
                        audioSource = AddAudioSource(1);
                        CopyAudioProperties(copyFromAudioSource, audioSource);
                        m_Index = m_AudioSources.Length - 1;
                    }
                } else {
                    audioSource = m_AudioSources[m_Index];
                }

                var count = 0;
                var copyProperties = forceCopyProperties;
                while (audioSource.isPlaying) {
                    if (count < m_AudioSources.Length && audioSource != null) {
                        m_Index = (m_Index + 1) % m_AudioSources.Length;
                        // The AudioSource is resrved and cannot be used if the index is less then the min reserved index. This allows AudioSources
                        // to be designated for a specific effect.
                        if (m_Index < m_ReserveCount) {
                            m_Index = m_ReserveCount % m_AudioSources.Length;
                            // If the index is still less then the Reserve Count then there aren't enough AudioSources available. Set the count to the max
                            // so a new AudioSource will be created.
                            if (m_Index < m_ReserveCount) {
                                count = m_AudioSources.Length;
                                continue;
                            }
                        }
                        audioSource = m_AudioSources[m_Index];
                        count++;
                    } else {
                        audioSource = AddAudioSource(1);
                        copyProperties = true;
                    }
                }
                if (copyProperties) {
                    CopyAudioProperties(copyFromAudioSource, audioSource);
                }
                audioSource.spatialBlend = m_AudioManagerGameObject ? 0 : 1;
                return audioSource;
            }

            /// <summary>
            /// Adds a new AudioSource to the array.
            /// </summary>
            /// <param name="volume">The volume of the AudioSource.</param>
            /// <returns>The added AudioSource.</returns>
            private AudioSource AddAudioSource(float volume)
            {
                // If the count is equal to the length then a new AudioSource needs to be added.
                var addGameObject = m_GameObject;
                // Any child AudioSources of the AudioManager should be attached to their own GameObject so it can be repositioned.
                if (m_AudioManagerGameObject) {
                    addGameObject = new GameObject("AudioSource");
                    addGameObject.transform.parent = m_GameObject.transform;
                }
                var newAudioSource = addGameObject.AddComponent<AudioSource>();
                newAudioSource.playOnAwake = false;
                newAudioSource.volume = volume;
                if (!m_AudioManagerGameObject) {
                    newAudioSource.spatialBlend = 1;
                    newAudioSource.maxDistance = 20;
                }
                // The new AudioSource may be active.
                newAudioSource.Stop();

                // Add the new AudioSource to the array.
                System.Array.Resize(ref m_AudioSources, m_AudioSources.Length + 1);
                m_AudioSources[m_AudioSources.Length - 1] = newAudioSource;

                // Return the new AudioSource.
                return newAudioSource;
            }

            /// <summary>
            /// Copies the AudioSource properties from the original AudioSource to the new AudioSource.
            /// </summary>
            /// <param name="originalAudioSource">The original AudioSource to copy from.</param>
            /// <param name="newAudioSource">The AudioSource to copy to.</param>
            private void CopyAudioProperties(AudioSource originalAudioSource, AudioSource newAudioSource)
            {
                newAudioSource.bypassEffects = originalAudioSource.bypassEffects;
                newAudioSource.bypassListenerEffects = originalAudioSource.bypassListenerEffects;
                newAudioSource.bypassReverbZones = originalAudioSource.bypassReverbZones;
                newAudioSource.dopplerLevel = originalAudioSource.dopplerLevel;
                newAudioSource.ignoreListenerPause = originalAudioSource.ignoreListenerPause;
                newAudioSource.ignoreListenerVolume = originalAudioSource.ignoreListenerVolume;
                newAudioSource.loop = originalAudioSource.loop;
                newAudioSource.maxDistance = originalAudioSource.maxDistance;
                newAudioSource.minDistance = originalAudioSource.minDistance;
                newAudioSource.mute = originalAudioSource.mute;
                newAudioSource.outputAudioMixerGroup = originalAudioSource.outputAudioMixerGroup;
                newAudioSource.panStereo = originalAudioSource.panStereo;
                newAudioSource.playOnAwake = originalAudioSource.playOnAwake;
                newAudioSource.pitch = originalAudioSource.pitch;
                newAudioSource.priority = originalAudioSource.priority;
                newAudioSource.reverbZoneMix = originalAudioSource.reverbZoneMix;
                newAudioSource.rolloffMode = originalAudioSource.rolloffMode;
                newAudioSource.spatialBlend = originalAudioSource.spatialBlend;
                newAudioSource.spatialize = originalAudioSource.spatialize;
                newAudioSource.spatializePostEffects = originalAudioSource.spatializePostEffects;
                newAudioSource.spread = originalAudioSource.spread;
                newAudioSource.velocityUpdateMode = originalAudioSource.velocityUpdateMode;
                newAudioSource.volume = originalAudioSource.volume;
            }

            /// <summary>
            /// Stops playing the AudioSource.
            /// </summary>
            /// <param name="reservedIndex">The index of the component that should be stopped. -1 indicates all components.</param>
            public void Stop(int reservedIndex)
            {
                if (reservedIndex == -1) {
                    for (int i = 0; i < m_AudioSources.Length; ++i) {
                        m_AudioSources[i].Stop();
                    }
                } else {
                    m_AudioSources[reservedIndex].Stop();
                }
            }
        }

        private Dictionary<GameObject, AudioSourcesIndex> m_GameObjectAudioSourcesMap = new Dictionary<GameObject, AudioSourcesIndex>();
        private GameObject m_GameObject;

        /// <summary>
        /// The object has been enabled.
        /// </summary>
        private void OnEnable()
        {
            // The object may have been enabled outside of the scene unloading.
            if (s_Instance == null) {
                s_Instance = this;
                s_Initialized = true;
                SceneManager.sceneUnloaded -= SceneUnloaded;
            }
        }

        /// <summary>
        /// The AudioManager can also play audio clips if the target GameObject is being disabled.
        /// </summary>
        private void Start()
        {
            m_GameObject = gameObject;

            RegisterInternal(m_GameObject, 1);
        }

        /// <summary>
        /// Registers the AudioSources on the GameObject so they can be played.
        /// </summary>
        /// <param name="gameObject">The GameObject to register.</param>
        public static void Register(GameObject gameObject)
        {
            Instance.RegisterInternal(gameObject, 1);
        }

        /// <summary>
        /// Registers the AudioSources on the GameObject so they can be played.
        /// </summary>
        /// <param name="gameObject">The GameObject to register.</param>
        /// <param name="volume">The volume of the new audio source.</param>
        public static void Register(GameObject gameObject, float volume)
        {
            Instance.RegisterInternal(gameObject, volume);
        }

        /// <summary>
        /// Internal method which registers the AudioSources on the GameObject so they can be played.
        /// </summary>
        /// <param name="gameObject">The GameObject to register.</param>
        /// <param name="volume">The volume of the new audio source.</param>
        protected virtual void RegisterInternal(GameObject gameObject, float volume)
        {
            // The same GameObject can act as an AudioSource for multiple objects so it may have already been registered.
            if (m_GameObjectAudioSourcesMap.ContainsKey(gameObject)) {
                return;
            }

            var audioSourcesIndex = new AudioSourcesIndex(gameObject, gameObject == m_GameObject, volume);
            m_GameObjectAudioSourcesMap.Add(gameObject, audioSourcesIndex);
        }

        /// <summary>
        /// Sets the number of components that should be reserved for a specific effect.
        /// </summary>
        /// <param name="gameObject">The GameObject that is reserving the components.</param>
        /// <param name="count">The number of components to reserve.</param>
        public static void SetReserveCount(GameObject gameObject, int count)
        {
            Instance.SetReserveCountInternal(gameObject, count);
        }

        /// <summary>
        /// Internal method which sets the number of components that should be reserved for a specific effect.
        /// </summary>
        /// <param name="gameObject">The GameObject that is reserving the components.</param>
        /// <param name="count">The number of components to reserve.</param>
        public void SetReserveCountInternal(GameObject gameObject, int count)
        {
            AudioSourcesIndex audioSourcesIndex;
            if (!m_GameObjectAudioSourcesMap.TryGetValue(gameObject, out audioSourcesIndex)) {
                Debug.LogError("Error: The GameObject " + gameObject.name + " has not been registered with the AudioManager.");
                return;
            }

            audioSourcesIndex.ReserveCount = count;
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <returns>The AudioSource that is playing the AudioClip (can be null).</returns>
        public static AudioSource Play(GameObject gameObject, AudioClip clip)
        {
            return Instance.PlayInternal(gameObject, clip, 1, false, 0, -1);
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="loop">Does the clip loop?</param>
        /// <returns>The AudioSource that is playing the AudioClip (can be null).</returns>
        public static AudioSource Play(GameObject gameObject, AudioClip clip, bool loop)
        {
            return Instance.PlayInternal(gameObject, clip, 1, loop, 0, -1);
        }

        /// <summary>
        /// Plays the audio clip with the specified delay.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="pitch">The pitch to play the clip at.</param>
        /// <param name="delay">The number of seconds to delay the clip from playing.</param>
        /// <returns>The AudioSource that is playing the AudioClip (can be null).</returns>
        public static AudioSource PlayDelayed(GameObject gameObject, AudioClip clip, float delay)
        {
            return Instance.PlayInternal(gameObject, clip, 1, false, delay, -1);
        }

        /// <summary>
        /// Plays the audio clip with the specified delay.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="pitch">The pitch to play the clip at.</param>
        /// <param name="loop">Does the clip loop?</param>
        /// <param name="delay">The number of seconds to delay the clip from playing.</param>
        /// <returns>The AudioSource that is playing the AudioClip (can be null).</returns>
        public static AudioSource PlayDelayed(GameObject gameObject, AudioClip clip, float delay, bool loop)
        {
            return Instance.PlayInternal(gameObject, clip, 1, loop, delay, -1);
        }

        /// <summary>
        /// Plays the audio clip with the specified delay.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="pitch">The pitch to play the clip at.</param>
        /// <param name="delay">The number of seconds to delay the clip from playing.</param>
        /// <param name="reservedIndex">The index of the component that should be played. -1 indicates any component.</param>
        /// <returns>The AudioSource that is playing the AudioClip (can be null).</returns>
        public static AudioSource PlayDelayed(GameObject gameObject, AudioClip clip, float delay, int reservedIndex)
        {
            return Instance.PlayInternal(gameObject, clip, 1, false, delay, reservedIndex);
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="pitch">The pitch to play the clip at.</param>
        /// <returns>The AudioSource that is playing the AudioClip (can be null).</returns>
        public static AudioSource Play(GameObject gameObject, AudioClip clip, float pitch)
        {
            return Instance.PlayInternal(gameObject, clip, pitch, false, 0, -1);
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="pitch">The pitch to play the clip at.</param>
        /// <param name="loop">Does the clip loop?</param>
        /// <returns>The AudioSource that is playing the AudioClip (can be null).</returns>
        public static AudioSource Play(GameObject gameObject, AudioClip clip, float pitch, bool loop)
        {
            return Instance.PlayInternal(gameObject, clip, pitch, loop, 0, -1);
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="pitch">The pitch to play the clip at.</param>
        /// <param name="delay">The number of seconds to delay the clip from playing.</param>
        /// <returns>The AudioSource that is playing the AudioClip (can be null).</returns>
        public static AudioSource Play(GameObject gameObject, AudioClip clip, float pitch, float delay)
        {
            return Instance.PlayInternal(gameObject, clip, pitch, false, delay, -1);
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="pitch">The pitch to play the clip at.</param>
        /// <param name="delay">The number of seconds to delay the clip from playing.</param>
        /// <param name="reservedIndex">The index of the component that should be played. -1 indicates any component.</param>
        /// <returns>The AudioSource that is playing the AudioClip (can be null).</returns>
        public static AudioSource Play(GameObject gameObject, AudioClip clip, float pitch, float delay, int reservedIndex)
        {
            return Instance.PlayInternal(gameObject, clip, pitch, false, delay, reservedIndex);
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="pitch">The pitch to play the clip at.</param>
        /// <param name="loop">Does the clip loop?</param>
        /// <param name="delay">The number of seconds to delay the clip from playing.</param>
        /// <param name="reservedIndex">The index of the component that should be played. -1 indicates any component.</param>
        /// <returns>The AudioSource that is playing the AudioClip (can be null).</returns>
        public static AudioSource Play(GameObject gameObject, AudioClip clip, float pitch, bool loop, float delay, int reservedIndex)
        {
            return Instance.PlayInternal(gameObject, clip, pitch, loop, delay, reservedIndex);
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="pitch">The pitch to play the clip at.</param>
        /// <param name="reservedIndex">The index of the component that should be played. -1 indicates any component.</param>
        /// <returns>The AudioSource that is playing the AudioClip (can be null).</returns>
        public static AudioSource Play(GameObject gameObject, AudioClip clip, float pitch, int reservedIndex)
        {
            return Instance.PlayInternal(gameObject, clip, pitch, false, 0, reservedIndex);
        }

        /// <summary>
        /// Internal method which plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="clip">The clip to play.</param>
        /// <param name="pitch">The pitch to play the clip at.</param>
        /// <param name="loop">Does the clip loop?</param>
        /// <param name="delay">The number of seconds to delay the clip from playing.</param>
        /// <param name="reservedIndex">The index of the component that should be played. -1 indicates any component.</param>
        /// <returns>The AudioSource that is playing the AudioClip (can be null).</returns>
        protected virtual AudioSource PlayInternal(GameObject gameObject, AudioClip clip, float pitch, bool loop, float delay, int reservedIndex)
        {
            if (clip == null) {
                return null;
            }

            AudioSourcesIndex audioSourcesIndex;
            AudioSource audioSource = null;
            if (gameObject != null && gameObject.activeInHierarchy) {
                if (!m_GameObjectAudioSourcesMap.TryGetValue(gameObject, out audioSourcesIndex)) {
                    RegisterInternal(gameObject, 1);
                    audioSourcesIndex = m_GameObjectAudioSourcesMap[gameObject];
                }
                audioSource = audioSourcesIndex.GetAvailableAudioSource(reservedIndex);
            } else {
                // If a GameObject is disabled then it can't play the AudioSource. Use the scene AudioSource which will always be active.
                var sceneAudioSourcesIndex = m_GameObjectAudioSourcesMap[m_GameObject];
                audioSource = sceneAudioSourcesIndex.GetAvailableAudioSource(reservedIndex, sceneAudioSourcesIndex.AudioSources[0], true);
                // The position may have been changed by PlayAtPosition.
                audioSource.transform.localPosition = Vector3.zero;
            }

            // Play the clip.
            audioSource.clip = clip;
            audioSource.pitch = pitch;
            audioSource.loop = loop;
            if (delay == 0) {
                audioSource.Play();
            } else {
                audioSource.PlayDelayed(delay);
            }
            return audioSource;
        }

        /// <summary>
        /// Plays the audio clip at the specified position.
        /// </summary>
        /// <param name="clip">The clip that should be played.</param>
        /// <param name="position">The position that the clip should be played at.</param>
        /// <returns>The AudioSource that is playing the AudioClip (can be null).</returns>
        public static AudioSource PlayAtPosition(AudioClip clip, Vector3 position)
        {
            return Instance.PlayAtPositionInternal(clip, position, 1, 1);
        }

        /// <summary>
        /// Plays the audio clip at the specified position.
        /// </summary>
        /// <param name="clip">The clip that should be played.</param>
        /// <param name="position">The position that the clip should be played at.</param>
        /// <param name="volume">The volume to play the clip at.</param>
        /// <param name="pitch">The pitch to play the clip at.</param>
        /// <returns>The AudioSource that is playing the AudioClip (can be null).</returns>
        public static AudioSource PlayAtPosition(AudioClip clip, Vector3 position, float volume, float pitch)
        {
            return Instance.PlayAtPositionInternal(clip, position, volume, pitch);
        }

        /// <summary>
        /// Internal method which plays the audio clip at the specified position.
        /// </summary>
        /// <param name="clip">The clip that should be played.</param>
        /// <param name="position">The position that the clip should be played at.</param>
        /// <param name="volume">The volume to play the clip at.</param>
        /// <param name="pitch">The pitch to play the clip at.</param>
        /// <returns>The AudioSource that is playing the AudioClip (can be null).</returns>
        protected virtual AudioSource PlayAtPositionInternal(AudioClip clip, Vector3 position, float volume, float pitch)
        {
            var sceneAudioSourcesIndex = m_GameObjectAudioSourcesMap[m_GameObject];
            var audioSource = sceneAudioSourcesIndex.GetAvailableAudioSource(-1, sceneAudioSourcesIndex.AudioSources[0], true);
            audioSource.transform.position = position;

            // Play the clip.
            audioSource.loop = false;
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.spatialBlend = 1;
            audioSource.maxDistance = 500;
            audioSource.Play();

            return audioSource;
        }

        /// <summary>
        /// Stops any playing audio on the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to stop the audio on.</param>
        public static void Stop(GameObject gameObject)
        {
            Instance.StopInternal(gameObject, -1);
        }

        /// <summary>
        /// Stops any playing audio on the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to stop the audio on.</param>
        /// <param name="reservedIndex">The index of the component that should be stopped. -1 indicates all components.</param>
        public static void Stop(GameObject gameObject, int reservedIndex)
        {
            Instance.StopInternal(gameObject, reservedIndex);
        }

        /// <summary>
        /// Internal method which stops any playing audio on the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to stop the audio on.</param>
        /// <param name="reservedIndex">The index of the component that should be stopped. -1 indicates all components.</param>
        private void StopInternal(GameObject gameObject, int reservedIndex)
        {
            AudioSourcesIndex audioSourcesIndex;
            if (!m_GameObjectAudioSourcesMap.TryGetValue(gameObject, out audioSourcesIndex)) {
                return;
            }

            audioSourcesIndex.Stop(reservedIndex);
        }

        /// <summary>
        /// Reset the initialized variable when the scene is no longer loaded.
        /// </summary>
        /// <param name="scene">The scene that was unloaded.</param>
        private void SceneUnloaded(Scene scene)
        {
            s_Initialized = false;
            s_Instance = null;
            SceneManager.sceneUnloaded -= SceneUnloaded;
        }

        /// <summary>
        /// The object has been disabled.
        /// </summary>
        private void OnDisable()
        {
            SceneManager.sceneUnloaded += SceneUnloaded;
        }

#if UNITY_2019_3_OR_NEWER
        /// <summary>
        /// Reset the static variables for domain reloading.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void DomainReset()
        {
            s_Initialized = false;
            s_Instance = null;
        }
#endif
    }
}