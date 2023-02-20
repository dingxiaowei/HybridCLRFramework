/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Game
{
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

        private List<SpawnPoint> m_SpawnPoints = new List<SpawnPoint>();
        private Dictionary<int, List<SpawnPoint>> m_SpawnPointGroupings = new Dictionary<int, List<SpawnPoint>>();

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
            m_SpawnPoints.Add(spawnPoint);
            if (spawnPoint.Grouping != -1) {
                AddSpawnPointGrouping(spawnPoint, spawnPoint.Grouping);
            }
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
        }

        /// <summary>
        /// The SpawnPoint's grouping value has changed. Update the internal group mapping.
        /// </summary>
        /// <param name="spawnPoint">The SpawnPoint whose grouping value changed.</param>
        /// <param name="newGroupingIndex">The new grouping index of the SpawnPoint.</param>
        public static void UpdateSpawnPointGrouping(SpawnPoint spawnPoint, int newGroupingIndex)
        {
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
            if (spawnPoint.Grouping != -1) {
                List<SpawnPoint> spawnPoints;
                if (m_SpawnPointGroupings.TryGetValue(spawnPoint.Grouping, out spawnPoints)) {
                    for (int i = 0; i < spawnPoints.Count; ++i) {
                        if (spawnPoints[i] == spawnPoint) {
                            spawnPoints.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            // Add to the updated grouping map.
            if (newGroupingIndex != -1) {
                AddSpawnPointGrouping(spawnPoint, newGroupingIndex);
            }
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
            if (grouping != -1) {
                if (!m_SpawnPointGroupings.TryGetValue(grouping, out spawnPoints)) {
                    Debug.LogError("Error: Unable to find a spawn point with the grouping index " + grouping);
                    return false;
                }
            } else {
                spawnPoints = m_SpawnPoints;
            }

            // Optionally try to spawn in the first spawn point.
            var firstSpawnIndex = 0;
            if (m_FirstSpawnPreferred && spawnPoints.Count > 1) {
                if (spawnPoints[0].GetPlacement(spawningObject, ref position, ref rotation)) {
                    return true;
                }

                firstSpawnIndex = 1;
            }

            // Choose a random spawn point and get the spawn placement.
            var attempt = 0;
            while (attempt < spawnPoints.Count) {
                var spawnPoint = spawnPoints[Random.Range(firstSpawnIndex, spawnPoints.Count - 1)];
                if (spawnPoint.GetPlacement(spawningObject, ref position, ref rotation)) {
                    return true;
                }
                attempt++;
            }
            return false;
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
            m_SpawnPoints.Remove(spawnPoint);
            if (spawnPoint.Grouping != -1) {
                List<SpawnPoint> spawnPoints;
                if (m_SpawnPointGroupings.TryGetValue(spawnPoint.Grouping, out spawnPoints)) {
                    spawnPoints.Remove(spawnPoint);
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
    }
}