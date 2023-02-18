/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Magic
{
    using UnityEngine;

    /// <summary>
    /// Specifies a magic object (particle, generic GameObject) that can be spawned over the network.
    /// </summary>
    public interface IMagicObjectAction
    {
        /// <summary>
        /// The GameObject that was spawned.
        /// </summary>
        GameObject SpawnedGameObject { set; }
        /// <summary>
        /// The ID of the cast.
        /// </summary>
        uint CastID { set; }
    }
}