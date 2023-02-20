/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Demo
{
    /// <summary>
    /// Destroys the GameObject when the game starts.
    /// </summary>
    public class DestroyOnStart : MonoBehaviour
    {
        /// <summary>
        /// Destroys the GameObject.
        /// </summary>
        private void Start()
        {
            Destroy(gameObject);
        }
    }
}