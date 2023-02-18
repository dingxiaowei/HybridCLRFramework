#if ULTIMATE_CHARACTER_CONTROLLER_MELEE
/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.VR
{
    /// <summary>
    /// Allows the MeleeWeapon to communicate with a VRMeleeWeapon.
    /// </summary>
    public interface IVRMeleeWeapon
    {
        /// <summary>
        /// Starts the item use.
        /// </summary>
        void StartItemUse();

        /// <summary>
        /// Returns true if the melee weapon can be used.
        /// </summary>
        /// <returns>True if the melee weapon be used.</returns>
        bool CanUseItem();

        /// <summary>
        /// Stops the item use.
        /// </summary>
        void StopItemUse();
    }
}
#endif