/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Objects
{
    using UnityEngine;

    /// <summary>
    /// A particle stream crate will reset its position when the crate reactivates.
    /// </summary>
    public class ParticleStreamCrate : MonoBehaviour
    {
        private Rigidbody m_Rigibody;

        private Vector3 m_Position;
        private Quaternion m_Rotation;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Rigibody = GetComponent<Rigidbody>();

            m_Position = transform.position;
            m_Rotation = transform.rotation;
        }

        /// <summary>
        /// The crate has been enabled.
        /// </summary>
        private void OnEnable()
        {
            m_Rigibody.velocity = Vector3.zero;
            m_Rigibody.angularVelocity = Vector3.zero;

            transform.position = m_Position;
            transform.rotation = m_Rotation;
        }
    }
}