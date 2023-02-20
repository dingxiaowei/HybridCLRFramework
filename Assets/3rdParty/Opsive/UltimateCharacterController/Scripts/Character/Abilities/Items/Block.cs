/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character.Abilities.Items
{
    /// <summary>
    /// The Block ability will play a blocking animation when another object comes into contact with the Shield ItemAction.
    /// </summary>
    [DefaultStartType(AbilityStartType.Manual)]
    [DefaultStopType(AbilityStopType.Manual)]
    public class Block : ItemAbility
    {
        [Tooltip("The slot that should be used. -1 will block all of the slots.")]
        [SerializeField] protected int m_SlotID = -1;
        [Tooltip("The Animator's Item State Index when the character blocks.")]
        [SerializeField] protected int m_BlockItemStateIndex = 7;
        [Tooltip("The Animator's Item State Index when the character parries.")]
        [SerializeField] protected int m_ParryItemStateIndex = 8;

        public int SlotID
        {
            get { return m_SlotID; }
            set
            {
                if (m_SlotID != value) {
                    UnregisterSlotEvents(m_SlotID);
                    m_SlotID = value;
                    RegisterSlotEvents(m_SlotID);
                }
            }
        }
        public int BlockItemStateIndex { get { return m_BlockItemStateIndex; } set { m_BlockItemStateIndex = value; } }
        public int ParryItemStateIndex { get { return m_ParryItemStateIndex; } set { m_ParryItemStateIndex = value; } }

        private Shield[] m_Shields;
        private object[] m_ImpactSources;
        private ScheduledEventBase[] m_BlockEvents;
        private bool m_Parry;

        public object[] ImpactSources { get { return m_ImpactSources; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            var count = m_SlotID == -1 ? m_Inventory.SlotCount : 1;
            m_Shields = new Shield[count];
            m_ImpactSources = new object[count];
            m_BlockEvents = new ScheduledEventBase[count];

            EventHandler.RegisterEvent(m_GameObject, "OnAnimatorItemImpactComplete", OnItemImpactComplete);
            EventHandler.RegisterEvent<Shield, object>(m_GameObject, "OnShieldImpact", StartBlock);
            RegisterSlotEvents(m_SlotID);
        }

        /// <summary>
        /// Registers for the interested events according to the slot id.
        /// </summary>
        /// <param name="slotID">The slot id to register for.</param>
        private void RegisterSlotEvents(int slotID)
        {
            if (!Application.isPlaying) {
                return;
            }
            if (slotID == 0) {
                EventHandler.RegisterEvent(m_GameObject, "OnAnimatorItemImpactCompleteFirstSlot", OnItemImpactCompleteFirstSlot);
            } else if (slotID == 1) {
                EventHandler.RegisterEvent(m_GameObject, "OnAnimatorItemImpactCompleteSecondSlot", OnItemImpactCompleteSecondSlot);
            } else if (slotID == 2) {
                EventHandler.RegisterEvent(m_GameObject, "OnAnimatorItemImpactCompleteThirdSlot", OnItemImpactCompleteThirdSlot);
            } else if (slotID != -1) {
                Debug.LogError("Error: The Block ability does not listen to slot " + m_SlotID);
            }
        }

        /// <summary>
        /// Unregisters from the interested events according to the slot id.
        /// </summary>
        /// <param name="slotID">The slot id to unregister from.</param>
        private void UnregisterSlotEvents(int slotID)
        {
            if (!Application.isPlaying) {
                return;
            }
            if (slotID == 0) {
                EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorItemImpactCompleteFirstSlot", OnItemImpactCompleteFirstSlot);
            } else if (slotID == 1) {
                EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorItemImpactCompleteSecondSlot", OnItemImpactCompleteSecondSlot);
            } else if (slotID == 2) {
                EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorItemImpactCompleteThirdSlot", OnItemImpactCompleteThirdSlot);
            }
        }

        /// <summary>
        /// Returns the Item State Index which corresponds to the slot ID.
        /// </summary>
        /// <param name="slotID">The ID of the slot that corresponds to the Item State Index.</param>
        /// <returns>The Item State Index which corresponds to the slot ID.</returns>
        public override int GetItemStateIndex(int slotID)
        {
            // Return the ItemStateIndex if the SlotID matches the requested slotID.
            if (m_SlotID == -1) {
                if (m_Shields[slotID] != null) {
                    return m_Parry ? m_ParryItemStateIndex : m_BlockItemStateIndex;
                }
            } else if (m_SlotID == slotID && m_Shields[0] != null) {
                return m_Parry ? m_ParryItemStateIndex : m_BlockItemStateIndex;
            }
            return -1;
        }

        /// <summary>
        /// Returns the Item Substate Index which corresponds to the slot ID.
        /// </summary>
        /// <param name="slotID">The ID of the slot that corresponds to the Item Substate Index.</param>
        /// <returns>The Item Substate Index which corresponds to the slot ID.</returns>
        public override int GetItemSubstateIndex(int slotID)
        {
            var substateIndex = -1;
            object impactSource = null;
            if (m_SlotID == -1) {
                if (m_Shields[slotID] != null) {
                    substateIndex = m_Shields[slotID].GetItemSubstateIndex();
                    impactSource = m_ImpactSources[slotID];
                }
            } else if (m_SlotID == slotID && m_Shields[0] != null) {
                substateIndex = m_Shields[0].GetItemSubstateIndex();
                impactSource = m_ImpactSources[0];
            }
            if (substateIndex == -1) {
                return -1;
            }

            // If the impact source is a MeleeWeapon determine which attack is being played. This will allow the block integer to depend on
            if (impactSource != null && impactSource is MeleeWeapon) {
                var meleeWeapon = impactSource as MeleeWeapon;
                var meleeUseSubstateIndex = meleeWeapon.UsedSubstateIndex;
                if (meleeUseSubstateIndex != -1) {
                    return MathUtility.Concatenate(meleeWeapon.Item.AnimatorItemID, meleeUseSubstateIndex, substateIndex);
                }
            }

            return substateIndex;
        }

        /// <summary>
        /// An object has impacted the shield. Start the blocking animation.
        /// </summary>
        /// <param name="shield">The shield that was impacted.</param>
        /// <param name="source">The object that is trying to damage the shield.</param>
        public void StartBlock(Shield shield, object source)
        {
            var slotID = shield.Item.SlotID;
            if (m_SlotID != -1 && slotID != m_SlotID) {
                return;
            } else if (slotID == m_SlotID) {
                // If the ability only responds to a single slot then the arrays will always have just a single element.
                slotID = 0;
            }

            if (m_Shields[slotID] != null) {
                return;
            }

            m_Shields[slotID] = shield;
            m_ImpactSources[slotID] = source;
            // The difference between a block and a parry is that a parry will occur if there is a melee weapon.
            m_Parry = shield.gameObject.GetCachedComponent<MeleeWeapon>() != null;
            StartAbility();

            if (!shield.ImpactCompleteEvent.WaitForAnimationEvent) {
                m_BlockEvents[slotID] = Scheduler.ScheduleFixed(shield.ImpactCompleteEvent.Duration, ImpactComplete, slotID);
            }
            m_CharacterLocomotion.UpdateItemAbilityAnimatorParameters();
        }

        /// <summary>
        /// The impact animation has completed for all of the items.
        /// </summary>
        private void OnItemImpactComplete()
        {
            for (int i = 0; i < m_Shields.Length; ++i) {
                if (m_Shields[i] != null) {
                    ImpactComplete(i);
                }
            }
        }

        /// <summary>
        /// The impact animation has completed for the first item slot.
        /// </summary>
        private void OnItemImpactCompleteFirstSlot()
        {
            ImpactComplete(0);
        }

        /// <summary>
        /// The impact animation has completed for the second item slot.
        /// </summary>
        private void OnItemImpactCompleteSecondSlot()
        {
            ImpactComplete(1);
        }

        /// <summary>
        /// The impact animation has completed for the third item slot.
        /// </summary>
        private void OnItemImpactCompleteThirdSlot()
        {
            ImpactComplete(2);
        }

        /// <summary>
        /// The impact animation has completed for the specified slot.
        /// </summary>
        /// <param name="slotID">The slot that has completed the impact.</param>
        private void ImpactComplete(int slotID)
        {
            if (m_Shields[slotID] == null) {
                return;
            }

            m_Shields[slotID].StopBlockImpact();
            m_Shields[slotID] = null;
            m_BlockEvents[slotID] = null;
            m_CharacterLocomotion.UpdateItemAbilityAnimatorParameters();

            var stopAbility = true;
            for (int i = 0; i < m_Shields.Length; ++i) {
                if (m_Shields[i] != null) {
                    stopAbility = false;
                    break;
                }
            }

            if (stopAbility) {
                StopAbility();
            }
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            for (int i = 0; i < m_Shields.Length; ++i) {
                if (m_Shields[i] != null) {
                    m_Shields[i].StopBlockImpact();
                    m_Shields[i] = null;
                    if (m_BlockEvents[i] != null) {
                        Scheduler.Cancel(m_BlockEvents[i]);
                        m_BlockEvents[i] = null;
                    }
                }
            }
        }

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorItemImpactComplete", OnItemImpactComplete);
            EventHandler.UnregisterEvent<Shield, object>(m_GameObject, "OnShieldImpact", StartBlock);
            UnregisterSlotEvents(m_SlotID);
        }
    }
}