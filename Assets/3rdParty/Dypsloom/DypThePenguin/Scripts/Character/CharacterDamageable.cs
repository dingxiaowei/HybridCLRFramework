/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Character
{
    using Dypsloom.DypThePenguin.Scripts.Damage;
    using System;
    using UnityEngine;

    /// <summary>
    /// Character damageable class used to receive damage.
    /// </summary>
    public class CharacterDamageable : Damageable, IDamageable
    {
        [Tooltip("The character for this damageable component.")]
        [SerializeField] protected Character m_Character;

        /// <summary>
        /// Get the character on awake.
        /// </summary>
        protected virtual void Awake()
        {
            if (m_Character == null) { m_Character = GetComponent<Character>(); }
        }

        /// <summary>
        /// Take damage, can be used to knock back the character.
        /// </summary>
        /// <param name="damage">The damage taken.</param>
        public override void TakeDamage(Damage damage)
        {
            base.TakeDamage(damage);

            if(Math.Abs(damage.Force.sqrMagnitude) < 0.01f){return;}
            
            m_Character.CharacterMover.AddExternalMover(new ImpulseForceMover(damage.Force,1.1f));
            
            m_Character.CharacterAnimator.Damaged(damage);
        }

        /// <summary>
        /// Kill the character.
        /// </summary>
        public override void Die()
        {
            base.Die();
            m_Character.Die();
        }
    }
}