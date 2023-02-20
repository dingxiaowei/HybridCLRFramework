/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using System;
using System.Collections.Generic;
using Opsive.UltimateCharacterController.Game;

namespace Opsive.UltimateCharacterController.Events
{
    /// <summary>
    /// Adds a generic event system. The event system allows objects to register, T2nregister, and execute events on a particular object.
    /// </summary>
    public class EventHandler : MonoBehaviour
    {
        private static Dictionary<object, Dictionary<string, List<InvokableActionBase>>> s_EventTable = new Dictionary<object, Dictionary<string, List<InvokableActionBase>>>();
        private static Dictionary<string, List<InvokableActionBase>> s_GlobalEventTable = new Dictionary<string, List<InvokableActionBase>>();

        /// <summary>
        /// Clear the event table when the GameObject is disabled.
        /// </summary>
        private void OnDisable()
        {
            if (gameObject != null && !gameObject.activeSelf) {
                return;
            }

            ClearTable();
        }

        /// <summary>
        /// Clear the event table when the GameObject is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            ClearTable();
        }

        /// <summary>
        /// Clears the actual events.
        /// </summary>
        private void ClearTable()
        {
            s_EventTable.Clear();
        }

        /// <summary>
        /// Registers a new global event.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="invokableAction">The invokableAction to call when the event executes.</param>
        private static void RegisterEvent(string eventName, InvokableActionBase invokableAction)
        {
            List<InvokableActionBase> actionList;
            if (s_GlobalEventTable.TryGetValue(eventName, out actionList)) {
#if UNITY_EDITOR
                for (int i = 0; i < actionList.Count; ++i) {
                    if (actionList[i].GetDelegate() == invokableAction.GetDelegate()) {
                        Debug.LogWarning("Warning: the function \"" + invokableAction.GetDelegate().Method.ToString() + 
                            "\" is trying to subscribe to the " + eventName + " more than once." + invokableAction.GetDelegate().Target);
                        break;
                    }
                }
#endif
                actionList.Add(invokableAction);
            } else {
                actionList = new List<InvokableActionBase>();
                actionList.Add(invokableAction);
                s_GlobalEventTable.Add(eventName, actionList);
            }
        }

        /// <summary>
        /// Registers a new event.
        /// </summary>
        /// <param name="obj">The target object.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="invokableAction">The invokableAction to call when the event executes.</param>
        private static void RegisterEvent(object obj, string eventName, InvokableActionBase invokableAction)
        {
#if UNITY_EDITOR
            if (obj == null) {
                Debug.LogError("EventHandler.RegisterEvent error: target object cannot be null.");
                return;
            }
#endif

            Dictionary<string, List<InvokableActionBase>> handlers;
            if (!s_EventTable.TryGetValue(obj, out handlers)) {
                handlers = new Dictionary<string, List<InvokableActionBase>>();
                s_EventTable.Add(obj, handlers);
            }

            List<InvokableActionBase> actionList;
            if (handlers.TryGetValue(eventName, out actionList)) {
#if UNITY_EDITOR
                for (int i = 0; i < actionList.Count; ++i) {
                    if (actionList[i].GetDelegate() == invokableAction.GetDelegate()) {
                        Debug.LogWarning("Warning: the function \"" + invokableAction.GetDelegate().Method.ToString() + 
                            "\" is trying to subscribe to the " + eventName + " on the " + obj + " object more than once.");
                        break;
                    }
                }
#endif
                actionList.Add(invokableAction);
            } else {
                actionList = new List<InvokableActionBase>();
                actionList.Add(invokableAction);
                handlers.Add(eventName, actionList);
            }
        }

        /// <summary>
        /// Updates the global event table to determine if any objects can be removed.
        /// </summary>
        /// <param name="eventName">The interested event name.</param>
        /// <returns>The invokeable actions for the interested event.</returns>
        private static List<InvokableActionBase> GetActionList(string eventName)
        {
            List<InvokableActionBase> actionList;
            if (s_GlobalEventTable.TryGetValue(eventName, out actionList)) {
                return actionList;
            }
            return null;
        }

        /// <summary>
        /// Determines if the event can be removed from the global event table.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="actionList">The list of actions subscribed to the event.</param>
        private static void CheckForEventRemoval(string eventName, List<InvokableActionBase> actionList)
        {
            if (actionList.Count == 0) {
                s_GlobalEventTable.Remove(eventName);
            }
        }

        /// <summary>
        /// Returns the invokeable actions for a particular event on a particular object.
        /// </summary>
        /// <param name="obj">The interested object.</param>
        /// <param name="eventName">The interested event name.</param>
        /// <returns>The invokeable actions for the interested event.</returns>
        private static List<InvokableActionBase> GetActionList(object obj, string eventName)
        {
#if UNITY_EDITOR
            if (obj == null) {
                Debug.LogError("EventHandler.GetActionList error: target object cannot be null.");
                return null;
            }
#endif

            Dictionary<string, List<InvokableActionBase>> handlers;
            if (s_EventTable.TryGetValue(obj, out handlers)) {
                List<InvokableActionBase> actionList;
                if (handlers.TryGetValue(eventName, out actionList)) {
                    return actionList;
                }
            }
            return null;
        }

        /// <summary>
        /// Updates the event table to determine if any objects can be removed.
        /// </summary>
        /// <param name="obj">The object that is subscribed to the event.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="actionList">The list of actions subscribed to the event.</param>
        private static void CheckForEventRemoval(object obj, string eventName, List<InvokableActionBase> actionList)
        {
            if (actionList.Count == 0) {
                Dictionary<string, List<InvokableActionBase>> handlers;
                if (s_EventTable.TryGetValue(obj, out handlers)) {
                    handlers.Remove(eventName);
                    if (handlers.Count == 0) {
                        s_EventTable.Remove(obj);
                    }
                }
            }
        }

        /// <summary>
        /// Register a new global event with no parameters.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The function to call when the event executes.</param>
        public static void RegisterEvent(string eventName, Action action)
        {
            var invokableAction = ObjectPool.Get<InvokableAction>();
            invokableAction.Initialize(action);
            RegisterEvent(eventName, invokableAction);
        }

        /// <summary>
        /// Register a new event with no parameters.
        /// </summary>
        /// <param name="obj">The target object.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The function to call when the event executes.</param>
        public static void RegisterEvent(object obj, string eventName, Action action)
        {
            var invokableAction = ObjectPool.Get<InvokableAction>();
            invokableAction.Initialize(action);
            RegisterEvent(obj, eventName, invokableAction);
        }

        /// <summary>
        /// Register a new global event with one parameter.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The function to call when the event executes.</param>
        public static void RegisterEvent<T1>(string eventName, Action<T1> action)
        {
            var invokableAction = ObjectPool.Get<InvokableAction<T1>>();
            invokableAction.Initialize(action);
            RegisterEvent(eventName, invokableAction);
        }

        /// <summary>
        /// Register a new event with one parameter.
        /// </summary>
        /// <param name="obj">The target object.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The function to call when the event executes.</param>
        public static void RegisterEvent<T1>(object obj, string eventName, Action<T1> action)
        {
            var invokableAction = ObjectPool.Get<InvokableAction<T1>>();
            invokableAction.Initialize(action);
            RegisterEvent(obj, eventName, invokableAction);
        }

        /// <summary>
        /// Register a new global event with two parameters.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The function to call when the event executes.</param>
        public static void RegisterEvent<T1, T2>(string eventName, Action<T1, T2> action)
        {
            var invokableAction = ObjectPool.Get<InvokableAction<T1, T2>>();
            invokableAction.Initialize(action);
            RegisterEvent(eventName, invokableAction);
        }

        /// <summary>
        /// Register a new event with two parameters.
        /// </summary>
        /// <param name="obj">The target object.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The function to call when the event executes.</param>
        public static void RegisterEvent<T1, T2>(object obj, string eventName, Action<T1, T2> action)
        {
            var invokableAction = ObjectPool.Get<InvokableAction<T1, T2>>();
            invokableAction.Initialize(action);
            RegisterEvent(obj, eventName, invokableAction);
        }

        /// <summary>
        /// Register a new global event with three parameters.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The function to call when the event executes.</param>
        public static void RegisterEvent<T1, T2, T3>(string eventName, Action<T1, T2, T3> action)
        {
            var invokableAction = ObjectPool.Get<InvokableAction<T1, T2, T3>>();
            invokableAction.Initialize(action);
            RegisterEvent(eventName, invokableAction);
        }

        /// <summary>
        /// Register a new event with three parameters.
        /// </summary>
        /// <param name="obj">The target object.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The function to call when the event executes.</param>
        public static void RegisterEvent<T1, T2, T3>(object obj, string eventName, Action<T1, T2, T3> action)
        {
            var invokableAction = ObjectPool.Get<InvokableAction<T1, T2, T3>>();
            invokableAction.Initialize(action);
            RegisterEvent(obj, eventName, invokableAction);
        }

        /// <summary>
        /// Register a new global event with four parameters.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The function to call when the event executes.</param>
        public static void RegisterEvent<T1, T2, T3, T4>(string eventName, Action<T1, T2, T3, T4> action)
        {
            var invokableAction = ObjectPool.Get<InvokableAction<T1, T2, T3, T4>>();
            invokableAction.Initialize(action);
            RegisterEvent(eventName, invokableAction);
        }

        /// <summary>
        /// Register a new event with four parameters.
        /// </summary>
        /// <param name="obj">The target object.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The function to call when the event executes.</param>
        public static void RegisterEvent<T1, T2, T3, T4>(object obj, string eventName, Action<T1, T2, T3, T4> action)
        {
            var invokableAction = ObjectPool.Get<InvokableAction<T1, T2, T3, T4>>();
            invokableAction.Initialize(action);
            RegisterEvent(obj, eventName, invokableAction);
        }

        /// <summary>
        /// Register a new global event with five parameters.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The function to call when the event executes.</param>
        public static void RegisterEvent<T1, T2, T3, T4, T5>(string eventName, Action<T1, T2, T3, T4, T5> action)
        {
            var invokableAction = ObjectPool.Get<InvokableAction<T1, T2, T3, T4, T5>>();
            invokableAction.Initialize(action);
            RegisterEvent(eventName, invokableAction);
        }

        /// <summary>
        /// Register a new event with five parameters.
        /// </summary>
        /// <param name="obj">The target object.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The function to call when the event executes.</param>
        public static void RegisterEvent<T1, T2, T3, T4, T5>(object obj, string eventName, Action<T1, T2, T3, T4, T5> action)
        {
            var invokableAction = ObjectPool.Get<InvokableAction<T1, T2, T3, T4, T5>>();
            invokableAction.Initialize(action);
            RegisterEvent(obj, eventName, invokableAction);
        }

        /// <summary>
        /// Register a new global event with six parameters.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The function to call when the event executes.</param>
        public static void RegisterEvent<T1, T2, T3, T4, T5, T6>(string eventName, Action<T1, T2, T3, T4, T5, T6> action)
        {
            var invokableAction = ObjectPool.Get<InvokableAction<T1, T2, T3, T4, T5, T6>>();
            invokableAction.Initialize(action);
            RegisterEvent(eventName, invokableAction);
        }

        /// <summary>
        /// Register a new event with six parameters.
        /// </summary>
        /// <param name="obj">The target object.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The function to call when the event executes.</param>
        public static void RegisterEvent<T1, T2, T3, T4, T5, T6>(object obj, string eventName, Action<T1, T2, T3, T4, T5, T6> action)
        {
            var invokableAction = ObjectPool.Get<InvokableAction<T1, T2, T3, T4, T5, T6>>();
            invokableAction.Initialize(action);
            RegisterEvent(obj, eventName, invokableAction);
        }

        /// <summary>
        /// Executes the global event with no parameters.
        /// </summary>
        /// <param name="obj">The object that the event is attached to.</param>
        /// <param name="eventName">The name of the event.</param>
        public static void ExecuteEvent(string eventName)
        {
            var actions = GetActionList(eventName);
            if (actions != null) {
                for (int i = actions.Count - 1; i >= 0; --i) {
                    (actions[i] as InvokableAction).Invoke();
                }
            }
        }

        /// <summary>
        /// Executes the event with no parameters.
        /// </summary>
        /// <param name="obj">The object that the event is attached to.</param>
        /// <param name="eventName">The name of the event.</param>
        public static void ExecuteEvent(object obj, string eventName)
        {
            var actions = GetActionList(obj, eventName);
            if (actions != null) {
                for (int i = actions.Count - 1; i >= 0; --i) {
                    (actions[i] as InvokableAction).Invoke();
                }
            }
        }

        /// <summary>
        /// Executes the global event with one parameter.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="arg1">The first parameter.</param>
        public static void ExecuteEvent<T1>(string eventName, T1 arg1)
        {
            var actions = GetActionList(eventName);
            if (actions != null) {
                for (int i = actions.Count - 1; i >= 0; --i) {
                    (actions[i] as InvokableAction<T1>).Invoke(arg1);
                }
            }
        }

        /// <summary>
        /// Executes the event with one parameter.
        /// </summary>
        /// <param name="obj">The object that the event is attached to.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="arg1">The first parameter.</param>
        public static void ExecuteEvent<T1>(object obj, string eventName, T1 arg1)
        {
            var actions = GetActionList(obj, eventName);
            if (actions != null) {
                for (int i = actions.Count - 1; i >= 0; --i) {
                    (actions[i] as InvokableAction<T1>).Invoke(arg1);
                }
            }
        }

        /// <summary>
        /// Executes the global event with two parameters.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="arg1">The first parameter.</param>
        /// <param name="arg2">The second parameter.</param>
        public static void ExecuteEvent<T1, T2>(string eventName, T1 arg1, T2 arg2)
        {
            var actions = GetActionList(eventName);
            if (actions != null) {
                for (int i = actions.Count - 1; i >= 0; --i) {
                    (actions[i] as InvokableAction<T1, T2>).Invoke(arg1, arg2);
                }
            }
        }

        /// <summary>
        /// Executes the event with two parameters.
        /// </summary>
        /// <param name="obj">The object that the event is attached to.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="arg1">The first parameter.</param>
        /// <param name="arg2">The second parameter.</param>
        public static void ExecuteEvent<T1, T2>(object obj, string eventName, T1 arg1, T2 arg2)
        {
            var actions = GetActionList(obj, eventName);
            if (actions != null) {
                for (int i = actions.Count - 1; i >= 0; --i) {
                    (actions[i] as InvokableAction<T1, T2>).Invoke(arg1, arg2);
                }
            }
        }

        /// <summary>
        /// Executes the global event with three parameters.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="arg1">The first parameter.</param>
        /// <param name="arg2">The second parameter.</param>
        /// <param name="arg3">The third parameter.</param>
        public static void ExecuteEvent<T1, T2, T3>(string eventName, T1 arg1, T2 arg2, T3 arg3)
        {
            var actions = GetActionList(eventName);
            if (actions != null) {
                for (int i = actions.Count - 1; i >= 0; --i) {
                    (actions[i] as InvokableAction<T1, T2, T3>).Invoke(arg1, arg2, arg3);
                }
            }
        }

        /// <summary>
        /// Executes the event with three parameters.
        /// </summary>
        /// <param name="obj">The object that the event is attached to.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="arg1">The first parameter.</param>
        /// <param name="arg2">The second parameter.</param>
        /// <param name="arg3">The third parameter.</param>
        public static void ExecuteEvent<T1, T2, T3>(object obj, string eventName, T1 arg1, T2 arg2, T3 arg3)
        {
            var actions = GetActionList(obj, eventName);
            if (actions != null) {
                for (int i = 0; i < actions.Count; ++i) {
                    (actions[i] as InvokableAction<T1, T2, T3>).Invoke(arg1, arg2, arg3);
                }
            }
        }

        /// <summary>
        /// Executes the global event with four parameters.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="arg1">The first parameter.</param>
        /// <param name="arg2">The second parameter.</param>
        /// <param name="arg3">The third parameter.</param>
        /// <param name="arg4">The fourth parameter.</param>
        public static void ExecuteEvent<T1, T2, T3, T4>(string eventName, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            var actions = GetActionList(eventName);
            if (actions != null) {
                for (int i = actions.Count - 1; i >= 0; --i) {
                    (actions[i] as InvokableAction<T1, T2, T3, T4>).Invoke(arg1, arg2, arg3, arg4);
                }
            }
        }

        /// <summary>
        /// Executes the event with four parameters.
        /// </summary>
        /// <param name="obj">The object that the event is attached to.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="arg1">The first parameter.</param>
        /// <param name="arg2">The second parameter.</param>
        /// <param name="arg3">The third parameter.</param>
        /// <param name="arg4">The fourth parameter.</param>
        public static void ExecuteEvent<T1, T2, T3, T4>(object obj, string eventName, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            var actions = GetActionList(obj, eventName);
            if (actions != null) {
                for (int i = actions.Count - 1; i >= 0; --i) {
                    (actions[i] as InvokableAction<T1, T2, T3, T4>).Invoke(arg1, arg2, arg3, arg4);
                }
            }
        }

        /// <summary>
        /// Executes the global event with five parameters.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="arg1">The first parameter.</param>
        /// <param name="arg2">The second parameter.</param>
        /// <param name="arg3">The third parameter.</param>
        /// <param name="arg4">The fourth parameter.</param>
        /// <param name="arg5">The fifth parameter.</param>
        public static void ExecuteEvent<T1, T2, T3, T4, T5>(string eventName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            var actions = GetActionList(eventName);
            if (actions != null) {
                for (int i = actions.Count - 1; i >= 0; --i) {
                    // TODO: version 2.1.5 changes the OnObjectImpact parameters.
                    var action = (actions[i] as InvokableAction<T1, T2, T3, T4, T5>);
                    if (action == null) {
                        continue;
                    }
                    action.Invoke(arg1, arg2, arg3, arg4, arg5);
                }
            }
        }

        /// <summary>
        /// Executes the event with five parameters.
        /// </summary>
        /// <param name="obj">The object that the event is attached to.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="arg1">The first parameter.</param>
        /// <param name="arg2">The second parameter.</param>
        /// <param name="arg3">The third parameter.</param>
        /// <param name="arg4">The fourth parameter.</param>
        /// <param name="arg5">The fifth parameter.</param>
        public static void ExecuteEvent<T1, T2, T3, T4, T5>(object obj, string eventName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            var actions = GetActionList(obj, eventName);
            if (actions != null) {
                for (int i = actions.Count - 1; i >= 0; --i) {
                    // TODO: version 2.1.5 changes the OnObjectImpact parameters.
                    var action = (actions[i] as InvokableAction<T1, T2, T3, T4, T5>);
                    if (action == null) {
                        continue;
                    }
                    action.Invoke(arg1, arg2, arg3, arg4, arg5);
                }
            }
        }

        /// <summary>
        /// Executes the global event with five parameters.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="arg1">The first parameter.</param>
        /// <param name="arg2">The second parameter.</param>
        /// <param name="arg3">The third parameter.</param>
        /// <param name="arg4">The fourth parameter.</param>
        /// <param name="arg5">The fifth parameter.</param>
        public static void ExecuteEvent<T1, T2, T3, T4, T5, T6>(string eventName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            var actions = GetActionList(eventName);
            if (actions != null) {
                for (int i = actions.Count - 1; i >= 0; --i) {
                    // TODO: version 2.1.5 changes the OnObjectImpact parameters.
                    var action = (actions[i] as InvokableAction<T1, T2, T3, T4, T5, T6>);
                    if (action == null) {
                        continue;
                    }
                    action.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
                }
            }
        }

        /// <summary>
        /// Executes the event with five parameters.
        /// </summary>
        /// <param name="obj">The object that the event is attached to.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="arg1">The first parameter.</param>
        /// <param name="arg2">The second parameter.</param>
        /// <param name="arg3">The third parameter.</param>
        /// <param name="arg4">The fourth parameter.</param>
        /// <param name="arg5">The fifth parameter.</param>
        public static void ExecuteEvent<T1, T2, T3, T4, T5, T6>(object obj, string eventName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            var actions = GetActionList(obj, eventName);
            if (actions != null) {
                for (int i = actions.Count - 1; i >= 0; --i) {
                    // TODO: version 2.1.5 changes the OnObjectImpact parameters.
                    var action = (actions[i] as InvokableAction<T1, T2, T3, T4, T5, T6>);
                    if (action == null) {
                        continue;
                    }
                    action.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
                }
            }
        }

        /// <summary>
        /// Unregisters the specified global event.
        /// </summary>
        /// <param name="obj">The object that the event is attached to.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The action delegate to remove.</param>
        public static void UnregisterEvent(string eventName, Action action)
        {
            var actions = GetActionList(eventName);
            if (actions != null) {
                for (int i = 0; i < actions.Count; ++i) {
                    var invokeableAction = (actions[i] as InvokableAction);
                    if (invokeableAction.IsAction(action)) {
                        ObjectPool.Return(invokeableAction);
                        actions.RemoveAt(i);
                        break;
                    }
                }
                CheckForEventRemoval(eventName, actions);
            }
        }

        /// <summary>
        /// Unregisters the specified event.
        /// </summary>
        /// <param name="obj">The object that the event is attached to.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The action to remove.</param>
        public static void UnregisterEvent(object obj, string eventName, Action action)
        {
            var actions = GetActionList(obj, eventName);
            if (actions != null) {
                for (int i = 0; i < actions.Count; ++i) {
                    var invokeableAction = (actions[i] as InvokableAction);
                    if (invokeableAction.IsAction(action)) {
                        ObjectPool.Return(invokeableAction);
                        actions.RemoveAt(i);
                        break;
                    }
                }
                CheckForEventRemoval(obj, eventName, actions);
            }
        }

        /// <summary>
        /// Unregisters the specified global event with one parameter.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The action to remove.</param>
        public static void UnregisterEvent<T1>(string eventName, Action<T1> action)
        {
            var actions = GetActionList(eventName);
            if (actions != null) {
                for (int i = 0; i < actions.Count; ++i) {
                    var invokeableAction = (actions[i] as InvokableAction<T1>);
                    if (invokeableAction.IsAction(action)) {
                        ObjectPool.Return(invokeableAction);
                        actions.RemoveAt(i);
                        break;
                    }
                }
                CheckForEventRemoval(eventName, actions);
            }
        }

        /// <summary>
        /// Unregisters the specified event with one parameter.
        /// </summary>
        /// <param name="obj">The object that the event is attached to.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The action to remove.</param>
        public static void UnregisterEvent<T1>(object obj, string eventName, Action<T1> action)
        {
            var actions = GetActionList(obj, eventName);
            if (actions != null) {
                for (int i = 0; i < actions.Count; ++i) {
                    var invokeableAction = (actions[i] as InvokableAction<T1>);
                    if (invokeableAction.IsAction(action)) {
                        ObjectPool.Return(invokeableAction);
                        actions.RemoveAt(i);
                        break;
                    }
                }
                CheckForEventRemoval(obj, eventName, actions);
            }
        }

        /// <summary>
        /// Unregisters the specified global event with two parameters.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The action to remove.</param>
        public static void UnregisterEvent<T1, T2>(string eventName, Action<T1, T2> action)
        {
            var actions = GetActionList(eventName);
            if (actions != null) {
                for (int i = 0; i < actions.Count; ++i) {
                    var invokeableAction = (actions[i] as InvokableAction<T1, T2>);
                    if (invokeableAction.IsAction(action)) {
                        ObjectPool.Return(invokeableAction);
                        actions.RemoveAt(i);
                        break;
                    }
                }
                CheckForEventRemoval(eventName, actions);
            }
        }

        /// <summary>
        /// Unregisters the specified event with two parameters.
        /// </summary>
        /// <param name="obj">The object that the event is attached to.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The action to remove.</param>
        public static void UnregisterEvent<T1, T2>(object obj, string eventName, Action<T1, T2> action)
        {
            var actions = GetActionList(obj, eventName);
            if (actions != null) {
                for (int i = 0; i < actions.Count; ++i) {
                    var invokeableAction = (actions[i] as InvokableAction<T1, T2>);
                    if (invokeableAction.IsAction(action)) {
                        ObjectPool.Return(invokeableAction);
                        actions.RemoveAt(i);
                        break;
                    }
                }
                CheckForEventRemoval(obj, eventName, actions);
            }
        }

        /// <summary>
        /// Unregisters the specified global event with three parameters.
        /// </summary>
        /// <param name="obj">The object that the event is attached to.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The action to remove.</param>
        public static void UnregisterEvent<T1, T2, T3>(string eventName, Action<T1, T2, T3> action)
        {
            var actions = GetActionList(eventName);
            if (actions != null) {
                for (int i = 0; i < actions.Count; ++i) {
                    var invokeableAction = (actions[i] as InvokableAction<T1, T2, T3>);
                    if (invokeableAction.IsAction(action)) {
                        ObjectPool.Return(invokeableAction);
                        actions.RemoveAt(i);
                        break;
                    }
                }
                CheckForEventRemoval(eventName, actions);
            }
        }

        /// <summary>
        /// Unregisters the specified event with three parameters.
        /// </summary>
        /// <param name="obj">The object that the event is attached to.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The action to remove.</param>
        public static void UnregisterEvent<T1, T2, T3>(object obj, string eventName, Action<T1, T2, T3> action)
        {
            var actions = GetActionList(obj, eventName);
            if (actions != null) {
                for (int i = 0; i < actions.Count; ++i) {
                    var invokeableAction = (actions[i] as InvokableAction<T1, T2, T3>);
                    if (invokeableAction.IsAction(action)) {
                        ObjectPool.Return(invokeableAction);
                        actions.RemoveAt(i);
                        break;
                    }
                }
                CheckForEventRemoval(obj, eventName, actions);
            }
        }

        /// <summary>
        /// Unregisters the specified global event with three parameters.
        /// </summary>
        /// <param name="obj">The object that the event is attached to.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The action to remove.</param>
        public static void UnregisterEvent<T1, T2, T3, T4>(string eventName, Action<T1, T2, T3, T4> action)
        {
            var actions = GetActionList(eventName);
            if (actions != null) {
                for (int i = 0; i < actions.Count; ++i) {
                    var invokeableAction = (actions[i] as InvokableAction<T1, T2, T3, T4>);
                    if (invokeableAction.IsAction(action)) {
                        ObjectPool.Return(invokeableAction);
                        actions.RemoveAt(i);
                        break;
                    }
                }
                CheckForEventRemoval(eventName, actions);
            }
        }

        /// <summary>
        /// Unregisters the specified event with four parameters.
        /// </summary>
        /// <param name="obj">The object that the event is attached to.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The action to remove.</param>
        public static void UnregisterEvent<T1, T2, T3, T4>(object obj, string eventName, Action<T1, T2, T3, T4> action)
        {
            var actions = GetActionList(obj, eventName);
            if (actions != null) {
                for (int i = 0; i < actions.Count; ++i) {
                    var invokeableAction = (actions[i] as InvokableAction<T1, T2, T3, T4>);
                    if (invokeableAction.IsAction(action)) {
                        ObjectPool.Return(invokeableAction);
                        actions.RemoveAt(i);
                        break;
                    }
                }
                CheckForEventRemoval(obj, eventName, actions);
            }
        }

        /// <summary>
        /// Unregisters the specified global event with five parameters.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The action to remove.</param>
        public static void UnregisterEvent<T1, T2, T3, T4, T5>(string eventName, Action<T1, T2, T3, T4, T5> action)
        {
            var actions = GetActionList(eventName);
            if (actions != null) {
                for (int i = 0; i < actions.Count; ++i) {
                    var invokeableAction = (actions[i] as InvokableAction<T1, T2, T3, T4, T5>);
                    if (invokeableAction.IsAction(action)) {
                        ObjectPool.Return(invokeableAction);
                        actions.RemoveAt(i);
                        break;
                    }
                }
                CheckForEventRemoval(eventName, actions);
            }
        }

        /// <summary>
        /// Unregisters the specified event with five parameters.
        /// </summary>
        /// <param name="obj">The object that the event is attached to.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The action to remove.</param>
        public static void UnregisterEvent<T1, T2, T3, T4, T5>(object obj, string eventName, Action<T1, T2, T3, T4, T5> action)
        {
            var actions = GetActionList(obj, eventName);
            if (actions != null) {
                for (int i = 0; i < actions.Count; ++i) {
                    var invokeableAction = (actions[i] as InvokableAction<T1, T2, T3, T4, T5>);
                    if (invokeableAction.IsAction(action)) {
                        ObjectPool.Return(invokeableAction);
                        actions.RemoveAt(i);
                        break;
                    }
                }
                CheckForEventRemoval(obj, eventName, actions);
            }
        }

        /// <summary>
        /// Unregisters the specified global event with six parameters.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The action to remove.</param>
        public static void UnregisterEvent<T1, T2, T3, T4, T5, T6>(string eventName, Action<T1, T2, T3, T4, T5, T6> action)
        {
            var actions = GetActionList(eventName);
            if (actions != null) {
                for (int i = 0; i < actions.Count; ++i) {
                    var invokeableAction = (actions[i] as InvokableAction<T1, T2, T3, T4, T5, T6>);
                    if (invokeableAction.IsAction(action)) {
                        ObjectPool.Return(invokeableAction);
                        actions.RemoveAt(i);
                        break;
                    }
                }
                CheckForEventRemoval(eventName, actions);
            }
        }

        /// <summary>
        /// Unregisters the specified event with six parameters.
        /// </summary>
        /// <param name="obj">The object that the event is attached to.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The action to remove.</param>
        public static void UnregisterEvent<T1, T2, T3, T4, T5, T6>(object obj, string eventName, Action<T1, T2, T3, T4, T5, T6> action)
        {
            var actions = GetActionList(obj, eventName);
            if (actions != null) {
                for (int i = 0; i < actions.Count; ++i) {
                    var invokeableAction = (actions[i] as InvokableAction<T1, T2, T3, T4, T5, T6>);
                    if (invokeableAction.IsAction(action)) {
                        ObjectPool.Return(invokeableAction);
                        actions.RemoveAt(i);
                        break;
                    }
                }
                CheckForEventRemoval(obj, eventName, actions);
            }
        }
    }
}