/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Enables or disables the ragdoll colliders. Can be started when the character dies.
    /// </summary>
    [DefaultStartType(AbilityStartType.Manual)]
    [DefaultState("Death")]
    [DefaultAllowPositionalInput(false)]
    [DefaultAllowRotationalInput(false)]
    public class Ragdoll : Ability
    {
        [Tooltip("Should the ability start when the character dies?")]
        [HideInInspector] [SerializeField] protected bool m_StartOnDeath = true;
        [Tooltip("Specifies the delay after the ability starts that the character should turn into a ragdoll.")]
        [HideInInspector] [SerializeField] protected float m_StartDelay;
        [Tooltip("The layer that the colliders should switch to when the ragdoll is active. This should be set to VisualEffect if other characters should not step over the current" +
            "character when the ability is active.")]
        [HideInInspector] [SerializeField] protected int m_RagdollLayer = LayerManager.Character;
        [Tooltip("The layer that the colliders should switch to when the ragdoll is inactive.")]
        [HideInInspector] [SerializeField] protected int m_InactiveRagdollLayer = LayerManager.SubCharacter;
        [Tooltip("The amount of force to add to the camera. This value will be multiplied by the death force magnitude.")]
        [HideInInspector] [SerializeField] protected Vector3 m_CameraRotationalForce = new Vector3(0, 0, 0.75f);

        public bool StartOnDeath { get { return m_StartOnDeath; } set { m_StartOnDeath = value; } }
        public float StartDelay { get { return m_StartDelay; } set { m_StartDelay = value; } }
        public int RagdollLayer { get { return m_RagdollLayer; } set { m_RagdollLayer = value; } }
        public int InactiveRagdollLayer { get { return m_InactiveRagdollLayer; } set { m_InactiveRagdollLayer = value; } }
        public Vector3 CameraRotationalForce { get { return m_CameraRotationalForce; } set { m_CameraRotationalForce = value; } }
        
        private Rigidbody[] m_Rigidbodies;
        private GameObject[] m_RigidbodyGameObjects;

        private Vector3 m_Force;
        private Vector3 m_Position;
        private bool m_FromDeath;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        private object[] m_StartData;
#endif

        [NonSerialized] public Vector3 Force { get { return m_Force; } set { m_Force = value; } }
        [NonSerialized] public Vector3 Position { get { return m_Position; } set { m_Position = value; } }
        public override bool CanStayActivatedOnDeath { get { return true; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            var characterRigidbody = m_GameObject.GetCachedComponent<Rigidbody>();
            var rigidbodies = m_GameObject.GetComponentsInChildren<Rigidbody>();
            // The character's Rigidbody should be ignored.
            var index = 0;
            m_Rigidbodies = new Rigidbody[rigidbodies.Length - 1];
            m_RigidbodyGameObjects = new GameObject[m_Rigidbodies.Length];
            for (int i = 0; i < rigidbodies.Length; ++i) {
                if (rigidbodies[i] == characterRigidbody) {
                    continue;
                }

                m_Rigidbodies[index] = rigidbodies[i];
                m_RigidbodyGameObjects[index] = rigidbodies[i].gameObject;
                index++;
            }

            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_GameObject, "OnWillRespawn", OnRespawn);

            EnableRagdoll(false, Vector3.zero, Vector3.zero);
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            EventHandler.ExecuteEvent(m_GameObject, "OnCameraRotationalForce", m_CameraRotationalForce * (m_FromDeath ? m_Force.magnitude : 1));

            Scheduler.ScheduleFixed(m_StartDelay, EnableRagdoll, true, m_Force, m_Position);

            m_FromDeath = false;
        }

        /// <summary>
        /// Enables or disables the ragdoll.
        /// </summary>
        /// <param name="enable">Should the ragdoll be enabled?</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="position">The position of the force.</param>
        private void EnableRagdoll(bool enable, Vector3 force, Vector3 position)
        {
            // When the ragdoll is active the animator should be disabled. An active animator will prevent the ragdoll from playing. 
            if (m_AnimatorMonitor != null) {
                m_AnimatorMonitor.EnableAnimator(!enable);
            }
            if (enable) {
                // When the ragdoll is enabled the character should no longer have any forces applied - it's up to the ragdoll to apply the forces now.
                m_CharacterLocomotion.ResetRotationPosition();
            }

            // The GameObject layer is going to change - enable the collision layer so it can be disabled again after the layer has been set. This will allow the controller
            // to cache the correct layers.
            m_CharacterLocomotion.EnableColliderCollisionLayer(true);

            // Add the ragdoll force.
            for (int i = 0; i < m_Rigidbodies.Length; ++i) {
                m_Rigidbodies[i].useGravity = enable;
                m_Rigidbodies[i].collisionDetectionMode = enable ? CollisionDetectionMode.ContinuousSpeculative : CollisionDetectionMode.Discrete;
                m_Rigidbodies[i].isKinematic = !enable;
                m_Rigidbodies[i].constraints = (enable ? RigidbodyConstraints.None : RigidbodyConstraints.FreezeAll);
                m_RigidbodyGameObjects[i].layer = enable ? m_RagdollLayer : m_InactiveRagdollLayer;
                if (enable) {
                    m_Rigidbodies[i].AddForceAtPosition(force, position, ForceMode.Force);
                }
            }
            m_CharacterLocomotion.EnableColliderCollisionLayer(false);

            // The main character colliders do not contribute to the ragdoll.
            for (int i = 0; i < m_CharacterLocomotion.ColliderCount; ++i) {
                m_CharacterLocomotion.Colliders[i].enabled = !enable;
            }
        }

        /// <summary>
        /// The character has died. Start the ability if requested.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            // The ability may not need to start from the death event.
            if (!m_StartOnDeath || !Enabled) {
                return;
            }

            m_Force = force * MathUtility.RigidbodyForceMultiplier;
            m_Position = position;
            m_FromDeath = true;

            StartAbility();
        }

        /// <summary>
        /// The character has respawned. Stop the ability if necessary.
        /// </summary>
        private void OnRespawn()
        {
            // The ability may not have been started from the death event.
            if (!m_StartOnDeath || !Enabled) {
                return;
            }

            StopAbility();
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            EnableRagdoll(false, Vector3.zero, Vector3.zero);

            base.AbilityStopped(force);

            // Snap the animator back into position.
            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterSnapAnimator");
        }

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.UnregisterEvent(m_GameObject, "OnWillRespawn", OnRespawn);
        }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        /// <summary>
        /// Returns any data required to start the ability.
        /// </summary>
        /// <returns>Any data required to start the ability.</returns>
        public override object[] GetNetworkStartData()
        {
            if (!IsActive) {
                return null;
            }

            if (m_StartData == null) {
                m_StartData = new object[m_Rigidbodies.Length * 4];
            }
            // Save the rigidbody values so they can be sent across the network.
            for (int i = 0; i < m_Rigidbodies.Length; ++i) {
                m_StartData[(i * 3)] = m_Rigidbodies[i].position;
                m_StartData[(i * 3) + 1] = m_Rigidbodies[i].rotation;
                m_StartData[(i * 3) + 2] = m_Rigidbodies[i].velocity;
                m_StartData[(i * 3) + 3] = m_Rigidbodies[i].angularVelocity;
            }
            return m_StartData; 
        }

        /// <summary>
        /// Sets the start data from the network.
        /// </summary>
        /// <param name="startData">The data required to start the ability.</param>
        public override void SetNetworkStartData(object[] startData)
        {
            m_Force = Vector3.zero;
            m_Position = Vector3.zero;
            m_CharacterLocomotion.TryStartAbility(this, true, true);

            // Restore the rigidbody momentum.
            for (int i = 0; i < m_Rigidbodies.Length; ++i) {
                m_Rigidbodies[i].position = (Vector3)m_StartData[(i * 3)];
                m_Rigidbodies[i].rotation = (Quaternion)m_StartData[(i * 3) + 1];
                m_Rigidbodies[i].velocity = (Vector3)m_StartData[(i * 3) + 2];
                m_Rigidbodies[i].angularVelocity = (Vector3)m_StartData[(i * 3) + 3];
            }
        }
#endif
    }
}