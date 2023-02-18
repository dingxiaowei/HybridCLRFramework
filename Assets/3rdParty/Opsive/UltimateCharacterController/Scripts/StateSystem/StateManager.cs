/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.StateSystem
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Handles the activation and deactivation of states.
    /// </summary>
    public class StateManager : MonoBehaviour
    {
        [Tooltip("Should the OnStateChange event be sent when the state changes active status?")]
        [SerializeField] protected bool m_SendStateChangeEvent;

        public bool SendStateChangeEvent { get { return m_SendStateChangeEvent; } set { m_SendStateChangeEvent = value; } }

        private static StateManager s_Instance;
        private static StateManager Instance
        {
            get
            {
                if (!s_Initialized) {
                    s_Instance = new GameObject("State Manager").AddComponent<StateManager>();
                    s_Initialized = true;
                }
                return s_Instance;
            }
        }
        private static bool s_Initialized;

        private Dictionary<object, Dictionary<string, State>> m_ObjectNameStateMap = new Dictionary<object, Dictionary<string, State>>();
        private Dictionary<GameObject, Dictionary<string, List<State>>> m_GameObjectNameStateList = new Dictionary<GameObject, Dictionary<string, List<State>>>();
        private Dictionary<GameObject, List<GameObject>> m_LinkedGameObjectList = new Dictionary<GameObject, List<GameObject>>();
        private Dictionary<State, State[]> m_StateArrayMap = new Dictionary<State, State[]>();
        private Dictionary<GameObject, HashSet<string>> m_ActiveCharacterStates = new Dictionary<GameObject, HashSet<string>>();
        private Dictionary<GameObject, Dictionary<string, ScheduledEventBase>> m_DisableStateTimerMap;

        /// <summary>
        /// The object has been enabled.
        /// </summary>
        private void OnEnable()
        {
            // The object may have been enabled outside of the scene unloading.
            if (s_Instance == null) {
                s_Instance = this;
                s_Initialized = true;
                SceneManager.sceneUnloaded -= SceneUnloaded;
            }
        }

        /// <summary>
        /// Initializes the states belonging to the owner on the GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to enable or disable all of the states on.</param>
        /// <param name="owner">The object that state belongs to.</param>
        /// <param name="states">A list of all of the states which the owner contains.</param>
        public static void Initialize(GameObject gameObject, IStateOwner owner, State[] states)
        {
            Instance.InitializeInternal(gameObject, owner, states);
        }

        /// <summary>
        /// Internal method which initializes the states belonging to the owner on the GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to enable or disable all of the states on.</param>
        /// <param name="owner">The object that state belongs to.</param>
        /// <param name="states">A list of all of the states which the owner contains.</param>
        private void InitializeInternal(GameObject gameObject, IStateOwner owner, State[] states)
        {
            // The last state will always be reserved for the default state.
            if (states[states.Length - 1] == null) {
                states[states.Length - 1] = new State("Default", true);
            }
            states[states.Length - 1].Preset = DefaultPreset.CreateDefaultPreset();

            Dictionary<string, State> nameStateMap;
            if (!m_ObjectNameStateMap.TryGetValue(owner, out nameStateMap)) {
                nameStateMap = new Dictionary<string, State>();
                m_ObjectNameStateMap.Add(owner, nameStateMap);
            }

            // Populate the maps for quick lookup based on owner and GameObject.
            GameObject characterGameObject = null;
            var characterLocomotion = gameObject.GetCachedParentComponent<Character.UltimateCharacterLocomotion>();
            if (characterLocomotion != null) {
                characterGameObject = characterLocomotion.gameObject;
            } else {
                var cameraController = gameObject.GetCachedParentComponent<UltimateCharacterController.Camera.CameraController>();
                if (cameraController != null) {
                    characterGameObject = cameraController.Character;
                }
            }
            for (int i = 0; i < states.Length; ++i) {
                if (states[i].Preset == null) {
                    Debug.LogError(string.Format("Error: The state {0} on {1} does not have a preset. Ensure each non-default state contains a preset.", states[i].Name, owner), owner as Object);
                }
                nameStateMap.Add(states[i].Name, states[i]);

                Dictionary<string, List<State>> nameStateList;
                if (!m_GameObjectNameStateList.TryGetValue(gameObject, out nameStateList)) {
                    nameStateList = new Dictionary<string, List<State>>();
                    m_GameObjectNameStateList.Add(gameObject, nameStateList);
                }

                // Child GameObjects should listen for states set on the parent. This for example allows an item to react to a state change even if that state change
                // is set on the character. The character GameObject does not need to be made aware of the Default state.
                if (i != states.Length - 1) {

                    if (characterGameObject != null && gameObject != characterGameObject) {
                        Dictionary<string, List<State>> characterNameStateList;
                        if (!m_GameObjectNameStateList.TryGetValue(characterGameObject, out characterNameStateList)) {
                            characterNameStateList = new Dictionary<string, List<State>>();
                            m_GameObjectNameStateList.Add(characterGameObject, characterNameStateList);
                        }

                        List<State> characterStateList;
                        if (!characterNameStateList.TryGetValue(states[i].Name, out characterStateList)) {
                            characterStateList = new List<State>();
                            characterNameStateList.Add(states[i].Name, characterStateList);
                        }

                        characterStateList.Add(states[i]);
                    }
                }

                List<State> stateList;
                if (!nameStateList.TryGetValue(states[i].Name, out stateList)) {
                    stateList = new List<State>();
                    nameStateList.Add(states[i].Name, stateList);
                }

                stateList.Add(states[i]);
                m_StateArrayMap.Add(states[i], states);
            }

            // Initialize the state after the map has been created.
            for (int i = 0; i < states.Length; ++i) {
                states[i].Initialize(owner, nameStateMap);
            }

            // The default state is always last.
            states[states.Length - 1].Active = true;

            // Remember the active character states so if a GameObject is initialized after a state has already been activated that newly initialized GameObject
            // can start the correct states. As an example an item could be picked up after the character is already aiming. That item should go directly
            // into the aim state instead of requring the character to aim again.
            if (characterGameObject != null) {
                if (characterGameObject == gameObject) {
                    // If the current GameObject is the character then the active states should be tracked.
                    if (!m_ActiveCharacterStates.ContainsKey(gameObject)) {
                        m_ActiveCharacterStates.Add(gameObject, new HashSet<string>());
                    }
                } else {
                    // If the current GameObject is not the character then the active character states should be applied to the child object.
                    HashSet<string> activeStates;
                    if (m_ActiveCharacterStates.TryGetValue(characterGameObject, out activeStates)) {
                        if (activeStates.Count > 0) {
                            foreach (var stateName in activeStates) {
                                SetState(gameObject, stateName, true);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Links the original GameObject to the linked GameObject. When GameObjects are linked the state will be updated for each GameObject even when only the
        /// original GameObject is set.
        /// </summary>
        /// <param name="original">The original GameObject to link.</param>
        /// <param name="linkedGameObject">The GameObject that should be linked to the original GameObject.</param>
        /// <param name="link">Should the GameObjects be linked. If fales the GameObjects will be unlinked.</param>
        public static void LinkGameObjects(GameObject original, GameObject linkedGameObject, bool link)
        {
            Instance.LinkGameObjectsInternal(original, linkedGameObject, link);
        }

        /// <summary>
        /// Internal method which links the original GameObject to the linked GameObject. When GameObjects are linked the state will be updated for each 
        /// GameObject even when only the original GameObject is set.
        /// </summary>
        /// <param name="original">The original GameObject to link.</param>
        /// <param name="linkedGameObject">The GameObject that should be linked to the original GameObject.</param>
        /// <param name="link">Should the GameObjects be linked. If fales the GameObjects will be unlinked.</param>
        private void LinkGameObjectsInternal(GameObject original, GameObject linkedGameObject, bool link)
        {
            List<GameObject> linkedGameObjectList;
            if (!m_LinkedGameObjectList.TryGetValue(original, out linkedGameObjectList) && link) {
                linkedGameObjectList = new List<GameObject>();
                m_LinkedGameObjectList.Add(original, linkedGameObjectList);
            }

            if (linkedGameObjectList != null) {
                if (link) {
                    linkedGameObjectList.Add(linkedGameObject);

                    // If the current GameObject is not the character then the active character states should be applied to the child object.
                    HashSet<string> activeStates;
                    if (m_ActiveCharacterStates.TryGetValue(original, out activeStates)) {
                        if (activeStates.Count > 0) {
                            foreach (var stateName in activeStates) {
                                SetState(linkedGameObject, stateName, true);
                            }
                        }
                    }
                } else {
                    linkedGameObjectList.Remove(linkedGameObject);
                }
            }
        }

        /// <summary>
        /// Activates or deactivates the specified state.
        /// </summary>
        /// <param name="owner">The object that state belongs to.</param>
        /// <param name="states">A list of all of the states which the owner contains.</param>
        /// <param name="stateName">The name of the state to change the active status of.</param>
        /// <param name="active">Should the state be activated?</param>
        public static void SetState(object owner, State[] states, string stateName, bool active)
        {
            Instance.SetStateInternal(owner, states, stateName, active);
        }

        /// <summary>
        /// Internal method which activates or deactivates the specified state.
        /// </summary>
        /// <param name="owner">The object that state belongs to.</param>
        /// <param name="states">A list of all of the states which the owner contains.</param>
        /// <param name="stateName">The name of the state to change the active status of.</param>
        /// <param name="active">Should the state be activated?</param>
        private void SetStateInternal(object owner, State[] states, string stateName, bool active)
        {
            // Lookup the state by owner.
            Dictionary<string, State> nameStateMap;
            if (!m_ObjectNameStateMap.TryGetValue(owner, out nameStateMap)) {
                Debug.LogWarning("Warning: Unable to find the name state map on object " + owner);
                return;
            }

            // Lookup the state by name.
            State state;
            if (!nameStateMap.TryGetValue(stateName, out state)) {
                Debug.LogWarning("Warning: Unable to find the state with name " + stateName);
                return;
            }

            // The state has been found, activate or deactivate the states.
            if (state.Active != active) {
                ActivateStateInternal(state, active, states);
            }
        }

        /// <summary>
        /// Activates or deactivates all of the states on the specified GameObject with the specified name.
        /// </summary>
        /// <param name="gameObject">The GameObject to enable or disable all of the states on.</param>
        /// <param name="stateName">The name of the state to change the active status of.</param>
        /// <param name="active">Should the state be activated?</param>
        public static void SetState(GameObject gameObject, string stateName, bool active)
        {
            Instance.SetStateInternal(gameObject, stateName, active);
        }

        /// <summary>
        /// Internal method which activates or deactivates all of the states on the specified GameObject with the specified name.
        /// </summary>
        /// <param name="gameObject">The GameObject to enable or disable all of the states on.</param>
        /// <param name="stateName">The name of the state to change the active status of.</param>
        /// <param name="active">Should the state be activated?</param>
        private void SetStateInternal(GameObject gameObject, string stateName, bool active)
        {
            // Remember the active character status.
            var characterLocomotion = gameObject.GetCachedComponent<Character.UltimateCharacterLocomotion>();
            if (characterLocomotion != null) {
                HashSet<string> activeStates;
                if (m_ActiveCharacterStates.TryGetValue(gameObject, out activeStates)) {
                    // If the state name appears within the set then the state is active.
                    if (active) {
                        activeStates.Add(stateName);
                    } else {
                        activeStates.Remove(stateName);
                    }
                }
            }

            // Lookup the states by GameObject.
            Dictionary<string, List<State>> nameStateList;
            if (!m_GameObjectNameStateList.TryGetValue(gameObject, out nameStateList)) {
                SetLinkStateInternal(gameObject, stateName, active);
                return;
            }

            // Lookup the states by name.
            List<State> stateList;
            if (!nameStateList.TryGetValue(stateName, out stateList)) {
                SetLinkStateInternal(gameObject, stateName, active);
                return;
            }

            // An event can be sent when the active status changes. This is useful for multiplayer in that it allows the networking implementation
            // to send the state changes across the network.
            if (m_SendStateChangeEvent) {
                EventHandler.ExecuteEvent("OnStateChange", gameObject, stateName, active);
            }

            // The states have been found, activate or deactivate the states.
            for (int i = 0; i < stateList.Count; ++i) {
                if (stateList[i].Active != active) {
                    // The state array must exist to be able to apply the changes.
                    State[] states;
                    if (!m_StateArrayMap.TryGetValue(stateList[i], out states)) {
                        Debug.LogWarning("Warning: Unable to find the state array with state name " + stateName);
                        return;
                    }

                    // Notify the owner that the states will change.
                    stateList[i].Owner.StateWillChange();

                    ActivateStateInternal(stateList[i], active, states);

                    // Notify the owner that the state has changed.
                    stateList[i].Owner.StateChange();
                }
            }

            SetLinkStateInternal(gameObject, stateName, active);
        }

        /// <summary>
        /// Internal method which activates or deactivates all of the states on the GameObjects linked from the GameObject with the specified name.
        /// </summary>
        /// <param name="gameObject">The GameObject to enable or disable all of the states on.</param>
        /// <param name="stateName">The name of the state to change the active status of.</param>
        /// <param name="active">Should the state be activated?</param>
        private void SetLinkStateInternal(GameObject gameObject, string stateName, bool active)
        {
            List<GameObject> linkedGameObjects;
            if (m_LinkedGameObjectList.TryGetValue(gameObject, out linkedGameObjects)) {
                for (int i = 0; i < linkedGameObjects.Count; ++i) {
                    SetStateInternal(linkedGameObjects[i], stateName, active);
                }
            }
        }

        /// <summary>
        /// Activates or deactivates the specified state. In most cases SetState should be used instead of ActivateState.
        /// </summary>
        /// <param name="state">The state to activate or deactivate.</param>
        /// <param name="active">Should the state be activated?</param>
        /// <param name="states">The array of states that the state belongs to.</param>
        public static void ActivateState(State state, bool active, State[] states)
        {
            Instance.ActivateStateInternal(state, active, states);
        }

        /// <summary>
        /// Internal method which activates or deactivates the specified state. In most cases SetState should be used instead of ActivateState.
        /// </summary>
        /// <param name="state">The state to activate or deactivate.</param>
        /// <param name="active">Should the state be activated?</param>
        /// <param name="states">The array of states that the state belongs to.</param>
        private void ActivateStateInternal(State state, bool active, State[] states)
        {
            // Return early if there no work needs to be done.
            if (state.Active == active) {
                return;
            }

            // Set the active state.
            state.Active = active;

            // Apply the changes.
            CombineStates(state, active, states);
        }

        /// <summary>
        /// Loops through the states and applies the value. The states are looped in the order specified within the inspector from top to bottom.
        /// </summary>
        /// <param name="state">The state that was activated or deactivated.</param>
        /// <param name="active">Was the activated?</param>
        /// <param name="states">The array of states that the state belongs to.</param>
        private void CombineStates(State state, bool active, State[] states)
        {
            if (active) {
                // Apply the default value of the blocked states before looping through all of the states. This will ensure the default value
                // is set for that property if no other states set the property value.
                for (int i = states.Length - 2; i > -1; --i) {
                    if (states[i].Active && states[i].IsBlocked()) {
                        states[states.Length - 1].ApplyValues(states[i].Preset.Delegates);
                    }
                }
            } else {
                // Restore the default values if the state is no longer active.
                states[states.Length - 1].ApplyValues(state.Preset.Delegates);
            }

            // Loop backwards so the higher priority states are applied first. Do not apply the default state because it was applied above.
            for (int i = states.Length - 2; i > -1; --i) {
                // Don't apply the state if the state isn't active.
                if (!states[i].Active) {
                    continue;
                }

                // Do not apply the state if it is currently blocked by another state.
                if (states[i].IsBlocked()) {
                    continue;
                }

                states[i].ApplyValues();
            }
        }

        /// <summary>
        /// Activates the state and then deactivates the state after the specified amount of time.
        /// </summary>
        /// <param name="gameObject">The Gameobject to set the state on.</param>
        /// <param name="stateName">The name of the state to activate and then deactivate.</param>
        /// <param name="time">The amount of time that should elapse before the state is disabled.</param>
        public static void DeactivateStateTimer(GameObject gameObject, string stateName, float time)
        {
            Instance.DeactivateStateTimerInternal(gameObject, stateName, time);
        }

        /// <summary>
        /// Internal method which activates the state and then deactivates the state after the specified amount of time.
        /// </summary>
        /// <param name="gameObject">The Gameobject to set the state on.</param>
        /// <param name="stateName">The name of the state to activate and then deactivate.</param>
        /// <param name="time">The amount of time that should elapse before the state is disabled.</param>
        private void DeactivateStateTimerInternal(GameObject gameObject, string stateName, float time)
        {
            if (m_DisableStateTimerMap == null) {
                m_DisableStateTimerMap = new Dictionary<GameObject, Dictionary<string, ScheduledEventBase>>();
            }

            Dictionary<string, ScheduledEventBase> stateNameEventMap;
            if (m_DisableStateTimerMap.TryGetValue(gameObject, out stateNameEventMap)) {
                ScheduledEventBase disableEvent;
                if (stateNameEventMap.TryGetValue(stateName, out disableEvent)) {
                    // The state name exists. This means that the timer is currently active and should first been cancelled.
                    Scheduler.Cancel(disableEvent);
                    disableEvent = Scheduler.Schedule(time, DeactivateState, gameObject, stateName);
                } else {
                    // The state name hasn't been added yet. Add it to the map.
                    disableEvent = Scheduler.Schedule(time, DeactivateState, gameObject, stateName);
                    stateNameEventMap.Add(stateName, disableEvent);
                }
            } else {
                // Neither the GameObject nor the state has been activated. Create the maps.
                stateNameEventMap = new Dictionary<string, ScheduledEventBase>();
                var disableEvent = Scheduler.Schedule(time, DeactivateState, gameObject, stateName);
                stateNameEventMap.Add(stateName, disableEvent);
                m_DisableStateTimerMap.Add(gameObject, stateNameEventMap);
            }
        }

        /// <summary>
        /// Deactives the specified state and removes it form the timer map.
        /// </summary>
        /// <param name="gameObject">The GameObject to set the state on.</param>
        /// <param name="stateName">The name of the state to set.</param>
        private void DeactivateState(GameObject gameObject, string stateName)
        {
            SetState(gameObject, stateName, false);

            Dictionary<string, ScheduledEventBase> stateNameEventMap;
            if (m_DisableStateTimerMap.TryGetValue(gameObject, out stateNameEventMap)) {
                stateNameEventMap.Remove(stateName);
            }
        }

        /// <summary>
        /// Reset the initialized variable when the scene is no longer loaded.
        /// </summary>
        /// <param name="scene">The scene that was unloaded.</param>
        private void SceneUnloaded(Scene scene)
        {
            s_Initialized = false;
            s_Instance = null;
            SceneManager.sceneUnloaded -= SceneUnloaded;
        }

        /// <summary>
        /// The object has been disabled.
        /// </summary>
        private void OnDisable()
        {
            SceneManager.sceneUnloaded += SceneUnloaded;
        }

#if UNITY_2019_3_OR_NEWER
        /// <summary>
        /// Reset the static variables for domain reloading.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void DomainReset()
        {
            s_Initialized = false;
            s_Instance = null;
        }
#endif
    }
}