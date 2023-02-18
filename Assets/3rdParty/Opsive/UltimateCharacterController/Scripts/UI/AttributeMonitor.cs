/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Traits;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// The AttributeMonitor will update the UI for the character's attributes.
    /// </summary>
    public class AttributeMonitor : CharacterMonitor
    {
        [Tooltip("The AttributeManager that contains the attribute. If null the character's Attribute Manager will be used.")]
        [SerializeField] protected AttributeManager m_AttributeManager;
        [Tooltip("The name of the attribute that the UI should monitor.")]
        [SerializeField] protected string m_AttributeName = "Health";
        [Tooltip("A reference used to the slider used to show the attribute value.")]
        [SerializeField] protected Slider m_Slider;

        private Attribute m_Attribute;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            if (m_Slider == null) {
                m_Slider = GetComponent<Slider>();
            }

            if (m_AttributeManager != null) {
                EventHandler.RegisterEvent<Attribute>(m_AttributeManager.gameObject, "OnAttributeUpdateValue", OnUpdateValue);
            }

            // The monitor can't display if there is no slider.
            if (m_Slider != null) {
                base.Awake();
            } else {
                enabled = false;
            }
        }

        /// <summary>
        /// Attaches the monitor to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the monitor to.</param>
        protected override void OnAttachCharacter(GameObject character)
        {
            if (m_Character != null) {
                if (m_AttributeManager != null && m_AttributeManager.gameObject == character) {
                    EventHandler.UnregisterEvent<Attribute>(m_AttributeManager.gameObject, "OnAttributeUpdateValue", OnUpdateValue);
                    m_AttributeManager = null;
                }
            }

            base.OnAttachCharacter(character);

            if (m_Character == null) {
                return;
            }

            if (m_AttributeManager == null) {
                m_AttributeManager = m_Character.GetCachedComponent<AttributeManager>();
                EventHandler.RegisterEvent<Attribute>(m_AttributeManager.gameObject, "OnAttributeUpdateValue", OnUpdateValue);
            }
            if (m_AttributeManager == null) {
                enabled = false;
                gameObject.SetActive(false);
                return;
            }

            m_Attribute = m_AttributeManager.GetAttribute(m_AttributeName);
            if (m_Attribute == null) {
                enabled = false;
                gameObject.SetActive(false);
                return;
            }
            enabled = true;
            m_Slider.value = (m_Attribute.Value - m_Attribute.MinValue) / (m_Attribute.MaxValue - m_Attribute.MinValue);
        }

        /// <summary>
        /// The attribute's value has been updated.
        /// </summary>
        /// <param name="attribute">The attribute that was updated.</param>
        private void OnUpdateValue(Attribute attribute)
        {
            if (attribute != m_Attribute) {
                return;
            }

            m_Slider.value = (m_Attribute.Value - m_Attribute.MinValue) / (m_Attribute.MaxValue - m_Attribute.MinValue);
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (m_AttributeManager != null) {
                EventHandler.UnregisterEvent<Attribute>(m_AttributeManager.gameObject, "OnAttributeUpdateValue", OnUpdateValue);
            }
        }

        /// <summary>
        /// Can the UI be shown?
        /// </summary>
        /// <returns>True if the UI can be shown.</returns>
        protected override bool CanShowUI()
        {
            return base.CanShowUI() && m_Attribute != null;
        }
    }
}
