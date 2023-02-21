/// ---------------------------------------------
/// Dyp Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.UI
{
    using Dypsloom.DypThePenguin.Scripts.Damage;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Health Monitor.
    /// </summary>
    public class HealthMonitor : MonoBehaviour
    {
        [Tooltip("The health slider.")]
        [SerializeField] protected Slider m_Slider;
        [Tooltip("The health text.")]
        [SerializeField] protected TextMeshProUGUI m_Text;
        [Tooltip("The damageable to monitor.")]
        [SerializeField] protected Damageable m_StartDamageable;

        protected IDamageable m_Damageable;

        /// <summary>
        /// Cache components.
        /// </summary>
        private void Awake()
        {
            m_Damageable = m_StartDamageable;
        }

        /// <summary>
        /// Set a listener.
        /// </summary>
        private void OnEnable()
        {
            SetListener(true);
        }

        /// <summary>
        /// Listen to the damageable.
        /// </summary>
        /// <param name="listen">listen?</param>
        protected void SetListener(bool listen)
        {
            if(m_Damageable == null){return;}
            if (listen) {
                m_Damageable.OnHpChanged += HealthChanged;
            } else {
                m_Damageable.OnHpChanged -= HealthChanged;
            }
        }

        /// <summary>
        /// Set the damageable to monitor.
        /// </summary>
        /// <param name="damageable">The damageable.</param>
        public void SetDamageable(IDamageable damageable)
        {
            SetListener(false);

            m_Damageable = damageable;
        
            SetListener(true);
        }

        /// <summary>
        /// The health amount changed.
        /// </summary>
        private void HealthChanged()
        {
            if (m_Slider != null) {
                m_Slider.value = (float)m_Damageable.CurrentHp / m_Damageable.MaxHp;
            }
            
            if (m_Text != null) {
                m_Text.text = m_Damageable.CurrentHp + "/" + m_Damageable.MaxHp;
            }
        }
    
        /// <summary>
        /// Remove the listener when disabled.
        /// </summary>
        private void OnDisable()
        {
            SetListener(false);
        }
    }
}
