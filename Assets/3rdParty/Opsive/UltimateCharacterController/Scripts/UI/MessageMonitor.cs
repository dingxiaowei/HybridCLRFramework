/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Events;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Objects.CharacterAssist;

namespace Opsive.UltimateCharacterController.UI
{
    /// <summary>
    /// The MessageMonitor will update the UI for any external object messages such as an ability being able to start or the character picking up an item.
    /// </summary>
    public class MessageMonitor : CharacterMonitor
    {
        [Tooltip("A reference to the object that will show the message icon.")]
        [SerializeField] protected Image m_Icon;
        [Tooltip("A reference to the object that will show the message text.")]
        [SerializeField] protected Text m_Text;
        [Tooltip("The length of time that the message should be visible for after picking up an object.")]
        [SerializeField] protected float m_ObjectVisiblityDuration = 1.5f;
        [Tooltip("The amount to fade the message after it should no longer be displayed.")]
        [SerializeField] protected float m_ObjectFadeSpeed = 0.05f;

        private GameObject m_GameObject;
        private Ability m_Ability;
        private ObjectPickup m_ObjectPickup;
        private bool m_ShouldFade;
        private float m_ObjectAlphaColor;
        private ScheduledEventBase m_ScheduledFade;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

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
                EventHandler.UnregisterEvent<Ability, bool>(m_Character, "OnAbilityMessageCanStart", OnAbilityCanStart);
                EventHandler.UnregisterEvent<ObjectPickup>(m_Character, "OnObjectPickedUp", OnObjectPickedUp);
            }

            base.OnAttachCharacter(character);

            if (m_Character == null) {
                return;
            }

            EventHandler.RegisterEvent<Ability, bool>(m_Character, "OnAbilityMessageCanStart", OnAbilityCanStart);
            EventHandler.RegisterEvent<ObjectPickup>(m_Character, "OnObjectPickedUp", OnObjectPickedUp);
        }

        /// <summary>
        /// The specified ability can start or no longer stop.
        /// </summary>
        /// <param name="ability">The ability that has changed start status.</param>
        /// <param name="canStart">Can the ability start?</param>
        private void OnAbilityCanStart(Ability ability, bool canStart)
        {
            // Only one ability message can be shown at a time.
            if (m_Ability != null && m_Ability != ability) {
                return;
            }

            if (canStart && (!string.IsNullOrEmpty(ability.AbilityMessageText) || ability.AbilityMessageIcon != null)) {
                m_Ability = ability;
                m_ShouldFade = false;
                if (m_ScheduledFade != null) {
                    Scheduler.Cancel(m_ScheduledFade);
                    m_ScheduledFade = null;
                }
            } else {
                m_Ability = null;
            }
            UpdateMessage();
        }

        /// <summary>
        /// An object has been picked up by the character.
        /// </summary>
        /// <param name="objectPickup">The object that was picked up.</param>
        private void OnObjectPickedUp(ObjectPickup objectPickup)
        {
            if ((!string.IsNullOrEmpty(objectPickup.PickupMessageText) || objectPickup.PickupMessageIcon != null)) {
                m_ObjectPickup = objectPickup;
                m_ShouldFade = true;
                m_ObjectAlphaColor = 1;
                if (m_ShouldFade) {
                    Scheduler.Cancel(m_ScheduledFade);
                }
            } else {
                // Keep showing the previous object message if the new message doesn't have text or icons.
                if (m_ObjectPickup != null && m_ObjectPickup != objectPickup) {
                    return;
                }

                m_ObjectPickup = null;
            }
            UpdateMessage();
        }

        /// <summary>
        /// Updates the text and icon UI.
        /// </summary>
        private void UpdateMessage()
        {
            // Abilities have priority.
            if (m_Text != null) {
                m_Text.text = m_Ability != null ? m_Ability.AbilityMessageText : (m_ObjectPickup != null ? m_ObjectPickup.PickupMessageText : string.Empty);
                m_Text.enabled = !string.IsNullOrEmpty(m_Text.text);
            }
            if (m_Icon != null) {
                m_Icon.sprite = m_Ability != null ? m_Ability.AbilityMessageIcon : (m_ObjectPickup != null ? m_ObjectPickup.PickupMessageIcon : null);
                m_Icon.enabled = m_Icon.sprite != null;
            }
            
            // The message will fade if an object is picked up.
            var messageVisible = m_Ability != null || m_ObjectPickup != null;
            if (messageVisible) {
                if (m_Text != null) {
                    var color = m_Text.color;
                    color.a = 1;
                    m_Text.color = color;
                }
                if (m_Icon != null) {
                    var color = m_Icon.color;
                    color.a = 1;
                    m_Icon.color = color;
                }

                if (m_ShouldFade) {
                    m_ScheduledFade = Scheduler.Schedule(m_ObjectVisiblityDuration, FadeMessage);
                }
            }

            m_GameObject.SetActive(m_ShowUI && (messageVisible || m_ShouldFade));
        }

        /// <summary>
        /// Fades the message according to the fade speed.
        /// </summary>
        private void FadeMessage()
        {
            m_ObjectAlphaColor = Mathf.Max(m_ObjectAlphaColor - m_ObjectFadeSpeed, 0);
            if (m_ObjectAlphaColor == 0) {
                m_GameObject.SetActive(false);
                m_ShouldFade = false;
                m_ScheduledFade = null;
                m_ObjectPickup = null;
                return;
            }

            // Fade the text and icon.
            if (m_Text != null) {
                var color = m_Text.color;
                color.a = m_ObjectAlphaColor;
                m_Text.color = color;
            }
            if (m_Icon) {
                var color = m_Icon.color;
                color.a = m_ObjectAlphaColor;
                m_Icon.color = color;
            }

            // Keep fading until there is nothing left to fade.
            m_ScheduledFade = Scheduler.Schedule(0.01f, FadeMessage);
        }

        /// <summary>
        /// Can the UI be shown?
        /// </summary>
        /// <returns>True if the UI can be shown.</returns>
        protected override bool CanShowUI()
        {
            return m_Ability != null || m_ObjectPickup != null || m_ShouldFade;
        }
    }
}