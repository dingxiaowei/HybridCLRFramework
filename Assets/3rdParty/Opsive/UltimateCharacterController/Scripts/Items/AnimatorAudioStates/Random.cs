/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.UltimateCharacterController.Character;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Items.AnimatorAudioStates
{
    /// <summary>
    /// The Random state will move from one state to another in a random order.
    /// </summary>
    public class Random : AnimatorAudioStateSelector
    {
        private int m_CurrentIndex = -1;

        /// <summary>
        /// Initializes the selector.
        /// </summary>
        /// <param name="gameObject">The GameObject that the state belongs to.</param>
        /// <param name="characterLocomotion">The character that the state bleongs to.</param>
        /// <param name="item">The item that the state belongs to.</param>
        /// <param name="states">The states which are being selected.</param>
        public override void Initialize(GameObject gameObject, UltimateCharacterLocomotion characterLocomotion, Item item, AnimatorAudioStateSet.AnimatorAudioState[] states)
        {
            base.Initialize(gameObject, characterLocomotion, item, states);

            // Call next state so the index will be initialized to a random value.
            NextState();
        }

        /// <summary>
        /// Returns the current state index. -1 indicates this index is not set by the class.
        /// </summary>
        /// <returns>The current state index.</returns>
        public override int GetStateIndex()
        {
            return m_CurrentIndex;
        }

        /// <summary>
        /// Moves to the next state.
        /// </summary>
        /// <returns>Was the state changed successfully?</returns>
        public override bool NextState()
        {
            var count = 0;
            var size = m_States.Length;
            do {
                m_CurrentIndex = UnityEngine.Random.Range(0, size);
                ++count;
            } while ((!IsStateValid(m_CurrentIndex) || !m_States[m_CurrentIndex].Enabled) && count <= size);
            return count <= size;
        }
    }
}