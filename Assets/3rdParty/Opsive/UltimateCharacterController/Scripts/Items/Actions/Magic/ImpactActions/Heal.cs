/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Magic.ImpactActions
{
    using Opsive.UltimateCharacterController.Traits;
    using UnityEngine;

    /// <summary>
    /// Heals the impacted object.
    /// </summary>
    public class Heal : ImpactAction
    {
        [Tooltip("The amount that should be added to the Health component.")]
        [SerializeField] protected float m_Amount = 10;
        [Tooltip("Should the subsequent Impact Actions be interrupted if the Health component doesn't exist?")]
        [SerializeField] protected bool m_InterruptImpactOnNullHealth = true;

        public float Amount { get { return m_Amount; } set { m_Amount = value; } }
        public bool InterruptImpactOnNullHealth { get { return m_InterruptImpactOnNullHealth; } set { m_InterruptImpactOnNullHealth = value; } }

        /// <summary>
        /// Perform the impact action.
        /// </summary>
        /// <param name="castID">The ID of the cast.</param>
        /// <param name="source">The object that caused the cast.</param>
        /// <param name="target">The object that was hit by the cast.</param>
        /// <param name="hit">The raycast that caused the impact.</param>
        protected override void ImpactInternal(uint castID, GameObject source, GameObject target, RaycastHit hit)
        {
            var health = target.GetComponent<Health>();
            if (health == null || !health.Heal(m_Amount)) {
                if (m_InterruptImpactOnNullHealth) {
                    m_MagicItem.InterruptImpact();
                }
            }
        }
    }
}