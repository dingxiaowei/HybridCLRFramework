/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.FirstPersonController.Camera.ViewTypes
{
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Camera.ViewTypes;
    using Opsive.UltimateCharacterController.FirstPersonController.Camera.ViewTypes;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Camera;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws a custom inspector for the Transform Look View Type.
    /// </summary>
    [InspectorDrawer(typeof(TransformLook))]
    public class TransformLookInspectorDrawer : ViewTypeInspectorDrawer
    {
        /// <summary>
        /// Called when the object should be drawn to the inspector.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        public override void OnInspectorGUI(object target, Object parent)
        {
            base.OnInspectorGUI(target, parent);

            if (!InspectorUtility.GetFieldValue<bool>(target, "m_RestrictPitch") && InspectorUtility.Foldout(target, "Limits")) {
                EditorGUI.indentLevel++;
                var minPitchLimit = InspectorUtility.GetFieldValue<float>(target, "m_MinPitchLimit");
                var maxPitchLimit = InspectorUtility.GetFieldValue<float>(target, "m_MaxPitchLimit");
                var minValue = Mathf.Round(minPitchLimit * 100f) / 100f;
                var maxValue = Mathf.Round(maxPitchLimit * 100f) / 100f;
                InspectorUtility.MinMaxSlider(ref minValue, ref maxValue, -90, 90, new GUIContent("Pitch Limit", "The min and max limit of the pitch angle (in degrees)."));
                if (minValue != minPitchLimit) {
                    InspectorUtility.SetFieldValue(target, "m_MinPitchLimit", minValue);
                }
                if (minValue != maxPitchLimit) {
                    InspectorUtility.SetFieldValue(target, "m_MaxPitchLimit", maxValue);
                }
                EditorGUI.indentLevel--;
            }
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
            var transformLook = viewType as TransformLook;
            transformLook.MoveTarget = animator.GetBoneTransform(HumanBodyBones.Head);
            transformLook.RotationTarget = animator.GetBoneTransform(HumanBodyBones.Hips);
        }
    }
}