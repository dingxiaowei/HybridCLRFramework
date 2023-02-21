/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Game
{
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// Trigger an event when X number of enemies are killed.
    /// </summary>
    public class EventOnXEnemyKills : MonoBehaviour
    {
        [Tooltip("The enemy kill count threshold.")]
        [SerializeField] protected int m_EnemyKillCount;
        [Tooltip("The event to call when the kill count reaches that amount.")]
        [SerializeField] protected UnityEvent m_Event;

        /// <summary>
        /// Listen to the event.
        /// </summary>
        private void Awake()
        {
            GameManager.Instance.EnemyDiedE += EnemyDied;
        }

        /// <summary>
        /// Check if the kill count matches and call the event if it does.
        /// </summary>
        /// <param name="killCount"></param>
        private void EnemyDied(int killCount)
        {
            if (killCount == m_EnemyKillCount) {
                m_Event.Invoke();
            }
        }
    }
}