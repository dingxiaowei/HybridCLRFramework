/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Damage
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// A component that spawns projectile repetitively. 
    /// </summary>
    public class AutomaticProjectileSpawner : ProjectileSpawner, IAutoAttack
    {
        [Tooltip("Start auto firing at the start of the game.")]
        [SerializeField] private bool m_AutoFireOnStart = true;
        [Tooltip("The time elapsed between each shot.")]
        [SerializeField] private float m_FirePeriod = 1;

        protected Coroutine m_FireCoroutine;
        protected WaitForSeconds m_WaitPeriod;
        private bool m_IsAttacking = false;

        public bool IsAttacking => m_IsAttacking;

        /// <summary>
        /// Start auto firing.
        /// </summary>
        void Start()
        {
            m_WaitPeriod = new WaitForSeconds(m_FirePeriod);
            if (m_AutoFireOnStart) {
                StartAutoAttack();
            }
        }
        
        /// <summary>
        /// Start the coroutine.
        /// </summary>
        public void StartAutoAttack()
        {
            if (m_FireCoroutine != null) { return; }

            m_IsAttacking = true;

            m_FireCoroutine = StartCoroutine(AutomaticFire());
        }
    
        /// <summary>
        /// Stop the coroutine.
        /// </summary>
        public void StopAutoAttack()
        {
            StopCoroutine(m_FireCoroutine);
            m_FireCoroutine = null;
            m_IsAttacking = false;
        }

        /// <summary>
        /// The coroutine.
        /// </summary>
        /// <returns>The IEnumerator.</returns>
        private IEnumerator AutomaticFire()
        {
            while (true) {
                yield return m_WaitPeriod;
                Shoot();
            }
        }

        /// <summary>
        /// Stop attacking on disable
        /// </summary>
        private void OnDisable()
        {
            m_IsAttacking = false;
        }
    }
}