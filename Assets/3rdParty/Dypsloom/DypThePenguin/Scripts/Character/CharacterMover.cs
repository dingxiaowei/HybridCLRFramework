/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Character
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Character Mover used to move a character using a character controller.
    /// </summary>
    public class CharacterMover : ICharacterMover
    {
        protected readonly Character m_Character;
        protected readonly CharacterController m_CharacterController;
        protected readonly List<IMover> m_ExternalMovers;
        protected Vector3 m_CharacterInputMovement;

        protected Vector3 m_Movement;
        protected JumpForceMover m_JumpForceMover;
        private Vector3 m_GravityMovement;
        private bool m_WasGrounded;
        protected bool m_IsJumping;

        public Character Character => m_Character;
        public Vector3 Movement => m_Movement;

        public Vector3 CharacterInputMovement => m_CharacterInputMovement;
        public bool IsJumping => m_IsJumping;

        protected const float c_StartFallGravity = -5f;
        protected const float c_StickyGravity = -0.3f;
        protected const float c_NoVerticalMovementOffset = 0.01f;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="character"></param>
        public CharacterMover(Character character)
        {
            m_Character = character;
            m_CharacterController = m_Character.CharacterController;
            m_ExternalMovers = new List<IMover>();
            m_JumpForceMover = new JumpForceMover();

            m_Character.OnDie += () => { m_ExternalMovers.Clear(); };
        }

        /// <summary>
        /// Update tick every frame.
        /// </summary>
        public void Tick()
        {
            if (m_Character.IsGrounded)
            {
                if (m_IsJumping)
                {
                    RemoveExternalMover(m_JumpForceMover);
                    m_IsJumping = false;
                }

                if (m_Character.CharacterInput.Jump)
                {
                    m_Character.IsGrounded = false;
                    m_IsJumping = true;
                    m_JumpForceMover.StartJump(m_Character.JumpForce, m_Character.JumpFallOff);
                    AddExternalMover(m_JumpForceMover);
                }

                if (m_Movement.y > c_NoVerticalMovementOffset) { m_GravityMovement = new Vector3(0, 0f, 0); }
                else
                {
                    m_GravityMovement = new Vector3(0, c_StickyGravity, 0);
                }

                m_WasGrounded = true;
            }
            else
            {
                if (m_WasGrounded) { m_GravityMovement = new Vector3(0, c_StartFallGravity, 0); }

                m_GravityMovement.y -= m_Character.Gravity * Time.deltaTime;
                m_WasGrounded = false;
            }

            var movementInput = m_Character.IsDead ? Vector3.zero : new Vector3(m_Character.CharacterInput.Horizontal, 0, m_Character.CharacterInput.Vertical);
            var movementRelativeCamera = m_Character.CharacterCamera.transform.TransformDirection(movementInput);
            movementRelativeCamera = new Vector3(movementRelativeCamera.x, 0, movementRelativeCamera.z).normalized;

            m_CharacterInputMovement = m_Character.Speed * movementRelativeCamera;

            Vector3 externalMovement = Vector3.zero;
            for (int i = m_ExternalMovers.Count - 1; i >= 0; i--)
            {
                externalMovement += m_ExternalMovers[i].Movement;
                m_ExternalMovers[i].Tick();
            }

            m_Movement = m_CharacterInputMovement + externalMovement + m_GravityMovement;

            m_CharacterController.Move(m_Movement * Time.deltaTime);
        }

        /// <summary>
        /// Add an external mover.
        /// </summary>
        /// <param name="mover">The external mover to add.</param>
        public void AddExternalMover(IMover mover)
        {
            if (mover == null || m_ExternalMovers.Contains(mover)) { return; }

            mover.SetParentMover(this);
            m_ExternalMovers.Add(mover);
        }

        /// <summary>
        /// Remove an external mover.
        /// </summary>
        /// <param name="mover">The external mover to remove.</param>
        public void RemoveExternalMover(IMover mover)
        {
            if (mover == null || !m_ExternalMovers.Contains(mover)) { return; }

            mover.SetParentMover(null);
            m_ExternalMovers.Remove(mover);
        }

        /// <summary>
        /// Do not do anything.
        /// </summary>
        /// <param name="parent">The parent mover.</param>
        public void SetParentMover(IParentMover parent)
        {
            //Nothing
        }
    }
}