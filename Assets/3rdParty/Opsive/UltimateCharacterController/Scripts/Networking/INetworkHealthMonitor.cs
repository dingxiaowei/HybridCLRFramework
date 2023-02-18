/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Networking.Traits
{
    using UnityEngine;

    /// <summary>
    /// Defines an object that can take damage over the network using the Health component.
    /// </summary>
    public interface INetworkHealthMonitor
    {
        /// <summary>
        /// The object has taken been damaged.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="direction">The direction that the object took damage from.</param>
        /// <param name="forceMagnitude">The magnitude of the force that is applied to the object.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        /// <param name="radius">The radius of the explosive damage. If 0 then a non-explosive force will be used.</param>
        /// <param name="attacker">The GameObject that did the damage.</param>
        /// <param name="hitCollider">The Collider that was hit.</param>
        void OnDamage(float amount, Vector3 position, Vector3 direction, float forceMagnitude, int frames, float radius, GameObject attacker, Collider hitCollider);

        /// <summary>
        /// The object is no longer alive.
        /// </summary>
        /// <param name="position">The position of the damage.</param>
        /// <param name="force">The amount of force applied to the object while taking the damage.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        void Die(Vector3 position, Vector3 force, GameObject attacker);

        /// <summary>
        /// Adds amount to health and then to the shield if there is still an amount remaining. Will not go over the maximum health or shield value.
        /// </summary>
        /// <param name="amount">The amount of health or shield to add.</param>
        void Heal(float amount);
    }
}