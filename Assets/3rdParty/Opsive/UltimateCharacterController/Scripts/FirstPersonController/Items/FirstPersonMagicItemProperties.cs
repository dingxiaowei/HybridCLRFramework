/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.FirstPersonController.Items
{
    using Opsive.UltimateCharacterController.Items.Actions.PerspectiveProperties;
    using UnityEngine;

    /// <summary>
    /// Describes any first person perspective dependent properties for the magic item.
    /// </summary>
    public class FirstPersonMagicItemProperties : FirstPersonItemProperties, IMagicItemPerspectiveProperties
    {
        [Tooltip("The location that the magic originates from.")]
        [SerializeField] protected Transform m_OriginLocation;

        public Transform OriginLocation { get { return m_OriginLocation; } set { m_OriginLocation = value; } }
    }
}