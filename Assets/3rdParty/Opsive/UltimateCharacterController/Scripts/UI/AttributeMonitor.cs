/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.UI
{
    /// <summary>
    /// The AttributeMonitor will update the UI for the character's attributes.
    /// </summary>
    public class AttributeMonitor : CharacterMonitor
    {
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
                EventHandler.UnregisterEvent<Attribute>(m_Character, "OnAttributeUpdateValue", OnUpdateValue);
            }

            base.OnAttachCharacter(character);

            if (m_Character == null) {
                return;
            }

            var attributeManager = m_Character.GetCachedComponent<AttributeManager>();
            if (attributeManager == null) {
                enabled = false;
            } else {
                m_Attribute = attributeManager.GetAttribute(m_AttributeName);
                if (m_Attribute == null) {
                    enabled = false;
                    gameObject.SetActive(false);
                    return;
                }
                enabled = true;
                m_Slider.value = (m_Attribute.Value - m_Attribute.MinValue) / (m_Attribute.MaxValue - m_Attribute.MinValue);

                EventHandler.RegisterEvent<Attribute>(m_Character, "OnAttributeUpdateValue", OnUpdateValue);
            }
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

            if (m_Character != null) {
                EventHandler.UnregisterEvent<Attribute>(m_Character, "OnAttributeUpdateValue", OnUpdateValue);
            }
        }

        /// <summary>
        /// Can the UI be shown?
        /// </summary>
        /// <returns>True if the UI can be shown.</returns>
        protected override bool CanShowUI()
        {
            return m_Attribute != null;
        }
    }
}
