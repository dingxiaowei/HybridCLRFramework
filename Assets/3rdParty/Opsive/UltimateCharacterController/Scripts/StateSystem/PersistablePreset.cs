/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using System;
using System.Collections.Generic;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.StateSystem
{
    /// <summary>
    /// Allows the Preset component to serialized the property value.
    /// </summary>
    public class PersistablePreset : Preset
    {
        [Tooltip("The serialized properties.")]
        [SerializeField] protected Serialization m_Data;

        public Serialization Data { get { return m_Data; } set { m_Data = value; } }

        /// <summary>
        /// Creates a persistable preset based off of the specified component.
        /// </summary>
        /// <param name="obj">The object to retrieve the property values of.</param>
        /// <returns>The created preset. Null if no properties have been found to create the preset with.</returns>
        public static PersistablePreset CreatePreset(object obj)
        {
            return CreatePreset(obj, MemberVisibility.None);
        }

        /// <summary>
        /// Creates a persistable preset based off of the specified component and visibility.
        /// </summary>
        /// <param name="obj">The object to retrieve the property values of.</param>
        /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        /// <returns>The created preset. Null if no properties have been found to create the preset with.</returns>
        public static PersistablePreset CreatePreset(object obj, MemberVisibility visibility)
        {
            var data = new Serialization();
            data.Serialize(obj, false, visibility);
            var preset = CreateInstance<PersistablePreset>();
            preset.Data = data;
            return preset;
        }

        /// <summary>
        /// Initializes the preset with the specified visiblity. The preset must be initialized before the preset values are applied so the delegates can be created.
        /// </summary>
        /// <param name="obj">The object to map the delegates to.</param>
        /// <param name="visibility">Specifies the visibility of the field/properties that should be retrieved.</param>
        public override void Initialize(object obj, MemberVisibility visibility)
        {
            m_Delegates = new BaseDelegate[m_Data.ValueHashes.Length];
            var valuePositionMap = new Dictionary<int, int>(m_Data.ValueHashes.Length);
            for (int i = 0; i < m_Data.ValueHashes.Length; ++i) {
                valuePositionMap.Add(m_Data.ValueHashes[i], i);
            }

            var valueCount = 0;
            var properties = Serialization.GetSerializedProperties(obj.GetType(), visibility);
            for (int i = 0; i < properties.Length; ++i) {
                var hash = Serialization.StringHash(properties[i].PropertyType.FullName) + Serialization.StringHash(properties[i].Name);
                int position;
                if (!valuePositionMap.TryGetValue(hash, out position)) {
                    continue;
                }

                // Create a generic delegate based on the property type.
                var genericDelegateType = typeof(GenericDelegate<>).MakeGenericType(properties[i].PropertyType);
                m_Delegates[valueCount] = Activator.CreateInstance(genericDelegateType) as BaseDelegate;

                // Initialize the delegate.
                if (m_Delegates[valueCount] != null) {
                    m_Delegates[valueCount].Initialize(obj, properties[i], valuePositionMap, m_Data, visibility);
                } else {
                    Debug.LogWarning("Warning: Unable to create preset of type " + properties[i].PropertyType);
                }
                valueCount++;
            }

            // The delegate length may not match if a property has been added but no longer exists.
            if (m_Delegates.Length != valueCount) {
                Array.Resize(ref m_Delegates, valueCount);
            }
        }
    }
}