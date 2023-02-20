/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Objects.CharacterAssist
{
    /// <summary>
    /// The Spherical Gravity Zone extends the Gravity Zone component by implementing a gravity force that is affected by spherical directions.
    /// </summary>
    public class SphericalGravityZone : GravityZone
    {
        [Tooltip("The amount of influence that the gravity force has. An x value of 0 represents the further point away from the sphere (at the distance of the radius), " +
                 "while an x value of 1 represents the cloest point (the center of the sphere). The y value represents the amount of force that should be applied at that distance (0-1).")]
        [SerializeField] protected AnimationCurve m_Influence = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [Tooltip("The value to multiply the influence by. A larger value can be used for larger spheres.")]
        [SerializeField] protected float m_InfluenceMultiplier = 1;

        private Transform m_Transform;
        private SphereCollider m_SphereCollider;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;
            m_SphereCollider = GetComponent<SphereCollider>();
        }

        /// <summary>
        /// Determines the direction of gravity that should be applied.
        /// </summary>
        /// <param name="position">The position of the character.</param>
        /// <returns>The direction of gravity that should be applied.</returns>
        public override Vector3 DetermineGravityDirection(Vector3 position)
        {
            var direction = (position - m_Transform.position);
            var influenceFactor = m_Influence.Evaluate(1 - (direction.magnitude / (m_SphereCollider.radius * MathUtility.ColliderRadiusMultiplier(m_SphereCollider)))) * m_InfluenceMultiplier;
            return direction.normalized * influenceFactor;
        }
    }
}