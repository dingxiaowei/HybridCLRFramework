/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Items
{
    using UnityEngine;

    /// <summary>
    /// An equippable Item.
    /// </summary>
    public class EquippableItem : Item, IUsableItem
    {
        [SerializeField] protected ItemUseAction[] m_ItemUseAction;

        protected bool m_IsEquipped;
        
        public IItemUseAction[] Actions => m_ItemUseAction;
        public IItem Item => this;

        public bool IsEquipped => m_IsEquipped;

        /// <summary>
        /// Equip the item to the inventory.
        /// </summary>
        /// <param name="itemInventory">The inventory where the item resides.</param>
        public override void Use(Inventory itemInventory)
        {
            base.Use(itemInventory);
            if (itemInventory is CharacterInventory characterInventory) {
                characterInventory.Equip(this);
            }
        }
        
        /// <summary>
        /// Set whether the item is equipped or not.
        /// </summary>
        /// <param name="isEquipped">Is the item equipped?</param>
        /// <param name="itemInventory">The inventory of the item.</param>
        public virtual void Equipped(bool isEquipped, Inventory itemInventory)
        {
            m_IsEquipped = isEquipped;
        }
    }
}