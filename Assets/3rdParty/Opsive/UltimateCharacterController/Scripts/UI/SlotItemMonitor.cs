/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Items.Actions;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// The ItemMonitor will update the UI for the character's items.
    /// </summary>
    public class SlotItemMonitor : ItemMonitor
    {
        [SerializeField] protected GameObject m_CountParent;
        [Tooltip("A reference to the text used for the usable item loaded count.")]
        [SerializeField] protected Text m_LoadedCount;
        [Tooltip("A reference to the text used for the usable item unloaded count.")]
        [SerializeField] protected Text m_UnloadedCount;
        [Tooltip("The ID that UI represents.")]
        [SerializeField] protected int m_ID;
        [Tooltip("A reference to the image used for the item's icon.")]
        [SerializeField] protected Image m_ItemIcon;
        [Tooltip("The action ID that the UI represents.")]
        [SerializeField] protected int m_ItemActionID;

        public Image ItemIcon { get { return m_ItemIcon; } }

        private GameObject m_GameObject;

        private RectTransform m_ItemRectTransform;
        private Item m_EquippedItem;
        private IItemIdentifier m_ConsumableItemIdentifier;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_GameObject = gameObject;
            if (m_ItemIcon == null) {
                m_ItemIcon = GetComponent<Image>();
            }
            m_ItemRectTransform = m_ItemIcon.GetComponent<RectTransform>();
            m_ItemIcon.sprite = null;

            // Wait until an item has been equipped to activate.
            m_GameObject.SetActive(false);
        }

        /// <summary>
        /// Attaches the monitor to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the monitor to.</param>
        protected override void OnAttachCharacter(GameObject character)
        {
            if (m_Character != null) {
                EventHandler.UnregisterEvent<Item, int>(m_Character, "OnAbilityWillEquipItem", OnEquipItem);
                EventHandler.UnregisterEvent<Item, int>(m_Character, "OnAbilityUnequipItemComplete", OnUnequipItem);
                EventHandler.UnregisterEvent<Item, int>(m_Character, "OnInventoryRemoveItem", OnUnequipItem);
                ResetMonitor();
            }

            base.OnAttachCharacter(character);

            if (m_Character == null || m_CharacterInventory == null) {
                return;
            }
            
            EventHandler.RegisterEvent<Item, int>(m_Character, "OnAbilityWillEquipItem", OnEquipItem);
            EventHandler.RegisterEvent<Item, int>(m_Character, "OnAbilityUnequipItemComplete", OnUnequipItem);
            EventHandler.RegisterEvent<Item, int>(m_Character, "OnInventoryRemoveItem", OnUnequipItem);
            // An item may already be equipped.
            for (int i = 0; i < m_CharacterInventory.SlotCount; ++i) {
                var item = m_CharacterInventory.GetActiveItem(i);
                if (item != null) {
                    OnEquipItem(item, i);
                }
            }
        }

        /// <summary>
        /// An ItemIdentifier has been picked up within the inventory.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier that has been picked up.</param>
        /// <param name="amount">The amount of item picked up.</param>
        /// <param name="immediatePickup">Was the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        protected override void OnPickupItemIdentifier(IItemIdentifier itemIdentifier, int amount, bool immediatePickup, bool forceEquip)
        {
            if (itemIdentifier != m_ConsumableItemIdentifier) {
                return;
            }

            var countString = m_CharacterInventory.GetItemIdentifierAmount(m_ConsumableItemIdentifier).ToString();
            if (m_PrimaryCount.enabled) {
                m_PrimaryCount.text = countString;
            } else {
                m_UnloadedCount.text = countString;
            }
        }

        /// <summary>
        /// An item has been equipped.
        /// </summary>
        /// <param name="item">The equipped item.</param>
        /// <param name="slotID">The slot that the item now occupies.</param>
        private void OnEquipItem(Item item, int slotID)
        {
            if (!item.DominantItem || item.UIMonitorID != m_ID) {
                return;
            }

            m_EquippedItem = item;
            m_ItemIcon.sprite = item.Icon;
            UnityEngineUtility.SizeSprite(m_ItemIcon.sprite, m_ItemRectTransform);
            m_GameObject.SetActive(m_ShowUI && m_ItemIcon.sprite != null);

            // Multiple item actions can be attached to the same item.
            ItemAction itemAction = null;
            if (m_ItemActionID < item.ItemActions.Length) {
                itemAction = item.ItemActions[m_ItemActionID];
            }

            if (itemAction is IUsableItem) {
                var usableItem = itemAction as IUsableItem;
                if ((m_ConsumableItemIdentifier = usableItem.GetConsumableItemIdentifier()) != null) {
                    var consumableItemIdentifierAmount = usableItem.GetConsumableItemIdentifierAmount();
                    // If the count is -1 then only the loaded should be shown.
                    if (consumableItemIdentifierAmount != -1) {
                        m_LoadedCount.text = usableItem.GetConsumableItemIdentifierAmount().ToString();
                        m_UnloadedCount.text = m_CharacterInventory.GetItemIdentifierAmount(usableItem.GetConsumableItemIdentifier()).ToString();
                        m_LoadedCount.enabled = m_UnloadedCount.enabled = true;
                        m_PrimaryCount.enabled = false;
                    } else {
                        m_PrimaryCount.text = m_CharacterInventory.GetItemIdentifierAmount(usableItem.GetConsumableItemIdentifier()).ToString();
                        m_PrimaryCount.enabled = true;
                        m_LoadedCount.enabled = m_UnloadedCount.enabled = false;
                    }
                    if (m_CountParent != null) {
                        m_CountParent.SetActive(true);
                    }
                } else {
                    DisableCountText();
                }
            } else {
                DisableCountText();
            }
        }

        /// <summary>
        /// Disables the text objects.
        /// </summary>
        private void DisableCountText()
        {
            if (m_CountParent != null) {
                m_CountParent.SetActive(false);
            }
            if (m_PrimaryCount != null) {
                m_PrimaryCount.enabled = false;
            }
            if (m_LoadedCount != null) {
                m_LoadedCount.enabled = false;
            }
            if (m_UnloadedCount != null) {
                m_UnloadedCount.enabled = false;
            }
        }

        /// <summary>
        /// The DominantItem field has been updated for the specified item.
        /// </summary>
        /// <param name="item">The Item whose DominantItem field was updated.</param>
        /// <param name="dominantItem">True if the item is now a dominant item.</param>
        protected override void OnUpdateDominantItem(Item item, bool dominantItem)
        {
            if ((m_EquippedItem != null && item != m_EquippedItem) || m_CharacterInventory.GetItemIdentifierAmount(item.ItemIdentifier) == 0 || m_CharacterInventory.GetActiveItem(item.SlotID) != item) {
                return;
            }

            if (item.DominantItem) {
                OnEquipItem(item, item.SlotID);
            } else {
                ResetMonitor();
            }
        }

        /// <summary>
        /// The specified consumable ItemIdentifier has been used.
        /// </summary>
        /// <param name="item">The Item that has been used.</param>
        /// <param name="itemIdentifier">The ItemIdentifier that has been used.</param>
        /// <param name="amount">The remaining amount of the specified ItemIdentifier.</param>
        protected override void OnUseConsumableItemIdentifier(Item item, IItemIdentifier itemIdentifier, int amount)
        {
            if (item.UIMonitorID != m_ID || itemIdentifier != m_ConsumableItemIdentifier) {
                return;
            }

            m_LoadedCount.text = amount.ToString();
        }

        /// <summary>
        /// The specified ItemIdentifier amount has been adjusted.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to adjust.</param>
        /// <param name="amount">The amount of ItemIdentifier to adjust.</param>
        protected override void OnAdjustItemIdentifierAmount(IItemIdentifier itemIdentifier, int amount)
        {
            if (itemIdentifier != m_ConsumableItemIdentifier) {
                return;
            }

            // The primary count will be disabled if the item has both a loaded and unloaded count.
            if (m_PrimaryCount.enabled) {
                m_PrimaryCount.text = amount.ToString();
            } else {
                m_UnloadedCount.text = amount.ToString();
            }
        }

        /// <summary>
        /// An item has been unequipped.
        /// </summary>
        /// <param name="item">The unequipped item.</param>
        /// <param name="slotID">The slot that the item previously occupied.</param>
        private void OnUnequipItem(Item item, int slotID)
        {
            Item equippedItem = null;
            if (!item.DominantItem || item.UIMonitorID != m_ID || ((equippedItem = m_CharacterInventory.GetActiveItem(slotID)) != null && equippedItem.DominantItem && equippedItem != item)) {
                return;
            }

            ResetMonitor();
        }

        /// <summary>
        /// Can the UI be shown?
        /// </summary>
        /// <returns>True if the UI can be shown.</returns>
        protected override bool CanShowUI()
        {
            return base.CanShowUI() && m_EquippedItem != null && m_ItemIcon.sprite != null;
        }

        /// <summary>
        /// Resets the monitor back to the default state.
        /// </summary>
        private void ResetMonitor()
        {
            m_EquippedItem = null;
            m_ConsumableItemIdentifier = null;
            m_GameObject.SetActive(false);
        }
    }
}
