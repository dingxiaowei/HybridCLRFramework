/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Magic.CastActions
{
    using UnityEngine;

    /// <summary>
    /// Immediately calls impact on the object at the target position.
    /// </summary>
    [System.Serializable]
    public class TargetImpact : CastAction
    {
        /// <summary>
        /// Performs the cast.
        /// </summary>
        /// <param name="origin">The location that the cast should spawn from.</param>
        /// <param name="direction">The direction of the cast.</param>
        /// <param name="targetPosition">The target position of the cast.</param>
        public override void Cast(Transform origin, Vector3 direction, Vector3 targetPosition)
        {
            if (Physics.Raycast(targetPosition - direction.normalized * 0.1f, direction.normalized, out var hit, direction.magnitude + 0.1f, m_MagicItem.DetectLayers)) {
                m_MagicItem.PerformImpact(m_CastID, m_GameObject, hit.transform.gameObject, hit);
            }
        }

        /// <summary>
        /// Stops the cast.
        /// </summary>
        public override void Stop()
        {
            if (m_MagicItem != null && m_MagicItem.ImpactActions != null) {
                for (int i = 0; i < m_MagicItem.ImpactActions.Length; ++i) {
                    m_MagicItem.ImpactActions[i].Reset(m_CastID);
                }
            }

            base.Stop();
        }
    }
}