/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character.Effects
{
    /// <summary>
    /// Moves the camera downward similar to how a large boss would shake the camera as they are stomping on the ground.
    /// </summary>
    public class BossStomp : Effect
    {
        [Tooltip("The direction to apply the positional force.")]
        [SerializeField] protected Vector3 m_PositionalStompDirection = Vector3.down;
        [Tooltip("The strength of the positional boss stomp.")]
        [SerializeField] protected MinMaxFloat m_PositionalStrength = new MinMaxFloat(0.5f, 1);
        [Tooltip("The direction to apply the rotational force.")]
        [SerializeField] protected Vector3 m_RotationalStompDirection = Vector3.forward;
        [Tooltip("The strength of the rotational boss stomp.")]
        [SerializeField] protected MinMaxFloat m_RotationalStrength = new MinMaxFloat(10, 15);
        [Tooltip("The number of times the stomp effect should play. Set to -1 to play the efffect until the effect is stopped or disabled.")]
        [SerializeField] protected int m_RepeatCount;
        [Tooltip("The delay until the stomp plays again.")]
        [SerializeField] protected float m_RepeatDelay = 1;

        public Vector3 PositionalStompDirection { get { return m_PositionalStompDirection; } set { m_PositionalStompDirection = value; } }
        public MinMaxFloat PositionalStrength { get { return m_PositionalStrength; } set { m_PositionalStrength = value; } }
        public Vector3 RotationalStompDirection { get { return m_RotationalStompDirection; } set { m_RotationalStompDirection = value; } }
        public MinMaxFloat RotationalStrength { get { return m_RotationalStrength; } set { m_RotationalStrength = value; } }
        public int RepeatCount { get { return m_RepeatCount; } set { m_RepeatCount = value; } }
        public float RepeatDelay { get { return m_RepeatDelay; } set { m_RepeatDelay = value; } }

        private int m_StopCount;
        private ScheduledEventBase m_StopEvent;

        /// <summary>
        /// Can the effect be started?
        /// </summary>
        /// <returns>True if the effect can be started.</returns>
        public override bool CanStartEffect()
        {
            return m_CameraController != null;
        }

        /// <summary>
        /// The effect has been started.
        /// </summary>
        protected override void EffectStarted()
        {
            base.EffectStarted();

            m_StopCount = 0;
            Stomp();
        }

        /// <summary>
        /// Performs the stomp effect.
        /// </summary>
        private void Stomp()
        {
            m_CameraController.AddSecondaryPositionalForce(m_PositionalStompDirection * m_PositionalStrength.RandomValue, 0);
            m_CameraController.AddSecondaryRotationalForce(m_RotationalStompDirection * m_RotationalStrength.RandomValue * (Random.value > 0.5f ? 1 : -1), 0);
            m_StopCount++;

            if (m_RepeatCount == -1 || m_StopCount < m_RepeatCount) {
                m_StopEvent = Scheduler.ScheduleFixed(m_RepeatDelay, Stomp);
            } else {
                StopEffect();
            }
        }

        /// <summary>
        /// The effect has stopped running.
        /// </summary>
        protected override void EffectStopped()
        {
            base.EffectStopped();

            Scheduler.Cancel(m_StopEvent);
            m_StopEvent = null;
        }
    }
}