/// ---------------------------------------------
/// Dyp Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Character
{
    using UnityEngine;

    /// <summary>
    /// script used to rotate a character.
    /// </summary>
    public class CharacterRotator
    {
        protected readonly Character m_Character;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="character">The character.</param>
        public CharacterRotator(Character character)
        {
            m_Character = character;
        }
        
        /// <summary>
        /// Rotate the character in respect to the camera.
        /// </summary>
        public virtual void Tick()
        {
            var charVelocity = m_Character.IsDead
                ? Vector2.zero
                : new Vector2( m_Character.CharacterInput.Horizontal, m_Character.CharacterInput.Vertical);
            
            if (Mathf.Abs(charVelocity.x) < 0.1f &&
                Mathf.Abs(charVelocity.y) < 0.1f) {
                return;
            }
            float targetRotation = 
                Mathf.Atan2(charVelocity.x, charVelocity.y) 
                * Mathf.Rad2Deg + m_Character.CharacterCamera.transform.eulerAngles.y;
            
            Quaternion lookAt = Quaternion.Slerp(m_Character.transform.rotation,
                Quaternion.Euler(0,targetRotation,0),
                0.5f);

            m_Character.transform.rotation = lookAt;
        }
    }
}