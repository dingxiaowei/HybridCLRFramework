/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Inventory;

namespace Opsive.UltimateCharacterController.UI
{
    /// <summary>
    /// The PersistentItemMonitor will update the UI for the specified ItemType.
    /// </summary>
    public class PersistentItemMonitor : ItemMonitor
    {
        [Tooltip("The ItemType that the UI should monitor.")]
        [SerializeField] protected ItemType m_ItemType;
        [Tooltip("Should the UI only be shown when the item is unequipped?")]
        [SerializeField] protected bool m_AlwaysVisible = true;
        [Tooltip("Should the UI still be shown when the character dies?")]
        [SerializeField] protected bool m_VisibleOnDeath = true;

        private GameObject m_GameObject;
        private bool m_Alive;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_GameObject = gameObject;
        }

        /// <summary>
        /// Attaches the monitor to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the monitor to.</param>
        protected override void OnAttachCharacter(GameObject character)
        {
            if (m_Character != null) {
                EventHandler.UnregisterEvent<Item, int>(m_Character, "OnInventoryEquipItem", OnEquipItem);
                EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
                EventHandler.UnregisterEvent(m_Character, "OnRespawn", OnRespawn);
            }

            base.OnAttachCharacter(character);

            if (m_Character == null || m_CharacterInventory == null) {
                return;
            }

            // Start the monitor with the correct count.
            m_PrimaryCount.text = m_CharacterInventory.GetItemTypeCount(m_ItemType).ToString();
            m_Alive = true;

            EventHandler.RegisterEvent<Item, int>(m_Character, "OnInventoryEquipItem", OnEquipItem);
            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_Character, "OnRespawn", OnRespawn);
        }

        /// <summary>
        /// An ItemType has been picked up within the inventory.
        /// </summary>
        /// <param name="itemType">The ItemType that has been picked up.</param>
        /// <param name="count">The amount of item picked up.</param>
        /// <param name="immediatePickup">Was the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        protected override void OnPickupItemType(ItemType itemType, float count, bool immediatePickup, bool forceEquip)
        {
            if (itemType != m_ItemType || (!m_AlwaysVisible && !ShowCount())) {
                return;
            }

            m_PrimaryCount.text = m_CharacterInventory.GetItemTypeCount(m_ItemType).ToString();
            m_GameObject.SetActive(m_ShowUI);
        }

        /// <summary>
        /// Returns true if the ItemMonitor count should be shown.
        /// </summary>
        /// <returns>True if the ItemMonitor count should be shown.</returns>
        private bool ShowCount()
        {
            var slotsOccupied = true;
            var itemTypesMatch = false;
            for (int i = 0; i < m_CharacterInventory.SlotCount; ++i) {
                var item = m_CharacterInventory.GetItem(i);
                if (item == null) {
                    slotsOccupied = false;
                    continue;
                }

                if (ItemTypeMatch(item)) {
                    itemTypesMatch = !item.DominantItem;
                    if (!itemTypesMatch) {
                        slotsOccupied = true;
                        break;
                    }
                }
            }
            return !slotsOccupied || itemTypesMatch;
        }

        /// <summary>
        /// An item has been equipped.
        /// </summary>
        /// <param name="itemType">The equipped item.</param>
        /// <param name="slotID">The slot that the item now occupies.</param>
        private void OnEquipItem(Item item, int slotID)
        {
            UpdateActiveState(!item.DominantItem && ItemTypeMatch(item));
        }

        /// <summary>
        /// The DominantItem field has been updated for the specified item.
        /// </summary>
        /// <param name="item">The Item whose DominantItem field was updated.</param>
        /// <param name="dominantItem">True if the item is now a dominant item.</param>
        protected override void OnUpdateDominantItem(Item item, bool dominantItem)
        {
            if (m_CharacterInventory.GetItemTypeCount(item.ItemType) == 0) {
                return;
            }
            UpdateActiveState(!item.DominantItem && ItemTypeMatch(item));
        }

        /// <summary>
        /// Updates the GameObject's activated/deactivated state.
        /// </summary>
        /// <param name="forceActive">Should the monitor be shown even if the item isn't equipped?</param>
        private void UpdateActiveState(bool forceActive)
        {
            var active = m_ShowUI && (m_VisibleOnDeath || m_Alive) && (m_AlwaysVisible || forceActive || ShowCount());
            m_GameObject.SetActive(active);
            if (active) {
                m_PrimaryCount.text = m_CharacterInventory.GetItemTypeCount(m_ItemType).ToString();
            }
        }

        /// <summary>
        /// Does the item match the ItemType used by the monitor?
        /// </summary>
        /// <param name="item">The item that may use the ItemType.</param>
        /// <returns>True if the item matches the ItemType used by the monitor.</returns>
        private bool ItemTypeMatch(Item item)
        {
            if (item.ItemType == m_ItemType) {
                return true;
            }

            // The consumable ItemType may be specified.
            var itemActions = item.ItemActions;
            for (int i = 0; i < itemActions.Length; ++i) {
                var usableItem = itemActions[i] as IUsableItem;
                if (usableItem == null) {
                    continue;
                }

                if (usableItem.GetConsumableItemType() == m_ItemType) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The specified consumable ItemType has been used.
        /// </summary>
        /// <param name="item">The Item that has been used.</param>
        /// <param name="itemType">The ItemType that has been used.</param>
        /// <param name="count">The remaining amount of the specified ItemType.</param>
        protected override void OnUseConsumableItemType(Item item, ItemType itemType, float count)
        {
            if (itemType != m_ItemType) {
                return;
            }

            m_PrimaryCount.text = count.ToString();
        }

        /// <summary>
        /// The specified ItemType has been used.
        /// </summary>
        /// <param name="itemType">The ItemType that has been used.</param>
        /// <param name="count">The remaining amount of the specified ItemType.</param>
        protected override void OnUseItemType(ItemType itemType, float count)
        {
            if (itemType != m_ItemType) {
                return;
            }

            m_PrimaryCount.text = count.ToString();
        }

        /// <summary>
        /// An item has been unequipped.
        /// </summary>
        /// <param name="itemType">The unequipped item.</param>
        /// <param name="slotID">The slot that the item previously occupied.</param>
        protected override void OnUnequipItem(Item item, int slotID)
        {
            UpdateActiveState(false);
        }

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            m_Alive = false;
            UpdateActiveState(false);
        }

        /// <summary>
        /// The character has respawned.
        /// </summary>
        private void OnRespawn()
        {
            m_Alive = true;
            UpdateActiveState(false);
        }

        /// <summary>
        /// Can the UI be shown?
        /// </summary>
        /// <returns>True if the UI can be shown.</returns>
        protected override bool CanShowUI()
        {
            return (m_VisibleOnDeath || m_Alive) && (m_AlwaysVisible || ShowCount());
        }
    }
}
