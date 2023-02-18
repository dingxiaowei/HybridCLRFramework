/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Managers
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.StateSystem;
    using Opsive.UltimateCharacterController.Utility;
    using Opsive.UltimateCharacterController.Utility.Builders;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// The SetupManager shows any project or scene related setup options.
    /// </summary>
    [OrderedEditorItem("Setup", 1)]
    public class SetupManager : Manager
    {
        private const string c_MonitorsPrefabGUID = "b5bf2e4077598914b83fc5e4ca20f2f4";
        private const string c_VirtualControlsPrefabGUID = "33d3d57ba5fc7484c8d09150e45066a4";

        /// <summary>
        /// Specifies the perspective that the ViewType can change into.
        /// </summary>
        private enum Perspective
        {
            First,  // The ViewType can only be in first person perspective.
            Third,  // The ViewType can only be in third person perspective.
            Both,   // The ViewType can be in first or third person perspective.
            None    // Default value.
        }

        private string[] m_ToolbarStrings = { "Scene", "Project" };
        [SerializeField] private bool m_DrawSceneSetup = true;

        [SerializeField] private bool m_CanCreateCamera = true;
        [SerializeField] private Perspective m_Perspective = Perspective.None;
        [SerializeField] private string m_FirstPersonViewType;
        [SerializeField] private string m_ThirdPersonViewType;
        [SerializeField] private bool m_StartFirstPersonPerspective;
        [SerializeField] private StateConfiguration m_StateConfiguration;
        [SerializeField] private int m_ProfileIndex;
        [SerializeField] private string m_ProfileName;

        private List<Type> m_FirstPersonViewTypes = new List<Type>();
        private string[] m_FirstPersonViewTypeStrings;
        private List<Type> m_ThirdPersonViewTypes = new List<Type>();
        private string[] m_ThirdPersonViewTypeStrings;
        private string[] m_PerspectiveNames = { "First", "Third", "Both" };

        /// <summary>
        /// Initialize the manager after deserialization.
        /// </summary>
        public override void Initialize(MainManagerWindow mainManagerWindow)
        {
            base.Initialize(mainManagerWindow);

            // Set the default perspective based on what asset is installed.
            if (m_Perspective == Perspective.None) {
#if FIRST_PERSON_CONTROLLER
                m_Perspective = Perspective.First;
#elif THIRD_PERSON_CONTROLLER
                m_Perspective = Perspective.Third;
#endif
            }

            // Get a list of the available view types.
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; ++i) {
                var assemblyTypes = assemblies[i].GetTypes();
                for (int j = 0; j < assemblyTypes.Length; ++j) {
                    // Must derive from ViewType.
                    if (!typeof(UltimateCharacterController.Camera.ViewTypes.ViewType).IsAssignableFrom(assemblyTypes[j])) {
                        continue;
                    }

                    // Ignore abstract classes.
                    if (assemblyTypes[j].IsAbstract) {
                        continue;
                    }

                    if (assemblyTypes[j].FullName.Contains("FirstPersonController")) {
                        m_FirstPersonViewTypes.Add(assemblyTypes[j]);
                    } else if (assemblyTypes[j].FullName.Contains("ThirdPersonController")) {
                        m_ThirdPersonViewTypes.Add(assemblyTypes[j]);
                    }
                }
            }

            // Create an array of display names for the popup.
            if (m_FirstPersonViewTypes.Count > 0) {
                m_FirstPersonViewTypeStrings = new string[m_FirstPersonViewTypes.Count];
                for (int i = 0; i < m_FirstPersonViewTypes.Count; ++i) {
                    m_FirstPersonViewTypeStrings[i] = InspectorUtility.DisplayTypeName(m_FirstPersonViewTypes[i], true);
                }
            }
            if (m_ThirdPersonViewTypes.Count > 0) {
                m_ThirdPersonViewTypeStrings = new string[m_ThirdPersonViewTypes.Count];
                for (int i = 0; i < m_ThirdPersonViewTypes.Count; ++i) {
                    m_ThirdPersonViewTypeStrings[i] = InspectorUtility.DisplayTypeName(m_ThirdPersonViewTypes[i], true);
                }
            }

            // Find the state configuration.
            var stateConfiguration = ManagerUtility.FindStateConfiguration(m_MainManagerWindow);
            if (stateConfiguration != null) {
                if (m_StateConfiguration == null) {
                    m_StateConfiguration = stateConfiguration;
                }
            }
        }

        /// <summary>
        /// Draws the Manager.
        /// </summary>
        public override void OnGUI()
        {
            var toolbarSelection = GUILayout.Toolbar(m_DrawSceneSetup ? 0 : 1, m_ToolbarStrings, EditorStyles.toolbarButton);
            m_DrawSceneSetup = toolbarSelection == 0;
            GUILayout.Space(10);

            if (m_DrawSceneSetup) {
                DrawSceneSetup();
            } else {
                DrawProjectSetup();
            }
        }

        /// <summary>
        /// Draws the controls for setting up the scene.
        /// </summary>
        private void DrawSceneSetup()
        {
            ManagerUtility.DrawControlBox("Manager Setup", null, "Adds the scene-level manager components to the scene.", true, "Add Managers", AddManagers, string.Empty);
            ManagerUtility.DrawControlBox("Camera Setup", DrawCameraViewTypes, "Sets up the camera within the scene to use the Ultimate Character Controller Camera Controller component.",
                                                m_CanCreateCamera, "Setup Camera", SetupCamera, string.Empty);
            ManagerUtility.DrawControlBox("UI Setup", null, "Adds the UI monitors to the scene.", true, "Add UI", AddUI, string.Empty);
            ManagerUtility.DrawControlBox("Virtual Controls Setup", null, "Adds the virtual controls to the scene.", true, "Add Virtual Controls", AddVirtualControls, string.Empty);
        }

        /// <summary>
        /// Draws the popup for the camera view types.
        /// </summary>
        private void DrawCameraViewTypes()
        {
            // Draw the perspective.
            var selectedPerspective = (Perspective)EditorGUILayout.Popup("Perspective", (int)m_Perspective, m_PerspectiveNames);
            var isSupported = true;
            // Determine if the selected perspective is supported.
#if !FIRST_PERSON_CONTROLLER
            if (selectedPerspective == Perspective.First || selectedPerspective == Perspective.Both) {
                EditorGUILayout.HelpBox("Unable to select the First Person Controller perspective. If you'd like to use a first person perspective ensure the " +
                                        "First Person Controller is imported.", MessageType.Error);
                isSupported = false;
            }
#endif
#if !THIRD_PERSON_CONTROLLER
            if (selectedPerspective == Perspective.Third || selectedPerspective == Perspective.Both) {
                EditorGUILayout.HelpBox("Unable to select the Third Person Controller perspective. If you'd like to use a third person perspective ensure the " +
                                        "Third Person Controller is imported.", MessageType.Error);
                isSupported = false;
            }
#endif
            if (selectedPerspective != m_Perspective) {
                m_Perspective = selectedPerspective;
            }

            m_CanCreateCamera = isSupported;
            if (!isSupported) {
                return;
            }

            // Show the available first person ViewTypes.
            if (m_Perspective == Perspective.First || m_Perspective == Perspective.Both) {
                var selectedViewType = -1;
                for (int i = 0; i < m_FirstPersonViewTypes.Count; ++i) {
                    if (m_FirstPersonViewTypes[i].FullName == m_FirstPersonViewType) {
                        selectedViewType = i;
                        break;
                    }
                }
                var viewType = selectedViewType == -1 ? 0 : selectedViewType;
                selectedViewType = EditorGUILayout.Popup("First Person View Type", viewType, m_FirstPersonViewTypeStrings);
                if (viewType != selectedViewType || string.IsNullOrEmpty(m_FirstPersonViewType)) {
                    m_FirstPersonViewType = m_FirstPersonViewTypes[selectedViewType].FullName;
                }
                if (m_Perspective != Perspective.Both) {
                    m_ThirdPersonViewType = string.Empty;
                }
            }
            // Show the available third person ViewTypes.
            if (m_Perspective == Perspective.Third || m_Perspective == Perspective.Both) {
                var selectedViewType = -1;
                for (int i = 0; i < m_ThirdPersonViewTypes.Count; ++i) {
                    if (m_ThirdPersonViewTypes[i].FullName == m_ThirdPersonViewType) {
                        selectedViewType = i;
                        break;
                    }
                }
                var viewType = selectedViewType == -1 ? 0 : selectedViewType;
                selectedViewType = EditorGUILayout.Popup("Third Person View Type", viewType, m_ThirdPersonViewTypeStrings);
                if (viewType != selectedViewType || string.IsNullOrEmpty(m_ThirdPersonViewType)) {
                    m_ThirdPersonViewType = m_ThirdPersonViewTypes[selectedViewType].FullName;
                }
                if (m_Perspective != Perspective.Both) {
                    m_FirstPersonViewType = string.Empty;
                }
            }
            if (m_Perspective == Perspective.Both) {
                m_StartFirstPersonPerspective = EditorGUILayout.Popup("Start Perspective", m_StartFirstPersonPerspective ? 0 : 1, new string[] { "First Person", "Third Person" }) == 0;
            } else {
                m_StartFirstPersonPerspective = (m_Perspective == Perspective.First);
            }
            // Show the possible base configurations.
            var updatedStateConfiguration = EditorGUILayout.ObjectField("State Configuration", m_StateConfiguration, typeof(StateConfiguration), false) as StateConfiguration;
            if (updatedStateConfiguration != m_StateConfiguration) {
                if (updatedStateConfiguration != null) {
                    EditorPrefs.SetString(ManagerUtility.LastStateConfigurationGUIDString, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(updatedStateConfiguration)));
                } else {
                    EditorPrefs.SetString(ManagerUtility.LastStateConfigurationGUIDString, string.Empty);
                }
                m_StateConfiguration = updatedStateConfiguration;
            }
            EditorGUI.indentLevel++;
            if (m_StateConfiguration != null) {
                var profiles = m_StateConfiguration.GetProfilesForGameObject(null, StateConfiguration.Profile.ProfileType.Camera);
                // The character can be added without any profiles.
                profiles.Insert(0, "(None)");
                m_ProfileIndex = EditorGUILayout.Popup("Profile", m_ProfileIndex, profiles.ToArray());
                m_ProfileName = profiles[m_ProfileIndex];
            }
            EditorGUI.indentLevel--;
            GUILayout.Space(5);
        }

        /// <summary>
        /// Sets up the camera if it hasn't already been setup.
        /// </summary>
        private void SetupCamera()
        {
            // Setup the camera.
            GameObject cameraGameObject;
            var addedCameraController = false;
            var camera = UnityEngine.Camera.main;
            if (camera == null) {
                // If the main camera can't be found then use the first available camera.
                var cameras = UnityEngine.Camera.allCameras;
                if (cameras != null && cameras.Length > 0) {
                    // Prefer cameras that are at the root level.
                    for (int i = 0; i < cameras.Length; ++i) {
                        if (cameras[i].transform.parent == null) {
                            camera = cameras[i];
                            break;
                        }
                    }
                    // No cameras are at the root level. Set the first available camera.
                    if (camera == null) {
                        camera = cameras[0];
                    }
                }

                // A new camera should be created if there isn't a valid camera.
                if (camera == null) {
                    cameraGameObject = new GameObject("Camera");
                    cameraGameObject.tag = "MainCamera";
                    camera = cameraGameObject.AddComponent<UnityEngine.Camera>();
                    cameraGameObject.AddComponent<AudioListener>();
                }
            }

            // The near clip plane should adjusted for viewing close objects.
            camera.nearClipPlane = 0.01f;
            
            // Add the CameraController if it isn't already added.
            cameraGameObject = camera.gameObject;
            if (cameraGameObject.GetComponent<CameraController>() == null) {
                var cameraController = cameraGameObject.AddComponent<CameraController>();
                if (m_Perspective == Perspective.Both) {
                    ViewTypeBuilder.AddViewType(cameraController, typeof(UltimateCharacterController.Camera.ViewTypes.Transition));
                }
                if (m_StartFirstPersonPerspective) {
                    if (!string.IsNullOrEmpty(m_ThirdPersonViewType)) {
                        ViewTypeBuilder.AddViewType(cameraController, UnityEngineUtility.GetType(m_ThirdPersonViewType));
                    }
                    if (!string.IsNullOrEmpty(m_FirstPersonViewType)) {
                        ViewTypeBuilder.AddViewType(cameraController, UnityEngineUtility.GetType(m_FirstPersonViewType));
                    }
                } else {
                    if (!string.IsNullOrEmpty(m_FirstPersonViewType)) {
                        ViewTypeBuilder.AddViewType(cameraController, UnityEngineUtility.GetType(m_FirstPersonViewType));
                    }
                    if (!string.IsNullOrEmpty(m_ThirdPersonViewType)) {
                        ViewTypeBuilder.AddViewType(cameraController, UnityEngineUtility.GetType(m_ThirdPersonViewType));
                    }

                }

                // Detect if a character exists in the scene. Automatically add the character if it does.
                var characters = GameObject.FindObjectsOfType<UltimateCharacterController.Character.CharacterLocomotion>();
                if (characters != null && characters.Length == 1) {
                    cameraController.InitCharacterOnAwake = true;
                    cameraController.Character = characters[0].gameObject;
                }

                // Setup the components which help the Camera Controller.
                Shared.Editor.Utility.InspectorUtility.AddComponent<CameraControllerHandler>(cameraGameObject);
#if THIRD_PERSON_CONTROLLER
                if (m_Perspective != Perspective.First) {
                    Shared.Editor.Utility.InspectorUtility.AddComponent<ThirdPersonController.Camera.ObjectFader>(cameraGameObject);
                }
#endif
                addedCameraController = true;

                if (m_StateConfiguration != null) {
                    if (m_ProfileIndex > 0) {
                        m_StateConfiguration.AddStatesToGameObject(m_ProfileName, cameraGameObject);
                        InspectorUtility.SetDirty(cameraGameObject);
                    }
                }
            }

            if (addedCameraController) {
                Debug.Log("The Camera Controller has been added.");
            } else {
                Debug.LogWarning("Warning: No action was performed, the Camera Controller component has already been added.");
            }
        }

        /// <summary>
        /// Adds the singleton manager components.
        /// </summary>
        private void AddManagers()
        {
            // Create the "Game" components if it doesn't already exists.
            Scheduler scheduler;
            GameObject gameGameObject;
            if ((scheduler = GameObject.FindObjectOfType<Scheduler>()) == null) {
                gameGameObject = new GameObject("Game");
            } else {
                gameGameObject = scheduler.gameObject;
            }

            // Add the Singletons.
            Shared.Editor.Utility.InspectorUtility.AddComponent<SurfaceSystem.SurfaceManager>(gameGameObject);
            Shared.Editor.Utility.InspectorUtility.AddComponent<SurfaceSystem.DecalManager>(gameGameObject);
            Shared.Editor.Utility.InspectorUtility.AddComponent<KinematicObjectManager>(gameGameObject);
            Shared.Editor.Utility.InspectorUtility.AddComponent<ObjectPool>(gameGameObject);
            Shared.Editor.Utility.InspectorUtility.AddComponent<Scheduler>(gameGameObject);
            Shared.Editor.Utility.InspectorUtility.AddComponent<Audio.AudioManager>(gameGameObject);
            Shared.Editor.Utility.InspectorUtility.AddComponent<SpawnPointManager>(gameGameObject);
            Shared.Editor.Utility.InspectorUtility.AddComponent<StateManager>(gameGameObject);
            Shared.Editor.Utility.InspectorUtility.AddComponent<LayerManager>(gameGameObject);
            Debug.Log("The managers have been added.");
        }

        /// <summary>
        /// Adds the UI to the scene.
        /// </summary>
        private void AddUI()
        {
            var canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas == null) {
                EditorApplication.ExecuteMenuItem("GameObject/UI/Canvas");
                canvas = GameObject.FindObjectOfType<Canvas>();
            }

            // Look up based on guid.
            GameObject uiPrefab = null;
            var monitorsPath = AssetDatabase.GUIDToAssetPath(c_MonitorsPrefabGUID);
            if (!string.IsNullOrEmpty(monitorsPath)) {
                uiPrefab = AssetDatabase.LoadAssetAtPath(monitorsPath, typeof(GameObject)) as GameObject;
            }

            // If the guid wasn't found try the path.
            if (uiPrefab == null) {
                var baseDirectory = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(m_MainManagerWindow))).Replace("\\", "/").Replace("Editor/Managers", "");
                uiPrefab = AssetDatabase.LoadAssetAtPath(baseDirectory + "Demo/Prefabs/UI/Monitors.prefab", typeof(GameObject)) as GameObject;
            }

            if (uiPrefab == null) {
                Debug.LogError("Error: Unable to find the UI Monitors prefab.");
                return;
            }

            // Instantiate the Monitors prefab.
            var uiGameObject = PrefabUtility.InstantiatePrefab(uiPrefab) as GameObject;
            uiGameObject.name = "Monitors";
            uiGameObject.GetComponent<RectTransform>().SetParent(canvas.transform, false);
        }

        /// <summary>
        /// Adds the UI to the scene.
        /// </summary>
        private void AddVirtualControls()
        {
            var canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas == null) {
                EditorApplication.ExecuteMenuItem("GameObject/UI/Canvas");
                canvas = GameObject.FindObjectOfType<Canvas>();
            }

            // Look up based on guid.
            GameObject virtualControlsPrefab = null;
            var virtualControlsPath = AssetDatabase.GUIDToAssetPath(c_VirtualControlsPrefabGUID);
            if (!string.IsNullOrEmpty(virtualControlsPath)) {
                virtualControlsPrefab = AssetDatabase.LoadAssetAtPath(virtualControlsPath, typeof(GameObject)) as GameObject;
            }

            // If the guid wasn't found try the path.
            if (virtualControlsPrefab == null) {
                var baseDirectory = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(m_MainManagerWindow))).Replace("\\", "/").Replace("Editor/Managers", "");
                virtualControlsPrefab = AssetDatabase.LoadAssetAtPath(baseDirectory + "Demo/Prefabs/UI/VirtualControls.prefab", typeof(GameObject)) as GameObject;
            }

            if (virtualControlsPrefab == null) {
                Debug.LogError("Error: Unable to find the UI Virtual Controls prefab.");
                return;
            }

            // Instantiate the Virtual Controls prefab.
            var virtualControls = PrefabUtility.InstantiatePrefab(virtualControlsPrefab) as GameObject;
            virtualControls.name = "VirtualControls";
            virtualControls.GetComponent<RectTransform>().SetParent(canvas.transform, false);
        }

        /// <summary>
        /// Draws the controls for button and input setup.
        /// </summary>
        private void DrawProjectSetup()
        {
            ManagerUtility.DrawControlBox("Button Mappings", null, "This option will add the default button mappings to the Unity Input Manager. If you are using a custom button mapping or " +
                            "an input integration then you do not neeed to update the Unity button mappings.", true, "Update Buttons", 
                            Utility.UnityInputBuilder.UpdateInputManager, "The button mappings were successfully updated.");
            GUILayout.Space(10);
            ManagerUtility.DrawControlBox("Layers", null, "This option will update the project layers to the default character controller layers. The layers do not need to be updated " +
                            "if you have already setup a custom set of layers.", true, "Update Layers", UpdateLayers, "The layers were successfully updated.");
        }

        /// <summary>
        /// Updates all of the layers to the Ultimate Character Controller defaults.
        /// </summary>
        public static void UpdateLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layersProperty = tagManager.FindProperty("layers");

            // Add the layers.
            AddLayer(layersProperty, LayerManager.Enemy, "Enemy");
            AddLayer(layersProperty, LayerManager.MovingPlatform, "MovingPlatform");
            AddLayer(layersProperty, LayerManager.VisualEffect, "VisualEffect");
            AddLayer(layersProperty, LayerManager.Overlay, "Overlay");
            AddLayer(layersProperty, LayerManager.SubCharacter, "SubCharacter");
            AddLayer(layersProperty, LayerManager.Character, "Character");

            tagManager.ApplyModifiedProperties();
        }

        /// <summary>
        /// Sets the layer index to the specified name if the string value is empty.
        /// </summary>
        private static void AddLayer(SerializedProperty layersProperty, int index, string name)
        {
            var layerElement = layersProperty.GetArrayElementAtIndex(index);
            if (string.IsNullOrEmpty(layerElement.stringValue)) {
                layerElement.stringValue = name;
            }
        }
    }
}