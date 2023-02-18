/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Objects
{
    using Opsive.Shared.Events;
    using Opsive.UltimateCharacterController.Traits;
    using UnityEngine;

    /// <summary>
    /// A healing crate will update its material if it is healed.
    /// </summary>
    public class HealingCrate : MonoBehaviour
    {
        [Tooltip("The material representing a damaged crate.")]
        [SerializeField] protected Material m_DamagedMaterial;
        [Tooltip("The material representing a healed crate.")]
        [SerializeField] protected Material m_HealedMaterial;
        [Tooltip("The crate is healed when the Health attribute is greater than the specified value.")]
        [SerializeField] protected float m_HealedAttributeValue = 40;

        private Renderer m_Renderer;
        private Attribute m_HealthAttribute;
        private float m_StartingAttributeValue;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Renderer = gameObject.GetComponent<Renderer>();
            var attributeManager = gameObject.GetComponent<AttributeManager>();
            m_HealthAttribute = attributeManager.GetAttribute("Health");
            m_StartingAttributeValue = m_HealthAttribute.Value;

            EventHandler.RegisterEvent<Attribute>(gameObject, "OnAttributeUpdateValue", OnUpdateValue);
        }

        /// <summary>
        /// The attribute value has beeen updated.
        /// </summary>
        /// <param name="attribute">The attribute that has been updated.</param>
        private void OnUpdateValue(Attribute attribute)
        {
            if (attribute != m_HealthAttribute) {
                return;
            }

            if (attribute.Value >= m_HealedAttributeValue) {
                m_Renderer.sharedMaterial = m_HealedMaterial;
            } else {
                m_Renderer.sharedMaterial = m_DamagedMaterial;
            }
        }

        /// <summary>
        /// Revert the health attribute value when the object is disabled.
        /// </summary>
        private void OnDisable()
        {
            m_HealthAttribute.Value = m_StartingAttributeValue;
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<Attribute>(gameObject, "OnAttributeUpdateValue", OnUpdateValue);
        }
    }
}