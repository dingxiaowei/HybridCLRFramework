/// ---------------------------------------------
/// Dyp The Penguin Character | Dypsloom
/// Copyright (c) Dyplsoom. All Rights Reserved.
/// https://www.dypsloom.com
/// ---------------------------------------------

namespace Dypsloom.DypThePenguin.Scripts.Items
{
    public interface IItem
    {
        ItemDefinition ItemDefinition { get; }
        void Use(Inventory itemInventory);
        void Drop(Inventory itemInventory, int amount);
    }
}