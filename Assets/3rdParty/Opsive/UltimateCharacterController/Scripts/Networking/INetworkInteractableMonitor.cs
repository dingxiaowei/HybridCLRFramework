/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Networking.Traits
{
    /// <summary>
    /// Allows the object to be interacted with on the network.
    /// </summary>
    public interface INetworkInteractableMonitor
    {
        /// <summary>
        /// Performs the interaction.
        /// </summary>
        /// <param name="character">The character that wants to interactact with the target.</param>
        void Interact(GameObject character);
    }
}