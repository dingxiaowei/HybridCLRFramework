/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Character
{
    using UnityEngine;

    /// <summary>
    /// Impulse Force mover used to create knock back effects for example.
    /// </summary>
    public class ImpulseForceMover : IMover
    {
        protected Vector3 m_Movement;
        protected Vector3 m_ImpulseForce;
        protected IParentMover m_Parent;
        protected float m_Resistance;
        
        public Vector3 Movement => m_Movement;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="force">The force to apply.</param>
        /// <param name="resistance">The base resistance amount, must be above 1.01f.</param>
        public ImpulseForceMover(Vector3 force, float resistance = 1.01f)
        {
            m_ImpulseForce = force*5f;
            m_Resistance = Mathf.Max(resistance,1.01f);
            m_Movement = m_ImpulseForce;
        }

        /// <summary>
        /// Update every frame.
        /// </summary>
        public virtual void Tick()
        {
            m_Movement = m_Movement/m_Resistance;
            if (m_Movement.sqrMagnitude < 0.01f) {
                m_Parent.RemoveExternalMover(this);
            }
        }

        /// <summary>
        /// Set the parent mover, if it is a character it will use its rigidbody mass as resistance.
        /// </summary>
        /// <param name="parent">The parent mover.</param>
        public virtual void SetParentMover(IParentMover parent)
        {
            m_Parent = parent;
            if (parent is CharacterMover characterMover) {
                m_Resistance = Mathf.Max(1.01f,characterMover.Character.Rigidbody.mass * m_Resistance);
            }
        }
    }
}