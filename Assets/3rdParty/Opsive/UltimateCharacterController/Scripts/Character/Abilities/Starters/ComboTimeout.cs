/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Input;

namespace Opsive.UltimateCharacterController.Character.Abilities.Starters
{
    /// <summary>
    /// The ComboTimeout is an AbilityStarter that will start the ability when a combo has been performed. If the next combo input isn't performed within the timeout value
    /// then the combo will need to be reset from the beginning.
    /// </summary>
    public class ComboTimeout : AbilityStarter
    {
        /// <summary>
        /// Structure which holds the data associated with a single combo element.
        /// </summary>
        public struct ComboInputElement
        {
            [Tooltip("The name of the input that should be checked. This input name should be mapped to a button.")]
            [SerializeField] string m_InputName;
            [Tooltip("Should the axis be checked? If false then a button down will be checked.")]
            [SerializeField] bool m_AxisInput;
            [Tooltip("The amount of time the current input can be performed in. The first element does not use the timeout value.")]
            [SerializeField] float m_Timeout;

            public string InputName { get { return m_InputName; } set { m_InputName = value; } }
            public bool AxisInput { get { return m_AxisInput; } set { m_AxisInput = value; } }
            public float Timeout { get { return m_Timeout; } set { m_Timeout = value; } }
        }

        [Tooltip("Specifies the combo that should be performed before the ability starts.")]
        [SerializeField] protected ComboInputElement[] m_ComboInputElements;

        public ComboInputElement[] ComboInputElements { get { return m_ComboInputElements; } set { m_ComboInputElements = value; } }

        private int m_CurrentComboElementIndex;
        private float m_LastComboElementTime = -1;

        /// <summary>
        /// Can the starter start the ability?
        /// </summary>
        /// <param name="playerInput">A reference to the input component.</param>
        /// <returns>True if the starter can start the ability.</returns>
        public override bool CanInputStartAbility(PlayerInput playerInput)
        {
            var currentComboElement = m_ComboInputElements[m_CurrentComboElementIndex];
            // Check for the next input if the element hasn't timed out. If the element has timed out then the combo should be reset from the beginning.
            if (m_LastComboElementTime == -1 || m_LastComboElementTime + currentComboElement.Timeout >= Time.time) {
                // The combo can be performed. Check against the button or axis to determine if the ability should start or the next element should be checked.
                if ((!currentComboElement.AxisInput && playerInput.GetButtonDown(currentComboElement.InputName)) ||
                    (currentComboElement.AxisInput && Mathf.Abs(playerInput.GetAxisRaw(currentComboElement.InputName)) > 0.00001f)) {
                    // The end of the combos has been reached - start the ability.
                    if (m_CurrentComboElementIndex == m_ComboInputElements.Length - 1) {
                        return true;
                    }
                    // More elements exist - increase the index.
                    m_LastComboElementTime = Time.time;
                    m_CurrentComboElementIndex++;
                }
            } else {
                // The next combo element wasn't perfomed in time.
                m_LastComboElementTime = -1;
                m_CurrentComboElementIndex = 0;
            }
            return false;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        public override void AbilityStarted()
        {
            base.AbilityStarted();

            m_LastComboElementTime = -1;
            m_CurrentComboElementIndex = 0;
        }
    }
}