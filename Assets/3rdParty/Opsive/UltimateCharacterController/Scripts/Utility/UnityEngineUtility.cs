/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Utility
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Camera;
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Contains a set of utility functions useful for interacting with the Unity Engine.
    /// </summary>
    public class UnityEngineUtility
    {
        private static Dictionary<string, Type> s_TypeLookup = new Dictionary<string, Type>();
        private static List<Assembly> s_LoadedAssemblies = null;
        private static Dictionary<GameObject, UnityEngine.Camera> s_GameObjectCameraMap = new Dictionary<GameObject, UnityEngine.Camera>();
        public static HashSet<object> s_ObjectUpdated = new HashSet<object>();
        public static ScheduledEventBase s_ObjectClearEvent;
        private static Dictionary<FieldInfo, Dictionary<Type, bool>> s_FieldAttributeMap;
        private static Dictionary<PropertyInfo, Dictionary<Type, bool>> s_PropertyAttributeMap;

        /// <summary>
        /// Searches through all of the loaded assembies for the specified type.
        /// </summary>
        /// <param name="name">The string value of the type.</param>
        /// <returns>The found Type. Can be null.</returns>
        public static Type GetType(string name)
        {
            if (string.IsNullOrEmpty(name)) {
                return null;
            }

            Type type;
            // Cache the results for quick repeated lookup.
            if (s_TypeLookup.TryGetValue(name, out type)) {
                return type;
            }

            type = Type.GetType(name);
            // Look in the loaded assemblies.
            if (type == null) {
                if (s_LoadedAssemblies == null || s_LoadedAssemblies.Count == 0) {
#if NETFX_CORE && !UNITY_EDITOR
                    s_LoadedAssemblies = GetStorageFileAssemblies(typeName).Result;
#else
                    s_LoadedAssemblies = new List<Assembly>();
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    for (int i = 0; i < assemblies.Length; ++i) {
                        s_LoadedAssemblies.Add(assemblies[i]);
                    }
#endif
                }
                // Continue until the type is found.
                for (int i = 0; i < s_LoadedAssemblies.Count; ++i) {
                    type = s_LoadedAssemblies[i].GetType(name);
                    if (type != null) {
                        break;
                    }
                }
            }
            if (type == null) {
                // TODO: QuickStart and QuickStop were renamed in version 2.1.3.
                if (name == "Opsive.UltimateCharacterController.Character.Abilities.StartMovement") {
                    return GetType("Opsive.UltimateCharacterController.Character.Abilities.QuickStart");
                }
                if (name == "Opsive.UltimateCharacterController.Character.Abilities.StopMovement") {
                    return GetType("Opsive.UltimateCharacterController.Character.Abilities.QuickStop");
                }
                // TODO: Add-on directory was renamed in 2.1.5.
                if (name.Contains("Opsive.UltimateCharacterController.Addons.")) {
                    return GetType(name.Replace("Opsive.UltimateCharacterController.Addons.", "Opsive.UltimateCharacterController.AddOns."));
                }
            }
            if (type != null) {
                s_TypeLookup.Add(name, type);
            }
            return type;
        }

        /// <summary>
        /// Returns a friendly name for the specified type.
        /// </summary>
        /// <param name="type">The type to retieve the name of.</param>
        /// <returns>A friendly name for the specified type.</returns>
        public static string GetFriendlyName(Type type)
        {
            return GetFriendlyName(type.FullName, type.Name);
        }

        /// <summary>
        /// Returns a friendly name for the specified type.
        /// </summary>
        /// <param name="fullName">The full name of the type.</param>
        /// <param name="name">The name of the type.</param>
        /// <returns>A friendly name for the specified type.</returns>
        public static string GetFriendlyName(string fullName, string name)
        {
            if (fullName.Contains("FirstPersonController")) {
                return "First Person " + name;
            } else if (fullName.Contains("ThirdPersonController")) {
                return "Third Person " + name;
            }
            return name;
        }

        /// <summary>
        /// Returns true if the field has the specified attribute.
        /// </summary>
        /// <param name="field">The field to determine if it has the attribute.</param>
        /// <param name="attribute">The attribute to compare against.</param>
        /// <returns>Tue if the field has the specified attribute.</returns>
        public static bool HasAttribute(FieldInfo field, Type attribute)
        {
            if (field == null) {
                return false;
            }

            // Cache the results for quick repeated lookup.
            if (s_FieldAttributeMap == null) {
                s_FieldAttributeMap = new Dictionary<FieldInfo, Dictionary<Type, bool>>();
            }

            Dictionary<Type, bool> typeLookup;
            if (!s_FieldAttributeMap.TryGetValue(field, out typeLookup)) {
                typeLookup = new Dictionary<Type, bool>();
                s_FieldAttributeMap.Add(field, typeLookup);
            }

            // The static field attribute map contains a dictionary of attributes that the specified type has. Add to that dictionary if the current
            // attribute type hasn't been retrieved before.
            var hasAttribute = false;
            if (!typeLookup.TryGetValue(attribute, out hasAttribute)) {
                hasAttribute = field.GetCustomAttributes(attribute, false).Length > 0;
                typeLookup.Add(attribute, hasAttribute);
            }

            return hasAttribute;
        }

        /// <summary>
        /// Returns true if the property has the specified attribute.
        /// </summary>
        /// <param name="property">The property to determine if it has the attribute.</param>
        /// <param name="attribute">The attribute to compare against.</param>
        /// <returns>Tue if the property has the specified attribute.</returns>
        public static bool HasAttribute(PropertyInfo property, Type attribute)
        {
            if (property == null) {
                return false;
            }

            // Cache the results for quick repeated lookup.
            if (s_PropertyAttributeMap == null) {
                s_PropertyAttributeMap = new Dictionary<PropertyInfo, Dictionary<Type, bool>>();
            }

            Dictionary<Type, bool> typeLookup;
            if (!s_PropertyAttributeMap.TryGetValue(property, out typeLookup)) {
                typeLookup = new Dictionary<Type, bool>();
                s_PropertyAttributeMap.Add(property, typeLookup);
            }

            // The static property attribute map contains a dictionary of attributes that the specified type has. Add to that dictionary if the current
            // attribute type hasn't been retrieved before.
            var hasAttribute = false;
            if (!typeLookup.TryGetValue(attribute, out hasAttribute)) {
                hasAttribute = property.GetCustomAttributes(attribute, false).Length > 0;
                typeLookup.Add(attribute, hasAttribute);
            }

            return hasAttribute;
        }

        /// <summary>
        /// Returns the camera with the MainCamera tag or the camera with the CameraController attached.
        /// </summary>
        /// <param name="character">The character that the camera is attached to.</param>
        /// <returns>The found camera (if any).</returns>
        public static UnityEngine.Camera FindCamera(GameObject character)
        {
            UnityEngine.Camera camera;
            if (character != null) {
                if (s_GameObjectCameraMap.TryGetValue(character, out camera)) {
                    // The reference may be null if the scene changed.
                    if (camera != null) {
                        return camera;
                    }
                    // The reference is null - search for the camera again.
                    s_GameObjectCameraMap.Remove(character);
                }
            }
            // First try to find the camera with the character attached. If no camera has the character attached the return the first camera with the CameraController.
            camera = SearchForCamera(character);
            if (camera == null) {
                camera = SearchForCamera(null);
                if (camera != null) {
                    // The camera controller's character field must be null or equal to the existing character.
                    var cameraController = camera.GetComponent<CameraController>();
                    if (cameraController.Character != null && cameraController.Character != character) {
                        camera = null;
                    }
                }
            }
            if (camera != null && character != null) {
                s_GameObjectCameraMap.Add(character, camera);
            }
            return camera;
        }

        /// <summary>
        /// Loops through the cameras searching for a camera with the character assigned.
        /// </summary>
        /// <param name="character">The character to search for. Can be null.</param>
        /// <returns>The camera with the character assigned.</returns>
        private static UnityEngine.Camera SearchForCamera(GameObject character)
        {
            CameraController cameraController;
            UnityEngine.Camera mainCamera;
            if ((mainCamera = UnityEngine.Camera.main) != null && (cameraController = mainCamera.GetComponent<CameraController>()) != null && (character == null || cameraController.Character == character)) {
                return mainCamera;
            }
            var cameraControllers = UnityEngine.Object.FindObjectsOfType<CameraController>();
            for (int i = 0; i < cameraControllers.Length; ++i) {
                if (character == null || cameraControllers[i].Character == character) {
                    return cameraControllers[i].GetComponent<UnityEngine.Camera>();
                }
            }
            return null;
        }

        /// <summary>
        /// Returns true if the specified object has been updated.
        /// </summary>
        /// <param name="obj">The object to check if it has been updated.</param>
        /// <returns>True if the specified object has been updated.</returns>
        public static bool HasUpdatedObject(object obj)
        {
            return s_ObjectUpdated.Contains(obj);
        }

        /// <summary>
        /// Adds the specified object to the set.
        /// </summary>
        /// <param name="obj">The object that has been updated.</param>
        public static void AddUpdatedObject(object obj)
        {
            AddUpdatedObject(obj, false);
        }

        /// <summary>
        /// Adds the specified object to the set.
        /// </summary>
        /// <param name="obj">The object that has been updated.</param>
        /// <param name="autoClear">Should the object updated map be automatically cleared on the next tick?</param>
        public static void AddUpdatedObject(object obj, bool autoClear)
        {
            s_ObjectUpdated.Add(obj);

            if (autoClear && s_ObjectClearEvent == null) {
                s_ObjectClearEvent = Scheduler.Schedule(0.0001f, ClearUpdatedObjectsEvent);
            }
        }

        /// <summary>
        /// Removes all of the objects from the set.
        /// </summary>
        public static void ClearUpdatedObjects()
        {
            s_ObjectUpdated.Clear();
        }

        /// <summary>
        /// Removes all of the objects from the set and sets the event to null.
        /// </summary>
        private static void ClearUpdatedObjectsEvent()
        {
            ClearUpdatedObjects();
            s_ObjectClearEvent = null;
        }

        /// <summary>
        /// Change the size of the RectTransform according to the size of the sprite.
        /// </summary>
        /// <param name="sprite">The sprite that the RectTransform should change its size to.</param>
        /// <param name="spriteRectTransform">A reference to the sprite's RectTransform.</param>
        public static void SizeSprite(Sprite sprite, RectTransform spriteRectTransform)
        {
            if (sprite != null) {
                var sizeDelta = spriteRectTransform.sizeDelta;
                sizeDelta.x = sprite.textureRect.width;
                sizeDelta.y = sprite.textureRect.height;
                spriteRectTransform.sizeDelta = sizeDelta;
            }
        }

        /// <summary>
        /// Clears the Unity Engine Utility cache.
        /// </summary>
        /// 
#if UNITY_2019_3_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        public static void ClearCache()
        {
            if (s_TypeLookup != null) { s_TypeLookup.Clear(); }
            if (s_GameObjectCameraMap != null) { s_GameObjectCameraMap.Clear(); }
            if (s_ObjectUpdated != null) { s_ObjectUpdated.Clear(); }
            if (s_LoadedAssemblies != null) { s_LoadedAssemblies.Clear(); }
            if (s_FieldAttributeMap != null) { s_FieldAttributeMap.Clear(); }
            if (s_PropertyAttributeMap != null) { s_PropertyAttributeMap.Clear(); }
        }

        /// <summary>
        /// Allows for comparison between RaycastHit objects.
        /// </summary>
        public class RaycastHitComparer : IComparer<RaycastHit>
        {
            /// <summary>
            /// Compare RaycastHit x to RaycastHit y. If x has a smaller distance value compared to y then a negative value will be returned.
            /// If the distance values are equal then 0 will be returned, and if y has a smaller distance value compared to x then a positive value will be returned.
            /// </summary>
            /// <param name="x">The first RaycastHit to compare.</param>
            /// <param name="y">The second RaycastHit to compare.</param>
            /// <returns>The resulting difference between RaycastHit x and y.</returns>
            public int Compare(RaycastHit x, RaycastHit y)
            {
                if (x.transform == null) {
                    return int.MaxValue;
                }
                if (y.transform == null) {
                    return int.MinValue;
                }
                return x.distance.CompareTo(y.distance);
            }
        }

        /// <summary>
        /// Allows for equity comparison checks between RaycastHit objects.
        /// </summary>
        public struct RaycastHitEqualityComparer : IEqualityComparer<RaycastHit>
        {
            /// <summary>
            /// Determines if RaycastHit x is equal to RaycastHit y.
            /// </summary>
            /// <param name="x">The first RaycastHit to compare.</param>
            /// <param name="y">The second RaycastHit to compare.</param>
            /// <returns>True if the raycasts are equal.</returns>
            public bool Equals(RaycastHit x, RaycastHit y)
            {
                if (x.distance != y.distance) {
                    return false;
                }
                if (x.point != y.point) {
                    return false;
                }
                if (x.normal != y.normal) {
                    return false;
                }
                if (x.transform != y.transform) {
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Returns a hash code for the RaycastHit.
            /// </summary>
            /// <param name="hit">The RaycastHit to get the hash code of.</param>
            /// <returns>The hash code for the RaycastHit.</returns>
            public int GetHashCode(RaycastHit hit)
            {
                // Don't use hit.GetHashCode because that has boxing. This hash function won't always prevent duplicates but it's fine for what it's used for.
                return ((int)(hit.distance * 10000)) ^ ((int)(hit.point.x * 10000)) ^ ((int)(hit.point.y * 10000)) ^ ((int)(hit.point.z * 10000)) ^
                        ((int)(hit.normal.x * 10000)) ^ ((int)(hit.normal.y * 10000)) ^ ((int)(hit.normal.z * 10000));
            }
        }
    }

    /// <summary>
    /// A container for a min and max float value.
    /// </summary>
    [Serializable]
    public struct MinMaxFloat
    {
        [Tooltip("The minimum Vector3 value.")]
        [SerializeField] private float m_MinValue;
        [Tooltip("The maximum Vector3 value.")]
        [SerializeField] private float m_MaxValue;

        public float MinValue { get { return m_MinValue; } set { m_MinValue = value; } }
        public float MaxValue { get { return m_MaxValue; } set { m_MaxValue = value; } }

        public float RandomValue
        {
            get
            {
                return UnityEngine.Random.Range(m_MinValue, m_MaxValue);
            }
        }

        /// <summary>
        /// MinMaxFloat constructor which can specify the min and max values.
        /// </summary>
        /// <param name="minValue">The minimum float value.</param>
        /// <param name="maxValue">The maximum float value.</param>
        public MinMaxFloat(float minValue, float maxValue)
        {
            m_MinValue = minValue;
            m_MaxValue = maxValue;
        }
    }

    /// <summary>
    /// A container for a min and max Vector3 value.
    /// </summary>
    [Serializable]
    public struct MinMaxVector3
    {
        [Tooltip("The minimum Vector3 value.")]
        [SerializeField] private Vector3 m_MinValue;
        [Tooltip("The maximum Vector3 value.")]
        [SerializeField] private Vector3 m_MaxValue;
        [Tooltip("The minimum magnitude value when determining a random value.")]
        [SerializeField] private Vector3 m_MinMagnitude;

        public Vector3 MinValue { get { return m_MinValue; } set { m_MinValue = value; } }
        public Vector3 MaxValue { get { return m_MaxValue; } set { m_MaxValue = value; } }
        public Vector3 MinMagnitude { get { return m_MinMagnitude; } set { m_MinMagnitude = value; } }

        public Vector3 RandomValue
        {
            get
            {
                var value = Vector3.zero;
                value.x = GetRandomFloat(m_MinValue.x, m_MaxValue.x, m_MinMagnitude.x);
                value.y = GetRandomFloat(m_MinValue.y, m_MaxValue.y, m_MinMagnitude.y);
                value.z = GetRandomFloat(m_MinValue.z, m_MaxValue.z, m_MinMagnitude.z);
                return value;
            }
        }

        /// <summary>
        /// MinMaxVector3 constructor which can specify the min and max values.
        /// </summary>
        /// <param name="minValue">The minimum Vector3 value.</param>
        /// <param name="maxValue">The maximum Vector3 value.</param>
        public MinMaxVector3(Vector3 minValue, Vector3 maxValue)
        {
            m_MinValue = minValue;
            m_MaxValue = maxValue;
            m_MinMagnitude = Vector3.zero;
        }

        /// <summary>
        /// MinMaxVector3 constructor which can specify the min and max values.
        /// </summary>
        /// <param name="minValue">The minimum Vector3 value.</param>
        /// <param name="maxValue">The maximum Vector3 value.</param>
        /// <param name="minMagnitude">The minimum magnitude of the random value.</param>
        public MinMaxVector3(Vector3 minValue, Vector3 maxValue, Vector3 minMagnitude)
        {
            m_MinValue = minValue;
            m_MaxValue = maxValue;
            m_MinMagnitude = minMagnitude;
        }

        /// <summary>
        /// Returns a random float between the min and max value with the specified minimum magnitude.
        /// </summary>
        /// <param name="minValue">The minimum float value.</param>
        /// <param name="maxValue">The maximum float value.</param>
        /// <param name="minMagnitude">The minimum magnitude of the random value.</param>
        /// <returns>A random float between the min and max value.</returns>
        private float GetRandomFloat(float minValue, float maxValue, float minMagnitude)
        {
            if (minMagnitude != 0 && Mathf.Sign(m_MinValue.x) != Mathf.Sign(m_MaxValue.x)) {
                if (Mathf.Sign(UnityEngine.Random.Range(m_MinValue.x, m_MaxValue.x)) > 0) {
                    return UnityEngine.Random.Range(minMagnitude, Mathf.Max(minMagnitude, maxValue));
                }
                return UnityEngine.Random.Range(-minMagnitude, Mathf.Min(-minMagnitude, minValue));
            } else {
                return UnityEngine.Random.Range(minValue, maxValue);
            }
        }
    }

    /// <summary>
    /// Represents the object which can be spawned.
    /// </summary>
    [System.Serializable]
    public class ObjectSpawnInfo
    {
#pragma warning disable 0649
        [Tooltip("The object that can be spawned.")]
        [SerializeField] private GameObject m_Object;
        [Tooltip("The probability that the object can be spawned.")]
        [Range(0, 1)] [SerializeField] private float m_Probability = 1;
        [Tooltip("Should a random spin be applied to the object after it has been spawned?")]
        [SerializeField] private bool m_RandomSpin;
#pragma warning restore 0649

        public GameObject Object { get { return m_Object; } }
        public float Probability { get { return m_Probability; } }
        public bool RandomSpin { get { return m_RandomSpin; } }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="position">The position to instantiate the object at.</param>
        /// <param name="normal">The normal of the instantiated object.</param>
        /// <param name="gravityDirection">The normalized direction of the character's gravity.</param>
        /// <returns>The instantiated object (can be null). </returns>
        public GameObject Instantiate(Vector3 position, Vector3 normal, Vector3 gravityDirection)
        {
            if (m_Object == null) {
                return null;
            }

            // There is a random chance that the object cannot be spawned.
            if (UnityEngine.Random.value < m_Probability) {
                var rotation = Quaternion.LookRotation(normal);
                // A random spin can be applied so the rotation isn't the same every hit.
                if (m_RandomSpin) {
                    rotation *= Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), normal);
                }
                var instantiatedObject = ObjectPool.Instantiate(m_Object, position, rotation);
                // If the DirectionalConstantForce component exists then the gravity direction should be set so the object will move in the correct direction.
                var directionalConstantForce = instantiatedObject.GetCachedComponent<Traits.DirectionalConstantForce>();
                if (directionalConstantForce != null) {
                    directionalConstantForce.Direction = gravityDirection;
                }
                return instantiatedObject;
            }
            return null;
        }
    }

    /// <summary>
    /// Struct which stores the material values to revert back to after the material has been faded.
    /// </summary>
    public struct OriginalMaterialValue
    {
        [Tooltip("The color of the material.")]
        private Color m_Color;
        [Tooltip("Does the material have a mode property?")]
        private bool m_ContainsMode;
        [Tooltip("The render mode of the material.")]
        private float m_Mode;
        [Tooltip("The SourceBlend BlendMode of the material.")]
        private int m_SrcBlend;
        [Tooltip("The DestinationBlend BlendMode of the material.")]
        private int m_DstBlend;
        [Tooltip("Is alpha blend enabled?")]
        private bool m_AlphaBlend;
        [Tooltip("The render queue of the material.")]
        private int m_RenderQueue;

        public Color Color { get { return m_Color; } set { m_Color = value; } }
        public bool ContainsMode { get { return m_ContainsMode; } set { m_ContainsMode = value; } }
        public float Mode { get { return m_Mode; } set { m_Mode = value; } }
        public int SrcBlend { get { return m_SrcBlend; } set { m_SrcBlend = value; } }
        public int DstBlend { get { return m_DstBlend; } set { m_DstBlend = value; } }
        public bool AlphaBlend { get { return m_AlphaBlend; } set { m_AlphaBlend = value; } }
        public int RenderQueue { get { return m_RenderQueue; } set { m_RenderQueue = value; } }

        private static int s_ModeID;
        private static int s_SrcBlendID;
        private static int s_DstBlendID;
        private static string s_AlphaBlendString = "_ALPHABLEND_ON";

        public static int ModeID { get { return s_ModeID; } }
        public static int SrcBlendID { get { return s_SrcBlendID; } }
        public static int DstBlendID { get { return s_DstBlendID; } }
        public static string AlphaBlendString { get { return s_AlphaBlendString; } }

        /// <summary>
        /// Initializes the OriginalMaterialValue.
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            s_ModeID = Shader.PropertyToID("_Mode");
            s_SrcBlendID = Shader.PropertyToID("_SrcBlend");
            s_DstBlendID = Shader.PropertyToID("_DstBlend");
        }

        /// <summary>
        /// Initializes the OriginalMaterialValue to the material values.
        /// </summary>
        /// <param name="color">The material to initialize.</param>
        /// <param name="colorID">The id of the color property.</param>
        /// <param name="mode">Does the material have a Mode property?</param>
        public void Initialize(Material material, int colorID, bool containsMode)
        {
            m_Color = material.GetColor(colorID);
            m_AlphaBlend = material.IsKeywordEnabled(s_AlphaBlendString);
            m_RenderQueue = material.renderQueue;
            m_ContainsMode = containsMode;
            if (containsMode) {
                m_Mode = material.GetFloat(s_ModeID);
                m_SrcBlend = material.GetInt(s_SrcBlendID);
                m_DstBlend = material.GetInt(s_DstBlendID);
            }
        }
    }

    /// <summary>
    /// Storage class for determining if an event is triggered based on an animation event or time.
    /// </summary>
    [System.Serializable]
    public class AnimationEventTrigger
    {
        [Tooltip("Is the event triggered with a Unity animation event?")]
        [SerializeField] private bool m_WaitForAnimationEvent;
        [Tooltip("The amount of time it takes to trigger the event if not using an animation event.")]
        [SerializeField] private float m_Duration;

        public bool WaitForAnimationEvent { get { return m_WaitForAnimationEvent; } set { m_WaitForAnimationEvent = value; } }
        public float Duration { get { return m_Duration; } set { m_Duration = value; } }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AnimationEventTrigger() { }

        /// <summary>
        /// Two parameter constructor for AnimationEventTrigger.
        /// </summary>
        /// <param name="waitForAnimationEvent">Is the event triggered with a Unity animation event?</param>
        /// <param name="duration">The amount of time it takes to trigger the event if not using an animation event.</param>
        public AnimationEventTrigger(bool waitForAnimationEvent, float duration)
        {
            m_WaitForAnimationEvent = waitForAnimationEvent;
            m_Duration = duration;
        }
    }

    /// <summary>
    /// Attribute which allows the inspector to draw a foldout without the need of a custom editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class InspectorFoldout : Attribute
    {
        private string m_Title;
        public string Title { get { return m_Title; } }
        public InspectorFoldout(string title)
        {
            m_Title = title;
        }
    }

    /// <summary>
    /// Attribute which allows the same type to be added multiple times.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AllowDuplicateTypes : Attribute
    {
        // Intentionally left blank.
    }
}