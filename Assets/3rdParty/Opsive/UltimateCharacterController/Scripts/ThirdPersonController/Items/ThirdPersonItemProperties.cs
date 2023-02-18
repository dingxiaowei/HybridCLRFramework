/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.ThirdPersonController.Items
{
    using Opsive.UltimateCharacterController.Items;

    /// <summary>
    /// Describes any third person perspective dependent properties for the item.
    /// </summary>
    public abstract class ThirdPersonItemProperties : ItemPerspectiveProperties
    {
        public override bool FirstPersonItem { get { return false; } }
    }
}