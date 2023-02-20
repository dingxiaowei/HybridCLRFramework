/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Demo
{
    /// <summary>
    /// Continuously applies damage to the character while the character is within the trigger.
    /// </summary>
    public class DamageZone : MonoBehaviour
    {
        [Tooltip("The delay until the damage is started to be applied.")]
        [SerializeField] protected float m_InitialDamageDelay = 0.5f;
        [Tooltip("The amount of damage to apply during each damage event.")]
        [SerializeField] protected float m_DamageAmount = 10;
        [Tooltip("The interval between damage events.")]
        [SerializeField] protected float m_DamageInterval = 0.2f;

        private Health m_Health;
        private Transform m_HealthTransform;
        private ScheduledEventBase m_ScheduledDamageEvent;

        /// <summary>
        /// An object has entered the trigger.
        /// </summary>
        /// <param name="other">The object that entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (m_Health != null) {
                return;
            }

            // A main character collider is required.
            if (!MathUtility.InLayerMask(other.gameObject.layer, 1 << LayerManager.Character)) {
                return;
            }

            // The object must be a character.
            var characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            // With a health component.
            var health = characterLocomotion.GetComponent<Health>();
            if (health == null) {
                return;
            }

            m_Health = health;
            m_HealthTransform = health.transform;
            m_ScheduledDamageEvent = Scheduler.Schedule(m_InitialDamageDelay, Damage);
        }

        /// <summary>
        /// Apply damage to the health component.
        /// </summary>
        private void Damage()
        {
            m_Health.Damage(m_DamageAmount, m_HealthTransform.position + Random.insideUnitSphere, Vector3.zero, 0);

            // Apply the damage again if the object still has health remaining.
            if (m_Health.Value > 0) {
                m_ScheduledDamageEvent = Scheduler.Schedule(m_DamageInterval, Damage);
            }
        }

        /// <summary>
        /// An object has exited the trigger.
        /// </summary>
        /// <param name="other">The collider that exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            // A main character collider is required.
            if (!MathUtility.InLayerMask(other.gameObject.layer, 1 << LayerManager.Character)) {
                return;
            }

            var health = other.GetComponentInParent<Health>();
            if (health == m_Health) {
                // The object has left the trigger - stop applying damage.
                Scheduler.Cancel(m_ScheduledDamageEvent);
                m_Health = null;
            }
        }

        /// <summary>
        /// Draw a gizmo showing the damage zone.
        /// </summary>
        private void OnDrawGizmos()
        {
            var meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null) {
                var color = Color.red;
                color.a = 0.5f;
                Gizmos.color = color;
                Gizmos.DrawMesh(meshCollider.sharedMesh, meshCollider.transform.position, meshCollider.transform.rotation);
            }
        }
    }
}