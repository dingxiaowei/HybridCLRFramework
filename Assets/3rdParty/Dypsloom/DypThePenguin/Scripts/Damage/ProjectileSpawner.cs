/// ---------------------------------------------
/// Dyp Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Damage
{
    using Dypsloom.Shared.Utility;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Component that spawns game objects.
    /// </summary>
    public class ProjectileSpawner : MonoBehaviour
    {
        [Tooltip("The projectile prefab.")]
        [SerializeField] protected GameObject m_ProjectilePrefab;
        [Tooltip("The projectile spawn point.")]
        [SerializeField] protected Transform m_ProjectileSpawnPoint;
        [Tooltip("Animator when Shooting Projectile.")]
        [SerializeField] protected Animator m_Animator;
        [Tooltip("The delay between calling shot and the projectile spawning.")]
        [SerializeField] protected float m_ShotDelay;
        
        private static readonly int s_Throw = Animator.StringToHash("Throw");

        /// <summary>
        /// Shoot the game object.
        /// </summary>
        public virtual void Shoot()
        {
            if(m_Animator != null){ m_Animator.SetTrigger(s_Throw);}

            if (m_ShotDelay <= 0) {
                ShotInternal();
            } else {
                StartCoroutine(DelayedShoot(m_ShotDelay));
            }
        }

        /// <summary>
        /// instantiate the object using a pool.
        /// </summary>
        protected virtual void ShotInternal()
        {
            var projectGameObject = PoolManager.Instantiate(m_ProjectilePrefab, m_ProjectileSpawnPoint.position,m_ProjectileSpawnPoint.rotation);
        }

        /// <summary>
        /// The coroutine.
        /// </summary>
        /// <returns>The IEnumerator.</returns>
        private IEnumerator DelayedShoot(float delay)
        {
            yield return new WaitForSeconds(delay);
            ShotInternal();
        }
    }
}