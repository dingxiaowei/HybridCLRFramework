/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Demo
{
    /// <summary>
    /// Adjusts the size of the trigger when the character enters. This is useful for movement type triggers so the character doesn't keep switching modes as the controls
    /// change while on the edge of the trigger.
    /// </summary>
    public class TriggerAdjustor : MonoBehaviour
    {
        [Tooltip("Specifies the amount to expand the BoxCollider trigger by.")]
        [SerializeField] protected Vector3 m_BoxColliderExpansion;
        [Tooltip("Specifies the amount to inflate the MeshCollider trigger by.")]
        [SerializeField] protected Mesh m_ExpandedMesh;

        private GameObject m_ActiveObject;
        private Collider m_Collider;
        private Mesh m_OriginalMesh;
        private bool m_AllowTriggerExit = true;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Collider = GetComponent<Collider>();

            if (m_Collider is MeshCollider) {
                m_OriginalMesh = (m_Collider as MeshCollider).sharedMesh;
            }
        }

        /// <summary>
        /// An object has entered the trigger.
        /// </summary>
        /// <param name="other">The object that entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (m_ActiveObject != null || !MathUtility.InLayerMask(other.gameObject.layer, 1 << LayerManager.Character)) {
                return;
            }

            var characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            // The object is a character. Expand the trigger.
            if (m_Collider is BoxCollider) {
                AdjustBoxCollider(m_Collider as BoxCollider, m_BoxColliderExpansion);
            } else if (m_Collider is MeshCollider) {
                // When the mesh is inflated it'll trigger an OnTriggerExit callback. Prevent this callback from doing anything until 
                // after the inflated mesh has stabalized.
                m_AllowTriggerExit = false;
                Game.Scheduler.ScheduleFixed(Time.fixedDeltaTime * 2, () => { m_AllowTriggerExit = true; });
                (m_Collider as MeshCollider).sharedMesh = m_ExpandedMesh;
            }
            m_ActiveObject = other.gameObject;
        }

        /// <summary>
        /// Adjusts the size of the BoxCollider.
        /// </summary>
        /// <param name="boxCollider">The BoxCollider that should be adjusted.</param>
        /// <param name="adjustment">The amount to adjust the BoxCollider by.</param>
        private void AdjustBoxCollider(BoxCollider boxCollider, Vector3 adjustment)
        {
            var size = boxCollider.size;
            size += adjustment;
            boxCollider.size = size;
        }

        /// <summary>
        /// An object has exited the trigger.
        /// </summary>
        /// <param name="other">The collider that exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            if (!m_AllowTriggerExit) {
                return;
            }

            if (m_ActiveObject == other.gameObject) {
                if (m_Collider is BoxCollider) {
                    AdjustBoxCollider(m_Collider as BoxCollider, -m_BoxColliderExpansion);
                } else if (m_Collider is MeshCollider) {
                    (m_Collider as MeshCollider).sharedMesh = m_OriginalMesh;
                }
                m_ActiveObject = null;
            }
        }
    }
}