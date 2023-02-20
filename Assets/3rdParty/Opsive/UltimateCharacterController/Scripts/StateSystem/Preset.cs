/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Opsive.UltimateCharacterController.Utility;
using System;
using System.Collections;

namespace Opsive.UltimateCharacterController.StateSystem
{
    /// <summary>
    /// Represents a set of values for any number of component properties. In order for the value to be applied to a property a getter and setter must exist,
    /// along with a derived class from BaseDelegate which creates the delegate which interfaces with the property getter and setter. Properties can be 
    /// ignored with the [Opsive.UltimateCharacterController.Utility.NonSerialized] attribute.
    /// </summary>
    public class Preset : ScriptableObject
    {
        protected BaseDelegate[] m_Delegates;

        public bool IsInitialized { get { return m_Delegates != null; } }
        public BaseDelegate[] Delegates { get { return m_Delegates; } }

        /// <summary>
        /// Creates a preset based off of the specified component.
        /// </summary>
        /// <param name="obj">The object to retrieve the property values of.</param>
        /// <returns>The created preset. Null if no properties have been found to create the preset with.</returns>
        public static Preset CreatePreset()
        {
            return CreateInstance<Preset>();
        }

        /// <summary>
        /// Initializes the preset. The preset must be initialized before the preset values are applied so the delegates can be created.
        /// </summary>
        /// <param name="obj">The object to map the delegates to.</param>
        public void Initialize(object obj)
        {
            Initialize(obj, MemberVisibility.Public);
        }

        /// <summary>
        /// Initializes the preset with the specified visiblity. The preset must be initialized before the preset values are applied so the delegates can be created.
        /// </summary>
        /// <param name="obj">The object to map the delegates to.</param>
        /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        public virtual void Initialize(object obj, MemberVisibility visibility)
        {
            var properties = Serialization.GetSerializedProperties(obj.GetType(), visibility);
            var valueCount = 0;
            m_Delegates = new BaseDelegate[properties.Length];
            for (int i = 0; i < properties.Length; ++i) {
                // The property may not be valid.
                if (Serialization.GetValidGetMethod(properties[i], visibility) == null) {
                    continue;
                }

                // Create a generic delegate based on the property type.
                var genericDelegateType = typeof(GenericDelegate<>).MakeGenericType(properties[i].PropertyType);
                m_Delegates[valueCount] = Activator.CreateInstance(genericDelegateType) as BaseDelegate;

                // Initialize the delegate.
                if (m_Delegates[valueCount] != null) {
                    m_Delegates[valueCount].Initialize(obj, properties[i], visibility);
                } else {
                    Debug.LogWarning("Warning: Unable to create preset of type " + properties[i].PropertyType);
                }
                valueCount++;
            }
            if (m_Delegates.Length != valueCount) {
                Array.Resize(ref m_Delegates, valueCount);
            }
        }

        /// <summary>
        /// Updates the stored value with the current property value.
        /// </summary>
        public virtual void UpdateValue()
        {
            for (int i = 0; i < m_Delegates.Length; ++i) {
                m_Delegates[i].UpdateValue();
            }
        }

        /// <summary>
        /// Applies the values to the component.
        /// </summary>
        public void ApplyValues()
        {
            for (int i = 0; i < m_Delegates.Length; ++i) {
                m_Delegates[i].ApplyValue();
            }
        }

        /// <summary>
        /// Applies the values to the component specified by the delegates.
        /// </summary>
        /// <param name="delegates">The properties that were changed.</param>
        public virtual void ApplyValues(BaseDelegate[] delegates) { }

        /// <summary>
        /// Abstract class which allows for a delegate to be created which can be called on when the preset value should be applied.
        /// </summary>
        public abstract class BaseDelegate
        {
            public abstract MethodInfo SetMethod { get; }

            /// <summary>
            /// Initialize the delegate and value.
            /// </summary>
            /// <param name="obj">The object which the delegate operates on.</param>
            /// <param name="property">The property that the delegate will invoke.</param>
            /// <param name="valuePositionMap">A mapping between the value hash and position.</param>
            /// <param name="data">The serialization data which contains the values for the property (as well as all other properties).</param>
            /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
            public abstract void Initialize(object obj, PropertyInfo property, Dictionary<int, int> valuePositionMap, Serialization data, MemberVisibility visibility);

            /// <summary>
            /// Initialize the delegate.
            /// </summary>
            /// <param name="obj">The object which the delegate operates on.</param>
            /// <param name="property">The property that the delegate will invoke.</param>
            /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
            public abstract void Initialize(object obj, PropertyInfo property, MemberVisibility visibility);

            /// <summary>
            /// Updates the stored value with the current property value.
            /// </summary>
            /// <param name="obj">The object which the delegate operates on.</param>
            /// <param name="property">The property that the delegate will invoke.</param>
            /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
            public abstract void UpdateValue();

            /// <summary>
            /// Applies the preset value to the delegate.
            /// </summary>
            public abstract void ApplyValue();
        }

        /// <summary>
        /// Generic class which implements a type specific delegate and value that can be called on when the preset value should be applied.
        /// See AOTLinker for an explanation of why a different class name is used for AOT platforms.
        /// </summary>
#if NET_4_6 || UNITY_2018_3_OR_NEWER || UNITY_WEBGL || UNITY_IOS || UNITY_ANDROID || UNITY_WII || UNITY_WIIU || UNITY_SWITCH || UNITY_PS3 || UNITY_PS4 || UNITY_XBOXONE || UNITY_WSA
        public class GenericDelegate<T> : BaseDelegate
#else
        public class GenericDelegate<T> : BaseDelegate where T : Type
#endif
        {
            private T m_Value;
            private MethodInfo m_SetMethod;
            private Action<T> m_Setter;
            private Func<T> m_Getter;
            private bool m_IsIList;

            public override MethodInfo SetMethod { get { return m_SetMethod; } }

            /// <summary>
            /// Initialize the delegate and value.
            /// </summary>
            /// <param name="obj">The object which the delegate operates on.</param>
            /// <param name="property">The property that the delegate will invoke.</param>
            /// <param name="valuePositionMap">A mapping between the value hash and position.</param>
            /// <param name="data">The serialization data which contains the values for the property (as well as all other properties).</param>
            /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
            public override void Initialize(object obj, PropertyInfo property, Dictionary<int, int> valuePositionMap, Serialization data, MemberVisibility visibility)
            {
                m_SetMethod = property.GetSetMethod(visibility != MemberVisibility.Public);
                if (m_SetMethod != null) {
                    m_Setter = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), obj, m_SetMethod);
#if NET_4_6 || UNITY_2018_3_OR_NEWER || UNITY_WEBGL || UNITY_IOS || UNITY_ANDROID || UNITY_WII || UNITY_WIIU || UNITY_SWITCH || UNITY_PS3 || UNITY_PS4 || UNITY_XBOXONE || UNITY_WSA
                    var value = Serializer.BytesToValue(typeof(T), property.Name, valuePositionMap, 0, data.Values, data.ValuePositions, data.UnityObjects, false, visibility);
                    if (value != null && !value.Equals(null)) {
                        m_Value = (T)value;
                    }
#else
                    m_Value = Serializer<T>.BytesToValue(property.Name, valuePositionMap, 0, data.Values, data.ValuePositions, data.UnityObjects, false, visibility);
#endif
                    var type = typeof(T);
                    m_IsIList = typeof(IList).IsAssignableFrom(type);
                    if (m_IsIList) {
                        // The Get method only needs to be assigned if the type is an IList because the actual object isn't copied by reference for arrays.
                        // Each individual element within the array needs to be interated on.
                        var getMethod = property.GetGetMethod(visibility != MemberVisibility.Public);
                        if (getMethod != null) {
                            m_Getter = (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), obj, getMethod);
                        }
                    }
                }
            }

            /// <summary>
            /// Initialize the delegate.
            /// </summary>
            /// <param name="obj">The object which the delegate operates on.</param>
            /// <param name="property">The property that the delegate will invoke.</param>
            /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
            public override void Initialize(object obj, PropertyInfo property, MemberVisibility visibility)
            {
                m_SetMethod = property.GetSetMethod(visibility != MemberVisibility.Public);
                if (m_SetMethod != null) {
                    m_Setter = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), obj, m_SetMethod);
                }
                
                var getMethod = property.GetGetMethod(visibility != MemberVisibility.Public);
                if (getMethod != null) {
                    m_Getter = (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), obj, getMethod);

                    // Create an instance of the value if it is an array or a list. This will allow a snapshot of the array/list elements to be saved without having the
                    // array/list change because it is later modified by reference.
                    var type = typeof(T);
                    m_IsIList = typeof(IList).IsAssignableFrom(type);
                    if (m_IsIList) {
                        if (typeof(T).IsArray) {
                            var value = m_Getter() as Array;
#if NET_4_6 || UNITY_2018_3_OR_NEWER || UNITY_WEBGL || UNITY_IOS || UNITY_ANDROID || UNITY_WII || UNITY_WIIU || UNITY_SWITCH || UNITY_PS3 || UNITY_PS4 || UNITY_XBOXONE || UNITY_WSA
                            m_Value = (T)(object)Array.CreateInstance(type.GetElementType(), value == null ? 0 : value.Length);
#else
                            m_Value = Array.CreateInstance(type.GetElementType(), value == null ? 0 : value.Length) as T;
#endif
                        } else {
                            var baseType = type;
                            while (!baseType.IsGenericType) {
                                baseType = baseType.BaseType;
                            }
                            var elementType = baseType.GetGenericArguments()[0];
                            if (type.IsGenericType) {
                                m_Value = (T)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                            } else {
                                m_Value = (T)Activator.CreateInstance(type);
                            }
                        }
                    }

                    // The value should be set at the same time the delegate is initailized.
                    UpdateValue();
                }
            }

            /// <summary>
            /// Updates the stored value with the current property value.
            /// </summary>
            public override void UpdateValue()
            {
                if (m_Getter == null) {
                    Debug.LogError("Error: Unable to retrieve an updated value - the Get method is null.");
                    return;
                }

                // Update the individual elements if the value is a list or an array. This will prevent the array/list from being changed because it is 
                // later modified by reference.
                if (m_IsIList) {
                    UpdateIList(m_Getter(), m_Value);
                } else { // Not an array/list.
                    m_Value = m_Getter();
                }
            }

            /// <summary>
            /// Applies the preset value to the delegate.
            /// </summary>
            public override void ApplyValue()
            {
                if (m_IsIList) {
                    UpdateIList(m_Value, m_Getter());
                } else {
                    m_Setter(m_Value);
                }
            }

            /// <summary>
            /// Updates the source array/list elements to the destination elements.
            /// </summary>
            /// <param name="source">The array/list to copy the references from.</param>
            /// <param name="destination">The array/list to copy the references to.</param>
            private void UpdateIList(T source, T destination)
            {
                var type = typeof(T);
                if (type.IsArray) {
                    var sourceArray = source as Array;
                    var destinationArray = destination as Array;
                    if (sourceArray != null && destinationArray != null) {
                        // The array sizes need to match.
                        if (destinationArray != null && sourceArray.Length != destinationArray.Length) {
                            destinationArray = Array.CreateInstance(typeof(T).GetElementType(), sourceArray.Length);
                            // There's no way to avoid the boxing/unboxing. It is recommended that arrays are not changed to avoid this.
                            m_Setter((T)(object)destinationArray);
                        }
                        for (int i = 0; i < sourceArray.Length; ++i) {
                            destinationArray.SetValue(sourceArray.GetValue(i), i);
                        }
                    }
                } else {
                    var sourceList = source as IList<T>;
                    var destinationList = destination as IList<T>;
                    // Remove any extra elements.
                    if (destinationList.Count > sourceList.Count) {
                        var removeCount = destinationList.Count - sourceList.Count;
                        for (int i = 0; i < removeCount; ++i) {
                            destinationList.RemoveAt(destinationList.Count - 1);
                        }
                    }
                    // Update the element referance.
                    for (int i = 0; i < sourceList.Count; ++i) {
                        if (i < destinationList.Count) {
                            destinationList[i] = sourceList[i];
                        } else {
                            destinationList.Add(sourceList[i]);
                        }
                    }
                }
            }
        }
    }
}