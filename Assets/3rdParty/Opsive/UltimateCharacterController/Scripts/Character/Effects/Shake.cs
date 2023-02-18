/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Effects
{
    using Opsive.Shared.Events;
    using Opsive.UltimateCharacterController.Motion;
    using UnityEngine;

    /// <summary>
    /// Shakes the camera, item, or character based on a force magnitude.
    /// </summary>
    public class Shake : Effect
    {
        /// <summary>
        /// Specifies which objects to apply the shaking force to.
        /// </summary>
        public enum ShakeTarget
        {
            Camera = 1,     // Shakes the camera.
            Item = 2,       // Shakes the equipped item.
            Character = 4   // Shakes the character.
        }

        [Tooltip("Specifies which objects to apply the shaking force to.")]
        [HideInInspector] [SerializeField] protected ShakeTarget m_Target = ShakeTarget.Camera | ShakeTarget.Item | ShakeTarget.Character;
        [Tooltip("The amount of force to apply to the shake.")]
        [SerializeField] protected Vector2 m_Force = new Vector2(0.4f, 0.4f);
        [Tooltip("Should a smooth horizontal force be added? If false a random force between 0 and Force.x will be used.")]
        [SerializeField] protected bool m_SmoothHorizontalForce = true;
        [Tooltip("Specifies the probability that a vertical force will be applied.")]
        [SerializeField] protected float m_VerticalForceProbability = 0.3f;
        [Tooltip("The amount of time that it takes for the effect to fade out.")]
        [SerializeField] protected float m_FadeOutDuration = 4;
        [Tooltip("Exaggerates or reduces the positional force imposed.")]
        [SerializeField] protected float m_PositionalFactor = 1;
        [Tooltip("Exaggerates or reduces the rotational force imposed.")]
        [SerializeField] protected float m_RotationalFactor = 3;
        [Tooltip("The number of seconds that the effect will last.")]
        [SerializeField] protected float m_Duration = 7;

        public ShakeTarget Target { get { return m_Target; } set { m_Target = value; } }
        public Vector2 Force { get { return m_Force; } set { m_Force = value; } }
        public bool SmoothHorizontalForce { get { return m_SmoothHorizontalForce; } set { m_SmoothHorizontalForce = value; } }
        public float VerticalForceProbability { get { return m_VerticalForceProbability; } set { m_VerticalForceProbability = value; } }
        public float FadeOutDuration { get { return m_FadeOutDuration; } set { m_FadeOutDuration = value; } }
        public float PositionalFactor { get { return m_PositionalFactor; } set { m_PositionalFactor = value; } }
        public float RotationalFactor { get { return m_RotationalFactor; } set { m_RotationalFactor = value; } }
        public float Duration { get { return m_Duration; } set { m_Duration = value; } }

        private float m_StartTime;
        private Vector3 m_TotalForce;

        /// <summary>
        /// Can the effect be started?
        /// </summary>
        /// <returns>True if the effect can be started.</returns>
        public override bool CanStartEffect()
        {
            if (m_Target == 0) {
                return false;
            }
            return base.CanStartEffect();
        }

        /// <summary>
        /// The effect has been started.
        /// </summary>
        protected override void EffectStarted()
        {
            base.EffectStarted();

            m_StartTime = Time.unscaledTime;
            m_TotalForce = Vector3.zero;
        }

        /// <summary>
        /// Updates the effect.
        /// </summary>
        public override void Update()
        {
            // Stop the effect if it has occurred for more than the duration.
            var endTime = m_StartTime + m_Duration;
            if (endTime < Time.unscaledTime) {
                StopEffect();
                return;
            }

            var force = Vector3.zero;
            if (m_SmoothHorizontalForce) {
                // Apply a horizontal force which is the perlin noise value between 0 and the force. This force will ease out during the specified fade out duration.
                force.x = SmoothRandom.GetVector3Centered(1).x * m_Force.x * Mathf.Min(endTime - Time.unscaledTime, m_FadeOutDuration) * Time.timeScale * m_CharacterLocomotion.TimeScale;
            } else {
                // If smooth horizontal force is false then apply a random force which will ease out during the specified fade out duration.
                force.x = Random.Range(-m_Force.x, m_Force.x) * Mathf.Min(endTime - Time.unscaledTime, m_FadeOutDuration);

                // Alternates between positive and negative to produce sharp shakes with nice spring smoothness.
                if (Mathf.Sign(m_TotalForce.x) == Mathf.Sign(force.x)) {
                    force.x = -force.x;
                }
            }

            // Restrict the number of times a vertical force is applied to prevent a jerky movements.
            if (Random.value <= m_VerticalForceProbability) {
                // Smoothly fade out during the specified fade out duration.
                force.y = Random.Range(0, m_Force.y) * Mathf.Min(endTime - Time.unscaledTime, m_FadeOutDuration);

                // Alternates between positive and negative to produce sharp shakes with nice spring smoothness.
                if (Mathf.Sign(m_TotalForce.y) == Mathf.Sign(force.y)) {
                    force.y = -force.y;
                }
            }

            m_TotalForce += force;

            // Add the force to the camera.
            if ((m_Target & ShakeTarget.Camera) != 0 && m_CameraController != null) {
                m_CameraController.AddPositionalForce(force * m_PositionalFactor);
                m_CameraController.AddRotationalForce(-force * 2 * m_RotationalFactor);
            }

            // Add the force to the item.
            if ((m_Target & ShakeTarget.Item) != 0) {
                var positionalForce = Vector3.forward * force.x * 0.015f;
                var rotationalForce = positionalForce;
                rotationalForce.Set(force.y * 2, -force.x, force.x * 2);
                EventHandler.ExecuteEvent(m_GameObject, "OnAddSecondaryForce", -1, positionalForce, rotationalForce, true);
            }

            // Add the horizontal force to the character.
            if ((m_Target & ShakeTarget.Character) != 0) {
                force.y = 0;
                m_CharacterLocomotion.AddForce(force * m_PositionalFactor);
            }
        }
    }
}