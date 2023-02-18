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
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Draws the inspector for an integrations that has been installed.
    /// </summary>
    public abstract class IntegrationInspector
    {
        protected MainManagerWindow m_MainManagerWindow;
        public MainManagerWindow MainManagerWindow { set { m_MainManagerWindow = value; } }

        /// <summary>
        /// Draws the integration inspector.
        /// </summary>
        public abstract void DrawInspector();
    }

    /// <summary>
    /// Draws a list of all of the available integrations.
    /// </summary>
    [OrderedEditorItem("Integrations", 10)]
    public class IntegrationsManager : Manager
    {
        private const int c_IntegrationCellWidth = 270;
        private const int c_IntegrationCellHeight = 100;
        private const int c_IntegrationCellSpacing = 5;

        private string[] m_ToolbarStrings = { "Integration Inspectors", "Available Integrations" };
        [SerializeField] private bool m_DrawIntegrationInspectors;
        [SerializeField] private bool m_Initialized;

        private IntegrationInspector[] m_IntegrationInspectors;
        private string[] m_IntegrationNames;

        private Vector2 m_InstalledScrollPosition;
        private Vector2 m_AvailableScrollPosition;

        private static GUIStyle s_IntegrationAssetTitle;
        private static GUIStyle IntegrationAssetTitle
        {
            get
            {
                if (s_IntegrationAssetTitle == null) {
                    s_IntegrationAssetTitle = new GUIStyle(InspectorStyles.CenterBoldLabel);
                    s_IntegrationAssetTitle.fontSize = 14;
                    s_IntegrationAssetTitle.alignment = TextAnchor.MiddleLeft;
                }
                return s_IntegrationAssetTitle;
            }
        }

        /// <summary>
        /// Stores the information about the integration asset.
        /// </summary>
        private class AssetIntegration
        {
            private const int c_IconSize = 78;

            private int m_ID;
            private string m_Name;
            private string m_IntegrationURL;
            private Texture2D m_Icon;
            private MainManagerWindow m_MainManagerWindow;

            private UnityEngine.Networking.UnityWebRequest m_IconRequest;
            private UnityEngine.Networking.DownloadHandlerTexture m_TextureDownloadHandler;

            /// <summary>
            /// Constructor for the AssetIntegration class.
            /// </summary>
            public AssetIntegration(int id, string name, string iconURL, string integrationURL, MainManagerWindow mainManagerWindow)
            {
                m_ID = id;
                m_Name = name;
                m_IntegrationURL = integrationURL;
                m_MainManagerWindow = mainManagerWindow;

                // Start loading the icon as soon as the url is retrieved.
                m_TextureDownloadHandler = new UnityEngine.Networking.DownloadHandlerTexture();
                m_IconRequest = UnityEngine.Networking.UnityWebRequest.Get(iconURL);
                m_IconRequest.downloadHandler = m_TextureDownloadHandler;
                m_IconRequest.SendWebRequest();
            }

            /// <summary>
            /// Draws the integration details at the specified position.
            /// </summary>
            public void DrawIntegration(Vector2 position)
            {
                if (m_IconRequest != null) {
                    if (m_IconRequest.isDone) {
                        if (string.IsNullOrEmpty(m_IconRequest.error)) {
                            m_Icon = m_TextureDownloadHandler.texture;
                        }
                        m_IconRequest = null;
                    } else {
                        m_MainManagerWindow.Repaint();
                    }
                }

                // Draw the icon, name, and integration/Asset Store link.
                if (m_Icon != null) {
                    GUI.DrawTexture(new Rect(position.x, position.y, c_IconSize, c_IconSize), m_Icon);
                }

                var rect = new Rect(position.x + c_IconSize + 10, position.y + 3, 250, 18);
                EditorGUI.LabelField(rect, m_Name, IntegrationAssetTitle);

                if (!string.IsNullOrEmpty(m_IntegrationURL) && GUI.Button(new Rect(rect.x, rect.y + 23, 80, 18), "Integration")) {
                    Application.OpenURL(m_IntegrationURL);
                }

                if (m_ID > 0 && GUI.Button(new Rect(rect.x, rect.y + (string.IsNullOrEmpty(m_IntegrationURL) ? 23 : 47), 80, 18), "Asset Store")) {
                    Application.OpenURL("https://opsive.com/asset/UltimateCharacterController/AssetRedirect.php?asset=" + m_ID);
                }
            }
        }

        private Vector2 m_ScrollPosition;
        private UnityEngine.Networking.UnityWebRequest m_IntegrationsReqest;
        private AssetIntegration[] m_Integrations;

        /// <summary>
        /// Initialize the manager after deserialization.
        /// </summary>
        public override void Initialize(MainManagerWindow mainManagerWindow)
        {
            base.Initialize(mainManagerWindow);

            BuildInstalledIntegrations();

            if (!m_Initialized) { 
                m_DrawIntegrationInspectors = m_IntegrationInspectors != null && m_IntegrationInspectors.Length > 0;
                m_Initialized = true;
            }
        }

        /// <summary>
        /// Finds and create an instance of the inspectors for all of the installed integrations.
        /// </summary>
        private void BuildInstalledIntegrations()
        {
            var integrationInspectors = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var integrationIndexes = new List<int>();
            for (int i = 0; i < assemblies.Length; ++i) {
                var assemblyTypes = assemblies[i].GetTypes();
                for (int j = 0; j < assemblyTypes.Length; ++j) {
                    // Must implement IntegrationInspector.
                    if (!typeof(IntegrationInspector).IsAssignableFrom(assemblyTypes[j])) {
                        continue;
                    }

                    // Ignore abstract classes.
                    if (assemblyTypes[j].IsAbstract) {
                        continue;
                    }

                    // A valid inspector class.
                    integrationInspectors.Add(assemblyTypes[j]);
                    var index = integrationIndexes.Count;
                    if (assemblyTypes[j].GetCustomAttributes(typeof(OrderedEditorItem), true).Length > 0) {
                        var item = assemblyTypes[j].GetCustomAttributes(typeof(OrderedEditorItem), true)[0] as OrderedEditorItem;
                        index = item.Index;
                    }
                    integrationIndexes.Add(index);
                }
            }

            // Do not reinitialize the inspectors if they are already initialized and there aren't any changes.
            if (m_IntegrationInspectors != null && m_IntegrationInspectors.Length == integrationInspectors.Count) {
                return;
            }

            // All of the manager types have been found. Sort by the index.
            var inspectorTypes = integrationInspectors.ToArray();
            Array.Sort(integrationIndexes.ToArray(), inspectorTypes);

            m_IntegrationInspectors = new IntegrationInspector[integrationInspectors.Count];
            m_IntegrationNames = new string[integrationInspectors.Count];

            // The inspector types have been found and sorted. Add them to the list.
            for (int i = 0; i < inspectorTypes.Length; ++i) {
                m_IntegrationInspectors[i] = Activator.CreateInstance(inspectorTypes[i]) as IntegrationInspector;
                m_IntegrationInspectors[i].MainManagerWindow = m_MainManagerWindow;

                var name = InspectorUtility.SplitCamelCase(inspectorTypes[i].Name);
                if (integrationInspectors[i].GetCustomAttributes(typeof(OrderedEditorItem), true).Length > 0) {
                    var item = inspectorTypes[i].GetCustomAttributes(typeof(OrderedEditorItem), true)[0] as OrderedEditorItem;
                    name = item.Name;
                }
                m_IntegrationNames[i] = name;
            }
        }

        /// <summary>
        /// Draws the Manager.
        /// </summary>
        public override void OnGUI()
        {
            var toolbarSelection = GUILayout.Toolbar(m_DrawIntegrationInspectors ? 0 : 1, m_ToolbarStrings, EditorStyles.toolbarButton);
            m_DrawIntegrationInspectors = toolbarSelection == 0;
            GUILayout.Space(10);

            if (m_DrawIntegrationInspectors) {
                DrawIntegrationInspectors();
            } else {
                DrawAvailableIntegrations();
            }
        }

        /// <summary>
        /// Draws the inspector for all installed integrations.
        /// </summary>
        private void DrawIntegrationInspectors()
        {
            if (m_IntegrationInspectors == null || m_IntegrationInspectors.Length == 0) {
                GUILayout.Label("No integrations installed use a custom inspector.\n\nSelect the \"Available Integrations\" tab to see a list of all of the available integrations.");
                return;
            }

            m_InstalledScrollPosition = EditorGUILayout.BeginScrollView(m_InstalledScrollPosition);
            for (int i = 0; i < m_IntegrationInspectors.Length; ++i) {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label(m_IntegrationNames[i], InspectorStyles.LargeBoldLabel);
                GUILayout.Space(4);
                m_IntegrationInspectors[i].DrawInspector();
                EditorGUILayout.EndVertical();
                if (i != m_IntegrationInspectors.Length - 1) {
                    GUILayout.Space(20);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Draws all of the integrations that are currently available.
        /// </summary>
        private void DrawAvailableIntegrations()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Integrations can also be found on the");
            GUILayout.Space(-3);
            if (GUILayout.Button("integrations page.", InspectorStyles.LinkStyle, GUILayout.Width(106))) {
                Application.OpenURL(GetIntegrationLink());
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            if (m_Integrations == null && m_IntegrationsReqest == null) {
                m_IntegrationsReqest = UnityEngine.Networking.UnityWebRequest.Get("https://opsive.com/asset/UltimateCharacterController/IntegrationsList.txt");
                m_IntegrationsReqest.SendWebRequest();
            } else if (m_Integrations == null && m_IntegrationsReqest.isDone && string.IsNullOrEmpty(m_IntegrationsReqest.error)) {
                var splitIntegrations = m_IntegrationsReqest.downloadHandler.text.Split('\n');
                m_Integrations = new AssetIntegration[splitIntegrations.Length];
                var count = 0;
                for (int i = 0; i < splitIntegrations.Length; ++i) {
                    if (string.IsNullOrEmpty(splitIntegrations[i])) {
                        continue;
                    }

                    // The data must contain info on the integration name, id, icon, and integraiton url.
                    var integrationData = splitIntegrations[i].Split(',');
                    if (integrationData.Length < 4) {
                        continue;
                    }

                    m_Integrations[count] = new AssetIntegration(int.Parse(integrationData[0].Trim()), integrationData[1].Trim(), integrationData[2].Trim(), integrationData[3].Trim(), m_MainManagerWindow);
                    count++;
                }

                if (count != m_Integrations.Length) {
                    System.Array.Resize(ref m_Integrations, count);
                }
                m_IntegrationsReqest = null;
            } else if (m_IntegrationsReqest != null) {
                m_MainManagerWindow.Repaint();
            }

            // Draw the integrations once they are loaded.
            if (m_Integrations != null && m_Integrations.Length > 0) {
                var lastRect = GUILayoutUtility.GetLastRect();
                // Multiple integrations can be drawn on a single row depending on the width of the window.
                var cellsPerRow = (int)(Screen.width - m_MainManagerWindow.MenuWidth - 2) / (c_IntegrationCellWidth + c_IntegrationCellSpacing);
                m_ScrollPosition = GUI.BeginScrollView(new Rect(0, lastRect.y, Screen.width - m_MainManagerWindow.MenuWidth - 2, Screen.height - 96), m_ScrollPosition,
                                            new Rect(0, 0, Screen.width - m_MainManagerWindow.MenuWidth - 25,
                                                    ((m_Integrations.Length / cellsPerRow) + (m_Integrations.Length % 2 == 0 ? 0 : 1)) * (c_IntegrationCellHeight + c_IntegrationCellSpacing)));
                var position = new Vector2(0, 20);
                // Draw each integration.
                for (int i = 0; i < m_Integrations.Length; ++i) {
                    position.x = (i % cellsPerRow) * c_IntegrationCellWidth;
                    m_Integrations[i].DrawIntegration(position + (new Vector2(0, c_IntegrationCellHeight + c_IntegrationCellSpacing) * (i / cellsPerRow)));
                }
                GUI.EndScrollView();
            } else {
                if (m_IntegrationsReqest == null) {
                    if (Event.current.type == EventType.Repaint) {
                        m_IntegrationsReqest = UnityEngine.Networking.UnityWebRequest.Get("https://opsive.com/asset/UltimateCharacterController/IntegrationsList.txt");
                        m_IntegrationsReqest.SendWebRequest();
                    }
                } else {
                    if (m_IntegrationsReqest != null && m_IntegrationsReqest.isDone && !string.IsNullOrEmpty(m_IntegrationsReqest.error)) {
                        EditorGUILayout.LabelField("Error: Unable to retrieve integrations.");
                    } else {
                        EditorGUILayout.LabelField("Retrieveing the list of current integrations...");
                    }
                }
            }
        }

        /// <summary>
        /// Returns the integration link for the current asset.
        /// </summary>
        /// <returns>The integration link for the current asset.</returns>
        public static string GetIntegrationLink()
        {
#pragma warning disable 0162
#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER && ULTIMATE_CHARACTER_CONTROLLER_SHOOTER && ULTIMATE_CHARACTER_CONTROLLER_MELEE
            return "https://opsive.com/downloads/?pid=923";
#endif
#if FIRST_PERSON_CONTROLLER && ULTIMATE_CHARACTER_CONTROLLER_SHOOTER && ULTIMATE_CHARACTER_CONTROLLER_MELEE
            return "https://opsive.com/downloads?pid=807";
#endif
#if THIRD_PERSON_CONTROLLER && ULTIMATE_CHARACTER_CONTROLLER_SHOOTER && ULTIMATE_CHARACTER_CONTROLLER_MELEE
            return "https://opsive.com/downloads?pid=926";
#endif
#if FIRST_PERSON_CONTROLLER && ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
            return "https://opsive.com/downloads?pid=185";
#endif
#if FIRST_PERSON_CONTROLLER && ULTIMATE_CHARACTER_CONTROLLER_MELEE
            return "https://opsive.com/downloads?pid=1106";
#endif
#if THIRD_PERSON_CONTROLLER && ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
            return "https://opsive.com/downloads?pid=1107";
#endif
#if THIRD_PERSON_CONTROLLER && ULTIMATE_CHARACTER_CONTROLLER_MELEE
            return "https://opsive.com/downloads?pid=1108";
#endif
            return string.Empty;
#pragma warning restore 0162
        }
    }
}