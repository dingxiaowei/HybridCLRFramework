/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Networking
{
    /// <summary>
    /// Contains information about the object on the network.
    /// </summary>
    public interface INetworkInfo
    {
        /// <summary>
        /// Is the networking implementation server authoritative?
        /// </summary>
        /// <returns>True if the network transform is server authoritative.</returns>
        bool IsServerAuthoritative();

        /// <summary>
        /// Is the game instance on the server?
        /// </summary>
        /// <returns>True if the game instance is on the server.</returns>
        bool IsServer();

        /// <summary>
        /// Is the character the local player?
        /// </summary>
        /// <returns>True if the character is the local player.</returns>
        bool IsLocalPlayer();
    }
}