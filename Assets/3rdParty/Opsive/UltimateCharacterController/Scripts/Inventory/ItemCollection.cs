/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Inventory
{
    /// <summary>
    /// The ItemCollection ScriptableObject is a container for the static item data.
    /// </summary>
    public class ItemCollection : ScriptableObject
    {
        /// <summary>
        /// A Category contains a grouping of ItemTypes.
        /// </summary>
        [System.Serializable]
        public class Category
        {
            [Tooltip("The ID of the category.")]
            [SerializeField] protected int m_ID;
            [Tooltip("The name of the category.")]
            [SerializeField] protected string m_Name;

            public int ID { get { return m_ID; } set { m_ID = value; } }
            public string Name { get { return m_Name; } set { m_Name = value; } }

            /// <summary>
            /// Default Category constructor.
            /// </summary>
            public Category() { }

            /// <summary>
            /// Three parameter Category constructor.
            /// </summary>
            /// <param name="id">The ID of the category.</param>
            /// <param name="name">The name of the category.</param>
            public Category(int id, string name)
            {
                m_ID = id;
                m_Name = name;
            }
        }

        [Tooltip("An array of all of the possible Categories.")]
        [SerializeField] protected Category[] m_Categories;
        [Tooltip("An array of all of the possible ItemTypes.")]
        [SerializeField] protected ItemType[] m_ItemTypes;

        public Category[] Categories { get { return m_Categories; } set { m_Categories = value; } }
        public ItemType[] ItemTypes { get { return m_ItemTypes; } set { m_ItemTypes = value; } }
    }
}