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
    /// Custom inspector for the SurfaceImpact component.
    /// </summary>
    [CustomEditor(typeof(SurfaceImpact))]
    public class SurfaceImpactInspector : InspectorBase
    {
        /// <summary>
        /// Creates a new SurfaceImpact.
        /// </summary>
        [MenuItem("Assets/Create/Ultimate Character Controller/Surface Impact")]
        public static void CreateSurfaceImpact()
        {
            var path = EditorUtility.SaveFilePanel("Save Surface Impact", InspectorUtility.GetSaveFilePath(), "SurfaceImpact.asset", "asset");
            if (path.Length != 0 && Application.dataPath.Length < path.Length) {
                var surfaceImpact = ScriptableObject.CreateInstance<SurfaceImpact>();

                // Save the impact effect.
                path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(surfaceImpact, path);
                AssetDatabase.ImportAsset(path);
            }
        }

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}