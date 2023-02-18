/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects.CharacterAssist
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Base class which allows an object with the Inventory component to pickup items when a character enters the trigger.
    /// </summary>
    public abstract class ItemPickupBase : ObjectPickup
    {
        /// <summary>
        /// Class which allows the specified item to be collected by the character at runtime.
        /// </summary>
        [System.Serializable]
        public class PickupSet
        {
            [Tooltip("The item prefab that can be picked up.")]
            [SerializeField] protected GameObject m_Item;
            [Tooltip("The ID of the category that the ItemSet should be added to.")]
            [SerializeField] protected uint m_CategoryID;
            [Tooltip("The ItemSet to load when the item is picked up.")]
            [SerializeField] protected ItemSet m_ItemSet;
            [Tooltip("Is the ItemSet the default ItemSet within the category?")]
            [SerializeField] protected bool m_Default;

            public GameObject Item { get { return m_Item; } set { m_Item = value; } }
            public uint CategoryID { get { return m_CategoryID; } set { m_CategoryID = value; } }
            public ItemSet ItemSet { get { return m_ItemSet; } set { m_ItemSet = value; } }
            public bool Default { get { return m_Default; } set { m_Default = value; } }

            /// <summary>
            /// Default PickupSet constructor.
            /// </summary>
            public PickupSet() { m_ItemSet = new ItemSet(); }
        }

        [Tooltip("An array of items and ItemSets to pick up.")]
        [SerializeField] protected PickupSet[] m_ItemPickupSet;
        [Tooltip("Should the object be picked up even if the inventory cannot hold any more of the ItemIdentifier?")]
        [SerializeField] protected bool m_AlwaysPickup;

        public PickupSet[] ItemPickupSet { get { return m_ItemPickupSet; } set { m_ItemPickupSet = value; } }
        public bool AlwaysPickup { get { return m_AlwaysPickup; } set { m_AlwaysPickup = value; } }

        private bool m_PickedUp;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (m_ItemPickupSet != null) {
                for (int i = 0; i < m_ItemPickupSet.Length; ++i) {
                    // The item GameObject must contain the Item component.
                    if (m_ItemPickupSet[i].Item != null && m_ItemPickupSet[i].Item.GetComponent<Item>() == null) {
                        Debug.LogError($"Error: {m_ItemPickupSet[i].Item.name} doesn't contain the Item component.");
                    }
                }
            }
        }

        /// <summary>
        /// A GameObject has entered the trigger.
        /// </summary>
        /// <param name="other">The GameObject that entered the trigger.</param>
        public override void TriggerEnter(GameObject other)
        {
            TriggerEnter(other, -1);
        }

        /// <summary>
        /// A GameObject has entered the trigger.
        /// </summary>
        /// <param name="other">The other GameObject which is trying to do the pickup.</param>
        /// <param name="slotID">The slot ID that picked up the item. A -1 value will indicate no specified slot.</param>
        public void TriggerEnter(GameObject other, int slotID)
        {
            // The object must have an enabled inventory in order for the item to be picked up.
            var inventory = other.GetCachedParentComponent<InventoryBase>();
            if (inventory == null || !inventory.enabled) {
                return;
            }

            // The collider must be a main character collider. Items or ragdoll colliders don't count.
            var layerManager = inventory.gameObject.GetCachedComponent<CharacterLayerManager>();
            if (layerManager == null || !MathUtility.InLayerMask(other.gameObject.layer, layerManager.CharacterLayer)) {
                return;
            }

            TryItemPickup(inventory, slotID);
        }

        /// <summary>
        /// Tries to pickup the item.
        /// </summary>
        /// <param name="inventory">The inventory belonging to the character.</param>
        /// <param name="slotID">The slot ID that picked up the item. A -1 value will indicate no specified slot.</param>
        private void TryItemPickup(InventoryBase inventory, int slotID)
        {
            if (m_PickupOnTriggerEnter) {
                DoItemPickup(inventory.gameObject, inventory, slotID, false, true);
            } else {
                // If the object is a character that has a disabled pickup item ability then the item should be picked up immediately, 
                // even if the pickup on trigger enter is disabled.
                var character = inventory.gameObject.GetCachedComponent<Character.UltimateCharacterLocomotion>();
                if (character != null) {
                    var pickupItem = character.GetAbility<Character.Abilities.PickupItem>();
                    if (pickupItem != null && pickupItem.CanItemPickup()) {
                        DoItemPickup(inventory.gameObject, inventory, slotID, false, true);
                    }
                }
            }
        }

        /// <summary>
        /// Picks up the item.
        /// </summary>
        /// <param name="character">The character that should pick up the item.</param>
        /// <param name="inventory">The inventory belonging to the character.</param>
        /// <param name="slotID">The slot ID that picked up the item. A -1 value will indicate no specified slot.</param>
        /// <param name="immediatePickup">Should the item be picked up immediately?</param>
        /// <param name="pickupItemIdentifier">Should the ItemIdentifier be picked up? This should be false if the ItemIdentifier will later be picked up.</param>
        public void DoItemPickup(GameObject character, InventoryBase inventory, int slotID, bool immediatePickup, bool pickupItemIdentifier)
        {
            // Add any items to the character.
            if (m_ItemPickupSet != null && m_ItemPickupSet.Length > 0) {
                // Spawn the item under the character's ItemPlacement GameObject.
                var itemPlacement = character.GetComponentInChildren<ItemPlacement>(true);
                if (itemPlacement == null) {
                    Debug.LogError($"Error: ItemPlacement doesn't exist under the character {character.name}.");
                    return;
                }
                for (int i = 0; i < m_ItemPickupSet.Length; ++i) {
                    // If the Item is null then only the ItemSet should be added.
                    if (m_ItemPickupSet[i].Item == null) {
                        var itemSetManager = character.GetCachedComponent<ItemSetManagerBase>();
                        if (itemSetManager == null) {
                            continue;
                        }

                        IItemCategoryIdentifier category = null;
                        var addItemSetParents = true;
                        // If no item is specified then the category should be retrieved from the Item Definition.
                        if (m_ItemPickupSet[i].CategoryID == 0) {
                            for (int j = 0; j < m_ItemPickupSet[i].ItemSet.Slots.Length; ++j) {
                                var itemDefinition = m_ItemPickupSet[i].ItemSet.Slots[j];
                                if (itemDefinition != null) {
                                    category = itemDefinition.GetItemCategory();
                                }
                            }
                        } else {
                            // A specific category was specified.
                            if (itemSetManager.CategoryItemSets != null) {
                                for (int j = 0; j < itemSetManager.CategoryItemSets.Length; ++j) {
                                    if (itemSetManager.CategoryItemSets[j].CategoryID == m_ItemPickupSet[i].CategoryID) {
                                        category = itemSetManager.CategoryItemSets[j].ItemCategory;
                                        addItemSetParents = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (category != null) {
                            itemSetManager.AddItemSet(m_ItemPickupSet[i].ItemSet, m_ItemPickupSet[i].Default, category, addItemSetParents);
                        }

                        continue;
                    }

                    if (slotID != -1 && (slotID >= m_ItemPickupSet[i].ItemSet.Slots.Length || m_ItemPickupSet[i].ItemSet.Slots[slotID] == null)) {
                        continue;
                    }

                    var item = m_ItemPickupSet[i].Item.GetCachedComponent<Item>();
                    if (inventory.HasItem(item)) {
                        continue;
                    }

                    // Instantiate the item that will be added to the character.
                    item = Item.SpawnItem(character, item);

                    // Add the ItemSet before the item so the item can use the added ItemSet.
                    if (m_ItemPickupSet[i].ItemSet != null) {
                        var itemSetManager = character.GetCachedComponent<ItemSetManager>();
                        if (itemSetManager != null) {
                            m_ItemPickupSet[i].ItemSet.ItemIdentifiers = new Shared.Inventory.IItemIdentifier[m_ItemPickupSet[i].ItemSet.Slots.Length];
                            m_ItemPickupSet[i].ItemSet.ItemIdentifiers[item.SlotID] = item.ItemIdentifier;
                            itemSetManager.AddItemSet(item, m_ItemPickupSet[i].ItemSet, m_ItemPickupSet[i].Default);
                        }
                    }

                    // All of the setup is complete - add the item to the inventory.
                    inventory.AddItem(item, false, false);
                }
            }

            m_PickedUp = m_AlwaysPickup;
            if (pickupItemIdentifier) {
                // Even if the ItemIdentifier doesn't have space it may be equipped by the inventory. The object should be considered as picked up in this situation.
                EventHandler.RegisterEvent<Item, int>(character, "OnAbilityWillEquipItem", OnWillEquipItem);
                if (DoItemIdentifierPickup(character, inventory, slotID, immediatePickup, true)) {
                    m_PickedUp = true;
                }
                EventHandler.UnregisterEvent<Item, int>(character, "OnAbilityWillEquipItem", OnWillEquipItem);
            } else {
                // If pickup ItemIdentifier is false then the PickupItem ability will pick up the ItemIdentifier.
                m_PickedUp = true;
            }

            if (m_PickedUp) {
                ObjectPickedUp(character);
            }
        }

        /// <summary>
        /// Returns the ItemDefinitionAmount that the ItemPickup contains.
        /// </summary>
        /// <returns>The ItemDefinitionAmount that the ItemPickup contains.</returns>
        public abstract ItemDefinitionAmount[] GetItemDefinitionAmounts();

        /// <summary>
        /// Sets the ItemPickup ItemDefinitionAmounts value.
        /// </summary>
        /// <param name="itemDefinitionAmounts">The ItemDefinitionAmount that should be set.</param>
        public abstract void SetItemDefinitionAmounts(ItemDefinitionAmount[] itemDefinitionAmounts);

        /// <summary>
        /// Picks up the ItemIdentifier.
        /// </summary>
        /// <param name="character">The character that should pick up the ItemIdentifier.</param>
        /// <param name="inventory">The inventory belonging to the character.</param>
        /// <param name="slotID">The slot ID that picked up the item. A -1 value will indicate no specified slot.</param>
        /// <param name="immediatePickup">Should the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        /// <returns>True if an ItemIdentifier was picked up.</returns>
        public bool DoItemIdentifierPickup(GameObject character, InventoryBase inventory, int slotID, bool immediatePickup, bool forceEquip)
        {
            EventHandler.ExecuteEvent(character, "OnItemPickupStartPickup");
            var result = DoItemIdentifierPickupInternal(character, inventory, slotID, immediatePickup, forceEquip);
            EventHandler.ExecuteEvent(character, "OnItemPickupStopPickup");
            return result;
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
        protected abstract bool DoItemIdentifierPickupInternal(GameObject character, InventoryBase inventory, int slotID, bool immediatePickup, bool forceEquip);

        /// <summary>
        /// The specified item will be equipped.
        /// </summary>
        /// <param name="item">The item that will be equipped.</param>
        /// <param name="slotID">The slot that the item will occupy.</param>
        private void OnWillEquipItem(Item item, int slotID)
        {
            m_PickedUp = true;
        }
    }
}