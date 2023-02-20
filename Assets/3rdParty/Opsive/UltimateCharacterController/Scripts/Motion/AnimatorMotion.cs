/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Motion
{
    /// <summary>
    /// Defines the character's movements when there is no root motion data available.
    /// </summary>
    public class AnimatorMotion : ScriptableObject
    {
        [Tooltip("An AnimationCurve representing the animations x position.")]
        [SerializeField] protected AnimationCurve m_XPosition = AnimationCurve.EaseInOut(0, 0, 5, 0);
        [Tooltip("An AnimationCurve representing the animations y position.")]
        [SerializeField] protected AnimationCurve m_YPosition = AnimationCurve.EaseInOut(0, 0, 5, 0);
        [Tooltip("An AnimationCurve representing the animations z position.")]
        [SerializeField] protected AnimationCurve m_ZPosition = AnimationCurve.EaseInOut(0, 0, 5, 0);

        [Tooltip("An AnimationCurve representing the animations x euler rotation.")]
        [SerializeField] protected AnimationCurve m_XRotation = AnimationCurve.EaseInOut(0, 0, 5, 0);
        [Tooltip("An AnimationCurve representing the animations y euler rotation.")]
        [SerializeField] protected AnimationCurve m_YRotation = AnimationCurve.EaseInOut(0, 0, 5, 0);
        [Tooltip("An AnimationCurve representing the animations z euler rotation.")]
        [SerializeField] protected AnimationCurve m_ZRotation = AnimationCurve.EaseInOut(0, 0, 5, 0);

        public AnimationCurve XPosition { get { return m_XPosition; } set { m_XPosition = value; } }
        public AnimationCurve YPosition { get { return m_YPosition; } set { m_YPosition = value; } }
        public AnimationCurve ZPosition { get { return m_ZPosition; } set { m_ZPosition = value; } }

        public AnimationCurve XRotation { get { return m_XRotation; } set { m_XRotation = value; } }
        public AnimationCurve YRotation { get { return m_YRotation; } set { m_YRotation = value; } }
        public AnimationCurve ZRotation { get { return m_ZRotation; } set { m_ZRotation = value; } }

        /// <summary>
        /// Evaluations the position at the specified time.
        /// </summary>
        /// <param name="time">The time to evaluate the position at.</param>
        /// <param name="position">The position that occurs at the specified time.</param>
        public void EvaluatePosition(float time, ref Vector3 position)
        {
            position.Set(m_XPosition.Evaluate(time), m_YPosition.Evaluate(time), m_ZPosition.Evaluate(time));
        }

        /// <summary>
        /// Evaluations the rotation at the specified time.
        /// </summary>
        /// <param name="time">The time to evaluate the rotation at.</param>
        /// <param name="position">The rotation that occurs at the specified time.</param>
        public void EvaluateRotation(float time, ref Quaternion rotation)
        {
            rotation = Quaternion.Euler(m_XRotation.Evaluate(time), m_YRotation.Evaluate(time), m_ZRotation.Evaluate(time));
        }
    }
}