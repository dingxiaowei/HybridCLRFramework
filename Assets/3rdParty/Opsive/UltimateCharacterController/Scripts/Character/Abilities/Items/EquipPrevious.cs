/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Character.Abilities.Items
{
    /// <summary>
    /// The EquipPrevious ability will equip the previous ItemSet in the specified category.
    /// </summary>
    [DefaultStartType(AbilityStartType.ButtonDown)]
    [DefaultInputName("Equip Previous Item")]
    [AllowMultipleAbilityTypes]
    public class EquipPrevious : EquipSwitcher
    {
        private int m_PrevItemSetIndex;
        private int m_ItemSetIndex = -1;

        /// <summary>
        /// The EquipUnequip ability has changed the active ItemSet.
        /// </summary>
        /// <param name="itemSetIndex">The updated active ItemSet index value.</param>
        protected override void OnItemSetIndexChange(int itemSetIndex)
        {
            if (itemSetIndex == -1 || (m_ItemSetIndex != -1 && itemSetIndex == m_ItemSetManager.GetDefaultItemSetIndex(m_ItemSetCategoryIndex))) {
                return;
            }

            m_PrevItemSetIndex = itemSetIndex;
            if (m_ItemSetIndex == -1) {
                m_ItemSetIndex = itemSetIndex;
            }
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            // An attribute may prevent the ability from starting.
            if (!base.CanStartAbility()) {
                return false;
            }

            m_ItemSetIndex = m_ItemSetManager.NextActiveItemSetIndex(m_ItemSetCategoryIndex, m_PrevItemSetIndex, false);

            return m_ItemSetIndex != -1 && m_ItemSetIndex != m_EquipUnequipItemAbility.ActiveItemSetIndex;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            m_EquipUnequipItemAbility.StartEquipUnequip(m_ItemSetIndex);

            // It is up to the EquipUnequip ability to do the actual equip - stop the current ability.
            StopAbility();
        }

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        protected override void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            if (m_Inventory.RemoveAllOnDeath) {
                m_PrevItemSetIndex = m_ItemSetIndex = -1;
            }
        }
    }
}