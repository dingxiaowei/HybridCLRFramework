/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character.Abilities.Items
{
    /// <summary>
    /// The ItemSetAbilityBase ability acts as a base class for common ItemSet operations such as equipping the previous or next item.
    /// </summary>
    public abstract class ItemSetAbilityBase : ItemAbility
    {
        [Tooltip("The category that the ability should respond to.")]
        [HideInInspector] [SerializeField] protected int m_ItemSetCategoryID;

        public int ItemSetCategoryID { get { return m_ItemSetCategoryID; } set { m_ItemSetCategoryID = value; } }

        protected EquipUnequip m_EquipUnequipItemAbility;
        protected ItemSetManager m_ItemSetManager;
        protected int m_ItemSetCategoryIndex;

        public int ItemSetCategoryIndex { get { return m_ItemSetCategoryIndex; } }

        /// <summary>
        /// Register for any interested events.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_ItemSetManager = m_GameObject.GetCachedComponent<ItemSetManager>();
            // If the CategoryID is 0 then the category hasn't been initialized. Use the first category index.
            if (m_ItemSetCategoryID == 0 && m_ItemSetManager.CategoryItemSets.Length > 0) {
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