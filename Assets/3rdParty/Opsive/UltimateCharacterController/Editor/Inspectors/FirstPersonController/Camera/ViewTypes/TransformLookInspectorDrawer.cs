/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Camera;
using Opsive.UltimateCharacterController.Camera.ViewTypes;
using Opsive.UltimateCharacterController.FirstPersonController.Camera.ViewTypes;
using Opsive.UltimateCharacterController.Editor.Inspectors.Camera;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.FirstPersonController.Camera.ViewTypes
{
    /// <summary>
    /// Draws a custom inspector for the Transform Look View Type.
    /// </summary>
    [InspectorDrawer(typeof(TransformLook))]
    public class TransformLookInspectorDrawer : ViewTypeInspectorDrawer
    {
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
            var transformLook = viewType as TransformLook;
            transformLook.MoveTarget = animator.GetBoneTransform(HumanBodyBones.Head);
            transformLook.RotationTarget = animator.GetBoneTransform(HumanBodyBones.Hips);
        }
    }
}