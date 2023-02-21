/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Interactions
{
    using UnityEngine;

    /// <summary>
    /// An abstract class for interactable behaviors.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class InteractableBehavior : MonoBehaviour
    {
        [SerializeField] protected bool m_DeactivateOnInteract = true;
        [SerializeField] protected GameObject[] m_SelectIndicators;
    
        protected Interactable m_Interactable;
        protected bool m_Selected;

        public Interactable Interactable => m_Interactable;

        /// <summary>
        /// Initialize.
        /// </summary>
        protected virtual void Awake()
        {
            m_Interactable = GetComponent<Interactable>();
        }
    
        /// <summary>
        /// Set the listeners. 
        /// </summary>
        protected virtual void Start()
        {
            m_Interactable.OnInteract += OnInteract;
            m_Interactable.OnSelect += OnSelect;
            m_Interactable.OnUnselect += OnUnselect;
            OnUnselect(null);

            Activate();
        }
    
        /// <summary>
        /// Enable the object and set interactable.
        /// </summary>
        public virtual void Activate()
        {
            gameObject.SetActive(true);
            m_Interactable.SetIsInteractable(true);
        }
    
        /// <summary>
        /// The event called when the interactable is selected by an interactor.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        public virtual void OnSelect(IInteractor interactor)
        {
            m_Selected = true;
            for (int i = 0; i < m_SelectIndicators.Length; i++) { m_SelectIndicators[i].SetActive(true); }

        }

        /// <summary>
        /// The event when the interactable is no longer selected.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        public virtual void OnUnselect(IInteractor interactor)
        {
            m_Selected = false;
            for (int i = 0; i < m_SelectIndicators.Length; i++) { m_SelectIndicators[i].SetActive(false); }

        }

        /// <summary>
        /// The event when the interactable is interacted with by an interactor.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        public virtual void OnInteract(IInteractor interactor)
        {
            if (!m_DeactivateOnInteract) { return; }
            interactor.RemoveInteractable(m_Interactable);
            Deactivate();
        }

        /// <summary>
        /// Deactivate the component.
        /// </summary>
        public virtual void Deactivate()
        {
            gameObject.SetActive(false);
            m_Interactable.SetIsInteractable(false);
        }

        /// <summary>
        /// Remove the listeners.
        /// </summary>
        protected virtual void OnDestroy()
        {
            m_Interactable.OnInteract -= OnInteract;
            m_Interactable.OnSelect -= OnSelect;
            m_Interactable.OnUnselect -= OnUnselect;
        }
    }
}