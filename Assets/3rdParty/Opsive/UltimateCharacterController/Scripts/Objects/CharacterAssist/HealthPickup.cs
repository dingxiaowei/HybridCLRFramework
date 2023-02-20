/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Objects.CharacterAssist
{
    /// <summary>
    /// Heals the object that has the Health component.
    /// </summary>
    public class HealthPickup : ObjectPickup
    {
        [Tooltip("The amount of health to replenish.")]
        [SerializeField] private float m_HealthAmount = 40;
        [Tooltip("Should the object be picked up even if the object has full health?")]
        [SerializeField] private bool m_AlwaysPickup;

        public float HealthAmount { get { return m_HealthAmount; } set { m_HealthAmount = value; } }
        public bool AlwaysPickup { get { return m_AlwaysPickup; } set { m_AlwaysPickup = value; } }

        /// <summary>
        /// A GameObject has entered the trigger.
        /// </summary>
        /// <param name="other">The GameObject that entered the trigger.</param>
        public override void TriggerEnter(GameObject other)
        {
            var health = other.GetCachedParentComponent<Health>();
            if (health != null && health.IsAlive()) {
                if (health.Heal(m_HealthAmount) || m_AlwaysPickup) {
                    ObjectPickedUp(health.gameObject);
                }
            }
        }
    }
}