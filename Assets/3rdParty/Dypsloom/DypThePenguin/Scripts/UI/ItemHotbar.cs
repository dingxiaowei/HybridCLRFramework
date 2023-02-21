/// ---------------------------------------------
/// Dyp Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.UI
{
    using Dypsloom.DypThePenguin.Scripts.Items;
    using UnityEngine;
    using Character = Dypsloom.DypThePenguin.Scripts.Character.Character;

    /// <summary>
    /// Item Hotbar allows you to show items in the inventory and use them.
    /// </summary>
    public class ItemHotbar : MonoBehaviour
    {
        [Tooltip("The hot item box prefab")]
        [SerializeField] protected GameObject m_ItemBoxPrefab;
        [Tooltip("The inventory to monitor.")]
        [SerializeField] protected Inventory m_Inventory;
        [Tooltip("The character.")]
        [SerializeField] protected Character m_Character;

        protected ItemHotbarSlotBox[] m_ItemBoxes;

        /// <summary>
        /// Cache components.
        /// </summary>
        private void Awake()
        {
            if (m_Character == null) { m_Character = FindObjectOfType<Character>();}

            if (m_Inventory == null && m_Character != null) { m_Inventory = m_Character.GetComponent<Inventory>();}
            InitializeItemBoxes();
        }

        /// <summary>
        /// Initialize the item boxes.
        /// </summary>
        protected void InitializeItemBoxes()
        {
            for (int i = transform.childCount - 1; i >= 0; i--) {
                Destroy(transform.GetChild(i).gameObject);
            }

            m_ItemBoxes = new ItemHotbarSlotBox[Mathf.Clamp(m_Inventory.MaxSize,0,9)];
            for (int i = 0; i < m_ItemBoxes.Length; i++) {
                m_ItemBoxes[i] = Instantiate(m_ItemBoxPrefab, transform).GetComponent<ItemHotbarSlotBox>();
                m_ItemBoxes[i].SetIndex(i);
            }
        }

        /// <summary>
        /// Listen to the inventory updates.
        /// </summary>
        private void Start()
        {
            m_Inventory.OnInventoryUpdate += UpdateUI;
            UpdateUI();
        }

        /// <summary>
        /// Draw the item boxes.
        /// </summary>
        protected virtual void UpdateUI()
        {
            for (int i = 0; i < m_ItemBoxes.Length; i++) {
                if (m_Inventory.Items.Count <= i) { m_ItemBoxes[i].DrawEmpty(); } else {
                    m_ItemBoxes[i].Draw(m_Inventory.Items[i]);
                }
            }
        }

        /// <summary>
        /// Use ot drop items.
        /// </summary>
        private void Update()
        {
            if(m_Character == null){return;}
        
            for (int i = 0; i < m_ItemBoxes.Length; i++) {
                var item = m_ItemBoxes[i].ItemAmount.Item;
                if( item == null){continue;}

                if (m_Character.CharacterInput.UseItemHotbarInput(i)) {
                    item.Use(m_Character.Inventory);
                }
                if (m_Character.CharacterInput.DropItemHotbarInput(i)) {
                    item.Drop(m_Character.Inventory,1);
                }
            }
        
        }
    }
}