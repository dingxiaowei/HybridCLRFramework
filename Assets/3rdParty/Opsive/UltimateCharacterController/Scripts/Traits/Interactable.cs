/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Traits
{
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.Shared.Game;
#endif
    using Opsive.UltimateCharacterController.Objects.CharacterAssist;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.UltimateCharacterController.Networking;
    using Opsive.UltimateCharacterController.Networking.Traits;
#endif
    using UnityEngine;

    /// <summary>
    /// Represents any object that can be interacted with by the character. Acts as a link between the character and IInteractableTarget.
    /// </summary>
    public class Interactable : MonoBehaviour
    {
        [Tooltip("The ID of the Interactable, used by the Interact ability for filtering. A value of -1 indicates no ID.")]
        [SerializeField] protected int m_ID = -1;
        [Tooltip("The object(s) that the interaction is performend on. This component must implement the IInteractableTarget.")]
        [SerializeField] protected MonoBehaviour[] m_Targets;

        public int ID { get { return m_ID; } set { m_ID = value; } }
        public MonoBehaviour[] Targets { get { return m_Targets; } set { m_Targets = value; } }

        private IInteractableTarget[] m_InteractableTargets;
        private AbilityIKTarget[] m_IKTargets;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        private INetworkInteractableMonitor m_NetworkInteractable;
#endif

        public AbilityIKTarget[] IKTargets { get { return m_IKTargets; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            if (m_Targets == null || m_Targets.Length == 0) {
                Debug.LogError("Error: An IInteractableTarget must be specified in the Targets field.");
                return;
            }

            m_InteractableTargets = new IInteractableTarget[m_Targets.Length];
            for (int i = 0; i < m_Targets.Length; ++i) {
                if (m_Targets[i] == null || !(m_Targets[i] is IInteractableTarget)) {
                    Debug.LogError("Error: element " + i + " of the Targets array is null or does not subscribe to the IInteractableTarget iterface.");
                } else {
                    m_InteractableTargets[i] = m_Targets[i] as IInteractableTarget;
                }
            }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            m_NetworkInteractable = gameObject.GetCachedComponent<INetworkInteractableMonitor>();
#endif

            m_IKTargets = GetComponentsInChildren<AbilityIKTarget>();
        }

        /// <summary>
        /// Determines if the character can interact with the InteractableTarget.
        /// </summary>
        /// <param name="character">The character that wants to interactact with the target.</param>
        /// <returns>True if the character can interact with the InteractableTarget</returns>
        public virtual bool CanInteract(GameObject character)
        {
            for (int i = 0; i < m_InteractableTargets.Length; ++i) {
                if (m_InteractableTargets[i] == null || !m_InteractableTargets[i].CanInteract(character)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Performs the interaction.
        /// </summary>
        /// <param name="character">The character that wants to interactact with the target.</param>
        public virtual void Interact(GameObject character)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            var characterNetworkInfo = character.GetCachedComponent<INetworkInfo>();
            if (characterNetworkInfo != null && characterNetworkInfo.IsLocalPlayer()) {
#if UNITY_EDITOR
                if (m_NetworkInteractable == null) {
                    Debug.LogError("Error: The object " + gameObject.name + " must have a NetworkInteractable component.");
                }
#endif
                m_NetworkInteractable.Interact(character);
            }
#endif

            for (int i = 0; i < m_InteractableTargets.Length; ++i) {
                m_InteractableTargets[i].Interact(character);
            }
        }

        /// <summary>
        /// Returns the message that should be displayed when the object can be interacted with.
        /// </summary>
        /// <returns>The message that should be displayed when the object can be interacted with.</returns>
        public string AbilityMessage()
        {
            if (m_InteractableTargets != null) {
                for (int i = 0; i < m_InteractableTargets.Length; ++i) {
                    // Returns the message from the first IInteractableMessage object.
                    if (m_InteractableTargets[i] is IInteractableMessage) {
                        return (m_InteractableTargets[i] as IInteractableMessage).AbilityMessage();
                    }
                }
            }
            return string.Empty;
        }
    }
}