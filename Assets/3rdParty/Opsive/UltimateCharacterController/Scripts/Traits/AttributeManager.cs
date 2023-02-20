/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.StateSystem;
using Opsive.UltimateCharacterController.Utility;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Traits
{
    /// <summary>
    /// An Attribute can be used to describe a set of values which change over time. Examples include health, shield, stamina, hunger, etc.
    /// </summary>
    [System.Serializable]
    public class Attribute : StateObject
    {
        /// <summary>
        /// Describes how the attribute should update the value.
        /// </summary>
        public enum AutoUpdateValue
        {
            None,       // Do not automatically update the value.
            Decrease,   // Decreases the value to the min value.
            Increase    // Increases the value to the max value.
        }

        [Tooltip("The name of the attribute.")]
        [SerializeField] protected string m_Name;
        [Tooltip("The minimum value of the attribute.")]
        [SerializeField] protected float m_MinValue;
        [Tooltip("The maximum value of the attribute.")]
        [SerializeField] protected float m_MaxValue = 100;
        [Tooltip("The current value of the attribute.")]
        [SerializeField] protected float m_Value = 100;
        [Tooltip("Describes how the attribute should update the value.")]
        [SerializeField] protected AutoUpdateValue m_AutoUpdateValueType;
        [Tooltip("The amount of time between a value change and when the auto updater should start.")]
        [SerializeField] protected float m_AutoUpdateStartDelay = 1f;
        [Tooltip("The amount of time to wait in between auto update loops.")]
        [SerializeField] protected float m_AutoUpdateInterval = 0.1f;
        [Tooltip("The amount to change the value with each auto update.")]
        [SerializeField] protected float m_AutoUpdateAmount = 0.2f;

        [NonSerialized] public string Name { get { return m_Name; } set { m_Name = value; } }
        public float MinValue { get { return m_MinValue; } set { m_MinValue = value; } }
        public float MaxValue { get { return m_MaxValue; } set { m_MaxValue = value; } }
        [NonSerialized] public float Value { get { return m_Value; }
            set
            {
                m_Value = Mathf.Clamp(value, m_MinValue, m_MaxValue);
                EventHandler.ExecuteEvent(m_GameObject, "OnAttributeUpdateValue", this);

                ScheduleAutoUpdate(m_AutoUpdateStartDelay);
            }
        }
        public AutoUpdateValue AutoUpdateValueType { get { return m_AutoUpdateValueType; } set { m_AutoUpdateValueType = value; ScheduleAutoUpdate(0.01f); } }
        public float AutoUpdateStartDelay { get { return m_AutoUpdateStartDelay; } set { m_AutoUpdateStartDelay = value; ScheduleAutoUpdate(0.01f); } }
        public float AutoUpdateInterval { get { return m_AutoUpdateInterval; } set { m_AutoUpdateInterval = value; } }
        public float AutoUpdateAmount { get { return m_AutoUpdateAmount; } set { m_AutoUpdateAmount = value; } }

        private GameObject m_GameObject;
        private float m_StartValue;
        private ScheduledEventBase m_AutoUpdateEvent;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Attribute() { }

        /// <summary>
        /// Two parameter constructor.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        public Attribute(string name, float value)
        {
            m_Name = name;
            m_Value = m_MaxValue = value;
        }

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        /// <param name="gameObject">The GameObject this object is attached to.</param>
        public override void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);

            m_GameObject = gameObject;
            m_StartValue = m_Value;

            ScheduleAutoUpdate(m_AutoUpdateStartDelay);
        }

        /// <summary>
        /// Schedules an auto update if the auto update value type is not set to none.
        /// </summary>
        /// <param name="delay">The amount to delay the attribute update event by.</param>
        private void ScheduleAutoUpdate(float delay)
        {
            Scheduler.Cancel(m_AutoUpdateEvent);
            if ((m_AutoUpdateValueType == AutoUpdateValue.Increase && m_Value != m_MaxValue) ||
                (m_AutoUpdateValueType == AutoUpdateValue.Decrease && m_Value != m_MinValue)) {
                m_AutoUpdateEvent = Scheduler.Schedule(delay, UpdateValue);
            }
        }

        /// <summary>
        /// Callback when the auto update event is executed.
        /// </summary>
        private void UpdateValue()
        {
            if (m_AutoUpdateValueType == AutoUpdateValue.Increase) {
                m_Value = Mathf.Min(m_Value + m_AutoUpdateAmount, m_MaxValue);
                if (m_Value < m_MaxValue) {
                    m_AutoUpdateEvent = Scheduler.Schedule(m_AutoUpdateInterval, UpdateValue);
                } else {
                    EventHandler.ExecuteEvent(this, "OnAttributeReachedDestinationValue");
                }
            } else { // Decrease.
                m_Value = Mathf.Max(m_Value - m_AutoUpdateAmount, m_MinValue);
                if (m_Value > m_MinValue) {
                    m_AutoUpdateEvent = Scheduler.Schedule(m_AutoUpdateInterval, UpdateValue);
                } else {
                    EventHandler.ExecuteEvent(this, "OnAttributeReachedDestinationValue");
                }
            }
            EventHandler.ExecuteEvent(m_GameObject, "OnAttributeUpdateValue", this);
        }

        /// <summary>
        /// Is the attribute currently valid? A valid attribute will not be at the minimum value.
        /// </summary>
        /// <param name="valueChange">The amount that the attribute is going to change values by.</param>
        /// <returns>True if the attribute value is currently valid.</returns>
        public bool IsValid(float valueChange)
        {
            return m_Value + valueChange >= m_MinValue;
        }

        /// <summary>
        /// Cancels the auto update value.
        /// </summary>
        public void CancelAutoUpdate()
        {
            Scheduler.Cancel(m_AutoUpdateEvent);
        }

        /// <summary>
        /// Resets the value to the starting value.
        /// </summary>
        public void ResetValue()
        {
            Scheduler.Cancel(m_AutoUpdateEvent);
            m_Value = m_StartValue;
            EventHandler.ExecuteEvent(m_GameObject, "OnAttributeUpdateValue", this);
        }

        /// <summary>
        /// Callback when the StateManager has changed the active state on the current object.
        /// </summary>
        public override void StateChange()
        {
            ScheduleAutoUpdate(m_AutoUpdateStartDelay);
        }

        /// <summary>
        /// Attribute destructor - will cancel the update event.
        /// </summary>
        ~Attribute()
        {
            Scheduler.Cancel(m_AutoUpdateEvent);
        }
    }

    /// <summary>
    /// Represents an Attribute that can have its values changed.
    /// </summary>
    [System.Serializable]
    public class AttributeModifier
    {
        [Tooltip("The name of the attribute.")]
        [SerializeField] protected string m_AttributeName;
        [Tooltip("Specifies the amount to change the attribute by when the modifier is enabled.")]
        [SerializeField] protected float m_ValueChange;
        [Tooltip("Should a new update value be set?")]
        [SerializeField] protected bool m_ChangeUpdateValue;
        [Tooltip("Describes how the attribute should update the value.")]
        [SerializeField] protected Attribute.AutoUpdateValue m_AutoUpdateValueType;
        [Tooltip("The amount of time between a value change and when the auto updater should start.")]
        [SerializeField] protected float m_AutoUpdateStartDelay = 1f;
        [Tooltip("The amount of time to wait in between auto update loops.")]
        [SerializeField] protected float m_AutoUpdateInterval = 0.1f;
        [Tooltip("The amount to change the value with each auto update.")]
        [SerializeField] protected float m_AutoUpdateAmount = 0.2f;

        [NonSerialized] public string AttributeName { get { return m_AttributeName; } set { m_AttributeName = value; } }
        [NonSerialized] public float ValueChange { get { return m_ValueChange; } set { m_ValueChange = value; } }
        [NonSerialized] public bool ChangeUpdateValue { get { return m_ChangeUpdateValue; } set { m_ChangeUpdateValue = value; } }
        [NonSerialized] public Attribute.AutoUpdateValue AutoUpdateValueType { get { return m_AutoUpdateValueType; } set { m_AutoUpdateValueType = value; } }
        [NonSerialized] public float AutoUpdateStartDelay { get { return m_AutoUpdateStartDelay; } set { m_AutoUpdateStartDelay = value; } }
        [NonSerialized] public float AutoUpdateInterval { get { return m_AutoUpdateInterval; } set { m_AutoUpdateInterval = value; } }
        [NonSerialized] public float AutoUpdateAmount { get { return m_AutoUpdateAmount; } set { m_AutoUpdateAmount = value; } }

        private Attribute m_Attribute;

        private bool m_AutoUpdating;
        private Attribute.AutoUpdateValue m_StoredAutoUpdateValueType;
        private float m_StoredAutoUpdateStartDelay;
        private float m_StoredAutoUpdateInterval;
        private float m_StoredAutoUpdateAmount;

        /// <summary>
        /// Initializes the AttributeModifier.
        /// </summary>
        /// <param name="gameObject">The GameObject that has the AttributeManager attached to it.</param>
        public void Initialize(GameObject gameObject)
        {
            if (string.IsNullOrEmpty(m_AttributeName)) {
                return;
            }

            var attributeManager = gameObject.GetCachedComponent<AttributeManager>();
            if (attributeManager != null) {
                m_Attribute = attributeManager.GetAttribute(m_AttributeName);
            }
        }

        /// <summary>
        /// Is the attribute currently valid? A valid attribute will not be at the minimum value.
        /// </summary>
        /// <returns>True if the attribute value is currently valid.</returns>
        public bool IsValid()
        {
            if (m_Attribute == null) {
                return true;
            }

            return m_Attribute.IsValid(m_ValueChange);
        }

        /// <summary>
        /// Enables or disables the modifier.
        /// </summary>
        /// <param name="enable"></param>
        public void EnableModifier(bool enable)
        {
            if (m_Attribute == null) {
                return;
            }

            // The attribute can be changed by a single value...
            if (enable && m_ValueChange != 0) {
                m_Attribute.Value += m_ValueChange;
            }

            if (!m_ChangeUpdateValue || m_AutoUpdating == enable) {
                return;
            }

            // ...Or a change with a longer duration.
            m_AutoUpdating = enable;
            if (enable) {
                m_StoredAutoUpdateAmount = m_Attribute.AutoUpdateAmount;
                m_StoredAutoUpdateInterval = m_Attribute.AutoUpdateInterval;
                m_StoredAutoUpdateStartDelay = m_Attribute.AutoUpdateStartDelay;
                m_StoredAutoUpdateValueType = m_Attribute.AutoUpdateValueType;

                m_Attribute.AutoUpdateAmount = m_AutoUpdateAmount;
                m_Attribute.AutoUpdateInterval = m_AutoUpdateInterval;
                m_Attribute.AutoUpdateStartDelay = m_AutoUpdateStartDelay;
                m_Attribute.AutoUpdateValueType = m_AutoUpdateValueType;    
            } else {
                m_Attribute.AutoUpdateAmount = m_StoredAutoUpdateAmount;
                m_Attribute.AutoUpdateInterval = m_StoredAutoUpdateInterval;
                m_Attribute.AutoUpdateStartDelay = m_StoredAutoUpdateStartDelay;
                m_Attribute.AutoUpdateValueType = m_StoredAutoUpdateValueType;
            }
        }
    }

    /// <summary>
    /// The AttributeManager will manage the array of Attributes.
    /// </summary>
    public class AttributeManager : MonoBehaviour
    {
        [Tooltip("The array of Attributes on the object.")]
        [SerializeField] protected Attribute[] m_Attributes = new Attribute[] { new Attribute("Health", 100) };

        public Attribute[] Attributes { get { return m_Attributes; } set { m_Attributes = value; } }

        private Dictionary<string, Attribute> m_NameAttributeMap = new Dictionary<string, Attribute>();

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            for (int i = 0; i < m_Attributes.Length; ++i) {
                m_NameAttributeMap.Add(m_Attributes[i].Name, m_Attributes[i]);
                m_Attributes[i].Initialize(gameObject);
            }
        }

        /// <summary>
        /// Gets the attribute with the specified name.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>The attribute with the specified name. Will return null if no attributes with the specified name exist.</returns>
        public Attribute GetAttribute(string name)
        {
            if (string.IsNullOrEmpty(name)) {
                return null;
            }
            Attribute attribute;
            if (m_NameAttributeMap.TryGetValue(name, out attribute)) {
                return attribute;
            }
            return null;
        }
    }
}