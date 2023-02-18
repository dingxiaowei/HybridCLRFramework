/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.StateSystem
{
    using Opsive.Shared.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The StateConfiguration class contains an array of profiles with prespecified states that can be added to an object.
    /// </summary>
    public class StateConfiguration : ScriptableObject
    {
        [Tooltip("An array of profiles which map a name to a list of states.")]
        [SerializeField] protected Profile[] m_Profiles;

        public Profile[] Profiles { get { return m_Profiles; } set { m_Profiles = value; ResetInitialization(); } }

        private Dictionary<string, Dictionary<Type, Profile.StateElement[]>> m_ProfileStateMap;
        
        /// <summary>
        /// The Profile class contains an array of states that should be added.
        /// </summary>
        [Serializable]
        public class Profile
        {
            /// <summary>
            /// Specifies the type of object that represents the profile.
            /// </summary>
            public enum ProfileType { Character, Item, Camera }

            /// <summary>
            /// A representation of the StateSystem.State object, used to restore states on an object.
            /// </summary>
            [Serializable]
            public class StateElement
            {
                [Tooltip("The name of the state.")]
                [SerializeField] protected string m_Name;
                [Tooltip("The preset which the state belongs to.")]
                [SerializeField] protected PersistablePreset m_Preset;
                [Tooltip("Any other states that the current state can block.")]
                [SerializeField] protected string[] m_BlockList;
                [Tooltip("Is the state the default state? Only one state can be the default for each object type.")]
                [SerializeField] protected bool m_Default;

                public string Name { get { return m_Name; } set { m_Name = value; } }
                public PersistablePreset Preset { get { return m_Preset; } set { m_Preset = value; } }
                public string[] BlockList { get { return m_BlockList; } set { m_BlockList = value; } }
                public bool Default { get { return m_Default; } set { m_Default = value; } }

                /// <summary>
                /// Default constructor.
                /// </summary>
                public StateElement() { }

                /// <summary>
                /// Three parameter constructor.
                /// </summary>
                /// <param name="name">The name of the state.</param>
                /// <param name="preset">The preset used by the state.</param>
                /// <param name="blockList">The list of states that the current state should block.</param>
                /// <param name="defaultState">Is the state a default state?</param>
                public StateElement(string name, PersistablePreset preset, string[] blockList, bool defaultState)
                {
                    m_Name = name;
                    m_Preset = preset;
                    m_BlockList = blockList;
                    m_Default = defaultState;
                }
            }

            [Tooltip("The name of the profile.")]
            [SerializeField] protected string m_Name;
            [Tooltip("The type of object the profile represents.")]
            [SerializeField] protected ProfileType m_Type;
            [Tooltip("The states which belong to the profile.")]
            [SerializeField] protected StateElement[] m_StateElements;

            public string Name { get { return m_Name; } set { m_Name = value; } }
            public ProfileType Type { get { return m_Type; } set { m_Type = value; } }
            public StateElement[] StateElements { get { return m_StateElements; } set { m_StateElements = value; } }
        }

        /// <summary>
        /// Creates a mapping for all of the profiles.
        /// </summary>
        private void Initialize()
        {
            // The mapping may have already been initialized.
            if (m_ProfileStateMap != null) {
                return;
            }

            m_ProfileStateMap = new Dictionary<string, Dictionary<Type, Profile.StateElement[]>>();
            if (m_Profiles == null) {
                return;
            }
            for (int i = 0; i < m_Profiles.Length; ++i) {
                var profileStateElements = m_Profiles[i].StateElements;
                var typeStateMap = new Dictionary<Type, Profile.StateElement[]>();
                m_ProfileStateMap.Add(m_Profiles[i].Name, typeStateMap);
                for (int j = 0; j < profileStateElements.Length; ++j) {
                    // The state must be valid.
                    if (string.IsNullOrEmpty(profileStateElements[j].Name) || profileStateElements[j].Preset == null) {
                        continue;
                    }

                    var objType = Utility.UnityEngineUtility.GetType(profileStateElements[j].Preset.Data.ObjectType);
                    if (objType == null) {
                        continue;
                    }

                    Profile.StateElement[] stateElements;
                    if (!typeStateMap.TryGetValue(objType, out stateElements)) {
                        stateElements = new Profile.StateElement[0];
                        typeStateMap.Add(objType, stateElements);
                    }

                    // Add the state to the array that is specific for the object type.
                    Array.Resize(ref stateElements, stateElements.Length + 1);
                    stateElements[stateElements.Length - 1] = profileStateElements[j];
                    // Allocating a new array requires the dictionary element to be updated.
                    typeStateMap[objType] = stateElements;
                }
            }
        }

        /// <summary>
        /// After a change the state map must be cleared so it can be initialized again.
        /// </summary>
        public void ResetInitialization()
        {
            m_ProfileStateMap = null;
        }

        /// <summary>
        /// Returns a list of profiles that have been added to the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to retrieve the profiles for.</param>
        /// <param name="type">The type of profiles that should be shown.</param>
        /// <returns>A list of profiles that have been added to the specified GameObject.</returns>
        public List<string> GetProfilesForGameObject(GameObject gameObject, Profile.ProfileType type)
        {
            Initialize();

            var profileNames = new List<string>();
            if (gameObject == null) {
                // All of the profiles for the specfied type should be shown if the GameObject is null.
                if (m_Profiles != null) {
                    for (int i = 0; i < m_Profiles.Length; ++i) {
                        if (type != m_Profiles[i].Type) {
                            continue;
                        }
                        profileNames.Add(m_Profiles[i].Name);
                    }
                }
                return profileNames;
            }
            var stateBehaviors = gameObject.GetComponents<StateBehavior>();
            for (int i = 0; i < stateBehaviors.Length; ++i) {
                GetProfilesForType(stateBehaviors[i].GetType(), type, profileNames);

                // The GameObject may contain the UltimateCharacterLocomotion component where the 
                // movement types, abilities, item abilities, and effects also need to be searched.
                if (stateBehaviors[i] is Character.UltimateCharacterLocomotion) {
                    var characterLocomotion = stateBehaviors[i] as Character.UltimateCharacterLocomotion;

                    characterLocomotion.DeserializeMovementTypes();
                    for (int j = 0; j < characterLocomotion.MovementTypes.Length; ++j) {
                        GetProfilesForType(characterLocomotion.MovementTypes[j].GetType(), type, profileNames);
                    }

                    characterLocomotion.DeserializeAbilities();
                    if (characterLocomotion.Abilities != null) {
                        for (int j = 0; j < characterLocomotion.Abilities.Length; ++j) {
                            GetProfilesForType(characterLocomotion.Abilities[j].GetType(), type, profileNames);
                        }
                    }

                    characterLocomotion.DeserializeItemAbilities();
                    if (characterLocomotion.ItemAbilities != null) {
                        for (int j = 0; j < characterLocomotion.ItemAbilities.Length; ++j) {
                            GetProfilesForType(characterLocomotion.ItemAbilities[j].GetType(), type, profileNames);
                        }
                    }

                    characterLocomotion.DeserializeEffects();
                    if (characterLocomotion.Effects != null) {
                        for (int j = 0; j < characterLocomotion.Effects.Length; ++j) {
                            GetProfilesForType(characterLocomotion.Effects[j].GetType(), type, profileNames);
                        }
                    }
                }

                // The GameObject may contain the CameraController component where the view types also need to be searched.
                if (stateBehaviors[i] is UltimateCharacterController.Camera.CameraController) {
                    var cameraController = stateBehaviors[i] as UltimateCharacterController.Camera.CameraController;

                    cameraController.DeserializeViewTypes();
                    for (int j = 0; j < cameraController.ViewTypes.Length; ++j) {
                        GetProfilesForType(cameraController.ViewTypes[j].GetType(), type, profileNames);
                    }
                }
            }

            return profileNames;
        }

        /// <summary>
        /// Adds to a list of profiles that contain the specified type.
        /// </summary>
        /// <param name="objType">The type of object to retrieve the profiles for.</param>
        /// <param name="profileNames">A list of profiles that contain the specifeid type.</param>
        private void GetProfilesForType(Type objType, Profile.ProfileType profileType, List<string> profileNames)
        {
            for (int i = 0; i < m_Profiles.Length; ++i) {
                if (m_Profiles[i].Type != profileType) {
                    continue;
                }
                Dictionary<Type, Profile.StateElement[]> typeStateMap;
                if (!m_ProfileStateMap.TryGetValue(m_Profiles[i].Name, out typeStateMap)) {
                    continue;
                }

                // Add the profile name if the type exists.
                if (typeStateMap.ContainsKey(objType) && !profileNames.Contains(m_Profiles[i].Name)) {
                    profileNames.Add(m_Profiles[i].Name);
                }
            }
        }

        /// <summary>
        /// Add the states to the GameObject that have been added to the specified profile name.
        /// </summary>
        /// <param name="profileName">The name of the profile to retrieve the states from.</param>
        /// <param name="type">The type of profiles that should be shown.</param>
        /// <param name="gameObject">The GameObject to add the states to.</param>
        public void AddStatesToGameObject(string profileName, GameObject gameObject)
        {
            Initialize();

            var stateBehaviors = gameObject.GetComponents<StateBehavior>();
            for (int i = 0; i < stateBehaviors.Length; ++i) {
                AddStatesToObject(profileName, stateBehaviors[i]);

                // The GameObject may contain the UltimateCharacterLocomotion component where the 
                // movement types, abilities, item abilities, and effects also need to be searched.
                if (stateBehaviors[i] is Character.UltimateCharacterLocomotion) {
                    var characterLocomotion = stateBehaviors[i] as Character.UltimateCharacterLocomotion;

                    characterLocomotion.DeserializeMovementTypes();
                    if (characterLocomotion.MovementTypes != null) {
                        for (int j = 0; j < characterLocomotion.MovementTypes.Length; ++j) {
                            AddStatesToObject(profileName, characterLocomotion.MovementTypes[j]);
                        }
                        var movementTypes = new List<Character.MovementTypes.MovementType>(characterLocomotion.MovementTypes);
                        characterLocomotion.MovementTypeData = Shared.Utility.Serialization.Serialize<Character.MovementTypes.MovementType>(movementTypes);
                        characterLocomotion.MovementTypes = movementTypes.ToArray();
                    }

                    characterLocomotion.DeserializeAbilities();
                    if (characterLocomotion.Abilities != null) {
                        for (int j = 0; j < characterLocomotion.Abilities.Length; ++j) {
                            AddStatesToObject(profileName, characterLocomotion.Abilities[j]);
                        }
                        Utility.Builders.AbilityBuilder.SerializeAbilities(characterLocomotion);
                    }

                    characterLocomotion.DeserializeItemAbilities();
                    if (characterLocomotion.ItemAbilities != null) {
                        for (int j = 0; j < characterLocomotion.ItemAbilities.Length; ++j) {
                            AddStatesToObject(profileName, characterLocomotion.ItemAbilities[j]);
                        }
                        Utility.Builders.AbilityBuilder.SerializeItemAbilities(characterLocomotion);
                    }

                    characterLocomotion.DeserializeEffects();
                    if (characterLocomotion.Effects != null) {
                        for (int j = 0; j < characterLocomotion.Effects.Length; ++j) {
                            AddStatesToObject(profileName, characterLocomotion.Effects[j]);
                        }
                        var effects = new List<Character.Effects.Effect>(characterLocomotion.Effects);
                        characterLocomotion.EffectData = Shared.Utility.Serialization.Serialize<Character.Effects.Effect>(effects);
                        characterLocomotion.Effects = effects.ToArray();
                    }
                }

                // The GameObject may contain the CameraController component where the view types also need to be searched.
                if (stateBehaviors[i] is UltimateCharacterController.Camera.CameraController) {
                    var cameraController = stateBehaviors[i] as UltimateCharacterController.Camera.CameraController;

                    cameraController.DeserializeViewTypes();
                    if (cameraController.ViewTypes != null) {
                        for (int j = 0; j < cameraController.ViewTypes.Length; ++j) {
                            AddStatesToObject(profileName, cameraController.ViewTypes[j]);
                        }
                        Utility.Builders.ViewTypeBuilder.SerializeViewTypes(cameraController);
                    }
                }
            }
        }

        /// <summary>
        /// Add the states to the object that have been added to the specified profile name.
        /// </summary>
        /// <param name="profileName">The name of the profile to retrieve the states from.</param>
        /// <param name="obj">The object to add the states to.</param>
        private void AddStatesToObject(string profileName, object obj)
        {
            Dictionary<Type, Profile.StateElement[]> typeStateMap;
            if (!m_ProfileStateMap.TryGetValue(profileName, out typeStateMap)) {
                return;
            }

            Profile.StateElement[] stateElements;
            if (!typeStateMap.TryGetValue(obj.GetType(), out stateElements)) {
                return;
            }

            var defaultIndex = -1;
            for (int i = 0; i < stateElements.Length; ++i) {
                if (stateElements[i].Default) {
                    stateElements[i].Preset.Initialize(obj, MemberVisibility.Public);
                    stateElements[i].Preset.ApplyValues();
                    defaultIndex = i;
                    break;
                }
            }
            // The profile may contain states from more than one of the same object type. Only one set of states should be applied.
            if (defaultIndex != -1 && defaultIndex < stateElements.Length - 1) {
                Array.Resize(ref stateElements, defaultIndex + 1);
            }
            var states = new State[stateElements.Length + (defaultIndex != -1 ? 0 : 1)]; // One element is reserved for the default state.
            for (int i = 0; i < stateElements.Length; ++i) {
                if (stateElements[i].Default) {
                    continue;
                }
                states[i] = new State(stateElements[i].Name, stateElements[i].Preset, stateElements[i].BlockList);
            }
            if (obj is StateBehavior) {
                var stateBehavior = obj as StateBehavior;
                states[states.Length - 1] = stateBehavior.States[stateBehavior.States.Length - 1];
                stateBehavior.States = states;
            } else { // StateObject.
                var stateObject = obj as StateObject;
                states[states.Length - 1] = stateObject.States[stateObject.States.Length - 1];
                stateObject.States = states;
            }
        }
    }
}