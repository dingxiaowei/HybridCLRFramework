/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Motion
{
    /// <summary>
    /// Allows for a user-defined path that objects can follow.
    /// </summary>
    public class Path : MonoBehaviour
    {
        [Tooltip("The points which represent the curve.")]
        [SerializeField] protected Vector3[] m_ControlPoints;

        public Vector3[] ControlPoints { get { return m_ControlPoints; } set { m_ControlPoints = value; } }

        private CubicBezierCurve[] m_Curve;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            if (m_ControlPoints == null || m_ControlPoints.Length <= 1) {
                return;
            }

            m_Curve = new CubicBezierCurve[(m_ControlPoints.Length / 3)];
            for (int i = 0; i < m_Curve.Length; ++i) {
                var startIndex = i * 3;
                m_Curve[i] = new CubicBezierCurve(transform.TransformPoint(m_ControlPoints[startIndex]), transform.TransformPoint(m_ControlPoints[startIndex + 1]), 
                    transform.TransformPoint(m_ControlPoints[startIndex + 2]), transform.TransformPoint(m_ControlPoints[startIndex + 3]));
            }
        }

        /// <summary>
        /// Returns the tangent of the curve near the specified position.
        /// </summary>
        /// <param name="position">The position to retrieve the tangent of.</param>
        /// <param name="index">The index of the last curve segnement.</param>
        /// <returns>The tangent of the curve near the specified position.</returns>
        public Vector3 GetTangent(Vector3 position, ref int index)
        {
            var time = m_Curve[index].GetTime(position);
            if (time == 1 && index < m_Curve.Length - 1) {
                // If the time is equal to 1 then the position is at an endpoint. Determine if the current curve is closer to the given position or if the next curve is closer.
                var distance = (m_Curve[index].GetClosestPoint(position) - position).sqrMagnitude;
                var nextDistance = (m_Curve[index + 1].GetClosestPoint(position) - position).sqrMagnitude;
                if (nextDistance < distance) {
                    // The next curve is closer - increase the index and retrieve a new time.
                    index++;
                    time = m_Curve[index].GetTime(position);
                }
            } else if (time == 0 && index > 0) {
                // If the time is equal to 0 then the position is at an endpoint. Determine if the current curve is closer to the given position or if the previous curve is closer.
                var distance = (m_Curve[index].GetClosestPoint(position) - position).sqrMagnitude;
                var prevDistance = (m_Curve[index - 1].GetClosestPoint(position) - position).sqrMagnitude;
                if (prevDistance < distance) {
                    // The previous curve is closer - decrease the index and retrieve a new time.
                    index--;
                    time = m_Curve[index].GetTime(position);
                }
            }

            return m_Curve[index].GetTangent(time);
        }
        /// <summary>
        /// Returns the tangent of the curve near the specified position.
        /// </summary>
        /// <param name="position">The position to retrieve the tangent of.</param>
        /// <param name="index">The index of the last curve segnement.</param>
        /// <returns>The tangent of the curve near the specified position.</returns>
        public Vector3 GetClosestPoint(Vector3 position, ref int index)
        {
            var time = m_Curve[index].GetTime(position);
            if (time == 1 && index < m_Curve.Length - 1) {
                // If the time is equal to 1 then the position is at an endpoint. Determine if the current curve is closer to the given position or if the next curve is closer.
                var distance = (m_Curve[index].GetClosestPoint(position) - position).sqrMagnitude;
                var nextDistance = (m_Curve[index + 1].GetClosestPoint(position) - position).sqrMagnitude;
                if (nextDistance < distance) {
                    // The next curve is closer - increase the index and retrieve a new time.
                    index++;
                }
            } else if (time == 0 && index > 0) {
                // If the time is equal to 0 then the position is at an endpoint. Determine if the current curve is closer to the given position or if the previous curve is closer.
                var distance = (m_Curve[index].GetClosestPoint(position) - position).sqrMagnitude;
                var prevDistance = (m_Curve[index - 1].GetClosestPoint(position) - position).sqrMagnitude;
                if (prevDistance < distance) {
                    // The previous curve is closer - decrease the index and retrieve a new time.
                    index--;
                }
            }

            return m_Curve[index].GetClosestPoint(position);
        }

        /// <summary>
        /// Represents one segment of a cubic bezier curve.
        /// </summary>
        public class CubicBezierCurve
        {
            private const int c_StepCount = 300;

            private Vector3 m_P0;
            private Vector3 m_P1;
            private Vector3 m_P2;
            private Vector3 m_P3;

            /// <summary>
            /// Four parameter constructor.
            /// </summary>
            /// <param name="p0">The first point that makes up the curve.</param>
            /// <param name="p1">The second point that makes up the curve.</param>
            /// <param name="p2">The third point that makes up the curve.</param>
            /// <param name="p3">The fourth point that makes up the curve.</param>
            public CubicBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
            {
                m_P0 = p0;
                m_P1 = p1;
                m_P2 = p2;
                m_P3 = p3;
            }

            /// <summary>
            /// Returns the point of the bezier curve at the normalized position of the curve.
            /// </summary>
            /// <param name="time">The normalized position within the curve.</param>
            /// <returns>The point of the bezier curve at the normalized position of the curve.</returns>
            public Vector3 GetPoint(float time)
            {
                return (((-m_P0 + 3 * (m_P1 - m_P2) + m_P3) * time + (3 * (m_P0 + m_P2) - 6 * m_P1)) * time + 3 * (m_P1 - m_P0)) * time + m_P0;
            }

            /// <summary>
            /// Returns the tangent. This tangent is the first derivative of the curve.
            /// </summary>
            /// <param name="time">The normalized position within the curve.</param>
            /// <returns>The tangent of the curve.</returns>
            public Vector3 GetTangent(float time)
            {
                return (3 * (1 - time) * (1 - time) * (m_P1 - m_P0) + 6 * (1 - time) * time * (m_P2 - m_P1) + 3 * time * time * (m_P3 - m_P2)).normalized;
            }

            /// <summary>
            /// Returns the closest time at the specified position.
            /// </summary>
            /// <param name="position">The position to retrieve the time of.</param>
            /// <param name="endCap">Should the end cap be included?</param>
            /// <returns>The closest time at the specified position.</returns>
            public float GetTime(Vector3 position)
            {
                return GetTime(position, 0, 1);
            }

            /// <summary>
            /// Returns the closest time at the specified position.
            /// </summary>
            /// <param name="position">The position to retrieve the time of.</param>
            /// <param name="minTime">The minimum time to search within the curve.</param>
            /// <param name="maxTime">The maximum time to search within the curve.</param>
            /// <returns>The closest time at the specified position.</returns>
            public float GetTime(Vector3 position, float minTime, float maxTime)
            {
                float closestTime = 0f;
                float closestDistance = float.MaxValue;
                float step = (maxTime - minTime) / c_StepCount;
                var steps = c_StepCount + 1;
                // Walk the curve looking for the closest point to the specified position. Store the closest point and return the corresponding time to that point.
                for (int i = 0; i < steps; ++i) {
                    var t = minTime + step * i;
                    var distance = (GetPoint(t) - position).sqrMagnitude;
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        closestTime = t;
                    }
                }
                return closestTime;
            }

            /// <summary>
            /// Returns the closest point on the curve to the specified position.
            /// </summary>
            /// <param name="position">The position to retrieve the closest point on the curve of.</param>
            /// <param name="endCap">Should the curve's end cap be included?</param>
            /// <returns>The closest point on the curve to the specified position.</returns>
            public Vector3 GetClosestPoint(Vector3 position)
            {
                return GetPoint(GetTime(position));
            }
        }
    }
}