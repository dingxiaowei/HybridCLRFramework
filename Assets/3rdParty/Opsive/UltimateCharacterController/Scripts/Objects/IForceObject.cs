/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Objects
{
    /// <summary>
    /// Interface for any object that is kinematic and can have forces applied to it.
    /// </summary>
    public interface IForceObject
    {
        /// <summary>
        /// Adds a force to the object.
        /// </summary>
        /// <param name="force">The force to add to the object.</param>
        void AddForce(Vector3 force);

        /// <summary>
        /// Adds a force to the object.
        /// </summary>
        /// <param name="force">The force to add to the object.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        void AddForce(Vector3 force, int frames);
    }
}