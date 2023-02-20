/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.UI
{
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
                EventHandler.UnregisterEvent<ItemType, float, bool, bool>(m_Character, "OnInventoryPickupItemType", OnPickupItemType);
                EventHandler.UnregisterEvent<Item, bool>(m_Character, "OnItemUpdateDominantItem", OnUpdateDominantItem);
                EventHandler.UnregisterEvent<Item, ItemType, float>(m_Character, "OnItemUseConsumableItemType", OnUseConsumableItemType);
                EventHandler.UnregisterEvent<ItemType, float>(m_Character, "OnInventoryUseItemType", OnUseItemType);
                EventHandler.UnregisterEvent<Item, int>(m_Character, "OnAbilityUnequipItemComplete", OnUnequipItem);
                EventHandler.UnregisterEvent<Item, int>(m_Character, "OnInventoryRemoveItem", OnUnequipItem);
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

            EventHandler.RegisterEvent<ItemType, float, bool, bool>(m_Character, "OnInventoryPickupItemType", OnPickupItemType);
            EventHandler.RegisterEvent<Item, bool>(m_Character, "OnItemUpdateDominantItem", OnUpdateDominantItem);
            EventHandler.RegisterEvent<Item, ItemType, float>(m_Character, "OnItemUseConsumableItemType", OnUseConsumableItemType);
            EventHandler.RegisterEvent<ItemType, float>(m_Character, "OnInventoryUseItemType", OnUseItemType);
            EventHandler.RegisterEvent<Item, int>(m_Character, "OnAbilityUnequipItemComplete", OnUnequipItem);
            EventHandler.RegisterEvent<Item, int>(m_Character, "OnInventoryRemoveItem", OnUnequipItem);
        }

        /// <summary>
        /// An ItemType has been picked up within the inventory.
        /// </summary>
        /// <param name="itemType">The ItemType that has been picked up.</param>
        /// <param name="count">The amount of item picked up.</param>
        /// <param name="immediatePickup">Was the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        protected virtual void OnPickupItemType(ItemType itemType, float count, bool immediatePickup, bool forceEquip) { }

        /// <summary>
        /// The DominantItem field has been updated for the specified item.
        /// </summary>
        /// <param name="item">The Item whose DominantItem field was updated.</param>
        /// <param name="dominantItem">True if the item is now a dominant item.</param>
        protected virtual void OnUpdateDominantItem(Item item, bool dominantItem) { }

        /// <summary>
        /// The specified consumable ItemType has been used.
        /// </summary>
        /// <param name="item">The Item that has been used.</param>
        /// <param name="itemType">The ItemType that has been used.</param>
        /// <param name="count">The remaining amount of the specified ItemType.</param>
        protected virtual void OnUseConsumableItemType(Item item, ItemType itemType, float count) { }

        /// <summary>
        /// The specified ItemType has been used.
        /// </summary>
        /// <param name="itemType">The ItemType that has been used.</param>
        /// <param name="count">The remaining amount of the specified ItemType.</param>
        protected virtual void OnUseItemType(ItemType itemType, float count) { }

        /// <summary>
        /// An item has been unequipped.
        /// </summary>
        /// <param name="itemType">The unequipped item.</param>
        /// <param name="slotID">The slot that the item previously occupied.</param>
        protected virtual void OnUnequipItem(Item item, int slotID) { }
    }
}
