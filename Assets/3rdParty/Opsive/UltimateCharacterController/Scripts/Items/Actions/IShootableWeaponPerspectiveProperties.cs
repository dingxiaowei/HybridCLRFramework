/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Items.Actions
{
    /// <summary>
    /// Interface for an object which contains the perspective dependent variables for a ShootableWeapon.
    /// </summary>
    public interface IShootableWeaponPerspectiveProperties
    {
        Transform FirePointLocation { get; set; }
        Transform MuzzleFlashLocation { get; set; }
        Transform ShellLocation { get; set; }
        Transform SmokeLocation { get; set; }
        Transform TracerLocation { get; set; }
        Transform ReloadableClip { get; set; }
        Transform ReloadableClipAttachment { get; set; }
        Transform ReloadProjectileAttachment { get; set; }
        GameObject ScopeCamera { get; set; }

        /// <summary>
        /// Can the weapon be fired?
        /// </summary>
        /// <param name="fireInLookSourceDirection">Should the weapon fire in the LookSource direction?</param>
        /// <param name="abilityActive">Is the Use ability active?</param>
        /// <returns>True if the item can be fired.</returns>
        bool CanFire(bool abilityActive, bool fireInLookSourceDirection);
    }
}