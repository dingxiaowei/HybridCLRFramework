/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Utility;

namespace Opsive.UltimateCharacterController.Demo
{
    /// <summary>
    /// Adjusts the global timescale when the object enters the trigger.
    /// </summary>
    public class TimeAdjuster : MonoBehaviour
    {
        [Tooltip("The TimeScale to adjust to.")]
        [SerializeField] protected float m_TimeScale = 0.5f;
        [Tooltip("The rate that the time scale changes from one value to another.")]
        [SerializeField] protected float m_TimeScaleChangeSpeed = 0.1f;
        [Tooltip("The length of time that the timescale is adjusted for.")]
        [SerializeField] protected float m_Duration = 5;
        [Tooltip("The LayerMask that triggers when the time is adjusted.")]
        [SerializeField] protected LayerMask m_LayerMask = 1 << LayerManager.Character;
        [Tooltip("The audio clip that should be played when the object enters the trigger.")]
        [SerializeField] protected AudioClip m_AudioClip;

        [Tooltip("The amount to rotate the object.")]
        [SerializeField] protected Vector3 m_RotationSpeed;
        [Tooltip("The amount that the object bounces.")]
        [SerializeField] protected float m_BounceAmount;
        [Tooltip("The speed at which the object bounces.")]
        [SerializeField] protected float m_BounceSpeed;

        private Transform m_Transform;
        private AudioSource m_AudioSource;
        private GameObject[] m_Children;
        private UltimateCharacterLocomotion m_CharacterLocomotion;

        private bool m_Active;
        private float m_Time;
        private float m_StartYPosition;
        private bool m_ForwardBounce;
        private float m_BounceTime;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;
            m_AudioSource = GetComponent<AudioSource>();
            m_Children = new GameObject[m_Transform.childCount];
            for (int i = 0; i < m_Transform.childCount; ++i) {
                m_Children[i] = m_Transform.GetChild(i).gameObject;
            }

            m_StartYPosition = m_Transform.position.y;
        }

        /// <summary>
        /// Move the object.
        /// </summary>
        private void Update()
        {
            m_Transform.rotation *= Quaternion.Euler(m_RotationSpeed);

            m_BounceTime += m_BounceSpeed * Time.deltaTime * (m_ForwardBounce ? 1 : -1);
            if (m_BounceTime < 0) {
                m_ForwardBounce = true;
                m_BounceTime = 0;
            } else if (m_BounceTime > 1) {
                m_ForwardBounce = false;
                m_BounceTime = 1;
            }

            var position = m_Transform.position;
            position.y = Mathf.SmoothStep(m_StartYPosition, m_StartYPosition + m_BounceAmount, m_BounceTime);
            m_Transform.position = position;
        }

        /// <summary>
        /// The other collider has entered the trigger.
        /// </summary>
        /// <param name="other">The collider which entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, m_LayerMask)) {
                return;
            }

            var characterLocomotion = other.gameObject.GetCachedParentComponent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null || characterLocomotion.TimeScale != 1) {
                return;
            }

            m_CharacterLocomotion = characterLocomotion;
            m_Active = true;
            m_Time = 0;
            UpdateTimeScale(true);

            // Hide the children until the time has been reset.
            for (int i = 0; i < m_Children.Length; ++i) {
                m_Children[i].SetActive(false);
            }

            // Optionally play an audio clip.
            if (m_AudioClip != null) {
                m_AudioSource.clip = m_AudioClip;
                m_AudioSource.Play();
            }
        }

        /// <summary>
        /// Updates the timescale according to the specified change speed.
        /// </summary>
        /// <param name="activate">Has the timescale been activated?</param>
        private void UpdateTimeScale(bool activate)
        {
            m_Time += m_TimeScaleChangeSpeed;
            var startTime = activate ? 1 : m_TimeScale;
            var endTime = activate ? m_TimeScale : 1;
            m_CharacterLocomotion.TimeScale = Mathf.SmoothStep(startTime, endTime, m_Time);
            if (m_Time > 1) {
                m_Time = 0;
                if (m_Active) {
                    // Reset the time after the duration.
                    m_Active = false;
                    m_Time = 0;
                    Scheduler.Schedule(m_Duration, UpdateTimeScale, !activate);
                } else {
                    for (int i = 0; i < m_Children.Length; ++i) {
                        m_Children[i].SetActive(true);
                    }
                }
            } else {
                // Keep updating the timescale until the time is 1.
                Scheduler.Schedule(0.05f, UpdateTimeScale, activate);
            }
        }
    }
}