/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Utility;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.SurfaceSystem
{
    /// <summary>
    /// The DecalManager is responsible for managing the spawned decals. The decals can be capped at a limit to prevent too many from being
    /// spawned. These decals can then slowly be faded (weathered) for a smooth transition rather than the decal just popping out of existance.
    /// </summary>
    public class DecalManager : MonoBehaviour
    {
        private static DecalManager s_Instance;
        private static DecalManager Instance
        {
            get
            {
                if (!s_Initialized) {
                    s_Instance = new GameObject("Decal Manager").AddComponent<DecalManager>();
                    s_Initialized = true;
                }
                return s_Instance;
            }
        }
        private static bool s_Initialized;

        [Tooltip("The maximum number of decals.")]
        [SerializeField] protected int m_DecalLimit = 100;
        [Tooltip("The number of decals which should slowly fade after the decal limit has been reached.")]
        [SerializeField] protected int m_WeatheredDecalLimit = 20;
        [Tooltip("The speed that the decals should fadeout after they have been removed from the weathered array.")]
        [SerializeField] protected int m_RemoveFadeoutSpeed = 10;

        public int DecalLimit { get { return m_DecalLimit; } set { m_DecalLimit = value; } }
        public int WeatheredDecalLimit { get { return m_WeatheredDecalLimit; } set { m_WeatheredDecalLimit = value; } }
        public int RemoveFadeoutSpeed { get { return m_RemoveFadeoutSpeed; } set { m_RemoveFadeoutSpeed = value; } }

        private List<GameObject> m_Decals = new List<GameObject>();
        private List<Renderer> m_WeatheredDecals = new List<Renderer>();
        private List<Renderer> m_DecalsToFade = new List<Renderer>();
        private Dictionary<GameObject, Renderer> m_DecalRendererMap = new Dictionary<GameObject, Renderer>();
        private Dictionary<GameObject, Mesh> m_DecalMeshMap = new Dictionary<GameObject, Mesh>();

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
        /// Instantiates a new decal.
        /// </summary>
        /// <param name="original">The original prefab to spawn an instance of.</param>
        /// <param name="hit">The RaycastHit which caused the decal to spawn.</param>
        /// <param name="scale">The scale of the decal to spawn.</param>
        /// <param name="allowedEdgeOverlap">How close to the edge the decal is allowed to spawn.</param>
        public static void Spawn(GameObject original, RaycastHit hit, float scale, float allowedEdgeOverlap)
        {
            Instance.SpawnInternal(original, hit, scale, allowedEdgeOverlap);
        }

        /// <summary>
        /// Internal method which instantiates a new decal.
        /// </summary>
        /// <param name="original">The original prefab to spawn an instance of.</param>
        /// <param name="hit">The RaycastHit which caused the decal to spawn.</param>
        /// <param name="scale">The scale of the decal to spawn.</param>
        /// <param name="allowedEdgeOverlap">How close to the edge the decal is allowed to spawn.</param>
        private void SpawnInternal(GameObject original, RaycastHit hit, float scale, float allowedEdgeOverlap)
        {
            SpawnDecal(original, hit, Quaternion.LookRotation(hit.normal) * Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward), scale, allowedEdgeOverlap);
        }

        /// <summary>
        /// Instantiates a new footprint.
        /// </summary>
        /// <param name="original">The original prefab to spawn an instance of.</param>
        /// <param name="hit">The RaycastHit which caused the footprint to spawn.</param>
        /// <param name="scale">The scale of the decal to spawn.</param>
        /// <param name="allowedEdgeOverlap">How close to the edge the footprint is allowed to spawn.</param>
        /// <param name="footprintDirection">The direction that the footprint decal should face.</param>
        /// <param name="flipFootprint">Should the footprint decal be flipped?</param>
        public static void SpawnFootprint(GameObject original, RaycastHit hit, float scale, float allowedEdgeOverlap, Vector3 footprintDirection, bool flipFootprint)
        {
            Instance.SpawnFootprintInternal(original, hit, scale, allowedEdgeOverlap, footprintDirection, flipFootprint);
        }

        /// <summary>
        /// Internal method which instantiates a new footprint.
        /// </summary>
        /// <param name="original">The original prefab to spawn an instance of.</param>
        /// <param name="hit">The RaycastHit which caused the footprint to spawn.</param>
        /// <param name="scale">The scale of the decal to spawn.</param>
        /// <param name="allowedEdgeOverlap">How close to the edge the footprint is allowed to spawn.</param>
        /// <param name="footprintDirection">The direction that the footprint decal should face.</param>
        /// <param name="flipFootprint">Should the footprint decal be flipped?</param>
        private void SpawnFootprintInternal(GameObject original, RaycastHit hit, float scale, float allowedEdgeOverlap, Vector3 footprintDirection, bool flipFootprint)
        {
            var decal = SpawnDecal(original, hit, Quaternion.LookRotation(hit.normal, footprintDirection), scale, allowedEdgeOverlap);
            // Changing the local x axis will flip the footprint.
            if (decal != null && flipFootprint) {
                var localScale = decal.transform.localScale;
                localScale.x *= -1;
                decal.transform.localScale = localScale;
            }
        }

        /// <summary>
        /// Instantiates a new decal.
        /// </summary>
        /// <param name="original">The original prefab to spawn an instance of.</param>
        /// <param name="hit">The RaycastHit which caused the footprint to spawn.</param>
        /// <param name="rotation">The rotation of the decal which should be spawned.</param>
        /// <param name="scale">The scale of the decal to spawn.</param>
        /// <param name="allowedEdgeOverlap">How close to the edge the footprint is allowed to spawn.</param>
        /// <returns>The spawned decal. Can be null.</returns>
        private GameObject SpawnDecal(GameObject original, RaycastHit hit, Quaternion rotation, float scale, float allowedEdgeOverlap)
        {
            // Prevent z fighting by slightly raising the decal off of the surface. 
            var decal = ObjectPool.Instantiate(original, hit.point + (hit.normal * 0.001f), rotation);
            // Only set the decal parent to the hit transform on uniform objects to prevent stretching.
            if (MathUtility.IsUniform(hit.transform.localScale)) {
                decal.transform.parent = hit.transform;
            }
            if (scale != 1) {
                var vectorScale = Vector3.one;
                vectorScale.x = vectorScale.y = scale;
                decal.transform.localScale = Vector3.Scale(decal.transform.localScale, vectorScale);
            }

            // Destroy the object if it cannot be cached. The object won't be able to be cached if it doesn't have all of the required components.
            if (!CacheMeshAndRenderer(decal)) {
                ObjectPool.Destroy(decal);
                return null;
            }

            // Do a test on the decal's quad to ensure all four corners are flush against a surface. This will prevent the decal from sticking out on an edge.
            if (allowedEdgeOverlap < 0.5f) {
                if (!DoQuadTest(decal, allowedEdgeOverlap)) {
                    ObjectPool.Destroy(decal);
                    return null;
                }
            }

            // The decal can be added.
            Add(decal);

            return decal;
        }

        /// <summary>
        /// Stores the decal's mesh and renderer.
        /// </summary>
        /// <param name="decal">The decal to store the mesh and renderer of.</param>
        /// <returns>True if the mesh and renderer were able to be cached.</returns>
        private bool CacheMeshAndRenderer(GameObject decal)
        {
            Renderer renderer;
            if (!m_DecalRendererMap.TryGetValue(decal, out renderer)) {
                var meshFilter = decal.GetComponent<MeshFilter>();
                if (meshFilter == null) {
                    return false;
                }
                if (meshFilter.mesh == null) {
                    return false;
                }

                renderer = decal.GetComponent<Renderer>();
                if (renderer == null) {
                    return false;
                }
                if (renderer.material == null) {
                    return false;
                }

                // Cache the decal renderer and mesh.
                m_DecalRendererMap.Add(decal, renderer);
                m_DecalMeshMap.Add(decal, meshFilter.mesh);
            }

            // The decal should start opaque.
            var color = renderer.material.color;
            color.a = 1;
            renderer.material.color = color;

            return true;
        }

        /// <summary>
        /// Check all four corners of the decal for surface contact.
        /// </summary>
        /// <param name="decal">The decal to check the corners of.</param>
        /// <param name="allowedEdgeOverlap">How close to the edge the decal is allowed to spawn.</param>
        /// <returns>True if all four corners are flush against a surface.</returns>
        private bool DoQuadTest(GameObject decal, float allowedEdgeOverlap)
        {
            Mesh mesh;
            if (!m_DecalMeshMap.TryGetValue(decal, out mesh)) {
                return false;
            }

            RaycastHit hit;
            for (int i = 0; i < 4; i++) {
                // The decal isn't hitting anything if the raycast returns false.
                if (!Physics.Raycast(decal.transform.TransformPoint(mesh.vertices[i] * (1 - (allowedEdgeOverlap * 2))) + (decal.transform.forward * 0.1f), 
                    -decal.transform.forward, out hit, 0.2f, ~((1 << LayerManager.TransparentFX) | (1 << LayerManager.IgnoreRaycast) | 
                                                               (1 << LayerManager.VisualEffect) | (1 << LayerManager.Water)), QueryTriggerInteraction.Ignore)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Adds the decal to the active decal stack.
        /// </summary>
        /// <param name="decal">The decal to add.</param>
        private void Add(GameObject decal)
        {
            m_Decals.Add(decal);
            // If the total decal count is greater than the specified limit then the oldest decal should start to be weathered.
            if (m_Decals.Count >= m_DecalLimit) {
                var oldestDecalRenderer = m_DecalRendererMap[m_Decals[0]];
                m_WeatheredDecals.Add(oldestDecalRenderer);
                m_Decals.RemoveAt(0);
                WeatherDecals();
            }
        }

        /// <summary>
        /// Slowly fade out the oldest decal in the weathered list.
        /// </summary>
        private void WeatherDecals()
        {
            // As each decal is added to the weathered list it should slowly fade out.
            for (int i = 0; i < m_WeatheredDecals.Count; ++i) {
                var color = m_WeatheredDecals[i].material.color;
                color.a = Mathf.Clamp01(color.a - (1 / (float)m_WeatheredDecalLimit));
                m_WeatheredDecals[i].material.color = color;
            }

            // Remove the oldest weathered decal if the limit is reached. This decal will be added to the fade list.
            if (m_WeatheredDecals.Count >= m_WeatheredDecalLimit) {
                m_DecalsToFade.Add(m_WeatheredDecals[0]);
                m_WeatheredDecals.RemoveAt(0);
                enabled = true;
            }
        }

        /// <summary>
        /// Fade out the decals in the decals to fade list.
        /// </summary>
        private void Update()
        {
            for (int i = m_DecalsToFade.Count - 1; i >= 0; --i) {
                var color = m_DecalsToFade[i].material.color;
                color.a = Mathf.Lerp(color.a, 0, Time.deltaTime * m_RemoveFadeoutSpeed);
                // The decal can be removed from the list when it is completely faded out.
                if (color.a == 0) {
                    ObjectPool.Destroy(m_DecalsToFade[i].gameObject);
                    m_DecalsToFade.RemoveAt(i);
                } else {
                    m_DecalsToFade[i].material.color = color;
                }
            }
            // The component can be disabled when there are no decals within the list.
            if (m_DecalsToFade.Count == 0) {
                enabled = false;
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
    }
}