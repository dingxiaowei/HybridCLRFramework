/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Character
{
    using Dypsloom.DypThePenguin.Scripts.Interactions;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Character interactor lets a character interact with interactable components. 
    /// </summary>
    public class CharacterInteractor : MonoBehaviour, ICharacterInteractor
    {
        [Tooltip("The character that will be set as an interactor.")]
        [SerializeField] protected Character m_Character;
        [Tooltip("The interactable indicator is enable when the character can interact with something.")]
        [SerializeField] protected GameObject m_InteractableIndicator;

        protected ICharacterInput m_CharacterInput;
        protected List<IInteractable> m_Interactables;

        protected IInteractable m_SelectedInteractable;

        public Character Character => m_Character;
        
        /// <summary>
        /// Set the interactables.
        /// </summary>
        private void Start()
        {
            m_Interactables = new List<IInteractable>();
            m_CharacterInput = m_Character.CharacterInput;
            m_InteractableIndicator.SetActive(false);
        }

        /// <summary>
        /// Add an interactable.
        /// </summary>
        /// <param name="interactable">An interactable.</param>
        public void AddInteractable(IInteractable interactable)
        {
            if(m_Interactables.Contains(interactable)){return;}
            
            m_Interactables.Add(interactable);
            if (m_Interactables.Count > 0) {
                m_InteractableIndicator.SetActive(true);
            }

            if (m_SelectedInteractable == null) { SetSelectedInteractable(interactable); }
        }
        
        /// <summary>
        /// Remove an interactable.
        /// </summary>
        /// <param name="interactable">The interactable.</param>
        public void RemoveInteractable(IInteractable interactable)
        {
            m_Interactables.Remove(interactable);
            if (m_Interactables.Count == 0) {
                m_InteractableIndicator.SetActive(false);
            }

            if (m_SelectedInteractable == interactable) { RemoveSelectedInteractable(); }
        }

        /// <summary>
        /// Update to interact with an interactable.
        /// </summary>
        public void Update()
        {
            if (m_CharacterInput.Interact && m_Interactables.Count > 0) {
                m_Character.CharacterAnimator.Interact(m_Interactables[0]);
                m_Interactables[0].Interact(this);
            }
        }

        /// <summary>
        /// Set the selected interactable.
        /// </summary>
        /// <param name="interactable">The interactable.</param>
        protected void SetSelectedInteractable(IInteractable interactable)
        {
            m_SelectedInteractable = interactable;
            interactable.Select(this);
        }

        /// <summary>
        /// Remove the selected interactable.
        /// </summary>
        protected void RemoveSelectedInteractable()
        {
            m_SelectedInteractable.Unselect(this);
            
            if (m_Interactables.Count != 0) {
                SetSelectedInteractable(m_Interactables[0]);
            } else {
                m_SelectedInteractable = null;
            }
            
        }
    }
}