/// ---------------------------------------------
/// Dypsloom Shared Utilities
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.Shared.Utility
{
    using UnityEngine;

    /// <summary>
    /// An extension that checks if a layer mask contains a layer.
    /// </summary>
    public static class LayerMaskExtension
    {
        /// <summary>
        /// Check if the layer mask contains a layer.
        /// </summary>
        /// <param name="mask">The mask.</param>
        /// <param name="layer">The layer.</param>
        /// <returns>True if the layer is contained.</returns>
        public static bool Contains(this LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }
        
        /// <summary>
        /// Check if the layer mask contains the game objects layer.
        /// </summary>
        /// <param name="mask">The layer mask.</param>
        /// <param name="collider">The Component.</param>
        /// <returns>True if the game object is contained.</returns>
        public static bool Contains(this LayerMask mask, Collider collider)
        {
            if (collider == null) { return false;}

            if (collider.attachedRigidbody != null) {
                return mask == (mask | (1 << collider.attachedRigidbody.gameObject.layer));
            }
            
            return mask == (mask | (1 << collider.gameObject.layer));
        }
        
        /// <summary>
        /// Check if the layer mask contains the game objects layer.
        /// </summary>
        /// <param name="mask">The layer mask.</param>
        /// <param name="component">The Component.</param>
        /// <returns>True if the game object is contained.</returns>
        public static bool Contains(this LayerMask mask, Component component)
        {
            if (component == null) { return false;}
            return mask == (mask | (1 << component.gameObject.layer));
        }
        
        /// <summary>
        /// Check if the layer mask contains the game objects layer.
        /// </summary>
        /// <param name="mask">The layer mask.</param>
        /// <param name="go">The game object.</param>
        /// <returns>True if the game object is contained.</returns>
        public static bool Contains(this LayerMask mask, GameObject go)
        {
            if (go == null) { return false;}
            return mask == (mask | (1 << go.layer));
        }
    }
}