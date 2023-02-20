/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using System;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Items.Actions
{
    /// <summary>
    /// Shows a custom inspector for the ThrowableItem component.
    /// </summary>
    [CustomEditor(typeof(ThrowableItem))]
    public class ThrowableItemInspector : UsableItemInspector
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
                if (Foldout("Throw")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_ThrownObject"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ConsumableItemType"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_DisableVisibleObject"));
                    InspectorUtility.DrawAnimationEventTrigger(target, "Activate Throwable Object Event", PropertyFromName("m_ActivateThrowableObjectEvent"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ThrowOnStopUse"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_Velocity"));
                    (target as ThrowableItem).StartLayer = EditorGUILayout.LayerField(new GUIContent("Start Layer", "The layer that the item should occupy when initially spawned."), (target as ThrowableItem).StartLayer);
                    (target as ThrowableItem).ThrownLayer = EditorGUILayout.LayerField(new GUIContent("Thrown Layer", "The layer that the thrown object should change to after being thrown."), (target as ThrowableItem).ThrownLayer);
                    EditorGUILayout.PropertyField(PropertyFromName("m_LayerChangeDelay"));
                    DrawUsableProperties();
                    EditorGUI.indentLevel--;
                }

                if (Foldout("Impact")) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PropertyFromName("m_DamageAmount"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ImpactLayers"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ImpactForce"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ImpactForceFrames"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ImpactStateName"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ImpactStateDisableTimer"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_SurfaceImpact"));
                    EditorGUI.indentLevel--;
                }

                if (Foldout("Reequip")) {
                    EditorGUI.indentLevel++;
                    InspectorUtility.DrawAnimationEventTrigger(target, "Reequip Event", PropertyFromName("m_ReequipEvent"));
                    EditorGUILayout.PropertyField(PropertyFromName("m_ReequipItemSubstateParameterValue"));
                    EditorGUI.indentLevel--;
                }

                if (Foldout("Trajectory")) {
                    EditorGUI.indentLevel++;
                    var showTrajectoryProperty = PropertyFromName("m_ShowTrajectoryOnAim");
                    EditorGUILayout.PropertyField(showTrajectoryProperty);
                    if (showTrajectoryProperty.boolValue) {
                        EditorGUILayout.PropertyField(PropertyFromName("m_TrajectoryOffset"));
                    }
                    EditorGUI.indentLevel--;
                }
            };

            return baseCallback;
        }

        /// <summary>
        /// Callback which allows subclasses to draw any usable properties.
        /// </summary>
        protected virtual void DrawUsableProperties() { }
    }
}