/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Traits
{
    /// <summary>
    /// A hitbox maps a collider to a multiplier. It is also used for collision detection by the MeleeWeapon.
    /// </summary>
    [System.Serializable]
    public class Hitbox
    {
        [Tooltip("The collider used for collisions in the hitbox.")]
        [SerializeField] protected Collider m_Collider;
        [Tooltip("The amount to multiply the damage amount by when the hitbox collides with an object.")]
        [SerializeField] protected float m_DamageMultiplier = 1;

        public Collider Collider { get { return m_Collider; } }
        public float DamageMultiplier { get { return m_DamageMultiplier; } }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Hitbox() { }

        /// <summary>
        /// Single parameter constructor.
        /// </summary>
        /// <param name="collider">The collider that represents the hitbox.</param>
        public Hitbox(Collider collider)
        {
            m_Collider = collider;
        }
    }
}
