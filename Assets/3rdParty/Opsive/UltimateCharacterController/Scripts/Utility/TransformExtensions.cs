/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Utility
{
    /// <summary>
    /// Extension methods for the UnityEngine.Transform class.
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// Sets the parent of the transform object to the specified parent.
        /// </summary>
        /// <param name="transform">The transform to set the parent of.</param>
        /// <param name="parent">The parent of the transform.</param>
        public static void SetParentOrigin(this Transform transform, Transform parent)
        {
            transform.parent = parent;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Recursively sets the layer on all of the children.
        /// </summary>
        /// <param name="transform">The transform to set the layer on.</param>
        /// <param name="layer">The layer to set.</param>
        public static void SetLayerRecursively(this Transform transform, int layer)
        {
            transform.gameObject.layer = layer;
            for (int i = 0; i < transform.childCount; ++i) {
                transform.GetChild(i).SetLayerRecursively(layer);
            }
        }

        /// <summary>
        /// Returns the component of the specified type in the GameObject or any of its parents.
        /// </summary>
        /// <param name="transform">The transform to get the component on.</param>
        /// <typeparam name="T">The type of component to return.</typeparam>
        /// <returns>THe component of the specified type in the GameObject or any of its parents. Can be null.</returns>
        public static T GetComponentInParentIncludeInactive<T>(this Transform transform) where T : Component
        {
            var parent = transform;
            T component;
            while (parent != null) {
                if ((component = parent.GetComponent<T>()) != null) {
                    return component;
                }
                parent = parent.parent;
            }
            return null;
        }
    }
}