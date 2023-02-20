/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
using System;

namespace Opsive.UltimateCharacterController.Editor.Managers
{
    /// <summary>
    /// Shows a starting window with useful links.
    /// </summary>
    [OrderedEditorItem("Welcome", 0)]
    public class WelcomeScreenManager : Manager
    {
        private const string c_DocumentationTextureGUID = "58591f58da2eed6429f27c500d2f5a98";
        private const string c_IntegrationsTextureGUID = "ecac100d11bb3dc40a93d7b1e30c015a";
        private const string c_ForumTextureGUID = "630622cb32bb7e64da8e2c1abbfdb1a3";
        private const string c_VideosTextureGUID = "fa530e1c250a12c4d88412795b5d8fa2";
        private const string c_DiscordTextureGUID = "b847fb48acf99c6478bfdc892f0276fc";
        private const string c_RateReviewTextureGUID = "32f45dfc0d71947458758e055696a118";
        private const string c_ShowcaseTextureGUID = "997f4ee10d474ab44ab9d9a030110117";

        Texture2D m_DocumentationTexture;
        Texture2D m_IntegrationsTexture;
        Texture2D m_ForumTexture;
        Texture2D m_VideosTexture;
        Texture2D m_DiscordTexture;
        Texture2D m_RateReviewTexture;
        Texture2D m_ShowcaseTexture;

        /// <summary>
        /// Initialize the manager after deserialization.
        /// </summary>
        public override void Initialize(MainManagerWindow mainManagerWindow)
        {
            base.Initialize(mainManagerWindow);

            m_DocumentationTexture = FindTexture(c_DocumentationTextureGUID);
            m_IntegrationsTexture = FindTexture(c_IntegrationsTextureGUID);
            m_ForumTexture = FindTexture(c_ForumTextureGUID);
            m_VideosTexture = FindTexture(c_VideosTextureGUID);
            m_DiscordTexture = FindTexture(c_DiscordTextureGUID);
            m_RateReviewTexture = FindTexture(c_RateReviewTextureGUID);
            m_ShowcaseTexture = FindTexture(c_ShowcaseTextureGUID);
        }

        /// <summary>
        /// Draws the Manager.
        /// </summary>
        public override void OnGUI()
        {
            EditorGUILayout.LabelField(string.Format("Thank you for purchasing the {0}.\nThe resources below will help you get the most out of the controller.", 
                                            UltimateCharacterController.Utility.AssetInfo.Name), InspectorStyles.WordWrapLabelCenter);
            // Draw the header image.
            GUILayout.BeginHorizontal();
            var width = m_MainManagerWindow.position.width - m_MainManagerWindow.MenuWidth - m_DocumentationTexture.width;
            GUILayout.Space(width / 2);
            GUILayout.Label(m_DocumentationTexture, InspectorStyles.CenterLabel, GUILayout.Width(m_DocumentationTexture.width), GUILayout.Height(m_DocumentationTexture.height));
            var lastRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseUp && lastRect.Contains(Event.current.mousePosition)) {
                Application.OpenURL("https://www.opsive.com/support/documentation/ultimate-character-controller/");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(3);

            // The remaining images should be drawn in a grid.
            GUILayout.BeginHorizontal();
            GUILayout.Space(width / 2 + 2);
            var selected = GUILayout.SelectionGrid(-1, 
                new Texture2D[] { m_IntegrationsTexture, m_ForumTexture, m_VideosTexture, m_DiscordTexture, m_RateReviewTexture, m_ShowcaseTexture }, 
                2,
                InspectorStyles.CenterLabel, GUILayout.Width(m_IntegrationsTexture.width * 2));
            if (selected != -1) {
                switch(selected) {
                    case 0:
                        Application.OpenURL(IntegrationsManager.GetIntegrationLink());
                        break;
                    case 1:
                        Application.OpenURL("https://www.opsive.com/forum/");
                        break;
                    case 2:
                        Application.OpenURL("https://opsive.com/support/documentation/ultimate-character-controller/videos/");
                        break;
                    case 3:
                        Application.OpenURL("https://discord.gg/QX6VFgc");
                        break;
                    case 4:
                        Application.OpenURL(GetAssetURL());
                        break;
                    case 5:
                        Application.OpenURL("https://www.opsive.com/showcase/");
                        break;
                }
            }
            GUILayout.EndHorizontal();

            // Draw the version at the bottom of the window.
            lastRect = GUILayoutUtility.GetLastRect();
            GUILayout.Space(m_MainManagerWindow.position.height - lastRect.yMax - 455);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(string.Format("{0} version {1}", UltimateCharacterController.Utility.AssetInfo.Name, UltimateCharacterController.Utility.AssetInfo.Version));
            try {
                var version = new Version(UltimateCharacterController.Utility.AssetInfo.Version);
                if (!string.IsNullOrEmpty(m_MainManagerWindow.LatestVersion) && version.CompareTo(new Version(m_MainManagerWindow.LatestVersion)) < 0) {
                    EditorGUILayout.LabelField(string.Format(" New version available: {0}", m_MainManagerWindow.LatestVersion));
                }
            } catch (Exception /*e*/) { }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Finds the texture based on the GUID.
        /// </summary>
        /// <param name="guid">The GUID to find the texture with.</param>
        /// <returns>The texture with the specified GUID.</returns>
        private Texture2D FindTexture(string guid)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(assetPath)) {
                return AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
            }
            return null;
        }

        /// <summary>
        /// Returns the URL for the asset page.
        /// </summary>
        /// <returns>The URL for the asset page.</returns>
        private string GetAssetURL()
        {
            switch (UltimateCharacterController.Utility.AssetInfo.Name) {
                case "Ultimate Character Controller":
                    return "https://assetstore.unity.com/packages/slug/99962";
                case "First Person Controller":
                    return "https://assetstore.unity.com/packages/slug/92082";
                case "Third Person Controller":
                    return "https://assetstore.unity.com/packages/slug/126347";
                case "Ultimate First Person Shooter":
                    return "https://assetstore.unity.com/packages/slug/106748";
                case "Ultimate First Person Melee":
                    return "https://assetstore.unity.com/packages/slug/99036";
                case "Ultimate Third Person Shooter":
                    return "https://assetstore.unity.com/packages/slug/99035";
                case "Ultimate Third Person Melee":
                    return "https://assetstore.unity.com/packages/slug/99037";
            }
            return string.Empty;
        }
    }
}