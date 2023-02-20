/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Utility;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Game
{
    /// <summary>
    /// Shows the gizmo for the spawn point.
    /// </summary>
    public class SpawnPointGizmo
    {
        /// <summary>
        /// Draws the spawn point gizmo.
        /// </summary>
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawSpawnPointGizmo(SpawnPoint spawnPoint, GizmoType gizmoType)
        {
            var transform = spawnPoint.transform;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

            Gizmos.color = spawnPoint.GizmoColor;
            var position = Vector3.zero;
            if (spawnPoint.Shape == SpawnPoint.SpawnShape.Point || spawnPoint.Shape == SpawnPoint.SpawnShape.Sphere) {
                var size = spawnPoint.Shape == SpawnPoint.SpawnShape.Sphere ? spawnPoint.Size : 0.2f;
                Gizmos.DrawSphere(position, size);

                // Draw the outline when the component is selected.
                if (MathUtility.InLayerMask((int)GizmoType.Selected, (int)gizmoType)) {
                    Gizmos.color = InspectorUtility.GetContrastColor(spawnPoint.GizmoColor);
                    Gizmos.DrawWireSphere(position, size);
                }
            } else if (spawnPoint.Shape == SpawnPoint.SpawnShape.Box) {
                var size = Vector3.zero;
                size.x = size.z = spawnPoint.Size;
                size.y = spawnPoint.GroundSnapHeight;
                position += spawnPoint.transform.up * size.y / 2;
                Gizmos.DrawCube(position, size);

                // Draw the outline when the component is selected.
                if (MathUtility.InLayerMask((int)GizmoType.Selected, (int)gizmoType)) {
                    Gizmos.color = InspectorUtility.GetContrastColor(spawnPoint.GizmoColor);
                    Gizmos.DrawWireCube(position, size);
                }
            }

            if (MathUtility.InLayerMask((int)GizmoType.Selected, (int)gizmoType)) {
                // The Gizmo class cannot draw a wire disk.
                Handles.color = InspectorUtility.GetContrastColor(spawnPoint.GizmoColor);
                Handles.DrawWireDisc(spawnPoint.transform.position, spawnPoint.transform.up, 1);

                // Draw directional arrows when selected.
                var rad = spawnPoint.Size > 0 ? spawnPoint.Size : 1;
                if (spawnPoint.RandomDirection) {
                    // Draw four big arrows, relative to the spawnpoint and perpendicular to each other.
                    Gizmos.DrawLine((Vector3.back * 2) * rad, (Vector3.forward * 2) * rad);
                    Gizmos.DrawLine((Vector3.left * 2) * rad, (Vector3.right * 2) * rad);
                    Gizmos.DrawLine((Vector3.forward * 2) * rad, (Vector3.forward * 1.5f * rad) + (Vector3.left * 0.5f) * rad);
                    Gizmos.DrawLine((Vector3.forward * 2) * rad, (Vector3.forward * 1.5f * rad) + (Vector3.right * 0.5f) * rad);
                    Gizmos.DrawLine((Vector3.back * 2) * rad, (Vector3.back * 1.5f * rad) + (Vector3.left * 0.5f) * rad);
                    Gizmos.DrawLine((Vector3.back * 2) * rad, (Vector3.back * 1.5f * rad) + (Vector3.right * 0.5f) * rad);
                    Gizmos.DrawLine((Vector3.left * 2) * rad, (Vector3.left * 1.5f * rad) + (Vector3.forward * 0.5f) * rad);
                    Gizmos.DrawLine((Vector3.left * 2) * rad, (Vector3.left * 1.5f * rad) + (Vector3.back * 0.5f) * rad);
                    Gizmos.DrawLine((Vector3.right * 2) * rad, (Vector3.right * 1.5f * rad) + (Vector3.forward * 0.5f) * rad);
                    Gizmos.DrawLine((Vector3.right * 2) * rad, (Vector3.right * 1.5f * rad) + (Vector3.back * 0.5f) * rad);
                } else {
                    // Draw a single big arrow pointing in the spawnpoint's forward direction.
                    Gizmos.DrawLine(Vector3.zero, (Vector3.forward * 2) * rad);
                    Gizmos.DrawLine((Vector3.forward * 2) * rad, (Vector3.forward * 1.5f * rad) + (Vector3.left * 0.5f) * rad);
                    Gizmos.DrawLine((Vector3.forward * 2) * rad, (Vector3.forward * 1.5f * rad) + (Vector3.right * 0.5f) * rad);
                }
            }
        }
    }
}