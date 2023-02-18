/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Objects
{
    using Opsive.UltimateCharacterController.Traits;
    using UnityEngine;

    /// <summary>
    /// Instantly kills the character if the character moves beneath the moving platform as it is moving down.
    /// </summary>
    public class MovingPlatformDeathZone : MonoBehaviour
    {
        private Transform m_Transform;
        private Vector3 m_PrevPosition;
        private bool m_DownwardMovement;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;
            m_PrevPosition = m_Transform.position;
        }

        /// <summary>
        /// Detect if the platform is moving downward.
        /// </summary>
        private void FixedUpdate()
        {
            m_DownwardMovement = m_Transform.InverseTransformDirection(m_Transform.position - m_PrevPosition).y < 0;
            m_PrevPosition = m_Transform.position;
        }

        /// <summary>
        /// An
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            // The platform has to be moving downward in order to kill the player.
            if (!m_DownwardMovement) {
                return;
            }

            // Kill the character.
            var health = other.GetComponentInParent<CharacterHealth>();
            if (health == null) {
                return;
            }

            health.ImmediateDeath(m_Transform.position, Vector3.down, (m_Transform.position - m_PrevPosition).magnitude);
        }
    }
}
