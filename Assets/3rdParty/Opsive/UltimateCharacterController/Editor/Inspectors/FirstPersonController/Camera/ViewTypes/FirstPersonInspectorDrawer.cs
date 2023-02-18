/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.FirstPersonController.Camera.ViewTypes
{
#if ULTIMATE_CHARACTER_CONTROLLER_LWRP || ULTIMATE_CHARACTER_CONTROLLER_UNIVERSALRP
    using Opsive.UltimateCharacterController.Camera;
#endif
    using Opsive.UltimateCharacterController.Editor.Inspectors.Camera;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.FirstPersonController.Camera.ViewTypes;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws a custom inspector for the First Person View Type.
    /// </summary>
    [InspectorDrawer(typeof(FirstPerson))]
    public class FirstPersonInspectorDrawer : ViewTypeInspectorDrawer
    {
        /// <summary>
        /// Called when the object should be drawn to the inspector.
        /// </summary>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="parent">The Unity Object that the object belongs to.</param>
        public override void OnInspectorGUI(object target, Object parent)
        {
            // Draw the fields related to rendering.
            if (InspectorUtility.Foldout(target, "Rendering")) {
                EditorGUI.indentLevel++;
                InspectorUtility.DrawField(target, "m_LookDirectionDistance");
                var lookOffsetValue = InspectorUtility.GetFieldValue<Vector3>(target, "m_LookOffset");
                var lookOffset = EditorGUILayout.Vector3Field(new GUIContent(InspectorUtility.SplitCamelCase("m_LookOffset"), InspectorUtility.GetFieldTooltip(target, "m_LookOffset")), lookOffsetValue);
                // Set the property if the game is playing so the camera will update.
                if (lookOffsetValue != lookOffset) {
                    (target as FirstPerson).LookOffset = lookOffset;
                    InspectorUtility.SetFieldValue(target, "m_LookOffset", lookOffset);
                }
                InspectorUtility.DrawField(target, "m_LookDownOffset");
                InspectorUtility.DrawField(target, "m_CullingMask");
                InspectorUtility.DrawFieldSlider(target, "m_FieldOfView", 1, 179);
                InspectorUtility.DrawFieldSlider(target, "m_FieldOfViewDamping", 0, 5);
#if ULTIMATE_CHARACTER_CONTROLLER_LWRP || ULTIMATE_CHARACTER_CONTROLLER_UNIVERSALRP
                var prevRenderType = InspectorUtility.GetFieldValue<FirstPerson.ObjectOverlayRenderType>(target, "m_OverlayRenderType");
                InspectorUtility.DrawField(target, "m_OverlayRenderType");
                var renderType = InspectorUtility.GetFieldValue<FirstPerson.ObjectOverlayRenderType>(target, "m_OverlayRenderType");
                // Ensure the render type has been switched for all first person view types.
                if (prevRenderType != renderType) {
                    if (renderType == FirstPerson.ObjectOverlayRenderType.SecondCamera) {
                        UltimateCharacterController.Utility.Builders.ViewTypeBuilder.AddFirstPersonCamera(parent as CameraController, target as FirstPerson);
                    } else if (prevRenderType == FirstPerson.ObjectOverlayRenderType.SecondCamera) {
                        var firstPersonCamera = InspectorUtility.GetFieldValue<UnityEngine.Camera>(target, "m_FirstPersonCamera");
                        if (firstPersonCamera != null) {
                            if (PrefabUtility.IsPartOfPrefabInstance(firstPersonCamera.gameObject)) {
                                PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(firstPersonCamera.gameObject), PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                            }
                            GameObject.DestroyImmediate(firstPersonCamera.gameObject, true);
                            (target as FirstPerson).FirstPersonCamera = null;
                        }
                    }
                    // All first person view types need to be switched to the currently active render type.
                    var viewTypes = (parent as CameraController).ViewTypes;
                    for (int i = 0; i < viewTypes.Length; ++i) {
                        var firstPersonViewType = viewTypes[i] as FirstPerson;
                        if (firstPersonViewType == null) {
                            continue;
                        }

                        if (renderType == FirstPerson.ObjectOverlayRenderType.RenderPipeline && firstPersonViewType.OverlayRenderType == FirstPerson.ObjectOverlayRenderType.SecondCamera) {
                            firstPersonViewType.FirstPersonCamera = null;
                            firstPersonViewType.OverlayRenderType = renderType;
                        } else if (renderType == FirstPerson.ObjectOverlayRenderType.SecondCamera && firstPersonViewType.OverlayRenderType == FirstPerson.ObjectOverlayRenderType.RenderPipeline) {
                            firstPersonViewType.FirstPersonCamera = (target as FirstPerson).FirstPersonCamera;
                            firstPersonViewType.OverlayRenderType = renderType;
                        }
                    }
                    UltimateCharacterController.Utility.Builders.ViewTypeBuilder.SerializeViewTypes(parent as CameraController);
                }
                var drawFirstPersonCamera = renderType == FirstPerson.ObjectOverlayRenderType.SecondCamera;
#else
                InspectorUtility.DrawField(target, "m_UseFirstPersonCamera");
                var drawFirstPersonCamera = InspectorUtility.GetFieldValue<bool>(target, "m_UseFirstPersonCamera");
#endif
                if (drawFirstPersonCamera && InspectorUtility.Foldout(target, "First Person Camera")) {
                    EditorGUI.indentLevel++;
                    InspectorUtility.DrawField(target, "m_FirstPersonCamera");
                    InspectorUtility.DrawField(target, "m_FirstPersonCullingMask");
                    InspectorUtility.DrawField(target, "m_SynchronizeFieldOfView");
                    var synchronizeFieldOfView = InspectorUtility.GetFieldValue<bool>(target, "m_SynchronizeFieldOfView");
                    if (!synchronizeFieldOfView) {
                        InspectorUtility.DrawFieldSlider(target, "m_FirstPersonFieldOfView", 1, 179);
                        InspectorUtility.DrawFieldSlider(target, "m_FirstPersonFieldOfViewDamping", 0, 5);
                    }
                    var positionOffsetValue = InspectorUtility.GetFieldValue<Vector3>(target, "m_FirstPersonPositionOffset");
                    var positionOffset = EditorGUILayout.Vector3Field(new GUIContent(InspectorUtility.SplitCamelCase("m_FirstPersonPositionOffset"), InspectorUtility.GetFieldTooltip(target, "m_FirstPersonPositionOffset")), positionOffsetValue);
                    // Set the property if the game is playing so the camera will update.
                    if (positionOffsetValue != positionOffset) {
                        (target as FirstPerson).FirstPersonPositionOffset = positionOffset;
                        InspectorUtility.SetFieldValue(target, "m_FirstPersonPositionOffset", positionOffset);
                    }
                    var rotationOffsetValue = InspectorUtility.GetFieldValue<Vector3>(target, "m_FirstPersonRotationOffset");
                    var rotationOffset = EditorGUILayout.Vector3Field(new GUIContent(InspectorUtility.SplitCamelCase("m_FirstPersonRotationOffset"), InspectorUtility.GetFieldTooltip(target, "m_FirstPersonRotationOffset")), rotationOffsetValue);
                    // Set the property so the camera will update.
                    if (rotationOffsetValue != rotationOffset) {
                        (target as FirstPerson).FirstPersonRotationOffset = rotationOffset;
                        InspectorUtility.SetFieldValue(target, "m_FirstPersonRotationOffset", rotationOffset);
                    }
                    EditorGUI.indentLevel--;
                }
#if ULTIMATE_CHARACTER_CONTROLLER_LWRP || ULTIMATE_CHARACTER_CONTROLLER_UNIVERSALRP
                else if (renderType == FirstPerson.ObjectOverlayRenderType.RenderPipeline) {
                    EditorGUI.indentLevel++;
                    InspectorUtility.DrawField(target, "m_FirstPersonCullingMask");
                    EditorGUI.indentLevel--;
                }
#endif
                EditorGUI.indentLevel--;
            }
            if (InspectorUtility.Foldout(target, "Primary Spring")) {
                EditorGUI.indentLevel++;
                InspectorUtility.DrawSpring(target, "Position Spring", "m_PositionSpring");
                var lowerLimit = InspectorUtility.GetFieldValue<float>(target, "m_PositionLowerVerticalLimit");
                var lowerLimitValue = EditorGUILayout.Slider(new GUIContent(InspectorUtility.SplitCamelCase("m_PositionLowerVerticalLimit"), 
                                        InspectorUtility.GetFieldTooltip(target, "m_PositionLowerVerticalLimit")), lowerLimit, 0, 5);
                // Set the property if the game is playing so the camera will update.
                if (lowerLimitValue != lowerLimit) {
                    (target as FirstPerson).PositionLowerVerticalLimit = lowerLimit;
                    InspectorUtility.SetFieldValue(target, "m_PositionLowerVerticalLimit", lowerLimitValue);
                }
                InspectorUtility.DrawFieldSlider(target, "m_PositionFallImpact", 0, 5);
                InspectorUtility.DrawFieldIntSlider(target, "m_PositionFallImpactSoftness", 1, 30);
                InspectorUtility.DrawFieldSlider(target, "m_RotationStrafeRoll", -5, 5);
                InspectorUtility.DrawSpring(target, "Rotation Spring", "m_RotationSpring");
                InspectorUtility.DrawFieldSlider(target, "m_RotationFallImpact", 0, 5);
                InspectorUtility.DrawFieldIntSlider(target, "m_RotationFallImpactSoftness", 1, 30);
                EditorGUI.indentLevel--;
            }
            if (InspectorUtility.Foldout(target, "Secondary Spring")) {
                EditorGUI.indentLevel++;
                InspectorUtility.DrawSpring(target, "Secondary Position Spring", "m_SecondaryPositionSpring");
                InspectorUtility.DrawSpring(target, "Secondary Rotation Spring", "m_SecondaryRotationSpring");
                EditorGUI.indentLevel--;
            }
            if (InspectorUtility.Foldout(target, "Shake")) {
                EditorGUI.indentLevel++;
                InspectorUtility.DrawFieldSlider(target, "m_ShakeSpeed", 0, 10);
                InspectorUtility.DrawField(target, "m_ShakeAmplitude");
                EditorGUI.indentLevel--;
            }
            if (InspectorUtility.Foldout(target, "Bob")) {
                EditorGUI.indentLevel++;
                InspectorUtility.DrawField(target, "m_BobPositionalRate");
                InspectorUtility.DrawField(target, "m_BobPositionalAmplitude");
                InspectorUtility.DrawField(target, "m_BobRollRate");
                InspectorUtility.DrawField(target, "m_BobRollAmplitude");
                InspectorUtility.DrawFieldSlider(target, "m_BobInputVelocityScale", 0, 10);
                InspectorUtility.DrawField(target, "m_BobMaxInputVelocity");
                InspectorUtility.DrawField(target, "m_BobMinTroughVerticalOffset");
                InspectorUtility.DrawField(target, "m_BobTroughForce");
                InspectorUtility.DrawField(target, "m_BobRequireGroundContact");
                EditorGUI.indentLevel--;
            }
            if (InspectorUtility.Foldout(target, "Limits")) {
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

                if (target is FreeLook) {
                    var minYawLimit = InspectorUtility.GetFieldValue<float>(target, "m_MinYawLimit");
                    var maxYawLimit = InspectorUtility.GetFieldValue<float>(target, "m_MaxYawLimit");
                    minValue = Mathf.Round(minYawLimit * 100f) / 100f;
                    maxValue = Mathf.Round(maxYawLimit * 100f) / 100f;
                    InspectorUtility.MinMaxSlider(ref minValue, ref maxValue, -180, 180, new GUIContent("Yaw Limit", "The min and max limit of the yaw angle (in degrees)."));
                    if (minValue != minYawLimit) {
                        InspectorUtility.SetFieldValue(target, "m_MinYawLimit", minValue);
                    }
                    if (minValue != maxYawLimit) {
                        InspectorUtility.SetFieldValue(target, "m_MaxYawLimit", maxValue);
                    }
                    InspectorUtility.DrawField(target, "m_YawLimitLerpSpeed");
                }
                InspectorUtility.DrawField(target, "m_LookDirectionDistance");
                EditorGUI.indentLevel--;
            }
            if (InspectorUtility.Foldout(target, "Head Tracking")) {
                EditorGUI.indentLevel++;
                InspectorUtility.DrawField(target, "m_SmoothHeadOffsetSteps");
                InspectorUtility.DrawField(target, "m_CollisionRadius");
                InspectorUtility.DrawField(target, "m_RotateWithHead");
                EditorGUI.indentLevel--;
            }
        }
    }
}