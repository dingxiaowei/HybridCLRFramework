/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.SurfaceSystem
{
    using Opsive.Shared.Game;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// The SurfaceManager is responsible for determining which SurfaceEffect to spawn based on the speicifed RaycastHit.
    /// </summary>
    public class SurfaceManager : MonoBehaviour
    {
        private static SurfaceManager s_Instance;
        private static SurfaceManager Instance
        {
            get
            {
                if (!s_Initialized) {
                    s_Instance = new GameObject("Surface Manager").AddComponent<SurfaceManager>();
                    s_Initialized = true;
                }
                return s_Instance;
            }
        }
        private static bool s_Initialized;
        private static int s_MaskID;
        private static int s_SecondaryTextureID;

        [Tooltip("An array of SurfaceTypes which are paired to a UV position within a texture.")]
        [SerializeField] protected ObjectSurface[] m_ObjectSurfaces;
        [Tooltip("Should the textures from trees on the terrain be detected? Note that this is a CPU-intensive operation.")]
        [SerializeField] protected bool m_DetectTerrainTreeTextures;
        [Tooltip("The fallback SurfaceImpact if no SurfaceImpacts can be found.")]
        [SerializeField] protected SurfaceImpact m_FallbackSurfaceImpact;
        [Tooltip("The fallback SurfaceType if no SurfaceTypes can be found.")]
        [SerializeField] protected SurfaceType m_FallbackSurfaceType;
        [Tooltip("The fallback allow decals if using a fallback SurfaceImpact or SurfaceTYpe.")]
        [SerializeField] protected bool m_FallbackAllowDecals = true;

        public ObjectSurface[] ObjectSurfaces { get { return m_ObjectSurfaces; } }
        public bool DetectTerrainTreeTextures { get { return m_DetectTerrainTreeTextures; } }
        public SurfaceImpact FallbackSurfaceImpact { get { return m_FallbackSurfaceImpact; } }
        public SurfaceType FallbackSurfaceType { get { return m_FallbackSurfaceType; } }
        public bool FallbackAllowDecals { get { return m_FallbackAllowDecals; } }

        private bool m_HasTerrain;
        private Rect m_DefaultUV = new Rect(0, 0, 1, 1);
        private int[] m_MaterialHitTriangle = new int[3];
        private Dictionary<Texture, ObjectSurface> m_TextureObjectSurfaceMap = new Dictionary<Texture, ObjectSurface>();
        private Dictionary<UVTexture, ObjectSurface> m_UVTextureObjectSurfaceMap = new Dictionary<UVTexture, ObjectSurface>();
        private Dictionary<Texture, List<UVTexture>> m_TextureUVTextureMap = new Dictionary<Texture, List<UVTexture>>();
        private Dictionary<SurfaceType, Dictionary<SurfaceImpact, SurfaceEffect>> m_SurfaceImpactEffectMap = new Dictionary<SurfaceType, Dictionary<SurfaceImpact, SurfaceEffect>>();
        private Dictionary<Texture, SurfaceType> m_TextureSurfaceTypeMap = new Dictionary<Texture, SurfaceType>();

        private Dictionary<Collider, SurfaceIdentifier> m_ColliderSurfaceIdentifiersMap = new Dictionary<Collider, SurfaceIdentifier>();
        private Dictionary<Collider, SurfaceType> m_ColliderSurfacesTypesMap = new Dictionary<Collider, SurfaceType>();
        private Dictionary<Collider, bool> m_ColliderComplexMaterialsMap = new Dictionary<Collider, bool>();
        private Dictionary<Collider, Renderer> m_ColliderRendererMap = new Dictionary<Collider, Renderer>();
        private Dictionary<Collider, Mesh> m_ColliderMeshMap = new Dictionary<Collider, Mesh>();
        private Dictionary<Collider, Texture> m_ColliderMainTextureMap = new Dictionary<Collider, Texture>();
        private Dictionary<Collider, Terrain> m_ColliderTerrainMap = new Dictionary<Collider, Terrain>();
        private Dictionary<Collider, bool> m_ColliderDecalsAllowedMap = new Dictionary<Collider, bool>();
        
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
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            InitObjectSurfaces();

            s_MaskID = Shader.PropertyToID("_Mask");
            s_SecondaryTextureID = Shader.PropertyToID("_MainTex2");

            m_HasTerrain = FindObjectsOfType<Terrain>() != null;
        }

        /// <summary>
        /// Stores all the textures added as an object surface to the TextureObjectSurfaceMap dictionary (if using the default UV) or
        /// the UVTextureObjectSurfaceMap dictionary (if using any other UV).
        /// </summary>
        protected void InitObjectSurfaces()
        {
            if (m_ObjectSurfaces == null) {
                return;
            }

            for (int i = 0; i < m_ObjectSurfaces.Length; ++i) {
                for (int j = 0; j < m_ObjectSurfaces[i].UVTextures.Length; ++j) {
                    if (ObjectSurfaces[i].UVTextures[j].Texture == null) {
                        continue;
                    }

                    if (m_UVTextureObjectSurfaceMap.ContainsKey(m_ObjectSurfaces[i].UVTextures[j])) {
                        continue;
                    }

                    m_TextureSurfaceTypeMap.Add(m_ObjectSurfaces[i].UVTextures[j].Texture, m_ObjectSurfaces[i].SurfaceType);

                    // Detect if the texture is a surface is a simple surface or complex surface. Simple surfaces have the default UV and only contain one instance.
                    // Complex surfaces have a custom UV and need to be stored in a separate variable so more processing can be done to detect the surface.
                    if (m_ObjectSurfaces[i].UVTextures[j].UV == m_DefaultUV && !IsDuplicateObjectTexture(m_ObjectSurfaces[i].UVTextures[j].Texture)) {
                        m_TextureObjectSurfaceMap.Add(m_ObjectSurfaces[i].UVTextures[j].Texture, m_ObjectSurfaces[i]);
                    } else { // Complex surface.
                        m_UVTextureObjectSurfaceMap.Add(m_ObjectSurfaces[i].UVTextures[j], m_ObjectSurfaces[i]);
                        // Store a mapping from the Texture to the UVTexture.
                        List<UVTexture> uvTextures;
                        if (!m_TextureUVTextureMap.TryGetValue(m_ObjectSurfaces[i].UVTextures[j].Texture, out uvTextures)) {
                            uvTextures = new List<UVTexture>();
                            uvTextures.Add(m_ObjectSurfaces[i].UVTextures[j]);
                            m_TextureUVTextureMap.Add(m_ObjectSurfaces[i].UVTextures[j].Texture, uvTextures);
                        } else {
                            uvTextures.Add(ObjectSurfaces[i].UVTextures[j]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Detects if the specified texture is a duplicate within the ObjectSurfaces array.
        /// </summary>
        /// <returns>True if the specified texture is a duplicate within the ObjectSurfaces array.</returns>
        private bool IsDuplicateObjectTexture(Texture texture)
        {
            var count = 0;
            for (int i = 0; i < m_ObjectSurfaces.Length; i++) {
                for (int j = 0; j < m_ObjectSurfaces[i].UVTextures.Length; j++) {
                    if (ObjectSurfaces[i].UVTextures[j].Texture == null) {
                        continue;
                    }
                    if (ObjectSurfaces[i].UVTextures[j].Texture == texture) {
                        count++;
                    }
                }
            }

            return count > 1;
        }

        /// <summary>
        /// Tries to spawn the effect based on the RaycastHit and SurfaceImpact.
        /// </summary>
        /// <param name="hit">The RaycastHit which caused the SurfaceEffect to spawn.</param>
        /// <param name="surfaceImpact">A reference to the Surface Impact triggered when the object hits an object.</param>
        /// <param name="gravityDirection">The normalized direction of the character's gravity.</param>
        /// <param name="timeScale">The timescale of the character.</param>
        /// <param name="originator">The object which spawned the effect.</param>
        /// <returns>True if the effect was spawned.</returns>
        public static bool SpawnEffect(RaycastHit hit, SurfaceImpact surfaceImpact, Vector3 gravityDirection, float timeScale, GameObject originator)
        {
            return Instance.SpawnEffectInternal(hit, hit.collider, surfaceImpact, gravityDirection, timeScale, originator);
        }

        /// <summary>
        /// Tries to spawn the effect based on the RaycastHit and SurfaceImpact.
        /// </summary>
        /// <param name="hit">The RaycastHit which caused the SurfaceEffect to spawn.</param>
        /// <param name="collider">The collider of the object that was hit.</param>
        /// <param name="surfaceImpact">A reference to the Surface Impact triggered when the object hits an object.</param>
        /// <param name="gravityDirection">The normalized direction of the character's gravity.</param>
        /// <param name="timeScale">The timescale of the character.</param>
        /// <param name="originator">The object which spawned the effect.</param>
        /// <returns>True if the effect was spawned.</returns>
        public static bool SpawnEffect(RaycastHit hit, Collider collider, SurfaceImpact surfaceImpact, Vector3 gravityDirection, float timeScale, GameObject originator)
        {
            return Instance.SpawnEffectInternal(hit, collider, surfaceImpact, gravityDirection, timeScale, originator);
        }

        /// <summary>
        /// Internal method which tries to spawn the effect based on the RaycastHit and SurfaceImpact.
        /// </summary>
        /// <param name="hit">The RaycastHit which caused the SurfaceEffect to spawn.</param>
        /// <param name="collider">The collider of the object that was hit.</param>
        /// <param name="surfaceImpact">A reference to the Surface Impact triggered when the object hits an object.</param>
        /// <param name="gravityDirection">The normalized direction of the character's gravity.</param>
        /// <param name="timeScale">The timescale of the character.</param>
        /// <param name="originator">The object which spawned the effect.</param>
        /// <returns>True if the effect was spawned.</returns>
        private bool SpawnEffectInternal(RaycastHit hit, Collider collider, SurfaceImpact surfaceImpact, Vector3 gravityDirection, float timeScale, GameObject originator)
        {
            SurfaceType surfaceType = null;
            var spawnDecals = false;
            var surfaceEffect = GetSurfaceEffect(hit, collider, surfaceImpact, ref surfaceType, ref spawnDecals);
            if (surfaceEffect == null) {
                return false;
            }

            surfaceEffect.Spawn(hit, gravityDirection, timeScale, originator, spawnDecals);

            return true;
        }

        /// <summary>
        /// Tries to spawn the effect based on the RaycastHit and SurfaceImpact.
        /// </summary>
        /// <param name="hit">The RaycastHit which caused the SurfaceEffect to spawn.</param>
        /// <param name="surfaceImpact">A reference to the Surface Impact triggered when the object hits an object.</param>
        /// <param name="gravityDirection">The normalized direction of the character's gravity.</param>
        /// <param name="timeScale">The timescale of the character.</param>
        /// <param name="originator">The object which spawned the effect.</param>
        /// <param name="footprintDirection">The direction that the footprint decal should face.</param>
        /// <param name="flipFootprint">Should the footprint decal be flipped?</param>
        /// <returns>True if the effect was spawned.</returns>
        public static bool SpawnEffect(RaycastHit hit, SurfaceImpact surfaceImpact, Vector3 gravityDirection, float timeScale, GameObject originator, Vector3 footprintDirection, bool flipFootprint)
        {
            return Instance.SpawnEffectInternal(hit, hit.collider, surfaceImpact, gravityDirection, timeScale, originator, footprintDirection, flipFootprint);
        }

        /// <summary>
        /// Tries to spawn the effect based on the RaycastHit and SurfaceImpact.
        /// </summary>
        /// <param name="hit">The RaycastHit which caused the SurfaceEffect to spawn.</param>
        /// <param name="collider">The collider of the object that was hit.</param>
        /// <param name="surfaceImpact">A reference to the Surface Impact triggered when the object hits an object.</param>
        /// <param name="gravityDirection">The normalized direction of the character's gravity.</param>
        /// <param name="timeScale">The timescale of the character.</param>
        /// <param name="originator">The object which spawned the effect.</param>
        /// <param name="footprintDirection">The direction that the footprint decal should face.</param>
        /// <param name="flipFootprint">Should the footprint decal be flipped?</param>
        /// <returns>True if the effect was spawned.</returns>
        public static bool SpawnEffect(RaycastHit hit, Collider collider, SurfaceImpact surfaceImpact, Vector3 gravityDirection, float timeScale, GameObject originator, Vector3 footprintDirection, bool flipFootprint)
        {
            return Instance.SpawnEffectInternal(hit, collider, surfaceImpact, gravityDirection, timeScale, originator, footprintDirection, flipFootprint);
        }

        /// <summary>
        /// Internal method which tries to spawn the effect based on the RaycastHit and SurfaceImpact.
        /// </summary>
        /// <param name="hit">The RaycastHit which caused the SurfaceEffect to spawn.</param>
        /// <param name="collider">The collider of the object that was hit.</param>
        /// <param name="surfaceImpact">A reference to the Surface Impact triggered when the object hits an object.</param>
        /// <param name="gravityDirection">The normalized direction of the character's gravity.</param>
        /// <param name="timeScale">The timescale of the character.</param>
        /// <param name="footprintDirection">The direction that the footprint decal should face.</param>
        /// <param name="flipFootprint">Should the footprint decal be flipped?</param>
        /// <param name="originator">The object which spawned the effect.</param>
        /// <returns>True if the effect was spawned.</returns>
        private bool SpawnEffectInternal(RaycastHit hit, Collider collider, SurfaceImpact surfaceImpact, Vector3 gravityDirection, float timeScale, GameObject originator, Vector3 footprintDirection, bool flipFootprint)
        {
            SurfaceType surfaceType = null;
            var spawnDecals = false;
            var surfaceEffect = GetSurfaceEffect(hit, collider, surfaceImpact, ref surfaceType, ref spawnDecals);
            if (surfaceType == null || surfaceEffect == null) {
                return false;
            }

            // Not all surfaces allow footprints - revert to the regular spawn if the surface doesn't allow it.
            if (surfaceType.AllowFootprints) {
                surfaceEffect.SpawnFootprint(hit, gravityDirection, timeScale, originator, spawnDecals, footprintDirection, flipFootprint);
            } else {
                surfaceEffect.Spawn(hit, gravityDirection, timeScale, originator, spawnDecals);
            }

            return true;
        }

        /// <summary>
        /// Returns the SurfaceEffect based on the RaycastHit and SurfaceImpact.
        /// </summary>
        /// <param name="hit">The RaycastHit which caused the SurfaceEffect to spawn.</param>
        /// <param name="collider">The collider of the object that was hit.</param>
        /// <param name="surfaceImpact">A reference to the Surface Impact triggered when the object hits an object.</param>
        /// <param name="surfaceType">The SurfaceType used to get the SurfaceEffect.</param>
        /// <param name="spawnDecals">True if the SurfaceEffect can spawn decals.</param>
        /// <returns>The SurfaceEffect based on the RaycastHit and SurfaceImpact. Can be null.</returns>
        private SurfaceEffect GetSurfaceEffect(RaycastHit hit, Collider collider, SurfaceImpact surfaceImpact, ref SurfaceType surfaceType, ref bool spawnDecals)
        {
            surfaceType = GetSurfaceType(hit, collider);
            spawnDecals = ShouldSpawnDecals(collider);
            return GetSurfaceEffect(surfaceImpact, ref surfaceType, ref spawnDecals);
        }

        /// <summary>
        /// Returns the SurfaceType based on the RaycastHit.
        /// </summary>
        /// <param name="hit">The RaycastHit which caused the SurfaceEffect to spawn.</param>
        /// <param name="collider">The collider of the object that was hit.</param>
        /// <returns>The SurfaceType based on the RaycastHit. Can be null.</returns>
        private SurfaceType GetSurfaceType(RaycastHit hit, Collider collider)
        {
            // The SurfaceType on the SurfaceIdentifier can provide a unique SurfaceType for that collider. Therefore it should be tested first.
            var surfaceIdentifier = GetSurfaceIdentifier(collider);
            if (surfaceIdentifier != null) {
                if (surfaceIdentifier.SurfaceType != null) {
                    return surfaceIdentifier.SurfaceType;
                }
            }

            // Detect objects with a single material and no texture regions.
            var surfaceType = GetSimpleSurfaceType(collider);
            if (surfaceType != null) {
                return surfaceType;
            }

            // Detect objects with texture regions (atlases), materials, or secondary maps.
            surfaceType = GetComplexSurfaceType(hit, collider);
            if (surfaceType != null) {
                return surfaceType;
            }

            // Check the terrain for a surface if all of the above failed.
            surfaceType = GetTerrainSurfaceType(hit, collider);
            if (surfaceType != null) {
                return surfaceType;
            }

            return null;
        }

        /// <summary>
        /// Returns the SurfaceIdentifier for the specified collider.
        /// </summary>
        /// <param name="collider">The collider to retrieve the SurfaceIdentifier of.</param>
        /// <returns>The SurfaceIdentifier for the specified collider. Can be null.</returns>
        private SurfaceIdentifier GetSurfaceIdentifier(Collider collider)
        {
            if (collider == null) {
                return null;
            }

            SurfaceIdentifier surfaceIdentifier;
            if (!m_ColliderSurfaceIdentifiersMap.TryGetValue(collider, out surfaceIdentifier)) {
                // Try to find a SurfaceIdentifier on the collider's GameObject.
                surfaceIdentifier = collider.GetComponent<SurfaceIdentifier>();
                // If there is no SurfaceIdentifier on the collider GameObject then try to find a SurfaceIdentifier withinin the children or parent.
                if (surfaceIdentifier == null) {
                    surfaceIdentifier = collider.GetComponentInChildren<SurfaceIdentifier>();

                    if (surfaceIdentifier == null) {
                        surfaceIdentifier = collider.GetComponentInParent<SurfaceIdentifier>();
                    }
                }

                m_ColliderSurfaceIdentifiersMap.Add(collider, surfaceIdentifier);
                // Remember if the SurfaceIdentifier allows decals.
                if (surfaceIdentifier != null && !m_ColliderDecalsAllowedMap.ContainsKey(collider)) {
                    m_ColliderDecalsAllowedMap.Add(collider, surfaceIdentifier.AllowDecals);
                }
            }

            return surfaceIdentifier;
        }

        /// <summary>
        /// Returns the SurfaceType of a single material and no texture regions.
        /// </summary>
        /// <param name="collider">The surface collider.</param>
        /// <returns>The SurfaceType of a single material with no texture regions. Can be null.</returns>
        private SurfaceType GetSimpleSurfaceType(Collider collider)
        {
            if (collider == null) {
                return null;
            }

            SurfaceType surfaceType;
            if (!m_ColliderSurfacesTypesMap.TryGetValue(collider, out surfaceType)) {
                // A simple surface will only have a single material.
                if (!HasComplexMaterial(collider)) {
                    var texture = GetMainTexture(collider);
                    if (texture != null && !m_TextureUVTextureMap.ContainsKey(texture)) {
                        surfaceType = GetNonUVSurfaceType(texture);
                    }
                }

                m_ColliderSurfacesTypesMap.Add(collider, surfaceType);
            }

            return surfaceType;
        }

        /// <summary>
        /// Returns true if the object associated with the specified collider has a complex material. A complex material includes multiple materials or materials with a secondary map.
        /// </summary>
        /// <param name="collider">The collider to determine if it has a complex material.</param>
        /// <returns>True if the object associated with the specified collider has complex material.</returns>
        private bool HasComplexMaterial(Collider collider)
        {
            if (collider == null) {
                return false;
            }

            if (!m_ColliderComplexMaterialsMap.TryGetValue(collider, out var complexMaterials)) {
                var renderer = GetRenderer(collider);
                if (renderer != null) {
                    complexMaterials = renderer.sharedMaterials.Length > 1;
                }

                // A complex material also includes a secondary map.
                if (!complexMaterials && renderer != null && renderer.sharedMaterials.Length > 0) {
                    var material = renderer.sharedMaterials[0];
                    complexMaterials = material != null && material.HasProperty(s_MaskID) && material.HasProperty(s_SecondaryTextureID);
                }

                m_ColliderComplexMaterialsMap.Add(collider, complexMaterials);
            }

            return complexMaterials;
        }


        /// <summary>
        /// Returns the main renderer of the specified collider.
        /// </summary>
        /// <param name="colllider">The collider to get the renderer of.</param>
        /// <returns>The main Renderer of the specified collider. Can be null.</returns>
        private Renderer GetRenderer(Collider collider)
        {
            // Ignore null colliders and triggers.
            if (collider == null || collider.isTrigger) {
                return null;
            }

            // Unity terrains have no renderers.
            if (collider is TerrainCollider) {
                return null;
            }

            Renderer renderer;
            if (!m_ColliderRendererMap.TryGetValue(collider, out renderer)) {
                // Try to get a renderer on the collider's GameObject.
                renderer = collider.GetComponent<Renderer>();

                // If no renderer exists, try to find a renderer in the collider's children.
                if (renderer == null || !renderer.enabled || renderer is SkinnedMeshRenderer) {
                    var childRenderers = collider.GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < childRenderers.Length; ++i) {
                        if (childRenderers[i] == renderer || !childRenderers[i].enabled || renderer is SkinnedMeshRenderer) {
                            continue;
                        }
                        renderer = childRenderers[i];
                        break;
                    }
                }

                // If no renderer exists, try to find a renderer in the collider's parent.
                if (renderer == null || !renderer.enabled || renderer is SkinnedMeshRenderer) {
                    var parentRenderers = collider.GetComponentsInParent<Renderer>();
                    for (int i = 0; i < parentRenderers.Length; ++i) {
                        if (parentRenderers[i] == renderer || !parentRenderers[i].enabled || renderer is SkinnedMeshRenderer) {
                            continue;
                        }
                        renderer = parentRenderers[i];
                        break;
                    }
                }

                // SkinnedMeshRenderers can not have their triangles fetched.
                if (renderer != null && (!renderer.enabled || renderer is SkinnedMeshRenderer)) {
                    renderer = null;
                }

                m_ColliderRendererMap.Add(collider, renderer);
            }

            return renderer;
        }

        /// <summary>
        /// Returns the texture of the specified collider.
        /// </summary>
        /// <param name="colllider">The collider to get the texture of.</param>
        /// <returns>The texture of the specified collider. Can be null.</returns>
        private Texture GetMainTexture(Collider collider)
        {
            if (collider == null) {
                return null;
            }

            Texture texture;
            if (!m_ColliderMainTextureMap.TryGetValue(collider, out texture)) {
                // The texture is retrieved from the renderer.
                var renderer = GetRenderer(collider);
                if (renderer != null && renderer.sharedMaterial != null && renderer.sharedMaterial.mainTexture != null) {
                    texture = renderer.sharedMaterial.mainTexture;
                }

                m_ColliderMainTextureMap.Add(collider, texture);
            }

            return texture;
        }

        /// <summary>
        /// Returns if the specified collider can spawn decals.
        /// </summary>
        /// <returns>False if the specified collider contains the SurfaceIdentifier component and it does not allow decals, otherwise true.</returns>
        private bool ShouldSpawnDecals(Collider collider)
        {
            if (collider == null) {
                return false;
            }

            var allowed = true;
            if (!m_ColliderDecalsAllowedMap.TryGetValue(collider, out allowed)) {
                return true;
            }
            return allowed;
        }

        /// <summary>
        /// Returns the SurfaceType for the specified texture.
        /// </summary>
        /// <param name="texture">The texture to get the surface type of.</param>
        /// <returns>The SurfaceType for the specified texture. Can be null.</returns>
        private SurfaceType GetNonUVSurfaceType(Texture texture)
        {
            if (texture == null) {
                return null;
            }

            SurfaceType surfaceType;
            if (!m_TextureSurfaceTypeMap.TryGetValue(texture, out surfaceType)) {
                ObjectSurface objectSurface;
                if (m_TextureObjectSurfaceMap.TryGetValue(texture, out objectSurface)) {
                    surfaceType = objectSurface.SurfaceType;
                }
                m_TextureSurfaceTypeMap.Add(texture, surfaceType);
            }

            return surfaceType;
        }

        /// <summary>
        /// A complex surface is a mesh which has a UV texture region or a secondary map.
        /// </summary>
        /// <param name="hit">The RaycastHit which caused the SurfaceEffect to spawn.</param>
        /// <returns>The SurfaceType of the complex surface. Can be null.</returns>
        private SurfaceType GetComplexSurfaceType(RaycastHit hit, Collider collider)
        {
            // GetHitMaterial will only return a value if the hit collider is a MeshCollider.
            var material = GetHitMaterial(hit, collider);
            if (material == null) {
                return null;
            }

            return GetComplexSurfaceType(material, hit, collider);
        }

        /// <summary>
        /// Returns the material for the specified RaycastHit.
        /// </summary>
        /// <param name="hit">The RaycastHit which caused the SurfaceEffect to spawn.</param>
        /// <returns>The material for the specified RaycastHit. Can be null.</returns>
        private Material GetHitMaterial(RaycastHit hit, Collider collider)
        {
            // triangleIndex will be -1 for any non-MeshCollider.
            if (hit.triangleIndex < 0) {
                return null;
            }

            var renderer = GetRenderer(collider);
            if (renderer == null || renderer.sharedMaterials == null) {
                return null;
            }

            // If the renderer only has one material then the uvs do not need to be used.
            if (renderer.sharedMaterials.Length == 1) {
                return renderer.sharedMaterials[0];
            }

            // The mesh not be null.
            var mesh = GetMesh(collider);
            if (mesh == null || hit.triangleIndex * 3 + 2 >= mesh.triangles.Length) {
                return null;
            }

            m_MaterialHitTriangle[0] = mesh.triangles[hit.triangleIndex * 3];
            m_MaterialHitTriangle[1] = mesh.triangles[hit.triangleIndex * 3 + 1];
            m_MaterialHitTriangle[2] = mesh.triangles[hit.triangleIndex * 3 + 2];

            // Search for the submesh which is affected by the RaycastHit. When the submesh matches the RaycastHit that material can be used.
            for (int i = 0; i < mesh.subMeshCount; i++) {
                var subMeshTriangles = mesh.GetTriangles(i);
                for (int j = 0; j < subMeshTriangles.Length; j += 3) {
                    if ((subMeshTriangles[j] == m_MaterialHitTriangle[0]) && (subMeshTriangles[j + 1] == m_MaterialHitTriangle[1])
                        && (subMeshTriangles[j + 2] == m_MaterialHitTriangle[2])) {
                        if (renderer.sharedMaterials.Length < i + 1) {
                            continue;
                        }
                        if (renderer.sharedMaterials[i] == null) {
                            continue;
                        }
                        return renderer.sharedMaterials[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the mesh of the specified collider.
        /// </summary>
        /// <param name="colllider">The collider to get the mesh of.</param>
        /// <returns>The mesh of the specified collider. Can be null.</returns>
        private Mesh GetMesh(Collider collider)
        {
            // Ignore null colliders and triggers.
            if (collider == null || collider.isTrigger) {
                return null;
            }

            Mesh mesh;
            if (!m_ColliderMeshMap.TryGetValue(collider, out mesh)) {
                // If no MeshCollider exists then try to get the mesh based off of the MeshFilter.
                var meshFilter = collider.GetComponent<MeshFilter>();

                // If no MeshFilter exists, try to find a MeshFilter in the collider's children.
                if (meshFilter == null) {
                    meshFilter = collider.GetComponentInChildren<MeshFilter>();
                }

                // If no MeshFilter exists, try to find a MeshFilter in the collider's parent.
                if (meshFilter == null) {
                    meshFilter = collider.GetComponentInParent<MeshFilter>();
                }

                // Get the mesh of the non-null MeshFilter.
                if (meshFilter != null) {
                    mesh = meshFilter.sharedMesh != null ? meshFilter.sharedMesh : meshFilter.mesh;

                    // The mesh can't be used if it's not readable or has no triangles.
                    if (!mesh.isReadable || mesh.triangles == null) {
                        mesh = null;
                    }
                }

                if (mesh == null) {
                    // If a mesh doesn't exist then use the MeshCollider (if it exists).
                    var meshCollider = collider.GetComponent<MeshCollider>();
                    if (meshCollider != null) {
                        mesh = meshCollider.sharedMesh;

                        // The mesh can't be used if it's not readable or has no triangles.
                        if (!mesh.isReadable || mesh.triangles == null) {
                            mesh = null;
                        }
                    }
                }

                m_ColliderMeshMap.Add(collider, mesh);
            }

            return mesh;
        }

        /// <summary>
        /// Returns the SurfaceType of the UVTexture.
        /// </summary>
        /// <param name="material">The material used to lookup the SurfaceType.</param>
        /// <param name="hit">The RaycastHit which caused the SurfaceEffect to spawn.</param>
        /// <param name="collider">The collider of the object that was hit.</param>
        /// <returns>The SurfaceType of the UVTexture. Cna be null.</returns>
        private SurfaceType GetComplexSurfaceType(Material material, RaycastHit hit, Collider collider)
        {
            if (material == null) {
                return null;
            }

            if (!(collider is MeshCollider)) {
                Debug.LogWarning("Warning: Surface UV regions on " + collider.name + " only support MeshColliders.");
                return null;
            }

            // The location may be on the secondary map.
            var texture = material.mainTexture;
            if (material.HasProperty(s_MaskID) && material.HasProperty(s_SecondaryTextureID)) {
                var maskTexture = material.GetTexture(s_MaskID) as Texture2D;
                if (maskTexture != null) {
                    var color = maskTexture.GetPixel((int)(hit.textureCoord.x * maskTexture.width), (int)(hit.textureCoord.y * maskTexture.height));
                    // A mask exists. If the alpha color is greater than 0.5f then the secondary texture should be used.
                    if (color.a > 0.5f) {
                        var secondaryTexture = material.GetTexture(s_SecondaryTextureID);
                        if (secondaryTexture != null) {
                            texture = secondaryTexture;
                        }
                    }
                }
            }

            if (texture == null) {
                return null;
            }

            List<UVTexture> uvTextures;
            if (m_TextureUVTextureMap.TryGetValue(texture, out uvTextures)) {
                for (int v = 0; v < uvTextures.Count; v++) {
                    // If the UV contains the adjusted texture coordinate then that portion of the texture was hit by the RaycastHit.
                    if (uvTextures[v].UV.Contains(AdjustTextureCoordinate(hit.textureCoord, material))) {
                        ObjectSurface objectSurface;
                        if (m_UVTextureObjectSurfaceMap.TryGetValue(uvTextures[v], out objectSurface)) {
                            return objectSurface.SurfaceType;
                        }
                    }
                }
            } else {
                // If the UVTexture doesn't exist then the object hit a material with a secondary map. Do a simple lookup based off of the texture.
                return GetNonUVSurfaceType(texture);
            }

            return null;
        }

        /// <summary>
        /// Adjusts the texture coordinate for flipping, scale, offset and tiling. The returned value is used for determining the exact texture coordinate of a hit point.
        /// </summary>
        /// <param name="textureCoordinate">The texture coordinate to adjust.</param>
        /// <param name="material">The material which is used by the texture coordinate.</param>
        /// <returns>The adjusted texture.</returns>
        private Vector2 AdjustTextureCoordinate(Vector2 textureCoordinate, Material material)
        {
            // Adjust for material tiling.
            textureCoordinate.x *= material.mainTextureScale.x;
            textureCoordinate.y *= material.mainTextureScale.y;

            // Adjust for material offset.
            textureCoordinate.x += material.mainTextureOffset.x;
            textureCoordinate.y -= material.mainTextureOffset.y;

            // Adjust for tiling on mesh.
            textureCoordinate.x %= 1;
            textureCoordinate.y %= 1;

            // Adjust for back projection.
            if (textureCoordinate.x < 0) {
                textureCoordinate.x = 1 - Mathf.Abs(textureCoordinate.x);
            }
            if (textureCoordinate.y < 0) {
                textureCoordinate.y = 1 - Mathf.Abs(textureCoordinate.y);
            }

            // Flip the UV upside down to have XY coordinates make more sense in the editor.
            textureCoordinate.y = 1 - textureCoordinate.y;

            return textureCoordinate;
        }

        /// <summary>
        /// Returns the terrain SurfaceType from the specified RaycastHit.
        /// </summary>
        /// <param name="hit">The RaycastHit which caused the SurfaceEffect to spawn.</param>
        /// <param name="collider">The collider of the object that was hit.</param>
        /// <returns>The terrain SurfaceType from the specified RaycastHit. Can be null.</returns>
        private SurfaceType GetTerrainSurfaceType(RaycastHit hit, Collider collider)
        {
            if (!m_HasTerrain || collider == null) {
                return null;
            }

            // Retrieve the terrain based off of the collider.
            Terrain terrain;
            if (!m_ColliderTerrainMap.TryGetValue(collider, out terrain)) {
                terrain = collider.GetComponent<Terrain>();
                m_ColliderTerrainMap.Add(collider, terrain);
            }

            if (terrain == null) {
                return null;
            }

            // The raycast may have hit a tree.
            var texture = GetTreeTexture(hit, terrain);
            if (texture == null) {
                // The raycast did not hit a tree. Test the terrain.
                texture = GetTerrainTexture(hit.point, terrain);
                if (texture == null) {
                    return null;
                }
            }

            SurfaceType surfaceType;
            m_TextureSurfaceTypeMap.TryGetValue(texture, out surfaceType);
            return surfaceType;
        }

        /// <summary>
        /// Returns the tree texture from the specified collider and position.
        /// </summary>
        /// <param name="hit">The RaycastHit which caused the SurfaceEffect to spawn.</param>
        /// <returns>The terrain that was hit by the raycast.</returns>
        /// <returns>The tree texture from the specified RaycastHit and terrain. Can be null.</returns>
        private Texture GetTreeTexture(RaycastHit hit, Terrain terrain)
        {
            if (!m_DetectTerrainTreeTextures) {
                return null;
            }

            var terrainData = terrain.terrainData;
            if (terrainData.treeInstanceCount == 0) {
                return null;
            }

            // At least one tree exists. Determine if the raycast hit a tree.
            Texture texture = null;
            for (int i = 0; i < terrainData.treeInstanceCount; ++i) {
                var treeInstance = terrainData.treeInstances[i];
                var position = Vector3.Scale(terrainData.size, treeInstance.position) + terrain.GetPosition();
                var treePrototype = terrainData.treePrototypes[treeInstance.prototypeIndex];
                if (treePrototype.prefab == null) {
                    continue;
                }

                // Determine if the raycast hit a tree. This is done by instantiating a tree from the prototype index and placing it in the world.
                var tree = ObjectPool.Instantiate(treePrototype.prefab);
                var treeCollider = tree.GetCachedComponent<Collider>();
                if (treeCollider == null) {
                    ObjectPool.Destroy(tree);
                    continue;
                }

                tree.transform.position = position;
                // The transforms need to be synced so the new tree position will respawn to tree raycasts.
                if (!Physics.autoSyncTransforms) {
                    Physics.SyncTransforms();
                }

                // Perform a raycast on the tree to determine if the tree was hit.
                if (treeCollider.Raycast(new Ray(hit.point + hit.normal * hit.distance, -hit.normal), out var treeHit, hit.distance + 1)) {
                    texture = GetMainTexture(treeCollider);
                }

                ObjectPool.Destroy(tree);
                if (texture == null) {
                    continue;
                }
                return texture;
            }
            return null;
        }

        /// <summary>
        /// Returns the terrain texture from the specified collider and position.
        /// </summary>
        /// <param name="worldPosition">The position to retrieve the texture of.</param>
        /// <returns>The terrain that was hit by the raycast.</returns>
        /// <returns>The terrain texture from the specified position and terrain. Can be null.</returns>
        private Texture GetTerrainTexture(Vector3 worldPosition, Terrain terrain)
        {
            // Return the dominant ground texture at the world position in terrain.
            var terrainTextureID = GetDominantTerrainTexture(worldPosition, terrain);
            if (terrain.terrainData.terrainLayers == null || terrainTextureID > terrain.terrainData.terrainLayers.Length - 1) {
                return null;
            }

            return terrain.terrainData.terrainLayers[terrainTextureID].diffuseTexture;
        }

        /// <summary>
        /// Returns the main texture at the specified position of the terrain.
        /// </summary>
        /// <param name="worldPosition">The position to retrieve the texture of.</param>
        /// <param name="terrain">The terrain to retrieve the texture of.</param>
        /// <returns>The main texture at the specified position of the terrain. Can be null.</returns>
        private int GetDominantTerrainTexture(Vector3 worldPosition, Terrain terrain)
        {
            if (terrain == null) {
                return 0;
            }

            var terrainData = terrain.terrainData;
            if (terrainData.alphamapTextures == null || terrainData.alphamapTextures.Length == 0) {
                return 0;
            }

            var terrainPos = terrain.transform.position;

            // Calculate which splat map cell the worldPosition falls within (ignoring y).
            var mapX = (int)(((worldPosition.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
            var mapZ = (int)(((worldPosition.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

            // Get the splat data for this cell as a 1x1xN 3D array (where N = number of textures).
            var splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

            // Extract the 3D array data to a 1D array.
            var mix = new float[splatmapData.GetUpperBound(2) + 1];
            for (int n = 0; n < mix.Length; ++n) {
                mix[n] = splatmapData[0, 0, n];
            }

            // Loop through each mix value and find the maximum.
            float maxMix = 0;
            int maxIndex = 0;
            for (int n = 0; n < mix.Length; ++n) {
                if (mix[n] > maxMix) {
                    maxIndex = n;
                    maxMix = mix[n];
                }
            }

            return maxIndex;
        }

        /// <summary>
        /// Returns the SurfaceEffect for the specified SurfaceImpact and SurfaceType.
        /// </summary>
        /// <param name="surfaceImpact">The SurfaceImpact used to lookup the SurfaceEffect.</param>
        /// <param name="surfaceType">The SurfaceType used to lookup the SurfaceEffect.</param>
        /// <param name="spawnDecals">True if the SurfaceEffect can spawn decals.</param>
        /// <returns>The SurfaceEffect for the specified SurfaceImpact and SurfaceType. Can be null.</returns>
        private SurfaceEffect GetSurfaceEffect(SurfaceImpact surfaceImpact, ref SurfaceType surfaceType, ref bool spawnDecals)
        {
            var usingFallbackImpact = false;
            var usingFallbackSurface = false;

            // If there is no SurfaceImpact then use the fallback.
            if (surfaceImpact == null && m_FallbackSurfaceImpact != null) {
                surfaceImpact = m_FallbackSurfaceImpact;
                usingFallbackImpact = true;
            }

            // The SurfaceImpact must exist.
            if (surfaceImpact == null) {
                return null;
            }
            
            // If there is no SurfaceType then use the fallback.
            if (surfaceType == null && m_FallbackSurfaceType != null) {
                surfaceType = m_FallbackSurfaceType;
                usingFallbackSurface = true;
            }

            // The SurfaceType must exist and be valid.
            if (surfaceType == null || surfaceType.ImpactEffects == null || surfaceType.ImpactEffects.Length == 0) {
                return null;
            }

            var surfaceEffect = GetPrimarySurfaceEffect(surfaceImpact, surfaceType);

            // If the SurfaceEffect is null and the fallback surface isn't being used then the scene does not contain the ItemIdentifier. Use the fallback.
            if (surfaceEffect == null && !usingFallbackSurface) {
                surfaceType = m_FallbackSurfaceType;
                surfaceEffect = GetPrimarySurfaceEffect(surfaceImpact, surfaceType);
            }

            // If the SurfaceEffect is null then the detected surface does not recognize the ItemIdentifier so try again with the SurfaceManager's fallback ItemIdentifier.
            if (surfaceEffect == null) {
                surfaceEffect = GetPrimarySurfaceEffect(m_FallbackSurfaceImpact, surfaceType);
                // If the SurfaceEffect is still null then nothing more can be done.
                if (surfaceEffect == null) {
                    return null;
                }
            }

            // A SurfaceEffect was found! Determine if the decal can be spawned.
            if (spawnDecals && surfaceEffect.Decals.Length > 0) {
                if (usingFallbackSurface || usingFallbackImpact) {
                    spawnDecals = m_FallbackAllowDecals;
                }
            } else {
                spawnDecals = false;
            }

            return surfaceEffect;
        }

        /// <summary>
        /// Returns the main SurfaceEffect which is used by the specified SurfaceImpact and SurfaceType.
        /// </summary>
        /// <param name="surfaceImpact">The SurfaceImpact used to lookup the SurfaceEffect.</param>
        /// <param name="surfaceType">The SurfaceType used to lookup the SurfaceEffect.</param>
        /// <returns>The main SurfaceEffect which is used by the specified SurfaceImpact and SurfaceType. Can be null.</returns>
        private SurfaceEffect GetPrimarySurfaceEffect(SurfaceImpact surfaceImpact, SurfaceType surfaceType)
        {
            if (surfaceImpact == null) {
                return null;
            }

            // Get the SurfaceImpact based off of the SurfaceType.
            var surfaceImpactMap = GetSurfaceImpactMap(surfaceType);
            if (surfaceImpactMap == null || surfaceImpactMap.Count == 0) {
                return null;
            }

            SurfaceEffect surfaceEffect;
            surfaceImpactMap.TryGetValue(surfaceImpact, out surfaceEffect);
            return surfaceEffect;
        }

        /// <summary>
        /// Returns the mapping of SurfaceImpacts stored in the SurfaceType's SurfaceImpacts variable.
        /// </summary>
        /// <param name="surfaceType">The SurfaceType to retrieve the surfaceImpact map of.</param>
        /// <returns>The mapping of SurfaceImpacts stored in the SurfaceType's SurfaceImpacts variable. Can be null.</returns>
        private Dictionary<SurfaceImpact, SurfaceEffect> GetSurfaceImpactMap(SurfaceType surfaceType)
        {
            if (surfaceType == null) {
                return null;
            }

            Dictionary<SurfaceImpact, SurfaceEffect> surfaceImpactSurfaceEffect;
            if (!m_SurfaceImpactEffectMap.TryGetValue(surfaceType, out surfaceImpactSurfaceEffect)) {
                surfaceImpactSurfaceEffect = new Dictionary<SurfaceImpact, SurfaceEffect>();
                // Search through the ImpactEffects to find the SurfaceImpact.
                for (int i = 0; i < surfaceType.ImpactEffects.Length; i++) {
                    if (surfaceType.ImpactEffects[i].SurfaceImpact == null) {
                        continue;
                    }

                    if (surfaceImpactSurfaceEffect.ContainsKey(surfaceType.ImpactEffects[i].SurfaceImpact)) {
                        Debug.LogWarning("Warning: Surface Type '" + surfaceType + "' has more than one '" + surfaceType.ImpactEffects[i].SurfaceImpact + "' added. Only the first one will be used.");
                        continue;
                    }
                    surfaceImpactSurfaceEffect.Add(surfaceType.ImpactEffects[i].SurfaceImpact, surfaceType.ImpactEffects[i].SurfaceEffect);
                }

                m_SurfaceImpactEffectMap.Add(surfaceType, surfaceImpactSurfaceEffect);
            }

            return surfaceImpactSurfaceEffect;
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