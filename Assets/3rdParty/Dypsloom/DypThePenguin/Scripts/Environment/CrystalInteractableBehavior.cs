/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Environment
{
    using Dypsloom.DypThePenguin.Scripts.Interactions;
    using Dypsloom.Shared.Utility;
    using UnityEngine;

    public class CrystalInteractableBehavior : InteractableBehavior
    {
        [Tooltip("Offset translation.")]
        [SerializeField] protected Vector3 m_Offset = new Vector3(0,1,0);
        [Tooltip("The object to translate.")]
        [SerializeField] protected Transform m_ObjectTransform;
        [Tooltip("How smoothly to move the object.")]
        [SerializeField] protected float m_SmoothFactor;
    
        [Tooltip("Rotation speed at idle.")]
        [SerializeField] protected Vector3 m_RotationSpeedIdle = new Vector3(0,1,0);
        [Tooltip("Rotation speed when selected.")]
        [SerializeField] protected Vector3 m_RotationSpeedSelected = new Vector3(0,1,0);
        [Tooltip("Rotation speed at interact.")]
        [SerializeField] protected Vector3 m_RotationSpeedInteract = new Vector3(0,1,0);
    
        [Tooltip("Interact period for rotation speed change.")]
        [SerializeField] protected float m_InteractPeriod = 0.5f;

        protected Vector3 m_StarPosition;
        private float m_Timer;
        protected Vector3 m_RotationSpeed;
        protected bool m_Interacting;

        /// <summary>
        /// Cache components.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
        
            if (m_ObjectTransform == null) {
                m_ObjectTransform = transform;
            }
            m_StarPosition = m_ObjectTransform.localPosition;
            m_RotationSpeed = m_RotationSpeedIdle;
        }

        /// <summary>
        /// Schedule spin.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        public override void OnInteract(IInteractor interactor)
        {
            base.OnInteract(interactor);
            m_Interacting = true;
            SchedulerManager.Schedule(
                ()=>m_Interacting = false,m_InteractPeriod);
        }

        /// <summary>
        /// Spin the crystal.
        /// </summary>
        private void Update()
        {
            if (m_Selected) {
                m_Timer += Time.deltaTime;
            } else { m_Timer = 0; }
        
            var targetPosition = m_StarPosition + m_Offset*Mathf.Abs(Mathf.Sin(m_Timer));

            if (m_Interacting) {
                m_RotationSpeed = m_RotationSpeedInteract;
                targetPosition = m_StarPosition + m_Offset;
            } else {
                m_RotationSpeed = m_Selected ? m_RotationSpeedSelected : m_RotationSpeedIdle;
            }
        
            var targetRotation = Quaternion.Euler(m_RotationSpeed *Time.deltaTime+m_ObjectTransform.localRotation.eulerAngles);

        
            m_ObjectTransform.localPosition = Vector3.Lerp(m_ObjectTransform.localPosition, targetPosition, Time.deltaTime * m_SmoothFactor);

            //m_ObjectTransform.localRotation =  targetRotation;//Quaternion.Slerp(m_ObjectTransform.localRotation, targetRotation, Time.deltaTime * m_SmoothFactor);
            m_ObjectTransform.Rotate(m_RotationSpeed*Time.deltaTime);
        }
    }
}
