/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Events;
    using Opsive.UltimateCharacterController.Input;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// The ItemHandler manages the movement for each equipped item.
    /// </summary>
    public class ItemHandler : MonoBehaviour
    {
        private InventoryBase m_Inventory;
        private PlayerInput m_PlayerInput;
        
        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Start()
        {
            m_PlayerInput = gameObject.GetCachedComponent<PlayerInput>();
            m_Inventory = gameObject.GetCachedComponent<InventoryBase>();

            EventHandler.RegisterEvent<Items.Item, int>(gameObject, "OnInventoryEquipItem", OnEquipItem);
        }

        /// <summary>
        /// Moves the item in each slot.
        /// </summary>
        private void FixedUpdate()
        {
            var lookVector = m_PlayerInput.GetLookVector(false);
            for (int i = 0; i < m_Inventory.SlotCount; ++i) {
                var item = m_Inventory.GetActiveItem(i);
                if (item != null && item.IsActive() && item.DominantItem) {
                    item.Move(lookVector.x, lookVector.y);
                }
            }

            // Each object should only be updated once. Clear the frame after execution to allow the objects to be updated again.
            UnityEngineUtility.ClearUpdatedObjects();
        }

        /// <summary>
        /// An item has been equipped.
        /// </summary>
        /// <param name="item">The equipped item.</param>
        /// <param name="slotID">The slot that the item now occupies.</param>
        private void OnEquipItem(Items.Item item, int slotID)
        {
            if (item.IsActive() && item.DominantItem) {
                UnityEngineUtility.ClearUpdatedObjects();
                item.Move(0, 0);
            }
        }

        /// <summary>
        /// The character has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<Items.Item, int>(gameObject, "OnInventoryEquipItem", OnEquipItem);
        }
    }
}