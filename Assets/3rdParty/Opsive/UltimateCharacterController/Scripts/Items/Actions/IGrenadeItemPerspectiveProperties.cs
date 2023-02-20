/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Items.Actions
{
    /// <summary>
    /// Interface for the grenade item action.
    /// </summary>
    public interface IGrenadeItemPerspectiveProperties
    {
        int PinAttachmentLocationID { get; set; }
        Transform PinAttachmentLocation { get; set; }
    }
}