/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects.ItemAssist
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Items.Actions;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.UltimateCharacterController.Networking.Game;
#endif
    using Opsive.UltimateCharacterController.Objects;
    using UnityEngine;

    /// <summary>
    /// The ParticleProjectile extends TrajectoryObject and notifies the MagicItem when the object has collided with another object.
    /// TrajectoryObject.CollisionMode should be set to Ignore for the projectile to pass through the object.
    /// </summary>
    public class MagicProjectile : TrajectoryObject
    {
        [Tooltip("Should the projectile be destroyed when there's a collision?")]
        [SerializeField] protected bool m_DestroyOnCollision;
        [Tooltip("Should the projectile be destroyed after the particle has stopped emitting?")]
        [SerializeField] protected bool m_WaitForParticleStop;

        public bool DestroyOnCollision { get { return m_DestroyOnCollision; } set { m_DestroyOnCollision = value; } }
        public bool WaitForParticleStop { get { return m_WaitForParticleStop; } set { m_WaitForParticleStop = value; } }

        protected MagicItem m_MagicItem;
        protected uint m_CastID;

        /// <summary>
        /// Initializes the object with the specified velocity and torque.
        /// </summary>
        /// <param name="velocity">The starting velocity.</param>
        /// <param name="torque">The starting torque.</param>
        /// <param name="originator">The object that instantiated the trajectory object.</param>
        /// <param name="magicItem">The MagicItem that created the projectile.</param>
        /// <param name="castID">The ID of the cast.</param>
        public void Initialize(Vector3 velocity, Vector3 torque, GameObject originator, MagicItem magicItem, uint castID)
        {
            m_MagicItem = magicItem;
            m_CastID = castID;

            Initialize(velocity, torque, originator);
            if (m_Collider != null) {
                m_Collider.enabled = false;
            }
        }

        /// <summary>
        /// The object has collided with another object.
        /// </summary>
        /// <param name="hit">The RaycastHit of the object. Can be null.</param>
        protected override void OnCollision(RaycastHit? hit)
        {
            base.OnCollision(hit);

            if (!hit.HasValue) {
                return;
            }

            m_MagicItem.PerformImpact(m_CastID, m_GameObject, hit.Value.transform.gameObject, hit.Value);

            // Destroys the projectile when it has collided with an object.
            if (m_DestroyOnCollision) {
                // The projectile can wait for any particles to stop emitting.
                var immediateDestroy = !m_WaitForParticleStop;
                if (!immediateDestroy) {
                    var particleSystem = m_GameObject.GetCachedComponent<ParticleSystem>();
                    if (particleSystem != null) {
                        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                        Scheduler.Schedule(particleSystem.main.duration, ReturnToObjectPool);
                        immediateDestroy = false;
                    }
                }
                if (immediateDestroy) {
                    ReturnToObjectPool();
                } else {
                    // The projectile is waiting on the particles to be destroyed. Stop moving.
                    Stop();
                }
            }
        }

        /// <summary>
        /// Returns the projectile back to the object pool.
        /// </summary>
        private void ReturnToObjectPool()
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (NetworkObjectPool.IsNetworkActive()) {
                // The object may have already been destroyed over the network.
                if (!m_GameObject.activeSelf) {
                    return;
                }
                NetworkObjectPool.Destroy(m_GameObject);
                return;
            }
#endif
            ObjectPool.Destroy(m_GameObject);
        }
    }
}