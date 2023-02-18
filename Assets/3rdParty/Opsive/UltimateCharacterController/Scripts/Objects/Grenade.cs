/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.SurfaceSystem;
    using UnityEngine;

    /// <summary>
    /// The Projectile component moves a Destructible object along the specified path. Can apply damage at the collision point.
    /// </summary>
    public class Grenade : Destructible
    {
        [Tooltip("The length of time before the grenade destructs.")]
        [SerializeField] protected float m_Lifespan = 5;
        [Tooltip("A reference to the pin that is removed.")]
        [SerializeField] protected Transform m_Pin;

        public float Lifespan { get { return m_Lifespan; } set { m_Lifespan = value; } }
        public Transform Pin { get { return m_Pin; } set { m_Pin = value; } }

        protected ScheduledEventBase m_ScheduledDeactivation;
        private Transform m_PinParent;
        private Vector3 m_PinLocalPosition;
        private Quaternion m_PinLocalRotation;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Remember the pin location so it can be reattached.
            if (m_Pin != null) {
                m_PinParent = m_Pin.parent;
                m_PinLocalPosition = m_Pin.localPosition;
                m_PinLocalRotation = m_Pin.localRotation;
            }
        }

        /// <summary>
        /// The grenade has been enabled.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            DetachAttachPin(null);
        }

        /// <summary>
        /// Initializes the object.
        /// </summary>
        /// <param name="velocity">The velocity to apply.</param>
        /// <param name="torque">The torque to apply.</param>
        /// <param name="damageAmount">The amount of damage to apply to the hit object.</param>
        /// <param name="impactForce">The amount of force to apply to the hit object.</param>
        /// <param name="impactForceFrames">The number of frames to add the force to.</param>
        /// <param name="impactLayers">The layers that the projectile can impact with.</param>
        /// <param name="impactStateName">The name of the state to activate upon impact.</param>
        /// <param name="impactStateDisableTimer">The number of seconds until the impact state is disabled.</param>
        /// <param name="surfaceImpact">A reference to the Surface Impact triggered when the object hits an object.</param>
        /// <param name="originator">The object that instantiated the trajectory object.</param>
        /// <param name="originatorCollisionCheck">Should a collision check against the originator be performed?</param>
        public virtual void Initialize(Vector3 velocity, Vector3 torque, float damageAmount, float impactForce, int impactForceFrames, LayerMask impactLayers,
                                     string impactStateName, float impactStateDisableTimer, SurfaceImpact surfaceImpact, GameObject originator, bool originatorCollisionCheck)
        {
            InitializeDestructibleProperties(damageAmount, impactForce, impactForceFrames, impactLayers, impactStateName, impactStateDisableTimer, surfaceImpact);

            base.Initialize(velocity, torque, originator, originatorCollisionCheck);
        }

        /// <summary>
        /// The grenade should start to cook.
        /// </summary>
        /// <param name="originator">The object that instantiated the trajectory object.</param>
        public void StartCooking(GameObject originator)
        {
            SetOriginator(originator, Vector3.up);

            // The grenade should destruct after a specified amount of time.
            m_ScheduledDeactivation = Scheduler.Schedule(m_Lifespan, Deactivate);
        }

        /// <summary>
        /// Detaches or attach the pin.
        /// </summary>
        /// <param name="attachTransform">The transform that the pin should be attached to. If null the pin will move back to the starting location.</param>
        public void DetachAttachPin(Transform attachTransform)
        {
            if (m_Pin == null || m_PinParent == null) {
                return;
            }

            if (attachTransform != null) {
                m_Pin.parent = attachTransform;
            } else { // Attach the pin back to the original transform.
                m_Pin.parent = m_PinParent;
                m_Pin.localPosition = m_PinLocalPosition;
                m_Pin.localRotation = m_PinLocalRotation;
            }
        }

        /// <summary>
        /// The grenade has reached its lifespan.
        /// </summary>
        protected void Deactivate()
        {
            Scheduler.Cancel(m_ScheduledDeactivation);

            InitializeComponentReferences(); // The grenade may explode before Awake is called.

            // Change the layer of the GameObject so the explosion doesn't detect the grenade when performing its overlap check.
            var prevLayer = m_GameObject.layer;
            m_GameObject.layer = LayerManager.IgnoreRaycast;
            Destruct(null);
            m_GameObject.layer = prevLayer;
        }
    }
}