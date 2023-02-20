/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Motion;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Motion
{
    /// <summary>
    /// Shows a custom inspector for the Path component.
    /// </summary>
    [CustomEditor(typeof(Path))]
    public class PathInspector : UnityEditor.Editor
    {
        private const string c_EditorPrefsSelectedIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.Path.SelectedIndex";
        private const string c_EditorPrefsSelectedMiddleIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.Path.SelectedMiddleIndex";

        private string EditorPrefsSelectedIndexKey { get { return c_EditorPrefsSelectedIndexKey + "." + target.name; } }
        private string EditorPrefsSelectedMiddleIndexKey { get { return c_EditorPrefsSelectedMiddleIndexKey + "." + target.name; } }

        private Path m_Path;
        private int m_SelectedIndex = -1;
        private int m_SelectedMiddleIndex = -1;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public void OnEnable()
        {
            m_Path = target as Path;

            m_SelectedIndex = EditorPrefs.GetInt(EditorPrefsSelectedIndexKey, -1);
            m_SelectedMiddleIndex = EditorPrefs.GetInt(EditorPrefsSelectedMiddleIndexKey, -1);
        }

        /// <summary>
        /// Draws the inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            // Allow for fine grain control of the position.
            if (m_SelectedIndex != -1 && m_SelectedIndex < m_Path.ControlPoints.Length) {
                m_Path.ControlPoints[m_SelectedIndex] = EditorGUILayout.Vector3Field("Position", m_Path.ControlPoints[m_SelectedIndex]);
                AdjustControlPoint(m_SelectedIndex);
            }
            
            EditorGUILayout.BeginHorizontal();
            // Adds a new curve to the end of the path.
            if (GUILayout.Button("Add Curve Segment")) {
                var controlPoints = m_Path.ControlPoints;
                int count;
                var position = Vector3.zero;
                if (controlPoints == null || controlPoints.Length == 0) {
                    // A Cubic Bezier Curve requires a minimum of four points.
                    count = 4;
                    position = Vector3.up;
                    controlPoints = new Vector3[count];
                } else {
                    // The last point from the previous curve will be used as the fourth point for the new curve.
                    count = 3;
                    position = controlPoints[controlPoints.Length - 1];
                    System.Array.Resize(ref controlPoints, controlPoints.Length + count);
                }
                for (int i = 0; i < count; ++i) {
                    controlPoints[controlPoints.Length - i - 1] = position;
                    position.x += 1;
                }
                m_Path.ControlPoints = controlPoints;
                // If a new curve is added then the control point should be adjusted so the tangents are equal distance.
                if (controlPoints.Length > 4) {
                    AdjustControlPoint(controlPoints.Length - 3);
                }

                m_SelectedIndex = m_SelectedMiddleIndex = controlPoints.Length - 1;
                EditorPrefs.SetInt(EditorPrefsSelectedIndexKey, m_SelectedIndex);
                EditorPrefs.SetInt(EditorPrefsSelectedMiddleIndexKey, m_SelectedMiddleIndex);
                Repaint();
            }

            if (GUILayout.Button("Remove Last Curve Segment")) {
                var controlPoints = m_Path.ControlPoints;
                var count = m_Path.ControlPoints.Length > 4 ? 3 : 4;
                System.Array.Resize(ref controlPoints, controlPoints.Length - count);
                m_Path.ControlPoints = controlPoints;
                if (m_SelectedIndex >= controlPoints.Length) {
                    m_SelectedIndex = m_SelectedMiddleIndex = - 1;
                    EditorPrefs.SetInt(EditorPrefsSelectedIndexKey, m_SelectedIndex);
                    EditorPrefs.SetInt(EditorPrefsSelectedMiddleIndexKey, m_SelectedMiddleIndex);
                }
                Repaint();
            }
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.SetDirty(m_Path);
            }
        }

        /// <summary>
        /// Adjusts the control point at the specified index to have a continuous velocity.
        /// </summary>
        /// <param name="index">The index that is being adjusted.</param>
        private void AdjustControlPoint(int index)
        {
            // No adjustment needed if the control point isn't a tangent control point or on the ends of the curve.
            if (index <= 1 || index >= m_Path.ControlPoints.Length - 2 || index % 3 == 0) {
                return;
            }

            // Use integer division to obtain the middle control point index.
            var middleIndex = ((index + 1) / 3) * 3;
            int oppositeIndex;
            // If the index occurs before the middle index then the control point after the middle index should be adjusted, and visa-versa.
            // This will prevent the currently active control point from being modified to keep a consistent tangent.
            if (index <= middleIndex) {
                oppositeIndex = middleIndex + 1;
            } else {
                oppositeIndex = middleIndex - 1;
            }

            // The opposite control point's tangent should match the vector of the adjusted control point's tangent. This will allow for a consistent velocity.
            var midPoint = m_Path.ControlPoints[middleIndex];
            var tangent = midPoint - m_Path.ControlPoints[index];
            m_Path.ControlPoints[oppositeIndex] = midPoint + tangent;
        }

        /// <summary>
        /// Draws the curve to the scene view.
        /// </summary>
        public void OnSceneGUI()
        {
            if (m_Path.ControlPoints == null || m_Path.ControlPoints.Length == 0) {
                return;
            }

            // The control points are relative to the transform's local position.
            var transform = m_Path.transform.transform;
            Handles.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            var p0 = m_Path.ControlPoints[0] = DrawControlPoint(0);
            for (int i = 1; i < m_Path.ControlPoints.Length; i += 3) {
                var p1 = DrawControlPoint(i);
                var p2 = DrawControlPoint(i + 1);
                var p3 = DrawControlPoint(i + 2);
                Handles.color = Color.gray;
                if ((i - 1) == m_SelectedMiddleIndex) {
                    Handles.DrawLine(p0, p1);
                }
                if ((i + 2) == m_SelectedMiddleIndex) {
                    Handles.DrawLine(p2, p3);
                }
                Handles.color = Color.white;
                Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
                p0 = p3;
            }
        }

        /// <summary>
        /// Draws the control point.
        /// </summary>
        /// <param name="index">The index that should be drawn.</param>
        /// <returns>The position of the control point</returns>
        private Vector3 DrawControlPoint(int index)
        {
            var position = m_Path.ControlPoints[index];
            if (index % 3 != 0 && (index < m_SelectedMiddleIndex - 1 || index > m_SelectedMiddleIndex + 1)) {
                return position;
            }

            if (index == 0) {
                Handles.color = Color.green;
            } else if (index == m_Path.ControlPoints.Length - 1) {
                Handles.color = Color.red;
            } else {
                Handles.color = Color.white;
            }
            var size = HandleUtility.GetHandleSize(position) * 0.02f * (index % 3 == 0 ? 2 : 1);
            if (Handles.Button(position, Quaternion.identity, size, size * 4, Handles.DotHandleCap)) {
                m_SelectedIndex = index;
                EditorPrefs.SetInt(EditorPrefsSelectedIndexKey, m_SelectedIndex);
                if (index % 3 == 0) {
                    m_SelectedMiddleIndex = index;
                    EditorPrefs.SetInt(EditorPrefsSelectedMiddleIndexKey, m_SelectedMiddleIndex);
                }
                Repaint();
            }

            // If the control point is selected then the handles should be drawn so it can be repositioned.
            if (index == m_SelectedIndex) {
                EditorGUI.BeginChangeCheck();
                position = Handles.DoPositionHandle(m_Path.ControlPoints[index], Quaternion.identity);
                if (EditorGUI.EndChangeCheck()) {
                    // Middle control points should also move their tangents.
                    if (index % 3 == 0) {
                        var delta = position - m_Path.ControlPoints[index];
                        if (index > 0) {
                            m_Path.ControlPoints[index - 1] += delta;
                        }
                        if (index < m_Path.ControlPoints.Length - 1) {
                            m_Path.ControlPoints[index + 1] += delta;
                        }
                    }
                    m_Path.ControlPoints[index] = position;
                    AdjustControlPoint(index);
                    InspectorUtility.SetDirty(m_Path);
                }
            }
            return m_Path.ControlPoints[index];
        }
    }
}