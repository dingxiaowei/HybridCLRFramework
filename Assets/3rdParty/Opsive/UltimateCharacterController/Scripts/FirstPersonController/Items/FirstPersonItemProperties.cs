/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.FirstPersonController.Items
{
    using Opsive.UltimateCharacterController.Items;

    /// <summary>
    /// Describes any first person perspective dependent properties for the item.
    /// </summary>
    public abstract class FirstPersonItemProperties : ItemPerspectiveProperties
    {
        public override bool FirstPersonItem { get { return true; } }
    }
}