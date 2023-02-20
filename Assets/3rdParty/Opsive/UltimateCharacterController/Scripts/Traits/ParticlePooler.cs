/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Game;

namespace Opsive.UltimateCharacterController.Traits
{
    /// <summary>
    /// Pools the ParticleSystem after it is done playing.
    /// </summary>
    public class ParticlePooler : MonoBehaviour
    {
        private GameObject m_GameObject;
        private ParticleSystem m_ParticleSystem;

        private ScheduledEventBase m_PoolEvent;

        /// <summary>
        /// Initialize the default variables.
        /// </summary>
        private void Awake()
        {
            m_GameObject = gameObject;
            m_ParticleSystem = GetComponent<ParticleSystem>();
        }

        /// <summary>
        /// Schedules the object to be pooled after the particle system has stopped playing.
        /// </summary>
        private void OnEnable()
        {
            m_PoolEvent = Scheduler.Schedule(m_ParticleSystem.main.duration, PoolGameObject);
        }

        /// <summary>
        /// Cancels the pool event if the object is disabled early.
        /// </summary>
        private void OnDisable()
        {
            Scheduler.Cancel(m_PoolEvent);
        }

        /// <summary>
        /// Returns the GameObject back to the ObjectPool.
        /// </summary>
        private void PoolGameObject()
        {
            ObjectPool.Destroy(m_GameObject);
        }
    }
}