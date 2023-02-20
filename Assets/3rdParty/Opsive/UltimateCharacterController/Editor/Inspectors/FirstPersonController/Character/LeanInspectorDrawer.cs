/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.FirstPersonController.Character.Abilities;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Utility;
using Opsive.UltimateCharacterController.Editor.Inspectors.Character;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using System;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.FirstPersonController.Character.Abilities
{
    /// <summary>
    /// Draws a custom inspector for the Lean Ability.
    /// </summary>
    [InspectorDrawer(typeof(Lean))]
    public class LeanInspectorDrawer : AbilityInspectorDrawer
    {
        /// <summary>
        /// The ability has been added to the Ultimate Character Locomotion. Perform any initialization.
        /// </summary>
        /// <param name="ability">The ability that has been added.</param>
        /// <param name="parent">The parent of the added ability.</param>
        public override void AbilityAdded(Ability ability, UnityEngine.Object parent)
        {
            AddLeanCollider(ability as Lean, (parent as Component).gameObject);
        }

        /// <summary>
        /// The ability has been removed from the Ultimate Character Locomotion. Perform any destruction.
        /// </summary>
        /// <param name="ability">The ability that has been removed.</param>
        /// <param name="parent">The parent of the removed ability.</param>
        public override void AbilityRemoved(Ability ability, UnityEngine.Object parent)
        {
            var leanAbility = ability as Lean;
            if (leanAbility.Collider != null) {
                RemoveLeanCollider(leanAbility, (parent as Component).gameObject);
            }
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
                var leanAbility = ability as Lean;
                GUI.enabled = leanAbility.Collider == null;
                if (GUILayout.Button("Add Lean Collider")) {
                    AddLeanCollider(leanAbility, (parent as Component).gameObject);
                }

                GUI.enabled = leanAbility.Collider != null;
                if (GUILayout.Button("Remove Lean Collider")) {
                    RemoveLeanCollider(leanAbility, (parent as Component).gameObject);
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            };

            return baseCallback;
        }

        /// <summary>
        /// Adds the collider to the lean ability.
        /// </summary>
        /// <param name="leanAbility">The ability to add the collider to.</param>
        /// <param name="parent">The parent of the lean ability.</param>
        private void AddLeanCollider(Lean leanAbility, GameObject parent)
        {
            // Position the collider under the Colliders GameObject if it exists.
            Transform collidersTransform;
            if ((collidersTransform = parent.transform.Find("Colliders"))) {
                parent = collidersTransform.gameObject;
            }
            var leanCollider = new GameObject("Lean Collider");
            leanCollider.layer = LayerManager.SubCharacter;
            leanCollider.transform.SetParentOrigin(parent.transform);
            leanCollider.transform.localPosition = new Vector3(0, 1.5f, 0);
            var leanCapsuleCollider = leanCollider.AddComponent<CapsuleCollider>();
            leanCapsuleCollider.radius = 0.3f;
            leanCapsuleCollider.height = 1;
            leanAbility.Collider = leanCapsuleCollider;
        }

        /// <summary>
        /// Removes the collider from the lean ability.
        /// </summary>
        /// <param name="leanAbility">The ability to remove the collider from.</param>
        /// <param name="parent">The parent of the lean ability.</param>
        private void RemoveLeanCollider(Lean leanAbility, GameObject parent)
        {
            UnityEngine.Object.DestroyImmediate(leanAbility.Collider.gameObject, true);
            leanAbility.Collider = null;
        }
    }
}