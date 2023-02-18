/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Magic.BeginEndActions
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Spawns a particle.
    /// </summary>
    [System.Serializable]
    public class SpawnParticle : BeginEndAction
    {
        [Tooltip("The particle prefab that should be spawned.")]
        [SerializeField] protected GameObject m_ParticlePrefab;
        [Tooltip("The positional offset that the particle should be spawned.")]
        [SerializeField] protected Vector3 m_PositionOffset;
        [Tooltip("The rotational offset that the particle should be spawned.")]
        [SerializeField] protected Vector3 m_RotationOffset;
        [Tooltip("Should the particle be parented to the origin?")]
        [SerializeField] protected bool m_ParentToOrigin;

        public GameObject ParticlePrefab { get { return m_ParticlePrefab; } set { m_ParticlePrefab = value; } }
        public Vector3 PositionOffset { get { return m_PositionOffset; } set { m_PositionOffset = value; } }
        public Vector3 RotationOffset { get { return m_RotationOffset; } set { m_RotationOffset = value; } }
        public bool ParentToOrigin { get { return m_ParentToOrigin; } set { m_ParentToOrigin = value; } }

        private Transform m_Transform;
        private Transform m_SpawnedTransform;

        /// <summary>
        /// Initializes the BeginEndAction.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="magicItem">The MagicItem that the BeginEndAction belongs to.</param>
        /// <param name="startAction">True if the action is a begin action.</param>
        /// <param name="index">The index of the BeginEndAction.</param>
        public override void Initialize(GameObject character, MagicItem magicItem, bool beginAction, int index)
        {
            base.Initialize(character, magicItem, beginAction, index);

            m_Transform = character.transform;
        }

        /// <summary>
        /// The action has started.
        /// </summary>
        /// <param name="origin">The location that the cast originates from.</param>
        public override void Start(Transform origin)
        {
            Spawn(origin);
        }

        /// <summary>
        /// The action has stopped.
        /// </summary>
        /// <param name="origin">The location that the cast originates from.</param>
        public override void Stop()
        {
            m_SpawnedTransform = null;
        }

        /// <summary>
        /// Spawns the particle.
        /// </summary>
        /// <param name="origin">The location that the cast originates from.</param>
        private void Spawn(Transform origin)
        {
            if (m_ParticlePrefab == null) {
                Debug.LogError("Error: A Particle Prefab must be specified.", m_MagicItem);
                return;
            }

            var obj = ObjectPool.Instantiate(m_ParticlePrefab, MathUtility.TransformPoint(origin.position, m_Transform.rotation, m_PositionOffset), 
                                                    origin.rotation * Quaternion.Euler(m_RotationOffset), m_ParentToOrigin ? origin : null);
            m_SpawnedTransform = obj.transform;
            var particleSystem = obj.GetCachedComponent<ParticleSystem>();
            if (particleSystem == null) {
                Debug.LogError($"Error: A Particle System must be specified on the particle {m_ParticlePrefab}.", m_MagicItem);
                return;
            }

            particleSystem.Clear(true);
        }

        /// <summary>
        /// The character has changed perspectives.
        /// </summary>
        /// <param name="origin">The location that the cast originates from.</param>
        public override void OnChangePerspectives(Transform origin) 
        { 
            if (m_SpawnedTransform == null || m_SpawnedTransform.parent == origin) {
                return;
            }

            var localRotation = m_SpawnedTransform.localRotation;
            var localScale = m_SpawnedTransform.localScale;
            m_SpawnedTransform.parent = origin;
            m_SpawnedTransform.position = MathUtility.TransformPoint(origin.position, m_Transform.rotation, m_PositionOffset);
            m_SpawnedTransform.localRotation = localRotation;
            m_SpawnedTransform.localScale = localScale;
        }
    }
}