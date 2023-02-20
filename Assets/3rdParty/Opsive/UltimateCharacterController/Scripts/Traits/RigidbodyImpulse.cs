/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Traits
{
    /// <summary>
    /// Applies an impulse to the Rigidbody when enabled.
    /// </summary>
    public class RigidbodyImpulse : MonoBehaviour
    {
        [Tooltip("Should the force be applied in the local space?")]
        [SerializeField] protected bool m_LocalForce = true;
        [Tooltip("The force to apply to the Rigidbody.")]
        [SerializeField] protected MinMaxVector3 m_Force = new MinMaxVector3(new Vector3(0, 5, 0), new Vector3(0, 5, 0));
        [Tooltip("Should the torque be applied in the local space?")]
        [SerializeField] protected bool m_LocalTorque = true;
        [Tooltip("The torque to apply to the Rigidbody.")]
        [SerializeField] protected MinMaxVector3 m_Torque;

        private Rigidbody m_Rigidbody;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Add the impulse force.
        /// </summary>
        private void OnEnable()
        {
            if (m_LocalForce) {
                m_Rigidbody.AddRelativeForce(m_Force.RandomValue, ForceMode.Impulse);
            } else {
                m_Rigidbody.AddForce(m_Force.RandomValue, ForceMode.Impulse);
            }

            if (m_LocalTorque) {
                m_Rigidbody.AddRelativeTorque(m_Torque.RandomValue);
            } else {
                m_Rigidbody.AddTorque(m_Torque.RandomValue);
            }
        }
    }
}