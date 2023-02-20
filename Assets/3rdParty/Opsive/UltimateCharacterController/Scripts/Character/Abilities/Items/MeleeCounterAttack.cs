/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character.Abilities.Items
{
    /// <summary>
    /// Plays a counter attack in response to an opponent's attack. In order for the counter attack ability to start the character
    /// must first block the opponent's melee attack.
    /// </summary>
    [DefaultStartType(AbilityStartType.ButtonDown)]
    [DefaultInputName("Fire1")]
    [DefaultItemStateIndex(2)]
    [DefaultState("Use")]
    [AllowMultipleAbilityTypes]
    public class MeleeCounterAttack : Use
    {
        [Tooltip("The maximum distance away from the opponent that the counter attack can start.")]
        [SerializeField] protected float m_AttackDistance = 0.6f;
        [Tooltip("The counter attack can start if the character blocked an attack within the specified amount of time.")]
        [SerializeField] protected float m_CounterAttackTimeFrame = 1f;

        private RaycastHit m_RaycastHit;

        private float m_ImpactTime = -1;
        private MeleeWeapon m_OpponentMeleeWeapon;
        private UltimateCharacterLocomotion m_OpponentLocomotion;
        private Use m_OpponentUseAbility;

        public override bool CanReceiveMultipleStarts { get { return false; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            EventHandler.RegisterEvent<ItemAbility, bool>(m_GameObject, "OnCharacterItemAbilityActive", OnItemAbilityActive);
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <returns>True if the item can be used.</returns>
        public override bool CanStartAbility()
        {
            if (!base.CanStartAbility()) {
                return false;
            }

            if (m_ImpactTime == -1 || m_ImpactTime + m_CounterAttackTimeFrame < Time.time) {
                return false;
            }

            // The IUsableWeapon must be a MeleeWeapon.
            var usableMeleeWeapon = false;
            if (m_SlotID == -1) {
                for (int i = 0; i < m_UsableItems.Length; ++i) {
                    if (m_UsableItems[i] is MeleeWeapon) {
                        usableMeleeWeapon = true;
                    } else {
                        m_UsableItems[i] = null;
                    }
                }
            } else {
                if (m_UsableItems[0] is MeleeWeapon) {
                    usableMeleeWeapon = true;
                } else {
                    m_UsableItems[0] = null;
                }
            }

            if (!usableMeleeWeapon) {
                return false;
            }

            // The opponent must be in front of the character.
            if (!m_CharacterLocomotion.SingleCast(m_Transform.forward * m_AttackDistance, Vector3.zero, m_CharacterLayerManager.EnemyLayers, ref m_RaycastHit) || 
                m_RaycastHit.collider.gameObject.GetCachedParentComponent<UltimateCharacterLocomotion>() != m_OpponentLocomotion) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        /// <returns>True if the ability should be stopped.</returns>
        public override bool ShouldStopActiveAbility(Ability activeAbility)
        {
            if (base.ShouldStopActiveAbility(activeAbility)) {
                return true;
            }

            return (activeAbility != this && activeAbility is Use);
        }

        /// <summary>
        /// Called when another ability is attempting to start and the current ability is active.
        /// Returns true or false depending on if the new ability should be blocked from starting.
        /// </summary>
        /// <param name="startingAbility">The ability that is starting.</param>
        /// <returns>True if the ability should be blocked.</returns>
        public override bool ShouldBlockAbilityStart(Ability startingAbility)
        {
            if (base.ShouldBlockAbilityStart(startingAbility)) {
                return true;
            }

            return (startingAbility != this && startingAbility is Use);
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            // The opponent should play an animation which responds to the counter attack.
            var opponentResponseAbility = m_OpponentLocomotion.GetAbility<MeleeCounterAttackResponse>();
            if (opponentResponseAbility == null) {
                return;
            }

            var substateIndex = -1;
            for (int i = 0; i < m_Inventory.SlotCount; ++i) {
                substateIndex = GetItemSubstateIndex(i);
                if (substateIndex != -1) {
                    break;
                }
            }
            if (substateIndex != -1) {
                opponentResponseAbility.StartResponse(substateIndex);
            }
        }

        /// <summary>
        /// Returns the Item Substate Index which corresponds to the slot ID.
        /// </summary>
        /// <param name="slotID">The ID of the slot that corresponds to the Item Substate Index.</param>
        /// <returns>The Item Substate Index which corresponds to the slot ID.</returns>
        public override int GetItemSubstateIndex(int slotID)
        {
            var substateIndex = base.GetItemSubstateIndex(slotID);
            if (substateIndex == -1) {
                return -1;
            }
            return MathUtility.Concatenate(m_OpponentMeleeWeapon.Item.AnimatorItemID, m_OpponentMeleeWeapon.UsedSubstateIndex, substateIndex);
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            m_OpponentMeleeWeapon = null;
            m_ImpactTime = -1;
        }

        /// <summary>
        /// An ItemAbility has been activated or deactivated.
        /// </summary>
        /// <param name="itemAbility">The ItemAbility activated or deactivated.</param>
        /// <param name="active">Was the ItemAbility activated?</param>
        private void OnItemAbilityActive(ItemAbility itemAbility, bool active)
        {
            if (!active || IsActive) {
                return;
            }

            // If another use ability is started or a use is active then the character shouldn't be able to counter attack.
            if (!(itemAbility is Block) || m_CharacterLocomotion.IsAbilityTypeActive<Use>()) {
                m_ImpactTime = -1;
                return;
            }

            // The block ability has been activated. The source of the block must be a melee weapon - counter attack doesn't work against non-melee weapons.
            var blockAbility = itemAbility as Block;
            m_OpponentMeleeWeapon = null;
            for (int i = 0; i < blockAbility.ImpactSources.Length; ++i) {
                if (blockAbility.ImpactSources[i] == null || !(blockAbility.ImpactSources[i] is MeleeWeapon)) {
                    continue;
                }

                m_OpponentMeleeWeapon = blockAbility.ImpactSources[i] as MeleeWeapon;
                break;
            }

            if (m_OpponentMeleeWeapon == null) {
                return;
            }

            // The opponent must actively be attacking.
            m_OpponentLocomotion = m_OpponentMeleeWeapon.CharacterLocomotion;
            m_OpponentUseAbility = null;
            var useAbilities = m_OpponentLocomotion.GetAbilities<Use>();
            if (useAbilities == null || useAbilities.Length == 0) {
                m_ImpactTime = -1;
                return;
            }

            for (int i = 0; i < useAbilities.Length; ++i) {
                if (!useAbilities[i].IsActive) {
                    continue;
                }

                // The ability is active. Ensure it is using a melee weapon.
                for (int j = 0; j < useAbilities[i].UsableItems.Length; ++j) {
                    var meleeWeapon = useAbilities[i].UsableItems[j] as MeleeWeapon;
                    if (meleeWeapon != m_OpponentMeleeWeapon) {
                        continue;
                    }

                    m_OpponentUseAbility = useAbilities[i];
                    break;
                }

                if (m_OpponentUseAbility != null) {
                    break;
                }
            }

            if (m_OpponentUseAbility == null) {
                m_ImpactTime = -1;
                return;
            }

            m_ImpactTime = Time.time;
        }

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<ItemAbility, bool>(m_GameObject, "OnCharacterItemAbilityActive", OnItemAbilityActive);
        }
    }
}