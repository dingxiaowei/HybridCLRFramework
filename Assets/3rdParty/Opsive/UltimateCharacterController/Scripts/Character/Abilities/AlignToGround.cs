/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using Opsive.UltimateCharacterController.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The AlignToGround ability will orient the character to the direction of the ground's normal.
    /// </summary>
    public class AlignToGround : AlignToGravity
    {
        [Tooltip("The distance from the ground that the character should align itself to.")]
        [SerializeField] protected float m_Distance = 4;
        [Tooltip("The depth offset when checking the ground normal.")]
        [SerializeField] protected float m_DepthOffset = 0.05f;
        [Tooltip("Should the direction from the align to ground depth offset be normalized? This is useful for generic characters whose length is long.")]
        [SerializeField] protected bool m_NormalizeDirection = false;

        public float Distance { get { return m_Distance; } set { m_Distance = value; } }
        public float DepthOffset { get { return m_DepthOffset; } set { m_DepthOffset = value; } }
        public bool NormalizeDirection { get { return m_NormalizeDirection; } set { m_NormalizeDirection = value; } }

        private RaycastHit[] m_CombinedRaycastHits;
        private Dictionary<RaycastHit, int> m_ColliderIndexMap;
        private UnityEngineUtility.RaycastHitComparer m_RaycastHitComparer = new UnityEngineUtility.RaycastHitComparer();

        /// <summary>
        /// Update the rotation forces.
        /// </summary>
        public override void UpdateRotation()
        {
            var updateNormalRotation = m_Stopping;
            var targetNormal = m_Stopping ? (m_StopGravityDirection.sqrMagnitude >  0 ? -m_StopGravityDirection : -m_CharacterLocomotion.GravityDirection) : m_CharacterLocomotion.Up;
            if (!m_Stopping) {
                // If the depth offset isn't zero then use two raycasts to determine the ground normal. This will allow a long character (such as a horse) to correctly
                // adjust to a slope.
                if (m_DepthOffset != 0) { 
                    var frontPoint = m_Transform.position;
                    bool frontHit;
                    RaycastHit raycastHit;
                    if ((frontHit = Physics.Raycast(m_Transform.TransformPoint(0, m_CharacterLocomotion.Radius, m_DepthOffset * Mathf.Sign(m_CharacterLocomotion.InputVector.y)),
                                                    -m_CharacterLocomotion.Up, out raycastHit, m_Distance + m_CharacterLocomotion.Radius, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore))) {
                        frontPoint = raycastHit.point;
                        targetNormal = raycastHit.normal;
                    }

                    if (Physics.Raycast(m_Transform.TransformPoint(0, m_CharacterLocomotion.Radius, m_DepthOffset * -Mathf.Sign(m_CharacterLocomotion.InputVector.y)),
                                            -m_CharacterLocomotion.Up, out raycastHit, m_Distance + m_CharacterLocomotion.Radius, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore)) {
                        if (frontHit) {
                            if (m_NormalizeDirection) {
                                var backPoint = raycastHit.point;
                                var direction = (frontPoint - backPoint).normalized;
                                targetNormal = Vector3.Cross(direction, Vector3.Cross(m_CharacterLocomotion.Up, direction)).normalized;
                            } else {
                                targetNormal = (targetNormal + raycastHit.normal).normalized;
                            }
                        } else {
                            targetNormal = raycastHit.normal;
                        }
                    }

                    m_CharacterLocomotion.GravityDirection = -targetNormal;
                    updateNormalRotation = true;
                } else {
                    var hitCount = m_CharacterLocomotion.Cast(-m_CharacterLocomotion.Up * m_Distance,
                        m_CharacterLocomotion.PlatformMovement + m_CharacterLocomotion.Up * m_CharacterLocomotion.ColliderSpacing, ref m_CombinedRaycastHits, ref m_ColliderIndexMap);

                    // The character hit the ground if any hit points are below the collider.
                    for (int i = 0; i < hitCount; ++i) {
                        var closestRaycastHit = QuickSelect.SmallestK(m_CombinedRaycastHits, hitCount, i, m_RaycastHitComparer);

                        // The hit point has to be under the collider for the character to align to it.
                        var activeCollider = m_CharacterLocomotion.ColliderCount > 1 ? m_CharacterLocomotion.Colliders[m_ColliderIndexMap[closestRaycastHit]] : m_CharacterLocomotion.Colliders[0];
                        if (!MathUtility.IsUnderCollider(m_Transform, activeCollider, closestRaycastHit.point)) {
                            continue;
                        }

                        targetNormal = m_CharacterLocomotion.GravityDirection = -closestRaycastHit.normal;
                        updateNormalRotation = true;
                        break;
                    }
                }
            }

            // The rotation is affected by aligning to the ground or having a different up rotation from gravity.
            if (updateNormalRotation) {
                Rotate(targetNormal);
            }
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            ResetAlignToGravity();
        }
    }
}