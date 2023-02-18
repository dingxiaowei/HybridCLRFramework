/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.PerspectiveProperties
{
    using UnityEngine;

    /// <summary>
    /// Interface for an item action that can be thrown.
    /// </summary>
    public interface IThrowableItemPerspectiveProperties
    {
        Transform ThrowLocation { get; set; }
        Transform TrajectoryLocation { get; set; }
    }
}