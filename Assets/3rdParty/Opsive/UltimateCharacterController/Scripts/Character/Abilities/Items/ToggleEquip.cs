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
    /// The ToggleEquip ability will equip or unequip the current ItemSet. ToggleEquip just specifies which ItemSet should be equipped/unequipped and then will let
    /// the EquipUnequip ability to do the actual equip/unequip.
    /// </summary>
    [DefaultStartType(AbilityStartType.ButtonDown)]
    [DefaultInputName("Toggle Item Equip")]
    [AllowMultipleAbilityTypes]
    public class ToggleEquip : EquipSwitcher
    {
        [Tooltip("Should the default ItemSet be toggled upon start?")]
        [SerializeField] protected bool m_ToggleDefaultItemSetOnStart;

        public bool ToggleDefaultItemSetOnStart { get { return m_ToggleDefaultItemSetOnStart; } set { m_ToggleDefaultItemSetOnStart = value; } }

        private int m_PrevItemSetIndex = -1;
        private bool m_ShouldEquipItem = true;
        private bool m_AbilityToggledEquip;
        private bool m_ImmediateEquipUnequip;

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

            EventHandler.RegisterEvent<int, bool, bool>(m_GameObject, "OnAbilityToggleSlots", OnToggleSlots);
        }

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
        /// An ability has started/stopped and should have the items in the specified mask equipped or unequipped.
        /// </summary>
        /// <param name="slotMask">The slots that should be equipped or unequipped.</param>
        /// <param name="equip">True if the items in the specified slot should be equipped.</param>
        /// <param name="forceEquipUnequip">Should the ability be force started? This will stop all abilities that would prevent EquipUnequip from starting.</param>
        private void OnToggleSlots(int slotMask, bool equip, bool immediateEquipUnequip)
        {
            m_ImmediateEquipUnequip = immediateEquipUnequip;
            if (equip) {
                if (m_AbilityToggledEquip) {
                    StartAbility();
                    m_AbilityToggledEquip = false;
                }
            } else {
                // Determine if the ability is responsible for equipping or unequipping the slot at the specified mask.
                var slotCount = m_Inventory.SlotCount;
                for (int i = 0; i < slotCount; ++i) {
                    // Determine if the item should be equpped.
                    if ((slotMask & (1 << i)) != (1 << i)) {
                        var item = m_Inventory.GetItem(i);
                        if (item != null) {
                            // Start unequipping the item.
                            if (item.ItemType.CategoryIDMatch(m_ItemSetCategoryID) && m_ShouldEquipItem == false) {
                                m_AbilityToggledEquip = true;
                                StartAbility();
                                break;
                            }
                        }
                    }
                }
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

            // PrevItemSetIndex will equal -1 if no no-default items have been equipped.
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
            m_EquipUnequipItemAbility.StartEquipUnequip(itemSetIndex, false, m_ImmediateEquipUnequip);
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

        /// <summary>
        /// The character has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            if (m_EquipUnequipItemAbility == null) {
                return;
            }

            EventHandler.UnregisterEvent<int, bool, bool>(m_GameObject, "OnAbilityToggleSlots", OnToggleSlots);
        }
    }
}