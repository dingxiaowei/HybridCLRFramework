/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Game;

namespace Opsive.UltimateCharacterController.Demo
{
    /// <summary>
    /// Indicates that a headshot occured by changing the color of the specified material.
    /// </summary>
    public class HeadshotIndicator : MonoBehaviour
    {
        [Tooltip("The base object that has the Health component.")]
        [SerializeField] protected GameObject m_Base;
        [Tooltip("A reference to the head that indicates a headshot.")]
        [SerializeField] protected Collider m_Head;
        [Tooltip("A reference to the material used by the headshot text that will change colors.")]
        [SerializeField] protected Material m_HeadshotTextMaterial;
        [Tooltip("The emissive color when a headshot is not triggered.")]
        [SerializeField] protected Color m_OriginalColor = Color.white;
        [Tooltip("The emissive color when a headshot is triggered.")]
        [SerializeField] protected Color m_HeadshotColor = Color.red;
        [Tooltip("The emissive color when a headshot is not triggered.")]
        [SerializeField] protected Color m_OriginalEmissiveColor = Color.black;
        [Tooltip("The emissive color when a headshot is triggered.")]
        [SerializeField] protected Color m_HeadshotEmissiveColor = Color.red;
        [Tooltip("The amount of time the text stays the solid headshot color.")]
        [SerializeField] protected float m_SolidTextDuration = 2;
        [Tooltip("The amount of time it takes for the text to fade back to the original color.")]
        [SerializeField] protected float m_FadeTextDuration = 1;
        [Tooltip("The audio clip that should play when a headshot occurs.")]
        [SerializeField] protected AudioClip m_HeadshotClip;

        private int m_Color;
        private int m_EmissiveColorID;
        private float m_StartFadeTime;
        private ScheduledEventBase m_TextColorChangeEvent;
        private AudioSource m_AudioSource;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_AudioSource = GetComponent<AudioSource>();
            m_Color = Shader.PropertyToID("_Color");
            m_EmissiveColorID = Shader.PropertyToID("_EmissionColor");
            m_HeadshotTextMaterial.SetColor(m_Color, m_OriginalColor);
            m_HeadshotTextMaterial.SetColor(m_EmissiveColorID, m_OriginalEmissiveColor);

            EventHandler.RegisterEvent<float, Vector3, Vector3, GameObject, Collider>(m_Base, "OnHealthDamage", OnDamage);
        }

        /// <summary>
        /// The object has taken damage.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="force">The amount of force applied to the object while taking the damage.</param>
        /// <param name="attacker">The GameObject that did the damage.</param>
        /// <param name="hitCollider">The Collider that was hit.</param>
        private void OnDamage(float amount, Vector3 position, Vector3 force, GameObject attacker, Collider hitCollider)
        {
            if (m_Head != hitCollider) {
                return;
            }

            if (m_TextColorChangeEvent != null) {
                Scheduler.Cancel(m_TextColorChangeEvent);
            }

            m_HeadshotTextMaterial.SetColor(m_Color, m_HeadshotColor);
            m_HeadshotTextMaterial.SetColor(m_EmissiveColorID, m_HeadshotEmissiveColor);

            m_StartFadeTime = Time.time + m_SolidTextDuration;
            m_TextColorChangeEvent = Scheduler.Schedule(m_SolidTextDuration, UpdateTextColor);

            if (m_HeadshotClip != null) {
                m_AudioSource.clip = m_HeadshotClip;
                m_AudioSource.Play();
            }
        }

        /// <summary>
        /// Lerp the text color back to the original material emissive color.
        /// </summary>
        private void UpdateTextColor()
        {
            var t = (Time.time - m_StartFadeTime) / m_FadeTextDuration;
            m_HeadshotTextMaterial.SetColor(m_Color, Color.Lerp(m_HeadshotColor, m_OriginalColor, t));
            m_HeadshotTextMaterial.SetColor(m_EmissiveColorID, Color.Lerp(m_HeadshotEmissiveColor, m_OriginalEmissiveColor, t));
            // Keep updating until the text is the original color.
            if (t < 1) {
                m_TextColorChangeEvent = Scheduler.Schedule(0.02f, UpdateTextColor);
            }
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<float, Vector3, Vector3, GameObject, Collider>(m_Base, "OnHealthDamage", OnDamage);
        }
    }
}