/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities.Items
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Inventory;
    using UnityEngine;

    /// <summary>
    /// The ItemSetAbilityBase ability acts as a base class for common ItemSet operations such as equipping the previous or next item.
    /// </summary>
    public abstract class ItemSetAbilityBase : ItemAbility
    {
        [Tooltip("The category that the ability should respond to.")]
        [HideInInspector] [SerializeField] protected uint m_ItemSetCategoryID;
        public uint ItemSetCategoryID { get { return m_ItemSetCategoryID; } }

        protected EquipUnequip m_EquipUnequipItemAbility;
        protected ItemSetManagerBase m_ItemSetManager;
        protected int m_ItemSetCategoryIndex;

        public int ItemSetCategoryIndex { get { return m_ItemSetCategoryIndex; } }

        /// <summary>
        /// Register for any interested events.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_ItemSetManager = m_GameObject.GetCachedComponent<ItemSetManagerBase>();
            m_ItemSetManager.Initialize(false);
            // If the CategoryID is empty then the category hasn't been initialized. Use the first category index.
            if (RandomID.IsIDEmpty(m_ItemSetCategoryID) && m_ItemSetManager.CategoryItemSets.Length > 0) {
                m_ItemSetCategoryID = m_ItemSetManager.CategoryItemSets[0].CategoryID;
            }
            m_ItemSetCategoryIndex = m_ItemSetManager.CategoryIDToIndex(m_ItemSetCategoryID);

            var equipUnequipAbilities = GetAbilities<EquipUnequip>();
            if (equipUnequipAbilities != null) {
                // The ItemSet CategoryID must match for the ToggleEquip ability to be able to use the EquipUnequip ability.
                for (int i = 0; i < equipUnequipAbilities.Length; ++i) {
                    if (equipUnequipAbilities[i].ItemSetCategoryID == m_ItemSetCategoryID) {
                        m_EquipUnequipItemAbility = equipUnequipAbilities[i];
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            // Use and Reload can prevent the ability from equipping or unequipping items.
            if (m_CharacterLocomotion.IsAbilityTypeActive<Use>()
#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
                || m_CharacterLocomotion.IsAbilityTypeActive<Reload>()
#endif
                ) {
                return false;
            }
            return base.CanStartAbility();
        }
    }
}