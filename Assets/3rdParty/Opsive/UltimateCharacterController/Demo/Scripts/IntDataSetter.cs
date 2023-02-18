/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Character.Abilities
{
    using Opsive.Shared.Events;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using UnityEngine;

    /// <summary>
    /// Sets the int data parameter to the specified value when move towards is not active.
    /// </summary>
    public class IntDataSetter : Ability
    {
        [Tooltip("The value to set the int data value to.")]
        [SerializeField] protected int m_IntDataValue = 1;
        [Tooltip("Should the ability stop when the Move Towards ability is active?")]
        [SerializeField] protected bool m_StopWhenMoveTowardsActive = true;

        public int IntDataValue { get { return m_IntDataValue; } set { m_IntDataValue = value; } }
        public bool StopWhenMoveTowardsActive { get { return m_StopWhenMoveTowardsActive; } set { m_StopWhenMoveTowardsActive = value; } }

        public override bool IsConcurrent { get { return true; } }
        public override int AbilityIntData { get { return m_IntDataValue; } set { m_IntDataValue = value; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            EventHandler.RegisterEvent<Ability, bool>(m_GameObject, "OnCharacterAbilityActive", OnAbilityActive);
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            return m_CharacterLocomotion.MoveTowardsAbility == null || !m_CharacterLocomotion.MoveTowardsAbility.IsActive;
        }

        /// <summary>
        /// The character's ability has been started or stopped.
        /// </summary>
        /// <param name="ability">The ability which was started or stopped.</param>
        /// <param name="active">True if the ability was started, false if it was stopped.</param>
        private void OnAbilityActive(Ability ability, bool active)
        {
            if (m_StopWhenMoveTowardsActive && active && ability is MoveTowards) {
                StopAbility();
            }
        }

        /// <summary>
        /// The character has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            EventHandler.UnregisterEvent<Ability, bool>(m_GameObject, "OnCharacterAbilityActive", OnAbilityActive);
        }
    }
}