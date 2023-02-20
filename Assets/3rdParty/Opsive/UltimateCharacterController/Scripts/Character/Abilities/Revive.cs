/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    /// The Revive ability will play a standing animation from laying on the ground. The OnRespawn event will be executed when the animation is complete.
    /// </summary>
    [DefaultStartType(AbilityStartType.Manual)]
    [DefaultState("Death")]
    [DefaultAbilityIndex(5)]
    public class Revive : Ability
    {
        [Tooltip("Should the ability start when the character dies?")]
        [SerializeField] protected bool m_StartOnDeath;
        [Tooltip("Specifies the number of seconds after the character dies that the ability should start.")]
        [SerializeField] protected float m_DeathStartDelay = 3;

        public bool StartOnDeath { get { return m_StartOnDeath; } set { m_StartOnDeath = value; } }
        public float DeathStartDelay { get { return m_DeathStartDelay; } set { m_DeathStartDelay = value; } }

        private Respawner m_Respawner;

        public override bool CanStayActivatedOnDeath { get { return true; } }

        /// <summary>
        /// The type of animation that the ability should play.
        /// </summary>
        private enum ReviveType {
            Forward, // Play a forward revive animation.
            Backward // Play a backward revive animation.
        }

        private int m_ReviveTypeIndex;

        public override int AbilityIntData { get { return m_ReviveTypeIndex; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_Respawner = m_GameObject.GetCachedComponent<Respawner>();
            if (!m_Respawner) {
                Debug.LogError("Error: The Revive ability requires the Respawner component to be added to the character.");
                return;
            }

            // If the ragdoll is starting when the character dies then the respawner will be called manually and should not reposition the character.
            if (m_StartOnDeath && Enabled) {
                m_Respawner.PositioningMode = Respawner.SpawnPositioningMode.None;
                m_Respawner.ScheduleRespawnOnDeath = false;
                m_Respawner.ScheduleRespawnOnDisable = false;
            }

            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorReviveComplete", OnReviveComplete);
        }

        /// <summary>
        /// The character has died. Start the ability if requested.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            // The ability may not need to start from the death event.
            if (!m_StartOnDeath) {
                return;
            }

            m_ReviveTypeIndex = GetReviveTypeIndex(position, force, attacker);
            Scheduler.ScheduleFixed(m_DeathStartDelay, StartRevive);
        }

        /// <summary>
        /// Returns the value that the AbilityIntData parameter should be set to.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        /// <returns>The value that the AbilityIntData parameter should be set to.</returns>
        protected virtual int GetReviveTypeIndex(Vector3 position, Vector3 force, GameObject attacker)
        {
            return (int)(m_Transform.InverseTransformPoint(position).z > 0 ? ReviveType.Forward : ReviveType.Backward);
        }

        /// <summary>
        /// Starts the ability.
        /// </summary>
        private void StartRevive()
        {
            StartAbility();
        }

        /// <summary>
        /// The revive animation has completed.
        /// </summary>
        private void OnReviveComplete()
        {
            StopAbility();

            // The respawn component will perform the necessary cleanup after the character has died.
            if (m_StartOnDeath) {
                m_Respawner.Respawn();
            }
        }

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorReviveComplete", OnReviveComplete);
        }
    }
}