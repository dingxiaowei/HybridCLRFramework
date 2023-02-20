/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.Audio;

namespace Opsive.UltimateCharacterController.Objects.CharacterAssist
{
    /// <summary>
    /// Sets a bool parameter value when interacted with (implements IInteractableTarget).
    /// </summary>
    public class AnimatedInteractable : MonoBehaviour, IInteractableTarget, IInteractableMessage
    {
        [Tooltip("Can the interactable be interacted with only once?")]
        [SerializeField] protected bool m_SingleInteract;
        [Tooltip("The bool parameter name that should be changed when interacted with. Can be empty.")]
        [SerializeField] protected string m_BoolParameter;
        [Tooltip("The value to set the bool to when interacted with. Only used if the bool parameter is not empty.")]
        [SerializeField] protected bool m_BoolInteractValue = true;
        [Tooltip("The UI message that should be displayed when the bool is enabled.")]
        [SerializeField] protected string m_BoolEnabledMessage;
        [Tooltip("The UI message that should be displayed when the bool is disabled.")]
        [SerializeField] protected string m_BoolDisabledMessage;
        [Tooltip("Should the bool interact value be toggled after an interact?")]
        [SerializeField] protected bool m_ToggleBoolInteractValue = true;
        [Tooltip("The trigger parameter name that should be set when interacted with. Can be empty.")]
        [SerializeField] protected string m_TriggerParameter;
        [Tooltip("An array of audio clips that can be played when the interaction starts.")]
        [SerializeField] protected AudioClip[] m_InteractAudioClips;

        private GameObject m_GameObject;
        private Animator m_Animator;
        private AnimatedInteractable[] m_AnimatedInteractables;
        private bool m_HasInteracted;
        private int m_BoolParameterHash;
        private int m_TriggerParameterHash;
        private int m_AudioClipIndex = -1;
        private bool m_ActiveBoolInteractable;

        public bool ActiveBoolInteractable { get { return m_ActiveBoolInteractable; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_GameObject = gameObject;
            m_Animator = GetComponent<Animator>();
            if (!string.IsNullOrEmpty(m_BoolParameter)) {
                m_BoolParameterHash = Animator.StringToHash(m_BoolParameter);
            }
            if (!string.IsNullOrEmpty(m_TriggerParameter)) {
                m_TriggerParameterHash = Animator.StringToHash(m_TriggerParameter);
            }
            var animatedInteractables = GetComponents<AnimatedInteractable>();
            if (animatedInteractables.Length > 1) {
                m_AnimatedInteractables = new AnimatedInteractable[animatedInteractables.Length - 1];
                var count = 0;
                for (int i = 0; i < animatedInteractables.Length; ++i) {
                    if (animatedInteractables[i] == this) {
                        continue;
                    }

                    m_AnimatedInteractables[count] = animatedInteractables[i];
                }
            }

        }

        /// <summary>
        /// Can the target be interacted with?
        /// </summary>
        /// <param name="character">The character that wants to interactact with the target.</param>
        /// <returns>True if the target can be interacted with.</returns>
        public bool CanInteract(GameObject character)
        {
            return !m_HasInteracted || !m_SingleInteract;
        }

        /// <summary>
        /// Interact with the target.
        /// </summary>
        /// <param name="character">The character that wants to interactact with the target.</param>
        public void Interact(GameObject character)
        {
            if (m_BoolParameterHash != 0) {
                // If the bool value can be toggled then there's a chance that another AnimatedInteractable is currently active. In that case the original
                // AnimatedInteractable should respond to the interact event.
                if (m_AnimatedInteractables != null) {
                    for (int i = 0; i < m_AnimatedInteractables.Length; ++i) {
                        if (m_AnimatedInteractables[i].ActiveBoolInteractable) {
                            m_AnimatedInteractables[i].Interact(character);
                            return;
                        }
                    }
                }

                m_Animator.SetBool(m_BoolParameterHash, m_BoolInteractValue);
                if (m_ToggleBoolInteractValue) {
                    m_BoolInteractValue = !m_BoolInteractValue;
                    m_ActiveBoolInteractable = !m_ActiveBoolInteractable;
                }
            }
            if (m_TriggerParameterHash != 0) {
                m_Animator.SetTrigger(m_TriggerParameterHash);
            }

            if (m_InteractAudioClips.Length > 0) {
                // Sequentually switch between audio clips.
                m_AudioClipIndex = (m_AudioClipIndex + 1) % m_InteractAudioClips.Length;
                AudioManager.Play(m_GameObject, m_InteractAudioClips[m_AudioClipIndex]);
            }
            m_HasInteracted = true;
        }

        /// <summary>
        /// Resets the interact variables.
        /// </summary>
        public void ResetInteract()
        {
            m_HasInteracted = false;
            m_AudioClipIndex = -1;
            m_Animator.Rebind();
        }

        /// <summary>
        /// Returns the message that should be displayed when the object can be interacted with.
        /// </summary>
        /// <returns>The message that should be displayed when the object can be interacted with.</returns>
        public string AbilityMessage()
        {
            if (m_BoolParameterHash != 0) {
                // If the bool value can be toggled then there's a chance that another AnimatedInteractable is currently active. In that case the original
                // AnimatedInteractable should respond to the message.
                if (m_AnimatedInteractables != null) {
                    for (int i = 0; i < m_AnimatedInteractables.Length; ++i) {
                        if (m_AnimatedInteractables[i].ActiveBoolInteractable) {
                            return m_AnimatedInteractables[i].AbilityMessage();
                        }
                    }
                }

                return m_BoolInteractValue ? m_BoolEnabledMessage : m_BoolDisabledMessage;
            }
            return string.Empty;
        }
    }
}