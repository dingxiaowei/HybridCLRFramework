/// ---------------------------------------------
/// Dyp Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Items
{
    using Dypsloom.DypThePenguin.Scripts.Character;

    public interface IItemUser
    {
        void TickUse(IUsableItem usableItem);
        Character Character { get; }
    }
}