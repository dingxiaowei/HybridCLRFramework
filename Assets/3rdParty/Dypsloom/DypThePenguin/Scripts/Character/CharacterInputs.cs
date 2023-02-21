using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dypsloom.DypThePenguin.Scripts.Character;
using Dypsloom.DypThePenguin.Scripts.Items;

public class CharacterInputs : ICharacterInput
{
    public float Horizontal
    {
        get
        {
            foreach (var input in characterInputList)
            {
                if (input.Horizontal != 0)
                    return input.Horizontal;
            }
            return 0;
        }
    }

    public float Vertical
    {
        get
        {
            foreach (var input in characterInputList)
            {
                if (input.Vertical != 0)
                    return input.Vertical;
            }
            return 0;
        }
    }

    public bool Jump
    {
        get
        {
            foreach (var input in characterInputList)
            {
                if (input.Jump)
                    return input.Jump;
            }
            return false;
        }
    }

    public bool Interact
    {
        get
        {
            foreach (var input in characterInputList)
            {
                if (input.Interact)
                    return input.Interact;
            }
            return false;
        }
    }

    protected Character m_Character;
    List<ICharacterInput> characterInputList = new List<ICharacterInput>();
    public CharacterInput keyCodeCharacterInput;

    public void AddInput(ICharacterInput input)
    {
        characterInputList.Add(input);
    }

    public CharacterInputs(Character character)
    {
        m_Character = character;
        keyCodeCharacterInput = new CharacterInput(character);
        characterInputList.Add(keyCodeCharacterInput);
    }

    public bool DropItemHotbarInput(int slotIndex)
    {
        return keyCodeCharacterInput.DropItemHotbarInput(slotIndex);
    }

    public bool UseEquippedItemInput(IUsableItem item, int actionIndex)
    {
        return keyCodeCharacterInput.UseEquippedItemInput(item, actionIndex);
    }

    public bool UseItemHotbarInput(int slotIndex)
    {
        return keyCodeCharacterInput.UseItemHotbarInput(slotIndex);
    }
}
