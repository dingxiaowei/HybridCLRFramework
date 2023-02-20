/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.FirstPersonController.Character;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using Opsive.UltimateCharacterController.Editor.Inspectors.StateSystem;
using System;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.FirstPersonController.Character
{
    /// <summary>
    /// Shows a custom inspector for the FirstPersonObjects.
    /// </summary>
    [CustomEditor(typeof(FirstPersonObjects))]
    public class FirstPersonObjectsInspector : StateBehaviorInspector
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
                // The value need to be changed through the property so at runtime any cached values will be updated.
                var minLimit = PropertyFromName("m_MinPitchLimit").floatValue;
                var maxLimit = PropertyFromName("m_MaxPitchLimit").floatValue;
                var minValue = Mathf.Round(minLimit * 100f) / 100f;
                var maxValue = Mathf.Round(maxLimit * 100f) / 100f;
                InspectorUtility.MinMaxSlider(ref minValue, ref maxValue, -90, 90, new GUIContent("Pitch Limit", "The min and max limit of the pitch angle (in degrees)."));
                if (minValue != minLimit) {
                    (target as FirstPersonObjects).MinPitchLimit = minValue;
                }
                if (maxValue != maxLimit) {
                    (target as FirstPersonObjects).MaxPitchLimit = maxValue;
                }

                var property = PropertyFromName("m_LockPitch");
                var value = property.boolValue;
                EditorGUILayout.PropertyField(property);
                if (value != property.boolValue) {
                    (target as FirstPersonObjects).LockPitch = property.boolValue;
                }

                minLimit = PropertyFromName("m_MinYawLimit").floatValue;
                maxLimit = PropertyFromName("m_MaxYawLimit").floatValue;
                minValue = Mathf.Round(minLimit * 100f) / 100f;
                maxValue = Mathf.Round(maxLimit * 100f) / 100f;
                InspectorUtility.MinMaxSlider(ref minValue, ref maxValue, -180, 180, new GUIContent("Yaw Limit", "The min and max limit of the yaw angle (in degrees)."));
                if (minValue != minLimit) {
                    (target as FirstPersonObjects).MinYawLimit = minValue;
                }
                if (maxValue != maxLimit) {
                    (target as FirstPersonObjects).MaxYawLimit = maxValue;
                }

                property = PropertyFromName("m_LockYaw");
                value = property.boolValue;
                EditorGUILayout.PropertyField(property);
                if (value != property.boolValue) {
                    (target as FirstPersonObjects).LockYaw = property.boolValue;
                }

                property = PropertyFromName("m_RotateWithCrosshairs");
                value = property.boolValue;
                EditorGUILayout.PropertyField(property);
                if (value != property.boolValue) {
                    (target as FirstPersonObjects).RotateWithCrosshairs = property.boolValue;
                }
                EditorGUILayout.PropertyField(PropertyFromName("m_RotationSpeed"));

                property = PropertyFromName("m_IgnorePositionalLookOffset");
                value = property.boolValue;
                EditorGUILayout.PropertyField(property);
                if (value != property.boolValue) {
                    (target as FirstPersonObjects).IgnorePositionalLookOffset = property.boolValue;
                }
                EditorGUILayout.PropertyField(PropertyFromName("m_PositionOffset"));
                EditorGUILayout.PropertyField(PropertyFromName("m_MoveSpeed"));
            };

            return baseCallback;
        }
    }
}