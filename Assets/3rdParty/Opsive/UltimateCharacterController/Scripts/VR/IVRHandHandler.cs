/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.VR
{
    using UnityEngine;

    /// <summary>
    /// The handler for the VR hands.
    /// </summary>
    public interface IVRHandHandler
    {
        /// <summary>
        /// Returns the number of slots that the hands can occupy.
        /// </summary>
        /// <returns>The number of slots that the hands can occupy.</returns>
        int GetSlotCount();

        /// <summary>
        /// Returns the hand GameObject at the specified slot ID.
        /// </summary>
        /// <param name="slotID">The ID to retrieve the hand of.</param>
        /// <returns>The hand GameObject at the specified slot ID.</returns>
        GameObject GetHand(int slotID);

        /// <summary>
        /// Returns the velocity of the hand at the specified slot.
        /// </summary>
        /// <param name="slotID">The slot to get the velocity of.</param>
        /// <returns>The velocity of the hand at the specifeid slot.</returns>
        Vector3 GetVelocity(int slotID);
    }
}