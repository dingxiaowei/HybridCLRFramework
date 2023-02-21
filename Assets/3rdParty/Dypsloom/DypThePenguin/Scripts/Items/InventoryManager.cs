/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Items
{
    using Dypsloom.Shared.Utility;
    using UnityEngine;

    /// <summary>
    /// The inventory manager initializes the items in the scene.
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] protected GameObject m_ItemPickupPrefab;


        static InventoryManager _instance;

        public static InventoryManager Instance => _instance != null ?
            _instance : new GameObject("Inventory Manager").AddComponent<InventoryManager>();

        /// <summary>
        /// Cache components.
        /// </summary>
        private void Awake()
        {
            if (_instance == null) {
                _instance = this; 
                return;
            }
            Destroy(gameObject);
        }

        /// <summary>
        /// Spawn a pickup at the position specified.
        /// </summary>
        /// <param name="itemDefinition">The item definition.</param>
        /// <param name="position">The position.</param>
        /// <param name="amount">The amount.</param>
        /// <returns>The item pickup.</returns>
        public static ItemPickup SpawnPickupAt(ItemDefinition itemDefinition, Vector3 position, int amount)
        {
            var obj = PoolManager.Instantiate(Instance.m_ItemPickupPrefab);
            obj.transform.position = position;
            var itemPickup = obj.GetComponent<ItemPickup>();
            itemPickup.SetItemDefinition(itemDefinition);
            itemPickup.SetAmount(amount);
            return itemPickup;
        } 
        
        /// <summary>
        /// Spawn a pickup.
        /// </summary>
        /// <param name="item">The item to spawn.</param>
        /// <param name="position">The position.</param>
        /// <param name="amount">The amount.</param>
        /// <returns>The item pickup.</returns>
        public static ItemPickup SpawnPickupAt(Item item, Vector3 position, int amount)
        {
            var obj = PoolManager.Instantiate(Instance.m_ItemPickupPrefab);
            obj.transform.position = position;
            var itemPickup = obj.GetComponent<ItemPickup>();
            itemPickup.SetItem(item);
            itemPickup.SetAmount(amount);
            return itemPickup;
        }
        
        /// <summary>
        /// Create an item.
        /// </summary>
        /// <param name="itemDefinition">The item definition.</param>
        /// <returns>The item.</returns>
        public static Item CreateItem(ItemDefinition itemDefinition)
        {
            var itemGO = PoolManager.Instantiate(itemDefinition.ItemPrefab);
            return itemGO.GetComponent<Item>();
        }

        /// <summary>
        /// Remove an item.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public static void RemoveItem(Item item)
        {
            if (item.ItemDefinition.Unique == false) {
                //if (PoolManager.HasObject(item.gameObject)) {
                    PoolManager.Destroy(item.gameObject);
                //}
                return;
            }
            
            item.gameObject.SetActive(false);
            item.transform.SetParent(Instance.transform);
        }
        
        /// <summary>
        /// Move an item to a new transform.
        /// </summary>
        /// <param name="item">The item to move.</param>
        /// <param name="itemParent">The item parent transform.</param>
        public static void MoveItem(ref Item item, Transform itemParent)
        {
            if (item.ItemDefinition.Unique == false) {

                if (PoolManager.HasObject(item.gameObject)) {
                    PoolManager.Destroy(item.gameObject);
                }
                
                item = CreateItem(item.ItemDefinition);
                
            }

            item.gameObject.SetActive(true);
            item.transform.SetParent(itemParent);
            item.transform.localPosition=Vector3.zero;
            item.transform.localRotation=Quaternion.identity;
        }
    }
}