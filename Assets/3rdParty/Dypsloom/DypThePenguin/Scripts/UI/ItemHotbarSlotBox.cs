/// ---------------------------------------------
/// Dyp Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.UI
{
    using Dypsloom.DypThePenguin.Scripts.Items;
    using TMPro;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>
    /// A hot bat slot box.
    /// </summary>
    public class ItemHotbarSlotBox : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] protected TextMeshProUGUI m_IndexText;
        [SerializeField] protected Image m_Icon;
        [SerializeField] protected TextMeshProUGUI m_ItemAmountText;
        [SerializeField] protected GameObject m_ItemDescriptionObject;
        [SerializeField] protected TextMeshProUGUI m_ItemDescription;

        protected ItemAmount m_ItemAmount;
        protected int m_Index;

        public ItemAmount ItemAmount => m_ItemAmount;

        /// <summary>
        /// Set the index of the slot.
        /// </summary>
        /// <param name="index"></param>
        public virtual void SetIndex(int index)
        {
            m_Index = index;
            m_IndexText.text = (m_Index+1).ToString();
        }
    
        /// <summary>
        /// Draw the item amount.
        /// </summary>
        /// <param name="itemAmount"></param>
        public virtual void Draw(ItemAmount itemAmount)
        {
            if (itemAmount.Item == null || itemAmount.Item.ItemDefinition == null) {
                DrawEmpty(); 
                return;
            }
        
            m_ItemAmount = itemAmount;
            m_ItemAmountText.text = "x"+itemAmount.Amount.ToString();
            m_Icon.gameObject.SetActive(true);
            m_Icon.sprite = itemAmount.Item.ItemDefinition.Icon;
            if(m_ItemDescription != null){m_ItemDescription.text = itemAmount.Item.ItemDefinition.Description;}
        }

        /// <summary>
        /// Draw the empty.
        /// </summary>
        public virtual void DrawEmpty()
        {
            m_ItemAmount = (null, 0);
            m_ItemAmountText.text = "";
            m_Icon.gameObject.SetActive(false);
            m_ItemDescriptionObject?.SetActive(false);
            if(m_ItemDescription != null){m_ItemDescription.text = "";}
        }

        /// <summary>
        /// Check for the mouse input.
        /// </summary>
        /// <param name="eventData">The enter event.</param>
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (m_ItemAmount.Item == null) {
                m_ItemDescriptionObject?.SetActive(false);
                return;
            }
            m_ItemDescriptionObject?.SetActive(true);
        }

        /// <summary>
        /// Check for the mouse input.
        /// </summary>
        /// <param name="eventData">The exit event.</param>
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            m_ItemDescriptionObject?.SetActive(false);
        }
    }
}