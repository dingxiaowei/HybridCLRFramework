/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Objects
{
    using Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Objects;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    /// <summary>
    /// Custom inspector for the MovingPlatform component.
    /// </summary>
    [CustomEditor(typeof(MovingPlatform), true)]
    public class MovingPlatformInspector : StateBehaviorInspector
    {
        private const int c_DelayWidth = 50;
        private const int c_StateWidth = 110;

        private ReorderableList m_WaypointReorderableList;
        private MovingPlatform m_Platform;
        private static GUIStyle s_DebugLabelStyle;

        /// <summary>
        /// Initializes the inspector.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            m_Platform = target as MovingPlatform;
        }

        /// <summary>
        /// Returns the actions to draw before the State list is drawn.
        /// </summary>
        /// <returns>The actions to draw before the State list is drawn.</returns>
        protected override Action GetDrawCallback()
        {
            var baseCallback = base.GetDrawCallback();

            baseCallback += () =>
            {
                EditorGUILayout.PropertyField(PropertyFromName("m_UpdateLocation"));
                if (Foldout("Path")) {
                    EditorGUI.indentLevel++;

                    if (m_WaypointReorderableList == null) {
                        if (m_Platform.Waypoints == null) {
                            m_Platform.Waypoints = new MovingPlatform.Waypoint[0];
                        }
                        m_WaypointReorderableList = new ReorderableList(m_Platform.Waypoints, typeof(MovingPlatform.Waypoint), true, true, true, true);
                        m_WaypointReorderableList.drawHeaderCallback = OnAnimatorWaypointStateListHeaderDraw;
                        m_WaypointReorderableList.drawElementCallback = OnAnimatorWaypointStateElementDraw;
                        m_WaypointReorderableList.onAddCallback = OnWaypointStateListAdd;
                        m_WaypointReorderableList.onRemoveCallback = OnWaypointStateListRemove;
                    }

                    // ReorderableLists do not like indentation.
                    var indentLevel = EditorGUI.indentLevel;
                    while (EditorGUI.indentLevel > 0) {
                        EditorGUI.indentLevel--;
                    }

                    var listRect = GUILayoutUtility.GetRect(0, m_WaypointReorderableList.GetHeight());
                    // Indent the list so it lines up with the rest of the content.
                    listRect.x += InspectorUtility.IndentWidth * indentLevel;
                    listRect.xMax -= InspectorUtility.IndentWidth * indentLevel;
                    m_WaypointReorderableList.DoList(listRect);
                    while (EditorGUI.indentLevel < indentLevel) {
                        EditorGUI.indentLevel++;
                    }

                    EditorGUILayout.PropertyField(PropertyFromName("m_Direction"));
                    var movementType = PropertyFromName("m_MovementType");
                    EditorGUILayout.PropertyField(movementType);
                    if (movementType.enumValueIndex == (int)MovingPlatform.PathMovementType.Target) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(PropertyFromName("m_TargetWaypoint"));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }

                if (Foldout("Movement")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_MovementSpeed"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_MovementInterpolation"));
                    var rotationInterpolation = PropertyFromName("m_RotationInterpolation");
                    EditorGUILayout.PropertyField(rotationInterpolation);
                    if (rotationInterpolation.enumValueIndex == (int)MovingPlatform.RotateInterpolationMode.CustomEaseOut) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(PropertyFromName("m_RotationEaseAmount"));
                        EditorGUI.indentLevel--;
                    } else if (rotationInterpolation.enumValueIndex == (int)MovingPlatform.RotateInterpolationMode.CustomRotate) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(PropertyFromName("m_CustomRotationSpeed"));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.PropertyField(PropertyFromName("m_MaxRotationDeltaAngle"));
                    EditorGUI.indentLevel--;
                }

                if (Foldout("Interaction")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_CharacterTriggerState"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_EnableOnInteract"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ChangeDirectionsOnInteract"));
                    EditorGUI.indentLevel--;
                }

                if (Foldout("Editor")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_GizmoColor"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_DrawDebugLabels"));
                    EditorGUI.indentLevel--;
                }
            };
            return baseCallback;
        }

        /// <summary>
        /// Draws the header for the WaypointState list.
        /// </summary>
        private void OnAnimatorWaypointStateListHeaderDraw(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x + 12, rect.y, rect.width - c_DelayWidth - c_StateWidth, EditorGUIUtility.singleLineHeight), "Transform");
            EditorGUI.LabelField(new Rect(rect.x + (rect.width - c_DelayWidth - c_StateWidth), rect.y, c_DelayWidth, EditorGUIUtility.singleLineHeight), "Delay");
            EditorGUI.LabelField(new Rect(rect.x + (rect.width - c_StateWidth) + 4, rect.y, c_StateWidth - 4, EditorGUIUtility.singleLineHeight), "State");
        }

        /// <summary>
        /// Draws the WaypointState element.
        /// </summary>
        private void OnAnimatorWaypointStateElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            m_Platform.Waypoints[index].Transform = (Transform)EditorGUI.ObjectField(new Rect(rect.x, rect.y + 1, (rect.width - c_DelayWidth - c_StateWidth - 2), EditorGUIUtility.singleLineHeight), 
                                                                m_Platform.Waypoints[index].Transform, typeof(Transform), true);
            m_Platform.Waypoints[index].Delay = EditorGUI.FloatField(new Rect(rect.x + (rect.width - c_DelayWidth - c_StateWidth), rect.y + 1, c_DelayWidth,
                                                                EditorGUIUtility.singleLineHeight), m_Platform.Waypoints[index].Delay);
            m_Platform.Waypoints[index].State = EditorGUI.TextField(new Rect(rect.x + (rect.width - c_StateWidth) + 4, rect.y + 1, c_StateWidth - 4, 
                                                                EditorGUIUtility.singleLineHeight), m_Platform.Waypoints[index].State);
        }

        /// <summary>
        /// Adds a new WaypointState element to the list.
        /// </summary>
        public void OnWaypointStateListAdd(ReorderableList list)
        {
            var waypoints = m_Platform.Waypoints;
            if (waypoints == null) {
                waypoints = new MovingPlatform.Waypoint[1];
            } else {
                Array.Resize(ref waypoints, waypoints.Length + 1);
            }
            list.list = m_Platform.Waypoints = waypoints;
            if (target != null) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
            }
        }

        /// <summary>
        /// Remove the WaypointState element at the list index.
        /// </summary>
        public void OnWaypointStateListRemove(ReorderableList list)
        {
            // Convert to a list and remove the waypoint. A new list needs to be assigned because a new allocation occurred.
            var waypointStateList = new List<MovingPlatform.Waypoint>(m_Platform.Waypoints);
            waypointStateList.RemoveAt(list.index);
            list.list = m_Platform.Waypoints = waypointStateList.ToArray();
            list.index = list.index - 1;
            if (target != null) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
            }
        }

        /// <summary>
        /// Draws the moving platform gizmo.
        /// </summary>
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawMovingPlatformGizmo(MovingPlatform movingPlatform, GizmoType gizmoType)
        {
            if (movingPlatform.Waypoints == null) {
                return;
            }

            Mesh mesh = null;
            var meshFilter = movingPlatform.GetComponent<MeshFilter>();
            if (meshFilter != null) {
                mesh = meshFilter.sharedMesh;
            }
            for (int i = 0; i < movingPlatform.Waypoints.Length; ++i) {
                if (movingPlatform.Waypoints[i].Transform == null) {
                    continue;
                }

                Gizmos.color = movingPlatform.GizmoColor;
                // Draw the mesh if it exists.
                if (mesh != null) {
                    Gizmos.DrawMesh(mesh, movingPlatform.Waypoints[i].Transform.position, movingPlatform.Waypoints[i].Transform.rotation, movingPlatform.transform.localScale);
                } 
                Gizmos.DrawWireSphere(movingPlatform.Waypoints[i].Transform.position, 0.5f);

                if (movingPlatform.DrawDebugLabels) {
                    if (s_DebugLabelStyle == null) {
                        s_DebugLabelStyle = new GUIStyle(EditorStyles.label);
                        s_DebugLabelStyle.fontSize = 16;
                        s_DebugLabelStyle.normal.textColor = InspectorUtility.GetContrastColor(movingPlatform.GizmoColor);
                    }
                    // Draw the delay in the center of the platform.
                    Handles.Label(movingPlatform.Waypoints[i].Transform.position, movingPlatform.Waypoints[i].Delay.ToString(), s_DebugLabelStyle);
                }

                // Draw a line connecting the platforms.
                if (i > 0 && movingPlatform.Waypoints[i - 1].Transform != null && movingPlatform.MovementType != MovingPlatform.PathMovementType.Target) {
                    Gizmos.color = InspectorUtility.GetContrastColor(movingPlatform.GizmoColor);
                    Gizmos.DrawLine(movingPlatform.Waypoints[i - 1].Transform.position, movingPlatform.Waypoints[i].Transform.position);

                    if (movingPlatform.DrawDebugLabels) {
                        // Draw a distance in the center of the line.
                        var distance = decimal.Round((decimal)Vector3.Distance(movingPlatform.Waypoints[i - 1].Transform.position, movingPlatform.Waypoints[i].Transform.position), 3);
                        Handles.Label((movingPlatform.Waypoints[i - 1].Transform.position + movingPlatform.Waypoints[i].Transform.position) / 2, distance.ToString(), s_DebugLabelStyle);
                    }
                }
            }

            // Complete the path drawing.
            if (movingPlatform.MovementType == MovingPlatform.PathMovementType.Loop && movingPlatform.Waypoints.Length > 0 && movingPlatform.Waypoints[0].Transform != null &&
                movingPlatform.Waypoints[movingPlatform.Waypoints.Length - 1].Transform != null) {
                Gizmos.color = InspectorUtility.GetContrastColor(movingPlatform.GizmoColor);
                Gizmos.DrawLine(movingPlatform.Waypoints[0].Transform.position, movingPlatform.Waypoints[movingPlatform.Waypoints.Length - 1].Transform.position);

                if (movingPlatform.DrawDebugLabels) {
                    // Draw a distance in the center of the line.
                    var distance = decimal.Round((decimal)Vector3.Distance(movingPlatform.Waypoints[0].Transform.position, movingPlatform.Waypoints[movingPlatform.Waypoints.Length - 1].Transform.position), 3);
                    Handles.Label((movingPlatform.Waypoints[0].Transform.position + movingPlatform.Waypoints[movingPlatform.Waypoints.Length - 1].Transform.position) / 2, distance.ToString(), s_DebugLabelStyle);
                }
            } else if (movingPlatform.MovementType == MovingPlatform.PathMovementType.Target && movingPlatform.TargetWaypoint < movingPlatform.Waypoints.Length && movingPlatform.Waypoints[movingPlatform.TargetWaypoint].Transform != null) {
                Gizmos.color = InspectorUtility.GetContrastColor(movingPlatform.GizmoColor);
                Gizmos.DrawLine(movingPlatform.transform.position, movingPlatform.Waypoints[movingPlatform.TargetWaypoint].Transform.position);
            }

            // Draw the current waypoint the platform is moving towards.
            if (Application.isPlaying && movingPlatform.enabled && movingPlatform.Waypoints.Length > 0) {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(movingPlatform.transform.position, movingPlatform.Waypoints[movingPlatform.NextWaypoint].Transform.position);
            }
        }
    }
}