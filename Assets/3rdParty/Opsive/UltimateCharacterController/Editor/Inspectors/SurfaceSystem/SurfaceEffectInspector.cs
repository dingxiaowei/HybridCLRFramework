/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.SurfaceSystem;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.SurfaceSystem
{
    /// <summary>
    /// Custom inspector for the SurfaceEffect component.
    /// </summary>
    [CustomEditor(typeof(SurfaceEffect))]
    public class SurfaceEffectInspector : InspectorBase
    {
        /// <summary>
        /// Creates a new SurfaceEffect.
        /// </summary>
        [MenuItem("Assets/Create/Ultimate Character Controller/Surface Effect")]
        public static void CreateSurfaceEffect()
        {
            var path = EditorUtility.SaveFilePanel("Save Surface Effect", InspectorUtility.GetSaveFilePath(), "SurfaceEffect.asset", "asset");
            if (path.Length != 0 && Application.dataPath.Length < path.Length) {
                var surfaceType = ScriptableObject.CreateInstance<SurfaceEffect>();

                // Save the collection.
                path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(surfaceType, path);
                AssetDatabase.ImportAsset(path);
            }
        }

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(PropertyFromName("m_SpawnedObjects"), true);
            if (Foldout("Decals")) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName("m_Decals"), new GUIContent("Prefabs"), true);

                var minValue = InspectorUtility.GetFieldValue<float>(target, "m_MinDecalScale");
                var maxValue = InspectorUtility.GetFieldValue<float>(target, "m_MaxDecalScale");
                minValue = EditorGUILayout.Slider(new GUIContent("Min Scale", InspectorUtility.GetFieldTooltip(target, "m_MinScale")), minValue, 0.01f, 2f);
                if (minValue > maxValue) {
                    maxValue = minValue;
                }
                maxValue = EditorGUILayout.Slider(new GUIContent("Max Scale", InspectorUtility.GetFieldTooltip(target, "m_MaxScale")), maxValue, 0.01f, 2f);
                if (maxValue < minValue) {
                    minValue = maxValue;
                }
                InspectorUtility.SetFieldValue(target, "m_MinDecalScale", minValue);
                InspectorUtility.SetFieldValue(target, "m_MaxDecalScale", maxValue);
                EditorGUILayout.PropertyField(PropertyFromName("m_AllowedDecalEdgeOverlap"));
                EditorGUI.indentLevel--;
            }

            if (Foldout("Audio")) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName("m_AudioClips"), true);

                var minValue = InspectorUtility.GetFieldValue<float>(target, "m_MinAudioVolume");
                var maxValue = InspectorUtility.GetFieldValue<float>(target, "m_MaxAudioVolume");
                minValue = EditorGUILayout.Slider(new GUIContent("Min Volume", InspectorUtility.GetFieldTooltip(target, "m_MinAudioVolume")), minValue, 0, 1f);
                if (minValue > maxValue) {
                    maxValue = minValue;
                }
                maxValue = EditorGUILayout.Slider(new GUIContent("Max Volume", InspectorUtility.GetFieldTooltip(target, "m_MaxAudioVolume")), maxValue, 0, 1f);
                if (maxValue < minValue) {
                    minValue = maxValue;
                }
                InspectorUtility.SetFieldValue(target, "m_MinAudioVolume", minValue);
                InspectorUtility.SetFieldValue(target, "m_MaxAudioVolume", maxValue);

                minValue = InspectorUtility.GetFieldValue<float>(target, "m_MinAudioPitch");
                maxValue = InspectorUtility.GetFieldValue<float>(target, "m_MaxAudioPitch");
                minValue = EditorGUILayout.Slider(new GUIContent("Min Pitch", InspectorUtility.GetFieldTooltip(target, "m_MinAudioPitch")), minValue, -2f, 2f);
                if (minValue > maxValue) {
                    maxValue = minValue;
                }
                maxValue = EditorGUILayout.Slider(new GUIContent("Max Pitch", InspectorUtility.GetFieldTooltip(target, "m_MaxAudioPitch")), maxValue, -2f, 2f);
                if (maxValue < minValue) {
                    minValue = maxValue;
                }
                InspectorUtility.SetFieldValue(target, "m_MinAudioPitch", minValue);
                InspectorUtility.SetFieldValue(target, "m_MaxAudioPitch", maxValue);
                EditorGUILayout.PropertyField(PropertyFromName("m_OneClipPerFrame"));
                EditorGUILayout.PropertyField(PropertyFromName("m_RandomClipSelection"));

                EditorGUI.indentLevel--;
            }

            if (Foldout("State")) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PropertyFromName("m_StateName"));
                EditorGUILayout.PropertyField(PropertyFromName("m_StateDisableTimer"));
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}