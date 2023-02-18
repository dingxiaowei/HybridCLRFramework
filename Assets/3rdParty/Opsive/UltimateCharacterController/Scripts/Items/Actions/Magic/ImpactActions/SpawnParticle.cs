/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Magic.ImpactActions
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Spawns a ParticleSystem upon impact.
    /// </summary>
    public class SpawnParticle : ImpactAction
    {
        [Tooltip("The particle prefab that should be spawned.")]
        [SerializeField] protected GameObject m_ParticlePrefab;
        [Tooltip("The positional offset that the particle should be spawned.")]
        [SerializeField] protected Vector3 m_PositionOffset;
        [Tooltip("The rotational offset that the particle should be spawned.")]
        [SerializeField] protected Vector3 m_RotationOffset;
        [Tooltip("Should the particle be parented to the object that was hit by the cast?")]
        [SerializeField] protected bool m_ParentToImpactedObject;

        public GameObject ParticlePrefab { get { return m_ParticlePrefab; } set { m_ParticlePrefab = value; } }
        public Vector3 PositionOffset { get { return m_PositionOffset; } set { m_PositionOffset = value; } }
        public Vector3 RotationOffset { get { return m_RotationOffset; } set { m_RotationOffset = value; } }
        public bool ParentToImpactedObject { get { return m_ParentToImpactedObject; } set { m_ParentToImpactedObject = value; } }

        private Dictionary<uint, ParticleSystem> m_CastIDParticleMap = new Dictionary<uint, ParticleSystem>();

        /// <summary>
        /// Perform the impact action.
        /// </summary>
        /// <param name="castID">The ID of the cast.</param>
        /// <param name="source">The object that caused the cast.</param>
        /// <param name="target">The object that was hit by the cast.</param>
        /// <param name="hit">The raycast that caused the impact.</param>
        protected override void ImpactInternal(uint castID, GameObject source, GameObject target, RaycastHit hit)
        {
            if (m_ParticlePrefab == null) {
                Debug.LogError("Error: A Particle Prefab must be specified.", m_MagicItem);
                return;
            }

            var rotation = Quaternion.LookRotation(hit.normal) * Quaternion.Euler(m_RotationOffset);
            var position = MathUtility.TransformPoint(hit.point, rotation, m_PositionOffset);

            if (m_CastIDParticleMap.TryGetValue(castID, out var existingParticleSystem)) {
                existingParticleSystem.transform.position = position;
                existingParticleSystem.transform.rotation = rotation;
                return;
            }

            var obj = ObjectPool.Instantiate(m_ParticlePrefab, position, rotation, m_ParentToImpactedObject ? target.transform : null);
            var particleSystem = obj.GetCachedComponent<ParticleSystem>();
            if (particleSystem == null) {
                Debug.LogError($"Error: A Particle System must be specified on the particle {m_ParticlePrefab}.", m_MagicItem);
                return;
            }
            particleSystem.Clear(true);
            m_CastIDParticleMap.Add(castID, particleSystem);
        }

        /// <summary>
        /// Resets the impact action.
        /// </summary>
        /// <param name="castID">The ID of the cast to reset.</param>
        public override void Reset(uint castID)
        {
            base.Reset(castID);

            // Stop the particle system from emitting.
            if (m_CastIDParticleMap.TryGetValue(castID, out var particleSystem)) {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}