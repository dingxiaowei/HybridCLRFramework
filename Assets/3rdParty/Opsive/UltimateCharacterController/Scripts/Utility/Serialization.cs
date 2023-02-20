/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Opsive.UltimateCharacterController.Utility
{
    /// <summary>
    /// The Serialization class stores the serialized values for a specified object type. It is generic enough to be used for both fields and properties.
    /// </summary>
    [Serializable]
    public class Serialization
    {
        [Tooltip("The class type that the values represent.")]
        [SerializeField] protected string m_ObjectType;
        [Tooltip("A list of all value hashes that the serialization applies to.")]
        [SerializeField] protected int[] m_ValueHashes;
        [Tooltip("Maps the value hash to a position within the value array.")]
        [SerializeField] protected int[] m_ValuePositions;
        [Tooltip("An array of saved values.")]
        [SerializeField] protected byte[] m_Values;
        [Tooltip("Unity objects are serialized by Unity so store a reference to those objects.")]
        [SerializeField] UnityEngine.Object[] m_UnityObjects;
        [Tooltip("The Ultimate Character Controller version used for serialization.")]
        [SerializeField] protected string m_Version;

        private static Dictionary<Type, FieldInfo[]>[] s_SerializedFieldsMaps;
        private static Dictionary<Type, PropertyInfo[]>[] s_SerializedPropertiesMaps;
        private static Dictionary<string, int> s_StringHashMap;

        public string ObjectType { get { return m_ObjectType; } set { m_ObjectType = value; } }
        public int[] ValueHashes { get { return m_ValueHashes; } set { m_ValueHashes = value; } }
        public byte[] Values { get { return m_Values; } set { m_Values = value; } }
        public int[] ValuePositions { get { return m_ValuePositions; } set { m_ValuePositions = value; } }
        public UnityEngine.Object[] UnityObjects { get { return m_UnityObjects; } set { m_UnityObjects = value; } }

        /// <summary>
        /// Serialize the given object. Can serialize the fields or properties.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <param name="useFields">Should the fields be serialized? If false the properties will be serialized.</param>
        /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        public void Serialize(object obj, bool useFields, MemberVisibility visibility)
        {
            if (obj == null) {
                return;
            }

            var objType = obj.GetType();
            m_ObjectType = objType.ToString();
            m_Version = AssetInfo.Version;

            // No fields/properties need to be serialized with a none visibility.
            if (visibility == MemberVisibility.None || visibility == MemberVisibility.Last) {
                m_ValueHashes = new int[0];
                m_ValuePositions = new int[0];
                m_Values = new byte[0];
                m_UnityObjects = new UnityEngine.Object[0];
                return;
            }

            if (useFields) {
                // Fields are not serialized during runtime so more allocations are ok.
                var validFields = 0;
                var fields = GetSerializedFields(objType, visibility);
                for (int i = 0; i < fields.Length; ++i) {
                    var value = fields[i].GetValue(obj);
                    if (value == null && !Serializer.IsSerializedType(fields[i].FieldType) && 
                                    !typeof(IList).IsAssignableFrom(fields[i].FieldType) && (fields[i].FieldType.IsClass || (fields[i].FieldType.IsValueType && !fields[i].FieldType.IsPrimitive))) {
                        value = Activator.CreateInstance(fields[i].FieldType);
                        fields[i].SetValue(obj, value);
                    }
                    validFields += GetValueCount(fields[i].FieldType, value, true, visibility);
                }

                // Initialize the values.
                m_ValueHashes = new int[validFields];
                m_ValuePositions = new int[validFields];
                var values = new List<byte>();
                var valueCount = 0;

                // Serialize all of the fields. This method is recursive so it'll also serialize any classes.
                SerializeFields(obj, 0, ref valueCount, ref m_ValueHashes, ref m_ValuePositions, ref values, ref m_UnityObjects, visibility);

                // The fields have been serialized. Store the byte array.
                m_Values = values.ToArray();
            } else {
                // Properties can be serialized at runtime by presets so two passes are necessary to reduce the number of total allocations.
                // The first pass will determine the number of properties/fields that can be added to the serializer, then the second pass will actually add those field/property values. 
                // After the first pass is complete the arrays will be allocated which contains the exact number of values that can be added.
                var validProperties = 0;
                var properties = GetSerializedProperties(objType, visibility);
                for (int i = 0; i < properties.Length; ++i) {
                    var getMethod = GetValidGetMethod(properties[i], visibility);
                    if (getMethod != null) {
                        var value = getMethod.Invoke(obj, null);
                        if (value == null && !Serializer.IsSerializedType(properties[i].PropertyType) && !typeof(IList).IsAssignableFrom(properties[i].PropertyType) && 
                                                    (properties[i].PropertyType.IsClass || (properties[i].PropertyType.IsValueType && !properties[i].PropertyType.IsPrimitive))) {
                            value = Activator.CreateInstance(properties[i].PropertyType);
                            var setMethod = properties[i].GetSetMethod((visibility != MemberVisibility.Public && visibility != MemberVisibility.AllPublic));
                            setMethod.Invoke(obj, new object[] { value });
                        }
                        validProperties += GetValueCount(properties[i].PropertyType, value, false, visibility);
                    }
                }

                // Initialize the values. If there are no valid properties then the array length will be zero.
                m_ValueHashes = new int[validProperties];
                m_ValuePositions = new int[validProperties];

                // If there are any valid properties then use the properties get method to get the byte value. The property can then be saved off to the serializer.
                if (validProperties > 0) {
                    // It is not known ahead of time how large the byte array will be so use a list to be able to add the values dynamically.
                    var values = new List<byte>();

                    // Serialize all of the valid properties. This method is recursive so it'll also serialize any classes.
                    var valueCount = 0;
                    SerializeProperties(obj, 0, ref valueCount, ref m_ValueHashes, ref m_ValuePositions, ref values, ref m_UnityObjects, visibility);

                    // The properties have been serialized. Store the byte array.
                    m_Values = values.ToArray();
                }
            }
        }

        /// <summary>
        /// Serialize the fields of the specified object.
        /// </summary>
        /// <param name="obj">The object to serialize the fields of.</param>
        /// <param name="hashPrefix">The prefix of the hash from the parent class. This value will prevent collisions with similarly named objects.</param>
        /// <param name="valueCount">The current index within the value hash and position array.</param>
        /// <param name="valueHashes">The unique hash for the field that is being serialized.</param>
        /// <param name="valuePositions">The positions of the all of the values.</param>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="unityObjects">A reference to the unity objects array. Used if the type is a UnityEngine.Object.</param>
        /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        public static void SerializeFields(object obj, int hashPrefix, ref int valueCount, ref int[] valueHashes, ref int[] valuePositions, ref List<byte> values, ref UnityEngine.Object[] unityObjects, MemberVisibility visibility)
        {
            var fields = GetSerializedFields(obj.GetType(), visibility);
            for (int i = 0; i < fields.Length; ++i) {
                Serializer.SerializeValue(fields[i].FieldType, fields[i].GetValue(obj), ref valueHashes, ref values, ref valuePositions, ref unityObjects, ref valueCount, hashPrefix, fields[i].Name, true, visibility);
            }
        }

        /// <summary>
        /// Serialize the properties of the specified object.
        /// </summary>
        /// <param name="obj">The object to serialize the properties of.</param>
        /// <param name="hashPrefix">The prefix of the hash from the parent class. This value will prevent collisions with similarly named objects.</param>
        /// <param name="valueCount">The current index within the value hash and position array.</param>
        /// <param name="valueHashes">The unique hash for the property that is being serialized.</param>
        /// <param name="valuePositions">The positions of the all of the values.</param>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="unityObjects">A reference to the unity objects array. Used if the type is a UnityEngine.Object.</param>
        /// /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        public static void SerializeProperties(object obj, int hashPrefix, ref int valueCount, ref int[] valueHashes, ref int[] valuePositions, ref List<byte> values, ref UnityEngine.Object[] unityObjects, MemberVisibility visibility)
        {
            var properties = GetSerializedProperties(obj.GetType(), visibility);
            for (int i = 0; i < properties.Length; ++i) {
                var getMethod = GetValidGetMethod(properties[i], visibility);
                if (getMethod != null) {
                    Serializer.SerializeValue(properties[i].PropertyType, getMethod.Invoke(obj, null), ref valueHashes, ref values, ref valuePositions, ref unityObjects, ref valueCount, hashPrefix, properties[i].Name, false, visibility);
                }
            }
        }

        /// <summary>
        /// Recursively determines the number of valid child values that the specified type contains.
        /// </summary>
        /// <param name="type">The object type to search.</param>
        /// <param name="obj">The object to determine the number of valid child values of.</param>
        /// <param name="useFields">Should the fields be searched? If false then the properties will be used.</param>
        /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        public static int GetValueCount(Type type, object obj, bool useFields, MemberVisibility visibility)
        {
            // If the type is a list, class or struct then the number of child values need to be determined. Nonclass/struct objects can only contain themself.
            // Classes/structs which have already been defined (such as Vector3) can be serialized/deserialized immediately so can return 1.
            if (typeof(IList).IsAssignableFrom(type)) {
                var listObj = obj as IList;
                // The list type doesn't need to be search - instead the base element type should be searched so all of the values can be found.
                Type elementType;
                if (type.IsArray) {
                    elementType = type.GetElementType();
                } else {
                    var baseFieldType = type;
                    while (!baseFieldType.IsGenericType) {
                        baseFieldType = baseFieldType.BaseType;
                    }
                    elementType = baseFieldType.GetGenericArguments()[0];
                }
                // The size of the array counts as a value in addition to any elements.
                var validFields = 1;
                if (listObj != null) {
                    for (int i = 0; i < listObj.Count; ++i) {
                        if (listObj[i] == null) {
                            validFields++;
                        } else {
                            validFields += GetValueCount(elementType, listObj[i], useFields, visibility);
                        }
                    }
                }
                return validFields;
            } else if (!Serializer.IsSerializedType(type) && (type.IsClass || (type.IsValueType && !type.IsPrimitive))) {
                if (useFields) {
                    var fields = GetSerializedFields(type, visibility);
                    var validFields = 1;
                    // Loop through all of the fields for the class/struct object. Recursively determine the number of valid child properties the current property type contains.
                    for (int i = 0; i < fields.Length; ++i) {
                        var value = fields[i].GetValue(obj);
                        if (value == null && !Serializer.IsSerializedType(fields[i].FieldType) && !typeof(IList).IsAssignableFrom(fields[i].FieldType) &&
                                                    (fields[i].FieldType.IsClass || (fields[i].FieldType.IsValueType && !fields[i].FieldType.IsPrimitive))) {
                            value = Activator.CreateInstance(fields[i].FieldType);
                            fields[i].SetValue(obj, value);
                        }
                        validFields += GetValueCount(fields[i].FieldType, value, useFields, visibility);
                    }
                    return validFields;
                } else {
                    var properties = GetSerializedProperties(type, visibility);
                    var validProperties = 1;
                    // Loop through all of the properties for the class/struct object. Recursively determine the number of valid child properties the current property type contains.
                    for (int i = 0; i < properties.Length; ++i) {
                        var getMethod = GetValidGetMethod(properties[i], visibility);
                        if (getMethod != null) {
                            var value = getMethod.Invoke(obj, null);
                            if (value == null && !Serializer.IsSerializedType(properties[i].PropertyType) && !typeof(IList).IsAssignableFrom(properties[i].PropertyType) &&
                                                    (properties[i].PropertyType.IsClass || (properties[i].PropertyType.IsValueType && !properties[i].PropertyType.IsPrimitive))) {
                                value = Activator.CreateInstance(properties[i].PropertyType);
                                var setMethod = properties[i].GetSetMethod((visibility != MemberVisibility.Public && visibility != MemberVisibility.AllPublic));
                                setMethod.Invoke(obj, new object[] { value });
                            }
                            validProperties += GetValueCount(properties[i].PropertyType, value, useFields, visibility);
                        }
                    }
                    return validProperties;
                }
            }
            // Not a list, class or struct: return 1.
            return 1;
        }

        /// <summary>
        /// Returns a get method with the specified property name. Null will be returned if the get method isn't valid.
        /// </summary>
        /// <param name="property">The property to get the get method of.</param>
        /// /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        /// <returns>The get method if it exists, otherwise null.</returns>
        public static MethodInfo GetValidGetMethod(PropertyInfo property, MemberVisibility visibility)
        {
            var getMethod = property.GetGetMethod((visibility != MemberVisibility.Public && visibility != MemberVisibility.AllPublic));
            if (getMethod == null) { // The property doesn't have a get method.
                return null;
            }

            // If the property doesn't have a set method then the value won't be able to be applied.
            var setMethod = property.GetSetMethod((visibility != MemberVisibility.Public && visibility != MemberVisibility.AllPublic));
            if (setMethod == null) {
                return null;
            }

            // Get the base property type for further checks.
            var type = property.PropertyType;
            if(typeof(IList).IsAssignableFrom(type)) {
                if (type.IsArray) {
                    type = type.GetElementType();
                } else {
                    while (!type.IsGenericType) {
                        type = type.BaseType;
                    }
                    type = type.GetGenericArguments()[0];
                }
            }

            // Do not serialize the Serialization class.
            if (type == typeof(Serialization)) {
                return null;
            }

            // Do not serialize the UnityEvent class.
            if (typeof(UnityEngine.Events.UnityEventBase).IsAssignableFrom(type)) {
                return null;
            }

            // Delegates and abstract classes cannot be serialized.
            // This check isn't necessary for the Snapshot MemberVisibility type because Snapshot doesn't serialize the values.
            if (visibility != MemberVisibility.Snapshot) {
                if (type.IsAbstract || typeof(Delegate).IsAssignableFrom(type)) {
                    return null;
                }
            }

            // The property is valid.
            return getMethod;
        }

        /// <summary>
        /// Deserializes the fields of the current object type.
        /// </summary>
        /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        /// <returns>The deserialized object.</returns>
        public object DeserializeFields(MemberVisibility visibility)
        {
            var objType = UnityEngineUtility.GetType(m_ObjectType);
            if (objType == null || objType.IsAbstract) { // The fields can't be deserialized if the type doesn't exist or is abstract.
                return null;
            }
            // The field position map allows the deserializer to quickly look up the index within the value arrays.
            var fieldPositionMap = new Dictionary<int, int>();
            if (m_ValueHashes != null) {
                for (int i = 0; i < m_ValueHashes.Length; ++i) {
                    fieldPositionMap.Add(m_ValueHashes[i], i);
                }
            }

            // Recursively deserialize all of the object fields.
            var obj = Activator.CreateInstance(objType);
            DeserializeFields(obj, 0, fieldPositionMap, m_ValuePositions, m_Values, m_UnityObjects, visibility);
            return obj;
        }

        /// <summary>
        /// Recursively deserializes all of the field values within the object.
        /// </summary>
        /// <param name="obj">The object to deserialize.</param>
        /// <param name="hashPrefix">The prefix of the hash from the parent class. This value will prevent collisions with similarly named objects.</param>
        /// <param name="valuePositionMap">A map between the value hash and the position within the positions array.</param>
        /// <param name="valuePositions">The positions of the all of the values.</param>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="unityObjects">A reference to the unity objects array. Used if the type is a UnityEngine.Object.</param>
        /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        /// <returns>The deserialized object.</returns>
        public static object DeserializeFields(object obj, int hashPrefix, Dictionary<int, int> valuePositionMap, int[] valuePositions, byte[] values, UnityEngine.Object[] unityObjects, MemberVisibility visibility)
        {
            // Only the serialized fields need to be deserialized.
            var fields = GetSerializedFields(obj.GetType(), visibility);
            for (int i = 0; i < fields.Length; ++i) {
                var value = Serializer.BytesToValue(fields[i].FieldType, fields[i].Name, valuePositionMap, hashPrefix, values, valuePositions, unityObjects, true, visibility);
                // Unity overrides the != operator, Equals will do a true null check.
                if (value != null && !value.Equals(null)) {
                    fields[i].SetValue(obj, value);
                }
            }
            return obj;
        }

        /// <summary>
        /// Recursively deserializes all of the property values within the object.
        /// </summary>
        /// <param name="obj">The object to deserialize.</param>
        /// <param name="hashPrefix">The prefix of the hash from the parent class. This value will prevent collisions with similarly named objects.</param>
        /// <param name="valuePositionMap">A map between the value hash and the position within the positions array.</param>
        /// <param name="valuePositions">The positions of the all of the values.</param>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="unityObjects">A reference to the unity objects array. Used if the type is a UnityEngine.Object.</param>
        /// /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        /// <returns>The deserialized object.</returns>
        public static object DeserializeProperties(object obj, int hashPrefix, Dictionary<int, int> valuePositionMap, int[] valuePositions, byte[] values, UnityEngine.Object[] unityObjects, MemberVisibility visibility)
        {
            // Only the serialized properties need to be deserialized.
            var properties = GetSerializedProperties(obj.GetType(), visibility);
            for (int i = 0; i < properties.Length; ++i) {
                var value = Serializer.BytesToValue(properties[i].PropertyType, properties[i].Name, valuePositionMap, hashPrefix, values, valuePositions, unityObjects, false, visibility);
                if (value != null) {
                    // Apply the value to the current object.
                    var setMethod = properties[i].GetSetMethod((visibility != MemberVisibility.Public && visibility != MemberVisibility.AllPublic));
                    if (setMethod != null) {
                        setMethod.Invoke(obj, new object[] { value });
                    }
                }
            }
            return obj;
        }

        /// <summary>
        /// Returns an array of all of the fields that can be serialized for the specified type
        /// </summary>
        /// <param name="type">The type to determine all of the fields that can be serialized on.</param>
        /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        /// <returns>An array of all of the fields that can be serialized.</returns>
        public static FieldInfo[] GetSerializedFields(Type type, MemberVisibility visibility)
        {
            // Cache the results for quick repeated lookup. The map will be different depending on if public or non-public fields are used so store two different dictionaries.
            if (s_SerializedFieldsMaps == null) {
                s_SerializedFieldsMaps = new Dictionary<Type, FieldInfo[]>[(int)MemberVisibility.Last];
            }
            var mapIndex = (int)visibility;
            if (s_SerializedFieldsMaps[mapIndex] == null) {
                s_SerializedFieldsMaps[mapIndex] = new Dictionary<Type, FieldInfo[]>();
            }
            FieldInfo[] fields;
            if (!s_SerializedFieldsMaps[mapIndex].TryGetValue(type, out fields)) {
                var fieldList = new List<FieldInfo>();
                var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

                // Recursively add any serialized fields.
                GetSerializedFields(type, ref fieldList, flags, visibility);

                // Cache the result.
                fields = fieldList.ToArray();
                s_SerializedFieldsMaps[mapIndex].Add(type, fields);
            }
            return fields;
        }

        /// <summary>
        /// Recursively searches for any serialized fields of the specified type.
        /// </summary>
        /// <param name="type">The type to determine all of the fields that can be serialized on.</param>
        /// <param name="fieldList">A list of the current serialized fields.</param>
        /// <param name="flags">The BindingFlags to serach for fields on.</param>
        /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        private static void GetSerializedFields(Type type, ref List<FieldInfo> fieldList, BindingFlags flags, MemberVisibility visibility)
        {
            if (type == null) {
                return;
            }

            // Recursively search the base type.
            GetSerializedFields(type.BaseType, ref fieldList, flags, visibility);

            // Get all fields based on the BindingFlag.
            var fields = type.GetFields(flags);
            for (int i = 0; i < fields.Length; ++i) {
                // Don't add the field if:
                // - The field is marked as NonSerialized and the visibility isn't AllPublic. AllPublic will serialize NonSerialized members.
                // - The visibility is public but the field is private/protected without the SerializeField attribute.
                // - The visiblity is snapshot but the field doesn't have the Snapshot attribute.
                // - The field is of type Serialization and doesn't have the ForceSerialized attribute.
                if ((visibility!= MemberVisibility.AllPublic && UnityEngineUtility.HasAttribute(fields[i], typeof(NonSerializedAttribute))) ||
                    (visibility == MemberVisibility.Public && (fields[i].IsPrivate || fields[i].IsFamily) && 
                                                !UnityEngineUtility.HasAttribute(fields[i], typeof(SerializeField))) ||
                    (visibility == MemberVisibility.Snapshot && !UnityEngineUtility.HasAttribute(fields[i], typeof(Snapshot))) ||
                    (!UnityEngineUtility.HasAttribute(fields[i], typeof(ForceSerialized)) && fields[i].FieldType == typeof(Serialization))) {
                    continue;
                }
                // The field can be serialized.
                fieldList.Add(fields[i]);
            }
        }

        /// <summary>
        /// Returns an array of all of the properties that can be serialized for the specified type.
        /// </summary>
        /// <param name="type">The type to determine all of the properties that can be serialized on.</param>
        /// /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        /// <returns>An array of all of the properties that can be serialized.</returns>
        public static PropertyInfo[] GetSerializedProperties(Type type, MemberVisibility visibility)
        {
            // Cache the results for quick repeated lookup. The map will be different depending on if public or non-public properties are used so store two different dictionaries.
            if (s_SerializedPropertiesMaps == null) {
                s_SerializedPropertiesMaps = new Dictionary<Type, PropertyInfo[]>[(int)MemberVisibility.Last];
            }
            var mapIndex = (int)visibility;
            if (s_SerializedPropertiesMaps[mapIndex] == null) {
                s_SerializedPropertiesMaps[mapIndex] = new Dictionary<Type, PropertyInfo[]>();
            }
            PropertyInfo[] properties;
            // If the properties on the specified type haven't been retrieve already then perform a lookup and cache the results.
            if (!s_SerializedPropertiesMaps[mapIndex].TryGetValue(type, out properties)) {
                var flags = BindingFlags.Public | BindingFlags.Instance;
                if (visibility != MemberVisibility.Public && visibility != MemberVisibility.AllPublic) {
                    flags |= BindingFlags.NonPublic;
                }

                var propertyList = new List<PropertyInfo>();
                properties = type.GetProperties(flags);
                for (int i = 0; i < properties.Length; ++i) {
                    // Do not add the property if:
                    // - The visibility isn't Snapshot or AllPublic and the property has the NonSerialized or Snapshot attribute.
                    // - The property is declared in a UnityEngine class.
                    // - The visibility is Snapshot but the property doesn't have the Snapshot attribute.
                    if ((visibility != MemberVisibility.Snapshot && visibility != MemberVisibility.AllPublic && 
                        (UnityEngineUtility.HasAttribute(properties[i], typeof(NonSerialized)) || UnityEngineUtility.HasAttribute(properties[i], typeof(Snapshot)))) ||
                        properties[i].DeclaringType == typeof(MonoBehaviour) ||  properties[i].DeclaringType == typeof(Behaviour) || 
                        properties[i].DeclaringType == typeof(Component) || properties[i].DeclaringType == typeof(UnityEngine.Object) ||
                        typeof(UnityEngine.Events.UnityEventBase).IsAssignableFrom(properties[i].PropertyType) ||
                        (visibility == MemberVisibility.Snapshot && !UnityEngineUtility.HasAttribute(properties[i], typeof(Snapshot)))) {
                        continue;
                    }

                    propertyList.Add(properties[i]);
                }

                // Convert the list to an array and cache the result.
                properties = propertyList.ToArray();
                s_SerializedPropertiesMaps[mapIndex].Add(type, properties);
            }
            return properties;
        }

        /// <summary>
        /// Adds the value to the existing byte value array.
        /// </summary>
        /// <param name="bytes">The bytes to add to the value array.</param>
        /// <param name="hash">The hash of the value field/property. This value will prevent collisions with similarly named objects.</param>
        /// <param name="valueCount">The current index within the value hash and position array.</param>
        /// <param name="valueHashes">The unique hash for the value that is being serialized.</param>
        /// <param name="valuePositions">The positions of the all of the values.</param>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        public static void AddByteValue(ICollection<byte> bytes, int hash, ref int valueCount, ref int[] valueHashes, ref int[] valuePositions, ref List<byte> values)
        {
            valueHashes[valueCount] = hash;
            valuePositions[valueCount] = values.Count;
            if (bytes != null) {
                values.AddRange(bytes);
            }
            valueCount++;
        }

        /// <summary>
        /// Returns the size of the value at the specified index.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <returns>The size of the value.</returns>
        public static int GetValueSize(int index, byte[] values, int[] valuePositions)
        {
            return (index == valuePositions.Length - 1 ? (values.Length - valuePositions[index]) : (valuePositions[index + 1] - valuePositions[index]));
        }

        /// <summary>
        /// Returns the indexes of all of the Unity Objects within the type. This method is recursive and traverses the object similar to UpdateUnityObjectIndexes.
        /// </summary>
        /// <param name="unityObjectIndexes">The list of all Unity Object indexes.</param>
        /// <param name="type">The type to retrieve the indexes of.</param>
        /// <param name="name">The name of the value. This is used to determine if the value exists within the vale position map.</param>
        /// <param name="hashPrefix">The prefix of the hash from the parent class. This value will prevent collisions with similarly named objects.</param>
        /// <param name="valuePositionMap">A map between the value hash and the position within the positions array.</param>
        /// <param name="valueHashes">The unique hash for each value.</param>
        /// <param name="valuePositions">The positions of the all of the values.</param>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="useFields">Should the fields be searched? If false then the properties will be used.</param>
        /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        public static void GetUnityObjectIndexes(ref List<int> unityObjectIndexes, Type type, string name, int hashPrefix, Dictionary<int, int> valuePositionMap, int[] valueHashes, int[] valuePositions, byte[] values, bool useFields, MemberVisibility visibility)
        {
            var hash = hashPrefix + StringHash(type.FullName) + StringHash(name);
            int position;
            // If the hash doesn't exist in the dictionary then that value hasn't been serialized.
            if (!valuePositionMap.TryGetValue(hash, out position)) {
                return;
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(type)) {
                // A Unity Object type was found. Add the index to the list.
                var index = Serializer.BytesToInt(values, valuePositions[position]);
                if (index != -1) {
                    unityObjectIndexes.Add(index);
                }
            } else if (typeof(IList).IsAssignableFrom(type)) {
                // Search the list for any Unity Object elements. This will also search any elements which are within a child class.
                var elementCount = Serializer.BytesToInt(values, valuePositions[position]);
                Type elementType;
                // The base element type is needed for deserialization.
                if (type.IsArray) {
                    elementType = type.GetElementType();
                } else {
                    var baseType = type;
                    while (!baseType.IsGenericType) {
                        baseType = baseType.BaseType;
                    }
                    elementType = baseType.GetGenericArguments()[0];
                }
                // Search each element.
                for (int i = 0; i < elementCount; ++i) {
                    GetUnityObjectIndexes(ref unityObjectIndexes, elementType, name, hash / (i + 2), valuePositionMap, valueHashes, valuePositions, values, useFields, visibility);
                }
            } else if (!Serializer.IsSerializedType(type) && (type.IsClass || (type.IsValueType && !type.IsPrimitive))) {
                if (useFields) {
                    // Search all of the object's fields for a Unity Object.
                    var fields = GetSerializedFields(type, visibility);
                    for (int i = 0; i < fields.Length; ++i) {
                        GetUnityObjectIndexes(ref unityObjectIndexes, fields[i].FieldType, fields[i].Name, hash, valuePositionMap, valueHashes, valuePositions, values, useFields, visibility);
                    }
                } else {
                    // Search all of the object's properties for a Unity Object.
                    var properties = GetSerializedProperties(type, visibility);
                    for (int i = 0; i < properties.Length; ++i) {
                        var getMethod = GetValidGetMethod(properties[i], visibility);
                        if (getMethod != null) {
                            GetUnityObjectIndexes(ref unityObjectIndexes, properties[i].PropertyType, properties[i].Name, hash, valuePositionMap, valueHashes, valuePositions, values, useFields, visibility);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the serialized index of the Unity Objects after the start position. This allows the Unity Object array to be modified while still retaining the correct serialized values.
        /// This method is recursive and traverses the object similar to GetUnityObjectIndexes.
        /// </summary>
        /// <param name="indexDiff">The difference in the Unity Object indexes.</param>
        /// <param name="startPosition">The starting position that the index difference should be applied to.</param>
        /// <param name="type">The type to retrieve the indexes of.</param>
        /// <param name="name">The name of the value. This is used to determine if the value exists within the vale position map.</param>
        /// <param name="hashPrefix">The prefix of the hash from the parent class. This value will prevent collisions with similarly named objects.</param>
        /// <param name="valuePositionMap">A map between the value hash and the position within the positions array.</param>
        /// <param name="valueHashes">The unique hash for each value.</param>
        /// <param name="valuePositions">The positions of the all of the values.</param>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="useFields">Should the fields be searched? If false then the properties will be used.</param>
        /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        public static void UpdateUnityObjectIndexes(int indexDiff, int startPosition, Type type, string name, int hashPrefix, Dictionary<int, int> valuePositionMap, int[] valueHashes,int[] valuePositions, ref byte[] values, bool useFields, MemberVisibility visibility)
        {
            var hash = hashPrefix + StringHash(type.FullName) + StringHash(name);
            int position;
            // If the hash doesn't exist in the dictionary then that value hasn't been serialized. If the hash does exist then the position should be greater than the start position otherwise the 
            // Unity Object isn't affected by the index change.
            if (!valuePositionMap.TryGetValue(hash, out position) || position <= startPosition) {
                return;
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(type)) {
                var index = Serializer.BytesToInt(values, valuePositions[position]);
                if (index != -1) {
                    // Update the index and replace the byte value.
                    var byteValue = Serializer.IntToBytes(index + indexDiff);
                    for (int i = 0; i < byteValue.Length; ++i) {
                        values[valuePositions[position] + i] = byteValue[i];
                    }
                }
            } else if (typeof(IList).IsAssignableFrom(type)) {
                // Search the list for any Unity Object elements. This will also search any elements which are within a child class.
                var elementCount = Serializer.BytesToInt(values, valuePositions[position]);
                Type elementType;
                // The base element type is needed for deserialization.
                if (type.IsArray) {
                    elementType = type.GetElementType();
                } else {
                    var baseType = type;
                    while (!baseType.IsGenericType) {
                        baseType = baseType.BaseType;
                    }
                    elementType = baseType.GetGenericArguments()[0];
                }
                // Search each element.
                for (int i = 0; i < elementCount; ++i) {
                    UpdateUnityObjectIndexes(indexDiff, startPosition, elementType, name, hash / (i + 2), valuePositionMap, valueHashes, valuePositions, ref values, useFields, visibility);
                }
            } else if (!Serializer.IsSerializedType(type) && (type.IsClass || (type.IsValueType && !type.IsPrimitive))) {
                if (useFields) {
                    // Search all of the object's fields for a Unity Object.
                    var fields = GetSerializedFields(type, visibility);
                    for (int i = 0; i < fields.Length; ++i) {
                        UpdateUnityObjectIndexes(indexDiff, startPosition, fields[i].FieldType, fields[i].Name, hash, valuePositionMap, valueHashes, valuePositions, ref values, useFields, visibility);
                    }
                } else {
                    // Search all of the object's properties for a Unity Object.
                    var properties = GetSerializedProperties(type, visibility);
                    for (int i = 0; i < properties.Length; ++i) {
                        var getMethod = GetValidGetMethod(properties[i], visibility);
                        if (getMethod != null) {
                            UpdateUnityObjectIndexes(indexDiff, startPosition, properties[i].PropertyType, properties[i].Name, hash, valuePositionMap, valueHashes, valuePositions, ref values, useFields, visibility);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes the property at the specified index from the serialized values.
        /// </summary>
        /// <param name="index">The index of the property to remove.</param>
        /// <param name="unityObjectIndexes">A list of Unity Objects that also need to be removed. This list is populated ahead of time using GetUnityObjectIndexes.</param>
        /// <param name="serialization">The serialized data.</param>
        /// /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        public static void RemoveProperty(int index, List<int> unityObjectIndexes, Serialization serialization, MemberVisibility visibility)
        {
            // Build the map for deserialization.
            var valuePositionMap = new Dictionary<int, int>(serialization.ValueHashes.Length);
            for (int i = 0; i < serialization.ValueHashes.Length; ++i) {
                valuePositionMap.Add(serialization.ValueHashes[i], i);
            }

            // Get the position of the property from the hash.
            var properties = GetSerializedProperties(UnityEngineUtility.GetType(serialization.ObjectType), visibility);
            var hash = StringHash(properties[index].PropertyType.FullName) + StringHash(properties[index].Name);
            int startPosition;
            if (valuePositionMap.TryGetValue(hash, out startPosition)) {
                // If the property is a class, struct, or list then it could have multiple values associated with it. Remove all of those values.
                int endPosition = 0;
                for (int i = 0; i < properties.Length; ++i) {
                    hash = StringHash(properties[i].PropertyType.FullName) + StringHash(properties[i].Name);
                    int localEndPosition;
                    // The serialized positions aren't in a linear order so do a search for the next property under the object. The next property will be the lowest position after the start position.
                    if (valuePositionMap.TryGetValue(hash, out localEndPosition)) {
                        if (localEndPosition > startPosition && (endPosition == 0 || localEndPosition < endPosition)) {
                            endPosition = localEndPosition;
                        }
                    }
                }

                // The Unity objects which were being referenced by the value no longer need to be referenced.
                if (unityObjectIndexes.Count > 0) {
                    var unityObjects = new List<UnityEngine.Object>(serialization.UnityObjects);
                    for (int i = unityObjectIndexes.Count - 1; i >= 0; --i) {
                        unityObjects.RemoveAt(unityObjectIndexes[i]);
                    }
                    serialization.UnityObjects = unityObjects.ToArray();

                    // Update the indexes of the remaining values.
                    var values = serialization.Values;
                    for (int i = 0; i < properties.Length; ++i) {
                        UpdateUnityObjectIndexes(-unityObjectIndexes.Count, startPosition, properties[i].PropertyType, properties[i].Name, 0, valuePositionMap, serialization.ValueHashes, serialization.ValuePositions, ref values, false, visibility);
                    }
                }

                // For every value that needs to be modified first convert it into a list so the array is mutable.
                // The changes will then be made on the list and saved back out to an array.
                var valueNameList = new List<int>(serialization.ValueHashes);
                var valueList = new List<byte>(serialization.Values);
                var valuePositionList = new List<int>(serialization.ValuePositions);

                // If endPosition is greater than startPosition then the next property exists in the value position map. Remove all of the values in between the start
                // and end position. If end position is less then or equal to start position then there aren't any properties after the soon to be removed property
                // so all of the values can be removed.
                var removeCount = (endPosition > startPosition ? (endPosition - startPosition) : (serialization.ValueHashes.Length - startPosition));
                for (int i = removeCount - 1; i >= 0; --i) {
                    // Remove from the name hash, value, and value position lists.
                    var removeIndex = startPosition + i;
                    valueNameList.RemoveAt(removeIndex);
                    var size = Serialization.GetValueSize(removeIndex, serialization.Values, serialization.ValuePositions);
                    valueList.RemoveRange(serialization.ValuePositions[removeIndex], size);
                    for (int j = removeIndex + 1; j < valuePositionList.Count; ++j) {
                        valuePositionList[j] -= size;
                    }
                    valuePositionList.RemoveAt(removeIndex);
                }

                // Save the lists back out as arrays.
                serialization.ValueHashes = valueNameList.ToArray();
                serialization.Values = valueList.ToArray();
                serialization.ValuePositions = valuePositionList.ToArray();
            }
        }

        /// <summary>
        /// Adds the specified property to the Serialization.
        /// </summary>
        /// <param name="property">The property to add.</param>
        /// <param name="value">The value of the property to add.</param>
        /// <param name="unityObjectIndexes">Any Unity Objects to add.</param>
        /// <param name="serialization">The serialized data.</param>
        /// /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        public static void AddProperty(PropertyInfo property, object value, List<int> unityObjectIndexes, Serialization serialization, MemberVisibility visibility)
        {
            var localValueCount = Serialization.GetValueCount(property.PropertyType, value, false, visibility);

            var localValueNameHashes = new int[localValueCount];
            var localValuePositions = new int[localValueCount];
            var localValues = new List<byte>();
            // Copy the unity objecst to the array so the index is correct when the value is serialized.
            var localUnityObjects = new UnityEngine.Object[(serialization.UnityObjects != null ? serialization.UnityObjects.Length : 0)];
            if (serialization.UnityObjects != null) {
                Array.Copy(serialization.UnityObjects, localUnityObjects, serialization.UnityObjects.Length);
            }

            localValueCount = 0;
            Serializer.SerializeValue(property.PropertyType, value, ref localValueNameHashes, ref localValues, ref localValuePositions, ref localUnityObjects, ref localValueCount, 0, property.Name, false, visibility);

            // The value position is based off of the existing values.
            if (serialization.ValuePositions.Length > 0) {
                for (int j = 0; j < localValuePositions.Length; ++j) {
                    localValuePositions[j] += serialization.Values.Length;
                }
            }

            // Add the new values to the end of the existing values.
            var valueNameHashes = new List<int>(serialization.ValueHashes);
            var values = new List<byte>(serialization.Values);
            var valuePositions = new List<int>(serialization.ValuePositions);

            valueNameHashes.AddRange(localValueNameHashes);
            valuePositions.AddRange(localValuePositions);
            values.AddRange(localValues);

            serialization.ValueHashes = valueNameHashes.ToArray();
            serialization.ValuePositions = valuePositions.ToArray();
            serialization.Values = values.ToArray();
            // The Unity Objects array already contains the previous list of objects so the new values do not need to be added to the end of the list.
            serialization.UnityObjects = localUnityObjects;
        }

        /// <summary>
        /// Converts a string value into a hash.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>A hash of the string value.</returns>
        public static int StringHash(string value)
        {
            // Cache the results for quick repeated lookup.
            if (s_StringHashMap == null) {
                s_StringHashMap = new Dictionary<string, int>();
            }

            int finalHash;
            if (!s_StringHashMap.TryGetValue(value, out finalHash)) {
                if (String.IsNullOrEmpty(value)) {
                    finalHash = 0;
                } else {
                    finalHash = 23;
                    var length = value.Length;
                    for (int i = 0; i < length; ++i) {
                        finalHash = finalHash * 31 + value[i];
                    }
                    return finalHash;
                }
                s_StringHashMap.Add(value, finalHash);
            }

            return finalHash;
        }

        /// <summary>
        /// Serialize the object to a Serialization object.
        /// </summary>
        public static Serialization Serialize<T>(T obj)
        {
            var serializedValue = new Serialization();
            serializedValue.Serialize(obj, true, MemberVisibility.Public);
            return serializedValue;
        }

        /// <summary>
        /// Serialize the list to a Serialization array.
        /// </summary>
        public static Serialization[] Serialize<T>(List<T> list)
        {
            var serializedValues = new List<Serialization>();
            for (int i = list.Count - 1; i >= 0; --i) {
                if (list[i] == null) {
                    list.RemoveAt(i);
                    continue;
                }
                var serializedValue = new Serialization();
                serializedValue.Serialize(list[i], true, MemberVisibility.Public);
                serializedValues.Insert(0, serializedValue);
            }

            return serializedValues.ToArray();
        }
    }

    /// <summary>
    /// The Serializer allows values to be serialized to/from a binary array.
    /// </summary>
    public static class Serializer
    {
        private static byte[] s_BigEndianFourByteArray;
        private static byte[] s_BigEndianEightByteArray;

        private static byte[] BigEndianFourByteArray { get { if (s_BigEndianFourByteArray == null) { s_BigEndianFourByteArray = new byte[4]; } return s_BigEndianFourByteArray; } set { s_BigEndianFourByteArray = value; } }
        private static byte[] BigEndianEightByteArray { get { if (s_BigEndianEightByteArray == null) { s_BigEndianEightByteArray = new byte[8]; } return s_BigEndianEightByteArray; } set { s_BigEndianEightByteArray = value; } }

        /// <summary>
        /// Returns true if the type is a serialized type.
        /// </summary>
        /// <param name="type">The type to compare against.</param>
        /// <returns>True if the type is a serialized type.</returns>
        public static bool IsSerializedType(Type type)
        {
            return type == typeof(int) || type.IsEnum || type == typeof(uint) || type == typeof(float) || type == typeof(double) || type == typeof(long) || type == typeof(bool) ||
                   type == typeof(string) || type == typeof(byte) || type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4) || type == typeof(Quaternion) || 
                   type == typeof(Color) || type == typeof(Rect) || type == typeof(Matrix4x4) || type == typeof(AnimationCurve) || type == typeof(LayerMask) || 
                   typeof(UnityEngine.Object).IsAssignableFrom(type);
        }

        /// <summary>
        /// Serializes the value based on the type.
        /// </summary>
        /// <param name="type">The type of value to serialize.</param>
        /// <param name="value">The value of the object.</param>
        /// <param name="unityObjects">A reference to the unity objects array. Used if the type is a UnityEngine.Object.</param>
        /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        /// <returns>The serialized byte array. Can be null.</returns>
        public static void SerializeValue(Type type, object value, ref int[] valueHashes, ref List<byte> values, ref int[] valuePositions, ref UnityEngine.Object[] unityObjects, ref int valueCount, int hashPrefix, string name, bool serializeFields, MemberVisibility visibility)
        {
            var hash = hashPrefix + Serialization.StringHash(type.FullName) + Serialization.StringHash(name);
            if (typeof(IList).IsAssignableFrom(type)) {
                Type elementType;
                if (type.IsArray) {
                    elementType = type.GetElementType();
                } else {
                    var baseType = type;
                    while (!baseType.IsGenericType) {
                        baseType = baseType.BaseType;
                    }
                    elementType = baseType.GetGenericArguments()[0];
                }
                var elements = value as IList;
                if (elements == null) {
                    Serialization.AddByteValue(IntToBytes(0), hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
                } else {
                    Serialization.AddByteValue(IntToBytes(elements.Count), hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
                    if (elements.Count > 0) {
                        for (int i = 0; i < elements.Count; ++i) {
                            if (elements[i] == null) {
                                Serialization.AddByteValue(IntToBytes(-1), hash / (i + 2), ref valueCount, ref valueHashes, ref valuePositions, ref values);// -1 is null.
                            } else {
                                // Divide by the element index to prevent field hash collisions with other array values. For example, without this division the following would cause a collision:
                                // ArrayName0/ArrayElement0
                                // ArrayName0/ArrayElement1 <-- collision
                                // ArrayName1/ArrayElement0 <-- collision
                                // ArrayName1/ArrayElement1
                                SerializeValue(elementType, elements[i], ref valueHashes, ref values, ref valuePositions, ref unityObjects, ref valueCount, hash / (i + 2), name, serializeFields, visibility);
                            }
                        }
                    }
                }
            } else if (type == typeof(int) || type.IsEnum) {
                Serialization.AddByteValue(IntToBytes((int)value), hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
            } else if (type == typeof(uint)) {
                Serialization.AddByteValue(UIntToBytes((uint)value), hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
            } else if (type == typeof(float)) {
                Serialization.AddByteValue(FloatToBytes((float)value), hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
            } else if (type == typeof(double)) {
                Serialization.AddByteValue(DoubleToBytes((double)value), hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
            } else if (type == typeof(long)) {
                Serialization.AddByteValue(LongToBytes((long)value), hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
            } else if (type == typeof(bool)) {
                Serialization.AddByteValue(BoolToBytes((bool)value), hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
            } else if (type == typeof(string)) {
                Serialization.AddByteValue(StringToBytes((string)value), hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
            } else if (type == typeof(byte)) {
                Serialization.AddByteValue(ByteToBytes((byte)value), hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
            } else if (type == typeof(Vector2)) {
                Serialization.AddByteValue(Vector2ToBytes((Vector2)value), hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
            } else if (type == typeof(Vector3)) {
                Serialization.AddByteValue(Vector3ToBytes((Vector3)value), hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
            } else if (type == typeof(Vector4)) {
                Serialization.AddByteValue(Vector4ToBytes((Vector4)value), hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
            } else if (type == typeof(Quaternion)) {
                Serialization.AddByteValue(QuaternionToBytes((Quaternion)value), hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
            } else if (type == typeof(Color)) {
                Serialization.AddByteValue(ColorToBytes((Color)value), hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
            } else if (type == typeof(Rect)) {
                Serialization.AddByteValue(RectToBytes((Rect)value), hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
            } else if (type == typeof(Matrix4x4)) {
                Serialization.AddByteValue(Matrix4x4ToBytes((Matrix4x4)value), hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
            } else if (type == typeof(AnimationCurve)) {
                Serialization.AddByteValue(AnimationCurveToBytes((AnimationCurve)value), hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
            } else if(type == typeof(LayerMask)) {
                Serialization.AddByteValue(LayerMaskToBytes((LayerMask)value), hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
            } else if (typeof(UnityEngine.Object).IsAssignableFrom(type)) {
                Serialization.AddByteValue(UnityObjectToBytes((UnityEngine.Object)value, ref unityObjects), hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
            } else if (type.IsClass || (type.IsValueType && !type.IsPrimitive)) {
                Serialization.AddByteValue(null, hash, ref valueCount, ref valueHashes, ref valuePositions, ref values);
                if (serializeFields) {
                    Serialization.SerializeFields(value, hash, ref valueCount, ref valueHashes, ref valuePositions, ref values, ref unityObjects, visibility);
                } else { // Serialize Properties. 
                    Serialization.SerializeProperties(value, hash, ref valueCount, ref valueHashes, ref valuePositions, ref values, ref unityObjects, visibility);
                }
            }
        }

        /// <summary>
        /// Deserialize the byte array into the object of the specified type.
        /// </summary>
        /// <param name="type">The type of object to deserialize.</param>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="valuePosition">The position of the value.</param>
        /// <param name="valueSize">The size of the value.</param>
        /// <param name="unityObjects">A reference to the unity objects array. Used if the type is a UnityEngine.Object.</param>
        /// <param name="useFields">Should the fields be searched? If false then the properties will be used.</param>
        /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        /// <returns>The deserialized object. Can be null.</returns>
        public static object BytesToValue(Type type, string name, Dictionary<int, int> valuePositionMap, int hashPrefix, byte[] values, int[] valuePositions, UnityEngine.Object[] unityObjects, bool useFields, MemberVisibility visibility)
        {
            var hash = hashPrefix + Serialization.StringHash(type.FullName) + Serialization.StringHash(name);
            int position;
            // If the hash doesn't exist in the dictionary then that value hasn't been serialized.
            if (!valuePositionMap.TryGetValue(hash, out position)) {
                return null;
            }

            if (typeof(IList).IsAssignableFrom(type)) {
                var elementCount = BytesToInt(values, valuePositions[position]);
                object value = null;
                // Arrays are handled differently from lists.
                if (type.IsArray) {
                    var elementType = type.GetElementType();
                    var objectArray = Array.CreateInstance(elementType, elementCount);
                    for (int i = 0; i < elementCount; ++i) {
                        var objectValue = BytesToValue(elementType, name, valuePositionMap, hash / (i + 2), values, valuePositions, unityObjects, useFields, visibility);
                        objectArray.SetValue(objectValue, i);
                    }
                    value = objectArray;
                } else {
                    var baseType = type;
                    while (!baseType.IsGenericType) {
                        baseType = baseType.BaseType;
                    }
                    var elementType = baseType.GetGenericArguments()[0];
                    IList objectList;
                    if (type.IsGenericType) {
                        objectList = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType)) as IList;
                    } else {
                        objectList = Activator.CreateInstance(type) as IList;
                    }
                    for (int i = 0; i < elementCount; ++i) {
                        var objectValue = BytesToValue(elementType, name, valuePositionMap, hash / (i + 2), values, valuePositions, unityObjects, useFields, visibility);
                        objectList.Add(objectValue);
                    }
                    value = objectList;
                }
                return value;
            }
            if (type == typeof(int)) {
                return BytesToInt(values, valuePositions[position]);
            }
            if (type.IsEnum) {
                var intValue = BytesToInt(values, valuePositions[position]);
                return Enum.ToObject(type, intValue);
            }
            if (type == typeof(uint)) {
                return BytesToUInt(values, valuePositions[position]);
            }
            if (type == typeof(float)) {
                return BytesToFloat(values, valuePositions[position]);
            }
            if (type == typeof(double)) {
                return BytesToDouble(values, valuePositions[position]);
            }
            if (type == typeof(long)) {
                return BytesToLong(values, valuePositions[position]);
            }
            if (type == typeof(bool)) {
                return BytesToBool(values, valuePositions[position]);
            }
            if (type == typeof(string)) {
                return BytesToString(values, valuePositions[position], Serialization.GetValueSize(position, values, valuePositions));
            }
            if (type == typeof(byte)) {
                return BytesToByte(values, valuePositions[position]);
            }
            if (type == typeof(Vector2)) {
                return BytesToVector2(values, valuePositions[position]);
            }
            if (type == typeof(Vector3)) {
                return BytesToVector3(values, valuePositions[position]);
            }
            if (type == typeof(Vector4)) {
                return BytesToVector4(values, valuePositions[position]);
            }
            if (type == typeof(Quaternion)) {
                return BytesToQuaternion(values, valuePositions[position]);
            }
            if (type == typeof(Color)) {
                return BytesToColor(values, valuePositions[position]);
            }
            if (type == typeof(Rect)) {
                return BytesToRect(values, valuePositions[position]);
            }
            if (type == typeof(Matrix4x4)) {
                return BytesToMatrix4x4(values, valuePositions[position]);
            }
            if (type == typeof(AnimationCurve)) {
                return BytesToAnimationCurve(values, valuePositions[position]);
            }
            if (type == typeof(LayerMask)) {
                return BytesToLayerMask(values, valuePositions[position]);
            }
            if (typeof(UnityEngine.Object).IsAssignableFrom(type)) {
                return BytesToUnityObject(values, valuePositions[position], unityObjects);
            }
            if (type.IsClass || (type.IsValueType && !type.IsPrimitive)) {
                var value = Activator.CreateInstance(type, true);
                if (useFields) {
                    return Serialization.DeserializeFields(value, hash, valuePositionMap, valuePositions, values, unityObjects, visibility);
                } else {
                    return Serialization.DeserializeProperties(value, hash, valuePositionMap, valuePositions, values, unityObjects, visibility);
                }
            }
            return null;
        }

        /// <summary>
        /// Converts an int into a binary array.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted binary array.</returns>
        public static byte[] IntToBytes(int value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Converts the byte array value to an int value.
        /// </summary>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="valuePosition">The position of the value.</param>
        /// <returns>The converted int value.</returns>
        public static int BytesToInt(byte[] values, int valuePosition)
        {
            if (!BitConverter.IsLittleEndian) {
                Array.Copy(values, valuePosition, BigEndianFourByteArray, 0, 4);
                Array.Reverse(BigEndianFourByteArray);
                return BitConverter.ToInt32(BigEndianFourByteArray, 0);
            }
            return BitConverter.ToInt32(values, valuePosition);
        }

        /// <summary>
        /// Converts an unsigned int into a binary array.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted binary array.</returns>
        public static byte[] UIntToBytes(uint value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Converts the byte array value to an unsigned int value.
        /// </summary>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="valuePosition">The position of the value.</param>
        public static uint BytesToUInt(byte[] values, int valuePosition)
        {
            if (!BitConverter.IsLittleEndian) {
                Array.Copy(values, valuePosition, BigEndianFourByteArray, 0, 4);
                Array.Reverse(BigEndianFourByteArray);
                return BitConverter.ToUInt32(BigEndianFourByteArray, 0);
            }
            return BitConverter.ToUInt32(values, valuePosition);
        }

        /// <summary>
        /// Converts a float into a binary array.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted binary array.</returns>
        public static byte[] FloatToBytes(float value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Converts the byte array value to a float value.
        /// </summary>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="valuePosition">The position of the value.</param>
        /// <returns>The converted float value.</returns>
        public static float BytesToFloat(byte[] values, int valuePosition)
        {
            if (!BitConverter.IsLittleEndian) {
                Array.Copy(values, valuePosition, BigEndianFourByteArray, 0, 4);
                Array.Reverse(BigEndianFourByteArray);
                return BitConverter.ToSingle(BigEndianFourByteArray, 0);
            }
            return BitConverter.ToSingle(values, valuePosition);
        }

        /// <summary>
        /// Converts a double into a binary array.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted binary array.</returns>
        public static byte[] DoubleToBytes(double value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Converts the byte array value to a double value.
        /// </summary>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="valuePosition">The position of the value.</param>
        public static double BytesToDouble(byte[] values, int valuePosition)
        {
            if (!BitConverter.IsLittleEndian) {
                Array.Copy(values, valuePosition, BigEndianEightByteArray, 0, 8);
                Array.Reverse(BigEndianEightByteArray);
                return BitConverter.ToDouble(BigEndianEightByteArray, 0);
            }
            return BitConverter.ToDouble(values, valuePosition);
        }

        /// <summary>
        /// Converts a long into a binary array.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted binary array.</returns>
        public static byte[] LongToBytes(long value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Converts the byte array value to a long value.
        /// </summary>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="valuePosition">The position of the value.</param>
        public static long BytesToLong(byte[] values, int valuePosition)
        {
            if (!BitConverter.IsLittleEndian) {
                Array.Copy(values, valuePosition, BigEndianEightByteArray, 0, 8);
                Array.Reverse(BigEndianEightByteArray);
                return BitConverter.ToInt64(BigEndianEightByteArray, 0);
            }
            return BitConverter.ToInt64(values, valuePosition);
        }

        /// <summary>
        /// Converts a bool into a binary array.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted binary array.</returns>
        public static byte[] BoolToBytes(bool value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Converts the byte array value to a bool value.
        /// </summary>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="valuePosition">The position of the value.</param>
        /// <returns>The converted bool value.</returns>
        public static bool BytesToBool(byte[] values, int valuePosition)
        {
            return BitConverter.ToBoolean(values, valuePosition);
        }

        /// <summary>
        /// Converts a string into a binary array.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted binary array.</returns>
        public static byte[] StringToBytes(string str)
        {
            if (str == null) {
                str = string.Empty;
            }
            return Encoding.UTF8.GetBytes(str);
        }

        /// <summary>
        /// Converts a byte into a binary array.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted binary array.</returns>
        public static byte[] ByteToBytes(byte value)
        {
            var result = new byte[1];
            result[0] = value;
            return result;
        }

        /// <summary>
        /// Converts the byte array value to a byte value.
        /// </summary>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="valuePosition">The position of the value.</param>
        public static byte BytesToByte(byte[] values, int valuePosition)
        {
            return values[valuePosition];
        }

        /// <summary>
        /// Converts the byte array value to a string value.
        /// </summary>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="valuePosition">The position of the value.</param>
        /// <param name="valueSize">The size of the value.</param>
        /// <returns>The converted string value.</returns>
        public static string BytesToString(byte[] values, int valuePosition, int valueSize)
        {
            return Encoding.UTF8.GetString(values, valuePosition, valueSize);
        }

        /// <summary>
        /// Converts a Vector2 into a binary array.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted binary array.</returns>
        public static ICollection<byte> Vector2ToBytes(Vector2 vector2)
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(vector2.x));
            bytes.AddRange(BitConverter.GetBytes(vector2.y));
            return bytes;
        }

        /// <summary>
        /// Converts the byte array value to a Vector2 value.
        /// </summary>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="valuePosition">The position of the value.</param>
        /// <returns>The converted Vector2 value.</returns>
        public static Vector2 BytesToVector2(byte[] values, int valuePosition)
        {
            var value = Vector2.zero;
            value.x = BytesToFloat(values, valuePosition);
            value.y = BytesToFloat(values, valuePosition + 4);
            return value;
        }

        /// <summary>
        /// Converts a Vector3 into a binary array.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted binary array.</returns>
        public static ICollection<byte> Vector3ToBytes(Vector3 vector3)
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(vector3.x));
            bytes.AddRange(BitConverter.GetBytes(vector3.y));
            bytes.AddRange(BitConverter.GetBytes(vector3.z));
            return bytes;
        }

        /// <summary>
        /// Converts the byte array value to a Vector3 value.
        /// </summary>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="valuePosition">The position of the value.</param>
        /// <returns>The converted Vector3 value.</returns>
        public static Vector3 BytesToVector3(byte[] values, int valuePosition)
        {
            var value = Vector3.zero;
            value.x = BytesToFloat(values, valuePosition);
            value.y = BytesToFloat(values, valuePosition + 4);
            value.z = BytesToFloat(values, valuePosition + 8);
            return value;
        }

        /// <summary>
        /// Converts a Vector4 into a binary array.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted binary array.</returns>
        public static ICollection<byte> Vector4ToBytes(Vector4 vector4)
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(vector4.x));
            bytes.AddRange(BitConverter.GetBytes(vector4.y));
            bytes.AddRange(BitConverter.GetBytes(vector4.z));
            bytes.AddRange(BitConverter.GetBytes(vector4.w));
            return bytes;
        }

        /// <summary>
        /// Converts the byte array value to a Vector4 value.
        /// </summary>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="valuePosition">The position of the value.</param>
        /// <returns>The converted Vector4 value.</returns>
        public static Vector4 BytesToVector4(byte[] values, int valuePosition)
        {
            var value = Vector4.zero;
            value.x = BytesToFloat(values, valuePosition);
            value.y = BytesToFloat(values, valuePosition + 4);
            value.z = BytesToFloat(values, valuePosition + 8);
            value.w = BytesToFloat(values, valuePosition + 12);
            return value;
        }

        /// <summary>
        /// Converts a Quaternion into a binary array.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted binary array.</returns>
        public static ICollection<byte> QuaternionToBytes(Quaternion quaternion)
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(quaternion.x));
            bytes.AddRange(BitConverter.GetBytes(quaternion.y));
            bytes.AddRange(BitConverter.GetBytes(quaternion.z));
            bytes.AddRange(BitConverter.GetBytes(quaternion.w));
            return bytes;
        }

        /// <summary>
        /// Converts the byte array value to a Quaternion value.
        /// </summary>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="valuePosition">The position of the value.</param>
        /// <returns>The converted Quaternion value.</returns>
        public static Quaternion BytesToQuaternion(byte[] values, int valuePosition)
        {
            var value = Quaternion.identity;
            value.x = BytesToFloat(values, valuePosition);
            value.y = BytesToFloat(values, valuePosition + 4);
            value.z = BytesToFloat(values, valuePosition + 8);
            value.w = BytesToFloat(values, valuePosition + 12);
            return value;
        }

        /// <summary>
        /// Converts a Color into a binary array.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted binary array.</returns>
        public static ICollection<byte> ColorToBytes(Color color)
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(color.r));
            bytes.AddRange(BitConverter.GetBytes(color.g));
            bytes.AddRange(BitConverter.GetBytes(color.b));
            bytes.AddRange(BitConverter.GetBytes(color.a));
            return bytes;
        }

        /// <summary>
        /// Converts the byte array value to a Color value.
        /// </summary>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="valuePosition">The position of the value.</param>
        public static Color BytesToColor(byte[] values, int valuePosition)
        {
            var color = Color.black;
            color.r = BytesToFloat(values, valuePosition);
            color.g = BytesToFloat(values, valuePosition + 4);
            color.b = BytesToFloat(values, valuePosition + 8);
            color.a = BytesToFloat(values, valuePosition + 12);
            return color;
        }

        /// <summary>
        /// Converts a Rect into a binary array.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted binary array.</returns>
        public static ICollection<byte> RectToBytes(Rect rect)
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(rect.x));
            bytes.AddRange(BitConverter.GetBytes(rect.y));
            bytes.AddRange(BitConverter.GetBytes(rect.width));
            bytes.AddRange(BitConverter.GetBytes(rect.height));
            return bytes;
        }

        /// <summary>
        /// Converts the byte array value to a Rect value.
        /// </summary>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="valuePosition">The position of the value.</param>
        public static Rect BytesToRect(byte[] values, int valuePosition)
        {
            var rect = new Rect();
            rect.x = BytesToFloat(values, valuePosition);
            rect.y = BytesToFloat(values, valuePosition + 4);
            rect.width = BytesToFloat(values, valuePosition + 8);
            rect.height = BytesToFloat(values, valuePosition + 12);
            return rect;
        }

        /// <summary>
        /// Converts a Matrix4x4 into a binary array.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted binary array.</returns>
        public static ICollection<byte> Matrix4x4ToBytes(Matrix4x4 matrix4x4)
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(matrix4x4.m00));
            bytes.AddRange(BitConverter.GetBytes(matrix4x4.m01));
            bytes.AddRange(BitConverter.GetBytes(matrix4x4.m02));
            bytes.AddRange(BitConverter.GetBytes(matrix4x4.m03));
            bytes.AddRange(BitConverter.GetBytes(matrix4x4.m10));
            bytes.AddRange(BitConverter.GetBytes(matrix4x4.m11));
            bytes.AddRange(BitConverter.GetBytes(matrix4x4.m12));
            bytes.AddRange(BitConverter.GetBytes(matrix4x4.m13));
            bytes.AddRange(BitConverter.GetBytes(matrix4x4.m20));
            bytes.AddRange(BitConverter.GetBytes(matrix4x4.m21));
            bytes.AddRange(BitConverter.GetBytes(matrix4x4.m22));
            bytes.AddRange(BitConverter.GetBytes(matrix4x4.m23));
            bytes.AddRange(BitConverter.GetBytes(matrix4x4.m30));
            bytes.AddRange(BitConverter.GetBytes(matrix4x4.m31));
            bytes.AddRange(BitConverter.GetBytes(matrix4x4.m32));
            bytes.AddRange(BitConverter.GetBytes(matrix4x4.m33));
            return bytes;
        }

        /// <summary>
        /// Converts the byte array value to a Matrix value.
        /// </summary>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="valuePosition">The position of the value.</param>
        /// <returns>The converted Quaternion value.</returns>
        public static Matrix4x4 BytesToMatrix4x4(byte[] values, int valuePosition)
        {
            var matrix4x4 = Matrix4x4.identity;
            matrix4x4.m00 = BytesToFloat(values, valuePosition);
            matrix4x4.m01 = BytesToFloat(values, valuePosition + 4);
            matrix4x4.m02 = BytesToFloat(values, valuePosition + 8);
            matrix4x4.m03 = BytesToFloat(values, valuePosition + 12);
            matrix4x4.m10 = BytesToFloat(values, valuePosition + 16);
            matrix4x4.m11 = BytesToFloat(values, valuePosition + 20);
            matrix4x4.m12 = BytesToFloat(values, valuePosition + 24);
            matrix4x4.m13 = BytesToFloat(values, valuePosition + 28);
            matrix4x4.m20 = BytesToFloat(values, valuePosition + 32);
            matrix4x4.m21 = BytesToFloat(values, valuePosition + 36);
            matrix4x4.m22 = BytesToFloat(values, valuePosition + 40);
            matrix4x4.m23 = BytesToFloat(values, valuePosition + 44);
            matrix4x4.m30 = BytesToFloat(values, valuePosition + 48);
            matrix4x4.m31 = BytesToFloat(values, valuePosition + 52);
            matrix4x4.m32 = BytesToFloat(values, valuePosition + 56);
            matrix4x4.m33 = BytesToFloat(values, valuePosition + 60);
            return matrix4x4;
        }

        /// <summary>
        /// Converts an AnimationCurve into a binary array.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted binary array.</returns>
        public static ICollection<byte> AnimationCurveToBytes(AnimationCurve animationCurve)
        {
            if (animationCurve == null) {
                return null;
            }

            var bytes = new List<byte>();
            var keys = animationCurve.keys;
            if (keys != null) {
                bytes.AddRange(BitConverter.GetBytes(keys.Length));
                for (int i = 0; i < keys.Length; ++i) {
                    bytes.AddRange(BitConverter.GetBytes(keys[i].time));
                    bytes.AddRange(BitConverter.GetBytes(keys[i].value));
                    bytes.AddRange(BitConverter.GetBytes(keys[i].inTangent));
                    bytes.AddRange(BitConverter.GetBytes(keys[i].outTangent));
                }
            } else {
                bytes.AddRange(BitConverter.GetBytes(0));
            }
            bytes.AddRange(BitConverter.GetBytes((int)animationCurve.preWrapMode));
            bytes.AddRange(BitConverter.GetBytes((int)animationCurve.postWrapMode));
            return bytes;
        }

        /// <summary>
        /// Converts the byte array value to an AnimationCurve value.
        /// </summary>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="valuePosition">The position of the value.</param>
        public static AnimationCurve BytesToAnimationCurve(byte[] values, int valuePosition)
        {
            var animationCurve = new AnimationCurve();
            var keyCount = BytesToInt(values, valuePosition);
            for (int i = 0; i < keyCount; ++i) {
                var keyframe = new Keyframe();
                keyframe.time = BytesToFloat(values, valuePosition + 4);
                keyframe.value = BytesToFloat(values, valuePosition + 8);
                keyframe.inTangent = BytesToFloat(values, valuePosition + 12);
                keyframe.outTangent = BytesToFloat(values, valuePosition + 16);
                animationCurve.AddKey(keyframe);
                valuePosition += 16;
            }
            animationCurve.preWrapMode = (WrapMode)BytesToInt(values, valuePosition + 4);
            animationCurve.postWrapMode = (WrapMode)BytesToInt(values, valuePosition + 8);
            return animationCurve;
        }

        /// <summary>
        /// Converts a LayerMask into a binary array.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted binary array.</returns>
        public static ICollection<byte> LayerMaskToBytes(LayerMask value)
        {
            return IntToBytes(value.value);
        }

        /// <summary>
        /// Converts the byte array value to a LayerMask value.
        /// </summary>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="valuePosition">The position of the value.</param>
        public static LayerMask BytesToLayerMask(byte[] values, int valuePosition)
        {
            var layerMask = new LayerMask();
            layerMask.value = BytesToInt(values, valuePosition);
            return layerMask;
        }

        /// <summary>
        /// Converts a UnityEngine.Object into a binary array.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="unityObjects">A reference to the existing Unity Objects.</param>
        /// <returns>The converted binary array.</returns>
        public static ICollection<byte> UnityObjectToBytes(UnityEngine.Object unityObject, ref UnityEngine.Object[] unityObjects)
        {
            if (unityObject == null) {
                return IntToBytes(-1);
            }

            // Store the object reference in a separate array. Unity must serialize the object.
            if (unityObjects == null) {
                unityObjects = new UnityEngine.Object[1];
            } else {
                Array.Resize<UnityEngine.Object>(ref unityObjects, unityObjects.Length + 1);
            }
            unityObjects[unityObjects.Length - 1] = unityObject;
            return IntToBytes(unityObjects.Length - 1);
        }

        /// <summary>
        /// Converts the byte array value to a UnityEngine.Object value.
        /// </summary>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="valuePosition">The position of the value.</param>
        /// <param name="unityObjects">An array of the current Unity Objects.</param>
        /// <returns>The converted UnityEngine.Object value.</returns>
        public static UnityEngine.Object BytesToUnityObject(byte[] values, int valuePosition, UnityEngine.Object[] unityObjects)
        {
            var index = BytesToInt(values, valuePosition);
            if (unityObjects != null && index >= 0 && index < unityObjects.Length) {
                return unityObjects[index];
            }
            return null;
        }
    }
    
    /// <summary>
    /// Allows values to be deserialized based on a generic type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    public static class Serializer<T> where T : Type
    {
        /// <summary>
        /// Deserialize the byte array into the object of the specified type.
        /// </summary>
        /// <param name="values">An array of all of the byte serialized values for the object.</param>
        /// <param name="valuePosition">The position of the value.</param>
        /// <param name="valueSize">The size of the value.</param>
        /// <param name="unityObjects">A reference to the unity objects array. Used if the type is a UnityEngine.Object.</param>
        /// <param name="useFields">Should the fields be searched? If false then the properties will be used.</param>
        /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        /// <returns>The deserialized object. Can be null.</returns>
        public static T BytesToValue(string name, Dictionary<int, int> valuePositionMap, int hashPrefix, byte[] values, int[] valuePositions, UnityEngine.Object[] unityObjects, bool useFields, MemberVisibility visibility)
        {
            var type = typeof(T);
            var hash = hashPrefix + Serialization.StringHash(type.FullName) + Serialization.StringHash(name);
            int position;
            // If the hash doesn't exist in the dictionary then that value hasn't been serialized.
            if (!valuePositionMap.TryGetValue(hash, out position)) {
                return null;
            }

            if (type == typeof(int)) {
                return Serializer.BytesToInt(values, valuePositions[position]) as T;
            }
            if (type.IsEnum) {
                var intValue = Serializer.BytesToInt(values, valuePositions[position]);
                return Enum.ToObject(type, intValue) as T;
            }
            if (type == typeof(uint)) {
                return Serializer.BytesToUInt(values, valuePositions[position]) as T;
            }
            if (type == typeof(float)) {
                return Serializer.BytesToFloat(values, valuePositions[position]) as T;
            }
            if (type == typeof(double)) {
                return Serializer.BytesToDouble(values, valuePositions[position]) as T;
            }
            if (type == typeof(long)) {
                return Serializer.BytesToLong(values, valuePositions[position]) as T;
            }
            if (type == typeof(bool)) {
                return Serializer.BytesToBool(values, valuePositions[position]) as T;
            }
            if (type == typeof(string)) {
                return Serializer.BytesToString(values, valuePositions[position], Serialization.GetValueSize(position, values, valuePositions)) as T;
            }
            if (type == typeof(byte)) {
                return Serializer.BytesToByte(values, valuePositions[position]) as T;
            }
            if (type == typeof(Vector2)) {
                return Serializer.BytesToVector2(values, valuePositions[position]) as T;
            }
            if (type == typeof(Vector3)) {
                return Serializer.BytesToVector3(values, valuePositions[position]) as T;
            }
            if (type == typeof(Vector4)) {
                return Serializer.BytesToVector4(values, valuePositions[position]) as T;
            }
            if (type == typeof(Quaternion)) {
                return Serializer.BytesToQuaternion(values, valuePositions[position]) as T;
            }
            if (type == typeof(Color)) {
                return Serializer.BytesToColor(values, valuePositions[position]) as T;
            }
            if (type == typeof(Rect)) {
                return Serializer.BytesToRect(values, valuePositions[position]) as T;
            }
            if (type == typeof(Matrix4x4)) {
                return Serializer.BytesToMatrix4x4(values, valuePositions[position]) as T;
            }
            if (type == typeof(AnimationCurve)) {
                return Serializer.BytesToAnimationCurve(values, valuePositions[position]) as T;
            }
            if (type == typeof(LayerMask)) {
                return Serializer.BytesToLayerMask(values, valuePositions[position]) as T;
            }
            if (typeof(UnityEngine.Object).IsAssignableFrom(type)) {
                return Serializer.BytesToUnityObject(values, valuePositions[position], unityObjects) as T;
            }
            if (typeof(IList).IsAssignableFrom(type) || type.IsClass || (type.IsValueType && !type.IsPrimitive)) {
                return Serializer.BytesToValue(type, name, valuePositionMap, hashPrefix, values, valuePositions, unityObjects, useFields, visibility) as T;
            }
            return null;
        }
    }

    /// <summary>
    /// Specifies the visibility of the field/properties that should be retrieved.
    /// </summary>
    public enum MemberVisibility
    {
        None,           // No fields will be retrieved.
        Public,         // Only returns public members.
        AllPublic,      // Returns all public members, even if they have the NonSerialized attribute.
        Snapshot,       // Only returns members with the Snapshot attribute.
        All,            // Returns both public and private members.
        Last
    }

    /// <summary>
    /// Attribute which prevents preset serialization on the property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class NonSerialized : Attribute
    {
        // Intentionally left blank.
    }

    /// <summary>
    /// Attribute which will serialize the property when taking a snapshot.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class Snapshot : Attribute
    {
        // Intentionally left blank.
    }

    /// <summary>
    /// Attribute which prevents forces serialization on the field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class ForceSerialized : Attribute
    {
        // Intentionally left blank.
    }
}