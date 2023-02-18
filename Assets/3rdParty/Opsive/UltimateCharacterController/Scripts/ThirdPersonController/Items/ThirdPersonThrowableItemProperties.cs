/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.ThirdPersonController.Items
{
    using Opsive.UltimateCharacterController.Items.Actions.PerspectiveProperties;
    using UnityEngine;

    /// <summary>
    /// Describes any third person perspective dependent properties for the ThrowableItem.
    /// </summary>
    public class ThirdPersonThrowableItemProperties : ThirdPersonWeaponProperties, IThrowableItemPerspectiveProperties
    {
        [Tooltip("The location to throw the object from.")]
        [SerializeField] protected Transform m_ThrowLocation;
        [Tooltip("The location of the trajectory curve.")]
        [SerializeField] protected Transform m_TrajectoryLocation;

        [Opsive.Shared.Utility.NonSerialized] public Transform ThrowLocation { get { return m_ThrowLocation; } set { m_ThrowLocation = value; } }
        [Opsive.Shared.Utility.NonSerialized] public Transform TrajectoryLocation { get { return m_TrajectoryLocation; } set { m_TrajectoryLocation = value; } }
    }
}