/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Utility.Builders
{
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Camera.ViewTypes;
    using Opsive.UltimateCharacterController.StateSystem;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Adds and serializes CameraController view types.
    /// </summary>
    public static class ViewTypeBuilder
    {
        private static Dictionary<Type, AddState[]> s_AddStates = new Dictionary<Type, AddState[]>();

        /// <summary>
        /// Adds the view type with the specified type.
        /// </summary>
        /// <param name="cameraController">The camera to add the ability to.</param>
        /// <param name="viewType">The type of view type to add.</param>
        /// <returns>The added view type.</returns>
        public static ViewType AddViewType(CameraController cameraController, Type viewType)
        {
            var viewTypes = cameraController.ViewTypes;
            if (viewTypes == null) {
                viewTypes = new ViewType[1];
            } else {
                Array.Resize(ref viewTypes, viewTypes.Length + 1);
            }

            var viewTypeObj = Activator.CreateInstance(viewType) as ViewType;
            viewTypes[viewTypes.Length - 1] = viewTypeObj;
            cameraController.ViewTypes = viewTypes;

#if FIRST_PERSON_CONTROLLER
            if (viewTypeObj is FirstPersonController.Camera.ViewTypes.FirstPerson) {
                AddFirstPersonCamera(cameraController, viewTypeObj as FirstPersonController.Camera.ViewTypes.FirstPerson);
            }
#endif

#if UNITY_EDITOR
            var addStates = GetAddStates(viewType);
            if (addStates != null && addStates.Length > 0) {
                var states = viewTypeObj.States;
                var addedStates = 0;
                var stateLength = states.Length;
                Array.Resize(ref states, stateLength + addStates.Length);
                // Default must always be at the end.
                states[states.Length - 1] = states[0];
                for (int i = 0; i < addStates.Length; ++i) {
                    var presetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(addStates[i].PresetGUID);
                    if (!string.IsNullOrEmpty(presetPath)) {
                        var preset = UnityEditor.AssetDatabase.LoadAssetAtPath(presetPath, typeof(PersistablePreset)) as PersistablePreset;
                        if (preset != null) {
                            states[i] = new State(addStates[i].Name, preset, null);
                            addedStates++;
                        }
                    }
                }
                if (addedStates != addStates.Length) {
                    Array.Resize(ref states, stateLength + addedStates);
                }
                viewTypeObj.States = states;
            }
#endif

            SerializeViewTypes(cameraController);
            if (!(viewTypeObj is Transition)) {
                cameraController.SetViewType(viewType, false);
            }
            return viewTypeObj;
        }

#if FIRST_PERSON_CONTROLLER
        /// <summary>
        /// Adds a first person camera to the view type.
        /// </summary>
        /// <param name="cameraController">The camera controller that contains the view type.</param>
        /// <param name="viewType">The first person view type.</param>
        public static void AddFirstPersonCamera(CameraController cameraController, FirstPersonController.Camera.ViewTypes.FirstPerson viewType)
        {
            // A first person camera must be added to the first peron view types.
            cameraController.DeserializeViewTypes();
            var viewTypes = cameraController.ViewTypes;
            UnityEngine.Camera firstPersonCamera = null;
            FirstPersonController.Camera.ViewTypes.FirstPerson firstPersonViewType;
            for (int i = 0; i < viewTypes.Length; ++i) {
                if ((firstPersonViewType = viewTypes[i] as FirstPersonController.Camera.ViewTypes.FirstPerson) != null &&
                    firstPersonViewType.FirstPersonCamera != null) {
                    firstPersonCamera = firstPersonViewType.FirstPersonCamera;
                    break;
                }
            }

            // If the camera is null then a new first person camera should be created.
            if (firstPersonCamera == null) {
                UnityEngine.Transform firstPersonCameraTransform;
                if ((firstPersonCameraTransform = cameraController.transform.Find("First Person Camera")) != null) {
                    firstPersonCamera = firstPersonCameraTransform.GetComponent<UnityEngine.Camera>();
                }

                if (firstPersonCamera == null) {
                    var cameraGameObject = new UnityEngine.GameObject("First Person Camera");
                    cameraGameObject.transform.SetParentOrigin(cameraController.transform);
                    firstPersonCamera = cameraGameObject.AddComponent<UnityEngine.Camera>();
                    firstPersonCamera.clearFlags = UnityEngine.CameraClearFlags.Depth;
                    firstPersonCamera.fieldOfView = 60f;
                    firstPersonCamera.nearClipPlane = 0.01f;
                    firstPersonCamera.depth = 0;
                    firstPersonCamera.renderingPath = cameraController.GetComponent<UnityEngine.Camera>().renderingPath;
                }
            }

            viewType.FirstPersonCamera = firstPersonCamera;
        }
#endif

        /// <summary>
        /// Serialize all of the view types to the ViewTypeData array.
        /// </summary>
        /// <param name="cameraController">The camera controller to serialize.</param>
        public static void SerializeViewTypes(CameraController cameraController)
        {
            var viewTypes = new List<ViewType>(cameraController.ViewTypes);
            cameraController.ViewTypeData = Shared.Utility.Serialization.Serialize<ViewType>(viewTypes);
#if UNITY_EDITOR
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(cameraController);
#endif
            cameraController.ViewTypes = viewTypes.ToArray();
        }

        /// <summary>
        /// Returns the AddState of the specified view type.
        /// </summary>
        /// <param name="type">The view type.</param>
        /// <returns>The AddState of the specified ability type. Can be null.</returns>
        private static AddState[] GetAddStates(Type type)
        {
            AddState[] addStates;
            if (s_AddStates.TryGetValue(type, out addStates)) {
                return addStates;
            }

            if (type.GetCustomAttributes(typeof(AddState), true).Length > 0) {
                addStates = type.GetCustomAttributes(typeof(AddState), true) as AddState[];
            }
            s_AddStates.Add(type, addStates);
            return addStates;
        }
    }
}