/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Game
{
   using Dypsloom.DypThePenguin.Scripts.Damage;
   using UnityEngine;

   /// <summary>
   /// Heal effect when the damageable heals.
   /// </summary>
   public class HealEffect : MonoBehaviour
   {
      [Tooltip("The damageable.")]
      [SerializeField] protected Damageable m_Damageable;
      [Tooltip("The animator.")]
      [SerializeField] protected Animator m_Animator;
      [Tooltip("The effect..")]
      [SerializeField] protected ParticleSystem m_Effect;
   
      private static readonly int s_Heal = Animator.StringToHash("Heal");

      /// <summary>
      /// Cache components.
      /// </summary>
      private void Awake()
      {
         if (m_Damageable == null) { m_Damageable = GetComponent<Damageable>(); }
         if (m_Animator == null) { m_Animator = GetComponent<Animator>(); }

         m_Damageable.OnHeal += OnHeal;
      }

      /// <summary>
      /// Play effects and animation when healing.
      /// </summary>
      /// <param name="healAmount">The heal amount.</param>
      private void OnHeal(int healAmount)
      {
         if (m_Animator != null) {
            m_Animator.SetTrigger(s_Heal);
         }
         
         if (m_Effect != null) {
            m_Effect.Play();
         }
      }
   }
}
