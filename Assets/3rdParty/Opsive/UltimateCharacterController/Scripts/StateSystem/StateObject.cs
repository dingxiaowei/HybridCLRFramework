/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using System;

namespace Opsive.UltimateCharacterController.StateSystem
{
    /// <summary>
    /// Acts as the parent object which can use the state system to change property values.
    /// </summary>
    public class StateObject : IStateOwner
    {
        [Tooltip("A list of all states that the component can change to.")]
        [HideInInspector] [SerializeField] protected State[] m_States = new State[] { new State("Default", true) };

        [Utility.NonSerialized] public State[] States { get { return m_States; } set { m_States = value; } }

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        /// <param name="gameObject">The GameObject this object is attached to.</param>
        public virtual void Initialize(GameObject gameObject)
        {
            if (Application.isPlaying) {
                StateManager.Initialize(gameObject, this, m_States);
            }
        }

        /// <summary>
        /// Activates or deactivates the specified state.
        /// </summary>
        /// <param name="stateName">The name of the state to change the active status of.</param>
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

    /// <summary>
    /// Attribute which allows the a state to automatically be added.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class AddState : Attribute
    {
        private string m_Name;
        private string m_PresetGUID;
        public string Name { get { return m_Name; } }
        public string PresetGUID { get { return m_PresetGUID; } }
        public AddState(string name, string presetGUID)
        {
            m_Name = name;
            m_PresetGUID = presetGUID;
        }
    }
}