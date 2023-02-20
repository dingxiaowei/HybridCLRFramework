/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using Opsive.UltimateCharacterController.Events;

namespace Opsive.UltimateCharacterController.UI
{
    /// <summary>
    /// The HealthFlashMonitor will show a full screen flash when the character takes damage or is healed.
    /// </summary>
    public class HealthFlashMonitor : CharacterMonitor
    {
        /// <summary>
        /// Stores the flash settings.
        /// </summary>
        [System.Serializable]
        public struct Flash
        {
            [Tooltip("Can the flash be activated?")]
            [SerializeField] private bool m_CanActivate;
            [Tooltip("The amount of time the flash should be fully visible for.")]
            [SerializeField] private float m_VisiblityDuration;
            [Tooltip("The amount of time it takes the flash UI to fade.")]
            [SerializeField] private float m_FadeDuration;
            [Tooltip("The color of the image flash.")]
            [SerializeField] private Color m_Color;
            [Tooltip("The image of the flash.")]
            [SerializeField] private Sprite m_Sprite;

            public bool CanActivate { get { return m_CanActivate; } }
            public float VisiblityDuration { get { return m_VisiblityDuration; } }
            public float FadeDuration { get { return m_FadeDuration; } }
            public Color Color { get { return m_Color; } }
            public Sprite Sprite { get { return m_Sprite; } }

            /// <summary>
            /// Constructor for the flash struct.
            /// </summary>
            /// <param name="canActivate">Can the flash be activated?</param>
            /// <param name="color">The amount of time the flash should be fully visible for.</param>
            /// <param name="visibilityDuration">The amount of time it takes the flash UI to fade.</param>
            /// <param name="fadeDuration">The amount of time it takes the flash UI to fade.</param>
            public Flash(bool canActivate, Color color, float visibilityDuration, float fadeDuration)
            {
                m_CanActivate = canActivate;
                m_Color = color;
                m_VisiblityDuration = visibilityDuration;
                m_FadeDuration = fadeDuration;
                m_Sprite = null;
            }
        }

        [Tooltip("The flash when the character is damaged.")]
        [SerializeField] protected Flash m_DamageFlash = new Flash(true, new Color(1, 0, 0, 0.7f), 1.5f, 1f);
        [Tooltip("The flash when the character is healed.")]
        [SerializeField] protected Flash m_HealFlash = new Flash(true, new Color(1, 1, 1, 0.2f), 0.05f, 0.2f);

        public Flash DamageFlash { get { return m_DamageFlash; } set { m_DamageFlash = value; } }
        public Flash HealFlash { get { return m_HealFlash; } set { m_HealFlash = value; } }

        private GameObject m_GameObject;
        private Image m_FlashImage;
        private float m_FlashDisplayTime;
        private Flash m_ActiveFlash;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_FlashImage = GetComponentInChildren<Image>();
            if (m_FlashImage == null) {
                Debug.LogError("Error: Unable to find an Image component for the damage flash. Disabling.");
                return;
            }

            m_FlashImage.color = Color.clear;
            m_GameObject = gameObject;
            m_GameObject.SetActive(false);
        }

        /// <summary>
        /// Attaches the monitor to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the monitor to.</param>
        protected override void OnAttachCharacter(GameObject character)
        {
            if (m_Character != null) {
                EventHandler.UnregisterEvent<float, Vector3, Vector3, GameObject, Collider>(m_Character, "OnHealthDamage", OnDamage);
                EventHandler.UnregisterEvent<float>(m_Character, "OnHealthHeal", OnHeal);
            }

            base.OnAttachCharacter(character);

            if (m_Character == null || m_GameObject == null) {
                return;
            }

            EventHandler.RegisterEvent<float, Vector3, Vector3, GameObject, Collider>(m_Character, "OnHealthDamage", OnDamage);
            EventHandler.RegisterEvent<float>(m_Character, "OnHealthHeal", OnHeal);
            m_GameObject.SetActive(false);
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
            if (!m_DamageFlash.CanActivate) {
                return;
            }

            // Show the flash image.
            m_ActiveFlash = m_DamageFlash;
            m_FlashImage.color = m_DamageFlash.Color;
            m_FlashImage.sprite = m_DamageFlash.Sprite;
            m_FlashDisplayTime = Time.time;

            // Allow the flash to fade.
            m_GameObject.SetActive(true);
        }

        /// <summary>
        /// The object has healed.
        /// </summary>
        /// <param name="amount">The amount that the object was healed by.</param>
        private void OnHeal(float amount)
        {
            if (!m_HealFlash.CanActivate) {
                return;
            }

            // Show the flash image.
            m_ActiveFlash = m_HealFlash;
            m_FlashImage.color = m_HealFlash.Color;
            m_FlashImage.sprite = m_HealFlash.Sprite;
            m_FlashDisplayTime = Time.time;

            // Allow the flash to fade.
            m_GameObject.SetActive(true);
        }

        /// <summary>
        /// Fade the flash. 
        /// </summary>
        private void Update()
        {
            // Update the flash alpha.
            var alpha = Mathf.Min((m_ActiveFlash.FadeDuration - (Time.time - (m_FlashDisplayTime + m_ActiveFlash.VisiblityDuration))) / m_ActiveFlash.FadeDuration, 1) * m_ActiveFlash.Color.a;
            var color = m_FlashImage.color;
            color.a = alpha;
            m_FlashImage.color = color;

            if (alpha <= 0) {
                m_GameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Can the UI be shown?
        /// </summary>
        /// <returns>True if the UI can be shown.</returns>
        protected override bool CanShowUI()
        {
            return m_FlashImage.color.a > 0;
        }
    }
}