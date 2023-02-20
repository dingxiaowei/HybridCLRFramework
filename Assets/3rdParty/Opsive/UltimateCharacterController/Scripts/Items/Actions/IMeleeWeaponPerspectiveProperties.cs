/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Items.Actions
{
    /// <summary>
    /// Interface for an object which contains the perspective dependent variables for a MeleeWeapon.
    /// </summary>
    public interface IMeleeWeaponPerspectiveProperties
    {
        MeleeWeapon.MeleeHitbox[] Hitboxes { get; set; }
        Transform TrailLocation { get; set; } 
    }
}