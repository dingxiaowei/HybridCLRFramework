/// ---------------------------------------------
/// Dyp Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Items
{
    using UnityEngine;

    /// <summary>
    /// The item class.
    /// </summary>
    public class Item : MonoBehaviour, IItem
    {
        [SerializeField] protected ItemDefinition m_ItemDefinition;
        public ItemDefinition ItemDefinition => m_ItemDefinition;

        /// <summary>
        /// Use an item.
        /// </summary>
        /// <param name="itemInventory">The item inventory.</param>
        public virtual void Use(Inventory itemInventory)
        {
            
        }

        /// <summary>
        /// Drop an item.
        /// </summary>
        /// <param name="itemInventory">THe item inventory.</param>
        /// <param name="amount">The amount to drop.</param>
        public virtual void Drop(Inventory itemInventory, int amount)
        {
            itemInventory.Remove(this,amount);
            InventoryManager.SpawnPickupAt(this, itemInventory.transform.position, amount);
        }
    }
}