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
    /// Damages the impacted object.
    /// </summary>
    public class Damage : ImpactAction
    {
        [Tooltip("The damage amount.")]
        [SerializeField] protected float m_Amount = 10;
        [Tooltip("The magnitude of the force that is applied to the object.")]
        [SerializeField] protected float m_ForceMagnitude;
        [Tooltip("The number of frames to add the force to.")]
        [SerializeField] protected int m_ForceFrames = 1;
        [Tooltip("Should the subsequent Impact Actions be interrupted if the Health component doesn't exist?")]
        [SerializeField] protected bool m_InterruptImpactOnNullHealth = true;

        public float Amount { get { return m_Amount; } set { m_Amount = value; } }
        public float ForceMagnitude { get { return m_ForceMagnitude; } set { m_ForceMagnitude = value; } }
        public int ForceFrames { get { return m_ForceFrames; } set { m_ForceFrames = value; } }
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
            if (health == null || !health.IsAlive()) {
                if (m_InterruptImpactOnNullHealth) {
                    m_MagicItem.InterruptImpact();
                }
                return;
            }
            health.Damage(m_Amount, source.transform.position, (source.transform.position - target.transform.position), m_ForceMagnitude, m_ForceFrames, 0, source);
        }
    }
}