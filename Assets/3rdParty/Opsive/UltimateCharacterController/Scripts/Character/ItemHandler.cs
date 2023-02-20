/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Input;
using Opsive.UltimateCharacterController.Inventory;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character
{
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
        }

        /// <summary>
        /// Moves the item in each slot.
        /// </summary>
        private void FixedUpdate()
        {
            var lookVector = m_PlayerInput.GetLookVector(false);
            for (int i = 0; i < m_Inventory.SlotCount; ++i) {
                var item = m_Inventory.GetItem(i);
                if (item != null && item.IsActive() && item.DominantItem) {
                    item.Move(lookVector.x, lookVector.y);
                }
            }

            // Each object should only be updated once. Clear the frame after execution to allow the objects to be updated again.
            UnityEngineUtility.ClearUpdatedObjects();
        }
    }
}