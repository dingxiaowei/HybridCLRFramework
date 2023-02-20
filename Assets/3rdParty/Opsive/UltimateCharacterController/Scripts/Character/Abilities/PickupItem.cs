/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Character.Abilities.Items;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Objects.CharacterAssist;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    /// <summary>
    /// Plays an animation which picks up the item.
    /// </summary>
    [DefaultStartType(AbilityStartType.ButtonDown)]
    [DefaultInputName("Action")]
    [DefaultAbilityIndex(11)]
    [DefaultObjectDetection(ObjectDetectionMode.Trigger)]
    [DefaultReequipSlots(false)]
    public class PickupItem : DetectObjectAbilityBase
    {
        [Tooltip("The slot ID to pick up. A value of -1 indicates any slot.")]
        [SerializeField] protected int m_SlotID = -1;
        [Tooltip("Specifies a list of ItemTypes that should be picked up. If the list is empty any ItemType will trigger the animation.")]
        [SerializeField] protected Inventory.ItemType[] m_PickupItemTypes;
        [Tooltip("Specifies if the ability should wait for the OnAnimatorPickupItem animation event or wait for the specified duration before picking up the item.")]
        [SerializeField] protected AnimationEventTrigger m_PickupEvent = new AnimationEventTrigger(true, 0.2f);
        [Tooltip("Specifies if the ability should wait for the OnAnimatorPickupItemComplete animation event or wait for the specified duration before stopping the ability.")]
        [SerializeField] protected AnimationEventTrigger m_PickupCompleteEvent = new AnimationEventTrigger(false, 0.4f);

        public int SlotID { get { return m_SlotID; } set { m_SlotID = value; } }
        public Inventory.ItemType[] PickupItemTypes { get { return m_PickupItemTypes; } set { m_PickupItemTypes = value; } }
        public AnimationEventTrigger PickupEvent { get { return m_PickupEvent; } set { m_PickupEvent = value; } }
        public AnimationEventTrigger PickupCompleteEvent { get { return m_PickupCompleteEvent; } set { m_PickupCompleteEvent = value; } }

        public override bool CanReceiveMultipleStarts { get { return true; } }
        public override bool ImmediateStartItemVerifier { get { return true; } }

        private ItemPickup m_ItemPickup;
        private ItemPickup[] m_AvailableItemPickups;
        private int m_AvailablePickupCount;
        private EquipUnequip[] m_EquipUnequipAbilities;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_AvailableItemPickups = new ItemPickup[m_MaxTriggerObjectCount];
            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorPickupItem", DoItemPickup);
            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorPickupItemComplete", PickupComplete);
        }

        /// <summary>
        /// Cache the abilities.
        /// </summary>
        public override void Start()
        {
            m_EquipUnequipAbilities = m_CharacterLocomotion.GetAbilities<EquipUnequip>();
        }

        /// <summary>
        /// Returns true if the ItemPickup component should pickup the item.
        /// </summary>
        /// <returns>True if the ItemPickup component should pickup the item.</returns>
        public bool CanItemPickup()
        {
            // The ItemPickup component should always pickup the item if the Ride ability is active.
            if (m_CharacterLocomotion.IsAbilityTypeActive<Ride>()) {
                return true;
            }
            // The ItemPickup component should always pickup the item if there are any active higher priority abilities active.
            for (int i = 0; i < m_CharacterLocomotion.ActiveAbilityCount; ++i) {
                if (m_CharacterLocomotion.ActiveAbilities[i].Index > Index) {
                    break;
                }
                if (!m_CharacterLocomotion.ActiveAbilities[i].IsConcurrent) {
                    return true;
                }
            }
            return !Enabled;
        }

        /// <summary>
        /// Validates the object to ensure it is valid for the current ability.
        /// </summary>
        /// <param name="obj">The object being validated.</param>
        /// <param name="fromTrigger">Is the object being validated from a trigger?</param>
        /// <returns>True if the object is valid. The object may not be valid if it doesn't have an ability-specific component attached.</returns>
        protected override bool ValidateObject(GameObject obj, bool fromTrigger)
        {
            if (!base.ValidateObject(obj, fromTrigger)) {
                return false;
            }

            if (m_AvailablePickupCount > 0) {
                for (int i = 0; i < m_AvailablePickupCount; ++i) {
                    if (obj == m_AvailableItemPickups[i].gameObject) {
                        return true;
                    }
                }
            }

            ItemPickup itemPickup;
            if ((itemPickup = obj.GetCachedComponent<ItemPickup>()) != null && !itemPickup.PickupOnTriggerEnter && !itemPickup.IsDepleted) {
                m_AvailableItemPickups[m_AvailablePickupCount] = itemPickup;
                m_AvailablePickupCount++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            // An attribute may prevent the ability from starting.
            if (!base.CanStartAbility()) {
                return false;
            }

            if (m_AvailablePickupCount == 0) {
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            // If the item pickup isn't null then the ability is currently working on equipping another item.
            if (m_ItemPickup != null && m_ItemPickup != m_AvailableItemPickups[0]) {
                m_ItemPickup.DoItemTypePickup(m_GameObject, m_Inventory, m_SlotID, true, false);
            }

            m_ItemPickup = m_AvailableItemPickups[0];
            m_AvailablePickupCount--;
            for (int i = 0; i < m_AvailablePickupCount; ++i) {
                m_AvailableItemPickups[i] = m_AvailableItemPickups[i + 1];
            }

            // If the PickupItemType array contains any ItemTypes then the PickupItem ability should only start if the PickupItem object contains one of the ItemTypes
            // within the array. If it doesn't contain the ItemType then that ItemType should be equipped as if the PickupItem ability doesn't exist.
            var immediatePickup = false;
            if (m_PickupItemTypes != null && m_PickupItemTypes.Length > 0) {
                immediatePickup = true;
                for (int i = 0; i < m_PickupItemTypes.Length; ++i) {
                    for (int j = 0; j < m_ItemPickup.ItemTypeCounts.Length; ++j) {
                        if (m_PickupItemTypes[i] == m_ItemPickup.ItemTypeCounts[j].ItemType) {
                            immediatePickup = false;
                            break;
                        }
                    }
                    if (immediatePickup) {
                        break;
                    }
                }
            }

            m_ItemPickup.DoItemPickup(m_GameObject, m_Inventory, m_SlotID, !immediatePickup, immediatePickup);

            // The ability shouldn't start if the ItemType has already been picked up.
            if (immediatePickup) {
                StopAbility();
                return;
            }

            m_AllowEquippedSlotsMask = (1 << m_Inventory.SlotCount) - 1;

            // Before the item can be picked up the currently equipped items need to be unequipped.
            for (int i = 0; i < m_EquipUnequipAbilities.Length; ++i) {
                m_EquipUnequipAbilities[i].WillStartPickup();
            }
            for (int i = 0; i < m_ItemPickup.ItemTypeCounts.Length; ++i) {
                var itemType = m_ItemPickup.ItemTypeCounts[i].ItemType;

                for (int j = 0; j < m_Inventory.SlotCount; ++j) {
                    var item = m_Inventory.GetItem(j, itemType);
                    if (item == null) {
                        continue;
                    }

                    // Determine if the item should be equipped. The current item needs to be unequipped if it doesn't match the item being picked up.
                    var categoryIndex = 0;
                    var shouldEquip = false;
                    for (int k = 0; k < m_EquipUnequipAbilities.Length; ++k) {
                        if (itemType.CategoryIDMatch(m_EquipUnequipAbilities[k].ItemSetCategoryID) && m_EquipUnequipAbilities[k].ShouldEquip(item, m_ItemPickup.ItemTypeCounts[i].Count)) {
                            shouldEquip = true;
                            categoryIndex = m_EquipUnequipAbilities[k].ItemSetCategoryIndex;
                            break;
                        }
                    }

                    if (!shouldEquip) {
                        continue;
                    }

                    // The item should be equipped.
                    var equippedItemType = m_Inventory.GetItem(item.SlotID);
                    if (itemType != equippedItemType) {
                        m_AllowEquippedSlotsMask &= ~(1 << item.SlotID);
                    }

                    break;
                }
            }

            // If the item doesn't need to be equipped then it should still be picked up.
            if (m_AllowEquippedSlotsMask == (1 << m_Inventory.SlotCount) - 1) {
                m_ItemPickup.DoItemPickup(m_GameObject, m_Inventory, m_SlotID, false, true);
                StopAbility();
                return;
            }

            // If the ability index is -1 then an animation will not play and the item should be picked up immediately.
            if (m_AbilityIndexParameter == -1) {
                DoItemPickup();
            } else if (!m_PickupEvent.WaitForAnimationEvent) {
                Scheduler.ScheduleFixed(m_PickupEvent.Duration, DoItemPickup);
            }
        }

        /// <summary>
        /// Picks up the item.
        /// </summary>
        private void DoItemPickup()
        {
            if (m_ItemPickup == null) {
                return;
            }

            m_ItemPickup.DoItemTypePickup(m_GameObject, m_Inventory, m_SlotID, true, true);

            if (!m_PickupCompleteEvent.WaitForAnimationEvent) {
                Scheduler.ScheduleFixed(m_PickupCompleteEvent.Duration, PickupComplete);
            }
        }

        /// <summary>
        /// Completes the ability.
        /// </summary>
        private void PickupComplete()
        {
            StopAbility();
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force, true);

            m_ItemPickup = null;
        }

        /// <summary>
        /// The character has exited a trigger.
        /// </summary>
        /// <param name="other">The GameObject that the character exited.</param>
        /// <returns>Returns true if the entered object leaves the trigger.</returns>
        protected override bool TriggerExit(GameObject other)
        {
            if (base.TriggerExit(other) && !IsActive) {
                var index = -1;
                for (int i = 0; i < m_AvailablePickupCount; ++i) {
                    if (m_AvailableItemPickups[i].gameObject == other) {
                        m_AvailableItemPickups[i] = null;
                        index = i;
                        break;
                    }
                }
                if (index != -1) {
                    m_AvailablePickupCount--;
                    for (int i = index; i < m_AvailablePickupCount; ++i) {
                        m_AvailableItemPickups[i] = m_AvailableItemPickups[i + 1];
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// The character has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorPickupItem", DoItemPickup);
            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorPickupItemComplete", PickupComplete);
        }
    }
}