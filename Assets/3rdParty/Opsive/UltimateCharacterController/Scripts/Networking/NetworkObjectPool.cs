/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Networking.Game
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Bridge component between the networking spawning system and the ObjectPool.
    /// </summary>
    public abstract class NetworkObjectPool : MonoBehaviour
    {
        private static NetworkObjectPool s_Instance;
        private static NetworkObjectPool Instance { get { return s_Instance; } }

        /// <summary>
        /// Does the NetworkObjectPool exist?
        /// </summary>
        /// <returns>True if the NetworkObjectPool exists.</returns>
        public static bool IsNetworkActive()
        {
            return s_Instance != null;
        }

        /// <summary>
        /// The object has been enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            // The object may have been enabled outside of the scene unloading.
            if (s_Instance == null) {
                s_Instance = this;
                SceneManager.sceneUnloaded -= SceneUnloaded;
            }
        }

        /// <summary>
        /// Spawns the object over the network. This does not instantiate a new object on the local client.
        /// </summary>
        /// <param name="original">The object that the object was instantiated from.</param>
        /// <param name="instanceObject">The object that was instantiated from the original object.</param>
        /// <param name="sceneObject">Is the object owned by the scene? If fales it will be owned by the character.</param>
        public static void NetworkSpawn(GameObject original, GameObject instanceObject, bool sceneObject)
        {
            if (s_Instance == null) {
                Debug.LogError("Error: Unable to spawn object - the Network Object Pool doesn't exist.");
                return;
            }
            s_Instance.NetworkSpawnInternal(original, instanceObject, sceneObject);
        }

        /// <summary>
        /// Internal method which spawns the object over the network. This does not instantiate a new object on the local client.
        /// </summary>
        /// <param name="original">The object that the object was instantiated from.</param>
        /// <param name="instanceObject">The object that was instantiated from the original object.</param>
        /// <param name="sceneObject">Is the object owned by the scene? If fales it will be owned by the character.</param>
        protected abstract void NetworkSpawnInternal(GameObject original, GameObject instanceObject, bool sceneObject);

        /// <summary>
        /// Destroys the object instance on the network.
        /// </summary>
        /// <param name="obj">The object to destroy.</param>
        public static void Destroy(GameObject obj)
        {
            if (s_Instance == null) {
                Debug.LogError("Error: Unable to destroy object - the Network Object Pool doesn't exist.");
                return;
            }
            s_Instance.DestroyInternal(obj);
        }

        /// <summary>
        /// Internal method which destroys the object instance on the network.
        /// </summary>
        /// <param name="obj">The object to destroy.</param>
        protected abstract void DestroyInternal(GameObject obj);

        /// <summary>
        /// Returns if the specified object was spawned with the network object pool.
        /// </summary>
        /// <param name="obj">The object instance to determine if was spawned with the object pool.</param>
        /// <returns>True if the object was spawned with the network object pool.</returns>
        public static bool SpawnedWithPool(GameObject obj)
        {
            if (s_Instance == null) {
                return false;
            }
            return s_Instance.SpawnedWithPoolInternal(obj);
        }

        /// <summary>
        /// Internal method which returns if the specified object was spawned with the network object pool.
        /// </summary>
        /// <param name="obj">The object instance to determine if was spawned with the object pool.</param>
        /// <returns>True if the object was spawned with the network object pool.</returns>
        protected abstract bool SpawnedWithPoolInternal(GameObject obj);

        /// <summary>
        /// Reset the initialized variable when the scene is no longer loaded.
        /// </summary>
        /// <param name="scene">The scene that was unloaded.</param>
        private void SceneUnloaded(Scene scene)
        {
            s_Instance = null;
            SceneManager.sceneUnloaded -= SceneUnloaded;
        }

        /// <summary>
        /// The object has been disabled.
        /// </summary>
        protected virtual void OnDisable()
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
            s_Instance = null;
        }
#endif
    }
}