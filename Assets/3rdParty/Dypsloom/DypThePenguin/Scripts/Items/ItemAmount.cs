/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Items
{
    using System;
    using UnityEngine;

    /// <summary>
    /// An item amount.
    /// </summary>
    [Serializable]
    public struct ItemAmount
    {
        [Tooltip("The item definition.")]
        [SerializeField] private ItemDefinition m_ItemDefinition;
        [Tooltip("The item.")]
        [SerializeField] private Item m_ItemComponent;
        [Tooltip("The amount.")]
        [SerializeField] private int m_Amount;

        private IItem m_Item;
        
        public int Amount => m_Amount;
        public IItem Item
        {
            get
            {
                if (m_Item != null) { return m_Item;}

                if (m_ItemComponent != null) {
                    m_Item = m_ItemComponent;
                    return m_Item;
                }
                
                if (m_ItemDefinition == null) { return null;}
                
                return m_ItemDefinition.DefaultItem;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="amount">The amount.</param>
        public ItemAmount(IItem item, int amount)
        {
            m_Amount = amount;
            m_Item = item;
            m_ItemComponent = item as Item;
            m_ItemDefinition = item?.ItemDefinition;
        }
    
        public static implicit operator ItemAmount( (int,IItem) x) 
            => new ItemAmount(x.Item2,x.Item1);
        public static implicit operator ItemAmount( (IItem,int) x) 
            => new ItemAmount(x.Item1,x.Item2);
    }
}