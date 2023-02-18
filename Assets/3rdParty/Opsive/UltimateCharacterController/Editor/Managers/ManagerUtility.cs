/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Managers
{
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Utility functions for the manager classes.
    /// </summary>
    public static class ManagerUtility
    {
        private static string[] s_AnimatorControllerGUIDs = new string[] { "00734c75e5484e24697dddaf47e8c152", "1c65957c39679034fb94019d52d6a984", "79f4dab00da40824fbd3697b6c773522",
                                                                            "2d9ab56181c2ca34abcc6645243cf341", "e567772a993c11f448f9b69023c6cef6", "e58cef58c651b36498088253ec70c3ba",
                                                                            "7d702f1c77d91684ab1774d5ce14a714"};
        private const string c_ItemCollectionGUID = "5481010ef14c32f4cb7b6661b0c59fb4";
        private const string c_InvisibleShadowCasterGUID = "0a580a5ea04fdab47941095489aa23b7";
        private static string[] s_StateConfigurationGUIDs = new string[] { "9d35e75efc940dd4184470a31d744f39", "c7627c1aa2c6b264d87709008477a69e", "da4073e1f8f631445b1aea02f03f4760",
                                                                           "95e1719ba13cc9446b2b61a5993d5e43", "8481381869bbb8b4d8b4d1386e322d67", "bf3920a4d30a0744f9d4139fd46498ca",
                                                                           "e64c674322ee9dd47a9cf94762d7ff73"};

        private const string c_LastItemCollectionGUIDString = "LastItemCollectionGUID";
        private const string c_LastStateConfigurationGUIDString = "LastStateConfigurationGUID";

        public static string StateConfigurationGUID { get { return s_StateConfigurationGUIDs[0]; } }
        public static string LastItemCollectionGUIDString { get { return c_LastItemCollectionGUIDString; } }
        public static string LastStateConfigurationGUIDString { get { return c_LastStateConfigurationGUIDString; } }

        /// <summary>
        /// Draws a control box which allows for an action when the button is pressed.
        /// </summary>
        /// <param name="title">The title of the control box.</param>
        /// <param name="additionalControls">Any additional controls that should appear before the message.</param>
        /// <param name="message">The message within the box.</param>
        /// <param name="enableButton">Is the button enabled?</param>
        /// <param name="button">The name of the button.</param>
        /// <param name="action">The action that is performed when the button is pressed.</param>
        /// <param name="successLog">The message to output to the log upon success.</param>
        public static void DrawControlBox(string title, System.Action additionalControls, string message, bool enableButton, string button, System.Action action, string successLog)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(title, InspectorStyles.BoldLabel);
            GUILayout.Space(4);
            GUILayout.Label(message, InspectorStyles.WordWrapLabel);
            if (additionalControls != null) {
                additionalControls();
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.enabled = enableButton;
            if (!string.IsNullOrEmpty(button) && GUILayout.Button(button, GUILayout.Width(130))) {
                action();
                if (!string.IsNullOrEmpty(successLog)) {
                    Debug.Log(successLog);
                }
            }
            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Searches for the default animator controller.
        /// </summary>
        public static RuntimeAnimatorController FindAnimatorController(ScriptableObject editorWindow)
        {
            // The GUID should remain consistant.
            string animatorControllerPath;
            for (int i = 0; i < s_AnimatorControllerGUIDs.Length; ++i) {
                animatorControllerPath = AssetDatabase.GUIDToAssetPath(s_AnimatorControllerGUIDs[i]);
                if (!string.IsNullOrEmpty(animatorControllerPath)) {
                    var animatorController = AssetDatabase.LoadAssetAtPath(animatorControllerPath, typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
                    if (animatorController != null) {
                        return animatorController;
                    }
                }
            }

            // The animator controller doesn't have the expected guid. Try to find the asset based on the path.
            animatorControllerPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(editorWindow))).Replace("\\", "/").Replace("Editor/Managers", "Demo/Animator/Characters/Demo.controller");
            if (System.IO.File.Exists(Application.dataPath + "/" + animatorControllerPath.Substring(7))) {
                return AssetDatabase.LoadAssetAtPath(animatorControllerPath, typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
            }

            return null;
        }

        /// <summary>
        /// Searches for the default item collection.
        /// </summary>
        public static Inventory.ItemCollection FindItemCollection(ScriptableObject editorWindow)
        {
            // If an ItemCollection asset exists within the scene then use that.
            var itemSetManager = Object.FindObjectOfType<Inventory.ItemSetManager>();
            if (itemSetManager != null) {
                if (itemSetManager.ItemCollection != null) {
                    return itemSetManager.ItemCollection;
                }
            }

            // Retrieve the last used ItemCollection.
            var lastItemCollectionGUID = EditorPrefs.GetString(LastItemCollectionGUIDString, string.Empty);
            if (!string.IsNullOrEmpty(lastItemCollectionGUID)) {
                var lastItemCollectionPath = AssetDatabase.GUIDToAssetPath(lastItemCollectionGUID);
                if (!string.IsNullOrEmpty(lastItemCollectionPath)) {
                    var itemCollection = AssetDatabase.LoadAssetAtPath(lastItemCollectionPath, typeof(Inventory.ItemCollection)) as Inventory.ItemCollection;
                    if (itemCollection != null) {
                        return itemCollection;
                    }
                }
            }

            // The GUID should remain consistant.
            var itemCollectionPath = AssetDatabase.GUIDToAssetPath(c_ItemCollectionGUID);
            if (!string.IsNullOrEmpty(itemCollectionPath)) {
                var itemCollection = AssetDatabase.LoadAssetAtPath(itemCollectionPath, typeof(Inventory.ItemCollection)) as Inventory.ItemCollection;
                if (itemCollection != null) {
                    return itemCollection;
                }
            }

            // The item collection doesn't have the expected guid. Try to find the asset based on the path.
            itemCollectionPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(editorWindow))).Replace("\\", "/").Replace("Editor/Managers", "Demo/Inventory/DemoItemCollection.asset");
            if (System.IO.File.Exists(Application.dataPath + "/" + itemCollectionPath.Substring(7))) {
                return AssetDatabase.LoadAssetAtPath(itemCollectionPath, typeof(Inventory.ItemCollection)) as Inventory.ItemCollection;
            }

            // Last chance: use resources to try to find the ItemCollection.
            var itemCollections = Resources.FindObjectsOfTypeAll<Inventory.ItemCollection>();
            if (itemCollections != null && itemCollections.Length > 0) {
                return itemCollections[0];
            }

            return null;
        }

        /// <summary>
        /// Searches for the invisible shadow caster material.
        /// </summary>
        public static Material FindInvisibleShadowCaster(ScriptableObject editorWindow)
        {
            // The GUID should remain consistant. 
            var shadowCasterPath = AssetDatabase.GUIDToAssetPath(c_InvisibleShadowCasterGUID);
            if (!string.IsNullOrEmpty(shadowCasterPath)) {
                var invisibleShadowCaster = AssetDatabase.LoadAssetAtPath(shadowCasterPath, typeof(Material)) as Material;
                if (invisibleShadowCaster != null) {
                    return invisibleShadowCaster;
                }
            }

            if (editorWindow != null) {
                // The invisible shadow caster doesn't have the expected guid. Try to find the material based on the path.
                shadowCasterPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(editorWindow))).Replace("\\", "/").Replace("Editor/Managers", "FirstPersonController/Materials/InvisibleShadowCaster.mat");
                if (System.IO.File.Exists(Application.dataPath + "/" + shadowCasterPath.Substring(7))) {
                    return AssetDatabase.LoadAssetAtPath(shadowCasterPath, typeof(Material)) as Material;
                }
            }

            return null;
        }

        /// <summary>
        /// Searches for the default state configuration.
        /// </summary>
        public static StateSystem.StateConfiguration FindStateConfiguration(ScriptableObject editorWindow)
        {
            // Retrieve the last used StateConfiguration.
            var lastStateConfigurationGUID = EditorPrefs.GetString(LastStateConfigurationGUIDString, string.Empty);
            if (!string.IsNullOrEmpty(lastStateConfigurationGUID)) {
                var lastStateConfigurationPath = AssetDatabase.GUIDToAssetPath(lastStateConfigurationGUID);
                if (!string.IsNullOrEmpty(lastStateConfigurationPath)) {
                    var stateConfiguration = AssetDatabase.LoadAssetAtPath(lastStateConfigurationPath, typeof(StateSystem.StateConfiguration)) as StateSystem.StateConfiguration;
                    if (stateConfiguration != null) {
                        return stateConfiguration;
                    }
                }
            }

            // The GUID should remain consistant.
            string stateConfigurationPath;

            for (int i = 0; i < s_StateConfigurationGUIDs.Length; ++i) {
                stateConfigurationPath = AssetDatabase.GUIDToAssetPath(s_StateConfigurationGUIDs[i]);
                if (!string.IsNullOrEmpty(stateConfigurationPath)) {
                    var stateConfiguration = AssetDatabase.LoadAssetAtPath(stateConfigurationPath, typeof(StateSystem.StateConfiguration)) as StateSystem.StateConfiguration;
                    if (stateConfiguration != null) {
                        return stateConfiguration;
                    }
                }
            }

            // The state configuration doesn't have the expected guid. Try to find the asset based on the path.
            stateConfigurationPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(editorWindow))).Replace("\\", "/").Replace("Editor/Managers", "Demo/Presets/DemoStateConfiguration.asset");
            if (System.IO.File.Exists(Application.dataPath + "/" + stateConfigurationPath.Substring(7))) {
                return AssetDatabase.LoadAssetAtPath(stateConfigurationPath, typeof(StateSystem.StateConfiguration)) as StateSystem.StateConfiguration;
            }

            // Last chance: use resources to try to find the StateConfiguration.
            var stateConfigurations = Resources.FindObjectsOfTypeAll<StateSystem.StateConfiguration>();
            if (stateConfigurations != null && stateConfigurations.Length > 0) {
                return stateConfigurations[0];
            }

            return null;
        }
    }
}