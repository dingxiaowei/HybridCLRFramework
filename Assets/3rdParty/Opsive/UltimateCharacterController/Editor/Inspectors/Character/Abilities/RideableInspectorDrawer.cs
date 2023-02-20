/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Utility;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using System;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character.Abilities
{
    /// <summary>
    /// Draws a custom inspector for the Lean Ability.
    /// </summary>
    [InspectorDrawer(typeof(Rideable))]
    public class RideableInspectorDrawer : AbilityInspectorDrawer
    {
        /// <summary>
        /// The ability has been added to the Ultimate Character Locomotion. Perform any initialization.
        /// </summary>
        /// <param name="ability">The ability that has been added.</param>
        /// <param name="parent">The parent of the added ability.</param>
        public override void AbilityAdded(Ability ability, UnityEngine.Object parent)
        {
            AddDismountColliders(ability as Rideable, (parent as Component).gameObject);
        }

        /// <summary>
        /// The ability has been removed from the Ultimate Character Locomotion. Perform any destruction.
        /// </summary>
        /// <param name="ability">The ability that has been removed.</param>
        /// <param name="parent">The parent of the removed ability.</param>
        public override void AbilityRemoved(Ability ability, UnityEngine.Object parent)
        {
            RemoveDismountColliders(ability as Rideable, (parent as Component).gameObject);
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
                var rideableAbility = ability as Rideable;
                GUI.enabled = rideableAbility.LeftDismountCollider == null || rideableAbility.RightDismountCollider == null;
                if (GUILayout.Button("Add Dismount Colliders")) {
                    AddDismountColliders(rideableAbility, (parent as Component).gameObject);
                }

                GUI.enabled = rideableAbility.LeftDismountCollider != null || rideableAbility.RightDismountCollider != null;
                if (GUILayout.Button("Remove Dismount Colliders")) {
                    RemoveDismountColliders(rideableAbility, (parent as Component).gameObject);
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            };

            return baseCallback;
        }

        /// <summary>
        /// Adds the colliders to the rideable ability.
        /// </summary>
        /// <param name="rideable">The ability to add the colliders to.</param>
        /// <param name="parent">The parent of the rideable ability.</param>
        private void AddDismountColliders(Rideable rideableAbility, GameObject parent)
        {
            // Position the collider under the Colliders GameObject if it exists.
            Transform collidersTransform;
            if ((collidersTransform = parent.transform.Find("Colliders"))) {
                parent = collidersTransform.gameObject;
            }
            rideableAbility.LeftDismountCollider = CreateCollider(parent, "Left Dismount Collider", new Vector3(-0.9f, 1, 0));
            rideableAbility.RightDismountCollider = CreateCollider(parent, "Right Dismount Collider", new Vector3(0.9f, 1, 0));
        }

        /// <summary>
        /// Creates the dismount collider
        /// </summary>
        /// <param name="parent">The parent of the rideable ability.</param>
        /// <param name="name">The name of the collider.</param>
        /// <param name="position">The local poistion of the collider.</param>
        /// <returns>The created collider.</returns>
        private Collider CreateCollider(GameObject parent, string name, Vector3 position)
        {
            var collider = new GameObject(name);
            collider.layer = LayerManager.SubCharacter;
            collider.transform.SetParentOrigin(parent.transform);
            collider.transform.localPosition = position;
            var capsuleCollider = collider.AddComponent<CapsuleCollider>();
            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 1.5f;
            return capsuleCollider;
        }

        /// <summary>
        /// Removes the collider from the rideable ability.
        /// </summary>
        /// <param name="rideableAbility">The ability to remove the colliders from.</param>
        /// <param name="parent">The parent of the rideable ability.</param>
        private void RemoveDismountColliders(Rideable rideableAbility, GameObject parent)
        {
            if (rideableAbility.LeftDismountCollider != null) {
                UnityEngine.Object.DestroyImmediate(rideableAbility.LeftDismountCollider.gameObject, true);
                rideableAbility.LeftDismountCollider = null;
            }
            if (rideableAbility.RightDismountCollider != null) {
                UnityEngine.Object.DestroyImmediate(rideableAbility.RightDismountCollider.gameObject, true);
                rideableAbility.RightDismountCollider = null;
            }
        }
    }
}