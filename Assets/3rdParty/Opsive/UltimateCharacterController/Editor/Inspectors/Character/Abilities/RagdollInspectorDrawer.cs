/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character.Abilities
{
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Items;
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws a custom inspector for the Ragdoll Ability.
    /// </summary>
    [InspectorDrawer(typeof(Ragdoll))]
    public class RagdollInspectorDrawer : AbilityInspectorDrawer
    {
        /// <summary>
        /// Draws the fields related to the inspector drawer.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        protected override void DrawInspectorDrawerFields(object target, UnityEngine.Object parent)
        {
            InspectorUtility.DrawField(target, "m_StartOnDeath");
            InspectorUtility.DrawField(target, "m_StartDelay");
            var ragdollLayerFieldValue = InspectorUtility.GetFieldValue<int>(target, "m_RagdollLayer");
            var value = EditorGUILayout.LayerField(new GUIContent("Ragdoll Layer", InspectorUtility.GetFieldTooltip(target, "m_RagdollLayer")), ragdollLayerFieldValue);
            if (ragdollLayerFieldValue != value) {
                InspectorUtility.SetFieldValue(target, "m_RagdollLayer", value);
            }
            ragdollLayerFieldValue = InspectorUtility.GetFieldValue<int>(target, "m_InactiveRagdollLayer");
            value = EditorGUILayout.LayerField(new GUIContent("Inactive Ragdoll Layer", InspectorUtility.GetFieldTooltip(target, "m_InactiveRagdollLayer")), ragdollLayerFieldValue);
            if (ragdollLayerFieldValue != value) {
                InspectorUtility.SetFieldValue(target, "m_InactiveRagdollLayer", value);
            }
            InspectorUtility.DrawField(target, "m_CameraRotationalForce");

            base.DrawInspectorDrawerFields(target, parent);
        }

        /// <summary>
        /// Allows abilities to draw custom controls under the "Editor" foldout of the ability inspector.
        /// </summary>
        /// <param name="ability">The ability whose editor controls are being retrieved.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        /// <returns>Any custom editor controls. Can be null.</returns>
        public override Action GetEditorCallback(Ability ability, UnityEngine.Object parent)
        {
            var baseCallback = base.GetEditorCallback(ability, parent);

            baseCallback += () =>
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(InspectorUtility.IndentWidth * 2);
                if (GUILayout.Button("Add Ragdoll Colliders")) {
                    AddRagdollColliders((parent as Component).gameObject);
                }

                if (GUILayout.Button("Remove Ragdoll Colliders")) {
                    RemoveRagdollColliders((parent as Component).gameObject);
                }
                EditorGUILayout.EndHorizontal();
            };

            return baseCallback;
        }

        /// <summary>
        /// Uses Unity's Ragdoll Builder to create the ragdoll.
        /// </summary>
        /// <param name="character">The character to add the ragdoll to.</param>
        public static void AddRagdollColliders(GameObject character)
        {
            var ragdollBuilderType = Type.GetType("UnityEditor.RagdollBuilder, UnityEditor");
            var windows = Resources.FindObjectsOfTypeAll(ragdollBuilderType);
            // Open the Ragdoll Builder if it isn't already opened.
            if (windows == null || windows.Length == 0) {
                EditorApplication.ExecuteMenuItem("GameObject/3D Object/Ragdoll...");
                windows = Resources.FindObjectsOfTypeAll(ragdollBuilderType);
            }

            if (windows != null && windows.Length > 0) {
                var ragdollWindow = windows[0] as ScriptableWizard;
                var animator = character.GetComponent<Animator>();
                if (animator == null) {
                    return;
                }

                SetFieldValue(ragdollWindow, "pelvis", animator.GetBoneTransform(HumanBodyBones.Hips));
                SetFieldValue(ragdollWindow, "leftHips", animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg));
                SetFieldValue(ragdollWindow, "leftKnee", animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg));
                SetFieldValue(ragdollWindow, "leftFoot", animator.GetBoneTransform(HumanBodyBones.LeftFoot));
                SetFieldValue(ragdollWindow, "rightHips", animator.GetBoneTransform(HumanBodyBones.RightUpperLeg));
                SetFieldValue(ragdollWindow, "rightKnee", animator.GetBoneTransform(HumanBodyBones.RightLowerLeg));
                SetFieldValue(ragdollWindow, "rightFoot", animator.GetBoneTransform(HumanBodyBones.RightFoot));
                SetFieldValue(ragdollWindow, "leftArm", animator.GetBoneTransform(HumanBodyBones.LeftUpperArm));
                SetFieldValue(ragdollWindow, "leftElbow", animator.GetBoneTransform(HumanBodyBones.LeftLowerArm));
                SetFieldValue(ragdollWindow, "rightArm", animator.GetBoneTransform(HumanBodyBones.RightUpperArm));
                SetFieldValue(ragdollWindow, "rightElbow", animator.GetBoneTransform(HumanBodyBones.RightLowerArm));
                SetFieldValue(ragdollWindow, "middleSpine", animator.GetBoneTransform(HumanBodyBones.Spine));
                SetFieldValue(ragdollWindow, "head", animator.GetBoneTransform(HumanBodyBones.Head));

                var method = ragdollWindow.GetType().GetMethod("CheckConsistency", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null) {
                    ragdollWindow.errorString = (string)method.Invoke(ragdollWindow, null);
                    ragdollWindow.isValid = string.IsNullOrEmpty(ragdollWindow.errorString);
                }
            }
        }

        /// <summary>
        /// Use reflection to set the value of the field.
        /// </summary>
        private static void SetFieldValue(ScriptableWizard obj, string name, object value)
        {
            if (value == null) {
                return;
            }

            var field = obj.GetType().GetField(name);
            if (field != null) {
                field.SetValue(obj, value);
            }
        }

        /// <summary>
        /// Removes the ragdoll colliders from the specified character.
        /// </summary>
        /// <param name="character">The character to remove the ragdoll colliders from.</param>
        private void RemoveRagdollColliders(GameObject character)
        {
            // If the character is a humanoid then the ragdoll colliders are known ahead of time. Generic characters are required to be searched recursively.
            var animator = character.GetComponent<Animator>();
            if (animator != null && animator.GetBoneTransform(HumanBodyBones.Head) != null) {
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.Hips), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.LeftFoot), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.RightUpperLeg), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.RightLowerLeg), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.RightFoot), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.RightUpperArm), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.RightLowerArm), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.Spine), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.Head), false);
            } else {
                RemoveRagdollColliders(character.transform, true);
            }
        }

        /// <summary>
        /// Removes the ragdoll colliders from the transform. If removeChildColliders is true then the method will be called recursively.
        /// </summary>
        /// <param name="transform">The transform to remove the colliders from.</param>
        /// <param name="removeChildColliders">True if the colliders should be searched for recursively.</param>
        private void RemoveRagdollColliders(Transform transform, bool removeChildColliders)
        {
            if (transform == null) {
                return;
            }

            if (removeChildColliders) {
                var children = transform.childCount;
                for (int i = 0; i < transform.childCount; ++i) {
                    var child = transform.GetChild(i);
                    // No ragdoll colliders exist under the Character layer GameObjects no under the item GameObjects.
                    if (child.gameObject.layer == LayerManager.Character || child.GetComponent<ItemPlacement>() != null || child.GetComponent<ItemSlot>() != null) {
                        continue;
                    }

#if FIRST_PERSON_CONTROLLER
                    // First person objects do not contain any ragdoll colliders.
                    if (child.GetComponent<UltimateCharacterController.FirstPersonController.Character.FirstPersonObjects>() != null) {
                        continue;
                    }
#endif
                    // Remove the ragdoll from the transform and recursively check the children.
                    RemoveRagdollCollider(child);
                    RemoveRagdollColliders(child, true);
                }
            } else {
                RemoveRagdollCollider(transform);
            }
        }

        /// <summary>
        /// Removes the ragdoll colliders from the specified transform.
        /// </summary>
        /// <param name="transform">The transform to remove the ragdoll colliders from.</param>
        private void RemoveRagdollCollider(Transform transform)
        {
            var collider = transform.GetComponent<Collider>();
            var rigidbody = transform.GetComponent<Rigidbody>();
            // If the object doesn't have a collider and a rigidbody then it isn't a ragdoll collider.
            if (collider == null || rigidbody == null) {
                return;
            }
            UnityEngine.Object.DestroyImmediate(collider, true);
            var characterJoint = transform.GetComponent<CharacterJoint>();
            if (characterJoint != null) {
                UnityEngine.Object.DestroyImmediate(characterJoint, true);
            }
            // The rigidbody must be removed last to prevent conflicts.
            UnityEngine.Object.DestroyImmediate(rigidbody, true);
        }
    }
}