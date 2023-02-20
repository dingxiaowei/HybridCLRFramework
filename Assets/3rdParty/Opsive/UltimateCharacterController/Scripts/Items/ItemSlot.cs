/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Items
{
    /// <summary>
    /// Identifier class used to determine where the item GameObjects are located.
    /// </summary>
    public class ItemSlot : MonoBehaviour
    {
        [Tooltip("An identifier for the ItemSlot component.")]
        [SerializeField] protected int m_ID;

        public int ID { get { return m_ID; } set { m_ID = value; } }
    }
}