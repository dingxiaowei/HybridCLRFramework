/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Game
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using System.Collections.Generic;

    /// <summary>
    /// Determines which SpawnPoint to use in order to determine where the object should spawn.
    /// </summary>
    public class SpawnPointManager : MonoBehaviour
    {
        private static SpawnPointManager s_Instance;
        private static SpawnPointManager Instance
        {
            get
            {
                if (!s_Initialized) {
                    s_Instance = new GameObject("Spawn Point Manager").AddComponent<SpawnPointManager>();
                    s_Initialized = true;
                }
                return s_Instance;
            }
        }
        private static bool s_Initialized;

        [Tooltip("Checks the first spawn point before checking the other spawn points.")]
        [SerializeField] protected bool m_FirstSpawnPreferred;

        private Dictionary<int, List<SpawnPoint>> m_SpawnPointGroupings = new Dictionary<int, List<SpawnPoint>>();
        private Dictionary<int, SpawnPoint> m_FirstSpawnPoint = new Dictionary<int, SpawnPoint>();

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
        /// Adds the specified spawn point to the manager.
        /// </summary>
        /// <param name="spawnPoint">The spawn point to add.</param>
        public static void AddSpawnPoint(SpawnPoint spawnPoint)
        {
            Instance.AddSpawnPointInternal(spawnPoint);
        }

        /// <summary>
        /// Internal method which adds the specified spawn point to the manager.
        /// </summary>
        /// <param name="spawnPoint">The spawn point to add.</param>
        private void AddSpawnPointInternal(SpawnPoint spawnPoint)
        {
            AddSpawnPointGrouping(spawnPoint, spawnPoint.Grouping);
        }

        /// <summary>
        /// Adds the SpawnPoint to the specified grouping.
        /// </summary>
        /// <param name="spawnPoint">The SpawnPoint that should be added.</param>
        /// <param name="groupingIndex">The value of the grouping index.</param>
        private void AddSpawnPointGrouping(SpawnPoint spawnPoint, int groupingIndex)
        {
            List<SpawnPoint> spawnPoints;
            if (!m_SpawnPointGroupings.TryGetValue(groupingIndex, out spawnPoints)) {
                spawnPoints = new List<SpawnPoint>();
                m_SpawnPointGroupings.Add(groupingIndex, spawnPoints);
            }
            spawnPoints.Add(spawnPoint);

            if (!m_FirstSpawnPoint.ContainsKey(groupingIndex)) {
                m_FirstSpawnPoint.Add(groupingIndex, spawnPoint);
            }
        }

        /// <summary>
        /// The SpawnPoint's grouping value has changed. Update the internal group mapping.
        /// </summary>
        /// <param name="spawnPoint">The SpawnPoint whose grouping value changed.</param>
        /// <param name="newGroupingIndex">The new grouping index of the SpawnPoint.</param>
        public static void UpdateSpawnPointGrouping(SpawnPoint spawnPoint, int newGroupingIndex)
        {
            // If the manager isn't initialized yet then the grouping will be updated when the spawn point is added to the manager.
            if (!s_Initialized) {
                return;
            }

            Instance.UpdateSpawnPointGroupingInternal(spawnPoint, newGroupingIndex);
        }

        /// <summary>
        /// The SpawnPoint's grouping value has changed. Internal method which updates the internal group mapping.
        /// </summary>
        /// <param name="spawnPoint">The SpawnPoint whose grouping value changed.</param>
        /// <param name="newGroupingIndex">The new grouping value of the SpawnPoint.</param>
        private void UpdateSpawnPointGroupingInternal(SpawnPoint spawnPoint, int newGroupingIndex)
        {
            // Remove from the old grouping map.
            RemoveSpawnPoint(spawnPoint);

            // Add to the updated grouping map.
            AddSpawnPointGrouping(spawnPoint, newGroupingIndex);
        }

        /// <summary>
        /// Gets the position and rotation of the spawn point with the specified grouping.
        /// If false is returned then the point wasn't successfully retrieved.
        /// </summary>
        /// <param name="spawningObject">The object that is spawning.</param>
        /// <param name="grouping">The grouping index of spawn points to select from.</param>
        /// <param name="position">The position of the spawn point.</param>
        /// <param name="rotation">The rotation of the spawn point.</param>
        /// <returns>True if the spawn point was successfully retrieved.</returns>
        public static bool GetPlacement(GameObject spawningObject, int grouping, ref Vector3 position, ref Quaternion rotation)
        {
            return Instance.GetPlacementInternal(spawningObject, grouping, ref position, ref rotation);
        }

        /// <summary>
        /// Internal method which gets the position and rotation of the spawn point with the specified grouping.
        /// If false is returned then the point wasn't successfully retrieved.
        /// </summary>
        /// <param name="spawningObject">The object that is spawning.</param>
        /// <param name="grouping">The grouping index of spawn points to select from.</param>
        /// <param name="position">The position of the spawn point.</param>
        /// <param name="rotation">The rotation of the spawn point.</param>
        /// <returns>True if the spawn point was successfully retrieved.</returns>
        protected virtual bool GetPlacementInternal(GameObject spawningObject, int grouping, ref Vector3 position, ref Quaternion rotation)
        {
            List<SpawnPoint> spawnPoints;
            if (!m_SpawnPointGroupings.TryGetValue(grouping, out spawnPoints)) {
                Debug.LogError("Error: Unable to find a spawn point with the grouping index " + grouping);
                return false;
            }

            // Optionally try to spawn in the first spawn point.
            SpawnPoint firstSpawnPoint;
            if (m_FirstSpawnPreferred && m_FirstSpawnPoint.TryGetValue(grouping, out firstSpawnPoint)) {
                if (firstSpawnPoint.GetPlacement(spawningObject, ref position, ref rotation)) {
                    return true;
                }
            }

            // Choose a random spawn point and get the spawn placement.
            ShuffleSpawnPoints(spawnPoints);
            var attempt = 0;
            while (attempt < spawnPoints.Count) {
                if (spawnPoints[attempt].GetPlacement(spawningObject, ref position, ref rotation)) {
                    return true;
                }
                attempt++;
            }
            return false;
        }

        /// <summary>
        /// Shuffles the spawn points list.
        /// </summary>
        /// <param name="spawnPoints">The list that should be shuffled.</param>
        private void ShuffleSpawnPoints(List<SpawnPoint> spawnPoints)
        {
            var n = spawnPoints.Count - 1;
            while (n > 1) {
                var k = Random.Range(0, n);
                var temp = spawnPoints[n];
                spawnPoints[n] = spawnPoints[k];
                spawnPoints[k] = temp;
                n--;
            }
        }

        /// <summary>
        /// Returns the list of spawn points that belong to the specified grouping.
        /// </summary>
        /// <param name="grouping">The grouping of spawn points that should be retrieved.</param>
        /// <returns>The list of spawn points that belong to the specifeid grouping.</returns>
        public static List<SpawnPoint> GetSpawnPoints(int grouping)
        {
            return Instance.GetSpawnPointsInternal(grouping);
        }

        /// <summary>
        /// Internal method which returns the list of spawn points that belong to the specified grouping.
        /// </summary>
        /// <param name="grouping">The grouping of spawn points that should be retrieved.</param>
        /// <returns>The list of spawn points that belong to the specifeid grouping.</returns>
        private List<SpawnPoint> GetSpawnPointsInternal(int grouping)
        {
            List<SpawnPoint> spawnPoints;
            if (!m_SpawnPointGroupings.TryGetValue(grouping, out spawnPoints)) {
                Debug.LogError("Error: Unable to find a spawn point with the grouping index " + grouping);
            }
            return spawnPoints;
        }

        /// <summary>
        /// Removes the specified spawn point from the manager.
        /// </summary>
        /// <param name="spawnPoint">The spawn point to remove.</param>
        public static void RemoveSpawnPoint(SpawnPoint spawnPoint)
        {
            Instance.RemoveSpawnPointInternal(spawnPoint);
        }

        /// <summary>
        /// Internal method which removes the specified spawn point from the manager.
        /// </summary>
        /// <param name="spawnPoint">The spawn point to remove.</param>
        private void RemoveSpawnPointInternal(SpawnPoint spawnPoint)
        {
            List<SpawnPoint> spawnPoints;
            if (m_SpawnPointGroupings.TryGetValue(spawnPoint.Grouping, out spawnPoints)) {
                spawnPoints.Remove(spawnPoint);

                SpawnPoint firstSpawnPoint;
                if (m_FirstSpawnPoint.TryGetValue(spawnPoint.Grouping, out firstSpawnPoint)) {
                    // Update the first spawn point with the new first spawn point element.
                    if (spawnPoints.Count > 0) {
                        m_FirstSpawnPoint[spawnPoint.Grouping] = spawnPoints[0];
                    } else {
                        m_FirstSpawnPoint.Remove(spawnPoint.Grouping);
                    }
                }
            }
        }

        /// <summary>
        /// The object has been disabled.
        /// </summary>
        private void OnDisable()
        {
            SceneManager.sceneUnloaded += SceneUnloaded;
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