/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities.Items
{
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Networking.Character;
#endif
    using UnityEngine;

    /// <summary>
    /// Abstract ability class which implements item specific logic for the ability system.
    /// </summary>
    public abstract class ItemAbility : Ability
    {
        [Tooltip("Specifies the index of the item state within the animator.")]
        [SerializeField] protected int m_ItemStateIndex = -1;

        public virtual int ItemStateIndex { get { return m_ItemStateIndex; } set { m_ItemStateIndex = value; } }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        protected INetworkCharacter m_NetworkCharacter;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_NetworkCharacter = m_GameObject.GetCachedComponent<INetworkCharacter>();
        }
#endif

        /// <summary>
        /// Returns the Item State Index which corresponds to the slot ID.
        /// </summary>
        /// <param name="slotID">The ID of the slot that corresponds to the Item State Index.</param>
        /// <returns>The Item State Index which corresponds to the slot ID.</returns>
        public virtual int GetItemStateIndex(int slotID)
        {
            return m_ItemStateIndex;
        }

        /// <summary>
        /// Returns the Item Substate Index which corresponds to the slot ID.
        /// </summary>
        /// <param name="slotID">The ID of the slot that corresponds to the Item Substate Index.</param>
        /// <returns>The Item Substate Index which corresponds to the slot ID.</returns>
        public virtual int GetItemSubstateIndex(int slotID)
        {
            return -1;
        }

        /// <summary>
        /// Called when another ability is attempting to start and the current ability is active.
        /// Returns true or false depending on if the new ability should be blocked from starting.
        /// </summary>
        /// <param name="startingAbility">The ability that is starting.</param>
        /// <returns>True if the ability should be blocked.</returns>
        public override bool ShouldBlockAbilityStart(Ability startingAbility)
        {
            // The StartMovement, QuickTurn, and StopMovement abilities should not be started when an ItemAbility is active.
            if (startingAbility is StoredInputAbilityBase) {
                return true;
            }
            return base.ShouldBlockAbilityStart(startingAbility);
        }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        /// <returns>True if the ability should be stopped.</returns>
        public override bool ShouldStopActiveAbility(Ability activeAbility)
        {
            // The StartMovement, QuickTurn, and StopMovement abilities should not be active when an ItemAbility is active.
            if (activeAbility is StoredInputAbilityBase) {
                return true;
            }
            return base.ShouldStopActiveAbility(activeAbility);
        }
    }
}