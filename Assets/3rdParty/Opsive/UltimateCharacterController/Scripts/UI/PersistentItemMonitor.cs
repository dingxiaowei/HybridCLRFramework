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
    using UnityEngine;

    /// <summary>
    /// The PersistentItemMonitor will update the UI for the specified ItemType.
    /// </summary>
    public class PersistentItemMonitor : ItemMonitor
    {
        [Tooltip("The ItemDefinition that the UI should monitor.")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_ItemType")]
        [SerializeField] protected ItemDefinitionBase m_ItemDefinition;
        [Tooltip("Should the UI only be shown when the item is unequipped?")]
        [SerializeField] protected bool m_AlwaysVisible = true;
        [Tooltip("Should the UI still be shown when the character dies?")]
        [SerializeField] protected bool m_VisibleOnDeath = true;

        public ItemDefinitionBase ItemDefinition { get { return m_ItemDefinition; } set { m_ItemDefinition = value; } }
        public bool AlwaysVisible { get { return m_AlwaysVisible; } set { m_AlwaysVisible = value; } }
        public bool VisibleOnDeath { get { return m_VisibleOnDeath; } set { m_VisibleOnDeath = value; } }

        private GameObject m_GameObject;
        private IItemIdentifier m_ItemIdentifier;
        private bool m_Alive = true;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_GameObject = gameObject;
            m_PrimaryCount.text = "0";
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

            EventHandler.RegisterEvent<Item, int>(m_Character, "OnInventoryEquipItem", OnEquipItem);
            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_Character, "OnRespawn", OnRespawn);
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
            if (itemIdentifier.GetItemDefinition() != m_ItemDefinition) {
                return;
            }

            m_ItemIdentifier = itemIdentifier;
            
            if (!m_AlwaysVisible && !ShowCount()) {
                return;
            }

            m_PrimaryCount.text = m_CharacterInventory.GetItemIdentifierAmount(itemIdentifier).ToString();
            m_GameObject.SetActive(m_ShowUI);
        }

        /// <summary>
        /// Returns true if the ItemMonitor count should be shown.
        /// </summary>
        /// <returns>True if the ItemMonitor count should be shown.</returns>
        private bool ShowCount()
        {
            var slotsOccupied = true;
            var itemIdentifierMatch = false;
            for (int i = 0; i < m_CharacterInventory.SlotCount; ++i) {
                var item = m_CharacterInventory.GetActiveItem(i);
                if (item == null) {
                    slotsOccupied = false;
                    continue;
                }

                if (ItemIdentifierMatch(item)) {
                    itemIdentifierMatch = !item.DominantItem;
                    if (!itemIdentifierMatch) {
                        slotsOccupied = true;
                        break;
                    }
                }
            }
            return !slotsOccupied || itemIdentifierMatch;
        }

        /// <summary>
        /// An item has been equipped.
        /// </summary>
        /// <param name="item">The equipped item.</param>
        /// <param name="slotID">The slot that the item now occupies.</param>
        private void OnEquipItem(Item item, int slotID)
        {
            UpdateActiveState(!item.DominantItem && ItemIdentifierMatch(item));
        }

        /// <summary>
        /// The DominantItem field has been updated for the specified item.
        /// </summary>
        /// <param name="item">The Item whose DominantItem field was updated.</param>
        /// <param name="dominantItem">True if the item is now a dominant item.</param>
        protected override void OnUpdateDominantItem(Item item, bool dominantItem)
        {
            if (m_CharacterInventory.GetItemIdentifierAmount(item.ItemIdentifier) == 0) {
                return;
            }
            UpdateActiveState(!item.DominantItem && ItemIdentifierMatch(item));
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
                m_PrimaryCount.text = m_CharacterInventory.GetItemIdentifierAmount(m_ItemIdentifier).ToString();
            }
        }

        /// <summary>
        /// Does the item match the ItemIdentifier used by the monitor?
        /// </summary>
        /// <param name="item">The item that may use the ItemIdentifier.</param>
        /// <returns>True if the item matches the ItemIdentifier used by the monitor.</returns>
        private bool ItemIdentifierMatch(Item item)
        {
            if (item.ItemIdentifier.GetItemDefinition() == m_ItemDefinition) {
                return true;
            }

            // The consumable ItemIdentifier may be specified.
            var itemActions = item.ItemActions;
            for (int i = 0; i < itemActions.Length; ++i) {
                var usableItem = itemActions[i] as IUsableItem;
                if (usableItem == null) {
                    continue;
                }

                var consumableItemIdentifier = usableItem.GetConsumableItemIdentifier();
                if (consumableItemIdentifier != null && consumableItemIdentifier.GetItemDefinition() == m_ItemDefinition) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The specified consumable ItemIdentifier has been used.
        /// </summary>
        /// <param name="item">The Item that has been used.</param>
        /// <param name="itemIdentifier">The ItemIdentifier that has been used.</param>
        /// <param name="amount">The remaining amount of the specified ItemIdentifier.</param>
        protected override void OnUseConsumableItemIdentifier(Item item, IItemIdentifier itemIdentifier, int amount)
        {
            if (itemIdentifier.GetItemDefinition() != m_ItemDefinition) {
                return;
            }

            m_PrimaryCount.text = amount.ToString();
        }

        /// <summary>
        /// The specified ItemIdentifier amount has been adjusted.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to adjust.</param>
        /// <param name="amount">The amount of ItemIdentifier to adjust.</param>
        protected override void OnAdjustItemIdentifierAmount(IItemIdentifier itemIdentifier, int amount)
        {
            if (itemIdentifier.GetItemDefinition() != m_ItemDefinition) {
                return;
            }

            m_PrimaryCount.text = amount.ToString();
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
            return base.CanShowUI() && (m_VisibleOnDeath || m_Alive) && (m_AlwaysVisible || ShowCount());
        }
    }
}
