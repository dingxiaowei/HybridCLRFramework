/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects.CharacterAssist
{
    using UnityEngine;

    /// <summary>
    /// Interface for any object that can be driven.
    /// </summary>
    public interface IDriveSource
    {
        /// <summary>
        /// The GameObject of the vehicle.
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// The Transform of the vehicle.
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// The location that the character drives the vehicle from.
        /// </summary>
        Transform DriverLocation { get; }

        /// <summary>
        /// The unique identifier of the object. This value is used within the AbilityIntData parameter of the character's animator.
        /// </summary>
        int AnimatorID { get; }

        /// <summary>
        /// The character has started to enter the vehicle.
        /// </summary>
        /// <param name="character">The character that is entering the vehicle.</param>
        void EnterVehicle(GameObject character);

        /// <summary>
        /// The character has entered the vehicle.
        /// </summary>
        /// <param name="character">The character that entered the vehicle.</param>
        void EnteredVehicle(GameObject character);

        /// <summary>
        /// The character has started to exit the vehicle.
        /// </summary>
        /// <param name="character">The character that is exiting the vehicle.</param>
        void ExitVehicle(GameObject character);

        /// <summary>
        /// The character has exited the vehicle.
        /// </summary>
        /// <param name="character">The character that exited the vehicle.</param>
        void ExitedVehicle(GameObject character);
    }
}