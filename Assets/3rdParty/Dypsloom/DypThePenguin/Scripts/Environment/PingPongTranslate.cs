/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Environment
{
    using UnityEngine;

    /// <summary>
    /// Ping Pong Translate.
    /// </summary>
    public class PingPongTranslate : MonoBehaviour
    {
        [Tooltip("Offset translation.")]
        [SerializeField] protected Vector3 m_Offset = new Vector3(0,1,0);
        [Tooltip("The object to translate.")]
        [SerializeField] protected Transform m_ObjectTransform;

        protected Vector3 m_StarPosition;
        
        /// <summary>
        /// Cache components.
        /// </summary>
        public void Awake()
        {
            if (m_ObjectTransform == null) {
                m_ObjectTransform = transform;
            }
            m_StarPosition = m_ObjectTransform.localPosition;
        }

        /// <summary>
        /// Update position.
        /// </summary>
        private void Update()
        {
            m_ObjectTransform.localPosition = m_StarPosition + m_Offset*Mathf.Abs(Mathf.Sin(Time.time));
        }

        /// <summary>
        /// Go back to the start on disable.
        /// </summary>
        private void OnDisable()
        {
            m_ObjectTransform.localPosition = m_StarPosition;
        }
    }
}