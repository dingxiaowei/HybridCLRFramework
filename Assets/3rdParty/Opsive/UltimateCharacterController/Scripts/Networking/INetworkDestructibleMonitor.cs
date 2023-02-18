/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Networking.Objects
{
    using UnityEngine;

    /// <summary>
    /// Defines an object that can take destruct over the network using the Destructible component.
    /// </summary>
    public interface IDestructibleMonitor
    {
        /// <summary>
        /// Destroys the object.
        /// </summary>
        /// <param name="hitPosition">The position of the destruction.</param>
        /// <param name="hitNormal">The normal direction of the destruction.</param>
        void Destruct(Vector3 hitPosition, Vector3 hitNormal);
    }
}