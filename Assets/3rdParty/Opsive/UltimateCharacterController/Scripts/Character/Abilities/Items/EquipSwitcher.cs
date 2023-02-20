/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;

namespace Opsive.UltimateCharacterController.Character.Abilities.Items
{
    /// <summary>
    /// The EquipSwitcher is an abstract class implemented by the abilities which change the item set.
    /// </summary>
    public abstract class EquipSwitcher : ItemSetAbilityBase
    {
        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            // The EquipUnequip must exist in order for the itemset to be able to be changed.
            if (m_EquipUnequipItemAbility == null) {
                Debug.LogError("Error: The EquipUnequip ItemAbility must be added to the character.");
                Enabled = false;
                return;
            }

            if (m_EquipUnequipItemAbility != null) {
                EventHandler.RegisterEvent<int>(m_EquipUnequipItemAbility, "OnEquipUnequipItemSetIndexChange", OnItemSetIndexChange);
                EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            }
        }

        /// <summary>
        /// The EquipUnequip ability has changed the active ItemSet.
        /// </summary>
        /// <param name="itemSetIndex">The updated active ItemSet index value.</param>
        protected virtual void OnItemSetIndexChange(int itemSetIndex) { }

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        protected virtual void OnDeath(Vector3 position, Vector3 force, GameObject attacker) { }

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            if (m_EquipUnequipItemAbility != null) {
                EventHandler.UnregisterEvent<int>(m_EquipUnequipItemAbility, "OnEquipUnequipItemSetIndexChange", OnItemSetIndexChange);
                EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            }
        }
    }
}