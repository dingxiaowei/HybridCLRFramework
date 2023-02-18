/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects.ItemAssist
{
    using Opsive.Shared.Game;
    using UnityEngine;

    /// <summary>
    /// Represents a shell casing which uses the trajectory object for kinematic shell movement.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Shell : TrajectoryObject
    {
        [Tooltip("Time to live in seconds before the shell is removed.")]
        [SerializeField] protected float m_Lifespan = 10;
        [Tooltip("Chance of shell not being removed after settling on the ground.")]
        [Range(0, 1)] [SerializeField] protected float m_Persistence = 1;

        private float m_RemoveTime;
        private Vector3 m_StartScale;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_StartScale = transform.localScale;

            // The Rigidbody is only used to notify Unity that the object isn't static. The Rigidbody doesn't control any movement.
            var rigidbody = GetComponent<Rigidbody>();
            rigidbody.mass = m_Mass;
            rigidbody.isKinematic = true;
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }

        /// <summary>
        /// The shell has been spawned - reset the timing and component values.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            m_RemoveTime = Time.time + m_Lifespan;
            m_Transform.localScale = m_StartScale;

            if (m_Collider != null) {
                m_Collider.enabled = true;
            }
        }

        /// <summary>
        /// Move and rotate the object according to a parabolic trajectory.
        /// </summary>
        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (Time.time > m_RemoveTime) { // The shell should be removed.
                m_Transform.localScale = Vector3.Lerp(m_Transform.localScale, Vector3.zero, Utility.TimeUtility.FramerateDeltaTime * 0.2f);
                if (Time.time > m_RemoveTime + 0.5f) {
                    ObjectPool.Destroy(m_GameObject);
                }
            }
        }

        /// <summary>
        /// The object has collided with another object.
        /// </summary>
        /// <param name="hit">The RaycastHit of the object. Can be null.</param>
        protected override void OnCollision(RaycastHit? hit)
        {
            base.OnCollision(hit);

            if (m_Velocity.sqrMagnitude > 4) { // Hard bounce.
                // Apply more random rotation velocity to make the shell behave a bit unpredictably on a hard bounce (similar to real brass shell behavior).
                AddTorque(Random.rotation.eulerAngles * 0.15f * (Random.value > 0.5f ? 1 : -1));
            } else if (Random.value > m_Persistence) { // Soft bounce.
                // Remove the shell after half a second on a soft bounce.
                m_RemoveTime = Time.time + 0.5f;
            }
        }
    }
}