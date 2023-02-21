/// ---------------------------------------------
/// Dyp Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Items
{
    using System;
    using UnityEngine;

    /// <summary>
    /// The item definition scriptable object.
    /// </summary>
    [CreateAssetMenu(fileName = "MyItemDefinition", menuName = "Dypsloom/Inventory/ItemDefinition", order = 1)]
    public class ItemDefinition : ScriptableObject
    {
        [Tooltip("The item icon.")]
        [SerializeField] protected Sprite m_Icon;
        [Tooltip("Is the item unique ot can it be stacked?")]
        [SerializeField] protected bool m_Unique;
        [Tooltip("The item prefab (must have an Item component that reference this scriptable object).")]
        [SerializeField] protected GameObject m_ItemPrefab;
        [Tooltip("The item description.")]
        [TextArea]
        [SerializeField] protected string m_Description;

        [NonSerialized] protected Type m_ItemType;
        [NonSerialized] protected Item m_DefaultItem;
        
        public Sprite Icon => m_Icon;
        public bool Unique => m_Unique;
        public string Description => m_Description;
        public GameObject ItemPrefab => m_ItemPrefab;

        public Item DefaultItem
        {
            get
            {
                if (m_DefaultItem == null) { m_DefaultItem = m_ItemPrefab.GetComponent<Item>(); }
                return m_DefaultItem;
            }
        }

        public Type  ItemType
        {
            get
            {
                if (m_ItemType == null) {
                    m_ItemType = DefaultItem.GetType();
                }
                return m_ItemType;
            }
        }
    }
}