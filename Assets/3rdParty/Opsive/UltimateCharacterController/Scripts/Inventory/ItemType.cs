/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Inventory
{
    /// <summary>
    /// An ItemType is a static representation of an item. Each item that interacts with the inventory must have an ItemType.
    /// </summary>
    public class ItemType : ScriptableObject
    {
        [Tooltip("The unique ID of the object within the collection.")]
        [SerializeField] protected int m_ID;
        [Tooltip("The ID of the categories that the ItemType belongs to.")]
        [SerializeField] protected int[] m_CategoryIDs = new int[] { 0 };
        [Tooltip("Describes what the ItemType represents.")]
        [SerializeField] protected string m_Description;
        [Tooltip("The maximum number of ItemTypes the character can hold.")]
        [SerializeField] protected float m_Capacity = float.MaxValue;
        [Tooltip("Specifies any other ItemTypes that are also dropped when the current ItemType is dropped.")]
        [SerializeField] protected ItemType[] m_DroppedItemTypes;

        public int ID { get { return m_ID; } set { m_ID = value; } }
        public int[] CategoryIDs { get { return m_CategoryIDs; } set { m_CategoryIDs = value; } }
        public string Description { get { return m_Description; } set { m_Description = value; } }
        public float Capacity { get { return m_Capacity; } set { m_Capacity = value; } }
        public ItemType[] DroppedItemTypes { get { return m_DroppedItemTypes; } set { m_DroppedItemTypes = value; } }

        private int[] m_CategoryIndices;

        public int[] CategoryIndices { get { return m_CategoryIndices; } set { m_CategoryIndices = value; } }

        /// <summary>
        /// Does the ItemType belong to the category at the specified ID?
        /// </summary>
        /// <param name="id">The ID to determine if it belongs to the ItemType.</param>
        /// <returns>True if the ItemType belongs to the category at the specified ID.</returns>
        public bool CategoryIDMatch(int id)
        {
            for (int i = 0; i < m_CategoryIDs.Length; ++i) {
                // A 0 category id matches all categories.
                if (m_CategoryIDs[i] == id || m_CategoryIDs[i] == 0) {
                    return true;
                }
            }
            return false;
        }
    }
}