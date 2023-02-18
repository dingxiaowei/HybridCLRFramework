/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Magic.ImpactActions
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Events;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Traits;
    using UnityEngine;

    /// <summary>
    /// Modifies the specified attribute on the impacted object.
    /// </summary>
    public class ModifyAttribute : ImpactAction
    {
        [Tooltip("The attribute that should be modified")]
        [SerializeField] protected AttributeModifier m_AttributeModifier = new AttributeModifier();

        public AttributeModifier AttributeModifier { get { return m_AttributeModifier; } set { m_AttributeModifier = value; } }

        /// <summary>
        /// Perform the impact action.
        /// </summary>
        /// <param name="castID">The ID of the cast.</param>
        /// <param name="source">The object that caused the cast.</param>
        /// <param name="target">The object that was hit by the cast.</param>
        /// <param name="hit">The raycast that caused the impact.</param>
        protected override void ImpactInternal(uint castID, GameObject source, GameObject target, RaycastHit hit)
        {
            var targetAttributeManager = target.GetCachedParentComponent<AttributeManager>();
            if (targetAttributeManager == null) {
                return;
            }

            // The impact action can collide with multiple objects. Use a pooled version of the AttributeModifier for each collision.
            var attributeModifier = GenericObjectPool.Get<AttributeModifier>();
            if (!attributeModifier.Initialize(m_AttributeModifier, targetAttributeManager)) {
                GenericObjectPool.Return(attributeModifier);
                return;
            }

            // The attribute exists. Enable the modifier. Return the modifier as soon as it is complete (which may be immediate).
            attributeModifier.EnableModifier(true);
            if (attributeModifier.AutoUpdating && attributeModifier.AutoUpdateDuration > 0) {
                EventHandler.RegisterEvent<AttributeModifier, bool>(attributeModifier, "OnAttributeModifierAutoUpdateEnable", ModifierAutoUpdateEnabled);
            } else {
                GenericObjectPool.Return(attributeModifier);
            }
        }

        /// <summary>
        /// The AttributeModifier auto updater has been enabled or disabled.
        /// </summary>
        /// <param name="attributeModifier">The modifier that has been enabled or disabled.</param>
        /// <param name="enable">True if the modifier has been enabled.</param>
        private void ModifierAutoUpdateEnabled(AttributeModifier attributeModifier, bool enable)
        {
            if (enable) {
                return;
            }

            EventHandler.UnregisterEvent<AttributeModifier, bool>(attributeModifier, "OnAttributeModifierAutoUpdateEnable", ModifierAutoUpdateEnabled);
            GenericObjectPool.Return(attributeModifier);
        }
    }
}