/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.StateSystem
{
    using Opsive.Shared.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A State contains a preset which can change property values at runtime. The state has a name associated with it which allows for easy reference to it.
    /// </summary>
    [System.Serializable]
    public class State
    {
        [Tooltip("The name of the state.")]
        [SerializeField] protected string m_Name;
        [Tooltip("The preset used by the state.")]
        [SerializeField] protected Preset m_Preset;
        [Tooltip("A list of other states that the current state can prevent from being enabled if the current state is enabled.")]
        [SerializeField] protected string[] m_BlockList;
        [Tooltip("Is the state the default state?")]
        [SerializeField] protected bool m_Default;
        [Tooltip("Is the state active?")]
        [System.NonSerialized] [SerializeField] protected bool m_Active;

        private IStateOwner m_Owner;
        public IStateOwner Owner { get { return m_Owner; } }

        public string Name { get { return m_Name; }
#if UNITY_EDITOR
            set { m_Name = value; }
#endif
        }
        public Preset Preset { get { return m_Preset; } set { m_Preset = value; } }
#if UNITY_EDITOR
        public string[] BlockList { get { return m_BlockList; } set { m_BlockList = value; } }
#endif
        public bool Default { get { return m_Default; }
#if UNITY_EDITOR
            set { m_Default = value; }
#endif
        }
        public bool Active { get { return m_Active; } set { m_Active = value; } }

        [System.NonSerialized] private State[] m_BlockStates;

        /// <summary>
        /// Default constructor for State.
        /// </summary>
        public State() { }

        /// <summary>
        /// Constructor for State. Used by the component inspector.
        /// </summary>
        /// <param name="name">The name of the state.</param>
        /// <param name="defaultState">Is the state the default state?</param>
        public State(string name, bool defaultState)
        {
            m_Name = name;
            m_Default = defaultState;
        }

        /// <summary>
        /// Constructor for State. Used by the State Configuration.
        /// </summary>
        /// <param name="name">The name of the state.</param>
        /// <param name="preset">The preset that the state uses.</param>
        /// <param name="blockList">The list of blocked states.</param>
        public State(string name, Preset preset, string[] blockList)
        {
            m_Name = name;
            m_Preset = preset;
            m_BlockList = blockList;
        }

        /// <summary>
        /// Initializes the state - binds the object and applies any blocking rules.
        /// </summary>
        /// <param name="owner">The object which is bound to the state.</param>
        /// <param name="nameStateMap">A mapping between the state name and the state object.</param>
        public void Initialize(IStateOwner owner, Dictionary<string, State> nameStateMap)
        {
            // The preset can be null if the Component has no valid properties that can be changed.
            if (m_Preset != null) {
                // The preset may be used by another object of the same type.
                // Initialize a new preset to prevent the delegates from conflicting with each other.
                if (m_Preset.IsInitialized) {
                    m_Preset = Object.Instantiate(m_Preset);
                }
                m_Preset.Initialize(owner, MemberVisibility.Public);
            }
            m_Owner = owner;
            UpdateBlockList(nameStateMap);
        }

        /// <summary>
        /// Update the block list to point to the state object.
        /// </summary>
        /// <param name="nameStateMap">A mapping between the state name and the state object.</param>
        private void UpdateBlockList(Dictionary<string, State> nameStateMap)
        {
            if (m_BlockList != null && m_BlockList.Length > 0) {
                m_BlockStates = new State[m_BlockList.Length];
                State state;
                var count = 0;
                for (int i = 0; i < m_BlockList.Length; ++i) {
                    if (m_BlockList[i] == null) {
                        continue;
                    }
                    if (nameStateMap.TryGetValue(m_BlockList[count], out state)) {
                        m_BlockStates[count] = state;
                    } else {
                        Debug.LogWarning("Error: Unable to find block state with name \"" + m_BlockList[count] + "\" within state " + m_Name + " on object " + m_Owner);
                    }
                    count++;
                }

                if (m_BlockList.Length != count) {
                    System.Array.Resize(ref m_BlockList, count);
                    System.Array.Resize(ref m_BlockStates, count);
                }
            }
        }

        /// <summary>
        /// Is the state blocked by another enabed state?
        /// </summary>
        /// <returns>True if the state is blocked.</returns>
        public bool IsBlocked()
        {
            if (m_BlockStates != null) {
                for (int i = 0; i < m_BlockStates.Length; ++i) {
                    if (m_BlockStates[i] != null && m_BlockStates[i].Active && !m_BlockStates[i].IsBlocked()) {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Applies the values to the component specified within the preset object.
        /// </summary>
        public void ApplyValues()
        {
            if (m_Preset != null) {
                m_Preset.ApplyValues();
            }
        }

        /// <summary>
        /// Applies the values to the component specified within the preset object.
        /// </summary>
        public void ApplyValues(Preset.BaseDelegate[] delegates)
        {
            if (m_Preset != null) {
                m_Preset.ApplyValues(delegates);
            }
        }
    }
}