/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.FirstPersonController.Camera
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.FirstPersonController.Character.Identifiers;
    using Opsive.UltimateCharacterController.FirstPersonController.Items;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Identifiers;
    using Opsive.UltimateCharacterController.Items;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Swaps the first and third person materials so multiple cameras can render the scene within the same instance.
    /// </summary>
    public class MaterialSwapper : MonoBehaviour
    {
        /// <summary>
        /// Specifies which renderers should be swapped.
        /// </summary>
        private enum SwapMask
        {
            FirstPerson = 1,    // The first person perspective renderers should be swapped.
            ThirdPerson = 2,    // The third person perspective renderers should be swapped.
        }

        [Tooltip("The material used to make the object invisible but still cast shadows.")]
        [SerializeField] protected Material m_InvisibleMaterial;

        private GameObject m_GameObject;
        private Camera m_Camera;
        private CameraController m_CameraController;
        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private SwapMask m_SwapMask = 0;

        private HashSet<Renderer> m_AddedFirstPersonRenderers;
        private List<Renderer> m_FirstPersonRenderers;
        private List<Material[]> m_FirstPersonOriginalMaterials;
        private List<Material[]> m_FirstPersonInvisibleMaterials;
        private HashSet<GameObject> m_FirstPersonBaseObjects;

        private HashSet<Renderer> m_AddedThirdPersonRenderers;
        private List<Renderer> m_ThirdPersonRenderers;
        private List<Material[]> m_ThirdPersonOriginalMaterials;
        private List<Material[]> m_ThirdPersonInvisibleMaterials;

#if THIRD_PERSON_CONTROLLER
        private ThirdPersonController.Camera.ObjectFader m_ObjectFader;
#endif

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Awake()
        {
            m_GameObject = gameObject;
            m_CameraController = m_GameObject.GetCachedComponent<CameraController>();
            if (m_CameraController == null) {
                // If the camera controller is null then the component has been added to the first person child camera.
                m_CameraController = gameObject.GetCachedParentComponent<CameraController>();
                m_SwapMask = SwapMask.FirstPerson;
            } else {
                var viewTypes = m_CameraController.ViewTypes;
                // The component exists on the main camera GameObject. If there is no child first person camera then the single component is responsible for
                // swapping both first and third person renderer materials.
                for (int i = 0; i < viewTypes.Length; ++i) {
                    if (viewTypes[i] is ViewTypes.FirstPerson) {
                        var firstPersonViewType = viewTypes[i] as ViewTypes.FirstPerson;
#if ULTIMATE_CHARACTER_CONTROLLER_LWRP || ULTIMATE_CHARACTER_CONTROLLER_UNIVERSALRP
                        m_SwapMask = firstPersonViewType.OverlayRenderType == ViewTypes.FirstPerson.ObjectOverlayRenderType.SecondCamera ? SwapMask.ThirdPerson : (SwapMask.FirstPerson | SwapMask.ThirdPerson);
#else
                        m_SwapMask = firstPersonViewType.UseFirstPersonCamera ? SwapMask.ThirdPerson : (SwapMask.FirstPerson | SwapMask.ThirdPerson);
#endif
#if UNITY_EDITOR
                        if ((m_SwapMask & SwapMask.ThirdPerson) != 0) {
                            // Ensure the child camera has the Material Swapper component.
                            if (firstPersonViewType.FirstPersonCamera != null) {
                                var firstPersonMaterialSwapper = firstPersonViewType.FirstPersonCamera.GetComponent<MaterialSwapper>();
                                if (firstPersonMaterialSwapper == null) {
                                    Debug.LogWarning("Warning: The First Person Camera should have the Material Swapper component added to the GameObject.");
                                }
                            }
                        }
#endif
                        break;
                    }
                }
#if THIRD_PERSON_CONTROLLER
                m_ObjectFader = m_CameraController.GetComponent<ThirdPersonController.Camera.ObjectFader>();
#endif
            }
            m_Camera = m_CameraController.GetComponent<Camera>();

            // Instantiate the storage objects based on the swap mode.
            if ((m_SwapMask & SwapMask.FirstPerson) != 0) {
                m_AddedFirstPersonRenderers = new HashSet<Renderer>();
                m_FirstPersonRenderers = new List<Renderer>();
                m_FirstPersonOriginalMaterials = new List<Material[]>();
                m_FirstPersonInvisibleMaterials = new List<Material[]>();
                m_FirstPersonBaseObjects = new HashSet<GameObject>();
            }
            if ((m_SwapMask & SwapMask.ThirdPerson) != 0) {
                m_AddedThirdPersonRenderers = new HashSet<Renderer>();
                m_ThirdPersonRenderers = new List<Renderer>();
                m_ThirdPersonOriginalMaterials = new List<Material[]>();
                m_ThirdPersonInvisibleMaterials = new List<Material[]>();
            }

            enabled = false;
            EventHandler.RegisterEvent<GameObject>(m_CameraController.gameObject, "OnCameraAttachCharacter", OnAttachCharacter);
        }

#if UNIVERSAL_RENDER_PIPELINE
        /// <summary>
        /// The component has been enabled.
        /// </summary>
        private void OnEnable()
        {
            UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
            UnityEngine.Rendering.RenderPipelineManager.endCameraRendering += EndCameraRendering;
        }
#endif

        /// <summary>
        /// Attaches the component to the specified character.
        /// </summary>
        /// <param name="character">The handler to attach the camera to.</param>
        protected virtual void OnAttachCharacter(GameObject character)
        {
            // Don't do anything if the character is the same.
            if (m_CharacterLocomotion != null && character == m_CharacterLocomotion.gameObject) {
                return;
            }

            if (m_CharacterLocomotion != null) {
                // The character is being changed.
                if (m_FirstPersonRenderers != null) {
                    m_AddedFirstPersonRenderers.Clear();
                    m_FirstPersonRenderers.Clear();
                    m_FirstPersonOriginalMaterials.Clear();
                    m_FirstPersonInvisibleMaterials.Clear();
                    m_FirstPersonBaseObjects.Clear();
                }
                if (m_ThirdPersonRenderers != null) {
                    m_AddedThirdPersonRenderers.Clear();
                    m_ThirdPersonRenderers.Clear();
                    m_ThirdPersonOriginalMaterials.Clear();
                    m_ThirdPersonInvisibleMaterials.Clear();
                }
                EventHandler.UnregisterEvent<bool>(m_CharacterLocomotion.gameObject, "OnCameraChangePerspectives", OnChangePerspectives);
                EventHandler.UnregisterEvent<Item>(m_CharacterLocomotion.gameObject, "OnInventoryAddItem", OnAddItem);
                EventHandler.UnregisterEvent(m_CharacterLocomotion.gameObject, "OnRespawn", OnRespawn);
                EventHandler.ExecuteEvent(m_CharacterLocomotion.gameObject, "OnCharacterIndependentFade", false, false);
                m_CharacterLocomotion = null;
            }

            enabled = character != null;
            if (character == null) {
                return;
            }

            m_CharacterLocomotion = character.GetCachedComponent<UltimateCharacterLocomotion>();
#if THIRD_PERSON_CONTROLLER
            // Force the character into using third person materials so the correct materials can be cached.
            var perspectiveMonitor = character.GetCachedComponent<ThirdPersonController.Character.PerspectiveMonitor>();
            perspectiveMonitor.UpdateThirdPersonMaterials(true);
#endif

            if ((m_SwapMask & SwapMask.FirstPerson) != 0) {
                var firstPersonBaseObjects = character.GetComponentsInChildren<FirstPersonBaseObject>(true);
                for (int i = 0; i < firstPersonBaseObjects.Length; ++i) {
                    CacheFirstPersonRenderers(firstPersonBaseObjects[i].gameObject);
                    // Remember the base objects so they are not added again if a runtime item is picked up.
                    m_FirstPersonBaseObjects.Add(firstPersonBaseObjects[i].gameObject);
                }
            }

            if ((m_SwapMask & SwapMask.ThirdPerson) != 0) {
                var thirdPersonObjects = character.GetComponentsInChildren<ThirdPersonObject>(true);
                for (int i = 0; i < thirdPersonObjects.Length; ++i) {
                    CacheThirdPersonRenderers(thirdPersonObjects[i].gameObject);
                }
            }

#if THIRD_PERSON_CONTROLLER
            perspectiveMonitor.UpdateThirdPersonMaterials(false);
#endif

            EventHandler.RegisterEvent<bool>(m_CharacterLocomotion.gameObject, "OnCameraChangePerspectives", OnChangePerspectives);
            EventHandler.RegisterEvent<Item>(m_CharacterLocomotion.gameObject, "OnInventoryAddItem", OnAddItem);
            EventHandler.RegisterEvent(m_CharacterLocomotion.gameObject, "OnRespawn", OnRespawn);
            EventHandler.ExecuteEvent(m_CharacterLocomotion.gameObject, "OnCharacterIndependentFade", true, false);

            // Assume the first person objects are not rendering.
            EnableThirdPersonMaterials();
        }

        /// <summary>
        /// The camera perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the camera in a first person perspective?</param>
        private void OnChangePerspectives(bool firstPersonPerspective)
        {
            enabled = firstPersonPerspective
#if THIRD_PERSON_CONTROLLER
                        || m_ObjectFader != null
#endif
                ;
            if (!enabled) {
                EnableThirdPersonMaterials();
            }
        }

        /// <summary>
        /// The inventory has added the specified item.
        /// </summary>
        /// <param name="item">The item that was added.</param>
        private void OnAddItem(Item item)
        {
            var perspectiveItems = item.GetComponents<PerspectiveItem>();
            for (int i = 0; i < perspectiveItems.Length; ++i) {
                if (perspectiveItems[i].FirstPersonItem) {
                    if ((m_SwapMask & SwapMask.FirstPerson) != 0) {
                        GameObject firstPersonObject;
                        if (m_FirstPersonBaseObjects.Contains(perspectiveItems[i].Object)) {
                            firstPersonObject = (perspectiveItems[i] as FirstPersonPerspectiveItem).VisibleItem;
                        } else {
                            firstPersonObject = perspectiveItems[i].Object;
                            m_FirstPersonBaseObjects.Add(firstPersonObject);
                        }

                        CacheFirstPersonRenderers(firstPersonObject);
                    }
                } else if ((m_SwapMask & SwapMask.ThirdPerson) != 0) {
                    CacheThirdPersonRenderers(perspectiveItems[i].Object);
                }
            }
        }

        /// <summary>
        /// Caches the renderers on the specified first person object.
        /// </summary>
        /// <param name="firstPersonObject">The first person object to cache the renderers of.</param>
        private void CacheFirstPersonRenderers(GameObject firstPersonObject)
        {
            if (firstPersonObject == null) {
                return;
            }

            var renderers = firstPersonObject.GetComponentsInChildren<Renderer>(true);
            var emptyMaterial = new Material[0];
            for (int i = 0; i < renderers.Length; ++i) {
                if (m_AddedFirstPersonRenderers.Contains(renderers[i])) {
                    continue;
                }
                m_AddedFirstPersonRenderers.Add(renderers[i]);

                m_FirstPersonRenderers.Add(renderers[i]);
                m_FirstPersonOriginalMaterials.Add(renderers[i].materials);
                m_FirstPersonInvisibleMaterials.Add(emptyMaterial);
            }
        }

        /// <summary>
        /// Caches the renderers on the specified third person object.
        /// </summary>
        /// <param name="thirdPersonObject">The third person object to cache the renderers of.</param>
        private void CacheThirdPersonRenderers(GameObject thirdPersonObject)
        {
            if (thirdPersonObject == null) {
                return;
            }

            var renderers = thirdPersonObject.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; ++i) {
                if (m_AddedThirdPersonRenderers.Contains(renderers[i])) {
                    continue;
                }
                m_AddedThirdPersonRenderers.Add(renderers[i]);

                m_ThirdPersonRenderers.Add(renderers[i]);
                m_ThirdPersonOriginalMaterials.Add(renderers[i].materials);
                var invisibleMaterials = new Material[renderers[i].materials.Length];
                for (int j = 0; j < invisibleMaterials.Length; ++j) {
                    invisibleMaterials[j] = m_InvisibleMaterial;
                }
                m_ThirdPersonInvisibleMaterials.Add(invisibleMaterials);
            }
        }

#if !UNIVERSAL_RENDER_PIPELINE
        /// <summary>
        /// The camera has started to render.
        /// </summary>
        private void OnPreRender()
        {
            BeginCameraRendering(m_Camera);
        }

        /// <summary>
        /// The camera has stopped rendering.
        /// </summary>
        private void OnPostRender()
        {
            EndCameraRendering(m_Camera);
        }
#endif

        /// <summary>
        /// The camera has started to render.
        /// </summary>
        /// <param name="context">The context of the SRP.</param>
        /// <param name="camera">The camera that is rendering.</param>
#if UNIVERSAL_RENDER_PIPELINE
        private void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
#else
        private void BeginCameraRendering(Camera camera)
#endif
        {
            if (camera != m_Camera) {
                return;
            }

#if THIRD_PERSON_CONTROLLER
            if (m_ObjectFader != null) {
                m_ObjectFader.MultiCameraRender(true);
            }
#endif
            if (!m_CharacterLocomotion.FirstPersonPerspective) {
                return;
            }

            // Swap the first person objects to the original material so the current camera can see the arms mesh.
            if ((m_SwapMask & SwapMask.FirstPerson) != 0) {
                for (int i = 0; i < m_FirstPersonRenderers.Count; ++i) {
                    m_FirstPersonRenderers[i].materials = m_FirstPersonOriginalMaterials[i];
                }
            }

            // The third person objects should be swapped to the invisible materials because the arms material will be rendered.
            if ((m_SwapMask & SwapMask.ThirdPerson) != 0) {
                for (int i = 0; i < m_ThirdPersonRenderers.Count; ++i) {
                    m_ThirdPersonRenderers[i].materials = m_ThirdPersonInvisibleMaterials[i];
                }
            }
        }

        /// <summary>
        /// The camera has stopped rendering.
        /// </summary>
        /// <param name="context">The context of the SRP.</param>
        /// <param name="camera">The camera that stopped rendering.</param>
#if UNIVERSAL_RENDER_PIPELINE
        private void EndCameraRendering(ScriptableRenderContext context, Camera camera)
#else
        private void EndCameraRendering(Camera camera)
#endif
        {
            if (camera != m_Camera) {
                return;
            }

#if THIRD_PERSON_CONTROLLER
            if (m_ObjectFader != null) {
                m_ObjectFader.MultiCameraRender(false);
            }
#endif
            if (!m_CharacterLocomotion.FirstPersonPerspective) {
                return;
            }

            EnableThirdPersonMaterials();
        }

        /// <summary>
        /// Swaps the materials for the third person mateirals.
        /// </summary>
        private void EnableThirdPersonMaterials()
        {
            // Swap the first person objects back to the invisible material so the separate arms are not seen by other cameras.
            if ((m_SwapMask & SwapMask.FirstPerson) != 0) {
                for (int i = 0; i < m_FirstPersonRenderers.Count; ++i) {
                    m_FirstPersonRenderers[i].materials = m_FirstPersonInvisibleMaterials[i];
                }
            }

            // The third person objects should be swapped back to the original materials so the full mesh is rendered by other cameras.
            if ((m_SwapMask & SwapMask.ThirdPerson) != 0) {
                for (int i = 0; i < m_ThirdPersonRenderers.Count; ++i) {
                    m_ThirdPersonRenderers[i].materials = m_ThirdPersonOriginalMaterials[i];
                }
            }
        }

        /// <summary>
        /// The character has respawned.
        /// </summary>
        private void OnRespawn()
        {
            OnChangePerspectives(m_CharacterLocomotion.FirstPersonPerspective);
        }

#if UNIVERSAL_RENDER_PIPELINE
        /// <summary>
        /// The component has been disabled.
        /// </summary>
        private void OnDisable()
        {
            UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
            UnityEngine.Rendering.RenderPipelineManager.endCameraRendering -= EndCameraRendering;
        }
#endif

        /// <summary>
        /// The camera has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            OnAttachCharacter(null);
            EventHandler.UnregisterEvent<GameObject>(m_CameraController.gameObject, "OnCameraAttachCharacter", OnAttachCharacter);
        }
    }
}