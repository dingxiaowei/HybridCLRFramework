/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.AnimatorAudioStates
{
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.StateSystem;
    using UnityEngine;

    /// <summary>
    /// The AnimatorAudioState will return a Item Substate Index parameter based on the object's state. 
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public abstract class AnimatorAudioStateSelector
    {
        protected Item m_Item;
        protected GameObject m_Character;
        protected UltimateCharacterLocomotion m_CharacterLocomotion;
        protected AnimatorAudioStateSet.AnimatorAudioState[] m_States;

        /// <summary>
        /// Initializes the selector.
        /// </summary>
        /// <param name="gameObject">The GameObject that the state belongs to.</param>
        /// <param name="characterLocomotion">The character that the state bleongs to.</param>
        /// <param name="item">The item that the state belongs to.</param>
        /// <param name="states">The states which are being selected.</param>
        public virtual void Initialize(GameObject gameObject, UltimateCharacterLocomotion characterLocomotion, Item item, AnimatorAudioStateSet.AnimatorAudioState[] states)
        {
            m_Item = item;
            m_Character = characterLocomotion.gameObject;
            m_CharacterLocomotion = characterLocomotion;
            m_States = states;
        }

        /// <summary>
        /// Starts or stops the state selection. Will activate or deactivate the state with the name specified within the AnimatorAudioState.
        /// </summary>
        /// <param name="start">Is the object starting?</param>
        public virtual void StartStopStateSelection(bool start)
        {
            // Activate or deactivate the state.
            var stateIndex = GetStateIndex();
            if (stateIndex == -1 || stateIndex >= m_States.Length) {
                return;
            }

            var stateName = m_States[stateIndex].StateName;
            if (string.IsNullOrEmpty(stateName)) {
                return;
            }

            StateManager.SetState(m_Character, stateName, start);
        }

        /// <summary>
        /// Returns the current state index. -1 indicates this index is not set by the class.
        /// </summary>
        /// <returns>The current state index.</returns>
        public virtual int GetStateIndex()
        {
            return -1;
        }

        /// <summary>
        /// Moves to the next state.
        /// </summary>
        /// <returns>Was the state changed successfully?</returns>
        public virtual bool NextState() { return true; }

        /// <summary>
        /// Is the state at the specified index valid?
        /// </summary>
        /// <param name="index">The index to check the state of.</param>
        /// <returns>True if the state at the specified index is valid.</returns>
        protected bool IsStateValid(int index) { return (m_States[index].AllowDuringMovement || !m_CharacterLocomotion.Moving) && 
                                                        (!m_States[index].RequireGrounded || m_CharacterLocomotion.Grounded); }

        /// <summary>
        /// Returns an additional value that should be added to the Item Substate Index.
        /// </summary>
        /// <returns>An additional value that should be added to the Item Substate Index.</returns>
        public virtual int GetAdditionalItemSubstateIndex() { return 0; }

        /// <summary>
        /// The selector has been destroyed.
        /// </summary>
        public virtual void OnDestroy() { }
    }
}