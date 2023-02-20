/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem;
using System;

namespace Opsive.UltimateCharacterController.Editor.Inspectors
{
    /// <summary>
    /// Shows a custom inspector for the ThirdPersonMeleeWeaponProperties.
    /// </summary>
    [CustomEditor(typeof(ItemPerspectiveProperties))]
    public class ItemPerspectivePropertiesInspector : StateBehaviorInspector
    {
        /// <summary>
        /// Returns the actions to draw before the State list is drawn.
        /// </summary>
        /// <returns>The actions to draw before the State list is drawn.</returns>
        protected override Action GetDrawCallback()
        {
            var baseCallback = base.GetDrawCallback();

            baseCallback += () =>
            {
                EditorGUILayout.PropertyField(PropertyFromName("m_ActionID"));
            };

            return baseCallback;
        }
    }
}