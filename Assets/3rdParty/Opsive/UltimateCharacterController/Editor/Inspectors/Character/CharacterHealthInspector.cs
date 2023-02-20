/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEditor;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using System;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Traits
{
    /// <summary>
    /// Shows a custom inspector for the CharacterHealth component.
    /// </summary>
    [CustomEditor(typeof(CharacterHealth), true)]
    public class CharacterHealthInspector : HealthInspector
    {
        /// <summary>
        /// Returns the actions to draw before the State list is drawn.
        /// </summary>
        /// <returns>The actions to draw before the State list is drawn.</returns>
        protected override Action GetHealthDrawCallback()
        {
            var baseCallback = base.GetHealthDrawCallback();

            baseCallback += () =>
            {
                if (Foldout("Fall Damage")) {
                    EditorGUI.indentLevel++;
                    var applyFallDamage = PropertyFromName("m_ApplyFallDamage");
                    EditorGUILayout.PropertyField(applyFallDamage);
                    if (applyFallDamage.boolValue) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(PropertyFromName("m_MinFallDamageHeight"));
                        EditorGUILayout.PropertyField(PropertyFromName("m_MinFallDamage"));
                        EditorGUILayout.PropertyField(PropertyFromName("m_MaxFallDamage"));
                        EditorGUILayout.PropertyField(PropertyFromName("m_DeathHeight"));
                        EditorGUILayout.PropertyField(PropertyFromName("m_DamageCurve"));
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }

                var damagedEffectValue = PropertyFromName("m_DamagedEffectName").stringValue;
                var newDamagedEffectValue = InspectorUtility.DrawTypePopup(typeof(UltimateCharacterController.Character.Effects.Effect), damagedEffectValue, "Damaged Effect", true);
                if (damagedEffectValue != newDamagedEffectValue) {
                    PropertyFromName("m_DamagedEffectName").stringValue = newDamagedEffectValue;
                    InspectorUtility.SetDirty(target);
                }
                if (!string.IsNullOrEmpty(newDamagedEffectValue)) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_DamagedEffectIndex"));
                    EditorGUI.indentLevel--;
                }
            };

            return baseCallback;
        }
    }
}