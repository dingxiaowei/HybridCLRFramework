/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Items;

namespace Opsive.UltimateCharacterController.Character.Abilities.Items
{
    /// <summary>
    /// The EquipNext ability will equip the Next ItemSet in the specified category.
    /// </summary>
    [DefaultStartType(AbilityStartType.ButtonDown)]
    [DefaultInputName("Equip Next Item")]
    [AllowMultipleAbilityTypes]
    public class EquipNext : EquipSwitcher
    {
        private int m_PrevItemSetIndex;
        private int m_ItemSetIndex = -1;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            // The EquipUnequip must exist in order for the item to be able to be equip toggled.
            if (m_EquipUnequipItemAbility == null) {
                return;
            }

            EventHandler.RegisterEvent<Item, bool>(m_GameObject, "OnNextItemSet", OnNextItemSet);
        }

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

            m_ItemSetIndex = m_ItemSetManager.NextActiveItemSetIndex(m_ItemSetCategoryIndex, m_PrevItemSetIndex, true);

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
        /// The next item from the item set should be equipped.
        /// </summary>
        /// <param name="item">The item that should be changed.</param>
        /// <param name="unequipOnFailure">Should the current ItemSet be unequipped if the next ItemSet cannot be activated?</param>
        private void OnNextItemSet(Item item, bool unequipOnFailure)
        {
            var itemType = item.ItemType;
            if (!itemType.CategoryIDMatch(m_ItemSetCategoryID)) {
                return;
            }

            // Don't equip the next item if the current ItemSet doesn't contain the ItemType.
            var activeItemType = m_ItemSetManager.GetEquipItemType(m_ItemSetCategoryIndex, item.SlotID);
            if (itemType != activeItemType) {
                return;
            }

            // Tries to equip the next item. If the ability can't be started then the next item cannot be equipped. If unequip on failure is true
            // and the next item is invalid then no items should be shown.
            if (!StartAbility() && unequipOnFailure) {
                m_EquipUnequipItemAbility.StartEquipUnequip(-1);
            }
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

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            if (m_EquipUnequipItemAbility == null) {
                EventHandler.UnregisterEvent<Item, bool>(m_GameObject, "OnNextItemSet", OnNextItemSet);
            }
        }
    }
}