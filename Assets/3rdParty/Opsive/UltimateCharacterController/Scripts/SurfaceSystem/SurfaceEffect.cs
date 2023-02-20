/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Audio;
using Opsive.UltimateCharacterController.StateSystem;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.SurfaceSystem
{
    /// <summary>
    /// Specifies a recipe for effects that can be spawned in response to a certain type of collision. This collision might occur when
    /// bullets hit a wall, a character places a footprint, or the character falls to the ground.
    /// </summary>
    public class SurfaceEffect : ScriptableObject
    {
        [Tooltip("Objects that should be spawned during a collision.")]
        [SerializeField] protected ObjectSpawnInfo[] m_SpawnedObjects;
        [Tooltip("An array of decals that can be spawned. A single decal will be randomlly chosen when the SurfaceEffect should spawn.")]
        [SerializeField] protected GameObject[] m_Decals;
        [Tooltip("The minimum scale of the decal.")]
        [SerializeField] protected float m_MinDecalScale = 1;
        [Tooltip("The maximum scale of the decal.")]
        [SerializeField] protected float m_MaxDecalScale = 1;
        [Tooltip("How close to the edge the decal is allowed to spawn. A value of 0 requires the full quad is required to sit on the background surface.")]
        [Range(0, 0.5f)] [SerializeField] protected float m_AllowedDecalEdgeOverlap = 0.25f;
        [Tooltip("The AudioClips that can be triggered from a collision.")]
        [SerializeField] protected AudioClip[] m_AudioClips;
        [Tooltip("The minimum volume of the AudioClip.")]
        [SerializeField] protected float m_MinAudioVolume = 1;
        [Tooltip("The maximum volume of the AudioClip.")]
        [SerializeField] protected float m_MaxAudioVolume = 1;
        [Tooltip("The minimum pitch of the AudioClip.")]
        [SerializeField] protected float m_MinAudioPitch = 1;
        [Tooltip("The maximum pitch of the AudioClip.")]
        [SerializeField] protected float m_MaxAudioPitch = 1;
        [Tooltip("Should a maximum of one clip be played per frame? This prevents too many sounds from playing at once.")]
        [SerializeField] protected bool m_OneClipPerFrame;
        [Tooltip("Should the AudioClip be randomly selected? If false the clips will be played sequentially.")]
        [SerializeField] protected bool m_RandomClipSelection = true;
        [Tooltip("The name of the state that should be activated upon impact.")]
        [SerializeField] protected string m_StateName;
        [Tooltip("The number of seconds until the specified state is disabled. A value of -1 will require the state to be disabled manually.")]
        [SerializeField] protected float m_StateDisableTimer = 10;

        public ObjectSpawnInfo[] SpawnedObjects { get { return m_SpawnedObjects; } set { m_SpawnedObjects = value; } }
        public GameObject[] Decals { get { return m_Decals; } set { m_Decals = value; } }
        public float MinDecalScale { get { return m_MinDecalScale; } set { m_MinDecalScale = value; } }
        public float MaxDecalScale { get { return m_MaxDecalScale; } set { m_MaxDecalScale = value; } }
        public float AllowedDecalEdgeOverlap { get { return m_AllowedDecalEdgeOverlap; } set { m_AllowedDecalEdgeOverlap = value; } }
        public AudioClip[] AudioClips { get { return m_AudioClips; } set { m_AudioClips = value; } }
        public float MinAudioVolume { get { return m_MinAudioVolume; } set { m_MinAudioVolume = value; } }
        public float MaxAudioVolume { get { return m_MaxAudioVolume; } set { m_MaxAudioVolume = value; } }
        public float MinAudioPitch { get { return m_MinAudioPitch; } set { m_MinAudioPitch = value; } }
        public float MaxAudioPitch { get { return m_MaxAudioPitch; } set { m_MaxAudioPitch = value; } }
        public bool OneClipPerFrame { get { return m_OneClipPerFrame; } set { m_OneClipPerFrame = value; } }
        public bool RandomClipSelection { get { return m_RandomClipSelection; } set { m_RandomClipSelection = value; } }
        public string StateName { get { return m_StateName; } set { m_StateName = value; } }
        public float StateDisableTimer { get { return m_StateDisableTimer; } set { m_StateDisableTimer = value; } }

        private GameObject m_LastSpawnedDecal;
        private AudioClip m_LastPlayedAudioClip;
        private int m_LastPlayedAudioClipFrame;
        private int m_AudioClipIndex;

        /// <summary>
        /// Spawns the surface objects and decals.
        /// </summary>
        /// <param name="hit">The RaycastHit which caused the collision.</param>
        /// <param name="gravityDirection">The normalized direction of the character's gravity.</param>
        /// <param name="timeScale">The timescale of the originator.</param>
        /// <param name="originator">The object which spawned the effect.</param>
        /// <param name="spawnDecals">Should the decals be spawned? Not all surfaces allow for decals.</param>
        public void Spawn(RaycastHit hit, Vector3 gravityDirection, float timeScale, GameObject originator, bool spawnDecals)
        {
            SpawnObjects(hit, gravityDirection);

            PlayAudioClip(hit, timeScale, null);

            SetState(hit);

            // Return early if the SurfaceType doesn't allow decals.
            if (!spawnDecals) {
                return;
            }

            SpawnDecal(hit);
        }

        /// <summary>
        /// Instantiates the Spawned Objects.
        /// </summary>
        /// <param name="hit">The RaycastHit which caused the collision.</param>
        /// <param name="gravityDirection">The normalized direction of the character's gravity.</param>
        private void SpawnObjects(RaycastHit hit, Vector3 gravityDirection)
        {
            for (int i = 0; i < m_SpawnedObjects.Length; ++i) {
                if (m_SpawnedObjects[i] == null) {
                    continue;
                }

                m_SpawnedObjects[i].Instantiate(hit.point, hit.normal, gravityDirection);
            }
        }

        /// <summary>
        /// Instantiates a decal.
        /// </summary>
        /// <param name="hit">The RaycastHit which caused the collision.</param>
        private void SpawnDecal(RaycastHit hit)
        {
            var decal = GetDecal();
            // The last spawned decal should be remembered so no two decals are spawned immediately after one another if multiple decals can be spawned.
            m_LastSpawnedDecal = decal;
            DecalManager.Spawn(decal, hit, Random.Range(m_MinDecalScale, m_MaxDecalScale), m_AllowedDecalEdgeOverlap);
        }

        /// <summary>
        /// Returns a random decal from the decals array.
        /// </summary>
        /// <returns>A random decal from the decals array.</returns>
        private GameObject GetDecal()
        {
            if (m_Decals.Length == 0) {
                return null;
            }

            // A random decal should be chosen from the array.
            var decal = m_Decals[Random.Range(0, m_Decals.Length)];
            if (decal == null) {
                return null;
            }

            // If there are multiple decals available then the same decal shouldn't spawn twice in a row.
            while (m_Decals.Length > 1 && decal == m_LastSpawnedDecal) {
                decal = m_Decals[Random.Range(0, m_Decals.Length)];
                if (decal == null) {
                    return null;
                }
            }

            return decal;
        }

        /// <summary>
        /// A footprint and any related effects should be spawned at the hit point.
        /// </summary>
        /// <param name="hit">The RaycastHit which caused the collision.</param>
        /// <param name="gravityDirection">The normalized direction of the character's gravity.</param>
        /// <param name="timeScale">The timescale of the originator.</param>
        /// <param name="originator">The object which spawned the effect.</param>
        /// <param name="spawnDecals">Should the decals be spawned? Not all surfaces allow for decals.</param>
        /// <param name="footprintDirection">The direction that the footprint decal should face.</param>
        /// <param name="flipFootprint">Should the footprint decal be flipped?</param>
        public void SpawnFootprint(RaycastHit hit, Vector3 gravityDirection, float timeScale, GameObject originator, bool spawnDecals, Vector3 footprintDirection, bool flipFootprint)
        {
            SpawnObjects(hit, gravityDirection);

            PlayAudioClip(hit, timeScale, originator);

            SetState(hit);

            // Return early if the SurfaceType doesn't allow decals.
            if (!spawnDecals) {
                return;
            }

            // The DecalManager will do the footprint spawn.
            DecalManager.SpawnFootprint(GetDecal(), hit, Random.Range(m_MinDecalScale, m_MaxDecalScale), m_AllowedDecalEdgeOverlap, footprintDirection, flipFootprint);
        }

        /// <summary>
        /// Plays an AudioClip on the specified GameObject.
        /// </summary>
        /// <param name="hit">The RaycastHit which caused the collision.</param>
        /// <param name="timeScale">The timescale of the originator.</param>
        /// <param name="originator">The object which should play the audio clip.</param>
        private void PlayAudioClip(RaycastHit hit, float timeScale, GameObject originator)
        {
            // No clips can be played if there are no clips in the array.
            if (m_AudioClips == null || m_AudioClips.Length == 0) {
                return;
            }

            // Don't play too many clips at once.
            if (m_OneClipPerFrame && m_LastPlayedAudioClipFrame == Time.frameCount) {
                return;
            }

            // Get the AudioClip.
            AudioClip audioClip;
            if (m_RandomClipSelection) {
                audioClip = m_AudioClips[Random.Range(0, m_AudioClips.Length)];
            } else {
                audioClip = m_AudioClips[m_AudioClipIndex];
                m_AudioClipIndex = (m_AudioClipIndex + 1) % m_AudioClips.Length;
            }

            // If there are multiple clips available then the same clip shouldn't be played twice in a row.
            while (m_AudioClips.Length > 1 && audioClip == m_LastPlayedAudioClip) {
                audioClip = m_AudioClips[Random.Range(0, m_AudioClips.Length)];
                if (audioClip == null) {
                    return;
                }
            }

            // Play the clip.
            var pitch = Random.Range(m_MinAudioPitch, m_MaxAudioPitch) * Time.timeScale * timeScale;
            if (originator != null) {
                AudioManager.Play(originator, audioClip, pitch);
            } else {
                var volume = Random.Range(m_MinAudioVolume, m_MaxAudioVolume);
                AudioManager.PlayAtPosition(audioClip, hit.point, volume, pitch);
            }
            // Update the state.
            m_LastPlayedAudioClip = audioClip;
            m_LastPlayedAudioClipFrame = Time.frameCount;
        }

        /// <summary>
        /// Sets the specified state on the hit object.
        /// </summary>
        /// <param name="hit">The RaycastHit which caused the collision.</param>
        private void SetState(RaycastHit hit)
        {
            if (string.IsNullOrEmpty(m_StateName)) {
                return;
            }

            var hitGameObject = hit.transform.gameObject;
            StateManager.SetState(hitGameObject, m_StateName, true);

            // If the timer isn't -1 then the state should be disabled after a specified amount of time. If it is -1 then the state
            // will have to be disabled manually.
            if (m_StateDisableTimer != -1) {
                StateManager.DeactivateStateTimer(hitGameObject, m_StateName, m_StateDisableTimer);
            }
        }
    }
}