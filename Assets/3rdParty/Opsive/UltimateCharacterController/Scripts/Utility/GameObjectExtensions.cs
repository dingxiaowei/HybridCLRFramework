/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Utility
{
    /// <summary>
    /// Extension methods for the UnityEngine.GameObject class.
    /// </summary>
    public static class GameObjectExtensions
    {
        private static Dictionary<GameObject, Dictionary<Type, object>> s_GameObjectComponentMap = new Dictionary<GameObject, Dictionary<Type, object>>();
        private static Dictionary<GameObject, Dictionary<Type, object>> s_GameObjectParentComponentMap = new Dictionary<GameObject, Dictionary<Type, object>>();
        private static Dictionary<GameObject, Dictionary<Type, object>> s_GameObjectInactiveParentComponentMap = new Dictionary<GameObject, Dictionary<Type, object>>();
        private static Dictionary<GameObject, Dictionary<Type, object[]>> s_GameObjectComponentsMap = new Dictionary<GameObject, Dictionary<Type, object[]>>();
        private static Dictionary<GameObject, Dictionary<Type, object[]>> s_GameObjectParentComponentsMap = new Dictionary<GameObject, Dictionary<Type, object[]>>();

        /// <summary>
        /// Returns a cached component reference for the specified type.
        /// </summary>
        /// <param name="gameObject">The GameObject (or child GameObject) to get the component reference of.</param>
        /// <param name="type">The type of component to get.</param>
        /// <returns>The cached component reference.</returns>
        public static T GetCachedComponent<T>(this GameObject gameObject)
        {
            Dictionary<Type, object> typeComponentMap;
            // Return the cached component if it exists.
            if (s_GameObjectComponentMap.TryGetValue(gameObject, out typeComponentMap)) {
                object targetObject;
                if (typeComponentMap.TryGetValue(typeof(T), out targetObject)) {
                    return (T)targetObject;
                }
            } else {
                // The cached component doesn't exist for the specified type.
                typeComponentMap = new Dictionary<Type, object>();
                s_GameObjectComponentMap.Add(gameObject, typeComponentMap);
            }

            // Find the component reference and cache the results.
            var targetComponent = gameObject.GetComponent<T>();
            typeComponentMap.Add(typeof(T), targetComponent);
            return targetComponent;
        }

        /// <summary>
        /// Returns a cached parent component reference for the specified type.
        /// </summary>
        /// <param name="gameObject">The GameObject (or child GameObject) to get the component reference of.</param>
        /// <param name="type">The type of component to get.</param>
        /// <returns>The cached component reference.</returns>
        public static T GetCachedParentComponent<T>(this GameObject gameObject)
        {
            Dictionary<Type, object> typeComponentMap;
            // Return the cached component if it exists.
            if (s_GameObjectParentComponentMap.TryGetValue(gameObject, out typeComponentMap)) {
                object targetObject;
                if (typeComponentMap.TryGetValue(typeof(T), out targetObject)) {
                    return (T)targetObject;
                }
            } else {
                // The cached component doesn't exist for the specified type.
                typeComponentMap = new Dictionary<Type, object>();
                s_GameObjectParentComponentMap.Add(gameObject, typeComponentMap);
            }

            // Find the component reference and cache the results.
            var targetComponent = gameObject.GetComponentInParent<T>();
            typeComponentMap.Add(typeof(T), targetComponent);
            return targetComponent;
        }

        /// <summary>
        /// Returns a cached component references for the specified type.
        /// </summary>
        /// <param name="gameObject">The GameObject (or child GameObject) to get the component reference of.</param>
        /// <param name="type">The type of component to get.</param>
        /// <returns>The cached component references.</returns>
        public static T[] GetCachedComponents<T>(this GameObject gameObject)
        {
            Dictionary<Type, object[]> typeComponentMap;
            // Return the cached component if it exists.
            if (s_GameObjectComponentsMap.TryGetValue(gameObject, out typeComponentMap)) {
                object[] targetObject;
                if (typeComponentMap.TryGetValue(typeof(T), out targetObject)) {
                    return targetObject as T[];
                }
            } else {
                // The cached component doesn't exist for the specified type.
                typeComponentMap = new Dictionary<Type, object[]>();
                s_GameObjectComponentsMap.Add(gameObject, typeComponentMap);
            }

            // Find the component references and cache the results.
            var targetComponents = gameObject.GetComponents<T>() as T[];
            typeComponentMap.Add(typeof(T), targetComponents as object[]);
            return targetComponents;
        }

        /// <summary>
        /// Returns a cached component references for the specified type.
        /// </summary>
        /// <param name="gameObject">The GameObject (or child GameObject) to get the component reference of.</param>
        /// <param name="type">The type of component to get.</param>
        /// <returns>The cached component references.</returns>
        public static T[] GetCachedParentComponents<T>(this GameObject gameObject)
        {
            Dictionary<Type, object[]> typeComponentMap;
            // Return the cached component if it exists.
            if (s_GameObjectParentComponentsMap.TryGetValue(gameObject, out typeComponentMap)) {
                object[] targetObject;
                if (typeComponentMap.TryGetValue(typeof(T), out targetObject)) {
                    return targetObject as T[];
                }
            } else {
                // The cached component doesn't exist for the specified type.
                typeComponentMap = new Dictionary<Type, object[]>();
                s_GameObjectParentComponentsMap.Add(gameObject, typeComponentMap);
            }

            // Find the component references and cache the results.
            var targetComponents = gameObject.GetComponentsInParent<T>() as T[];
            typeComponentMap.Add(typeof(T), targetComponents as object[]);
            return targetComponents;
        }

        /// <summary>
        /// Finds the parent component even on a disabled GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to get the parent reference of.</param>
        /// <returns>The found component (can be null).</returns>
        public static T GetCachedInactiveComponentInParent<T>(this GameObject gameObject) where T : Component
        {
            Dictionary<Type, object> typeComponentMap;
            // Return the cached component if it exists.
            if (s_GameObjectInactiveParentComponentMap.TryGetValue(gameObject, out typeComponentMap)) {
                object targetObject;
                if (typeComponentMap.TryGetValue(typeof(T), out targetObject)) {
                    return (T)targetObject;
                }
            } else {
                // The cached component doesn't exist for the specified type.
                typeComponentMap = new Dictionary<Type, object>();
                s_GameObjectInactiveParentComponentMap.Add(gameObject, typeComponentMap);
            }

            T foundComponent = null;
            var parent = gameObject.transform;
            while (parent != null) {
                if ((foundComponent = parent.GetComponent<T>()) != null) {
                    // The component was found.
                    break;
                }
                parent = parent.parent;
            }
            typeComponentMap.Add(typeof(T), foundComponent);
            return foundComponent;
        }
    }
}