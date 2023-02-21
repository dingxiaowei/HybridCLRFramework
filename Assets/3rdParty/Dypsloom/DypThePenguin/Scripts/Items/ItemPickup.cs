/// ---------------------------------------------
/// Dyp Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Items
{
    using Dypsloom.DypThePenguin.Scripts.Interactions;
    using Dypsloom.Shared.Utility;
    using UnityEngine;

    public class ItemPickup : InteractableBehavior
    {
        [Tooltip("Respawn the pickup after x seconds (-1 won't respawn the pickup)")]
        [SerializeField] protected float m_RespawnTimer = -1;
        [Tooltip("The item definition.")]
        [SerializeField] protected ItemDefinition m_ItemDefinition;
        [Tooltip("The item.")]
        [SerializeField] protected Item m_Item;
        [Tooltip("The amount")]
        [SerializeField] protected int m_Amount = 1;

        /// <summary>
        /// Set the item definition.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (m_Item == null && m_ItemDefinition != null) {
                SetItemDefinition(m_ItemDefinition);
            }
        }

        /// <summary>
        /// Pickup the item on interact.
        /// </summary>
        /// <param name="interactor"></param>
        public override void OnInteract(IInteractor interactor)
        {
            base.OnInteract(interactor);
            if (!(interactor is ICharacterInteractor characterInteractor)) { return; }

            var itemToPickup = m_Item;
            
            RemoveItem();
            
            characterInteractor.RemoveInteractable(m_Interactable);
            
            characterInteractor.Character.Inventory.Add((itemToPickup, m_Amount));

            if (m_RespawnTimer <= 0) {
                PoolManager.Destroy(gameObject,true);
            } else {
                gameObject.SetActive(false);
                SchedulerManager.Schedule(()=> gameObject.SetActive(true),m_RespawnTimer);
            }

        }
        
        /// <summary>
        /// Set the amount to pickup.
        /// </summary>
        /// <param name="amount"></param>
        public void SetAmount(int amount)
        {
            m_Amount = Mathf.Clamp(amount, 1, int.MaxValue);
        }

        /// <summary>
        /// Set the item definition of the item to pickup.
        /// </summary>
        /// <param name="itemDefinition">The item definition.</param>
        public void SetItemDefinition(ItemDefinition itemDefinition)
        {
            RemoveItem();
            
            m_ItemDefinition = itemDefinition;
            m_Item = InventoryManager.CreateItem(itemDefinition);
            InventoryManager.MoveItem(ref m_Item, transform);
        }

        /// <summary>
        /// Set the item to pickup.
        /// </summary>
        /// <param name="item">The item.</param>
        public void SetItem(Item item)
        {
            if (item.ItemDefinition.Unique == false) {
                SetItemDefinition(item.ItemDefinition);
                return;
            }

            RemoveItem();
            
            m_Item = item;
            m_ItemDefinition = m_Item.ItemDefinition;
            InventoryManager.MoveItem(ref m_Item, transform);
        }

        /// <summary>
        /// Remove the item.
        /// </summary>
        protected void RemoveItem()
        {
            if (m_Item != null) {
                InventoryManager.RemoveItem(m_Item);
                m_Item = null;
            }
        }
    }
}
