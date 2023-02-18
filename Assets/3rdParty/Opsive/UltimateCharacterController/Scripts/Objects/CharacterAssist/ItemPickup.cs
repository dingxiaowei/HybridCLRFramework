/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects.CharacterAssist
{
    using Opsive.UltimateCharacterController.Inventory;
    using UnityEngine;

    /// <summary>
    /// Extends ItemPickupBase to allow for ItemIdentifier pickups.
    /// </summary>
    public class ItemPickup : ItemPickupBase
    {
        [Tooltip("An array of ItemIdentifiers to be picked up.")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_ItemTypeCounts")]
        [SerializeField] protected ItemDefinitionAmount[] m_ItemDefinitionAmounts;

        /// <summary>
        /// Returns the ItemDefinitionAmount that the ItemPickup contains.
        /// </summary>
        /// <returns>The ItemDefinitionAmount that the ItemPickup contains.</returns>
        public override ItemDefinitionAmount[] GetItemDefinitionAmounts()
        {
            return m_ItemDefinitionAmounts;
        }

        /// <summary>
        /// Sets the ItemPickup ItemDefinitionAmounts value.
        /// </summary>
        /// <param name="itemDefinitionAmounts">The ItemDefinitionAmount that should be set.</param>
        public override void SetItemDefinitionAmounts(ItemDefinitionAmount[] itemDefinitionAmounts)
        {
            m_ItemDefinitionAmounts = itemDefinitionAmounts;
        }

        /// <summary>
        /// Internal method which picks up the ItemIdentifier.
        /// </summary>
        /// <param name="character">The character that should pick up the ItemIdentifier.</param>
        /// <param name="inventory">The inventory belonging to the character.</param>
        /// <param name="slotID">The slot ID that picked up the item. A -1 value will indicate no specified slot.</param>
        /// <param name="immediatePickup">Should the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        /// <returns>True if an ItemIdentifier was picked up.</returns>
        protected override bool DoItemIdentifierPickupInternal(GameObject character, InventoryBase inventory, int slotID, bool immediatePickup, bool forceEquip)
        {
            // Add the ItemIdentifiers to the Inventory. This allows the character to pick up the actual item and any consumable ItemIdentifier (such as ammo).
            var pickedUp = false;
            if (m_ItemDefinitionAmounts != null) {
                for (int i = 0; i < m_ItemDefinitionAmounts.Length; ++i) {
                    if (inventory.Pickup(m_ItemDefinitionAmounts[i].ItemIdentifier, m_ItemDefinitionAmounts[i].Amount, slotID, immediatePickup, forceEquip)) {
                        pickedUp = true;
                    }
                }
            }
            return pickedUp;
        }
    }
}