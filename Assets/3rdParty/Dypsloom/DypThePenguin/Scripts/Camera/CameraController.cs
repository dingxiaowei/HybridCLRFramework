/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Camera
{
    using UnityEngine;

    /// <summary>
    /// A camera controller that follows the transform referenced.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Tooltip("The transform that the camera will follow.")]
        [SerializeField] protected Transform m_Follow;

        protected Vector3 m_StartOffset;
    
        /// <summary>
        /// Follow the transform.
        /// </summary>
        void Start()
        {
            if (m_Follow == null) {
                m_Follow = GameObject.FindWithTag("Player").transform;
            }
        
            m_StartOffset = transform.position - m_Follow.position;
        }
    
        /// <summary>
        /// Late Update to remove hiccups.
        /// </summary>
        void LateUpdate () 
        {
            transform.position = m_Follow.position + m_StartOffset;
        }
    }
}
