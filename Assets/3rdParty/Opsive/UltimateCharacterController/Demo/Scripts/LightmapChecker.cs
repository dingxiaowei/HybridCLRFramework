/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Demo
{
    /// <summary>
    /// The scene will run slowly if the lightmaps aren't baked with the lights active. If the lightmaps are not baked then disable the lights
    /// so the scene will run at a normal speed.
    /// </summary>
    public class LightmapChecker : MonoBehaviour
    {
        /// <summary>
        /// Determine if there are any lightmaps.
        /// </summary>
        private void Start()
        {
            if (LightmapSettings.lightmaps == null || LightmapSettings.lightmaps.Length == 0) {
                var lights = GetComponentsInChildren<Light>();
                for (int i = 0; i < lights.Length; ++i) {
                    if (lights[i].type != LightType.Directional) {
                        lights[i].gameObject.SetActive(false);
                    }
                }
            } else {
                Destroy(this);
            }
        }
    }
}