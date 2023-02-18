/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.StateSystem
{
    /// <summary>
    /// Interface for any object which uses the StateSystem. Allows the StateManager to interact with a common object.
    /// </summary>
    public interface IStateOwner
    {
        /// <summary>
        /// Callback when the StateManager will change the active state on the current object.
        /// </summary>
        void StateWillChange();

        /// <summary>
        /// Callback when the StateManager has changed the active state on the current object.
        /// </summary>
        void StateChange();
    }
}