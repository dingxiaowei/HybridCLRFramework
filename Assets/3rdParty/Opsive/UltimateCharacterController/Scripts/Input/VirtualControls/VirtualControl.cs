/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Input.VirtualControls
{
    /// <summary>
    /// An abstract class which represents a virtual control on the screen.
    /// </summary>
    public abstract class VirtualControl : MonoBehaviour
    {
        protected VirtualControlsManager m_VirtualControlsManager;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected virtual void Awake()
        {
            m_VirtualControlsManager = GetComponentInParent<VirtualControlsManager>();
            if (m_VirtualControlsManager == null) {
                Debug.LogError("Error: Unable to find the VirtualControlsManager. This component must be a parent to the virtual input monitors.");
            }
        }

        /// <summary>
        /// Returns if the button is true with the specified ButtonAction.
        /// </summary>
        /// <param name="action">The type of action to check.</param>
        /// <returns>The status of the action.</returns>
        public virtual bool GetButton(InputBase.ButtonAction action) { return false; }

        /// <summary>
        /// Returns the value of the axis.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The value of the axis.</returns>
        public virtual float GetAxis(string name) { return 0; }
    }
}