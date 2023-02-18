/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.PerspectiveProperties
{
    using UnityEngine;

    /// <summary>
    /// Interface for a magic item.
    /// </summary>
    public interface IMagicItemPerspectiveProperties
    {
        Transform OriginLocation { get; set; }
    }
}