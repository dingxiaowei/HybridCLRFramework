/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Traits
{
    /// <summary>
    /// Sets the ConstantForce component to the specified gravity direction.
    /// </summary>
    [RequireComponent(typeof(ConstantForce))]
    public class DirectionalConstantForce : MonoBehaviour
    {
        [Tooltip("The normalized direction of the constant force.")]
        [SerializeField] protected Vector3 m_Direction = new Vector3(0, -1, 0);
        [Tooltip("The magnitude of the starting constant force.")]
        [SerializeField] protected float m_StartMagnitude = 1;
        [Tooltip("The amount of magnitude to add each frame.")]
        [SerializeField] protected float m_FrameMagnitudeAddition = 0.2f;

        private float m_Magnitude;

        public Vector3 Direction { get { return m_Direction; } set { m_Direction = value; m_ConstantForce.force = m_Direction * m_StartMagnitude; } }
        public float Magnitude { get { return m_StartMagnitude; } set { m_StartMagnitude = value; m_ConstantForce.force = m_Direction * m_StartMagnitude; } }

        private ConstantForce m_ConstantForce;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_ConstantForce = GetComponent<ConstantForce>();
        }

        /// <summary>
        /// Reset the magnitude.
        /// </summary>
        private void OnEnable()
        {
            m_Magnitude = m_StartMagnitude;
            // The component doesn't need to update if there is no per frame force.
            if (m_FrameMagnitudeAddition == 0) {
                enabled = false;
            }
        }

        /// <summary>
        /// Add the per frame magnitude to the constant force.
        /// </summary>
        private void Update()
        {
            m_ConstantForce.force = m_Direction * m_Magnitude;
            m_Magnitude += m_FrameMagnitudeAddition;
        }
    }
}