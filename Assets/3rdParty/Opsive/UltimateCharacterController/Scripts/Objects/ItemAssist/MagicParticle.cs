/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects.ItemAssist
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Items.Actions;
    using UnityEngine;

    /// <summary>
    /// The MagicParticle will perform a MagicItem impact when it collides with an object. In order for this to work correctly the ParticleSystem must
    /// have collisions enabled and the "Send Collision Event" parameter enabled. See this page for more information:
    /// https://docs.unity3d.com/Manual/PartSysCollisionModule.html.
    /// </summary>
    public class MagicParticle : MonoBehaviour
    {
        [Tooltip("Can the particle collide with the originator?")]
        [SerializeField] protected bool m_CanCollideWithOriginator = false;

        private GameObject m_GameObject;
        private Transform m_Transform;
        private MagicItem m_MagicItem;
        private uint m_CastID;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_GameObject = gameObject;
            m_Transform = transform;

            var particleSystem = GetComponent<ParticleSystem>();
            if (particleSystem == null) {
                Debug.LogError($"Error: The MagicProjectile {m_GameObject.name} does not have a ParticleSystem attached.");
                return;
            }

            if (!particleSystem.collision.enabled) {
                Debug.LogError($"Error: The collision module on the MagicProjectile {m_GameObject.name} is disabled. This should be enabled in order to receive collision events.");
                return;
            }

            if (!particleSystem.collision.sendCollisionMessages) {
                Debug.LogError($"Error: Send Collision Messages on the the MagicProjectile {m_GameObject.name} is disabled. This should be enabled in order to receive collision events.");
                return;
            }
        }

        /// <summary>
        /// Initializes the particle to the specified MagicItem.
        /// </summary>
        /// <param name="magicItem">The MagicItem that casted the particle.</param>
        /// <param name="castID">The ID of the MagicItem cast.</param>
        public void Initialize(MagicItem magicItem, uint castID)
        {
            m_MagicItem = magicItem;
            m_CastID = castID;
        }

        /// <summary>
        /// A particle has collided with another object.
        /// </summary>
        /// <param name="other">The object that the particle collided with.</param>
        public void OnParticleCollision(GameObject other)
        {
            // If the transform is null the particle hasn't been initialized yet.
            if (m_Transform == null) {
                return;
            }

            // Prevent the particle from colliding with the originator.
            if (!m_CanCollideWithOriginator) {
                var characterLocomotion = other.GetCachedComponent<Character.UltimateCharacterLocomotion>();
                if (characterLocomotion != null && m_MagicItem.Character == characterLocomotion.gameObject) {
                    return;
                }
            }

            // PerformImpact requires a RaycastHit.
            var colliders = other.GetCachedComponents<Collider>();
            if (colliders == null) {
                return;
            }
            for (int i = 0; i < colliders.Length; ++i) {
                if (colliders[i].isTrigger) {
                    continue;
                }
                Vector3 closestPoint;
                if (colliders[i] is BoxCollider || colliders[i] is SphereCollider || colliders[i] is CapsuleCollider || (colliders[i] is MeshCollider && (colliders[i] as MeshCollider).convex)) {
                    closestPoint = colliders[i].ClosestPoint(m_Transform.position);
                } else {
                    closestPoint = m_Transform.position;
                }
                var direction = other.transform.position - closestPoint;
                if (Physics.Raycast(closestPoint - direction.normalized * 0.1f, direction.normalized, out var hit, direction.magnitude + 0.1f, 1 << other.layer)) {
                    m_MagicItem.PerformImpact(m_CastID, m_GameObject, other, hit);
                    break;
                }
            }
        }

        /// <summary>
        /// The particle has been disabled.
        /// </summary>
        private void OnDisable()
        {
            // All of the impact actions should be reset for the particle spawn id.
            if (m_MagicItem != null && m_MagicItem.ImpactActions != null) {
                for (int i = 0; i < m_MagicItem.ImpactActions.Length; ++i) {
                    m_MagicItem.ImpactActions[i].Reset(m_CastID);
                }
            }
        }
    }
}