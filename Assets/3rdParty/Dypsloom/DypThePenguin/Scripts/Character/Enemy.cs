/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Character
{
    using Dypsloom.DypThePenguin.Scripts.Damage;
    using Dypsloom.DypThePenguin.Scripts.Game;
    using Dypsloom.Shared.Utility;
    using UnityEngine;

    public class Enemy : MonoBehaviour
    {
        [Tooltip("Look at the player when in view range.")]
        [SerializeField] protected bool m_LookAtThePlayer = true;
        [Tooltip("The distance at which the enemy will start seeing the character.")]
        [SerializeField] protected float m_ViewDistance = 15;
        [Tooltip("The distance at which the enemy will start attacking the player.")]
        [SerializeField] protected float m_AttackDistance = 8;
        [Tooltip("The Animator.")]
        [SerializeField] protected Animator m_Animator;
        [Tooltip("Death Effect.")]
        [SerializeField] protected GameObject m_DeathEffects;
        
        private static readonly int s_DeadAnimHash = Animator.StringToHash("Dead");
        
        protected Damageable m_Damageable;
        protected Character m_PlayerCharacter;
        protected Transform m_PlayerTransform;

        protected bool m_Dead;

        protected IAutoAttack m_AutoAttack;

        /// <summary>
        /// Cache the components.
        /// </summary>
        private void Awake()
        {
            m_PlayerCharacter = FindObjectOfType<Character>();
            m_PlayerTransform = m_PlayerCharacter?.transform;

            m_Animator = GetComponent<Animator>();

            m_AutoAttack = GetComponent<IAutoAttack>();
            
            m_Damageable = GetComponent<Damageable>();
            m_Damageable.OnDie += Die;
        }

        /// <summary>
        /// Reset the enemy if it was pooled.
        /// </summary>
        private void OnEnable()
        {
            m_Dead = false;
            m_Animator.SetBool(s_DeadAnimHash, false);
            m_DeathEffects?.SetActive(false);
        }

        /// <summary>
        /// Kill the enemy.
        /// </summary>
        private void Die()
        {
            if(m_Dead){ return; }
            
            GameManager.Instance.EnemyDied();
            m_Dead = true;
            m_Animator.SetBool(s_DeadAnimHash, true);
            m_AutoAttack?.StopAutoAttack();
            
            SchedulerManager.Schedule(() => m_DeathEffects?.SetActive(true), 1.1f);
        }

        /// <summary>
        /// Check the distance to the character to see if it can attack it.
        /// </summary>
        private void Update()
        {
            if(m_Dead){ return; }
            if(m_PlayerTransform == null){ return; }
            
            var distance = Vector3.Distance(transform.position, m_PlayerTransform.position);

            if (m_AutoAttack != null) {
                
                if (distance <= m_AttackDistance) {
              
                    if(m_AutoAttack.IsAttacking == false)
                    {
                        m_AutoAttack?.StartAutoAttack();
                    }
                
                }else if (m_AutoAttack.IsAttacking) {
              
                    m_AutoAttack?.StopAutoAttack();
                }
            }
            

            if(m_LookAtThePlayer == false || distance > m_ViewDistance){ return;}

            var playerXZPosition = new Vector3(m_PlayerCharacter.transform.position.x,transform.position.y, m_PlayerCharacter.transform.position.z);
            
            transform.LookAt(playerXZPosition);
        }
    }
}
