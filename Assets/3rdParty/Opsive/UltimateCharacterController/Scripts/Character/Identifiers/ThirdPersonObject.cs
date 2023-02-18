/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Identifiers
{
    using Opsive.UltimateCharacterController.StateSystem;
#if THIRD_PERSON_CONTROLLER
    using Opsive.UltimateCharacterController.ThirdPersonController.Character;
#endif
    using UnityEngine;

    /// <summary>
    /// Identifying component which specifies the object should be hidden while in first person view.
    /// </summary>
    public class ThirdPersonObject : StateBehavior
    {
        [Tooltip("Should the object be forced visible even if it is in a first person view?")]
        [SerializeField] protected bool m_ForceVisible;
        [Tooltip("Should the object be visible when the character dies? This value will only be checked if the PerspectiveMonitor.ObjectDeathVisiblity is set to ThirdPersonObjectDetermined.")]
        [SerializeField] protected bool m_FirstPersonVisibleOnDeath = false;

        public bool ForceVisible { get { return m_ForceVisible; }
#if THIRD_PERSON_CONTROLLER
            set { if (m_ForceVisible != value) { m_ForceVisible = value; m_PerspectiveMonitor?.UpdateThirdPersonMaterials(false); } }
#endif
        }
        public bool FirstPersonVisibleOnDeath { get { return m_FirstPersonVisibleOnDeath; } }

#if THIRD_PERSON_CONTROLLER
        private PerspectiveMonitor m_PerspectiveMonitor;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Start()
        {
            m_PerspectiveMonitor = gameObject.GetComponentInParent<PerspectiveMonitor>();
        }
#endif
    }
}