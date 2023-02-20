/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Character
{
    /// <summary>
    /// Interface which specifies an object which can be used for determine the character's look direction.
    /// </summary>
    public interface ILookSource
    {
        /// <summary>
        /// Returns the GameObject of the look source.
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// Returns the Transform of the look source.
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// Returns the distance that the character should look ahead.
        /// </summary>
        float LookDirectionDistance { get; }
        
        /// <summary>
        /// Returns the pitch angle of the look source.
        /// </summary>
        float Pitch { get; }

        /// <summary>
        /// Returns the position of the look source.
        /// </summary>
        Vector3 LookPosition();

        /// <summary>
        /// Returns the direction that the character is looking.
        /// </summary>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <returns>The direction that the character is looking.</returns>
        Vector3 LookDirection(bool characterLookDirection);

        /// <summary>
        /// Returns the direction that the character is looking.
        /// </summary>
        /// <param name="lookPosition">The position that the character is looking from.</param>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <param name="layerMask">The LayerMask value of the objects that the look direction can hit.</param>
        /// <param name="useRecoil">Should recoil be included in the look direction?</param>
        /// <returns>The direction that the character is looking.</returns>
        Vector3 LookDirection(Vector3 lookPosition, bool characterLookDirection, int layerMask, bool useRecoil);
    }
}