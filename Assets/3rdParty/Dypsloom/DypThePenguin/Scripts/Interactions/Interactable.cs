/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Interactions
{
    using Dypsloom.Shared.Utility;
    using System;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// The interactable class allows you to easily select, unselect and interact with an object.
    /// </summary>
    public class Interactable : MonoBehaviour, IInteractable
    {
        public event Action<IInteractor> OnInteract;
        public event Action<IInteractor>  OnSelect;
        public event Action<IInteractor>  OnUnselect;
        
        [Tooltip("The layers that can interact with the interactable.")]
        [SerializeField] protected LayerMask m_InteractorLayerMask = -1;
        [Tooltip("Is the interactable interactable?")]
        [SerializeField] protected bool m_IsInteractable = true;
        [Tooltip("The event called on interact.")]
        [SerializeField] protected UnityEvent m_OnInteract;
        [Tooltip("The event called on select.")]
        [SerializeField] protected UnityEvent m_OnSelect;
        [Tooltip("The event called on unselect.")]
        [SerializeField] protected UnityEvent m_OnUnselect;

        public virtual bool IsInteractable => true;

        /// <summary>
        /// On trigger enter.
        /// </summary>
        /// <param name="other">The other collider.</param>
        protected virtual void OnTriggerEnter(Collider other)
        {
            OnTriggerEnterInternal(other.gameObject);
        }

        /// <summary>
        /// On trigger exit.
        /// </summary>
        /// <param name="other">The other collider.</param>
        protected virtual void OnTriggerExit(Collider other)
        {
            OnTriggerExitInternal(other.gameObject);
        }
        
        /// <summary>
        /// On trigger enter internal.
        /// </summary>
        /// <param name="other">The other game object.</param>
        protected virtual void OnTriggerEnterInternal(GameObject other)
        {
            if (!m_IsInteractable || !m_InteractorLayerMask.Contains(other)) { return; }

            var interactor = other.GetComponentInParent<IInteractor>();
            interactor?.AddInteractable(this);
        }

        /// <summary>
        /// On trigger exit internal.
        /// </summary>
        /// <param name="other">The other game object.</param>
        protected virtual void OnTriggerExitInternal(GameObject other)
        {
            if (!m_InteractorLayerMask.Contains(other)) { return; }

            var interactor = other.GetComponentInParent<IInteractor>();
            interactor?.RemoveInteractable(this);
        }
        
        /// <summary>
        /// Select the interactable.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        /// <returns>False if not interactable.</returns>
        public virtual bool Select(IInteractor interactor)
        {
            if (!IsInteractable) { return false;}

            m_OnSelect.Invoke();
            OnSelect?.Invoke(interactor);
            return true;
        }
        
        /// <summary>
        /// Unselect the interactable.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        /// <returns>False if not interactable.</returns>
        public virtual bool Unselect(IInteractor interactor)
        {
            if (!IsInteractable) { return false;}

            m_OnUnselect.Invoke();
            OnUnselect?.Invoke(interactor);
            return true;
        }

        /// <summary>
        /// Interact with the interactable.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        /// <returns>False if not interactable.</returns>
        public virtual bool Interact(IInteractor interactor)
        {
            if (!IsInteractable) { return false;}

            m_OnInteract.Invoke();
            OnInteract?.Invoke(interactor);
            return true;
        }

        /// <summary>
        /// Set if the interactable is interactable.
        /// </summary>
        /// <param name="isInteractable">Is it interactable.</param>
        public virtual void SetIsInteractable(bool isInteractable)
        {
            m_IsInteractable = isInteractable;
        }
    }
}