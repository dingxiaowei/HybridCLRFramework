/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Game
{
    using Dypsloom.Shared.Utility;
    using UnityEngine;

    /// <summary>
    /// Enable/Disable a gameobject after a delay.
    /// </summary>
    public class EnableDelay : MonoBehaviour
    {
        [Tooltip("Time after awake to start the enable action.")]
        [SerializeField] protected float m_TimeAfterAwake = -1;
        [Tooltip("Toggle the enable disable on and off?")]
        [SerializeField] protected bool m_Toggle;
        [Tooltip("Enable of Disable?.")]
        [SerializeField] protected bool m_Enable;
        [Tooltip("The target to disable/enable.")]
        [SerializeField] protected GameObject m_Target;
        
        /// <summary>
        /// Cache component.
        /// </summary>
        protected void Awake()
        {
            if(m_TimeAfterAwake <= 0){return;}
            SchedulerManager.Schedule(() => gameObject.SetActive(m_Enable),m_TimeAfterAwake);
        }

        /// <summary>
        /// Enable dealyed.
        /// </summary>
        /// <param name="delay">The delay.</param>
        public void EnableDelayed(float delay)
        {
            SchedulerManager.Schedule(
                ()=> m_Target.SetActive(m_Toggle?!m_Target.activeSelf : m_Enable),delay);
        }
    }
}