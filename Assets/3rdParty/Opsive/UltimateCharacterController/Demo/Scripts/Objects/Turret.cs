/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Objects
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Game;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.UltimateCharacterController.Networking;
    using Opsive.UltimateCharacterController.Networking.Game;
#endif
    using Opsive.UltimateCharacterController.Objects;
    using Opsive.UltimateCharacterController.SurfaceSystem;
    using Opsive.UltimateCharacterController.Traits;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// A simple turret which will fire a projectile towards the character. This turret is setup for the demo scene and will likely require modifications if used in other areas.
    /// </summary>
    public class Turret : MonoBehaviour
    {
        [Tooltip("The object that rotates on the turret.")]
        [SerializeField] protected Transform m_TurretHead;
        [Tooltip("The speed at which the head rotates.")]
        [SerializeField] protected float m_RotationSpeed = 5;

        [Tooltip("The location that the projectile should be fired.")]
        [SerializeField] protected Transform m_FireLocation;
        [Tooltip("The distance in which the turret can start firing.")]
        [SerializeField] protected float m_FireRange = 10;
        [Tooltip("The delay until the turret will fire again.")]
        [SerializeField] protected float m_FireDelay = 0.5f;

        [Tooltip("The projectile that is fired.")]
        [SerializeField] protected GameObject m_Projectile;
        [Tooltip("The magnitude of the projectile velocity when fired. The direction is determined by the fire direction.")]
        [SerializeField] protected float m_VelocityMagnitude = 10;
        [Tooltip("A LayerMask of the layers that can be hit when fired at.")]
        [SerializeField] protected LayerMask m_ImpactLayers = ~(1 << LayerManager.IgnoreRaycast | 1 << LayerManager.TransparentFX | 1 << LayerManager.UI | 1 << LayerManager.Overlay);
        [Tooltip("The amount of damage to apply to the hit object.")]
        [SerializeField] protected float m_DamageAmount = 10;
        [Tooltip("How much force to apply to the hit object.")]
        [SerializeField] protected float m_ImpactForce = 0.05f;
        [Tooltip("The number of frames to add the force to.")]
        [SerializeField] protected int m_ImpactForceFrames = 1;
        [Tooltip("The Surface Impact triggered when the weapon hits an object.")]
        [SerializeField] protected SurfaceImpact m_SurfaceImpact;

        [Tooltip("Optionally specify a muzzle flash that should appear when the turret is fired.")]
        [SerializeField] protected GameObject m_MuzzleFlash;
        [Tooltip("The location that the muzzle flash should spawn.")]
        [SerializeField] protected Transform m_MuzzleFlashLocation;

        [Tooltip("Optionally specify an audio clip that should play when the turret is fired.")]
        [SerializeField] protected AudioClip m_FireAudioClip;

        private GameObject m_GameObject;
        private Transform m_Transform;
        private AudioSource m_AudioSource;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        private INetworkInfo m_NetworkInfo;
#endif

        private Transform m_Target;
        private Health m_Health;
        private float m_LastFireTime;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_GameObject = gameObject;
            m_Transform = transform;
            m_AudioSource = GetComponent<AudioSource>();
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            m_NetworkInfo = GetComponent<INetworkInfo>();
#endif

            // A turret head is required.
            if (m_TurretHead == null) {
                m_TurretHead = m_Transform;
            }
        }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        /// <summary>
        /// Determine if the object should be enabled on the network.
        /// </summary>
        private void Start()
        {
            if (m_NetworkInfo != null && !m_NetworkInfo.IsLocalPlayer()) {
                enabled = false;
            }
        }
#endif

        /// <summary>
        /// The turret has been enabled.
        /// </summary>
        private void OnEnable()
        {
            m_LastFireTime = Time.time;
        }

        /// <summary>
        /// Rotates the turret head and attacks if the character is within range.
        /// </summary>
        private void Update()
        {
            if (m_Target == null) {
                return;
            }

            RotateTowardsTarget();
            CheckForAttack();
        }

        /// <summary>
        /// Keep facing the target so the turret can fire at any time.
        /// </summary>
        public void RotateTowardsTarget()
        {
            var targetRotation = Quaternion.Euler(0, Quaternion.LookRotation(m_TurretHead.position - m_Target.position).eulerAngles.y, 0);
            m_TurretHead.rotation = Quaternion.Slerp(m_TurretHead.rotation, targetRotation, m_RotationSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Determine if the turret can attack.
        /// </summary>
        public void CheckForAttack()
        {
            // The turret can attack if it hasn't fired recently and the target is in front of the turret.
            if (m_LastFireTime + m_FireDelay < Time.time && (m_Transform.position - m_Target.position).magnitude < m_FireRange && (m_Health == null || m_Health.Value > 0)) {
                Fire();
            }
        }

        /// <summary>
        /// Does the actual fire.
        /// </summary>
        public void Fire()
        {
            m_LastFireTime = Time.time;

            // Spawn a projectile which will move in the direction that the turret is facing
            var projectile = ObjectPool.Instantiate(m_Projectile, m_FireLocation.position, m_Transform.rotation).GetCachedComponent<Projectile>();
            projectile.Initialize(m_FireLocation.forward * m_VelocityMagnitude, Vector3.zero, m_DamageAmount, m_ImpactForce, m_ImpactForceFrames,
                                    m_ImpactLayers, string.Empty, 0, m_SurfaceImpact, m_GameObject);
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null) {
                NetworkObjectPool.NetworkSpawn(m_Projectile, projectile.gameObject, true);
            }
#endif

            // Spawn a muzzle flash.
            if (m_MuzzleFlash) {
                var muzzleFlash = ObjectPool.Instantiate(m_MuzzleFlash, m_MuzzleFlashLocation.position, m_MuzzleFlashLocation.rotation, m_Transform).GetCachedComponent<MuzzleFlash>();
                muzzleFlash.Show(null, 0, true, null);
            }

            // Play a firing sound.
            if (m_FireAudioClip != null) {
                m_AudioSource.clip = m_FireAudioClip;
                m_AudioSource.Play();
            }
        }

        /// <summary>
        /// An object has entered the trigger.
        /// </summary>
        /// <param name="other">The object that entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (m_Target != null || !MathUtility.InLayerMask(other.gameObject.layer, 1 << LayerManager.Character)) {
                return;
            }

            var characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            m_Target = characterLocomotion.transform;
            m_Health = characterLocomotion.GetComponent<Health>();
        }

        /// <summary>
        /// An object has exited the trigger.
        /// </summary>
        /// <param name="other">The collider that exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject != m_Target) {
                return;
            }

            m_Target = null;
        }
    }
}