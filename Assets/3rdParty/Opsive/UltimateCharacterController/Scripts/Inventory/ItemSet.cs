/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Inventory;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.StateSystem;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Represents a set of ItemIdentifiers that can be equipped at the same time.
    /// </summary>
    [System.Serializable]
    public class ItemSet : StateObject
    {
        [Tooltip("The Item Definitions that occupy the inventory slots.")]
        [SerializeField] protected ItemDefinitionBase[] m_Slots;
        [Tooltip("The state to change to when the ItemSet is active.")]
        [SerializeField] protected string m_State;
        [Tooltip("Is the ItemSet enabled?")]
        [SerializeField] protected bool m_Enabled = true;
        [Tooltip("Can the ItemSet be switched to by the EquipNext/EquipPrevious abilities?")]
        [SerializeField] protected bool m_CanSwitchTo = true;
        [Tooltip("The ItemSet index that should be activated when the current ItemSet is active and disabled.")]
        [SerializeField] protected int m_DisabledIndex = -1;

        [NonSerialized] public ItemDefinitionBase[] Slots { get { return m_Slots; } set { m_Slots = value; } }
        [NonSerialized] public string State { get { return m_State; } set { m_State = value; } }
        public bool Enabled { get { return m_Enabled; } set { m_Enabled = value; } }
        public bool CanSwitchTo { get { return m_CanSwitchTo; } set { m_CanSwitchTo = value; } }
        public int DisabledIndex { get { return m_DisabledIndex; } set { m_DisabledIndex = value; } }

        private ItemSetManagerBase m_ItemSetManager;
        private EquipUnequip m_EquipUnequip;

        private int m_CategoryIndex;
        private int m_Index;
        private IItemIdentifier[] m_ItemIdentifiers;
        private bool m_Active;
        private bool m_EmptyItemSet;

        public IItemIdentifier[] ItemIdentifiers { get { return m_ItemIdentifiers; } set { m_ItemIdentifiers = value; } }
        public int Index { set { m_Index = value; } }
        public bool Active { set { m_Active = value; } }

        /// <summary>
        /// Default ItemSet constructor. 
        /// </summary>
        public ItemSet()
        {
            m_Enabled = true;
        }

        /// <summary>
        /// ItemSet constructor which copies the parameters from an existing ItemSet. 
        /// </summary>
        /// <param name="itemSet">The ItemSet to copy the values of.</param>
        public ItemSet(ItemSet itemSet)
        {
            m_Slots = new ItemDefinitionBase[itemSet.Slots.Length];
            System.Array.Copy(itemSet.Slots, m_Slots, itemSet.Slots.Length);
            m_State = itemSet.State;
            m_Enabled = itemSet.Enabled;
            m_CanSwitchTo = itemSet.CanSwitchTo;
            m_DisabledIndex = itemSet.DisabledIndex;
            m_ItemIdentifiers = new IItemIdentifier[itemSet.Slots.Length];
            if (itemSet.ItemIdentifiers != null && itemSet.ItemIdentifiers.Length > 0) {
                System.Array.Copy(itemSet.ItemIdentifiers, m_ItemIdentifiers, itemSet.ItemIdentifiers.Length);
            }
            if (itemSet.States != null && itemSet.States.Length > 1) {
                m_States = new State[itemSet.States.Length];
                System.Array.Copy(itemSet.States, m_States, itemSet.States.Length);
            }
        }

        /// <summary>
        /// Four parameter ItemSet constructor. 
        /// </summary>
        /// <param name="slotCount">The number of slots used by the ItemSet.</param>
        /// <param name="slotID">The ID of the slot that will use the ItemSet.</param>
        /// <param name="itemDefinition">The ItemDefinition of the ItemSet.</param>
        /// <param name="itemIdentifier">The ItemIdentifier of the ItemSet.</param>
        /// <param name="state">The state to change to when the ItemSet is active.</param>
        public ItemSet(int slotCount, int slotID, ItemDefinitionBase itemDefinition, IItemIdentifier itemIdentifier, string state)
        {
            m_Slots = new ItemDefinitionBase[slotCount];
            m_Slots[slotID] = itemDefinition;
            m_State = state;
            m_Enabled = true;
            m_ItemIdentifiers = new IItemIdentifier[slotCount];
            m_ItemIdentifiers[slotID] = itemIdentifier;
        }

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        /// <param name="gameObject">The GameObject this object is attached to.</param>
        /// <param name="itemSetManager">The ItemSetManager which owns the ItemSet.</param>
        /// <param name="categoryID">The ID of the category that the ItemSet belongs to.</param>
        /// <param name="categoryIndex">The index of the category that the ItemSet belongs to.</param>
        /// <param name="index">The index of the ItemSet.</param>
        public void Initialize(GameObject gameObject, ItemSetManagerBase itemSetManager, uint categoryID, int categoryIndex, int index)
        {
            // The ItemSet may have already been initialized.
            if (m_ItemSetManager != null) {
                return;
            }

            base.Initialize(gameObject);

            m_ItemSetManager = itemSetManager;
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
            if (m_ItemIdentifiers == null) {
                m_ItemIdentifiers = new IItemIdentifier[m_Slots.Length];
            }
            m_EmptyItemSet = true;
            for (int i = 0; i < m_Slots.Length; ++i) {
                if (m_Slots[i] != null) {
                    m_EmptyItemSet = false;
                    return;
                }
            }

            EventHandler.RegisterEvent<int, int>(m_ItemSetManager.gameObject, "OnItemSetManagerUpdateItemSet", OnUpdateItemSet);
        }

        /// <summary>
        /// Callback when the StateManager has changed the active state on the current object.
        /// </summary>
        public override void StateChange()
        {
            base.StateChange();

            // The item set is active and the enabled state changed then the item set should be activated or deactivated. This is done through the Equip Unequip ability.
            if (m_Active) {
                if (m_Enabled) {
                    var targetItemSetIndex = m_EquipUnequip.IsActive ? m_EquipUnequip.ActiveItemSetIndex : m_ItemSetManager.ActiveItemSetIndex[m_CategoryIndex];
                    if ((targetItemSetIndex == -1 || targetItemSetIndex == m_ItemSetManager.GetDefaultItemSetIndex(m_CategoryIndex)) && targetItemSetIndex != m_Index) {
                        m_EquipUnequip.StartEquipUnequip(m_Index);
                    }
                } else {
                    if (m_DisabledIndex == -1) {
                        var defaultItemSetIndex = m_ItemSetManager.GetDefaultItemSetIndex(m_CategoryIndex);
                        if (m_Index == defaultItemSetIndex || !m_ItemSetManager.IsItemSetValid(m_CategoryIndex, defaultItemSetIndex, false)) {
                            // The current item set is equal to the ItemSet being disabled. Equip an empty item set.
                            m_EquipUnequip.StartEquipUnequip(-1);
                        } else {
                            m_EquipUnequip.StartEquipUnequip(defaultItemSetIndex);
                        }
                    } else {
                        if (m_ItemSetManager.IsItemSetValid(m_CategoryIndex, m_DisabledIndex, false)) {
                            m_EquipUnequip.StartEquipUnequip(m_DisabledIndex);
                        } else {
                            m_EquipUnequip.StartEquipUnequip(-1);
                        }
                    }
                }
            } else if (m_Enabled && (m_EmptyItemSet || m_ItemSetManager.IsItemSetValid(m_CategoryIndex, m_Index, false))) {
                // If the item set is not active and it is enabled then the item set should be enabled if it can be.
                var targetItemSetIndex = m_EquipUnequip.IsActive ? m_EquipUnequip.ActiveItemSetIndex : m_ItemSetManager.ActiveItemSetIndex[m_CategoryIndex];
                if (targetItemSetIndex == -1) {
                    m_EquipUnequip.StartEquipUnequip(m_Index);
                }
            }
        }

        /// <summary>
        /// The ItemSet has changed.
        /// </summary>
        /// <param name="categoryIndex">The index of the changed category.</param>
        /// <param name="itemSetIndex">The index of the changed ItemSet.</param>
        private void OnUpdateItemSet(int categoryIndex, int itemSetIndex)
        {
            if (categoryIndex == m_CategoryIndex || !m_Enabled) {
                return;
            }

            var activeItemSetIndex = m_ItemSetManager.ActiveItemSetIndex[m_CategoryIndex];
            if (activeItemSetIndex != -1) {
                return;
            }

            // The ItemSet may need to be enabled.
            if (!m_EquipUnequip.IsActive) {
                m_EquipUnequip.StartEquipUnequip(m_Index);
            }
        }

        /// <summary>
        /// The ItemSet has been destroyed.
        /// </summary>
        public void OnDestroy()
        {
            if (m_ItemSetManager == null) {
                return;
            }
            EventHandler.UnregisterEvent<int, int>(m_ItemSetManager.gameObject, "OnItemSetManagerUpdateItemSet", OnUpdateItemSet);
        }
    }

    /// <summary>
    /// Contains a list of ItemSets which belong in the same grouping.
    /// </summary>
    [System.Serializable]
    public class CategoryItemSet
    {
        [Tooltip("The ID of the category.")]
        [SerializeField] protected uint m_CategoryID;
        [Tooltip("The name of the category.")]
        [SerializeField] protected string m_CategoryName;
        [Tooltip("The index of the ItemSet that should be activated when no other ItemSets are activated.")]
        [SerializeField] protected int m_DefaultItemSetIndex = -1;
        [Tooltip("A list of the belonging ItemSets.")]
        [SerializeField] protected List<ItemSet> m_ItemSetList;

        public uint CategoryID {
            get {
                if (RandomID.IsIDEmpty(m_CategoryID)) { m_CategoryID = RandomID.Generate(); }
                return m_CategoryID;
            }
            set { m_CategoryID = value; }
        }
        public string CategoryName { get { return m_CategoryName; } set { m_CategoryName = value; } }
        public int DefaultItemSetIndex { get { return m_DefaultItemSetIndex; } set { m_DefaultItemSetIndex = value; } }
        public List<ItemSet> ItemSetList { get { return m_ItemSetList; } set { m_ItemSetList = value; } }

        private IItemCategoryIdentifier m_ItemCategory;

        public IItemCategoryIdentifier ItemCategory { get { return m_ItemCategory; } set { m_ItemCategory = value; } }

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
        /// <param name="categoryName">The name of the category.</param>
        /// <param name="itemCategory">The category that the ItemSet belongs to.</param>
        public CategoryItemSet(uint categoryID, string categoryName, IItemCategoryIdentifier itemCategory)
        {
            CategoryID = categoryID;
            m_CategoryName = categoryName;
            m_ItemCategory = itemCategory;
            m_ItemSetList = new List<ItemSet>();
        }
    }
}