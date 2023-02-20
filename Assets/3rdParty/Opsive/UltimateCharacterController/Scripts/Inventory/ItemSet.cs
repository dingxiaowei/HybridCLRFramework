/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.Abilities.Items;
using Opsive.UltimateCharacterController.StateSystem;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Inventory
{
    /// <summary>
    /// Represents a set of ItemTypes that can be equipped at the same time.
    /// </summary>
    [System.Serializable]
    public class ItemSet : StateObject
    {
        [Tooltip("The ItemTypes that occupy the inventory slots.")]
        [SerializeField] protected ItemType[] m_Slots;
        [Tooltip("The state to change to when the ItemSet is active.")]
        [SerializeField] protected string m_State;
        [Tooltip("Is the ItemSet enabled?")]
        [SerializeField] protected bool m_Enabled = true;
        [Tooltip("Can the ItemSet be switched to by the EquipNext/EquipPrevious abilities?")]
        [SerializeField] protected bool m_CanSwitchTo = true;
        [Tooltip("The ItemSet index that should be activated when the current ItemSet is active and disabled.")]
        [SerializeField] protected int m_DisabledIndex = -1;

        [NonSerialized] public ItemType[] Slots { get { return m_Slots; } set { m_Slots = value; } }
        [NonSerialized] public string State { get { return m_State; } set { m_State = value; } }
        public bool Enabled { get { return m_Enabled; } set { m_Enabled = value; } }
        public bool CanSwitchTo { get { return m_CanSwitchTo; } set { m_CanSwitchTo = value; } }
        public int DisabledIndex { get { return m_DisabledIndex; } set { m_DisabledIndex = value; } }

        private ItemSetManager m_ItemSetManager;
        private EquipUnequip m_EquipUnequip;
        private ToggleEquip m_ToggleEquip;

        private int m_CategoryIndex;
        private int m_Index;
        private bool m_Active;

        public bool Active { set { m_Active = true; } }

        /// <summary>
        /// Default ItemSet constructor. 
        /// </summary>
        public ItemSet()
        {
            m_Enabled = true;
        }

        /// <summary>
        /// Two parameter ItemSet constructor. 
        /// </summary>
        /// <param name="slots">The ItemTypes that occupy the inventory slots.</param>
        /// <param name="state">The state to change to when the ItemSet is active.</param>
        public ItemSet(ItemType[] slots, string state)
        {
            m_Slots = slots;
            m_State = state;
            m_Enabled = true;
        }

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        /// <param name="gameObject">The GameObject this object is attached to.</param>
        /// <param name="itemSetManager">The ItemSetManager which owns the ItemSet.</param>
        /// <param name="categoryID">The ID of the category that the ItemSet belongs to.</param>
        /// <param name="categoryIndex">The index of the category that the ItemSet belongs to.</param>
        /// <param name="index">The index of the ItemSet.</param>
        public void Initialize(GameObject gameObject, ItemSetManager itemSetManager, int categoryID, int categoryIndex, int index)
        {
            // The ItemSet may have already been initialized.
            if (m_ItemSetManager != null) {
                return;
            }

            base.Initialize(gameObject);

            m_ItemSetManager = itemSetManager;
            var toggleEquipAbilities = gameObject.GetCachedComponent<UltimateCharacterLocomotion>().GetAbilities<ToggleEquip>();
            if (toggleEquipAbilities != null) {
                for (int i = 0; i < toggleEquipAbilities.Length; ++i) {
                    if (toggleEquipAbilities[i].ItemSetCategoryID == categoryID) {
                        m_ToggleEquip = toggleEquipAbilities[i];
                        break;
                    }
                }
            }
            var equipUnequipAbilities = gameObject.GetCachedComponent<UltimateCharacterLocomotion>().GetAbilities<EquipUnequip>();
            if (equipUnequipAbilities != null) {
                for (int i = 0; i < equipUnequipAbilities.Length; ++i) {
                    if (equipUnequipAbilities[i].ItemSetCategoryID == categoryID) {
                        m_EquipUnequip = equipUnequipAbilities[i];
                        break;
                    }
                }
            }
            m_CategoryIndex = categoryIndex;
            m_Index = index;
        }

        /// <summary>
        /// Callback when the StateManager has changed the active state on the current object.
        /// </summary>
        public override void StateChange()
        {
            base.StateChange();

            // The item set is active and the enabled state changed then the item set should be activated or deactivated. This is done through the Toggle Equip ability.
            if (m_Active) {
                if (m_Enabled) {
                    if (m_ToggleEquip != null && m_ItemSetManager.ActiveItemSetIndex[m_CategoryIndex] == m_ItemSetManager.GetDefaultItemSetIndex(m_CategoryIndex) &&
                            m_ItemSetManager.ActiveItemSetIndex[m_CategoryIndex] != m_Index) {
                        m_ToggleEquip.StartAbility();
                    }
                } else {
                    if (m_EquipUnequip != null && m_ItemSetManager.ActiveItemSetIndex[m_CategoryIndex] == m_Index) {
                        if (m_DisabledIndex == -1) {
                            m_EquipUnequip.StartEquipUnequip(m_ItemSetManager.GetDefaultItemSetIndex(m_CategoryIndex));
                        } else {
                            m_EquipUnequip.StartEquipUnequip(m_DisabledIndex);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Contains a list of ItemSets which belong in the same grouping.
    /// </summary>
    [System.Serializable]
    public class CategoryItemSet
    {
        [Tooltip("The ID of the category.")]
        [SerializeField] protected int m_CategoryID;
        [Tooltip("The index of the ItemSet that should be activated when no other ItemSets are activated.")]
        [SerializeField] protected int m_DefaultItemSetIndex = -1;
        [Tooltip("A list of the belonging ItemSets.")]
        [SerializeField] protected List<ItemSet> m_ItemSetList;

        public int CategoryID { get { return m_CategoryID; } set { m_CategoryID = value; } }
        public int DefaultItemSetIndex { get { return m_DefaultItemSetIndex; } set { m_DefaultItemSetIndex = value; } }
        public List<ItemSet> ItemSetList { get { return m_ItemSetList; } set { m_ItemSetList = value; } }

        /// <summary>
        /// CategoryItemSet default constructor.
        /// </summary>
        public CategoryItemSet()
        {
            m_ItemSetList = new List<ItemSet>();
        }

        /// <summary>
        /// CategoryItemSet constructor with a single parameter.
        /// </summary>
        /// <param name="categoryID">The ID of the category.</param>
        public CategoryItemSet(int categoryID)
        {
            m_CategoryID = categoryID;
            m_ItemSetList = new List<ItemSet>();
        }
    }
}