/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Game
{
    using UnityEngine;

    /// <summary>
    /// Interface for any kinematic object that can be moved with no parameters.
    /// </summary>
    public interface IKinematicObject
    {
        /// <summary>
        /// Specifies the location that the object should be updated.
        /// </summary>
        KinematicObjectManager.UpdateLocation UpdateLocation { get; }

        /// <summary>
        /// A reference to the object's transform component.
        /// </summary>
        Transform transform { get; }

        /// <summary>
        /// Sets the index of the kinematic object.
        /// </summary>
        int KinematicObjectIndex { set; }

        /// <summary>
        /// Moves the object.
        /// </summary>
        void Move();
    }
}