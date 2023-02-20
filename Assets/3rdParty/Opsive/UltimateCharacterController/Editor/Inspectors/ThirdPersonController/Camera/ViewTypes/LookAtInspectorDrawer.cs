/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Camera;
using Opsive.UltimateCharacterController.Camera.ViewTypes;
using Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes;
using Opsive.UltimateCharacterController.Editor.Inspectors.Camera;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.ThirdPersonController.Camera.ViewTypes
{
    /// <summary>
    /// Draws a custom inspector for the Look At View Type.
    /// </summary>
    [InspectorDrawer(typeof(LookAt))]
    public class LookAtInspectorDrawer : ViewTypeInspectorDrawer
    {
        /// <summary>
        /// Called when the object should be drawn to the inspector.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        public override void OnInspectorGUI(object target, Object parent)
        {
            InspectorUtility.DrawField(target, "m_Target");
            InspectorUtility.DrawField(target, "m_Offset");
            InspectorUtility.DrawField(target, "m_MinLookDistance");
            InspectorUtility.DrawField(target, "m_MaxLookDistance");
            InspectorUtility.DrawField(target, "m_MoveSpeed");
            InspectorUtility.DrawField(target, "m_RotationalLerpSpeed");
            InspectorUtility.DrawField(target, "m_CollisionRadius");
            InspectorUtility.DrawSpring(target, "Rotation Spring", "m_RotationSpring");
        }

        /// <summary>
        /// The ability has been added to the camera. Perform any initialization.
        /// </summary>
        /// <param name="viewType">The view type that has been added.</param>
        /// <param name="parent">The parent of the added ability.</param>
        public override void ViewTypeAdded(ViewType viewType, Object parent)
        {
            var cameraController = parent as CameraController;
            if (cameraController.Character == null) {
                return;
            }

            var animator = cameraController.Character.GetComponent<Animator>();
            if (animator == null || !animator.isHuman) {
                return;
            }

            // Automatically set the Transform variables if the character is a humanoid.
            var lookAt = viewType as LookAt;
            lookAt.Target = animator.GetBoneTransform(HumanBodyBones.Head);
        }
    }
}