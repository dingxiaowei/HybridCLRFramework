/// ---------------------------------------------
/// Dyp Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Character
{
    using UnityEngine;

    /// <summary>
    /// Interface for the character mover.
    /// </summary>
    public interface ICharacterMover : IParentMover
    {
        Vector3 CharacterInputMovement { get; }
        bool IsJumping { get; }
    }

    /// <summary>
    /// Interface for the parent mover.
    /// </summary>
    public interface IParentMover : IMover
    {
        void AddExternalMover(IMover mover);
        void RemoveExternalMover(IMover mover);
    }
    
    /// <summary>
    /// Interface for the mover.
    /// </summary>
    public interface IMover
    {
        void Tick();
        Vector3 Movement { get; }
        void SetParentMover(IParentMover parent);
    }
}