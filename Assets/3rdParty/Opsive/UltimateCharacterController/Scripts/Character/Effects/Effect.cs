/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.StateSystem;
using Opsive.UltimateCharacterController.Camera;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Character.Effects
{
    /// <summary>
    /// Effects allow for extra camera/item movements that are applied to the character. Examples of an effect include an earthquake shake or a boss stomp. Effects 
    /// do not affect the Animator and are not synchronized over the network. For anything more involved an Ability should be used instead.
    /// </summary>
    [System.Serializable]
    public abstract class Effect : StateObject
    {
        [Tooltip("Can the ability be activated?")]
        [HideInInspector] [SerializeField] protected bool m_Enabled = true;
        [Tooltip("Should the effect be started when it is enabled?")]
        [SerializeField] protected bool m_StartWhenEnabled;
        [Tooltip("Specifies the name of the state that the effect should activate.")]
        [SerializeField] protected string m_State;
#if UNITY_EDITOR
        [Tooltip("An editor only description of the effect.")]
        [HideInInspector] [SerializeField] protected string m_InspectorDescription;
#endif

        public bool Enabled { get { return m_Enabled; }
            set
            {
                if (m_Enabled == value) {
                    return;
                }
                m_Enabled = value;
                if (!m_Enabled && IsActive) {
                    StopEffect(false);
                } else if (Application.isPlaying && m_Enabled && !IsActive && m_StartWhenEnabled) {
                    StartEffect();
                }
            }
        }
#if UNITY_EDITOR
        public string InspectorDescription { get { return m_InspectorDescription; } set { m_InspectorDescription = value; } }
#endif
        public bool StartWhenEnabled { get { return m_StartWhenEnabled; } set { m_StartWhenEnabled = value; } }

        protected GameObject m_GameObject;
        protected Transform m_Transform;
        protected UltimateCharacterLocomotion m_CharacterLocomotion;
        protected CameraController m_CameraController;

        private int m_ActiveIndex = -1;
        private int m_Index = -1;

        public bool IsActive { get { return m_ActiveIndex != -1; } }
        [NonSerialized] public int Index { get { return m_Index; } set { m_Index = value; } }
        [NonSerialized] public int ActiveIndex { get { return m_ActiveIndex; } set { m_ActiveIndex = value; } }

        /// <summary>
        /// Initializes the effect to the specified controller.
        /// </summary>
        /// <param name="characterLocomotion">The character locomotion component to initialize the effect to.</param>
        /// <param name="index">The prioirty index of the ability within the controller.</param>
        public void Initialize(UltimateCharacterLocomotion characterLocomotion, int index)
        {
            m_CharacterLocomotion = characterLocomotion;
            m_GameObject = characterLocomotion.gameObject;
            m_Transform = characterLocomotion.transform;
            m_Index = index;

            // The StateObject class needs to initialize itself.
            Initialize(m_GameObject);
        }

        /// <summary>
        /// Method called by MonoBehaviour.Awake. Can be used for initialization.
        /// </summary>
        public virtual void Awake()
        {
            EventHandler.RegisterEvent<CameraController>(m_GameObject, "OnCharacterAttachCamera", OnAttachCamera);
        }

        /// <summary>
        /// Method called by MonoBehaviour.Start. This method is called on all effects when the MonoBehaviour.Start method is called.
        /// </summary>
        public virtual void Start() { }

        /// <summary>
        /// Can the effect be started?
        /// </summary>
        /// <returns>True if the effect can be started.</returns>
        public virtual bool CanStartEffect() { return true; }

        /// <summary>
        /// Tries to start the effect.
        /// </summary>
        /// <returns>True if the effect was successfully started.</param>
        public bool StartEffect()
        {
            return m_CharacterLocomotion.TryStartEffect(this);
        }

        /// <summary>
        /// Starts executing the effect.
        /// </summary>
        public void StartEffect(int index)
        {
            m_ActiveIndex = index;

            EffectStarted();
        }

        /// <summary>
        /// The effect has been started.
        /// </summary>
        protected virtual void EffectStarted()
        {
            if (!string.IsNullOrEmpty(m_State)) {
                StateManager.SetState(m_GameObject, m_State, true);
            }
        }

        /// <summary>
        /// Updates the effect. Called during the MonoBehaviour.Update loop.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Stop the effect from running.
        /// </summary>
        public void StopEffect()
        {
            StopEffect(false);
        }

        /// <summary>
        /// Stop the effect from running.
        /// </summary>
        /// <param name="fromController">Is the effect being stopped from the UltimateCharacterController?</param>
        public void StopEffect(bool fromController)
        {
            // If the effect wasn't stopped from the character controller then call the controller's stop effect method. The controller must be aware of the stopping.
            if (!fromController) {
                m_CharacterLocomotion.TryStopEffect(this);
                return;
            }

            m_ActiveIndex = -1;

            EffectStopped();
        }

        /// <summary>
        /// The effect has stopped running.
        /// </summary>
        protected virtual void EffectStopped()
        {
            if (!string.IsNullOrEmpty(m_State)) {
                StateManager.SetState(m_GameObject, m_State, false);
            }
        }

        /// <summary>
        /// The character has been attached to the camera. Initialze the camera-related values.
        /// </summary>
        /// <param name="cameraController">The camera controller attached to the character. Can be null.</param>
        private void OnAttachCamera(CameraController cameraController)
        {
            m_CameraController = cameraController;
        }

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public virtual void OnDestroy()
        {
            EventHandler.UnregisterEvent<CameraController>(m_GameObject, "OnCharacterAttachCamera", OnAttachCamera);
        }
    }
}