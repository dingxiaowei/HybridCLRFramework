/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.StateSystem
{
    /// <summary>
    /// Acts as the parent component which can use the state system to change property values.
    /// </summary>
    public class StateBehavior : MonoBehaviour, IStateOwner
    {
        [Tooltip("A list of all states that the component can change to.")]
        [HideInInspector] [SerializeField] protected State[] m_States = new State[] { new State("Default", true) };

        [Utility.NonSerialized] public State[] States { get { return m_States; } set { m_States = value; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected virtual void Awake()
        {
            if (Application.isPlaying) {
                StateManager.Initialize(gameObject, this, m_States);
            }
        }

        /// <summary>
        /// Activates or deactivates the specified state.
        /// </summary>
        /// <param name="stateName">The name of the state to change the activate status of.</param>
        /// <param name="active">Should the state be activated?</param>
        public void SetState(string stateName, bool active)
        {
            StateManager.SetState(this, m_States, stateName, active);
        }

        /// <summary>
        /// Callback when the StateManager will change the active state on the current object.
        /// </summary>
        public virtual void StateWillChange() { }

        /// <summary>
        /// Callback when the StateManager has changed the active state on the current object.
        /// </summary>
        public virtual void StateChange() { }
    }
}