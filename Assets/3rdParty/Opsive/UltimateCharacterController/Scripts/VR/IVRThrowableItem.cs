/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.VR
{
    using UnityEngine;

    /// <summary>
    /// Allows the ThrowableItem to communicate with a VRThrowableItem.
    /// </summary>
    public interface IVRThrowableItem
    {
        /// <summary>
        /// The character has equipped the item.
        /// </summary>
        void Equip();

        /// <summary>
        /// Returns the velocity that the item should be thrown at.
        /// </summary>
        /// <returns>The velocity that the item should be thrown at.</returns>
        Vector3 GetVelocity();

        /// <summary>
        /// The character has unequipped the item.
        /// </summary>
        void Unequip();
    }
}