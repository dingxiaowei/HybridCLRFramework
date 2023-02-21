/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Items
{
    /// <summary>
    /// An abstract class used to create actions for item objects.
    /// </summary>
    public interface IItemAction
    {
        bool CanUse { get; }
        void Use(IItem item, IItemUser itemUser);
    }
}