/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Audio;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Objects;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
using Opsive.UltimateCharacterController.Networking;
using Opsive.UltimateCharacterController.Networking.Traits;
#endif
using Opsive.UltimateCharacterController.StateSystem;
using Opsive.UltimateCharacterController.Utility;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Traits
{
    /// <summary>
    /// Adds health and a shield to the object.
    /// </summary>
    [RequireComponent(typeof(AttributeManager))]
    public class Health : StateBehavior
    {
        [Tooltip("Is the object invincible?")]
        [SerializeField] protected bool m_Invincible;
        [Tooltip("The amount of time that the object is invincible after respawning.")]
        [SerializeField] protected float m_TimeInvincibleAfterSpawn;
        [Tooltip("The name of the health attribute.")]
        [SerializeField] protected string m_HealthAttributeName = "Health";
        [Tooltip("The name of the shield attribute.")]
        [SerializeField] protected string m_ShieldAttributeName;
        [Tooltip("The list of Colldiers that should apply a multiplier when damaged.")]
        [SerializeField] protected Hitbox[] m_Hitboxes;
        [Tooltip("The maximum number of colliders that can be detected when determining if a hitbox was damaged.")]
        [SerializeField] protected int m_MaxHitboxCollisionCount = 10;
        [Tooltip("Any object that should spawn when the object dies.")]
        [SerializeField] protected GameObject[] m_SpawnedObjectsOnDeath;
        [Tooltip("Any object that should be destroyed when the object dies.")]
        [SerializeField] protected GameObject[] m_DestroyedObjectsOnDeath;
        [Tooltip("Should the object be deactivated on death?")]
        [SerializeField] protected bool m_DeactivateOnDeath;
        [Tooltip("If DeactivateOnDeath is enabled, specify a delay for the object to be deactivated.")]
        [SerializeField] protected float m_DeactivateOnDeathDelay;
        [Tooltip("The layer that the GameObject should switch to upon death.")]
        [SerializeField] protected LayerMask m_DeathLayer;
        [Tooltip("A set of AudioClips that can be played when the object takes damage.")]
        [HideInInspector] [SerializeField] protected AudioClipSet m_TakeDamageAudioClipSet = new AudioClipSet();
        [Tooltip("A set of AudioClips that can be played when the object is healed.")]
        [HideInInspector] [SerializeField] protected AudioClipSet m_HealAudioClipSet = new AudioClipSet();
        [Tooltip("A set of AudioClips that can be played when the object dies.")]
        [HideInInspector] [SerializeField] protected AudioClipSet m_DeathAudioClipSet = new AudioClipSet();
        [Tooltip("Unity event invoked when taking damage.")]
        [SerializeField] protected UnityFloatVector3Vector3GameObjectEvent m_OnDamageEvent;
        [Tooltip("Unity event invoked when healing.")]
        [SerializeField] protected UnityFloatEvent m_OnHealEvent;
        [Tooltip("Unity event invoked when the object dies.")]
        [SerializeField] protected UnityVector3Vector3GameObjectEvent m_OnDeathEvent;

        public bool Invincible { get { return m_Invincible; } set { m_Invincible = value; } }
        public float TimeInvincibleAfterSpawn { get { return m_TimeInvincibleAfterSpawn; } set { m_TimeInvincibleAfterSpawn = value; } }
        public string HealthAttributeName { get { return m_HealthAttributeName; }
            set
            {
                m_HealthAttributeName = value;
                if (Application.isPlaying) {
                    if (!string.IsNullOrEmpty(m_HealthAttributeName)) {
                        m_HealthAttribute = m_AttributeManager.GetAttribute(m_HealthAttributeName);
                    } else {
                        m_HealthAttribute = null;
                    }
                }
            }
        }
        public string ShieldAttributeName
        {
            get { return m_ShieldAttributeName; }
            set
            {
                m_ShieldAttributeName = value;
                if (Application.isPlaying) {
                    if (!string.IsNullOrEmpty(m_ShieldAttributeName)) {
                        m_ShieldAttribute = m_AttributeManager.GetAttribute(m_ShieldAttributeName);
                    } else {
                        m_ShieldAttribute = null;
                    }
                }
            }
        }
        [NonSerialized] public Hitbox[] Hitboxes { get { return m_Hitboxes; } set { m_Hitboxes = value; } }
        public int MaxHitboxCollisionCount { get { return m_MaxHitboxCollisionCount; } set { m_MaxHitboxCollisionCount = value; } }
        public GameObject[] SpawnedObjectsOnDeath { get { return m_SpawnedObjectsOnDeath; } set { m_SpawnedObjectsOnDeath = value; } }
        public GameObject[] DestroyedObjectsOnDeath { get { return m_DestroyedObjectsOnDeath; } set { m_DestroyedObjectsOnDeath = value; } }
        public bool DeactivateOnDeath { get { return m_DeactivateOnDeath; } set { m_DeactivateOnDeath = value; } }
        public float DeactivateOnDeathDelay { get { return m_DeactivateOnDeathDelay; } set { m_DeactivateOnDeathDelay = value; } }
        public LayerMask DeathLayer { get { return m_DeathLayer; } set { m_DeathLayer = value; } }
        public AudioClipSet TakeDamageAudioClipSet { get { return m_TakeDamageAudioClipSet; } set { m_TakeDamageAudioClipSet = value; } }
        public AudioClipSet HealAudioClipSet { get { return m_HealAudioClipSet; } set { m_HealAudioClipSet = value; } }
        public AudioClipSet DeathAudioClipSet { get { return m_DeathAudioClipSet; } set { m_DeathAudioClipSet = value; } }
        public UnityFloatVector3Vector3GameObjectEvent OnDamageEvent { get { return m_OnDamageEvent; } set { m_OnDamageEvent = value; } }
        public UnityFloatEvent OnHealEvent { get { return m_OnHealEvent; } set { m_OnHealEvent = value; } }
        public UnityVector3Vector3GameObjectEvent OnDeathEvent { get { return m_OnDeathEvent; } set { m_OnDeathEvent = value; } }

        protected GameObject m_GameObject;
        protected Transform m_Transform;
        private IForceObject m_ForceObject;
        private Rigidbody m_Rigidbody;
        private AttributeManager m_AttributeManager;
        private Attribute m_HealthAttribute;
        private Attribute m_ShieldAttribute;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        private INetworkInfo m_NetworkInfo;
        private INetworkHealthMonitor m_NetworkHealthMonitor;
#endif

        private float m_SpawnTime;
        private int m_AliveLayer;
        private Dictionary<Collider, Hitbox> m_ColliderHitboxMap;
        private RaycastHit[] m_RaycastHits;
        private UnityEngineUtility.RaycastHitComparer m_RaycastHitComparer;

        public float HealthValue { get { return (m_HealthAttribute != null ? m_HealthAttribute.Value : 0); } }
        public float ShieldValue { get { return (m_ShieldAttribute != null ? m_ShieldAttribute.Value : 0); } }
        public float Value { get { return HealthValue + ShieldValue; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_GameObject = gameObject;
            m_Transform = transform;
            m_ForceObject = m_GameObject.GetCachedComponent<IForceObject>();
            m_Rigidbody = m_GameObject.GetCachedComponent<Rigidbody>();
            m_AttributeManager = GetComponent<AttributeManager>();
            if (!string.IsNullOrEmpty(m_HealthAttributeName)) {
                m_HealthAttribute = m_AttributeManager.GetAttribute(m_HealthAttributeName);
            }
            if (!string.IsNullOrEmpty(m_ShieldAttributeName)) {
                m_ShieldAttribute = m_AttributeManager.GetAttribute(m_ShieldAttributeName);
            }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            m_NetworkInfo = m_GameObject.GetCachedComponent<INetworkInfo>();
            m_NetworkHealthMonitor = m_GameObject.GetCachedComponent<INetworkHealthMonitor>();
            if (m_NetworkInfo != null && m_NetworkHealthMonitor == null) {
                Debug.LogError("Error: The object " + m_GameObject.name + " must have a NetworkHealthMonitor component.");
            }
#endif

            if (m_Hitboxes != null && m_Hitboxes.Length > 0) {
                m_ColliderHitboxMap = new Dictionary<Collider, Hitbox>();
                for (int i = 0; i < m_Hitboxes.Length; ++i) {
                    m_ColliderHitboxMap.Add(m_Hitboxes[i].Collider, m_Hitboxes[i]);
                }
                m_RaycastHits = new RaycastHit[m_MaxHitboxCollisionCount];
                m_RaycastHitComparer = new UnityEngineUtility.RaycastHitComparer();
            }

            EventHandler.RegisterEvent(m_GameObject, "OnRespawn", OnRespawn);
        }

        /// <summary>
        /// The object has been damaged.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        public void Damage(float amount)
        {
            Damage(amount, m_Transform.position, Vector3.zero, 1, 0, 0, null, null);
        }

        /// <summary>
        /// The object has been damaged.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="direction">The direction that the object took damage from.</param>
        /// <param name="forceMagnitude">The magnitude of the force that is applied to the object.</param>
        public void Damage(float amount, Vector3 position, Vector3 direction, float forceMagnitude)
        {
            Damage(amount, position, direction, forceMagnitude, 1, 0, null, null);
        }

        /// <summary>
        /// The object has been damaged.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="direction">The direction that the object took damage from.</param>
        /// <param name="forceMagnitude">The magnitude of the force that is applied to the object.</param>
        /// <param name="radius">The radius of the explosive damage. If 0 then a non-exposive force will be used.</param>
        public void Damage(float amount, Vector3 position, Vector3 direction, float forceMagnitude, float radius)
        {
            Damage(amount, position, direction, forceMagnitude, 1, radius, null, null);
        }

        /// <summary>
        /// The object has been damaged.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="direction">The direction that the object took damage from.</param>
        /// <param name="forceMagnitude">The magnitude of the force that is applied to the object.</param>
        /// <param name="attacker">The GameObject that did the damage.</param>
        public void Damage(float amount, Vector3 position, Vector3 direction, float forceMagnitude, GameObject attacker)
        {
            Damage(amount, position, direction, forceMagnitude, 1, 0, attacker, null);
        }

        /// <summary>
        /// The object has been damaged.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="direction">The direction that the object took damage from.</param>
        /// <param name="forceMagnitude">The magnitude of the force that is applied to the object.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        /// <param name="radius">The radius of the explosive damage. If 0 then a non-exposive force will be used.</param>
        /// <param name="attacker">The GameObject that did the damage.</param>
        public void Damage(float amount, Vector3 position, Vector3 direction, float forceMagnitude, int frames, float radius, GameObject attacker)
        {
            Damage(amount, position, direction, forceMagnitude, frames, radius, attacker, null);
        }

        /// <summary>
        /// The object has been damaged.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="direction">The direction that the object took damage from.</param>
        /// <param name="forceMagnitude">The magnitude of the force that is applied to the object.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        /// <param name="radius">The radius of the explosive damage. If 0 then a non-explosive force will be used.</param>
        /// <param name="attacker">The GameObject that did the damage.</param>
        /// <param name="hitCollider">The Collider that was hit.</param>
        public void Damage(float amount, Vector3 position, Vector3 direction, float forceMagnitude, int frames, float radius, GameObject attacker, Collider hitCollider)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && !m_NetworkInfo.IsLocalPlayer()) {
                return;
            }
#endif

            // Don't take any damage if the object is invincible, already dead, or just spawned and is invincible for a small amount of time.
            if (m_Invincible || !IsAlive() || m_SpawnTime + m_TimeInvincibleAfterSpawn > Time.time || amount == 0) {
                return;
            }

            OnDamage(amount, position, direction, forceMagnitude, frames, radius, attacker, hitCollider);
        }

        /// <summary>
        /// The object has taken been damaged.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="direction">The direction that the object took damage from.</param>
        /// <param name="forceMagnitude">The magnitude of the force that is applied to the object.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        /// <param name="radius">The radius of the explosive damage. If 0 then a non-explosive force will be used.</param>
        /// <param name="attacker">The GameObject that did the damage.</param>
        /// <param name="hitCollider">The Collider that was hit.</param>
        public virtual void OnDamage(float amount, Vector3 position, Vector3 direction, float forceMagnitude, int frames, float radius, GameObject attacker, Collider hitCollider)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.IsServer()) {
                m_NetworkHealthMonitor.OnDamage(amount, position, direction, forceMagnitude, frames, radius, attacker, hitCollider);
            }
#endif

            // Add a multiplier if a particular collider was hit. Do not apply a multiplier if the damage is applied through a radius because multiple
            // collider are hit.
            if (radius == 0 && direction != Vector3.zero && hitCollider != null) {
                Hitbox hitbox;
                if (m_ColliderHitboxMap != null && m_ColliderHitboxMap.Count > 0) {
                    if (m_ColliderHitboxMap.TryGetValue(hitCollider, out hitbox)) {
                        amount *= hitbox.DamageMultiplier;
                    } else {
                        // The main collider may be overlapping child hitbox colliders. Perform one more raycast to ensure a hitbox collider shouldn't be hit.
                        float distance = 0.2f;
                        if (hitCollider is CapsuleCollider) {
                            distance = (hitCollider as CapsuleCollider).radius;
                        } else if (hitCollider is SphereCollider) {
                            distance = (hitCollider as SphereCollider).radius;
                        }

                        // The hitbox collider may be underneath the base collider. Fire a raycast to detemine if there are any colliders underneath the hit collider 
                        // that should apply a multiplier.
                        var hitCount = Physics.RaycastNonAlloc(position, direction, m_RaycastHits, distance,
                                        ~(1 << LayerManager.IgnoreRaycast | 1 << LayerManager.Overlay | 1 << LayerManager.VisualEffect), QueryTriggerInteraction.Ignore);
                        for (int i = 0; i < hitCount; ++i) {
                            var closestRaycastHit = QuickSelect.SmallestK(m_RaycastHits, hitCount, i, m_RaycastHitComparer);
                            if (closestRaycastHit.collider == hitCollider) {
                                continue;
                            }
                            // A new collider has been found - stop iterating if the hitbox map exists and use the hitbox multiplier.
                            if (m_ColliderHitboxMap.TryGetValue(closestRaycastHit.collider, out hitbox)) {
                                amount *= hitbox.DamageMultiplier;
                                hitCollider = hitbox.Collider;
                                break;
                            }
                        }
                    }
                }
            }

            // Apply the damage to the shield first because the shield can regenrate.
            if (m_ShieldAttribute != null && m_ShieldAttribute.Value > m_ShieldAttribute.MinValue) {
                var shieldAmount = Mathf.Min(amount, m_ShieldAttribute.Value - m_ShieldAttribute.MinValue);
                amount -= shieldAmount;
                m_ShieldAttribute.Value -= shieldAmount;
            }

            // Decrement the health by remaining amount after the shield has taken damage.
            if (m_HealthAttribute != null && m_HealthAttribute.Value > m_HealthAttribute.MinValue) {
                m_HealthAttribute.Value -= Mathf.Min(amount, m_HealthAttribute.Value - m_HealthAttribute.MinValue);
            }

            var force = direction * forceMagnitude;
            if (forceMagnitude > 0) {
                // Apply a force to the object.
                if (m_ForceObject != null) {
                    m_ForceObject.AddForce(force, frames);
                } else {
                    // Apply a force to the rigidbody if the object isn't a character.
                    if (m_Rigidbody != null && !m_Rigidbody.isKinematic) {
                        if (radius == 0) {
                            m_Rigidbody.AddForceAtPosition(force * MathUtility.RigidbodyForceMultiplier, position);
                        } else {
                            m_Rigidbody.AddExplosionForce(force.magnitude * MathUtility.RigidbodyForceMultiplier, position, radius);
                        }
                    }
                }
            }

            // Let other interested objects know that the object took damage.
            EventHandler.ExecuteEvent<float, Vector3, Vector3, GameObject, Collider>(m_GameObject, "OnHealthDamage", amount, position, force, attacker, hitCollider);
            if (m_OnDamageEvent != null) {
                m_OnDamageEvent.Invoke(amount, position, force, attacker);
            }

            // The object is dead when there is no more health or shield.
            if (!IsAlive()) {
                Die(position, force, attacker);
            } else {
                // Play any take damage audio if the object did not die. If the object died then the death audio will play.
                m_TakeDamageAudioClipSet.PlayAudioClip(m_GameObject);
            }
        }
        
        /// <summary>
        /// Is the object currently alive?
        /// </summary>
        /// <returns>True if the object is currently alive.</returns>
        public bool IsAlive()
        {
            return (m_HealthAttribute != null && m_HealthAttribute.Value > m_HealthAttribute.MinValue) || 
                   (m_ShieldAttribute != null && m_ShieldAttribute.Value > m_ShieldAttribute.MinValue);
        }

        /// <summary>
        /// The object is no longer alive.
        /// </summary>
        /// <param name="position">The position of the damage.</param>
        /// <param name="force">The amount of force applied to the object while taking the damage.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        public virtual void Die(Vector3 position, Vector3 force, GameObject attacker)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.IsServer()) {
                m_NetworkHealthMonitor.Die(position, force, attacker);
            }
#endif

            // Spawn any objects on death, such as an explosion if the object is an explosive barrel.
            if (m_SpawnedObjectsOnDeath != null) {
                for (int i = 0; i < m_SpawnedObjectsOnDeath.Length; ++i) {
                    var spawnedObject = ObjectPool.Instantiate(m_SpawnedObjectsOnDeath[i], transform.position, transform.rotation);
                    Explosion explosion;
                    if ((explosion = spawnedObject.GetCachedComponent<Explosion>()) != null) {
                        explosion.Explode(gameObject);
                    }
                    var rigidbodies = spawnedObject.GetComponentsInChildren<Rigidbody>();
                    for (int j = 0; j < rigidbodies.Length; ++j) {
                        rigidbodies[j].AddForceAtPosition(force, position);
                    }
                }
            }

            // Destroy any objects on death. The objects will be placed back in the object pool if they were created within it otherwise the object will be destroyed.
            if (m_DestroyedObjectsOnDeath != null) {
                for (int i = 0; i < m_DestroyedObjectsOnDeath.Length; ++i) {
                    if (ObjectPool.InstantiatedWithPool(m_DestroyedObjectsOnDeath[i])) {
                        ObjectPool.Destroy(m_DestroyedObjectsOnDeath[i]);
                    } else {
                        Object.Destroy(m_DestroyedObjectsOnDeath[i]);
                    }
                }
            }

            // Change the layer to a death layer.
            if (m_DeathLayer.value != 0) {
                m_AliveLayer = m_GameObject.layer;
                m_GameObject.layer = m_DeathLayer;
            }

            // Play any take death audio.
            m_DeathAudioClipSet.PlayAudioClip(m_GameObject);

            // Deactivate the object if requested.
            if (m_DeactivateOnDeath) {
                Scheduler.Schedule(m_DeactivateOnDeathDelay, Deactivate);
            }

            // The attributes shouldn't regenerate.
            if (m_ShieldAttribute != null) {
                m_ShieldAttribute.CancelAutoUpdate();
            }
            if (m_HealthAttribute != null) {
                m_HealthAttribute.CancelAutoUpdate();
            }

            // Notify those interested.
            EventHandler.ExecuteEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", position, force, attacker);
            if (m_OnDeathEvent != null) {
                m_OnDeathEvent.Invoke(position, force, attacker);
            }
        }

        /// <summary>
        /// Kills the object immediately.
        /// </summary>
        public void ImmediateDeath()
        {
            ImmediateDeath(m_Transform.position, Vector3.zero, 0);
        }

        /// <summary>
        /// Kills the object immediately.
        /// </summary>
        /// <param name="position">The position the character died.</param>
        /// <param name="direction">The direction that the object took damage from.</param>
        /// <param name="forceMagnitude">The magnitude of the force that is applied to the object.</param>
        public void ImmediateDeath(Vector3 position, Vector3 direction, float forceMagnitude)
        {
            var amount = 0f;
            if (m_HealthAttribute != null) {
                amount += m_HealthAttribute.Value;
            }
            if (m_ShieldAttribute != null) {
                amount += m_ShieldAttribute.Value;
            }
            // If ImmediateDeath is called then the object should die even if it is invincible.
            var invincible = m_Invincible;
            m_Invincible = false;
            Damage(amount, position, direction, forceMagnitude);
            m_Invincible = invincible;
        }

        /// <summary>
        /// Adds amount to health and then to the shield if there is still an amount remaining. Will not go over the maximum health or shield value.
        /// </summary>
        /// <param name="amount">The amount of health or shield to add.</param>
        /// <returns>True if the object was healed.</returns>
        public virtual bool Heal(float amount)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.IsLocalPlayer()) {
                m_NetworkHealthMonitor.Heal(amount);
            }
#endif

            var healed = false;

            // Contribute the amount of the health first.
            if (m_HealthAttribute != null && m_HealthAttribute.Value < m_HealthAttribute.MaxValue) {
                var healthAmount = Mathf.Min(amount, m_HealthAttribute.MaxValue - m_HealthAttribute.Value);
                amount -= healthAmount;
                m_HealthAttribute.Value += healthAmount;
                healed = true;
            }

            // Add any remaining amount to the shield.
            if (m_ShieldAttribute != null && amount > 0 && m_ShieldAttribute.Value < m_ShieldAttribute.MaxValue) {
                var shieldAmount = Mathf.Min(amount, m_ShieldAttribute.MaxValue - m_ShieldAttribute.Value);
                m_ShieldAttribute.Value += shieldAmount;
                healed = true;
            }

            // Don't play any effects if the object wasn't healed.
            if (!healed) {
                return false;
            }

            // Play any heal audio.
            m_HealAudioClipSet.PlayAudioClip(m_GameObject);

            EventHandler.ExecuteEvent<float>(m_GameObject, "OnHealthHeal", amount);
            if (m_OnHealEvent != null) {
                m_OnHealEvent.Invoke(amount);
            }

            return true;
        }

        /// <summary>
        /// The object doesn't have any health or shield left and should be deactivated.
        /// </summary>
        private void Deactivate()
        {
            m_GameObject.SetActive(false);
        }

        /// <summary>
        /// The object has spawned again. Set the health and shield back to their starting values.
        /// </summary>
        protected virtual void OnRespawn()
        {
            if (m_HealthAttribute != null) {
                m_HealthAttribute.ResetValue();
            }
            if (m_ShieldAttribute != null) {
                m_ShieldAttribute.ResetValue();
            }
            // Change the layer back to the alive layer.
            if (m_DeathLayer.value != 0) {
                m_GameObject.layer = m_AliveLayer;
            }
            m_SpawnTime = Time.time;
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent(m_GameObject, "OnRespawn", OnRespawn);
        }
    }
}