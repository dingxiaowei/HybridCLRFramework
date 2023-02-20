/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;


namespace Opsive.UltimateCharacterController.Inventory
{
    /// <summary>
    /// Specifies the amount of each ItemType that the character can pickup or is loaded with the default inventory.
    /// </summary>
    [System.Serializable]
    public class ItemTypeCount
    {
        [Tooltip("The type of item.")]
        [SerializeField] protected ItemType m_ItemType;
        [Tooltip("The number of ItemType units to pickup.")]
        [SerializeField] protected float m_Count = 1;

        public ItemType ItemType { get { return m_ItemType; } set { m_ItemType = value; } }
        public float Count { get { return m_Count; } set { m_Count = value; } }

        /// <summary>
        /// ItemTypeCount constructor with no parameters.
        /// </summary>
        public ItemTypeCount() { }

        /// <summary>
        /// ItemTypeAmount constructor with two parameters.
        /// </summary>
        /// <param name="itemType">The type of item.</param>
        /// <param name="count">The amount of ItemType.</param>
        public ItemTypeCount(ItemType itemType, float count)
        {
            Initialize(itemType, count);
        }

        /// <summary>
        /// Initializes the ItemAmount to the specified values.
        /// </summary>
        /// <param name="itemType">The type of item.</param>
        /// <param name="count">The amount of ItemType.</param>
        public void Initialize(ItemType itemType, float count)
        {
            m_ItemType = itemType;
            m_Count = count;
        }
    }
}