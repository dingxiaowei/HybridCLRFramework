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
    /// Describes any first person perspective dependent properties for the flashlight.
    /// </summary>
    public class FirstPersonFlashlightProperties : FirstPersonItemProperties, IFlashlightPerspectiveProperties
    {
        [Tooltip("A reference to the light used by the flashlight.")]
        [SerializeField] protected GameObject m_Light;

        [Shared.Utility.NonSerialized] public GameObject Light { get { return m_Light; } set { m_Light = value; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_Light.SetActive(false);
        }
    }
}