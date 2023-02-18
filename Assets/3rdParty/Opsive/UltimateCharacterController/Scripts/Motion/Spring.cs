/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Motion
{
    using Opsive.Shared.Game;
    using UnityEngine;

    /// <summary>
    /// Simple but powerful spring logic for transform manipulation.
    /// </summary>
    [System.Serializable]
    public class Spring
    {
        [Tooltip("Spring stiffness - or mechanical strength - determines how loosely or rigidly the spring's velocity behaves.")]
        [Range(0, 1)] [SerializeField] protected float m_Stiffness = 0.2f;
        [Tooltip("Damping makes the spring velocity wear off as it approaches its rest state.")]
        [Range(0, 1)] [SerializeField] protected float m_Damping = 0.25f;
        [Tooltip("The amount of time it takes for the velocity to have its full impact.")]
        [SerializeField] protected float m_VelocityFadeInLength = 1;
        [Tooltip("The maximum number of frames that the soft force can be spread over.")]
        [SerializeField] protected int m_MaxSoftForceFrames = 120;
        [Tooltip("The minimum value of the velocity.")]
        [SerializeField] protected float m_MinVelocity = 0.00001f;
        [Tooltip("The maximum value of the velocity.")]
        [SerializeField] protected float m_MaxVelocity = 10000.0f;
        [Tooltip("The minimum value of the spring.")]
        [SerializeField] protected Vector3 m_MinValue = new Vector3(-10000, -10000, -10000);
        [Tooltip("The maximum value of the spring.")]
        [SerializeField] protected Vector3 m_MaxValue = new Vector3(10000, 10000, 10000);

        public float Stiffness { get { return m_Stiffness; } set { m_Stiffness = value; } }
        public float Damping { get { return m_Damping; } set { m_Damping = value; } }
        public float VelocityFadeInLength { get { return m_VelocityFadeInLength; } set { m_VelocityFadeInLength = value; } }
        public int MaxSoftForceFrames { get { return m_MaxSoftForceFrames; } set { m_MaxSoftForceFrames = value; } }
        public float MinVelocity { get { return m_MinVelocity; } set { m_MinVelocity = value; } }
        public float MaxVelocity { get { return m_MaxVelocity; } set { m_MaxVelocity = value; } }
        public Vector3 MinValue { get { return m_MinValue; } set { m_MinValue = value; } }
        public Vector3 MaxValue { get { return m_MaxValue; } set { m_MaxValue = value; } }

        private Vector3 m_Value;
        private Vector3 m_Velocity;
        private Vector3 m_RestValue;
        private bool m_RotationalSpring;
        
        private float m_VelocityFadeInCap;
        private float m_VelocityFadeInEndTime;
        private Vector3[] m_SoftForceFrames;
        private float m_TimeScale = 1;
        private bool m_Resting;

        // Update the spring forces with the Scheduler.
        ScheduledEventBase m_ScheduledEvent;

        [Opsive.Shared.Utility.NonSerialized] public Vector3 Value { get { return m_Value; } set { m_Value = value; } }
        [Opsive.Shared.Utility.NonSerialized] public Vector3 Velocity { get { return m_Velocity; } set { m_Velocity = value; } }
        [Opsive.Shared.Utility.NonSerialized] public Vector3 RestValue { get { return m_RestValue; }
            set {
                m_Resting = false;
                if (m_RotationalSpring) {
                    m_RestValue.x = Utility.MathUtility.ClampInnerAngle(value.x);
                    m_RestValue.y = Utility.MathUtility.ClampInnerAngle(value.y);
                    m_RestValue.z = Utility.MathUtility.ClampInnerAngle(value.z);
                } else {
                    m_RestValue = value;
                }
            }
        }
        public float TimeScale { set { m_TimeScale = value; } }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Spring() { }

        /// <summary>
        /// Two parameter constructor.
        /// </summary>
        /// <param name="stiffness">The default stiffness of the spring.</param>
        /// <param name="damping">The default damping of the spring.</param>
        public Spring(float stiffness, float damping)
        {
            m_Stiffness = stiffness;
            m_Damping = damping;
        }

        /// <summary>
        /// Initializes the spring. 
        /// </summary>
        /// <param name="rotationalSpring">Is the spring used for rotations?</param>
        /// <param name="fixedUpdate">Should the event be invoked within the FixedUpdate loop? If false Update will be used.</param>
        public void Initialize(bool rotationalSpring, bool fixedUpdate)
        {
            if (!Application.isPlaying) {
                return;
            }

            // If the ScheduledEvent is null then the spring has already been initialized.
            if (m_ScheduledEvent != null) {
                return;
            }

            m_SoftForceFrames = new Vector3[m_MaxSoftForceFrames];
            m_ScheduledEvent = fixedUpdate ? Scheduler.ScheduleFixed(-1, Tick) : Scheduler.Schedule(-1, Tick);
            m_VelocityFadeInEndTime = Time.time + m_VelocityFadeInLength;
            m_Resting = false;
            m_RotationalSpring = rotationalSpring;
            if (m_RotationalSpring) {
                m_RestValue.x = Utility.MathUtility.ClampInnerAngle(m_RestValue.x);
                m_RestValue.y = Utility.MathUtility.ClampInnerAngle(m_RestValue.y);
                m_RestValue.z = Utility.MathUtility.ClampInnerAngle(m_RestValue.z);
            }

            Reset();
        }

        /// <summary>
        /// Update the spring forces.
        /// </summary>
        private void Tick()
        {
            if (Time.timeScale == 0 || m_TimeScale == 0) {
                return;
            }

            // Slowly fade in the velocity at the start.
            if (m_VelocityFadeInCap != 1) {
                if (m_VelocityFadeInEndTime > Time.time) {
                    m_VelocityFadeInCap = Mathf.Clamp01(1 - ((m_VelocityFadeInEndTime - Time.time) / (m_VelocityFadeInLength / m_TimeScale)));
                } else {
                    m_VelocityFadeInCap = 1;
                }
            }

            // Update the smooth force each frame.
            if (m_SoftForceFrames[0] != Vector3.zero) {
                AddForceInternal(m_SoftForceFrames[0]);
                for (int v = 0; v < m_MaxSoftForceFrames; v++) {
                    m_SoftForceFrames[v] = (v < m_MaxSoftForceFrames - 1) ? m_SoftForceFrames[v + 1] : Vector3.zero;
                    if (m_SoftForceFrames[v] == Vector3.zero) {
                        break;
                    }
                }
            }

            Calculate();
        }

        /// <summary>
        /// Performs the spring calculations.
        /// </summary>
        private void Calculate()
        {
            // No work is necessary if the spring is currently resting.
            if (m_Resting) {
                return;
            }

            // Update the velocity based on the current stiffness and damping values.
            m_Velocity += (m_RestValue - m_Value) * (1 - m_Stiffness);
            m_Velocity *= m_Damping;
            m_Velocity = Vector3.ClampMagnitude(m_Velocity, m_MaxVelocity);
            // Move towards the rest point.
            Move();

            // Reset the spring if the velocity is below minimum.
            if ((m_RestValue - m_Value).sqrMagnitude <= (m_MinVelocity * m_MinVelocity)) {
                Reset();
            }
        }

        /// <summary>
        /// Adds the velocity to the state and clamps state between min and max values.
        /// </summary>
        private void Move()
        {
            m_Value += m_Velocity * m_TimeScale * Time.timeScale;
            m_Value.x = Mathf.Clamp(m_Value.x, m_MinValue.x, m_MaxValue.x);
            m_Value.y = Mathf.Clamp(m_Value.y, m_MinValue.y, m_MaxValue.y);
            m_Value.z = Mathf.Clamp(m_Value.z, m_MinValue.z, m_MaxValue.z);
        }

        /// <summary>
        /// Adds an external velocity to the spring in one frame.
        /// </summary>
        /// <param name="force">The force to add.</param>
        public void AddForce(Vector3 force)
        {
            AddForce(force, 1);
        }

        /// <summary>
        /// Adds an external velocity to the spring in specified number of frames. The force will either be an external or soft force.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        public void AddForce(Vector3 force, int frames)
        {
            if (frames > 1) {
                AddSoftForce(force, frames);
            } else {
                AddForceInternal(force);
            }
        }

        /// <summary>
        /// Adds an external velocity to the spring in one frame.
        /// </summary>
        /// <param name="force">The force to add.</param>
        private void AddForceInternal(Vector3 force)
        {
            force *= m_VelocityFadeInCap;
            m_Velocity += force;
            m_Velocity = Vector3.ClampMagnitude(m_Velocity, m_MaxVelocity);
            if (m_RotationalSpring) {
                m_Velocity.x = Utility.MathUtility.ClampInnerAngle(m_Velocity.x);
                m_Velocity.y = Utility.MathUtility.ClampInnerAngle(m_Velocity.y);
                m_Velocity.z = Utility.MathUtility.ClampInnerAngle(m_Velocity.z);
            }
            m_Resting = m_Velocity.sqrMagnitude <= (m_MinVelocity * m_MinVelocity) && m_Value == m_RestValue;
        }

        /// <summary>
        /// Adds a force distributed over up to 120 frames.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="frames">The number of frames to distribute the force over.</param>
        private void AddSoftForce(Vector3 force, float frames)
        {
            frames = Mathf.Clamp(frames, 1, m_MaxSoftForceFrames);
            AddForceInternal(force / frames);
            for (int v = 0; v < (Mathf.RoundToInt(frames) - 1); v++) {
                m_SoftForceFrames[v] += (force / frames);
            }
        }

        /// <summary>
        /// Resets the spring velocity and resets state to the static equilibrium.
        /// </summary>
        public void Reset()
        {
            m_Value = m_RestValue;
            m_Resting = true;
            Stop(true);
        }

        /// <summary>
        /// Stops spring velocity.
        /// </summary>
        /// <param name="includeSoftForce">Should the soft force also be stopped?</param>
        public void Stop(bool includeSoftForce)
        {
            m_Velocity = Vector3.zero;
            if (includeSoftForce && m_SoftForceFrames != null) {
                for (int v = 0; v < 120; v++) {
                    m_SoftForceFrames[v] = Vector3.zero;
                }
            }
        }

        /// <summary>
        /// Destroys the spring.
        /// </summary>
        public void Destroy()
        {
            if (m_ScheduledEvent != null) {
                Scheduler.Cancel(m_ScheduledEvent);
                m_ScheduledEvent = null;
            }
            m_SoftForceFrames = null;
        }

        /// <summary>
        /// Spring destructor. The scheduled event is no longer needed.
        /// </summary>
        ~Spring()
        {
            Destroy();
        }
    }
}