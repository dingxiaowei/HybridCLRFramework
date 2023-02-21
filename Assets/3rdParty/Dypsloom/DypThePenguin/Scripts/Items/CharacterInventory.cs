/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Items
{
    using Dypsloom.DypThePenguin.Scripts.Character;
    using UnityEngine;

    /// <summary>
    /// The character inventory.
    /// </summary>
    public class CharacterInventory : Inventory, IItemUser
    {
        [SerializeField] protected Transform m_RightHandItem;

        protected Character m_Character;
        protected Item m_EquippedRightHandItem;
        
        public Character Character => m_Character;

        /// <summary>
        /// Cache components.
        /// </summary>
        private void Awake()
        {
            m_Character = GetComponent<Character>();
        }

        /// <summary>
        /// Try to use the equipped item.
        /// </summary>
        protected void Update()
        {
            if (m_EquippedRightHandItem is IUsableItem usableItem) {
                TickUse(usableItem);
            }
        }
        
        /// <summary>
        /// Try to use the an item.
        /// </summary>
        /// <param name="usableItem">The usable item.</param>
        public void TickUse(IUsableItem usableItem)
        {
            for (int i = 0; i < usableItem.Actions.Length; i++) {
                if (usableItem.Actions[i].UseInput || m_Character.CharacterInput.UseEquippedItemInput(usableItem, i)) {
                    if(usableItem.Actions[i].TargetAction.CanUse==false){continue;}
                    usableItem.Actions[i].TargetAction.Use(usableItem.Item,this);
                }
            }
        }

        /// <summary>
        /// Remove an Item.
        /// </summary>
        /// <param name="itemAmount">The item amount.</param>
        protected override void ItemRemoved(ItemAmount itemAmount)
        {
            var unequipCondition = itemAmount.Item.ItemDefinition.Unique
                ? GetItemIndex(itemAmount.Item) == -1
                : GetItemIndex(itemAmount.Item.ItemDefinition) == -1;
            
            if (unequipCondition && m_EquippedRightHandItem == (Item)itemAmount.Item) {
                Unequip(m_EquippedRightHandItem);
            }
            base.ItemRemoved(itemAmount);
        }

        /// <summary>
        /// Adn item was added.
        /// </summary>
        /// <param name="itemAmount">The item amount added.</param>
        protected override void ItemAdded(ItemAmount itemAmount)
        {
            Equip(itemAmount.Item);
            base.ItemAdded(itemAmount);
        }
        
        /// <summary>
        /// Equip an item on the character.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Equip(IItem item)
        {
            if(!(item is EquippableItem equippableItem)){return;}
            if((Item) item == m_EquippedRightHandItem){return;}
            if(item.ItemDefinition.Unique == false 
                && m_EquippedRightHandItem != null 
                && item.ItemDefinition == m_EquippedRightHandItem.ItemDefinition){return;}

            var index = GetItemIndex(item);
            if(index == -1){return;}

            Unequip(m_EquippedRightHandItem);

            m_EquippedRightHandItem = equippableItem;
            equippableItem.Equipped(true,this);
            
            InventoryManager.MoveItem(ref m_EquippedRightHandItem,m_RightHandItem);

            UpdateInventory();
        }
        
        /// <summary>
        /// Unequip an item.
        /// </summary>
        /// <param name="item">The item to unequip.</param>
        public void Unequip(Item item)
        {
            if(!(item is EquippableItem equippableItem)){return;}
            if(m_EquippedRightHandItem == null || item != m_EquippedRightHandItem){return;}

            
            InventoryManager.RemoveItem(m_EquippedRightHandItem);
            m_EquippedRightHandItem = null;
            equippableItem.Equipped(false,this);

            UpdateInventory();
        }
    }
}