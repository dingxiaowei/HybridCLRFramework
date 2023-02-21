/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Camera
{
    using UnityEngine;

    /// <summary>
    /// Billboard Allows you to make a game object look at the camera.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        [Tooltip("Use the lookAt function or make the object face the same way as the camera?")]
        [SerializeField] protected bool m_LookAt;
        
        protected Transform m_CameraTransform;
        
        /// <summary>
        /// GEt the camera transform.
        /// </summary>
        private void Awake()
        {
            m_CameraTransform = Camera.main.transform;
        }

        /// <summary>
        /// Look at the camera.
        /// </summary>
        void Update() 
        {
            if (m_LookAt) {
                transform.LookAt(m_CameraTransform.position, Vector3.up);
            } else {
                transform.forward = m_CameraTransform.forward;
            }
        }
    }
}