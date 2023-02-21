/// ---------------------------------------------
/// Dyp Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Damage
{
    using System;
    using UnityEngine;

    /// <summary>
    /// The damageable interface.
    /// </summary>
    public interface IDamageable
    {
        event Action OnHpChanged;
        event Action<Damage> OnTakeDamage;
        event Action<int> OnHeal;
        event Action OnDie;

        GameObject gameObject { get; }

        int MaxHp { get; }
        int CurrentHp { get; }

        void TakeDamage(int amount);
        void TakeDamage(Damage damage);
        
        void Heal(int amount);
        void Die();
    }
}