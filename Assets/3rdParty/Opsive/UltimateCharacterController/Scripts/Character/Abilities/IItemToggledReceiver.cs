/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities.Items
{
    /// <summary>
    /// Interface for any ability that should be notified when the item is toggled while the ability is active.
    /// </summary>
    public interface IItemToggledReceiver
    {
        /// <summary>
        /// The ItemEquipVerifier ability has toggled an item slot.
        /// </summary>
        void ItemToggled();
    }
}