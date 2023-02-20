/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Items.AnimatorAudioStates
{
    /// <summary>
    /// The Sequence state will move from one state to the in a sequence order.
    /// </summary>
    public class Sequence : AnimatorAudioStateSelector
    {
        [Tooltip("Resets the index back to the start after the specified delay. Set to -1 to never reset.")]
        [SerializeField] protected float m_ResetDelay = -1;

        public float ResetDelay { get { return m_ResetDelay; } set { m_ResetDelay = value; } }

        private int m_CurrentIndex = -1;
        private float m_EndTime = -1;

        /// <summary>
        /// Starts or stops the state selection.
        /// </summary>
        /// <param name="start">Is the object starting?</param>
        public override void StartStopStateSelection(bool start)
        {
            base.StartStopStateSelection(start);

            // The Sequence task can reset which index is returned if the next state is selected too slowly. 
            if (start && m_ResetDelay != -1 && m_EndTime != -1 && m_EndTime + m_ResetDelay < Time.time) {
                m_CurrentIndex = -1;
            } else if (!start) {
                m_EndTime = Time.time;
            }
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
                m_CurrentIndex = (m_CurrentIndex + 1) % size;
                count++;
            } while ((!IsStateValid(m_CurrentIndex) || !m_States[m_CurrentIndex].Enabled) && count <= size);
            return count <= size;
        }
    }
}