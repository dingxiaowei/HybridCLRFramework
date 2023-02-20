/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Items.Actions
{
    /// <summary>
    /// Interface for an item action that can be thrown.
    /// </summary>
    public interface IThrowableItemPerspectiveProperties
    {
        Transform ThrowLocation { get; set; }
        Transform TrajectoryLocation { get; set; }
    }
}