/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Damage
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Damageable component, used to take damage, heal and die.
    /// </summary>
    public class Damageable : MonoBehaviour, IDamageable
    {
        public event Action OnHpChanged;
        public event Action<Damage> OnTakeDamage;
        public event Action<int> OnHeal;
        public event Action OnDie;
        
        [Tooltip("The Max amount of Hp.")]
        [SerializeField] protected int m_MaxHp=100;
        [Tooltip("The starting HP amount.")]
        [SerializeField] protected int m_CurrentHp=50;
        [Tooltip("The time in the which the damageable is invincible after getting hit.")]
        [SerializeField] protected float m_InvincibilityTime;
        [Tooltip("Disable the object on death?.")]
        [SerializeField] protected bool m_DisableOnDeath;

        protected double m_LastHitTime;
        
        public virtual int MaxHp => m_MaxHp;
        public virtual int CurrentHp => m_CurrentHp;
        
        public virtual bool CanTakDamage => m_LastHitTime + m_InvincibilityTime <= Time.timeSinceLevelLoad;

        /// <summary>
        /// Initialize.
        /// </summary>
        private void Start()
        {
            m_CurrentHp = Mathf.Clamp(m_CurrentHp,1,m_MaxHp);
            OnHpChanged?.Invoke();
        }

        /// <summary>
        /// Take Damage.
        /// </summary>
        /// <param name="amount">The amount.</param>
        public void TakeDamage(int amount)
        {
            TakeDamage((amount,Vector3.zero));
        }

        /// <summary>
        /// Take damage.
        /// </summary>
        /// <param name="damage">The damage information.</param>
        public virtual void TakeDamage(Damage damage)
        {
            if (CanTakDamage==false) { return; }

            var amount = damage.Amount;
            if (amount < 0) { amount = 0; }

            m_CurrentHp = Mathf.Clamp(CurrentHp - amount, 0, MaxHp);

            m_LastHitTime = Time.timeSinceLevelLoad;
      
            OnTakeDamage?.Invoke((amount,damage));
            OnHpChanged?.Invoke();

            if (m_CurrentHp == 0) {
                Die();
            }
        }

        /// <summary>
        /// Heal amount.
        /// </summary>
        /// <param name="amount">The amount.</param>
        public virtual void Heal(int amount)
        {
            amount = Mathf.Clamp(amount, 0, MaxHp-CurrentHp);
            m_CurrentHp = Mathf.Clamp(CurrentHp + amount, 0, MaxHp);
            
            OnHeal?.Invoke(amount);
            OnHpChanged?.Invoke();
        }

        /// <summary>
        /// Die.
        /// </summary>
        public virtual void Die()
        {
            if (m_DisableOnDeath) {
                gameObject.SetActive(false);
            }
            OnDie?.Invoke();
        }
    }
}