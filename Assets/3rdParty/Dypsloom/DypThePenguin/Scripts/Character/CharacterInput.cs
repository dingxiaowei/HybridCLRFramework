/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Character
{
    using Dypsloom.DypThePenguin.Scripts.Items;
    using UnityEngine;

    /// <summary>
    /// The character Input.
    /// </summary>
    public class CharacterInput : ICharacterInput
    {
        protected Character m_Character;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="character"></param>
        public CharacterInput(Character character)
        {
            m_Character = character;
        }

        public float Horizontal => Input.GetAxisRaw("Horizontal");
        public float Vertical => Input.GetAxisRaw("Vertical");
        public bool Jump => Input.GetButtonDown("Jump");
        public bool Interact => (Input.GetKeyDown(KeyCode.E) ||Input.GetButtonDown("Fire2"));

        /// <summary>
        /// The input to use an item action.
        /// </summary>
        /// <param name="usableItemObject">The usable ItemObject.</param>
        /// <param name="actionIndex">The action index.</param>
        /// <returns>True if the input is valid.</returns>
        public bool UseEquippedItemInput(IUsableItem usableItemObject, int actionIndex)
        {
            if (usableItemObject == null || usableItemObject.Item == null) { return false; }

            if (actionIndex == 0) {
                return Input.GetButtonDown("Fire1");
            }

            return true;
        }

        /// <summary>
        /// Use the item hot bar button.
        /// </summary>
        /// <param name="slotIndex">The hot bar index</param>
        /// <returns>True if the item should be used.</returns>
        public bool UseItemHotbarInput(int slotIndex)
        {
            return !Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1 + slotIndex);
        }

        /// <summary>
        /// Drop the item in the hot bar slot specified.
        /// </summary>
        /// <param name="slotIndex">The slot index.</param>
        /// <returns>True if the item should be dropped.</returns>
        public bool DropItemHotbarInput(int slotIndex)
        {
            return Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1 + slotIndex);
        }
    }
}