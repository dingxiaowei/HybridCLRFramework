/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Items;
    using UnityEngine;

    /// <summary>
    /// Enables or disables the cave light based on the flashlight equipped status.
    /// </summary>
    public class FlashlightCave : MonoBehaviour
    {
        [Tooltip("The ItemDefinition used by the flashlight.")]
        [SerializeField] protected ItemDefinitionBase m_FlashlightItemDefinition;
        [Tooltip("The light within the cave.")]
        [SerializeField] protected Light m_Light;

        private GameObject m_Character;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Character = FindObjectOfType<DemoManager>().Character;

            EventHandler.RegisterEvent<Item, int>(m_Character, "OnInventoryEquipItem", OnEquipItem);
            EventHandler.RegisterEvent<Item, int>(m_Character, "OnInventoryUnequipItem", OnUnequipItem);
        }

        /// <summary>
        /// The character has equipped or unequipped the flashlight.
        /// </summary>
        /// <param name="equipped">True if the flashlight was equipped.</param>
        private void FlashlightEquippedUnequipped(bool equipped)
        {
            m_Light.enabled = !equipped;
        }

        /// <summary>
        /// An item has been equipped.
        /// </summary>
        /// <param name="item">The equipped item.</param>
        /// <param name="slotID">The slot that the item now occupies.</param>
        private void OnEquipItem(Item item, int slotID)
        {
            if (item.ItemDefinition != m_FlashlightItemDefinition) {
                return;
            }

            FlashlightEquippedUnequipped(true);
        }

        /// <summary>
        /// An item has been unequipped.
        /// </summary>
        /// <param name="item">The item that was unequipped.</param>
        /// <param name="slotID">The slot that the item was unequipped from.</param>
        private void OnUnequipItem(Item item, int slotID)
        {
            if (item.ItemDefinition != m_FlashlightItemDefinition) {
                return;
            }

            FlashlightEquippedUnequipped(false);
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<Item, int>(m_Character, "OnInventoryEquipItem", OnEquipItem);
            EventHandler.UnregisterEvent<Item, int>(m_Character, "OnInventoryUnequipItem", OnUnequipItem);
        }
    }
}