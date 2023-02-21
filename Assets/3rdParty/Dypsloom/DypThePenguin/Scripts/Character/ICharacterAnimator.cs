/// ---------------------------------------------
/// Dyp Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Character
{
    using Dypsloom.DypThePenguin.Scripts.Damage;
    using Dypsloom.DypThePenguin.Scripts.Interactions;

    /// <summary>
    /// Interface for the character animator.
    /// </summary>
    public interface ICharacterAnimator
    {
        void Tick();
        void ItemAction(int item, int itemAction);
        void EquipWeapon(int item);
        void UnequipWeapon();
        void Die(bool dead);
        void Damaged(Damage damage);
        void Interact(IInteractable interactable);
    }
}