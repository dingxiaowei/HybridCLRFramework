/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Damage
{
    using Dypsloom.DypThePenguin.Scripts.Items;
    using Dypsloom.Shared.Utility;
    using UnityEngine;

    /// <summary>
    /// Projectile component.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Projectile : MonoBehaviour
    {
        [Tooltip("The projectiles speed in unit per second.")]
        [SerializeField] protected float m_Speed = 1;
        [Tooltip("The damage dealt by the projectile.")]
        [SerializeField] protected int m_Damage = 10;
        [Tooltip("The knock back dealt to the object being hit. (KnockBack works on RigidBodies or Damageables with Movers).")]
        [SerializeField] protected int m_KnockBack = 10;
        [Tooltip("The life time of the projectile before it is returned to the pool.")]
        [SerializeField] protected int m_LifeTime = 5;
        [Tooltip("The layers affected by the projectile.")]
        [SerializeField] protected LayerMask m_LayerMask;
        [Tooltip("The effect that gets spawned on impact.")]
        [SerializeField] protected GameObject m_ImpactPrefab;

        protected float m_StartTime;
        protected IItemUser m_ItemUser;

        /// <summary>
        /// Set the start time.
        /// </summary>
        private void OnEnable()
        {
            m_StartTime = Time.time;
            m_ItemUser = null;
        }

        /// <summary>
        /// Set the item user.
        /// </summary>
        /// <param name="itemUser">The item user.</param>
        public void SetItemUser(IItemUser itemUser)
        {
            m_ItemUser = itemUser;
        }

        /// <summary>
        /// Move the transform.
        /// </summary>
        void Update()
        {
            transform.position += m_Speed * Time.deltaTime * transform.forward;

            if (m_StartTime + m_LifeTime < Time.time) {
                DestroyProjectile();
            }
        }

        /// <summary>
        /// Trigger on enter.
        /// </summary>
        /// <param name="other">The other collider.</param>
        private void OnTriggerEnter(Collider other)
        {
            if(other.isTrigger){return;}
            if(m_LayerMask.Contains(other) == false){return;}
            
            var hitDirection = (other.transform.position-transform.position).normalized;
            var otherRigidBody = other.attachedRigidbody;
            
            var damageable = otherRigidBody?.GetComponent<IDamageable>() ?? other.GetComponent<IDamageable>();
            if (damageable != null && damageable == m_ItemUser?.Character.CharacterDamageable) { return; }
            
            if (damageable != null) {
                damageable.TakeDamage((m_Damage,m_KnockBack*hitDirection));
            }else if (otherRigidBody != null) {
                otherRigidBody.AddForce(hitDirection*m_KnockBack,ForceMode.Impulse);
                if (other.attachedRigidbody.GetComponent<EquippableItem>()) {
                    return;
                }
            }

            if (m_ImpactPrefab != null) {
                PoolManager.Instantiate(m_ImpactPrefab, transform.position, transform.rotation);
            }

            DestroyProjectile();
        }

        /// <summary>
        /// Destroy the projectile.
        /// </summary>
        public void DestroyProjectile()
        {
            PoolManager.Destroy(gameObject);
        }
    }
}