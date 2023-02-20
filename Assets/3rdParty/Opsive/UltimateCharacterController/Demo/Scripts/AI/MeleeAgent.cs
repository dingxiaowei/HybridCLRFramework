/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.Abilities.Items;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Demo.AI
{
    /// <summary>
    /// An extremely simple AI agent that will attack at a fixed interval.
    /// </summary>
    public class MeleeAgent : MonoBehaviour
    {
        [Tooltip("The interval that the agent should attack.")]
        [SerializeField] protected MinMaxFloat m_AttackInterval = new MinMaxFloat(2, 4);
        [Tooltip("The target must be within the specified distance before the agent can attack.")]
        [SerializeField] protected float m_TargetDistance = 3;
        [Tooltip("Attack immediately if the character is within the close distance.")]
        [SerializeField] protected float m_ImmediateAttackDistance = 1.5f;
        [Tooltip("The delay between immediate attacks to prevent the agent from attacking too often.")]
        [SerializeField] protected float m_ImmediateAttackDelay = 0.75f;

        private Transform m_Transform;
        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private AgentMovement m_AgentMovement;
        private Use m_UseAbility;
        private LocalLookSource m_LocalLookSource;
        private float m_AttackTime;
        private float m_NextAttackTime;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Start()
        {
            m_Transform = transform;
            m_CharacterLocomotion = GetComponent<UltimateCharacterLocomotion>();
            m_AgentMovement = m_CharacterLocomotion.GetAbility<AgentMovement>();
            m_UseAbility = m_CharacterLocomotion.GetAbility<Use>();
            m_LocalLookSource = GetComponent<LocalLookSource>();

            m_AgentMovement.Enabled = false;
            enabled = false;
        }

        /// <summary>
        /// Attacks the target when within distance.
        /// </summary>
        public void Update()
        {
            var attack = false;
            var distance = (m_LocalLookSource.Target.position - m_Transform.position).sqrMagnitude;
            if (m_AttackTime + m_ImmediateAttackDelay < Time.time && distance < m_ImmediateAttackDistance * m_ImmediateAttackDistance) {
                attack = true;
            } else if (m_NextAttackTime < Time.time && distance < m_TargetDistance * m_TargetDistance) {
                attack = true;
            }

            if (attack) {
                m_CharacterLocomotion.TryStartAbility(m_UseAbility);
                m_AttackTime = Time.time;
                m_NextAttackTime = Time.time + m_AttackInterval.RandomValue;
            }
        }

        /// <summary>
        /// Starts the attack.
        /// </summary>
        public void Attack()
        {
            // The agent should be able to move while attacking.
            if (!m_AgentMovement.Enabled) {
                m_AgentMovement.Enabled = true;
            }
            enabled = true;
            m_NextAttackTime = Time.time + m_AttackInterval.RandomValue;
        }

        /// <summary>
        /// Cancels the attack.
        /// </summary>
        public void CancelAttack()
        {
            m_AgentMovement.Enabled = false;
            enabled = false;
        }
    }
}