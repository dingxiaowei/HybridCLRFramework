/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.UI
{
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Inventory;
    using UnityEngine;

    /// <summary>
    /// Manages the magic zone. Allows switching between magic items.
    /// </summary>
    public class MagicZone : UIZone
    {
        [Tooltip("A mapping of the ItemDefinitions to the button.")]
        [SerializeField] protected ItemDefinitionBase[] m_ItemDefinitions;
        [Tooltip("The parent GameObject that should be enabled when the corresponding ItemDefinition is enabled.")]
        [SerializeField] protected GameObject[] m_Objects;
        [Tooltip("The Particle Stream Item Definition.")]
        [SerializeField] protected ItemDefinitionBase m_ParticleStreamItemDefinition;
        [Tooltip("A reference to the Attribute Monitor for the Particle Stream.")]
        [SerializeField] protected GameObject m_ParticleStreamAttributeMonitor;

        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private InventoryBase m_Inventory;
        private EquipUnequip[] m_EquipUnequipAbilities;
        private int[] m_ActiveItemSets;
        private int m_Index;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Start()
        {
            var character = GameObject.FindObjectOfType<DemoManager>().Character;
            m_CharacterLocomotion = character.GetComponent<UltimateCharacterLocomotion>();
            m_Inventory = character.GetComponent<InventoryBase>();
            m_EquipUnequipAbilities = m_CharacterLocomotion.GetAbilities<EquipUnequip>();
            m_ActiveItemSets = new int[m_EquipUnequipAbilities.Length];
            m_Index = -1;

            for (int i = 0; i < m_Objects.Length; ++i) {
                m_Objects[i].SetActive(false);
            }
            m_ParticleStreamAttributeMonitor.SetActive(false);
        }

        /// <summary>
        /// The character has entered from the zone.
        /// </summary>
        /// <param name="characterLocomotion">The character that entered the zone.</param>
        protected override void CharacterEnter(UltimateCharacterLocomotion characterLocomotion)
        {
            for (int i = 0; i < m_ActiveItemSets.Length; ++i) {
                m_ActiveItemSets[i] = m_EquipUnequipAbilities[i].ActiveItemSetIndex;
            }
            ChangeMagicItem(0); // Start with the first item.
        }

        /// <summary>
        /// Change the magic item to the specified item.
        /// </summary>
        /// <param name="itemIndex">The item index to change the value to.</param>
        public void ChangeMagicItem(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= m_ItemDefinitions.Length || itemIndex == m_Index) {
                return;
            }
            if (m_CharacterLocomotion.IsAbilityTypeActive<Use>()) {
                return;
            }

            if (m_Index != -1) {
                m_Objects[m_Index].SetActive(false);
                m_ParticleStreamAttributeMonitor.SetActive(false);
                SetButtonColor(m_Index, m_NormalColor);
            }
            m_Index = itemIndex;
            m_Inventory.Pickup(m_ItemDefinitions[itemIndex].CreateItemIdentifier(), 1, 0, false, true);
            m_Objects[m_Index].SetActive(true);
            for (int i = 0; i < m_Objects[m_Index].transform.childCount; ++i) {
                m_Objects[m_Index].transform.GetChild(i).gameObject.SetActive(true);
            }
            if (m_ItemDefinitions[itemIndex] == m_ParticleStreamItemDefinition) {
                m_ParticleStreamAttributeMonitor.SetActive(true);
            }
            SetButtonColor(m_Index, m_PressedColor);
        }

        /// <summary>
        /// The character has exited from the zone.
        /// </summary>
        /// <param name="characterLocomotion">The character that exited the zone.</param>
        protected override void CharacterExit(UltimateCharacterLocomotion characterLocomotion) 
        { 
            if (m_Index == -1) {
                return;
            }

            m_Objects[m_Index].SetActive(false);
            m_ParticleStreamAttributeMonitor.SetActive(false);
            SetButtonColor(m_Index, m_NormalColor);
            m_Index = -1;

            for (int i = 0; i < m_ActiveItemSets.Length; ++i) {
                if (m_ActiveItemSets[i] == -1) {
                    continue;
                }

                m_EquipUnequipAbilities[i].StartEquipUnequip(m_ActiveItemSets[i], true);
            }
        }
    }
}