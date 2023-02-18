/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using Opsive.Shared.Inventory;
    using Opsive.Shared.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A Category contains a grouping of ItemTypes.
    /// </summary>
    [System.Serializable]
    public class Category : ScriptableObject, IItemCategoryIdentifier
    {
        [Tooltip("The ID of the category.")]
        [SerializeField] protected uint m_ID;
        
        public uint ID { get { if (RandomID.IsIDEmpty(m_ID)) { m_ID = RandomID.Generate(); } return m_ID; } set { m_ID = value; } }

        private Category[] m_Parents;
        public Category[] Parents { set { m_Parents = value; } }

        /// <summary>
        /// Returns a read only array of the direct parents of the current category.
        /// </summary>
        /// <returns>The direct parents of the current category.</returns>
        public IReadOnlyList<IItemCategoryIdentifier> GetDirectParents()
        {
            return m_Parents as IReadOnlyList<IItemCategoryIdentifier>;
        }

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        /// <returns>A string representation of the object.</returns>
        public override string ToString() { return name; }
    }
}