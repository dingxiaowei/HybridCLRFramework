/// ---------------------------------------------
/// Dyp Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Interactions
{
    using Dypsloom.DypThePenguin.Scripts.Character;

    /// <summary>
    /// The interactor allows you to interact with interactables.
    /// </summary>
    public interface IInteractor
    {
        void AddInteractable(IInteractable interactable);

        void RemoveInteractable(IInteractable interactable);
    }

    /// <summary>
    /// The character interactor has a reference to a character.
    /// </summary>
    public interface ICharacterInteractor : IInteractor
    {
        Character Character { get; }
    }
}