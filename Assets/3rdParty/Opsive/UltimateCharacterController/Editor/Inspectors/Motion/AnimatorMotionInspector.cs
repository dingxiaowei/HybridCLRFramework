/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Motion;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Motion
{
    /// <summary>
    /// Shows a custom inspector for the AnimatorMotion component.
    /// </summary>
    [CustomEditor(typeof(AnimatorMotion))]
    public class AnimatorMotionInspector : InspectorBase
    {
        private AnimatorMotion m_AnimatorMotion;
        private float m_Duration;

        /// <summary>
        /// Creates a new AnimatorMotion.
        /// </summary>
        [MenuItem("Assets/Create/Ultimate Character Controller/Animator Motion")]
        public static void CreateStateConfiguration()
        {
            var path = EditorUtility.SaveFilePanel("Save Animator Motion", InspectorUtility.GetSaveFilePath(), "AnimatorMotion.asset", "asset");
            if (path.Length != 0 && Application.dataPath.Length < path.Length) {
                var animatorMotion = ScriptableObject.CreateInstance<AnimatorMotion>();

                // Save the asset file.
                path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(animatorMotion, path);
                AssetDatabase.ImportAsset(path);
            }
        }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public void OnEnable()
        {
            m_AnimatorMotion = target as AnimatorMotion;

            m_Duration = m_AnimatorMotion.XPosition.keys[m_AnimatorMotion.XPosition.length - 1].time;
        }

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // The duration should be consistent across animation curves.
            var duration = EditorGUILayout.FloatField(new GUIContent("Duration", "The duration of all of the animation curves."), m_Duration);
            if (duration != m_Duration) {
                m_Duration = duration;
                NormalizeTime();

                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                serializedObject.ApplyModifiedProperties();
            }

            // Draw the animation curves after the time has been displayed. The CurveField must be called manually because PropertyField doesn't update
            // after changing the time.
            EditorGUI.BeginChangeCheck();
            if (Foldout("Delta Position")) {
                EditorGUI.indentLevel++;
                m_AnimatorMotion.XPosition = EditorGUILayout.CurveField("X Position", m_AnimatorMotion.XPosition);
                m_AnimatorMotion.YPosition = EditorGUILayout.CurveField("Y Position", m_AnimatorMotion.YPosition);
                m_AnimatorMotion.ZPosition = EditorGUILayout.CurveField("Z Position", m_AnimatorMotion.ZPosition);
                EditorGUI.indentLevel--;
            }
            if (Foldout("Delta Rotation")) {
                EditorGUI.indentLevel++;
                m_AnimatorMotion.XRotation = EditorGUILayout.CurveField("X Rotation", m_AnimatorMotion.XRotation);
                m_AnimatorMotion.YRotation = EditorGUILayout.CurveField("Y Rotation", m_AnimatorMotion.YRotation);
                m_AnimatorMotion.ZRotation = EditorGUILayout.CurveField("Z Rotation", m_AnimatorMotion.ZRotation);
                EditorGUI.indentLevel--;
            }
            if (EditorGUI.EndChangeCheck()) {
                InspectorUtility.RecordUndoDirtyObject(target, "Change Value");
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Normalize all of the animation curves to have a consistent time.
        /// </summary>
        private void NormalizeTime()
        {
            NormalizeTime(m_AnimatorMotion.XPosition);
            NormalizeTime(m_AnimatorMotion.YPosition);
            NormalizeTime(m_AnimatorMotion.ZPosition);

            NormalizeTime(m_AnimatorMotion.XRotation);
            NormalizeTime(m_AnimatorMotion.YRotation);
            NormalizeTime(m_AnimatorMotion.ZRotation);
        }

        /// <summary>
        /// Normalize all of the specified animation curve to have a consistent time.
        /// </summary>
        /// <param name="animationCurve">The animation curve that should be normalized.</param>
        private void NormalizeTime(AnimationCurve animationCurve)
        {
            if (animationCurve.length < 2) {
                animationCurve = AnimationCurve.EaseInOut(0, 0, m_Duration, 0);
            } else {
                var key = animationCurve.keys[animationCurve.length - 1];
                key.time = m_Duration;
                animationCurve.MoveKey(animationCurve.length - 1, key);
            }
        }
    }
}