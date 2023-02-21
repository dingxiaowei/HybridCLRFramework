/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Camera
{
    using Cinemachine;
    using Dypsloom.Shared.Utility;
    using UnityEngine;

    public class CameraTransition : MonoBehaviour
    {
        [Tooltip("The virtual camera.")]
        [SerializeField] protected CinemachineVirtualCamera m_Vcam;
        [Tooltip("The priority the camera will have at the start of the transistion.")]
        [SerializeField] protected int m_Priority = 20;
        [Tooltip("The time to transition back, 0 or lower won't transition back.")]
        [SerializeField] protected float m_TransitionBackTime = 2;

        protected int m_PreviousPriority;

        /// <summary>
        /// Start the camera transition.
        /// </summary>
        public void Transition()
        {
            m_PreviousPriority = m_Vcam.Priority;
            
            m_Vcam.Priority = m_Priority;
            
            if (m_TransitionBackTime <= 0) {
                return;
            }
            
            SchedulerManager.Schedule(
                ()=> m_Vcam.Priority = m_PreviousPriority,m_TransitionBackTime);
        }
    }
}