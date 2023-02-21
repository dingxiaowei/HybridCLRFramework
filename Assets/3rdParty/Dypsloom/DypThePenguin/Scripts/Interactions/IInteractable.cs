/// ---------------------------------------------
/// Dyp Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Interactions
{
    /// <summary>
    /// The interactable interface used to be interact with by an interactor.. 
    /// </summary>
    public interface IInteractable
    {
        bool IsInteractable { get; }
        bool Interact(IInteractor interactor);
        bool Select(IInteractor interactor);
        bool Unselect(IInteractor interactor);
    }
}
