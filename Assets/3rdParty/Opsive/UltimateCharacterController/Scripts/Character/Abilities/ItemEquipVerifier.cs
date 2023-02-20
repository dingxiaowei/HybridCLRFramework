/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character.Abilities.Items;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Utility;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    /// Verifies that the items are equipped or unequipped according to the AllowEquippedSlotsMask.
    /// </summary>
    [DefaultStartType(AbilityStartType.Manual)]
    public class ItemEquipVerifier : Ability
    {
        public override bool IsConcurrent { get { return true; } }

        private bool m_Equip;
        private Ability m_OriginalAbility;
        private bool m_UnequippedItems;
        private ItemSetManager m_ItemSetManager;
        private bool m_CanToggleItem = true;
        private bool m_Active;
        private bool m_CanStopAbility;
        private HashSet<ItemAbility> m_ActiveEquipUnequipAbilities = new HashSet<ItemAbility>();

        private int m_StartEquippedSlotMask;
        private bool m_StartAllowPositionalInput;
        private bool m_StartAllowRotationalInput;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_ItemSetManager = m_GameObject.GetCachedComponent<ItemSetManager>();
        }

        /// <summary>
        /// Tries to toggle an equip or unequip based on the AllowedEquippedSlotsMask. True will be returned if the ability starts.
        /// </summary>
        /// <param name="ability">The ability that is trying to be started/stopped.</param>
        /// <param name="activate">True if the ability is being activated, false if it is being deactivated.</param>
        /// <returns>True if the ability started.</returns>
        public bool TryToggleItem(Ability ability, bool activate)
        {
            if (!Enabled || !m_CanToggleItem || ability is PickupItem) {
                return false;
            }

            // No need to run again if the ability is already working on toggling the item equip.
            if (ability == m_OriginalAbility || (m_Active && activate)) {
                return ability == m_OriginalAbility;
            }

            var start = false;
            // If the ability is activated then the current set of items may need to be unequipped.
            if (activate && ability.AllowEquippedSlotsMask != -1) {
                // The ability may not need to activate if the not allowed equipped items are already not equipped.
                var currentEquippedSlots = 0;
                Item item;
                for (int i = 0; i < m_Inventory.SlotCount; ++i) {
                    if ((item = m_Inventory.GetItem(i)) != null) {
                        // If the current ItemSet is the defualt ItemSet then the current item can be considered unequipped. This for example will allow the body item
                        // (for puncing/kicking) to be active even if the ability says no item should be active.
                        var isDefault = true;
                        if (m_ItemSetManager != null) {
                            for (int j = 0; j < item.ItemType.CategoryIndices.Length; ++j) {
                                var categoryIndex = item.ItemType.CategoryIndices[j];
                                var activeItemSetIndex = m_ItemSetManager.ActiveItemSetIndex[categoryIndex];
                                if (m_ItemSetManager.CategoryItemSets[categoryIndex].DefaultItemSetIndex != activeItemSetIndex) {
                                    isDefault = false;
                                    break;
                                } 
                            }
                        } else {
                            isDefault = false;
                        }
                        // The equipped item is a defualt item - it can be considered unequipped.
                        if (isDefault) {
                            continue;
                        }
                        currentEquippedSlots |= 1 << i;
                    }
                }

                if (currentEquippedSlots != 0 && !MathUtility.InLayerMask(currentEquippedSlots, ability.AllowEquippedSlotsMask)) {
                    m_Equip = false;
                    start = true;
                    m_UnequippedItems = true;
                } else {
                    m_UnequippedItems = false;
                }

            // If slots were unequipped when the ability started then they should be equipped when the ability stops.
            } else if (!activate && ability.ReequipSlots && ability.AllowEquippedSlotsMask != -1 && m_UnequippedItems) {
                m_Equip = true;
                start = true;
            }

            if (start) {
                if (ability is MoveTowards) {
                    ability = (ability as MoveTowards).OnArriveAbility;
                }

                // When the ability is unequipping the items it should inherit the allow field values from the starting ability.
                // When the ability is complete (equpping the items) it should use the starting field values.
                if (!m_Equip) {
                    m_StartEquippedSlotMask = m_AllowEquippedSlotsMask;
                    m_StartAllowPositionalInput = m_AllowPositionalInput;
                    m_StartAllowRotationalInput = m_AllowRotationalInput;

                    m_AllowEquippedSlotsMask = ability.AllowEquippedSlotsMask;
                    m_AllowPositionalInput = ability.AllowPositionalInput;
                    m_AllowRotationalInput = ability.AllowRotationalInput;
                } else {
                    m_AllowEquippedSlotsMask = m_StartEquippedSlotMask;
                    m_AllowPositionalInput = m_StartAllowPositionalInput;
                    m_AllowRotationalInput = m_StartAllowRotationalInput;
                }

                // Active should only be true if the ability is equipping and the original ability is reequipping the slots. If the ability is not
                // reequipping slots then the Item Equip Verifier will not be run again after it is complete.
                m_Active = !m_Equip && ability.ReequipSlots;
                m_OriginalAbility = ability;
                StartAbility();
            }
            return start;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            m_ActiveEquipUnequipAbilities.Clear();
            EventHandler.RegisterEvent<ItemAbility, bool>(m_GameObject, "OnCharacterItemAbilityActive", OnItemAbilityActive);
            // The original ability may be null on the network.
            if (m_OriginalAbility != null) {
                m_CanStopAbility = !m_Equip && m_OriginalAbility.ImmediateUnequip;
                EventHandler.ExecuteEvent(m_GameObject, "OnAbilityToggleSlots", m_AllowEquippedSlotsMask, m_Equip, !m_Equip && m_OriginalAbility.ImmediateUnequip);
            }
            m_CanStopAbility = true;

            // If the count is still zero then all of the items were unequipped in a single frame.
            if (m_ActiveEquipUnequipAbilities.Count == 0) {
                ItemToggled();
            }
        }

        /// <summary>
        /// An ItemAbility has been activated or deactivated.
        /// </summary>
        /// <param name="itemAbility">The ItemAbility activated or deactivated.</param>
        /// <param name="active">Was the ItemAbility activated?</param>
        private void OnItemAbilityActive(ItemAbility itemAbility, bool active)
        {
            if (itemAbility is EquipUnequip) {
                // Keep a count of the number of EquipUnequip abilities started. This will allow the ability to call ItemToggled when no more 
                // EquipUnequip abilities are active.
                if (active) {
                    m_ActiveEquipUnequipAbilities.Add(itemAbility);
                } else {
                    m_ActiveEquipUnequipAbilities.Remove(itemAbility);
                }
                if (m_ActiveEquipUnequipAbilities.Count == 0 && m_CanStopAbility) {
                    ItemToggled();
                }
            }
        }

        /// <summary>
        /// The EquipUnequip ability has toggled an item slot.
        /// </summary>
        private void ItemToggled()
        {
            if (m_OriginalAbility == null) {
                return;
            }

            // Stop the ability before starting the OriginalAbility ability so ItemEquipVerifier doesn't prevent the ability from starting.
            StopAbility();

            // The ability should only be started if the items were unequipped and the MoveTowards ability isn't active. If the MoveTowards ability is 
            // active then the MoveTowards ability will start the ability.
            if (!m_Equip && (m_CharacterLocomotion.MoveTowardsAbility == null || m_CharacterLocomotion.MoveTowardsAbility.OnArriveAbility == null)) {
                m_CanToggleItem = false;
                if (!m_OriginalAbility.IsActive) {
                    m_CharacterLocomotion.TryStartAbility(m_OriginalAbility, true, true);
                } else if (m_OriginalAbility is IItemToggledReceiver) {
                    // If the ability is already active then the ability is the one that toggled the item and it should receive the callback.
                    (m_OriginalAbility as IItemToggledReceiver).ItemToggled();
                }
                m_CanToggleItem = true;
            }

            m_OriginalAbility = null;
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            EventHandler.UnregisterEvent<ItemAbility, bool>(m_GameObject, "OnCharacterItemAbilityActive", OnItemAbilityActive);
        }

        /// <summary>
        /// Resets the ability back to the starting state.
        /// </summary>
        public void Reset()
        {
            m_OriginalAbility = null;
            m_Active = false;
        }
    }
}