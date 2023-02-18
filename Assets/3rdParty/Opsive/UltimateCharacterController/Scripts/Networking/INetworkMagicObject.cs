/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Networking.Objects
{
    using Opsive.UltimateCharacterController.Items.Actions;
    using UnityEngine;

    /// <summary>
    /// Defines a magic object that can is spawned over the network.
    /// </summary>
    public interface INetworkMagicObject
    {
        /// <summary>
        /// Sets the spawn data.
        /// </summary>
        /// <param name="character">The character that is instantiating the object.</param>
        /// <param name="magicItem">The MagicItem that the object belongs to.</param>
        /// <param name="actionIndex">The index of the action that is instantiating the object.</param>
        /// <param name="castID">The ID of the cast that is instantiating the object.</param>
        void Instantiate(GameObject character, MagicItem magicItem, int actionIndex, uint castID);
    }
}