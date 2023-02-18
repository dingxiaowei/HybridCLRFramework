/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Inventory;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// The ItemMonitor will update the UI for the character's items.
    /// </summary>
    public abstract class ItemMonitor : CharacterMonitor
    {
        [Tooltip("A reference to the text used for primary ItemType count.")]
        [SerializeField] protected Text m_PrimaryCount;

        protected InventoryBase m_CharacterInventory;

        /// <summary>
        /// Attaches the monitor to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the monitor to.</param>
        protected override void OnAttachCharacter(GameObject character)
        {
            if (m_Character != null) {
                EventHandler.UnregisterEvent<IItemIdentifier, int, bool, bool>(m_Character, "OnInventoryPickupItemIdentifier", OnPickupItemIdentifier);
                EventHandler.UnregisterEvent<Item, bool>(m_Character, "OnItemUpdateDominantItem", OnUpdateDominantItem);
                EventHandler.UnregisterEvent<Item, IItemIdentifier, int>(m_Character, "OnItemUseConsumableItemIdentifier", OnUseConsumableItemIdentifier);
                EventHandler.UnregisterEvent<IItemIdentifier, int>(m_Character, "OnInventoryAdjustItemIdentifierAmount", OnAdjustItemIdentifierAmount);
            }

            base.OnAttachCharacter(character);

            if (m_Character == null) {
                return;
            }
            
            // The character must have an inventory.
            m_CharacterInventory = m_Character.GetCachedComponent<InventoryBase>();
            if (m_CharacterInventory == null) {
                return;
            }

            EventHandler.RegisterEvent<IItemIdentifier, int, bool, bool>(m_Character, "OnInventoryPickupItemIdentifier", OnPickupItemIdentifier);
            EventHandler.RegisterEvent<Item, bool>(m_Character, "OnItemUpdateDominantItem", OnUpdateDominantItem);
            EventHandler.RegisterEvent<Item, IItemIdentifier, int>(m_Character, "OnItemUseConsumableItemIdentifier", OnUseConsumableItemIdentifier);
            EventHandler.RegisterEvent<IItemIdentifier, int>(m_Character, "OnInventoryAdjustItemIdentifierAmount", OnAdjustItemIdentifierAmount);
        }

        /// <summary>
        /// An ItemIdentifier has been picked up within the inventory.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier that has been picked up.</param>
        /// <param name="amount">The amount of item picked up.</param>
        /// <param name="immediatePickup">Was the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        protected virtual void OnPickupItemIdentifier(IItemIdentifier itemIdentifier, int amount, bool immediatePickup, bool forceEquip) { }

        /// <summary>
        /// The DominantItem field has been updated for the specified item.
        /// </summary>
        /// <param name="item">The Item whose DominantItem field was updated.</param>
        /// <param name="dominantItem">True if the item is now a dominant item.</param>
        protected virtual void OnUpdateDominantItem(Item item, bool dominantItem) { }

        /// <summary>
        /// The specified consumable ItemIdentifier has been used.
        /// </summary>
        /// <param name="item">The Item that has been used.</param>
        /// <param name="itemIdentifier">The ItemIdentifier that has been used.</param>
        /// <param name="amount">The remaining amount of the specified IItemIdentifier.</param>
        protected virtual void OnUseConsumableItemIdentifier(Item item, IItemIdentifier itemIdentifier, int amount) { }

        /// <summary>
        /// The specified ItemIdentifier amount has been adjusted.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to adjust.</param>
        /// <param name="amount">The amount of ItemIdentifier to adjust.</param>
        protected virtual void OnAdjustItemIdentifierAmount(IItemIdentifier itemIdentifier, int amount) { }
    }
}
