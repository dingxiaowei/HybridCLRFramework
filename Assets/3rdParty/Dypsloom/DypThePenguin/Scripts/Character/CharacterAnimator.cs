/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Character
{
    using Dypsloom.DypThePenguin.Scripts.Damage;
    using Dypsloom.DypThePenguin.Scripts.Interactions;
    using UnityEngine;

    /// <summary>
    /// This script is used to animate a character.
    /// </summary>
    public class CharacterAnimator : ICharacterAnimator
    {
        protected readonly Character m_Character;
        protected Animator m_Animator;
        
        private static readonly int m_HorizontalSpeedAnimHash = Animator.StringToHash("Horizontal Speed");
        private static readonly int m_VerticalSpeedAnimHash = Animator.StringToHash("Vertical Speed");
        private static readonly int m_GroundedAnimHash = Animator.StringToHash("Grounded");
        private static readonly int m_DamagedAnimHash = Animator.StringToHash("Damaged");
        private static readonly int m_ItemActionIndexAnimHash = Animator.StringToHash("ItemActionIndex");
        private static readonly int m_ItemActionAnimHash = Animator.StringToHash("ItemAction");
        private static readonly int m_ItemAnimHash = Animator.StringToHash("Item");
        private static readonly int m_DieAnimHash = Animator.StringToHash("Die");
        private static readonly int m_InteractAnimHash = Animator.StringToHash("Interact");
        private static readonly int m_EquippedItemAnimHash = Animator.StringToHash("EquippedItem");

        //Item and item action animation ids
        public const int PickAxeItemAnimID = 1;
        public const int PickAxeSwingAnimID = 1;
        
        public const int SnowBallAnimID = 2;
        public const int ThrowSnowballAnimID = 1;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="character">The character.</param>
        public CharacterAnimator(Character character)
        {
            m_Character = character;
            m_Animator = character.Animator;
        }

        /// <summary>
        /// Update each frame.
        /// </summary>
        public virtual void Tick()
        {
            HorizontalMove(m_Character.CharacterMover.CharacterInputMovement.sqrMagnitude);
            VerticalMove(m_Character.CharacterMover.Movement.y);
            Grounded(m_Character.IsGrounded);
        }

        /// <summary>
        /// Move animation.
        /// </summary>
        /// <param name="speed">The move speed.</param>
        public void HorizontalMove(float speed)
        {
            m_Animator.SetFloat(m_HorizontalSpeedAnimHash, speed, 0f,Time.deltaTime);
        }
        
        /// <summary>
        /// Move animation.
        /// </summary>
        /// <param name="speed">The move speed.</param>
        public void VerticalMove(float speed)
        {
            m_Animator.SetFloat(m_VerticalSpeedAnimHash, speed, 0f,Time.deltaTime);
        }
        
        /// <summary>
        /// Grounded.
        /// </summary>
        /// <param name="Grounded">The grounded.</param>
        public void Grounded(bool grounded)
        {
            m_Animator.SetBool(m_GroundedAnimHash, grounded);
        }
        
        /// <summary>
        /// Attack animation.
        /// </summary>
        public void ItemAction(int item, int itemAction)
        {
            m_Animator.SetInteger(m_ItemAnimHash,item);
            m_Animator.SetInteger(m_ItemActionIndexAnimHash,itemAction);
            m_Animator.SetTrigger(m_ItemActionAnimHash);
        }
        
        /// <summary>
        /// Equip animation.
        /// </summary>
        public void EquipWeapon(int item)
        {
            m_Animator.SetInteger(m_EquippedItemAnimHash,item);
        }
        
        /// <summary>
        /// Unequip animation.
        /// </summary>
        public void UnequipWeapon()
        {
            m_Animator.SetInteger(m_EquippedItemAnimHash,-1);
        }
        
        /// <summary>
        /// Play the death animation or the respawn animation.
        /// </summary>
        /// <param name="dead">Is the character dead?</param>
        public void Die(bool dead)
        {
            m_Animator.SetBool(m_DieAnimHash,dead);
        }
        
        /// <summary>
        /// Damage animation.
        /// </summary>
        /// <param name="damage">The damage information.</param>
        public void Damaged(Damage damage)
        {
            m_Animator.SetTrigger(m_DamagedAnimHash);
        }
        
        /// <summary>
        /// Interact animation.
        /// </summary>
        /// <param name="interactable">The interactable information.</param>
        public void Interact(IInteractable interactable)
        {
            m_Animator.SetTrigger(m_InteractAnimHash);
        }
    }
}