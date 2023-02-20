/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.ThirdPersonController.Character.Abilities.Items;
using Opsive.UltimateCharacterController.Utility;
using Opsive.UltimateCharacterController.Editor.Inspectors.Character;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using System;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.ThirdPersonController.Character.Abilities
{
    /// <summary>
    /// Draws a custom inspector for the ItemPullback Ability.
    /// </summary>
    [InspectorDrawer(typeof(ItemPullback))]
    public class ItemPullbackColliderInspector : AbilityInspectorDrawer
    {
        /// <summary>
        /// The ability has been added to the Ultimate Character Locomotion. Perform any initialization.
        /// </summary>
        /// <param name="ability">The ability that has been added.</param>
        /// <param name="parent">The parent of the added ability.</param>
        public override void AbilityAdded(Ability ability, UnityEngine.Object parent)
        {
            AddCollider(ability as ItemPullback, (parent as Component).gameObject);
        }

        /// <summary>
        /// The ability has been removed from the Ultimate Character Locomotion. Perform any destruction.
        /// </summary>
        /// <param name="ability">The ability that has been removed.</param>
        /// <param name="parent">The parent of the removed ability.</param>
        public override void AbilityRemoved(Ability ability, UnityEngine.Object parent)
        {
            var itemPullbackAbility = ability as ItemPullback;
            if (itemPullbackAbility.Collider != null) {
                RemoveCollider(itemPullbackAbility, (parent as Component).gameObject);
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
                var itemPullbackAbility = ability as ItemPullback;
                GUI.enabled = itemPullbackAbility.Collider == null;
                if (GUILayout.Button("Add Collider")) {
                    AddCollider(itemPullbackAbility, (parent as Component).gameObject);
                }

                GUI.enabled = itemPullbackAbility.Collider != null;
                if (GUILayout.Button("Remove Collider")) {
                    RemoveCollider(itemPullbackAbility, (parent as Component).gameObject);
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            };

            return baseCallback;
        }

        /// <summary>
        /// Adds the collider to the ability.
        /// </summary>
        /// <param name="itemPullbackAbility">The ability to add the collider to.</param>
        /// <param name="parent">The parent of the item pullback ability.</param>
        private void AddCollider(ItemPullback itemPullbackAbility, GameObject parent)
        {
            // Position the collider under the Colliders GameObject if it exists.
            Transform collidersTransform;
            if ((collidersTransform = parent.transform.Find("Colliders"))) {
                parent = collidersTransform.gameObject;
            }
            var itemPullbackCollider = new GameObject("Item Pullback Collider");
            itemPullbackCollider.layer = LayerManager.SubCharacter;
            itemPullbackCollider.transform.SetParentOrigin(parent.transform);
            itemPullbackCollider.transform.localPosition = new Vector3(0, 1.5f, 0.65f);
            var itemPullbackCapsuleCollider = itemPullbackCollider.AddComponent<CapsuleCollider>();
            itemPullbackCapsuleCollider.radius = 0.25f;
            itemPullbackCapsuleCollider.height = 1;
            itemPullbackAbility.Collider = itemPullbackCapsuleCollider;
            itemPullbackCollider.AddComponent<ItemPullbackCollider>();
        }

        /// <summary>
        /// Removes the collider from the ability.
        /// </summary>
        /// <param name="itemPullbackAbility">The ability to remove the collider from.</param>
        /// <param name="parent">The parent of the item pullback ability.</param>
        private void RemoveCollider(ItemPullback itemPullbackAbility, GameObject parent)
        {
            UnityEngine.Object.DestroyImmediate(itemPullbackAbility.Collider.gameObject, true);
            itemPullbackAbility.Collider = null;
        }
    }
}