/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using Opsive.Shared.Inventory;
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// The ItemSetManager manages the ItemSets belonging to the character.
    /// </summary>
    public class ItemSetManager : ItemSetManagerBase
    {
        [Tooltip("A reference to the ItemCollection that the inventory is using.")]
        [SerializeField] protected ItemCollection m_ItemCollection;

        public ItemCollection ItemCollection { get { return m_ItemCollection; } set { var prevItemCollection = m_ItemCollection; 
                                                                                        m_ItemCollection = value; 
                                                                                        Initialize(prevItemCollection != m_ItemCollection); } }

        /// <summary>
        /// Initializes the ItemSetManager.
        /// </summary>
        /// <param name="force">Should the ItemSet be force initialized?</param>
        public override void Initialize(bool force)
        {
            if (m_Initialized && !force) {
                return;
            }
            m_Initialized = true;

            if (m_ItemCollection == null) {
                m_CategoryItemSets = null;
                return;
            }

            // The ItemTypes get their categories from the ItemCollection.
            for (int i = 0; i < m_ItemCollection.ItemTypes.Length; ++i) {
                m_ItemCollection.ItemTypes[i].Initialize(m_ItemCollection);
            }

            // Initialize the categories.
            if ((m_CategoryItemSets == null || m_CategoryItemSets.Length == 0)) {
                if (m_ItemCollection.Categories.Length > 0) {
                    m_CategoryItemSets = new CategoryItemSet[m_ItemCollection.Categories.Length];
                }
            } else if (m_CategoryItemSets.Length != m_ItemCollection.Categories.Length) {
                System.Array.Resize(ref m_CategoryItemSets, m_ItemCollection.Categories.Length);
            }
            if (m_CategoryIndexMap == null) {
                m_CategoryIndexMap = new Dictionary<IItemCategoryIdentifier, int>();
            } else {
                m_CategoryIndexMap.Clear();
            }
            m_ActiveItemSetIndex = new int[m_CategoryItemSets.Length];
            m_NextItemSetIndex = new int[m_ActiveItemSetIndex.Length];
            for (int i = 0; i < m_CategoryItemSets.Length; ++i) {
                m_ActiveItemSetIndex[i] = -1;
                m_NextItemSetIndex[i] = -1;
                if (m_CategoryItemSets[i] == null) {
                    m_CategoryItemSets[i] = new CategoryItemSet(m_ItemCollection.Categories[i].ID, m_ItemCollection.Categories[i].name, m_ItemCollection.Categories[i]);
                } else {
                    m_CategoryItemSets[i].CategoryID = m_ItemCollection.Categories[i].ID;
                    m_CategoryItemSets[i].CategoryName = m_ItemCollection.Categories[i].name;
                    m_CategoryItemSets[i].ItemCategory = m_ItemCollection.Categories[i];
                }

                // Create a mapping between the category and index.
                var category = m_ItemCollection.GetCategory(m_CategoryItemSets[i].CategoryID);
                m_CategoryIndexMap.Add(category, i);

                // The ItemSet must be initialized.
                for (int j = 0; j < m_CategoryItemSets[i].ItemSetList.Count; ++j) {
                    m_CategoryItemSets[i].ItemSetList[j].Initialize(gameObject, this, m_CategoryItemSets[i].CategoryID, i, j);
                }
            }
        }
    }
}