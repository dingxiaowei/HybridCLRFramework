/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Managers
{
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws the inspector for an add-on that has been installed.
    /// </summary>
    public abstract class AddOnInspector
    {
        protected MainManagerWindow m_MainManagerWindow;
        public MainManagerWindow MainManagerWindow { set { m_MainManagerWindow = value; } }

        /// <summary>
        /// Draws the add-on inspector.
        /// </summary>
        public abstract void DrawInspector();
    }

    /// <summary>
    /// Draws a list of all of the available add-ons.
    /// </summary>
    [OrderedEditorItem("Add-Ons", 11)]
    public class AddOnsManager : Manager
    {
        private string[] m_ToolbarStrings = { "Installed Add-Ons", "Available Add-Ons" };
        [SerializeField] private bool m_DrawInstalledAddOns;
        [SerializeField] private bool m_Initialized;

        private AddOnInspector[] m_AddOnInspectors;
        private string[] m_AddOnNames;

        private static GUIStyle s_AddOnTitle;
        private static GUIStyle AddOnTitle
        {
            get
            {
                if (s_AddOnTitle == null) {
                    s_AddOnTitle = new GUIStyle(InspectorStyles.CenterBoldLabel);
                    s_AddOnTitle.fontSize = 14;
                    s_AddOnTitle.alignment = TextAnchor.MiddleLeft;
                }
                return s_AddOnTitle;
            }
        }

        /// <summary>
        /// Stores the information about the add-on.
        /// </summary>
        private class AvailableAddOn
        {
            private const int c_IconSize = 78;

            private int m_ID;
            private string m_Name;
            private string m_AddOnURL;
            private string m_Description;
            private bool m_Installed;
            private Texture2D m_Icon;
            private MainManagerWindow m_MainManagerWindow;

#if UNITY_2018_3_OR_NEWER
            private UnityEngine.Networking.UnityWebRequest m_IconRequest;
            private UnityEngine.Networking.DownloadHandlerTexture m_TextureDownloadHandler;
#else
            private WWW m_IconRequest;
#endif

            /// <summary>
            /// Constructor for the AvailableAddOn class.
            /// </summary>
            public AvailableAddOn(int id, string name, string iconURL, string addOnURL, string description, string type, MainManagerWindow mainManagerWindow)
            {
                m_ID = id;
                m_Name = name;
                m_AddOnURL = addOnURL;
                m_Description = description;
                // The add-on is installed if the type exists.
                m_Installed = !string.IsNullOrEmpty(type) && UltimateCharacterController.Utility.UnityEngineUtility.GetType(type) != null;
                m_MainManagerWindow = mainManagerWindow;

                // Start loading the icon as soon as the url is retrieved.
#if UNITY_2018_3_OR_NEWER
                m_TextureDownloadHandler = new UnityEngine.Networking.DownloadHandlerTexture();
                m_IconRequest = UnityEngine.Networking.UnityWebRequest.Get(iconURL);
                m_IconRequest.downloadHandler = m_TextureDownloadHandler;
                m_IconRequest.SendWebRequest();
#else
                m_IconRequest = new WWW(iconURL);
#endif
            }

            /// <summary>
            /// Draws the inspector for the available add-on.
            /// </summary>
            public void DrawAddOn()
            {
                if (m_IconRequest != null) {
                    if (m_IconRequest.isDone) {
                        if (string.IsNullOrEmpty(m_IconRequest.error)) {
#if UNITY_2018_3_OR_NEWER
                            m_Icon = m_TextureDownloadHandler.texture;
#else
                            m_Icon = m_IconRequest.texture;
#endif
                        }
                        m_IconRequest = null;
                    } else {
                        m_MainManagerWindow.Repaint();
                    }
                }

                // Draw the add-on details.
                EditorGUILayout.BeginHorizontal();
                if (m_Icon != null) {
                    GUILayout.Label(m_Icon);
                }

                EditorGUILayout.BeginVertical();
                var name = m_Name;
                if (m_Installed) {
                    name += " (INSTALLED)";
                }
                EditorGUILayout.LabelField(name, InspectorStyles.BoldLabel, GUILayout.Height(20));
                EditorGUILayout.BeginHorizontal();
                if (!string.IsNullOrEmpty(m_AddOnURL) && GUILayout.Button("Overview", GUILayout.MaxWidth(150))) {
                    Application.OpenURL(m_AddOnURL);
                }
                if (m_ID > 0 && GUILayout.Button("Asset Store", GUILayout.MaxWidth(150))) {
                    Application.OpenURL("https://opsive.com/asset/UltimateCharacterController/AssetRedirect.php?asset=" + m_ID);
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
                EditorGUILayout.LabelField(m_Description, InspectorStyles.WordWrapLabel);
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }
        }

        private Vector2 m_InstalledScrollPosition;
        private Vector2 m_AvailableScrollPosition;
#if UNITY_2018_3_OR_NEWER
        private UnityEngine.Networking.UnityWebRequest m_AddOnsReqest;
#else
        private WWW m_AddOnsReqest;
#endif
        private AvailableAddOn[] m_AvailableAddOns;

        /// <summary>
        /// Initialize the manager after deserialization.
        /// </summary>
        public override void Initialize(MainManagerWindow mainManagerWindow)
        {
            base.Initialize(mainManagerWindow);

            BuildInstalledAddOns();

            if (!m_Initialized) {
                m_DrawInstalledAddOns = m_AvailableAddOns != null && m_AvailableAddOns.Length > 0;
                m_Initialized = true;
            }
        }

        /// <summary>
        /// Finds and create an instance of the inspectors for all of the installed add-ons.
        /// </summary>
        private void BuildInstalledAddOns()
        {
            var addOnInspectors = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var addOnIndexes = new List<int>();
            for (int i = 0; i < assemblies.Length; ++i) {
                var assemblyTypes = assemblies[i].GetTypes();
                for (int j = 0; j < assemblyTypes.Length; ++j) {
                    // Must implement AddOnInspector.
                    if (!typeof(AddOnInspector).IsAssignableFrom(assemblyTypes[j])) {
                        continue;
                    }

                    // Ignore abstract classes.
                    if (assemblyTypes[j].IsAbstract) {
                        continue;
                    }

                    // A valid inspector class.
                    addOnInspectors.Add(assemblyTypes[j]);
                    var index = addOnIndexes.Count;
                    if (assemblyTypes[j].GetCustomAttributes(typeof(OrderedEditorItem), true).Length > 0) {
                        var item = assemblyTypes[j].GetCustomAttributes(typeof(OrderedEditorItem), true)[0] as OrderedEditorItem;
                        index = item.Index;
                    }
                    addOnIndexes.Add(index);
                }
            }

            // Do not reinitialize the inspectors if they are already initialized and there aren't any changes.
            if (m_AddOnInspectors != null && m_AddOnInspectors.Length == addOnInspectors.Count) {
                return;
            }

            // All of the manager types have been found. Sort by the index.
            var inspectorTypes = addOnInspectors.ToArray();
            Array.Sort(addOnIndexes.ToArray(), inspectorTypes);

            m_AddOnInspectors = new AddOnInspector[addOnInspectors.Count];
            m_AddOnNames = new string[addOnInspectors.Count];

            // The inspector types have been found and sorted. Add them to the list.
            for (int i = 0; i < inspectorTypes.Length; ++i) {
                m_AddOnInspectors[i] = Activator.CreateInstance(inspectorTypes[i]) as AddOnInspector;
                m_AddOnInspectors[i].MainManagerWindow = m_MainManagerWindow;

                var name = InspectorUtility.SplitCamelCase(inspectorTypes[i].Name);
                if (addOnInspectors[i].GetCustomAttributes(typeof(OrderedEditorItem), true).Length > 0) {
                    var item = inspectorTypes[i].GetCustomAttributes(typeof(OrderedEditorItem), true)[0] as OrderedEditorItem;
                    name = item.Name;
                }
                m_AddOnNames[i] = name;
            }
        }

        /// <summary>
        /// Draws the Manager.
        /// </summary>
        public override void OnGUI()
        {
            var toolbarSelection = GUILayout.Toolbar(m_DrawInstalledAddOns ? 0 : 1, m_ToolbarStrings, EditorStyles.toolbarButton);
            m_DrawInstalledAddOns = toolbarSelection == 0;
            GUILayout.Space(10);

            if (m_DrawInstalledAddOns) {
                DrawInstalledAddOns();
            } else {
                DrawAvailableAddOns();
            }
        }

        /// <summary>
        /// Draws the inspector for all installed add-ons.
        /// </summary>
        private void DrawInstalledAddOns()
        {
            if (m_AddOnInspectors == null || m_AddOnInspectors.Length == 0) {
                GUILayout.Label("No add-ons are currently installed.\n\nSelect the \"Available Add-Ons\" tab to see a list of all of the available add-ons.");
                return;
            }

            m_InstalledScrollPosition = EditorGUILayout.BeginScrollView(m_InstalledScrollPosition);
            for (int i = 0; i < m_AddOnInspectors.Length; ++i) {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label(m_AddOnNames[i], InspectorStyles.LargeBoldLabel);
                GUILayout.Space(4);
                m_AddOnInspectors[i].DrawInspector();
                EditorGUILayout.EndVertical();
                if (i != m_AddOnInspectors.Length - 1) {
                    GUILayout.Space(20);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Draws all of the add-ons that are currently available.
        /// </summary>
        private void DrawAvailableAddOns()
        {
            if (m_AvailableAddOns == null && m_AddOnsReqest == null) {
#if UNITY_2018_3_OR_NEWER
                m_AddOnsReqest = UnityEngine.Networking.UnityWebRequest.Get("https://opsive.com/asset/UltimateCharacterController/AddOnsList.txt");
                m_AddOnsReqest.SendWebRequest();
#else
                m_AddOnsReqest = new WWW("https://opsive.com/asset/UltimateCharacterController/AddOnsList.txt");
#endif
            } else if (m_AvailableAddOns == null && m_AddOnsReqest.isDone && string.IsNullOrEmpty(m_AddOnsReqest.error) && Event.current.type == EventType.Layout) {
#if UNITY_2018_3_OR_NEWER
                var splitAddOns = m_AddOnsReqest.downloadHandler.text.Split('\n');
#else
                var splitAddOns = m_AddOnsReqest.text.Split('\n');
#endif
                m_AvailableAddOns = new AvailableAddOn[splitAddOns.Length];
                var count = 0;
                for (int i = 0; i < splitAddOns.Length; ++i) {
                    if (string.IsNullOrEmpty(splitAddOns[i])) {
                        continue;
                    }

                    // The data must contain info on the add-on name, id, icon, add-on url, description, and type.
                    var addOnData = splitAddOns[i].Split(',');
                    if (addOnData.Length < 6) {
                        continue;
                    }

                    m_AvailableAddOns[count] = new AvailableAddOn(int.Parse(addOnData[0].Trim()), addOnData[1].Trim(), addOnData[2].Trim(), addOnData[3].Trim(), addOnData[4].Trim(), addOnData[5].Trim(), m_MainManagerWindow);
                    count++;
                }

                if (count != m_AvailableAddOns.Length) {
                    Array.Resize(ref m_AvailableAddOns, count);
                }
                m_AddOnsReqest = null;
            } else if (m_AddOnsReqest != null) {
                m_MainManagerWindow.Repaint();
            }

            // Draw the add-ons once they are loaded.
            if (m_AvailableAddOns != null && m_AvailableAddOns.Length > 0) {
                m_AvailableScrollPosition = EditorGUILayout.BeginScrollView(m_AvailableScrollPosition);
                // Draw each add-on.
                for (int i = 0; i < m_AvailableAddOns.Length; ++i) {
                    m_AvailableAddOns[i].DrawAddOn();
                    if (i != m_AvailableAddOns.Length - 1) {
                        GUILayout.Space(20);
                    }
                }
                EditorGUILayout.EndScrollView();
            } else {
                if (m_AddOnsReqest != null && m_AddOnsReqest.isDone && !string.IsNullOrEmpty(m_AddOnsReqest.error)) {
                    EditorGUILayout.LabelField("Error: Unable to retrieve add-ons.");
                } else {
                    EditorGUILayout.LabelField("Retrieveing the list of current add-ons...");
                }
            }
        }
    }
}