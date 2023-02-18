/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.PerspectiveProperties
{
    using UnityEngine;

    /// <summary>
    /// Interface for the grenade item action.
    /// </summary>
    public interface IGrenadeItemPerspectiveProperties
    {
        int PinAttachmentLocationID { get; set; }
        Transform PinAttachmentLocation { get; set; }
    }
}