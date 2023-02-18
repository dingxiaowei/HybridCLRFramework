/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Networking.Traits
{
    using UnityEngine;

    /// <summary>
    /// Defines an object that can respawn over the network using the Respawner component.
    /// </summary>
    public interface INetworkRespawnerMonitor
    {
        /// <summary>
        /// Does the respawn by setting the position and rotation to the specified values.
        /// Enable the GameObject and let all of the listening objects know that the object has been respawned.
        /// </summary>
        /// <param name="position">The respawn position.</param>
        /// <param name="rotation">The respawn rotation.</param>
        /// <param name="transformChange">Was the position or rotation changed?</param>
        void Respawn(Vector3 position, Quaternion rotation, bool transformChange);
    }
}