/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities.Items
{
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// The ToggleEquip ability will equip or unequip the current ItemSet. ToggleEquip just specifies which ItemSet should be equipped/unequipped and then will let
    /// the EquipUnequip ability to do the actual equip/unequip.
    /// </summary>
    [DefaultStartType(AbilityStartType.ButtonDown)]
    [DefaultInputName("Toggle Item Equip")]
    [AllowDuplicateTypes]
    public class ToggleEquip : EquipSwitcher
    {
        [Tooltip("Should the default ItemSet be toggled upon start?")]
        [SerializeField] protected bool m_ToggleDefaultItemSetOnStart;

        public bool ToggleDefaultItemSetOnStart { get { return m_ToggleDefaultItemSetOnStart; } set { m_ToggleDefaultItemSetOnStart = value; } }

        private int m_PrevItemSetIndex = -1;
        private bool m_ShouldEquipItem = true;

        /// <summary>
        /// Start the ability if the default ItemSet should be equipped.
        /// </summary>
        public override void Start()
        {
            if (m_ToggleDefaultItemSetOnStart) {
                var itemSetIndex = m_ItemSetManager.ActiveItemSetIndex[m_ItemSetCategoryIndex];
                if (itemSetIndex == -1) {
                    m_ShouldEquipItem = false;
                    StartAbility();
                }
            }
        }

        /// <summary>
        /// The EquipUnequip ability has changed the active ItemSet. Store this value so ToggleEquip knows which ItemSet to equip after the unequip.
        /// </summary>
        /// <param name="itemSetIndex">The updated active ItemSet index value.</param>
        protected override void OnItemSetIndexChange(int itemSetIndex)
        {
            if (!Enabled) {
                return;
            }

            var defaultItemSetIndex = m_ItemSetManager.GetDefaultItemSetIndex(m_ItemSetCategoryIndex);
            m_ShouldEquipItem = itemSetIndex == defaultItemSetIndex;
            if (itemSetIndex == defaultItemSetIndex) {
                // The previous ItemSet may have been removed.
                if (!m_ItemSetManager.IsItemSetValid(m_ItemSetCategoryIndex, m_PrevItemSetIndex, false)) {
                    m_PrevItemSetIndex = -1;
                }
                return;
            }
            m_PrevItemSetIndex = itemSetIndex;
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

            // PrevItemSetIndex will equal -1 if no non-default items have been equipped.
            if (m_PrevItemSetIndex == -1) {
                return false;
            }

            return m_PrevItemSetIndex != m_ItemSetManager.GetDefaultItemSetIndex(m_ItemSetCategoryIndex);
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            // Start the EquipUnequip ability and then stop the ability. The EquipUnequip ability will do the actual work of equipping or unequipping the items.
            var defaultItemSetIndex = m_ItemSetManager.GetDefaultItemSetIndex(m_ItemSetCategoryIndex);
            var itemSetIndex = m_ShouldEquipItem ? m_PrevItemSetIndex : defaultItemSetIndex;
            m_EquipUnequipItemAbility.StartEquipUnequip(itemSetIndex, false, false);
            m_ShouldEquipItem = itemSetIndex == defaultItemSetIndex;
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
                m_PrevItemSetIndex = -1;
            }
        }
    }
}