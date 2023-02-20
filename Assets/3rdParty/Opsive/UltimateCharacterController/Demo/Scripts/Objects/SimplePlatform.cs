using UnityEngine;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Demo.Objects
{
    /// <summary>
    /// Moves the platform between two points. This gives an example of using the Move With Object ability and moving on an object that is
    /// updated outside of the Kinematic Object Manager update loop.
    /// </summary>
    public class SimplePlatform : MonoBehaviour
    {
        [Tooltip("The position that the platform should move towards when the character is not on top of it.")]
        [SerializeField] protected Vector3 m_RestingPosition;
        [Tooltip("The position that the platform should move towards when the character is on top of it.")]
        [SerializeField] protected Vector3 m_ActivePosition;
        [Tooltip("The speed that the platform should move.")]
        [SerializeField] protected float m_MoveSpeed = 0.05f;

        private Transform m_Transform;
        private MoveWithObject m_MoveWithObjectAbility;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;
        }

        /// <summary>
        /// Teleport the character to the specified destination.
        /// </summary>
        /// <param name="other">The collider that entered the trigger. May or may not be a character.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, 1 << LayerManager.Character)) {
                return;
            }

            UltimateCharacterLocomotion characterLocomotion;
            if ((characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>()) == null) {
                return;
            }

            m_MoveWithObjectAbility = characterLocomotion.GetAbility<MoveWithObject>();
            if (m_MoveWithObjectAbility == null) {
                return;
            }

            m_MoveWithObjectAbility.Target = m_Transform;
        }

        /// <summary>
        /// Moves the platform.
        /// </summary>
        private void FixedUpdate()
        {
            // Move towards the active position when the ability reference is not null. This will be set within OnTriggerEnter/Exit.
            m_Transform.position = Vector3.MoveTowards(m_Transform.position, (m_MoveWithObjectAbility != null ? m_ActivePosition : m_RestingPosition), m_MoveSpeed);
        }

        /// <summary>
        /// An object has exited the trigger.
        /// </summary>
        /// <param name="other">The collider that entered the trigger. May or may not be a character.</param>
        private void OnTriggerExit(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, 1 << LayerManager.Character) || m_MoveWithObjectAbility == null) {
                return;
            }

            m_MoveWithObjectAbility.Target = null;
            m_MoveWithObjectAbility = null;
        }
    }
}