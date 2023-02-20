/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Objects.ItemAssist
{
    /// <summary>
    /// The Smoke component is attached to a GameObject with the a ParticleSystem attached representing smoke. 
    /// </summary>
    public class Smoke : MonoBehaviour
    {
#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
        private GameObject m_GameObject;
        private Transform m_Transform;
        private Item m_Item;
        private int m_ItemActionID;
        private ParticleSystem[] m_Particles;
        private ParticleSystemSimulationSpace[] m_SimulationSpace;

        private GameObject m_Character;
        private int m_StartLayer;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_GameObject = gameObject;
            m_Transform = transform;
            m_Particles = GetComponentsInChildren<ParticleSystem>();
            m_SimulationSpace = new ParticleSystemSimulationSpace[m_Particles.Length];
            m_StartLayer = m_GameObject.layer;
        }
#endif

        /// <summary>
        /// A weapon has been fired and the smoke needs to show. 
        /// </summary>
        /// <param name="item">The item that the muzzle flash is attached to.</param>
        /// <param name="itemActionID">The ID which corresponds to the ItemAction that spawned the smoke.</param>
        /// <param name="characterLocomotion">The character that the smoke is attached to.</param>
        public void Show(Item item, int itemActionID, UltimateCharacterLocomotion characterLocomotion)
        {
#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
            m_Character = characterLocomotion.gameObject;
            EventHandler.RegisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);

            m_Item = item;
            m_ItemActionID = itemActionID;
            m_GameObject.layer = characterLocomotion.FirstPersonPerspective ? LayerManager.Overlay : m_StartLayer;

            // Disable the object after the particles are done playing.
            float maxLifeTime = 0;
            for (int i = 0; i < m_Particles.Length; ++i) {
                m_Particles[i].Play();
                var lifeTime = 0f;
                if ((lifeTime = m_Particles[i].main.startLifetime.Evaluate(0)) > maxLifeTime) {
                    maxLifeTime = lifeTime;
                }
            }

            Scheduler.Schedule(maxLifeTime, DestroySelf);
#endif
        }

#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
        /// <summary>
        /// Place itself back in the ObjectPool.
        /// </summary>
        public void DestroySelf()
        {
            ObjectPool.Destroy(m_GameObject);
        }

        /// <summary>
        /// The character perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the character in a first person perspective?</param>
        private void OnChangePerspectives(bool firstPersonPerspective)
        {
            // All of the particles should be set to local space so they'll change correctly when switching perspective.
            for (int i = 0; i < m_Particles.Length; ++i) {
                m_SimulationSpace[i] = m_Particles[i].main.simulationSpace;
                var mainParticle = m_Particles[i].main;
                mainParticle.simulationSpace = ParticleSystemSimulationSpace.Local;
            }

            // When switching locations the local position and rotation should remain the same.
            var localPosition = m_Transform.localPosition;
            var localRotation = m_Transform.rotation;

            var itemAction = m_Item.ItemActions[m_ItemActionID];
            var perspectiveProperties = (firstPersonPerspective ? itemAction.FirstPersonPerspectiveProperties : itemAction.ThirdPersonPerspectiveProperties);
            var smokeLocation = (perspectiveProperties as IShootableWeaponPerspectiveProperties).SmokeLocation;
            m_Transform.parent = smokeLocation;
            m_Transform.localPosition = localPosition;
            m_Transform.rotation = localRotation;

            // Switch the particle simulation space back to the previous value.
            for (int i = 0; i < m_Particles.Length; ++i) {
                var mainParticle = m_Particles[i].main;
                mainParticle.simulationSpace = m_SimulationSpace[i];
            }

            m_GameObject.layer = firstPersonPerspective ? LayerManager.Overlay : m_StartLayer;
        }

        /// <summary>
        /// The object has been disabled.
        /// </summary>
        private void OnDisable()
        {
            if (m_Character != null) {
                EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
            }
            m_Character = null;
        }
#endif
    }
}