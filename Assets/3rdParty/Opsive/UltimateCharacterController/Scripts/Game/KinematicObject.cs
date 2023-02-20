using UnityEngine;

namespace Opsive.UltimateCharacterController.Game
{
    /// <summary>
    /// The Kinematic Object component allows an object to be moved outside of the Kinematic Object Manager loop while still being tracked by the Kinematic Object Manager.
    /// This component should be used with the Move With Object ability:
    /// https://opsive.com/support/documentation/ultimate-character-controller/character/abilities/included-abilities/move-with-object/
    /// </summary>
    public class KinematicObject : MonoBehaviour, IKinematicObject
    {
        private Transform m_Transform;
        private Vector3 m_LastPosition;
        private Quaternion m_LastRotation;

        private int m_KinematicObjectIndex;
        public int KinematicObjectIndex { set { m_KinematicObjectIndex = value; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;
        }

        /// <summary>
        /// Registers the object with the Kinematic Object Manager.
        /// </summary>
        public void OnEnable()
        {
            m_KinematicObjectIndex = KinematicObjectManager.RegisterKinematicObject(this);
            m_LastPosition = m_Transform.position;
            m_LastRotation = m_Transform.rotation;
        }

        /// <summary>
        /// Updates the position/rotation of the object. The Kinematic Object component should execute after the object has been moved.
        /// </summary>
        public void FixedUpdate()
        {
            m_LastPosition = m_Transform.position;
            m_LastRotation = m_Transform.rotation;
        }

        /// <summary>
        /// Sets up the object to be moved by the Kinematic Object Manager.
        /// </summary>
        public void Move()
        {
            m_Transform.position = m_LastPosition;
            m_Transform.rotation = m_LastRotation;
        }

        /// <summary>
        /// Unregisters the object with the Kinematic Object Manager.
        /// </summary>
        public void OnDisable()
        {
            KinematicObjectManager.UnregisterKinematicObject(m_KinematicObjectIndex);
        }
    }
}