/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Environment
{
    using UnityEngine;

    /// <summary>
    /// Ping Pong Scale.
    /// </summary>
    public class PingPongScale : MonoBehaviour
    {
        [Tooltip("Offset translation.")]
        [SerializeField] protected Vector3 m_Offset = new Vector3(0,1,0);
        [Tooltip("Offset translation.")]
        [SerializeField] protected float m_Speed = 1;
        [Tooltip("The object to translate.")]
        [SerializeField] protected Transform m_ObjectTransform;

        protected Vector3 m_StarScale;
        
        /// <summary>
        /// Cache components.
        /// </summary>
        public void Awake()
        {
            if (m_ObjectTransform == null) {
                m_ObjectTransform = transform;
            }
            m_StarScale = m_ObjectTransform.localScale;
        }

        /// <summary>
        /// Set the scale.
        /// </summary>
        private void Update()
        {
            m_ObjectTransform.localScale = m_StarScale + m_Offset*Mathf.Abs(Mathf.Sin(m_Speed*Time.time));
        }

        /// <summary>
        /// Disable the component.
        /// </summary>
        private void OnDisable()
        {
            if(m_ObjectTransform == null){ return; }
            
            m_ObjectTransform.localScale = m_StarScale;
        }
    }
}