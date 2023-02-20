/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Objects.CharacterAssist
{
    /// <summary>
    /// Allows an object with the Invetory component to pickup items when a character enters the trigger.
    /// </summary>
    public class ItemPickup : ObjectPickup
    {
        /// <summary>
        /// Class which allows the specified item to be collected by the character at runtime.
        /// </summary>
        [System.Serializable]
        public class PickupSet
        {
            [Tooltip("The item prefab that can be picked up.")]
            [SerializeField] protected GameObject m_Item;
            [Tooltip("The ItemSet to load when the item is picked up.")]
            [SerializeField] protected ItemSet m_ItemSet;
            [Tooltip("Is the ItemSet the default ItemSet within the category?")]
            [SerializeField] protected bool m_Default;

            public GameObject Item { get { return m_Item; } set { m_Item = value; } }
            public ItemSet ItemSet { get { return m_ItemSet; } set { m_ItemSet = value; } }
            public bool Default { get { return m_Default; } set { m_Default = value; } }

            /// <summary>
            /// Default PickupSet constructor.
            /// </summary>
            public PickupSet() { m_ItemSet = new ItemSet(); }

            /// <summary>
            /// Two parameter PickupSet constructor.
            /// </summary>
            /// <param name="item">The item to use within the PickupSet.</param>
            /// <param name="itemSet">The ItemSet to use within the PickupSet.</param>
            public PickupSet(GameObject item, ItemSet itemSet)
            {
                m_Item = item;
                m_ItemSet = itemSet;
                m_ItemSet.Enabled = true;
            }
        }

        [Tooltip("An array of items and ItemSets to pick up.")]
        [SerializeField] protected PickupSet[] m_ItemPickupSet;
        [Tooltip("An array of ItemTypes to be picked up.")]
        [SerializeField] protected ItemTypeCount[] m_ItemTypeCounts;
        [Tooltip("Should the object be picked up even if the inventory cannot hold any more of the ItemType?")]
        [SerializeField] protected bool m_AlwaysPickup;

        public PickupSet[] ItemPickupSet { get { return m_ItemPickupSet; } set { m_ItemPickupSet = value; } }
        public ItemTypeCount[] ItemTypeCounts { get { return m_ItemTypeCounts; } set { m_ItemTypeCounts = value; } }
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
                        Debug.LogError("Error: " + m_ItemPickupSet[i].Item.name + " doesn't contain the Item component.");
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

            TryItemPickup(other, inventory, slotID);
        }

        /// <summary>
        /// Tries to pickup the item.
        /// </summary>
        /// <param name="other">The GameObject that entered the trigger.</param>
        /// <param name="inventory">The inventory belonging to the character.</param>
        /// <param name="slotID">The slot ID that picked up the item. A -1 value will indicate no specified slot.</param>
        private void TryItemPickup(GameObject other, InventoryBase inventory, int slotID)
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
        /// <param name="pickupItemType">Should the ItemType be picked up? This should be false if the ItemType will later be picked up.</param>
        public void DoItemPickup(GameObject character, InventoryBase inventory, int slotID, bool immediatePickup, bool pickupItemType)
        {
            // Add any items to the character.
            if (m_ItemPickupSet != null && m_ItemPickupSet.Length > 0) {
                // Spawn the item under the character's ItemPlacement GameObject.
                var itemPlacement = character.GetComponentInChildren<ItemPlacement>(true);
                if (itemPlacement == null) {
                    Debug.LogError("Error: ItemPlacement doesn't exist under the character " + character.name);
                    return;
                }
                for (int i = 0; i < m_ItemPickupSet.Length; ++i) {
                    // The Item must exist.
                    if (m_ItemPickupSet[i].Item == null || (slotID != -1 && (slotID >= m_ItemPickupSet[i].ItemSet.Slots.Length || m_ItemPickupSet[i].ItemSet.Slots[slotID] == null))) {
                        continue;
                    }

                    var item = m_ItemPickupSet[i].Item.GetCachedComponent<Item>();

                    // Add the ItemSet before the item so the item can use the added ItemSet.
                    if (m_ItemPickupSet[i].ItemSet != null) {
                        var itemSetManager = character.GetCachedComponent<ItemSetManager>();
                        if (itemSetManager != null) {
                            itemSetManager.AddItemSet(item, m_ItemPickupSet[i].ItemSet, m_ItemPickupSet[i].Default);
                        }
                    }

                    // Instantiate and add the item to the character.
                    if (!inventory.HasItem(item)) {
                        var itemGameObject = ObjectPool.Instantiate(m_ItemPickupSet[i].Item, Vector3.zero, Quaternion.identity, itemPlacement.transform);
                        itemGameObject.name = m_ItemPickupSet[i].Item.name;
                        itemGameObject.transform.localPosition = Vector3.zero;
                        itemGameObject.transform.localRotation = Quaternion.identity;
                        item = itemGameObject.GetComponent<Item>();
                        inventory.AddItem(item, false);
                    }
                }
            }

            m_PickedUp = m_AlwaysPickup;
            if (pickupItemType) {
                // Even if the ItemType doesn't have space it may be equipped by the inventory. The object should be considered as picked up in this situation.
                EventHandler.RegisterEvent<Item, int>(character, "OnAbilityWillEquipItem", OnWillEquipItem);
                if (DoItemTypePickup(character, inventory, slotID, immediatePickup, true)) {
                    m_PickedUp = true;
                }
                EventHandler.UnregisterEvent<Item, int>(character, "OnAbilityWillEquipItem", OnWillEquipItem);
            } else {
                // If pickup ItemType is false then the PickupItem ability will pick up the ItemType.
                m_PickedUp = true;
            }

            if (m_PickedUp) {
                ObjectPickedUp(character);
            }
        }

        /// <summary>
        /// Picks up the ItemType.
        /// </summary>
        /// <param name="character">The character that should pick up the ItemType.</param>
        /// <param name="inventory">The inventory belonging to the character.</param>
        /// <param name="slotID">The slot ID that picked up the item. A -1 value will indicate no specified slot.</param>
        /// <param name="immediatePickup">Should the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        /// <returns>True if an ItemType was picked up.</returns>
        public bool DoItemTypePickup(GameObject character, InventoryBase inventory, int slotID, bool immediatePickup, bool forceEquip)
        {
            // Add the ItemTypes to the Inventory. This allows the character to pick up the actual item and any consumable ItemTypes (such as ammo).
            var pickedUp = false;
            EventHandler.ExecuteEvent(character, "OnItemPickupStartPickup");
            if (m_ItemTypeCounts != null) {
                for (int i = 0; i < m_ItemTypeCounts.Length; ++i) {
                    if (inventory.PickupItemType(m_ItemTypeCounts[i].ItemType, m_ItemTypeCounts[i].Count, slotID, immediatePickup, forceEquip)) {
                        pickedUp = true;
                    }
                }
            }
            EventHandler.ExecuteEvent(character, "OnItemPickupStopPickup");
            return pickedUp;
        }

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