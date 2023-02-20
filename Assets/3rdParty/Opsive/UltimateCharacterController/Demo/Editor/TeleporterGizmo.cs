/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Demo.Objects;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Demo
{
    /// <summary>
    /// Shows a custom gizmo for the Teleporter component.
    /// </summary>
    public class TeleporterGizmo
    {
        /// <summary>
        /// Draws the teleporter gizmo.
        /// </summary>
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawSpawnPointGizmo(Teleporter teleporter, GizmoType gizmoType)
        {
            var boxCollider = teleporter.GetComponent<BoxCollider>();
            if (boxCollider != null) {
                Gizmos.color = teleporter.GizmoColor;
                var transform = teleporter.transform;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.DrawCube(boxCollider.center, Vector3.Scale(boxCollider.size, boxCollider.transform.localScale));

                Gizmos.color = InspectorUtility.GetContrastColor(teleporter.GizmoColor);
                Gizmos.DrawWireCube(boxCollider.center, Vector3.Scale(boxCollider.size, boxCollider.transform.localScale));
            }

            if (teleporter.Destination != null) {
                Gizmos.color = teleporter.GizmoColor;
                var transform = teleporter.transform;
                Gizmos.matrix = Matrix4x4.TRS(teleporter.Destination.position, teleporter.Destination.rotation, teleporter.Destination.lossyScale);
                Gizmos.DrawSphere(Vector3.zero, 0.2f);

                Gizmos.color = InspectorUtility.GetContrastColor(teleporter.GizmoColor);
                Gizmos.DrawWireSphere(Vector3.zero, 0.2f);
            }
        }
    }
}