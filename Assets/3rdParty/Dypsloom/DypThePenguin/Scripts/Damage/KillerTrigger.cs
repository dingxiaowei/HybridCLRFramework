/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Damage
{
    using Dypsloom.Shared.Utility;
    using UnityEngine;

    /// <summary>
    /// Kills any damageable that enters its trigger.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class KillerTrigger : MonoBehaviour
    {
        [Tooltip("The layers affected by the trigger.")]
        [SerializeField] protected LayerMask m_LayerMask;

        /// <summary>
        /// Trigger on enter.
        /// </summary>
        /// <param name="other">The other collider.</param>
        private void OnTriggerEnter(Collider other)
        {
            if(other.isTrigger){return;}
            if(m_LayerMask.Contains(other) == false){return;}
            
            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null) {
                damageable.Die();
            }
        }
    }
}