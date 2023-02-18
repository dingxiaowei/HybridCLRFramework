/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Input
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Events;
    using Opsive.UltimateCharacterController.Utility;
    using Opsive.UltimateCharacterController.StateSystem;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    /// <summary>
    /// Abstract class to expose a common interface for any input implementation.
    /// </summary>
    public abstract class PlayerInput : StateBehavior
    {
        /// <summary>
        /// Specifies how to set the look vector.
        /// </summary>
        public enum LookVectorMode
        {
            Smoothed,       // A smoothing will be applied to the look vector.
            UnitySmoothed,  // The smoothed Unity input values will be used.
            Raw,            // The raw input values will be used.
            Manual          // The look vector is assigned manually. This is useful for VR head movement.
        }

        [Tooltip("The name of the horizontal camera input mapping.")]
        [SerializeField] protected string m_HorizontalLookInputName = "Mouse X";
        [Tooltip("The name of the vertical camera input mapping.")]
        [SerializeField] protected string m_VerticalLookInputName = "Mouse Y";
        [Tooltip("Specifies how the look vector is assigned.")]
        [SerializeField] protected LookVectorMode m_LookVectorMode = LookVectorMode.Smoothed;
        [Tooltip("If using look smoothing, specifies how sensitive the mouse is. The higher the value the more sensitive.")]
        [SerializeField] protected Vector2 m_LookSensitivity = new Vector2(2f, 2f);
        [Tooltip("If using look smoothing, specifies a multiplier to apply to the LookSensitivity value.")]
        [SerializeField] protected float m_LookSensitivityMultiplier = 1;
        [Tooltip("If using look smoothing, the amount of history to store of previous look values.")]
        [SerializeField] protected int m_SmoothLookSteps = 20;
        [Tooltip("If using look smoothing, specifies how much weight each element should have on the total smoothed value (range 0-1).")]
        [SerializeField] protected float m_SmoothLookWeight = 0.5f;
        [Tooltip("If using look smoothing, specifies an exponent to give a smoother feel with smaller inputs.")]
        [SerializeField] protected float m_SmoothExponent = 1.05f;
        [Tooltip("If using look smoothing, specifies a maximum acceleration value of the smoothed look value (0 to disable).")]
        [SerializeField] protected float m_LookAccelerationThreshold = 0.4f;
        [Tooltip("The rate (in seconds) the component checks to determine if a controller is connected.")]
        [SerializeField] protected float m_ControllerConnectedCheckRate = 1f;
        [Tooltip("The state that should be activated when a controller is connected.")]
        [SerializeField] protected string m_ConnectedControllerState = "ConnectedController";
        [Tooltip("Unity event invoked when the gameplay input is enabled or disabled.")]
        [SerializeField] protected UnityBoolEvent m_EnableGamplayInputEvent;

        public string HorizontalLookInputName { get { return m_HorizontalLookInputName; } set { m_HorizontalLookInputName = value; } }
        public string VerticalLookInputName { get { return m_VerticalLookInputName; } set { m_VerticalLookInputName = value; } }
        public LookVectorMode LookMode { get { return m_LookVectorMode; }
            set
            {
                m_LookVectorMode = value;
                if (m_LookVectorMode == LookVectorMode.Smoothed && m_SmoothLookBuffer == null) {
                    m_SmoothLookBuffer = new Vector2[m_SmoothLookSteps];
                }
            }
        }
        public Vector2 LookSensitivity { get { return m_LookSensitivity; } set { m_LookSensitivity = value; } }
        public float LookSensitivityMultiplier { get { return m_LookSensitivityMultiplier; } set { m_LookSensitivityMultiplier = value; } }
        public int SmoothLookSteps { get { return m_SmoothLookSteps; } set { m_SmoothLookSteps = value; } }
        public float SmoothLookWeight { get { return m_SmoothLookWeight; } set { m_SmoothLookWeight = value; } }
        public float SmoothExponent { get { return m_SmoothExponent; } set { m_SmoothExponent = value; } }
        public float LookAccelerationThreshold { get { return m_LookAccelerationThreshold; } set { m_LookAccelerationThreshold = value; } }
        public float ControllerConnectedCheckRate { get { return m_ControllerConnectedCheckRate; } set { m_ControllerConnectedCheckRate = value; } }
        public string ConnectedControllerState { get { return m_ConnectedControllerState; } set { m_ConnectedControllerState = value; } }
        public UnityBoolEvent EnableGameplayInputEvent { get { return m_EnableGamplayInputEvent; } set { m_EnableGamplayInputEvent = value; } }

        private Vector2[] m_SmoothLookBuffer;
        private int m_SmoothLookBufferIndex;
        private int m_SmoothLookBufferCount;
        protected Vector2 m_RawLookVector;
        protected Vector2 m_CurrentLookVector;
        private float m_TimeScale = 1;
        private bool m_ControllerConnected;
        private Dictionary<string, float> m_ButtonDownTime;
        private Dictionary<string, float> m_ButtonUpTime;
        private ScheduledEventBase m_ControllerCheckEvent;
        private bool m_AllowInput = true;
        private bool m_Death = false;

        public Vector2 RawLookVector { set { m_RawLookVector = value; } }
        public Vector2 CurrentLookVector { set { m_CurrentLookVector = value; } }
        public bool ControllerConnected { get { return m_ControllerConnected; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (m_LookVectorMode == LookVectorMode.Smoothed) {
                m_SmoothLookBuffer = new Vector2[m_SmoothLookSteps];
            }

            EventHandler.RegisterEvent<float>(gameObject, "OnCharacterChangeTimeScale", ChangeTimeScale);
            EventHandler.RegisterEvent<bool>(gameObject, "OnEnableGameplayInput", EnableGameplayInput);
            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(gameObject, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(gameObject, "OnRespawn", OnRespawn);

            CheckForController();
        }

        /// <summary>
        /// Returns true if the button is being pressed.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <returns>True of the button is being pressed.</returns>
        public bool GetButton(string name)
        {
            return GetButtonInternal(name);
        }

        /// <summary>
        /// Internal method which returns true if the button is being pressed.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <returns>True of the button is being pressed.</returns>
        protected virtual bool GetButtonInternal(string name) { return false; }

        /// <summary>
        /// Returns true if the button was pressed this frame.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <returns>True if the button is pressed this frame.</returns>
        public bool GetButtonDown(string name) { return GetButtonDownInternal(name); }

        /// <summary>
        /// Internal method which returns true if the button was pressed this frame.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <returns>True if the button is pressed this frame.</returns>
        protected virtual bool GetButtonDownInternal(string name) { return false; }

        /// <summary>
        /// Returns true if the button is up.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <returns>True if the button is up.</returns>
        public bool GetButtonUp(string name) { return GetButtonUpInternal(name); }

        /// <summary>
        /// Internal method which returns true if the button is up.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <returns>True if the button is up.</returns>
        protected virtual bool GetButtonUpInternal(string name) { return false; }

        /// <summary>
        /// Returns true if a double press occurred (double click or double tap).
        /// </summary>
        /// <param name="name">The button name to check for a double press.</param>
        /// <returns>True if a double press occurred (double click or double tap).</returns>
        public bool GetDoublePress(string name)
        {
            if (GetButtonDown(name)) {
                if (m_ButtonDownTime == null) {
                    m_ButtonDownTime = new Dictionary<string, float>();
                }
                var time = -1f;
                if (m_ButtonDownTime.TryGetValue(name, out time)) {
                    if (time != Time.unscaledTime && time + 0.2f > Time.unscaledTime) {
                        return true;
                    }
                    m_ButtonDownTime[name] = Time.unscaledTime;
                } else {
                    m_ButtonDownTime.Add(name, Time.unscaledTime);
                }
            }

            return false;
        }

        /// <summary>
        /// Internal method which returns true if a double press occurred (double click or double tap).
        /// </summary>
        /// <param name="name">The button name to check for a double press.</param>
        /// <returns>True if a double press occurred (double click or double tap).</returns>
        protected virtual bool GetDoublePressInternal(string name) { return false; }

        /// <summary>
        /// Returns true if a tap occurred.
        /// </summary>
        /// <param name="name">The button name to check for a tap.</param>
        /// <returns>True if a tap occurred.</returns>
        public bool GetTap(string name)
        {
            var time = -1f;
            if (GetButton(name)) {
                if (m_ButtonDownTime == null) {
                    m_ButtonDownTime = new Dictionary<string, float>();
                }
                if (!m_ButtonDownTime.ContainsKey(name)) {
                    m_ButtonDownTime.Add(name, Time.unscaledTime);
                }
            } else if (m_ButtonDownTime != null && m_ButtonDownTime.TryGetValue(name, out time)) {
                m_ButtonDownTime.Remove(name);
                if (time != Time.unscaledTime && time + 0.2f > Time.unscaledTime) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if a long press occurred.
        /// </summary>
        /// <param name="name">The button name to check for a long press.</param>
        /// <param name="duration">The duration of a long press.</param>
        /// <param name="waitForRelease">Indicates if the long press should occur after the button has been released (true) or after the duration (false).</param>
        /// <returns>True if a long press occurred.</returns>
        public bool GetLongPress(string name, float duration, bool waitForRelease)
        {
            // Button down and up times won't be allocated unless double or long press inputs are used.
            if (m_ButtonDownTime == null) {
                m_ButtonDownTime = new Dictionary<string, float>();
                m_ButtonUpTime = new Dictionary<string, float>();
            }

            if (GetButtonInternal(name)) {
                var downTime = -1f;
                if (m_ButtonDownTime.TryGetValue(name, out downTime)) {
                    // Only set the down time if the up time is greater than the down time. This will prevent the current time from being set every tick. 
                    var upTime = -1f;
                    m_ButtonUpTime.TryGetValue(name, out upTime);
                    if (upTime > downTime) {
                        m_ButtonDownTime[name] = downTime = Time.unscaledTime;
                    }
                    // Return true as soon as the button has been pressed for the duration.
                    if (!waitForRelease) {
                        return downTime + duration <= Time.unscaledTime;
                    }
                } else {
                    m_ButtonDownTime.Add(name, Time.unscaledTime);
                }
            } else {
                var upTime = -1f;
                if (m_ButtonUpTime.TryGetValue(name, out upTime)) {
                    // Only set the up time if the down time is greater than the up time. This will prevent the current time from being set every tick. 
                    var downTime = -1f;
                    m_ButtonDownTime.TryGetValue(name, out downTime);
                    if (downTime > upTime) {
                        m_ButtonUpTime[name] = upTime = Time.unscaledTime;
                        if (waitForRelease) {
                            return downTime + duration <= Time.unscaledTime;
                        }
                    }
                } else {
                    m_ButtonUpTime.Add(name, Time.unscaledTime);
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the value of the axis with the specified name.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The value of the axis.</returns>
        public float GetAxis(string name) { return GetAxisInternal(name); }

        /// <summary>
        /// Internal method which returns the value of the axis with the specified name.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The value of the axis.</returns>
        protected virtual float GetAxisInternal(string name) { return 0; }

        /// <summary>
        /// Returns the value of the raw axis with the specified name.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The value of the raw axis.</returns>
        public float GetAxisRaw(string name) { return GetAxisRawInternal(name); }

        /// <summary>
        /// Returns the value of the raw axis with the specified name.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The value of the raw axis.</returns>
        protected virtual float GetAxisRawInternal(string name) { return 0; }

        /// <summary>
        /// Is a controller connected?
        /// </summary>
        /// <returns>True if a controller is connected.</returns>
        public bool IsControllerConnected() { return m_ControllerConnected; }

        /// <summary>
        /// Is the cursor visible?
        /// </summary>
        /// <returns>True if the cursor is visible.</returns>
        public bool IsCursorVisible()
        {
            return Cursor.visible;
        }

        /// <summary>
        /// Returns the position of the mouse.
        /// </summary>
        /// <returns>The mouse position.</returns>
        public virtual Vector2 GetMousePosition() { return UnityEngine.Input.mousePosition; }

        /// <summary>
        /// Determines if a controller is connected.
        /// </summary>
        private void CheckForController()
        {
            var controllerConencted = UnityEngine.Input.GetJoystickNames().Length > 0;
            if (m_ControllerConnected != controllerConencted) {
                m_ControllerConnected = controllerConencted;
                if (!string.IsNullOrEmpty(m_ConnectedControllerState)) {
                    StateManager.SetState(gameObject, m_ConnectedControllerState, m_ControllerConnected);
                }
                EventHandler.ExecuteEvent<bool>(gameObject, "OnInputControllerConnected", m_ControllerConnected);
            }

            // Schedule the controller check event if the rate is positive.
            // UnityEngine.Input.GetJoystickNames generates garbage so limit the amount of time the controller is checked.
            if (m_ControllerConnectedCheckRate > 0 && (m_ControllerCheckEvent == null || !m_ControllerCheckEvent.Active)) {
                m_ControllerCheckEvent = Scheduler.Schedule(m_ControllerConnectedCheckRate, CheckForController);
            }
        }

        /// <summary>
        /// Updates the look smoothing buffer to the current look vector.
        /// </summary>
        private void FixedUpdate()
        {
            if (!Application.isFocused) {
                return;
            }

            m_RawLookVector.x = GetAxisRaw(m_HorizontalLookInputName);
            m_RawLookVector.y = GetAxisRaw(m_VerticalLookInputName);

            if (m_LookVectorMode == LookVectorMode.Smoothed) {
                // Set the current input to the look buffer.
                m_SmoothLookBuffer[m_SmoothLookBufferIndex].x = m_RawLookVector.x;
                m_SmoothLookBuffer[m_SmoothLookBufferIndex].y = m_RawLookVector.y;
                if (m_SmoothLookBufferCount < m_SmoothLookBufferIndex + 1) {
                    m_SmoothLookBufferCount = m_SmoothLookBufferIndex + 1;
                }

                // Calculate the input smoothing value. The more recent the input value occurred the higher the influence it has on the final smoothing value.
                var weight = 1f;
                var average = Vector2.zero;
                var averageTotal = 0f;
                var deltaTime = m_TimeScale * TimeUtility.FramerateDeltaTime;
                for (int i = 0; i < m_SmoothLookBufferCount; ++i) {
                    var index = m_SmoothLookBufferIndex - i;
                    if (index < 0) { index = m_SmoothLookBufferCount + m_SmoothLookBufferIndex - i; }
                    average += m_SmoothLookBuffer[index] * weight;
                    averageTotal += weight;
                    // The deltaTime will be 0 if Unity just started to play after stepping through the editor.
                    if (deltaTime > 0) {
                        weight *= (m_SmoothLookWeight / deltaTime);
                    }
                }
                m_SmoothLookBufferIndex = (m_SmoothLookBufferIndex + 1) % m_SmoothLookBuffer.Length;

                // Store the averaged input value.
                averageTotal = Mathf.Max(1, averageTotal);
                m_CurrentLookVector = average / averageTotal;

                // Apply any look acceleration. The delta time will be zero on the very first frame.
                var lookAcceleration = 0f;
                if (m_LookAccelerationThreshold > 0 && deltaTime != 0) {
                    var accX = Mathf.Abs(m_CurrentLookVector.x);
                    var accY = Mathf.Abs(m_CurrentLookVector.y);
                    lookAcceleration = Mathf.Sqrt((accX * accX) + (accY * accY)) / deltaTime;
                    if (lookAcceleration > m_LookAccelerationThreshold) {
                        lookAcceleration = m_LookAccelerationThreshold;
                    }
                }

                // Determine the final value.
                m_CurrentLookVector.x *= ((m_LookSensitivity.x * m_LookSensitivityMultiplier) + lookAcceleration) * TimeUtility.FramerateDeltaTime;
                m_CurrentLookVector.y *= ((m_LookSensitivity.y * m_LookSensitivityMultiplier) + lookAcceleration) * TimeUtility.FramerateDeltaTime;

                m_CurrentLookVector.x = Mathf.Sign(m_CurrentLookVector.x) * Mathf.Pow(Mathf.Abs(m_CurrentLookVector.x), m_SmoothExponent);
                m_CurrentLookVector.y = Mathf.Sign(m_CurrentLookVector.y) * Mathf.Pow(Mathf.Abs(m_CurrentLookVector.y), m_SmoothExponent);
            } else if (m_LookVectorMode == LookVectorMode.UnitySmoothed) {
                m_CurrentLookVector.x = GetAxis(m_HorizontalLookInputName);
                m_CurrentLookVector.y = GetAxis(m_VerticalLookInputName);
            } else if (m_LookVectorMode == LookVectorMode.Raw) {
                m_CurrentLookVector = m_RawLookVector;
            }
        }

        /// <summary>
        /// Returns the look vector. Will apply smoothing if specified otherwise will return the GetAxis value.
        /// </summary>
        /// <param name="smoothed">Should the smoothing value be returned? If false the raw look vector will be returned.</param>
        /// <returns>The current look vector.</returns>
        public virtual Vector2 GetLookVector(bool smoothed)
        {
            if (smoothed) {
                return m_CurrentLookVector;
            }
            return m_RawLookVector;
        }

        /// <summary>
        /// Returns true if the pointer is over a UI element.
        /// </summary>
        /// <returns>True if the pointer is over a UI element.</returns>
        public virtual bool IsPointerOverUI()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// The character's local timescale has changed.
        /// </summary>
        /// <param name="timeScale">The new timescale.</param>
        private void ChangeTimeScale(float timeScale)
        {
            m_TimeScale = timeScale;
        }

        /// <summary>
        /// Enables or disables gameplay input. An example of when it will not be enabled is when there is a fullscreen UI over the main camera.
        /// </summary>
        /// <param name="enable">True if the input is enabled.</param>
        protected virtual void EnableGameplayInput(bool enable)
        {
            m_AllowInput = enable;
            enabled = m_AllowInput && !m_Death;
            if (enabled && !Application.isFocused) {
                OnApplicationFocus(true);
            } else if (!enabled) {
                m_RawLookVector = m_CurrentLookVector = Vector3.zero;
            }

            if (m_EnableGamplayInputEvent != null) {
                m_EnableGamplayInputEvent.Invoke(enable);
            }
        }

        /// <summary>
        /// The character has died. Disable the component.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            m_Death = true;
            enabled = m_AllowInput && !m_Death;
        }

        /// <summary>
        /// The character has respawned. Enable the component.
        /// </summary>
        private void OnRespawn()
        {
            m_Death = false;
            enabled = m_AllowInput && !m_Death;
        }

        /// <summary>
        /// Does the game have focus?
        /// </summary>
        /// <param name="hasFocus">True if the game has focus.</param>
        protected virtual void OnApplicationFocus(bool hasFocus)
        {
            if (Application.isFocused) {
                CheckForController();
            } else {
                m_CurrentLookVector = Vector3.zero;
            }
        }
        
        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<float>(gameObject, "OnCharacterChangeTimeScale", ChangeTimeScale);
            EventHandler.UnregisterEvent<bool>(gameObject, "OnEnableGameplayInput", EnableGameplayInput);
            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(gameObject, "OnDeath", OnDeath);
            EventHandler.UnregisterEvent(gameObject, "OnRespawn", OnRespawn);
            if (m_ControllerCheckEvent != null) {
                Scheduler.Cancel(m_ControllerCheckEvent);
                m_ControllerCheckEvent = null;
            }
        }
    }
}